// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using ProjectWikiDomain;

// Что здесь проверяется, по-человечески.
//
// Это перенос из опыта сто восемнадцать: выученный текст плана должен доходить до ответа в
// обобщённом режиме, а не только в выученном.
//
// Что было неправильно, по коду. Сборка ответа обобщённого режима
// (42.ProjectWikiDomain/TrainedRouteComposerUnit.cs, метод RenderMode2Plan) заканчивалась строкой
// с именем режима и возвращала текст. Из выбранного урока в ответ попадал только его НОМЕР —
// строка style_source. Ни одного слова его плана в теле не оставалось никогда, при любом
// обучении. Соседний выученный режим (RenderLearnedFlowPlan) в той же строке кода кладёт тело
// плана дословно через SerializeChosen, то есть способ записи у продукта уже был — он просто не
// применялся на обобщённой дороге.
//
// Что это измерил опыт. Опыт сто восемнадцать проверил это построчно на двух плечах: в выученном
// режиме подмена текста плана меняла тело ответа (0 ответов из 2 с текстом урока стало 2 из 2), а
// в обобщённом режиме тела обоих плеч совпадали по обоим запросам, и выученный текст не доходил
// ни разу — 0 из 2. Правка на копии одного метода довела текст до тела при точном совпадении
// запроса и ни одного ухудшения на остальных запросах не показала.
//
// Условие срабатывания, названное фактически. В опыте это «точное совпадение запроса». В продукте
// такого понятия до этой правки НЕ БЫЛО вовсе: единственная мера близости запроса к уроку — доля
// общих слов в поиске ближайшего примера (OrchestrationKnnStore, метод Jaccard). Поэтому условие
// вводится новым методом IsExactPromptMatch: запросы сравниваются как текст после сведения
// пробелов к одному и приведения к нижнему регистру. Это равнозначно формулировке опыта и
// НАМЕРЕННО СТРОЖЕ существующей меры близости: у запроса с теми же словами в другом порядке доля
// общих слов равна единице, а точного совпадения нет, и тело плана он не получает. Ровно это и
// проверяется отдельным тестом ниже.
//
// Чего эта правка НЕ лечит, и я этого не заявляю. Первое: режим человеческого объяснения
// (RenderHumanExplanation) так же называет номер урока вместо содержания — он этой правкой не
// затронут, и опыт его не измерял. Второе: запрос без точного совпадения по-прежнему получает
// только номер урока; обобщения на похожие, а не совпадающие запросы здесь нет. Третье:
// предметная точность ответа правкой не обещана — если урок учит неверному плану, теперь это
// станет видно в теле, а не спрячется за номером.
//
// Отдельная находка для решения владельца, правкой НЕ закрытая: при точном совпадении запроса
// обобщённый режим всё равно называет себя generalized_non_lookup, то есть «обобщённый, не поиск
// по совпадению». Ровно в том случае, который правка добавляет, это имя становится неточным.
// Переименование — продуктовое решение владельца, и я его не принимаю.
//
// О красном прогоне на прежней логике. Красные по сути, то есть на прежнем коде они обязаны были
// падать: «точный запрос доводит текст плана до ответа», «правка добавляет, а не заменяет» (на
// прежнем коде после метки режима не было ни одной строки, и дописанного тела не существовало) и
// «лишние пробелы и верхний регистр — тот же запрос». Положительные контроли, зелёные и до, и
// после: запрос без точного совпадения отвечает ПОБАЙТОВО прежней пятистрочной формой; те же
// слова в другом порядке тела плана не получают, хотя урок выбран тот же; выученный режим не
// затронут; машина без уроков тела не получает.
//
// Прогона не было: сборка и тесты в этой среде запрещены политикой, я их не запускал и зелёными
// не называю.
public sealed class GeneralizedModePlanBodyTests
{
    // Запрос урока. Взят дословно из опыта сто восемнадцать, чтобы проверка шла на том же тексте,
    // на котором мерили. Цифр и ключевых слов в нём нет, поэтому классификатор границы запроса
    // выдаёт обычное намерение исполнения без области потока, области фазы и обоих требований.
    private const string LessonPrompt = "validate payment ledger release";

    // Запрос, точного совпадения не имеющий: другой набор слов. Это контрольный запрос опыта.
    private const string OtherPrompt = "validate payment approval ledger";

    // Те же слова урока в другом порядке: доля общих слов равна единице, точного совпадения нет.
    private const string ReorderedPrompt = "release ledger payment validate";

    // Поля урока, которые должны оказаться в теле ответа при точном совпадении.
    private const string LessonPairId = "00-b";
    private const string LessonFlowRef = "LAB-B";
    private const string LessonTitle = "Approval ledger plan";
    private const string LessonPlanText = "Validate payment, then save the ledger.";

