// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using ProjectWikiDomain;

// Что здесь проверяется, по-человечески.
//
// Это названная приёмка переноса «выбранному маршруту выдаётся его собственный выученный план»
// (строка R02 реестра переносов, код правки FIX-IM; карточка — файл 48 «тождество выученного
// маршрута и тела плана» в папке исправлений по опытам). Механизм в продукте уже принят и здесь
// не меняется ни на знак: второй поиск тела ограничен уже выбранным маршрутом
// (42.ProjectWikiDomain/PromptFlowGroundingModel.cs, метод FindLearnedPair, строки 154–169).
//
// Чего не хватало. Пять прежних проверок тождества маршрута и тела стоят на уровне самой модели:
// они вызывают поиск напрямую и смотрят на идентификатор найденной пары и на её поле плана. Весь
// выданный пользователю текст и его заголовок они не читают вовсе. А заголовок и тело в печати
// берутся из ДВУХ РАЗНЫХ источников в соседних строках одного метода
// (42.ProjectWikiDomain/TrainedRouteComposerUnit.cs, метод RenderLearnedFlowPlan: строка 19 печатает
// маршрут из grounding.FlowRef, строка 34 печатает тело из learned.Chosen). Поэтому правильный
// заголовок может стоять над чужим телом, и проверка одного заголовка этого не увидит.
//
// Поэтому приёмка идёт через фактического получателя — продуктовый метод выдачи плана
// (42.ProjectWikiDomain/OrchestrationPlanMachine.cs, метод GeneratePlanAsync, строки 66–138) — и
// на ВОССТАНОВЛЕННОМ состоянии: обучение, выгрузка контрольной точки, загрузка в чистую машину
// (метод ImportCheckpoint, строки 383–418), и только потом запрос.
//
// Какие числа подтверждены наблюдением, а какие посчитаны.
// Подтверждено ссылкой на исходник: получатель пускает выученный маршрут при уверенности не ниже
// 0,75 (OrchestrationPlanMachine.cs, строка 107). Это НЕ тот же порог, что собственный порог
// модели 0,50 (PromptFlowGroundingModel.cs, строка 142), и не порог точности оценки 0,75
// (OrchestrationPlanMachine.cs, строка 17) — три разных числа в трёх разных местах.
// Подтверждено прогоном этих проверок: выбранный маршрут — длинный, и напечатанная уверенность
// равна единице (это читается в сверке всего текста ниже).
// Посчитано расчётом-гипотезой, прогоном отдельно НЕ измерено: промежуточные оценки весов
// (у длинного маршрута четыре совпавших слова, у короткого три). Эти величины наружу не выходят,
// и ни одно утверждение здесь на них не опирается.
//
// Граница трёх подмен — читать обязательно, иначе приёмка выйдет переоценённой.
// Три испорченных состояния ниже доказывают РОВНО ОДНО: договор приёмки видит подмену тела и
// краснеет на ней, то есть проверка чувствительна. Они НЕ доказывают, что продукт защищён от
// такого состояния, и эта правка такого не лечит: фильтр узкий — он отбирает пары по выбранному
// маршруту, и если в памяти лежит пара маршрута LAB-A, в чьём теле записан текст другого
// маршрута, поиск вернёт её законно. Отвергает ли продукт испорченную контрольную точку с
// несовпадающими полями — не проверено и этой правкой не лечится; об этом прямо говорит и
// карточка переноса. Ниже это записано ещё и утверждением, чтобы предел был видим, а не спрятан.
//
// Два учебных урока взяты дословно из опубликованного состояния опыта тождества маршрута и тела
// (docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/A/states.json,
// запись phase="after-lessons", раздел flow_grounding_knn). Новых учебных данных здесь нет, и
// числа того опыта (точность второго плеча три из трёх, состояние 3801 байт) заново НЕ измерялись.
public sealed class LearnedRouteBodyDeliveryTests
{
    // Короткий маршрут: его урок — короткий план про выдачу квитанции.
    private const string ShortRouteId = "LAB-B";

    // Длинный маршрут: его урок — план про проверку одобрения перед записью журнала.
    private const string LongRouteId = "LAB-A";

