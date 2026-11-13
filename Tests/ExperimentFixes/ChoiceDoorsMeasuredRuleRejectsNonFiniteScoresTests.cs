// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using ProjectWikiDomain;

/// <summary>
/// Проверяемое поведение продукта: двери ВЫБОРА в домене страниц знаний о
/// проекте обязаны отказать, когда обученное число не измерено, и обязаны
/// сделать это ДО сравнения с порогом. Речь о трёх местах выбора: модель
/// обоснования потока по подсказке, машина плана оркестрации и обучаемая
/// модель маршрутизации запросов.
///
/// Почему это не придирка. В этом языке любое сравнение с «не-числом» ложно,
/// поэтому дверь вида «отказать, если показатель ниже порога» при неизмеренном
/// числе молча отвечает «порог пройден». С плюс бесконечностью беда зеркальная:
/// она больше любого порога, поэтому положительная форма проверки объявляет её
/// успехом. В маршрутизации запросов беда шла дальше всех: сам выбор лучшего
/// маршрута возвращал именно нечисловой маршрут, потому что вытеснение шло по
/// условию «строго больше», а оно для «не-числа» ложно.
///
/// Честная граница этих проверок. Нечислового обученного веса никто не
/// наблюдал. Это защита от достижимого входа — обученное состояние приходит с
/// загруженной контрольной точкой и никакой проверки по дороге не проходит, —
/// а не описание случившейся поломки. Проверки доказывают решения дверей на
/// враждебных числах, а не факт прошлой аварии.
///
/// Что эти проверки НЕ доказывают. Общее правило отвергает ровно «не-число» и
/// обе бесконечности; конечное бессмысленное значение остаётся измеренным и
/// проходит порог — это отдельная открытая находка. Дверь «ближайший сосед не
/// больше нуля» в модели обоснования здесь не проверяется вовсе: её число —
/// мера Жаккара (доля общих токенов среди всех) с явными защитами от деления
/// на ноль, неизмеренным оно стать не может, поэтому поставленная там проверка
/// измеренности является защитой на будущее, а не доказанной починкой, и
/// падающей проверки на неё не существует.
/// </summary>
public sealed class ChoiceDoorsMeasuredRuleRejectsNonFiniteScoresTests
{
    // Имя потока, по которому обучается и спрашивается модель обоснования.
    private const string FlowRef = "project-wiki/orchestration-plan";

    // Подсказка урока и запроса: одна и та же, чтобы мера близости к
    // обученному примеру была заведомо здоровой и отказ нельзя было списать
    // на плохое совпадение текста.
    private const string Prompt = "построить план оркестрации страниц знаний о проекте";

    // Три враждебных значения, ровно те, из-за которых порог молчит.
    public static TheoryData<string, double> UnmeasuredValues() => new()
    {
        { "не-число", double.NaN },
        { "плюс бесконечность", double.PositiveInfinity },
        { "минус бесконечность", double.NegativeInfinity }
    };

    // ------------------------------------------------------------------
    // Нейтральный дом правила и обращённая к публикации форма того же правила
    // ------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void NeutralHomeRule_RejectsEveryUnmeasuredValue(string caseName, double value)
    {
        // Нейтральный дом правила обязан назвать такое значение неизмеренным:
        // на этом единственном определении стоят теперь и двери выбора.
        Assert.False(
            ProjectWikiMeasuredNumberRule.IsMeasured(value),
            $"Значение «{caseName}» не является измерением и обязано быть отвергнуто правилом.");

        // Короткая форма для дверей, читающих несколько чисел, обязана
        // отвечать то же самое, иначе два определения разошлись бы.
        Assert.True(
            ProjectWikiMeasuredNumberRule.AnyNotMeasured(0.90, value),
            $"Набор со значением «{caseName}» обязан считаться содержащим неизмеренное число.");
    }