    // Урок обобщённого режима. Собран руками, потому что проверка должна зависеть только от
    // содержания урока, а не от состава учебного набора проекта.
    private static OrchestrationTrainingPair Mode2Lesson()
        => new(
            LessonPairId,
            PairMode.Mode2Generalization,
            LessonPrompt,
            new OrchestrationPlanChosen(LessonFlowRef, LessonTitle, "docs/lab/lab-b.md", LessonPlanText),
            "Release the receipt before validating payment.",
            GenerationRouteMode.Mode2Generalization,
            BoundarySource.Classifier,
            true,
            IntentKind.ImplementationExecutionRequest,
            LessonFlowRef,
            "lab-bundle",
            false);

    // Машина с ровно одним уроком обобщённого режима. Модель привязки запроса к потоку остаётся
    // пустой, поэтому маршрут гарантированно обобщённый: учить её этот урок не может вовсе.
    private static async Task<OrchestrationPlanMachine> MachineWithLessonAsync()
    {
        var machine = new OrchestrationPlanMachine();
        var trained = await machine.TrainOrchestrationBatchAsync(
            new OrchestrationTrainingBatch("p118-transfer", [Mode2Lesson()]));
        Assert.True(trained.IsSuccess);
        Assert.Equal(1, trained.Value!.Mode2RowsTrained);
        Assert.Equal(0, trained.Value.Mode1RowsTrained);
        return machine;
    }

    // Прежняя форма тела обобщённого режима: пять строк, последняя — имя режима. Записана здесь
    // руками, а не взята у продукта, иначе проверка сверялась бы сама с собой.
    private static string ExpectedBodyBeforeThisEdit(string prompt)
        => string.Join(Environment.NewLine, [
            "ORCHESTRATION_PLAN",
            "intent=" + IntentKind.ImplementationExecutionRequest,
            "prompt=" + prompt,
            "style_source=" + LessonPairId,
            "mode=generalized_non_lookup"
        ]);

    // Сколько раз подстрока встречается в тексте. Нужна, чтобы доказать, что тело плана дописано
    // РОВНО один раз и выученный режим не получил второго.
    private static int CountOccurrences(string text, string needle)
    {
        var count = 0;
        var at = text.IndexOf(needle, StringComparison.Ordinal);
        while (at >= 0)
        {
            count++;
            at = text.IndexOf(needle, at + needle.Length, StringComparison.Ordinal);
        }

        return count;
    }