    // Первый запрос опыта: именно на нём маршрут и тело расходились до правки.
    private const string ApprovalQuery = "validate payment approval ledger";

    // Запрос короткого урока, он же его собственный текст запроса.
    private const string ReleaseQuery = "validate payment ledger release";

    // Названия уроков: по ним тело в печати тоже опознаётся.
    private const string ShortTitle = "Receipt plan";
    private const string LongTitle = "Approval ledger plan";

    // Тела планов — дословные строковые константы. Ожидаемое значение НИГДЕ не берётся из
    // сериализатора продукта и не из того же объекта, который проверяется: иначе это была бы
    // сверка копии с собой.
    private const string ShortPlanText =
        "Validate payment, persist the ledger, then release the receipt.";

    private const string LongPlanText =
        "Validate approval before writing the ledger; preserve the approval record.";

    // Старое тело ТОГО ЖЕ длинного маршрута: первая половина правильного тела без второй. Подмена
    // выбрана самой опасной намеренно — она повторяет всю первую фразу, поэтому проверка «текст
    // содержит такую подстроку» или «начинается с неё» её бы ПРОПУСТИЛА. Отвергает её только
    // сверка всего тела дословно, а это и есть названный пробел приёмки.
    private const string StaleBodyOfTheLongRoute =
        "Validate approval before writing the ledger.";

    // Имя маршрута, под которым в памяти соседей нет ни одного урока.
    private const string RouteWithoutLesson = "LAB-MISSING";

    // Весь текст, который получатель обязан выдать на подлинном восстановленном состоянии.
    // Собран здесь руками, строка за строкой, а не взят у продукта. Разделитель строк — тот же
    // платформенный перевод строки, которым склеивает печать; число уверенности записано как
    // литерал «единица» в печатной записи продукта, чтобы проверка не зависела от местных
    // настроек разделителя дробной части, а не для того, чтобы взять значение у продукта.
    private static string ExpectedDeliveredPlanText => string.Join(Environment.NewLine, [
        "LEARNED_FLOW_GROUNDING_PLAN",
        "flow_ref=" + LongRouteId,
        "confidence=" + 1.0.ToString("0.000"),
        "intent=" + IntentKind.ImplementationExecutionRequest,
        "prompt=" + ApprovalQuery,
        "title=" + LongTitle,
        "mode=learned_flow_grounding",
        "FLOW_REF=" + LongRouteId,
        "TITLE=" + LongTitle,
        LongPlanText
    ]);

    // Ровно три последние строки предыдущего текста: заголовок тела и само тело. Нужны отдельно,
    // чтобы сказать «тело выдано целиком и принадлежит той же паре, что и заголовок».
    private static string ExpectedDeliveredBody => string.Join(Environment.NewLine, [
        "FLOW_REF=" + LongRouteId,
        "TITLE=" + LongTitle,
        LongPlanText
    ]);

    // Весь текст обобщённого ответа, который получатель обязан выдать, когда у выбранного маршрута
    // урока нет. Ни одной строки чужого тела в нём быть не может: обобщённая память здесь пуста,
    // поэтому ни ссылки на номер урока, ни тела плана печать не добавляет.
    private static string ExpectedFallbackPlanText => string.Join(Environment.NewLine, [
        "ORCHESTRATION_PLAN",
        "intent=" + IntentKind.ImplementationExecutionRequest,
        "prompt=" + ApprovalQuery,
        "mode=generalized_non_lookup"
    ]);

    // Договор приёмки собран в ОДНОМ месте: именно этот метод обязан краснеть на каждом из трёх
    // ошибочных выходов, и именно его чувствительность доказывают три проверки в конце файла.
    private static void AssertDeliveredPlanIsTheLongRoutesOwnPlan(OrchestrationGenerationResult delivered)
    {
        // Маршрут выдачи — выученный, а не обобщённый откат.
        Assert.Equal(GenerationRouteMode.LearnedFlowGrounding, delivered.RouteMode);

        // Выбранный маршрут назван и это длинный маршрут.
        Assert.NotNull(delivered.FlowGrounding);
        Assert.Equal(LongRouteId, delivered.FlowGrounding!.FlowRef);

        // Весь выданный текст сверяется дословно: заголовок и тело одной сверкой, поэтому
        // правильный заголовок над чужим телом пройти не может.
        Assert.Equal(ExpectedDeliveredPlanText, delivered.PlanText);
    }