    [Theory]
    [InlineData("порог точности", 0.75)]
    [InlineData("ноль", 0.0)]
    [InlineData("отрицательный запас предпочтения", -0.30)]
    [InlineData("очень большое, но конечное число", 1.7e308)]
    public void HonestFiniteNumber_StaysMeasured_SoGoodRunsAreNotClosed(string caseName, double value)
    {
        // Обратная сторона правила: честное конечное число обязано проходить,
        // иначе проверка измеренности была бы просто запретом работы. Эта
        // проверка не обязана падать ни при одном ослаблении — наоборот, её
        // падение означало бы, что правило закрыло годные прогоны.
        Assert.True(
            ProjectWikiMeasuredNumberRule.IsMeasured(value),
            $"Значение «{caseName}» измерено и обязано проходить правило.");
        Assert.False(ProjectWikiMeasuredNumberRule.AnyNotMeasured(value, 0.90, 0.42));
    }

    [Fact]
    public void PublicationDoor_AsksTheNeutralHome_AndKeepsNoCopyOfTheCondition()
    {
        // Дверь публикации сохранила свои публичные имена, но условие в ней
        // больше не записано: одно понятие, два обращения, ни одной копии.
        // Падает эта проверка не при ослаблении правила, а при возврате копии
        // условия в дверь публикации: тогда ослабленный дом и неослабленная
        // копия ответят по-разному. Ослабление — это то, что делает копию
        // видимой.
        foreach (var value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0.75, 0.0 })
        {
            Assert.Equal(
                ProjectWikiMeasuredNumberRule.IsMeasured(value),
                ProjectWikiDpoPublicationMetricGate.IsMeasured(value));
            Assert.Equal(
                ProjectWikiMeasuredNumberRule.AnyNotMeasured(value, 0.90),
                ProjectWikiDpoPublicationMetricGate.AnyNotMeasured(value, 0.90));
        }

        // Пустая ссылка — тот же отказывающий запас по умолчанию у обоих имён.
        Assert.Equal(
            ProjectWikiMeasuredNumberRule.AnyNotMeasured(null),
            ProjectWikiDpoPublicationMetricGate.AnyNotMeasured(null));