    // КРАСНЫЙ на прежней логике. Главное утверждение переноса: при точном совпадении запроса
    // выученный текст плана доходит до тела ответа обобщённого режима.
    [Fact]
    public async Task ExactRequest_BringsTheLearnedPlanTextIntoTheAnswer()
    {
        var machine = await MachineWithLessonAsync();
        var result = await machine.GeneratePlanAsync(LessonPrompt);

        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
        Assert.Contains(LessonPlanText, result.Value.PlanText, StringComparison.Ordinal);
        Assert.Contains("FLOW_REF=" + LessonFlowRef, result.Value.PlanText, StringComparison.Ordinal);
        Assert.Contains("TITLE=" + LessonTitle, result.Value.PlanText, StringComparison.Ordinal);
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после. «Ухудшений ноль» проверяется побайтово:
    // запрос без точного совпадения получает прежнюю пятистрочную форму целиком, знак в знак.
    [Fact]
    public async Task RequestWithoutExactMatch_AnswerStaysByteIdenticalToThePreviousShape()
    {
        var machine = await MachineWithLessonAsync();
        var result = await machine.GeneratePlanAsync(OtherPrompt);

        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
        Assert.Equal(ExpectedBodyBeforeThisEdit(OtherPrompt), result.Value.PlanText);
        Assert.DoesNotContain(LessonPlanText, result.Value.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain("FLOW_REF=", result.Value.PlanText, StringComparison.Ordinal);
    }

    // КРАСНЫЙ на прежней логике. Правка ДОБАВЛЯЕТ, а не заменяет: прежние пять строк стоят первыми
    // и не изменились ни на знак, имя режима и ссылка на номер урока остались на своих местах, а
    // тело плана дописано после них и ровно одно.
    [Fact]
    public async Task TheEditAddsAndDoesNotReplace_MarkerAndLessonNumberStayAboveThePlanBody()
    {
        var machine = await MachineWithLessonAsync();
        var result = await machine.GeneratePlanAsync(LessonPrompt);

        Assert.True(result.IsSuccess);
        var previousShape = ExpectedBodyBeforeThisEdit(LessonPrompt);
        Assert.StartsWith(previousShape + Environment.NewLine, result.Value!.PlanText, StringComparison.Ordinal);

        var appended = result.Value.PlanText[(previousShape.Length + Environment.NewLine.Length)..];
        var expectedAppended = string.Join(Environment.NewLine, [
            "FLOW_REF=" + LessonFlowRef,
            "TITLE=" + LessonTitle,
            LessonPlanText
        ]);
        Assert.Equal(expectedAppended, appended);
        Assert.Equal(1, CountOccurrences(result.Value.PlanText, "FLOW_REF="));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после, и он же доказательство того, что условие НЕ
    // расширено. Те же слова в другом порядке: поиск ближайшего примера даёт долю общих слов РОВНО
    // единицу и выбирает тот же урок, а точного совпадения нет, поэтому тела плана запрос не
    // получает и ответ остаётся прежней формой.
    [Fact]
    public async Task SameWordsInAnotherOrder_GetNoPlanBodyAlthoughTheSameLessonIsChosen()
    {
        var store = new OrchestrationKnnStore();
        store.Add(Mode2Lesson());
        var nearest = store.FindNearestWithScore(ReorderedPrompt, PairMode.Mode2Generalization);
        Assert.NotNull(nearest);
        Assert.Equal(LessonPairId, nearest!.Pair.PairId);
        Assert.Equal(1.0, nearest.Score);
        Assert.False(TrainedRouteComposerUnit.IsExactPromptMatch(ReorderedPrompt, LessonPrompt));

        var machine = await MachineWithLessonAsync();
        var result = await machine.GeneratePlanAsync(ReorderedPrompt);

        Assert.True(result.IsSuccess);
        Assert.Contains("style_source=" + LessonPairId, result.Value!.PlanText, StringComparison.Ordinal);
        Assert.Equal(ExpectedBodyBeforeThisEdit(ReorderedPrompt), result.Value.PlanText);
        Assert.DoesNotContain(LessonPlanText, result.Value.PlanText, StringComparison.Ordinal);
    }

    // КРАСНЫЙ на прежней логике. Лишние пробелы, табуляция и верхний регистр — это тот же запрос:
    // сравнение идёт после сведения пробелов и приведения к нижнему регистру, ровно как в опыте.
    [Fact]
    public async Task ExtraSpacesAndUpperCase_StillCountAsTheSameRequest()
    {
        var machine = await MachineWithLessonAsync();
        var result = await machine.GeneratePlanAsync("  VALIDATE   payment\tledger  RELEASE ");

        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
        Assert.Contains(LessonPlanText, result.Value.PlanText, StringComparison.Ordinal);
        Assert.Contains("mode=generalized_non_lookup", result.Value.PlanText, StringComparison.Ordinal);
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после. Выученный режим правкой не затронут: его тело
    // плана приходит как раньше, ровно одно, и при НЕсовпадающем запросе тоже — условие точного
    // совпадения на эту дорогу не перенесено.
    [Fact]
    public void LearnedModeAnswer_IsNotTouchedByThisEdit()
    {
        var composer = new TrainedRouteComposerUnit();
        var grounding = new FlowGroundingResult(LessonFlowRef, 1.0, [], [], [], []);
        var text = composer.RenderLearnedFlowPlan(
            Mode2Lesson(),
            grounding,
            HumanIntentContract.Empty,
            "a request that does not match the lesson at all");

        Assert.Contains("mode=learned_flow_grounding", text, StringComparison.Ordinal);
        Assert.DoesNotContain("mode=generalized_non_lookup", text, StringComparison.Ordinal);
        Assert.Contains(LessonPlanText, text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "FLOW_REF="));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после. Без уроков выбирать нечего: ни ссылки на номер
    // урока, ни тела плана в ответе нет, и правка ничего не дописывает.
    [Fact]
    public async Task MachineWithoutLessons_HasNeitherLessonNumberNorPlanBody()
    {
        var result = await new OrchestrationPlanMachine().GeneratePlanAsync(LessonPrompt);

        Assert.True(result.IsSuccess);
        Assert.Equal(GenerationRouteMode.Mode2Generalization, result.Value!.RouteMode);
        Assert.DoesNotContain("style_source=", result.Value.PlanText, StringComparison.Ordinal);
        Assert.DoesNotContain("FLOW_REF=", result.Value.PlanText, StringComparison.Ordinal);
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после. Границы условия названы прямо: отсутствующий
    // текст не роняет сравнение, потому что сведение к одной строке превращает отсутствие в пустую
    // строку. Две пустые строки считаются совпадающими — это важно только для урока с пустым
    // запросом, и я называю это поведение, а не прячу.
    [Fact]
    public void TheConditionDoesNotThrowOnAbsentText()
    {
        Assert.True(TrainedRouteComposerUnit.IsExactPromptMatch(null!, null!));
        Assert.False(TrainedRouteComposerUnit.IsExactPromptMatch(LessonPrompt, null!));
        Assert.False(TrainedRouteComposerUnit.IsExactPromptMatch(null!, LessonPrompt));
    }
}