    // УТВЕРЖДЕНИЕ ПЕРВОЕ. Положительный случай идёт на ПОДЛИННОМ историческом состоянии, отдельно
    // от всех испорченных: запрос, чей маршрут выбран обучением, получает через получателя
    // заголовок своего маршрута и полный дословный текст тела своей пары.
    [Fact]
    public async Task RestoredHistoricalState_DeliversItsOwnRouteHeadingAndItsOwnWholeBody()
    {
        var restored = await RestoredFromHistoricalStateAsync();
        var result = await restored.GeneratePlanAsync(ApprovalQuery);

        // Выдача состоялась без отказа.
        Assert.True(result.IsSuccess);

        // Весь договор приёмки целиком.
        AssertDeliveredPlanIsTheLongRoutesOwnPlan(result.Value!);

        // Тело выдано целиком и стоит последним: это та же сверка, но названная отдельно, чтобы в
        // отчёте было видно, что проверялось именно полное тело, а не его начало.
        Assert.EndsWith(ExpectedDeliveredBody, result.Value!.PlanText, StringComparison.Ordinal);

        // Чужого тела в ответе нет ни одной буквой.
        Assert.DoesNotContain(ShortPlanText, result.Value.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain(ShortTitle, result.Value.PlanText, StringComparison.Ordinal);
    }

    // УТВЕРЖДЕНИЕ ПЕРВОЕ, вторая половина: маршрут, идентификатор пары и тело относятся к ОДНОЙ
    // паре. Владелец тела находится в восстановленной памяти по дословной константе тела, и его
    // номер и маршрут сверяются с дословными же константами. Ожидаемое значение при этом не
    // берётся у продукта: по константе выполняется только опознание владельца.
    [Fact]
    public async Task RestoredHistoricalState_HeadingPairIdAndBodyAreOneAndTheSamePair()
    {
        var restored = await RestoredFromHistoricalStateAsync();

        // В восстановленной памяти ровно одна пара несёт это тело — иначе опознание было бы
        // двусмысленным, и проверка упала бы здесь.
        var owner = restored.ExportCheckpoint().FlowGroundingKnn
            .Single(pair => string.Equals(pair.Chosen.PlanText, LongPlanText, StringComparison.Ordinal));

        // Её номер и её маршрут — те самые, что назвал опыт.
        Assert.Equal("10-a", owner.PairId);
        Assert.Equal(LongRouteId, owner.FlowRef);
        Assert.Equal(LongRouteId, owner.Chosen.FlowRef);
        Assert.Equal(LongTitle, owner.Chosen.Title);

        // И ровно это тело под ровно этим заголовком доходит до получателя.
        var result = await restored.GeneratePlanAsync(ApprovalQuery);
        Assert.True(result.IsSuccess);
        Assert.Contains("flow_ref=" + LongRouteId, result.Value!.PlanText, StringComparison.Ordinal);
        Assert.EndsWith(ExpectedDeliveredBody, result.Value.PlanText, StringComparison.Ordinal);
    }

    // УТВЕРЖДЕНИЕ ВТОРОЕ, первая половина. Отсутствующий маршрут: обученные веса называют маршрут,
    // под которым в памяти соседей урока нет. Через получателя это обязано дать обобщённый ответ
    // без чужой пары, а не чужую пару под чужим заголовком.
    [Fact]
    public async Task MissingRouteAfterRestore_DeliversNoOtherRoutesPairThroughTheReceiver()
    {
        var restored = await RestoredWithLearnedWeightsRenamedToAsync(RouteWithoutLesson);
        var result = await restored.GeneratePlanAsync(ApprovalQuery);

        Assert.True(result.IsSuccess);
        AssertFallbackCarriesNoOtherRoutesPair(result.Value!);
    }

    // УТВЕРЖДЕНИЕ ВТОРОЕ, вторая половина. Пустой маршрут — это имя, которого не носит ни один
    // урок, а не подстановочный знак: тихого расширения поиска назад к глобальному быть не может.
    [Fact]
    public async Task EmptyRouteAfterRestore_DeliversNoOtherRoutesPairThroughTheReceiver()
    {
        var restored = await RestoredWithLearnedWeightsRenamedToAsync(string.Empty);
        var result = await restored.GeneratePlanAsync(ApprovalQuery);

        Assert.True(result.IsSuccess);
        AssertFallbackCarriesNoOtherRoutesPair(result.Value!);
    }

    // УТВЕРЖДЕНИЕ ТРЕТЬЕ, подмена первая: правильный идентификатор маршрута с ПУСТЫМ телом.
    // Доказывает чувствительность договора приёмки и ничего больше.
    [Fact]
    public async Task EmptyBodyUnderItsOwnHeading_MakesTheAcceptanceCheckRed()
    {
        var delivered = await DeliverWithSubstitutedLongRouteBodyAsync(string.Empty);

        // Заголовок остался своим: ошибка ровно в теле, и это главное условие подмены.
        Assert.Equal(LongRouteId, delivered.FlowGrounding!.FlowRef);
        Assert.Contains("flow_ref=" + LongRouteId, delivered.PlanText, StringComparison.Ordinal);

        // Договор приёмки обязан покраснеть.
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(
            () => AssertDeliveredPlanIsTheLongRoutesOwnPlan(delivered));
    }

    // УТВЕРЖДЕНИЕ ТРЕТЬЕ, подмена вторая: правильный идентификатор маршрута с телом ДРУГОГО
    // маршрута. Это ровно та беда, ради которой перенос и делался, но здесь она внесена в
    // состояние руками, поэтому доказывает только чувствительность проверки.
    [Fact]
    public async Task AnotherRoutesBodyUnderItsOwnHeading_MakesTheAcceptanceCheckRed()
    {
        var delivered = await DeliverWithSubstitutedLongRouteBodyAsync(ShortPlanText);

        Assert.Equal(LongRouteId, delivered.FlowGrounding!.FlowRef);
        Assert.Contains("flow_ref=" + LongRouteId, delivered.PlanText, StringComparison.Ordinal);

        Assert.ThrowsAny<Xunit.Sdk.XunitException>(
            () => AssertDeliveredPlanIsTheLongRoutesOwnPlan(delivered));

        // НАЗВАННЫЙ ПРЕДЕЛ, а не желаемое поведение. Узкий фильтр отбирает пары по выбранному
        // маршруту, поэтому пара маршрута LAB-A с чужим телом возвращается ЗАКОННО, и чужой текст
        // доходит до ответа. Отвергает ли продукт такое испорченное восстановленное состояние —
        // не проверено и этой правкой не лечится; решение об отдельной проверке целостности
        // состояния принимает владелец. Утверждение стоит здесь, чтобы предел был видим.
        Assert.Contains(ShortPlanText, delivered.PlanText, StringComparison.Ordinal);
    }

    // УТВЕРЖДЕНИЕ ТРЕТЬЕ, подмена третья: правильный идентификатор маршрута со СТАРЫМ телом того
    // же маршрута. Старое тело повторяет всю первую фразу правильного, поэтому проверка на
    // подстроку или на начало текста его пропустила бы; красной она становится только потому, что
    // сверяется всё тело дословно.
    [Fact]
    public async Task StaleBodyOfTheSameRouteUnderItsOwnHeading_MakesTheAcceptanceCheckRed()
    {
        var delivered = await DeliverWithSubstitutedLongRouteBodyAsync(StaleBodyOfTheLongRoute);

        Assert.Equal(LongRouteId, delivered.FlowGrounding!.FlowRef);
        Assert.Contains("flow_ref=" + LongRouteId, delivered.PlanText, StringComparison.Ordinal);

        Assert.ThrowsAny<Xunit.Sdk.XunitException>(
            () => AssertDeliveredPlanIsTheLongRoutesOwnPlan(delivered));

        // Доказательство того, что слабая проверка здесь бы прошла: общая первая фраза в тексте
        // есть, а правильного тела целиком — нет. Поэтому сверка целого тела обязательна.
        Assert.Contains("Validate approval before writing the ledger", delivered.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain(LongPlanText, delivered.PlanText, StringComparison.Ordinal);
    }

    // Общая часть утверждения второго: обобщённый ответ не несёт ни чужого тела, ни чужого
    // заголовка тела, и сверяется целиком, знак в знак.
    private static void AssertFallbackCarriesNoOtherRoutesPair(OrchestrationGenerationResult delivered)
    {
        // Выдача ушла на обобщённую дорогу: выученного маршрута у ответа нет.
        Assert.Equal(GenerationRouteMode.Mode2Generalization, delivered.RouteMode);
        Assert.Null(delivered.FlowGrounding);

        // Весь текст ответа — прежняя обобщённая форма, без единой лишней строки.
        Assert.Equal(ExpectedFallbackPlanText, delivered.PlanText);

        // Ни заголовка тела, ни ссылки на номер урока, ни одного из двух тел в ответе нет.
        Assert.DoesNotContain("FLOW_REF=", delivered.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain("style_source=", delivered.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain(ShortPlanText, delivered.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain(LongPlanText, delivered.PlanText, StringComparison.Ordinal);

        // И никакого предупреждения об откате: область потока в запросе не названа.
        Assert.Empty(delivered.Warnings);
    }

    // Обучение на подлинных уроках и выгрузка контрольной точки. Учебная память создаётся ровно
    // один раз на вызов и из тех же двух исторических уроков; новых учебных данных здесь нет.
    private static async Task<OrchestrationPlanMachineCheckpoint> TrainedCheckpointAsync()
    {
        var trainer = new OrchestrationPlanMachine();
        var trained = await trainer.TrainOrchestrationBatchAsync(
            new OrchestrationTrainingBatch("fix-im-identity-delivery", LearnedPairs()));

        // Обучение прошло и оба урока — уроки привязки запроса к маршруту.
        Assert.True(trained.IsSuccess);
        Assert.Equal(2, trained.Value!.Mode1RowsTrained);
        Assert.Equal(0, trained.Value.Mode2RowsTrained);

        return trainer.ExportCheckpoint();
    }

    // Восстановление состояния в ЧИСТУЮ машину. Именно это и значит «после восстановления
    // состояния»: отвечает не та машина, которая училась.
    private static async Task<OrchestrationPlanMachine> RestoredFromHistoricalStateAsync()
        => Restore(await TrainedCheckpointAsync());

    // Восстановление того же состояния, но обученные веса называют маршрут, под которым урока нет.
    // Так выглядит частично восстановленная или прореженная контрольная точка: веса маршрут знают,
    // памяти соседей по нему нет.
    private static async Task<OrchestrationPlanMachine> RestoredWithLearnedWeightsRenamedToAsync(string routeName)
    {
        var checkpoint = await TrainedCheckpointAsync();

        // Берутся веса длинного маршрута и кладутся под другое имя маршрута; сами веса не
        // меняются, поэтому выбор маршрута по оценке остаётся прежним.
        var renamed = checkpoint.FlowGroundingWeights
            .Where(entry => string.Equals(entry.Key, LongRouteId, StringComparison.Ordinal))
            .ToDictionary(_ => routeName, entry => entry.Value, StringComparer.Ordinal);

        // Переименование состоялось: иначе дальше проверялось бы не то состояние.
        Assert.Single(renamed);

        return Restore(checkpoint with
        {
            FlowGroundingWeights = renamed
        });
    }

    // Восстановление состояния, в котором у пары длинного маршрута подменено ТОЛЬКО тело плана, и
    // выдача ответа через получателя. Веса не затронуты, поэтому маршрут выбирается прежний, а
    // расхождение остаётся ровно в теле.
    private static async Task<OrchestrationGenerationResult> DeliverWithSubstitutedLongRouteBodyAsync(string body)
    {
        var checkpoint = await TrainedCheckpointAsync();

        var substituted = checkpoint.FlowGroundingKnn
            .Select(pair => string.Equals(pair.FlowRef, LongRouteId, StringComparison.Ordinal)
                ? pair with { Chosen = pair.Chosen with { PlanText = body } }
                : pair)
            .ToArray();

        // Подмена состоялась ровно в паре длинного маршрута, а пара короткого маршрута не
        // затронута. Опознавать подменённую пару по тексту тела нельзя: когда подменой служит
        // тело другого маршрута, этот текст несут обе пары, и такая охрана была бы ложна по
        // построению. Поэтому пары опознаются по маршруту, а тела сверяются по отдельности.
        Assert.Equal(2, substituted.Length);
        var longRoutePair = Assert.Single(
            substituted,
            pair => string.Equals(pair.FlowRef, LongRouteId, StringComparison.Ordinal));
        Assert.Equal(body, longRoutePair.Chosen.PlanText);
        var shortRoutePair = Assert.Single(
            substituted,
            pair => string.Equals(pair.FlowRef, ShortRouteId, StringComparison.Ordinal));
        Assert.Equal(ShortPlanText, shortRoutePair.Chosen.PlanText);

        var restored = Restore(checkpoint with { FlowGroundingKnn = substituted });
        var result = await restored.GeneratePlanAsync(ApprovalQuery);

        // Отказа выдачи нет: подменено состояние, а не работоспособность получателя.
        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    // Загрузка контрольной точки в чистую машину с проверкой, что загрузка принята.
    private static OrchestrationPlanMachine Restore(OrchestrationPlanMachineCheckpoint checkpoint)
    {
        var restored = new OrchestrationPlanMachine();
        var imported = restored.ImportCheckpoint(checkpoint);

        // Загрузка обязана быть принятой: иначе отвечала бы пустая машина и проверка стала бы
        // бессмысленно зелёной.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        Assert.Equal(2, restored.Mode1FlowGroundingKnnSize);

        return restored;
    }

    // Два урока опыта, дословно. Длинный запрос сохранён во всю длину намеренно: его длина — вторая
    // половина причины, по которой прежний глобальный поиск отвечал чужим телом.
    private static IReadOnlyList<OrchestrationTrainingPair> LearnedPairs() =>
    [
        new OrchestrationTrainingPair(
            PairId: "00-b",
            Mode: PairMode.Mode1LearnedFlowGrounding,
            Prompt: ReleaseQuery,
            Chosen: new OrchestrationPlanChosen(
                ShortRouteId,
                ShortTitle,
                "simulation:IDENTITY/00-b",
                ShortPlanText),
            RejectedText: "Release the receipt before validating payment.",
            ExpectedRouteMode: GenerationRouteMode.LearnedFlowGrounding,
            ExpectedBoundarySource: BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            ExpectedIntentKind: IntentKind.ImplementationExecutionRequest,
            FlowRef: ShortRouteId,
            SourceBundleId: "simulation:IDENTITY/00-b",
            IsHeldOut: false),
        new OrchestrationTrainingPair(
            PairId: "10-a",
            Mode: PairMode.Mode1LearnedFlowGrounding,
            Prompt: "validate payment approval ledger archive audit compliance evidence journal "
                + "record retention reconciliation balance account transaction customer invoice "
                + "consent signature security trace backup version timestamp report settlement "
                + "currency receipt notification queue pending review deadline exception checksum "
                + "authorization persist recovery replica durability log history status monitoring "
                + "integrity",
            Chosen: new OrchestrationPlanChosen(
                LongRouteId,
                LongTitle,
                "simulation:IDENTITY/10-a",
                LongPlanText),
            RejectedText: "Write the ledger without validating approval.",
            ExpectedRouteMode: GenerationRouteMode.LearnedFlowGrounding,
            ExpectedBoundarySource: BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            ExpectedIntentKind: IntentKind.ImplementationExecutionRequest,
            FlowRef: LongRouteId,
            SourceBundleId: "simulation:IDENTITY/10-a",
            IsHeldOut: false)
    ];
}