        // Человеческое название болезни тоже одно, и дверь публикации не
        // держит его отдельной копией: иначе отчёты двери публикации и дверей
        // выбора называли бы одну беду разными словами.
        Assert.Equal(
            ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix,
            ProjectWikiDpoPublicationMetricGate.NotMeasuredReasonSuffix);
    }

    // ------------------------------------------------------------------
    // Модель обоснования потока по подсказке: дверь обученной оценки
    // ------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void UnmeasuredTrainedWeight_ClosesGrounding_BeforeTheThresholdComparison(
        string caseName,
        double unmeasuredWeight)
    {
        // Испорчен ровно один обученный вес входного признака, всё остальное
        // здоровое: так видно, что отказ вызвал именно он.
        var model = GroundingModelWithInputWeight((float)unmeasuredWeight);

        var grounding = model.TryGround(Prompt, HumanIntentContract.Empty);

        // Обоснование обязано не состояться: верить оценке, которой нет,
        // нельзя ни при каком пороге.
        Assert.True(
            grounding is null,
            $"Обученный вес «{caseName}» обязан закрыть обоснование потока.");
    }

    [Fact]
    public void HonestTrainedWeight_StillGrounds_WithMeasuredConfidence()
    {
        // Честный обученный вес: дверь обязана открыться, иначе проверка
        // измеренности была бы запретом обоснования вообще.
        var model = GroundingModelWithInputWeight(2.0f);

        var grounding = model.TryGround(Prompt, HumanIntentContract.Empty);

        Assert.NotNull(grounding);
        Assert.Equal(FlowRef, grounding!.FlowRef);

        // И уверенность обязана быть измеренной: обоснование с неизмеренной
        // уверенностью — это то, что чинит вся эта правка.
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(grounding.Confidence));
    }

    [Fact]
    public void TakingTheMinimumIsNotADefence_AgainstAnUnmeasuredScore()
    {
        // Свойство арифметики, из-за которого прежняя дверь уверенности была
        // бессильна. Это объяснение причины, а не живой путь беды: живой путь
        // проверяется выше, через обученный вес. Поэтому проверка не обязана
        // падать при ослаблении — она держится за арифметику языка.
        Assert.True(double.IsNaN(Math.Min(1.0, double.NaN)));

        // А с плюс бесконечностью взятие минимума молча выдаёт ровно единицу,
        // то есть наибольшую возможную уверенность из неизмеренного веса.
        Assert.Equal(1.0, Math.Min(1.0, double.PositiveInfinity));
    }

    // ------------------------------------------------------------------
    // Машина плана оркестрации: дверь точности из чужой передаточной записи
    // ------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public async Task UnmeasuredImportedAccuracy_DoesNotBecomeAPassedTrainingGate(
        string caseName,
        double unmeasuredAccuracy)
    {
        var machine = new OrchestrationPlanMachine();

        var imported = await machine.ImportAsync(ExportWithAccuracy(unmeasuredAccuracy));

        // Сам импорт по-прежнему успешен: эта правка не превращает импорт в
        // отказ, она не даёт неизмеренной точности стать пройденным порогом.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        // Принятого порога обучения быть не должно: точность, которой нет, не
        // доказывает ничего.
        Assert.True(
            machine.LastEvaluationGate is null,
            $"Точность «{caseName}» из чужой записи не имеет права стать пройденным порогом.");

        // И обратно наружу машина не отдаёт неизмеренное число как свою
        // точность: прежде оно уходило и в ссылку на контрольную точку, и в
        // ссылку на паспорт модели как «проверенное оценкой».
        var exported = await machine.ExportAsync();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(exported.Value!.AccuracyAtExport));
    }

    [Fact]
    public async Task HonestImportedAccuracy_StillBecomesAPassedTrainingGate()
    {
        var machine = new OrchestrationPlanMachine();

        var imported = await machine.ImportAsync(ExportWithAccuracy(0.80));

        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        // Честная точность обязана по-прежнему приниматься, иначе проверка
        // измеренности закрыла бы годный импорт.
        Assert.NotNull(machine.LastEvaluationGate);
        Assert.True(machine.LastEvaluationGate!.PassesGate);
        Assert.Equal(0.80, machine.LastEvaluationGate.Accuracy);
    }

    // ------------------------------------------------------------------
    // Обучаемая модель маршрутизации запросов: выбор, запас и уверенность
    // ------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void WhenNoRouteScoreIsMeasured_TheModelDefersWithItsOwnReason(
        string caseName,
        double unmeasuredScore)
    {
        // Все оценки маршрутов неизмерены. Проверка «все оценки нули» этого
        // случая не видит: неизмеренное значение нулём не является.
        var model = RoutingModelWithBiases(Enumerable.Repeat(unmeasuredScore, 15).ToArray());

        var decision = model.Route(Prompt);

        // Решение обязано быть отложено, и запускать исполнителя маршрута
        // нельзя.
        Assert.True(
            decision.DecisionStatus == ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
            $"Оценки маршрутов «{caseName}» обязаны отложить решение, а не выбрать маршрут.");
        Assert.False(decision.CanStartTargetRunner);

        // Причина обязана быть своей собственной и отличимой и от «запас ниже
        // порога», и от «все оценки нули»: человек должен видеть, что
        // обученных оценок нет вовсе.
        Assert.Contains(
            "request_routing_scores_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix,
            decision.Warnings);

        // И сам отказ не имеет права нести неизмеренную уверенность.
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(decision.Confidence));
        _ = caseName;
    }

    [Fact]
    public void AnUnmeasuredScore_MustNotWinTheSelection()
    {
        // Самое важное место. Нечисло стоит на нулевом маршруте, остальные
        // оценки здоровые и дают законный запас между двумя лучшими. Прежний
        // выбор возвращал именно нулевой маршрут: вытеснение шло по условию
        // «строго больше», а оно для «не-числа» ложно. Дверь запаса эту беду
        // пропускала законно, потому что «не-число» в сортировке по убыванию
        // уходит в конец и верхние два значения конечны.
        var model = RoutingModelWithBiases([double.NaN, 0.5, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        var decision = model.Route(Prompt);

        // Выбор обязан состояться по лучшей ИЗМЕРЕННОЙ оценке — это второй
        // маршрут, а не нулевой.
        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.Selected, decision.DecisionStatus);
        Assert.Equal(ProjectWikiRequestRouteKind.NegativeDpoTraining, decision.RouteKind);

        // И уверенность выбранного маршрута обязана быть измеренной: прежде
        // здесь уходило «не-число», потому что ограничение отрезком
        // «не-числа» — снова «не-число».
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(decision.Confidence));
        Assert.True(decision.Confidence >= ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence);
    }

    [Fact]
    public void UnmeasuredMargin_ClosesTheRoute_EvenWhenAMeasuredScoreExists()
    {
        // Плюс бесконечность на нулевом маршруте. В выборе она больше не
        // участвует, поэтому лучший измеренный маршрут есть, но в подсчёте
        // запаса бесконечность остаётся и даёт бесконечный запас. Прежде это
        // был выбор с уверенностью 0,99 на маршруте, оценки которого нет.
        var model = RoutingModelWithBiases([double.PositiveInfinity, 0.5, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        var decision = model.Route(Prompt);

        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, decision.DecisionStatus);
        Assert.False(decision.CanStartTargetRunner);

        // Причина обязана называть именно неизмеренный запас, а не «запас ниже
        // порога»: первая беда лечится починкой обученного состояния, вторая
        // улучшением модели.
        Assert.Contains(
            "request_routing_margin_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix,
            decision.Warnings);
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(decision.Confidence));
    }

    [Fact]
    public void HonestRouteScores_StillSelectARoute_WithMeasuredConfidence()
    {
        // Здоровые оценки: выбор обязан состояться, иначе проверка
        // измеренности закрыла бы маршрутизацию вообще.
        var model = RoutingModelWithBiases([0.1, 0.5, 0.05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        var decision = model.Route(Prompt);

        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.Selected, decision.DecisionStatus);
        Assert.Equal(ProjectWikiRequestRouteKind.NegativeDpoTraining, decision.RouteKind);
        Assert.True(ProjectWikiMeasuredNumberRule.IsMeasured(decision.Confidence));
    }

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void ADeferredDecision_NeverCarriesAnUnmeasuredConfidence(
        string caseName,
        double unmeasuredMargin)
    {
        // Общедоступная сборка отложенного решения принимает запас аргументом,
        // поэтому неизмеренное число попадает в неё достижимо. Даже отказ не
        // имеет права нести уверенность, которую нельзя сравнить ни с одним
        // порогом.
        var decision = ProjectWikiRequestRoutingTrainableModel.BuildLowConfidenceDecision(
            "Request routing deferred for second pass in a measured-rule check.",
            ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
            gateEvidence: null,
            topTwoMargin: unmeasuredMargin);

        Assert.True(
            ProjectWikiMeasuredNumberRule.IsMeasured(decision.Confidence),
            $"Отказ с запасом «{caseName}» обязан нести измеренную уверенность.");
        Assert.Equal(0.0, decision.Confidence);

        // А сам наблюдённый запас в решении сохраняется как есть: читателю
        // важно видеть, что именно пришло, и это не сравнивается с порогом.
        Assert.False(ProjectWikiMeasuredNumberRule.IsMeasured(decision.TopTwoMargin));
    }

    [Fact]
    public void AHonestSmallMargin_KeepsItsPreviousLowConfidenceValue()
    {
        // Честный малый запас обязан дать ровно прежнее значение низкой
        // уверенности: половина порога запаса даёт половину потолка низкой
        // уверенности. Так видно, что проверка измеренности не переписала
        // формулу и не сдвинула пороги.
        var decision = ProjectWikiRequestRoutingTrainableModel.BuildLowConfidenceDecision(
            "Request routing deferred for second pass in a measured-rule check.",
            ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass,
            gateEvidence: null,
            topTwoMargin: ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin / 2.0);

        Assert.Equal(0.245, decision.Confidence, 6);
    }

    // ------------------------------------------------------------------
    // Сборка подопытных состояний
    // ------------------------------------------------------------------

    // Модель обоснования с одним обученным примером и ровно одним заданным
    // весом входного признака: всё остальное в обученном состоянии здоровое.
    private static PromptFlowGroundingModel GroundingModelWithInputWeight(float inputWeight)
    {
        var pair = Mode1Pair();
        var model = new PromptFlowGroundingModel();

        // Обычное обучение: так в хранилище примеров появляется здоровый
        // пример, по которому мера близости заведомо положительна.
        model.Train(pair);

        // Затем подменяется ровно обученный вес — тот путь, которым
        // неизмеренное число и может прийти в эту модель: вместе с
        // загруженной контрольной точкой.
        model.ImportState(
            [pair],
            new Dictionary<string, OrchestrationFlowGroundingWeights>(StringComparer.Ordinal)
            {
                [FlowRef] = WeightsWithInputWeight(inputWeight)
            });
        return model;
    }

    // Обученная пара первого режима: только такие пары модель обоснования и
    // принимает.
    private static OrchestrationTrainingPair Mode1Pair()
        => new(
            "choice-doors-measured-rule-pair",
            PairMode.Mode1LearnedFlowGrounding,
            Prompt,
            new OrchestrationPlanChosen(
                FlowRef,
                "План оркестрации страниц знаний о проекте",
                "docs/plans/choice-doors-measured-rule.md",
                "Шаг 1. Собрать замысел. Шаг 2. Проверить порог."),
            "Отвергнутый ответ без плана.",
            GenerationRouteMode.LearnedFlowGrounding,
            BoundarySource.Classifier,
            true,
            IntentKind.KnownFlowPlanning,
            FlowRef,
            "choice-doors-measured-rule-bundle",
            false);

    // Обученные веса, в которых задан ровно один вес входного признака:
    // слово «план» есть в подсказке, поэтому оценка целиком равна этому весу.
    private static OrchestrationFlowGroundingWeights WeightsWithInputWeight(float inputWeight)
        => new(
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { ["план"] = inputWeight },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { ["classification"] = 1.0f },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { ["learned-flow-plan"] = 1.0f },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { ["Gate B"] = 1.0f },
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { ["project-wiki"] = 1.0f });

    // Передаточная запись чужой обученной машины с заданной точностью. Ссылки
    // указывают на пройденную оценку, иначе отказ наступил бы по другой
    // причине и проверка доказывала бы не то.
    private static LearningMachineExport ExportWithAccuracy(double accuracyAtExport)
        => new(
            OrchestrationPlanMachine.MachineId,
            OrchestrationPlanMachine.DomainTag,
            "1.0.0",
            $"{OrchestrationPlanMachine.MachineId}/1.0.0/eval/choice-doors-measured-rule/checkpoint",
            $"{OrchestrationPlanMachine.MachineId}/1.0.0/eval/choice-doors-measured-rule/manifest.json",
            accuracyAtExport,
            DateTimeOffset.UtcNow);

    // Обучаемая модель маршрутизации с заданными оценками маршрутов и с уже
    // пройденной отложенной оценкой: без неё решение отказало бы по другой
    // причине. Веса токенов пустые, поэтому оценки маршрутов равны ровно
    // заданным числам.
    private static ProjectWikiRequestRoutingTrainableModel RoutingModelWithBiases(double[] routeScores)
        => ProjectWikiRequestRoutingTrainableModel
            .FromSnapshot(SnapshotWithBiases(routeScores))
            .WithGateEvidence(PassedGateEvidence());

    private static ProjectWikiRequestRoutingModelSnapshot SnapshotWithBiases(double[] routeScores)
        => new(
            "averaged-token-perceptron-v1",
            [],
            new Dictionary<string, double[]>(StringComparer.Ordinal),
            routeScores,
            18,
            0,
            120,
            40,
            "choice-doors-measured-rule-eval",
            0.96,
            1.0,
            0.90,
            false,
            true,
            ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy,
            ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate,
            ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence,
            ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin,
            ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence,
            true,
            []);

    // Пройденная отложенная оценка: все три её числа выдерживают свои пороги,
    // поэтому дверь оценки пропускает решение дальше и проверяется ровно то,
    // что заявлено.
    private static ProjectWikiRequestRoutingGateEvidence PassedGateEvidence()
        => new(
            "choice-doors-measured-rule-eval",
            120,
            40,
            0.96,
            1.0,
            0.90,
            false,
            true,
            true,
            ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy,
            ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate,
            ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence,
            ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin,
            ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence,
            []);
}
