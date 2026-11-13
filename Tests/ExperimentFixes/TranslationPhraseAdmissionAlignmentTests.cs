// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ, ПО-ЧЕЛОВЕЧЕСКИ
//
// Переводчик собирает ответ из выученных фраз в два приёма. Сначала планировщик
// выбирает, как разрезать предложение на выученные фразы. Потом вызывающий код
// применяет к выбранному разрезу правило допуска и может его отвергнуть.
//
// Правило допуска пропускает ответ в двух случаях: покрыто не меньше 85 процентов
// слов источника, ЛИБО ответ собран не меньше чем из двух фраз и покрыто не меньше
// 50 процентов. Планировщик же искал другое: больше покрытых слов, а при равном
// покрытии — МЕНЬШЕ отдельных фраз. Второе прямо противоположно второму случаю
// допуска: поиск сокращал число фраз, тогда как допуску нужно иметь их две.
//
// Отсюда дефект: существует вход, на котором годный ответ есть, но предъявлен
// другой — и ответа не получает никто. Два таких входа проверяются ниже.
//
// Вход первый, восемь слов «close user session window panel list item now» и три
// выученные фразы. Длинная фраза покрывает шесть слов из восьми одна: это 75
// процентов, до 85 не хватает, а вторым случаем воспользоваться нельзя — фраза
// одна. Разрез из двух фраз покрывает четыре слова из восьми, это ровно 50
// процентов, и он проходит. Прежний поиск предъявлял длинную фразу, и ответа не
// было.
//
// Вход второй, шесть слов «close user session window panel list». Длинная фраза и
// пара коротких покрывают одинаково — четыре слова из шести. Правило «меньше фраз
// лучше» выбирало одну длинную: 66,7 процента до 85 не дотягивают, фраза одна,
// отказ. Пара коротких на том же покрытии проходит вторым случаем.
//
// ЧТО ТЕПЕРЬ ТРЕБУЕТСЯ: выбранный разрез — наилучший среди ТЕХ, что проходят
// действующее правило допуска; если допустимого разреза нет, планировщик честно
// сообщает об отсутствии плана, и продукт уходит в свой прежний резервный путь.
// Порог 85 процентов и условие двух фраз при 50 процентах не меняются и во второй
// файл не копируются: их единственный владелец — обученные единицы перевода,
// которые передают правило допуска планировщику как предикат.
//
// ЧЕГО ЭТИ ПРОВЕРКИ НЕ ДОКАЗЫВАЮТ: что наибольшее допустимое покрытие означает
// понимание роли каждого слова; качество перевода на живых данных; время и память.
// Ожидания записаны до прогона в
// artifacts/fbe/AUDIT-A07-PHRASE-ADMISSION/attempt-0001/pre-registered-expectations.md.

namespace LlmFromScratch.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using LlmFromScratch.Tests.ExperimentFixes;
using TranslationDomain;

// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class TranslationPhraseAdmissionAlignmentTests
    : IClassFixture<TranslationPhraseAdmissionAlignmentTests.AttemptScopeFixture>
{
    // ── Вход первый: больший охват вытеснил допустимый разрез из двух фраз ─────

    // Восемь слов источника: ровно предел разрешённой области планировщика.
    private const string WideSource = "close user session window panel list item now";

    // Выученная фраза на местах 1-2.
    private const string WideShortOneKey = "close user";
    private const string WideShortOneTarget = "закрой пользователя";

    // Выученная фраза на местах 5-6.
    private const string WideShortTwoKey = "panel list";
    private const string WideShortTwoTarget = "список панели";

    // Выученная фраза на местах 2-7: она покрывает больше, но одна и потому недопустима.
    private const string WideLongKey = "user session window panel list item";
    private const string WideLongTarget = "окно сеанса пользователя со списком панели";

    // Золотой ответ входа: две короткие фразы в порядке мест.
    private const string WideExpected = "закрой пользователя список панели";

    // ── Вход второй: правило равенства предпочло недопустимый вариант ──────────

    // Шесть слов источника.
    private const string TieSource = "close user session window panel list";

    // Выученная фраза на местах 1-2.
    private const string TieShortOneKey = "close user";
    private const string TieShortOneTarget = "закрой пользователя";

    // Выученная фраза на местах 3-4.
    private const string TieShortTwoKey = "session window";
    private const string TieShortTwoTarget = "окно сеанса";

    // Выученная фраза на местах 2-5: покрывает столько же, сколько две короткие вместе.
    private const string TieLongKey = "user session window panel";
    private const string TieLongTarget = "панель окна сеанса пользователя";

    // Золотой ответ входа: две короткие фразы в порядке мест.
    private const string TieExpected = "закрой пользователя окно сеанса";

    // Опознаватель источника уроков этой проверки.
    private const string SourceId = "a07-phrase-admission-alignment-v1";

    [Fact]
    public async Task ProductPath_WiderCoverageWouldBeRefused_StillAnswersWithTheAdmissiblePlan()
    {
        // Машина перевода учит три фразы входа первого одной партией.
        var machine = new TranslationMachine();
        // Обучение обязано пройти, иначе выученной памяти нет и проверять нечего.
        var train = await machine.TrainStructuredBatchAsync(BuildBatch(
            "a07-wide-coverage-batch",
            (WideShortOneKey, WideShortOneTarget),
            (WideShortTwoKey, WideShortTwoTarget),
            (WideLongKey, WideLongTarget)));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        // Экзаменационная строка читается через весь продуктовый путь.
        var actual = await TranslateEnRuAsync(machine, WideSource);

        // Ответ пришёл из допустимого разреза двух фраз, а не потерялся на отказе.
        Assert.Equal(WideExpected, actual);
        // Договорённый резерв здесь означал бы, что годный разрез так и не предъявлен.
        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
    }

    [Fact]
    public async Task ProductPath_EqualCoverageTie_AnswersWithTheAdmissibleTwoPhrasePlan()
    {
        // Машина перевода учит три фразы входа второго одной партией.
        var machine = new TranslationMachine();
        // Обучение обязано пройти, иначе выученной памяти нет.
        var train = await machine.TrainStructuredBatchAsync(BuildBatch(
            "a07-equal-coverage-batch",
            (TieShortOneKey, TieShortOneTarget),
            (TieShortTwoKey, TieShortTwoTarget),
            (TieLongKey, TieLongTarget)));
        Assert.True(train.IsSuccess, train.ErrorMessage);

        // Экзаменационная строка читается через весь продуктовый путь.
        var actual = await TranslateEnRuAsync(machine, TieSource);

        // При равном покрытии выбран тот разрез, который правило допуска пропускает.
        Assert.Equal(TieExpected, actual);
        // И это не договорённый резерв.
        Assert.DoesNotContain("[EN_RU:BOOTSTRAP]", actual);
    }

    // ── Уровень алгоритма: тот же спор без обучения и без продуктового пути ────

    [Fact]
    public void Planner_WiderCoverageIsNotAdmissible_ChoosesTheAdmissibleTwoPhrasePlan()
    {
        // Вход первый на уровне алгоритма: восемь слов и три выученные фразы.
        var tokens = WideTokens();

        // Выбор с правилом допуска как ограничением.
        var plan = TranslationPhraseCompositionPlanner.PlanAdmissible(
            tokens, WideEdges(), candidate => IsAdmissibleLikeTheOwner(candidate, tokens.Length));

        // Выбран разрез из двух коротких фраз: четыре покрытых слова из восьми.
        Assert.Equal(4, plan.CoveredSourceTokens);
        Assert.Equal(2, plan.PhraseSpanCount);
        Assert.Equal(new[] { WideShortOneKey, WideShortTwoKey }, plan.ChosenSourceKeys);
        Assert.Equal(new[] { WideShortOneTarget, WideShortTwoTarget }, plan.Parts);
        // И выбранный разрез действительно проходит то самое правило допуска.
        Assert.True(IsAdmissibleLikeTheOwner(plan, tokens.Length));
    }

    [Fact]
    public void Planner_EqualCoverageTie_ChoosesTheAdmissibleTwoPhrasePlan()
    {
        // Вход второй на уровне алгоритма: шесть слов и три выученные фразы.
        var tokens = TieTokens();

        // Тот же выбор с тем же правилом допуска.
        var plan = TranslationPhraseCompositionPlanner.PlanAdmissible(
            tokens, TieEdges(), candidate => IsAdmissibleLikeTheOwner(candidate, tokens.Length));

        // При равном покрытии выбран тот разрез, который допуск пропускает.
        Assert.Equal(4, plan.CoveredSourceTokens);
        Assert.Equal(2, plan.PhraseSpanCount);
        Assert.Equal(new[] { TieShortOneKey, TieShortTwoKey }, plan.ChosenSourceKeys);
        Assert.Equal(new[] { TieShortOneTarget, TieShortTwoTarget }, plan.Parts);
        // Проверка того же правила на выбранном разрезе.
        Assert.True(IsAdmissibleLikeTheOwner(plan, tokens.Length));
    }

    [Fact]
    public void Planner_SingleAnswerSearch_StillHandsOverTheRefusedPlan()
    {
        // Эта проверка фиксирует сам дефект, а не ремонт: прежний поиск одного ответа
        // по-прежнему предъявляет разрез, который правило допуска отвергает. Так видно,
        // что расхождение реально и что новый выбор исправляет именно его, а не что-то
        // другое. Прежний метод намеренно не менялся: он отвечает за ранее принятые
        // ответы ДВАДЦАТИ проверок покрытия фраз — число сверено 12.11.2026 прямым счётом
        // по файлу `TranslationPhraseCoverageLearningTests.cs`. Прежде здесь стояло
        // «четырнадцати»: это было верно до того, как в тот файл добавили три проверки
        // восстановления состояния и три проверки расхода памяти.
        var wideTokens = WideTokens();
        // Прежний поиск на входе первом.
        var widePlan = TranslationPhraseCompositionPlanner.Plan(wideTokens, WideEdges());
        // Он берёт длинную фразу: шесть покрытых слов, одна фраза.
        Assert.Equal(6, widePlan.CoveredSourceTokens);
        Assert.Equal(1, widePlan.PhraseSpanCount);
        // И этот разрез правило допуска отвергает: 0,75 меньше 0,85 при одной фразе.
        Assert.False(IsAdmissibleLikeTheOwner(widePlan, wideTokens.Length));

        var tieTokens = TieTokens();
        // Прежний поиск на входе втором.
        var tiePlan = TranslationPhraseCompositionPlanner.Plan(tieTokens, TieEdges());
        // Правило равенства предпочло одну длинную фразу.
        Assert.Equal(4, tiePlan.CoveredSourceTokens);
        Assert.Equal(1, tiePlan.PhraseSpanCount);
        // И он тоже отвергается: 0,667 меньше 0,85 при одной фразе.
        Assert.False(IsAdmissibleLikeTheOwner(tiePlan, tieTokens.Length));
    }

    [Fact]
    public void Planner_NoAdmissiblePlanExists_HonestlyReportsNoPlan()
    {
        // Восьмисловный край принятого переноса покрытия фраз: наибольшее покрытие двумя
        // фразами равно трём словам из восьми, то есть 0,375 — меньше половины. Допустимого
        // разреза здесь нет вовсе, и этот отказ ПРАВИЛЬНЫЙ. Ремонт обязан его сохранить:
        // порог не снижается, а честный «плана нет» уводит продукт в его прежний резерв.
        string[] tokens = ["close", "user", "session", "now", "please", "again", "today", "safely"];
        // Те же три урока, что у принятого переноса.
        TranslationPhraseEdge[] edges =
        [
            new("close user", ["close", "user"], "закрой пользователя"),
            new("user session", ["user", "session"], "сеанс пользователя"),
            new("close", ["close"], "закрой")
        ];

        // Выбор с правилом допуска.
        var plan = TranslationPhraseCompositionPlanner.PlanAdmissible(
            tokens, edges, candidate => IsAdmissibleLikeTheOwner(candidate, tokens.Length));

        // Плана нет: ни покрытия, ни фраз, ни частей ответа.
        Assert.Equal(0, plan.CoveredSourceTokens);
        Assert.Equal(0, plan.PhraseSpanCount);
        Assert.Empty(plan.Parts);
        Assert.Empty(plan.ChosenSourceKeys);
        // Пустой ответ сам не проходит правило допуска, поэтому продукт его не выдаст.
        Assert.False(IsAdmissibleLikeTheOwner(plan, tokens.Length));
    }

    [Fact]
    public void Planner_AdmitEverything_GivesTheSameAnswerAsTheSingleAnswerSearch()
    {
        // Когда правило допуска ничего не отвергает, новый выбор обязан совпасть с прежним
        // до последнего ключа: это и есть доказательство, что ремонт сузил выбор правилом
        // допуска, а не поменял сам порядок предпочтения.
        string[][] inputs =
        [
            WideTokens(),
            TieTokens(),
            ["close", "user", "session"],
            ["close", "user", "session", "now", "please", "again", "today", "safely"],
            ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"]
        ];
        // Три набора выученной памяти: два контрпримера, уроки переноса и плотная память.
        TranslationPhraseEdge[][] memories = [WideEdges(), TieEdges(), LessonEdges(), DenseEdges()];

        foreach (var tokens in inputs)
            foreach (var memory in memories)
            {
                // Прежний поиск одного ответа.
                var single = TranslationPhraseCompositionPlanner.Plan(tokens, memory);
                // Новый поиск с правилом «допустимо всё».
                var admitAll = TranslationPhraseCompositionPlanner.PlanAdmissible(tokens, memory, _ => true);

                // Покрытие, число фраз, ключи и части совпадают полностью.
                Assert.Equal(single.CoveredSourceTokens, admitAll.CoveredSourceTokens);
                Assert.Equal(single.PhraseSpanCount, admitAll.PhraseSpanCount);
                Assert.Equal(single.ChosenSourceKeys, admitAll.ChosenSourceKeys);
                Assert.Equal(single.Parts, admitAll.Parts);
            }
    }

    [Fact]
    public void Planner_TwoSelectionRules_CarryTwoDistinctIdentities()
    {
        // Опознаватель правила выбора нужен для того, чтобы читатель квитанции мог сказать,
        // какое правило дало запись. До этого ремонта значение было одно на оба пути, и записи
        // двух РАЗНЫХ семантик были неразличимы: прежняя — «лучший по покрытию, при равенстве
        // меньше фраз», новая — «лучший среди допустимых». Теперь у каждого пути своё значение.
        Assert.Equal("phrase-coverage-canonical-v1", TranslationPhraseCompositionPlanner.AlgorithmIdentity);
        Assert.Equal("phrase-coverage-admissible-v2", TranslationPhraseCompositionPlanner.AdmissibleAlgorithmIdentity);
        // Два значения обязаны различаться, иначе разделение семантик существует лишь на словах.
        Assert.NotEqual(
            TranslationPhraseCompositionPlanner.AlgorithmIdentity,
            TranslationPhraseCompositionPlanner.AdmissibleAlgorithmIdentity);
        // Прежнее значение сохранено намеренно: проверки и квитанции, уже несущие его, описывают
        // именно прежний поиск одного ответа, и переименование молча переклеило бы им ярлык.
        Assert.Equal(8, TranslationPhraseCompositionPlanner.MaxPlannedSourceTokens);
    }

    // ── Охрана расхода памяти НОВОГО пути настоящими числами ──────────────────
    //
    // ЗАЧЕМ ЗДЕСЬ АБСОЛЮТНЫЕ ПРЕДЕЛЫ, А НЕ ТОЛЬКО ОТНОСИТЕЛЬНЫЕ. Абсолютные бюджеты
    // принятого переноса покрытия фраз — 18 496 байта на шесть чтений и 9 312 байт на
    // плотное чтение — измеряют ПРЕЖНИЙ метод выбора, потому что их проверки вызывают
    // именно его. После проводки продукта на новый метод прежний перестанет быть
    // продуктовым путём, и те два числа перестанут охранять продукт. Охрана, которая
    // перестала охранять, — отдельный класс дефекта, поэтому у нового пути есть своё
    // абсолютное число, снятое с него самого.
    //
    // КАК ВЫВЕДЕН ЗАПАС. Наблюдённые числа получены при нулевом разбросе по семи
    // повторам, то есть измерение детерминированное, и запас нужен не под шум, а под
    // законный мелкий дрейф: смену версии среды выполнения, изменение внутреннего размера
    // списка, добавление поля в запись плана. Запас взят равным уровню существенности
    // владельца — пять процентов, — и округлён вверх до сотни байт. Это значит: рост
    // больше примерно пяти процентов краснеет, а рост в пределах пяти процентов проходит
    // зелёным. Полоса ровно по границе существенности владельца, не уже и не шире.
    //
    // ЧЕГО ЭТИ ПРЕДЕЛЫ НЕ ДОКАЗЫВАЮТ. Они измеряют плечи планировщика, а не целое
    // продуктовое чтение, и ничего не говорят о времени. Числа сняты на этой машине для
    // целевой платформы net9.0; на другой среде выполнения наблюдённое значение может
    // законно отличаться, и тогда предел переназначается от нового наблюдения, а не
    // поднимается ради зелёного ответа.

    // Плотное чтение области: восемь слов, четырнадцать выученных фраз. Наблюдено 13 552
    // байта при нулевом разбросе по семи повторам (прогон 11.11.2026); предел — это
    // наблюдённое плюс пять процентов с округлением вверх до сотни, то есть плюс 5,5 %.
    private const long AdmissibleSearchBudgetBytesDenseReading = 14_300;

    // Панель принятого переноса: шесть его чтений, три его выученные фразы. Наблюдено
    // 9 864 байта при нулевом разбросе по семи повторам (тот же прогон); предел — плюс
    // пять процентов с округлением вверх до сотни, то есть плюс 5,4 %.
    private const long AdmissibleSearchBudgetBytesAcceptedPanel = 10_400;

    // Второй, грубый рубеж: предохранитель от обвала. Новый выбор держит до девяти рядов
    // вместо одного при восьми словах, поэтому двадцатикратный расход заведомо больше
    // законного и ловит только катастрофу — например возврат к хранению всех готовых
    // вариантов. Он оставлен рядом с абсолютным пределом намеренно: абсолютное число
    // привязано к среде, а относительное переживает её смену.
    private const int AdmissibleSearchCatastropheFuseTimes = 20;

    [Fact]
    public void Planner_AdmissibleSearch_AllocationStaysBoundedOnTheDensestAdmittedCase()
    {
        string[] tokens = ["a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"];
        // Самая плотная память внутри области: четырнадцать выученных фраз на восемь слов.
        var memory = DenseEdges();
        // Правило допуска той же формы, что у владельца.
        var admission = (TranslationPhrasePlan candidate) => IsAdmissibleLikeTheOwner(candidate, tokens.Length);

        // Прогрев: первая цена компиляции не входит в измерение ни у одного плеча.
        AllocationSink = TranslationPhraseCompositionPlanner.Plan(tokens, memory).CoveredSourceTokens;
        AllocationSink = TranslationPhraseCompositionPlanner.PlanAdmissible(tokens, memory, admission).CoveredSourceTokens;

        // Семь чередующихся повторов, чтобы медиана была одним наблюдённым значением.
        const int repeats = 7;
        var singleBytes = new long[repeats];
        var admissibleBytes = new long[repeats];
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            // Плечо прежнего поиска.
            singleBytes[repeat] = MeasureThreadAllocatedBytes(() =>
                TranslationPhraseCompositionPlanner.Plan(tokens, memory).CoveredSourceTokens);
            // Плечо нового выбора с правилом допуска.
            admissibleBytes[repeat] = MeasureThreadAllocatedBytes(() =>
                TranslationPhraseCompositionPlanner.PlanAdmissible(tokens, memory, admission).CoveredSourceTokens);
        }

        // Медианы обоих плеч.
        var singleMedian = Median(singleBytes);
        var admissibleMedian = Median(admissibleBytes);

        // Измеритель живой: оба плеча что-то выделяют, иначе показание ничего не значит.
        Assert.True(singleMedian > 0, "прежний поиск выделил 0 байт — измеритель не живой");
        Assert.True(admissibleMedian > 0, "новый выбор выделил 0 байт — измеритель не живой");
        // Первый рубеж — абсолютный предел НОВОГО пути, снятый с него самого: наблюдённое
        // плюс уровень существенности владельца в пять процентов.
        Assert.True(admissibleMedian < AdmissibleSearchBudgetBytesDenseReading,
            $"новый выбор выделил {admissibleMedian} байт на плотное чтение, а это не ниже предела {AdmissibleSearchBudgetBytesDenseReading} (наблюдённое 13 552 плюс пять процентов)");
        // Второй, грубый рубеж — предохранитель от обвала относительно прежнего поиска.
        Assert.True(admissibleMedian < singleMedian * AdmissibleSearchCatastropheFuseTimes,
            $"новый выбор выделил {admissibleMedian} байт против {singleMedian} у прежнего поиска, это не ниже {AdmissibleSearchCatastropheFuseTimes}-кратного предохранителя");

        // Улика: абсолютные числа по повторам, медианы и отношение.
        WriteReceipt("allocation-admissible-search-versus-single-answer-dense.json", new
        {
            // Опознаватели правил выбора обоих плеч: без них записи двух разных семантик
            // неразличимы, и читатель квитанции не может сказать, какое правило её дало.
            AdmissibleArmAlgorithmIdentity = TranslationPhraseCompositionPlanner.AdmissibleAlgorithmIdentity,
            SingleAnswerArmAlgorithmIdentity = TranslationPhraseCompositionPlanner.AlgorithmIdentity,
            Compared = "TranslationPhraseCompositionPlanner.PlanAdmissible with the owner-shaped admission predicate versus TranslationPhraseCompositionPlanner.Plan, same eight-word reading, same fourteen learned phrases (DenseEdges)",
            FirstGuard = $"absolute budget for the NEW path: {AdmissibleSearchBudgetBytesDenseReading} bytes = observed 13 552 plus the owner's five-percent materiality, rounded up to the next hundred (+5.5 percent); growth beyond roughly five percent turns this red",
            SecondGuard = $"catastrophe fuse: below {AdmissibleSearchCatastropheFuseTimes} times the previous search on the same input; it survives a change of runtime, which an absolute number does not",
            SingleAnswerBytesPerRepeat = singleBytes,
            AdmissibleBytesPerRepeat = admissibleBytes,
            SingleAnswerMedianBytes = singleMedian,
            AdmissibleMedianBytes = admissibleMedian,
            TimesOfSingleAnswer = singleMedian == 0 ? double.NaN : Math.Round(admissibleMedian / (double)singleMedian, 3),
            WhyTheOldBudgetsNoLongerGuard = "the accepted transfer's absolute budgets (18 496 bytes for six readings, 9 312 for the dense reading) are asserted against TranslationPhraseCompositionPlanner.Plan, which stops being the product path once the product is wired to PlanAdmissible; they then guard a method the product no longer calls and must be reassigned by the owner of that transfer",
            Boundary = "planner arms only, not the whole product reading path; no timing measured; numbers observed on this machine for target net9.0"
        });
    }

    [Fact]
    public void Planner_AdmissibleSearch_AllocationOnTheAcceptedSixReadingPanel_IsRecorded()
    {
        // ЗАЧЕМ ЭТО ИЗМЕРЕНИЕ ОТДЕЛЬНО ОТ ПЛОТНОГО ВХОДА. Владелец принятого переноса
        // покрытия фраз сравнивает расход по ШЕСТИ чтениям своей панели и по доле в целом
        // продуктовом чтении. Плотный вход из четырнадцати выученных фраз — верхняя оценка
        // роста, а не та величина, с которой владелец сверяет свой порог. Здесь берутся
        // ровно те шесть чтений и ровно те три выученные фразы, поэтому число сравнимо с
        // принятым показанием прежнего поиска. Утверждение остаётся предохранителем от
        // катастрофы; решение о допуске по расходу принимает владелец.
        string[][] readings =
        [
            ["close", "user", "session"],
            ["close", "user"],
            ["user", "session"],
            ["close"],
            ["close", "user", "session", "now", "please", "again", "today", "safely"],
            ["close", "user", "session", "now", "please", "again", "today", "safely", "tomorrow"]
        ];
        // Три урока принятого переноса — та же выученная память, что у его показаний.
        var memory = LessonEdges();

        // Прогрев обоих плеч: первая цена компиляции в измерение не входит.
        AllocationSink = RunSingleAnswerArm(readings, memory);
        AllocationSink = RunAdmissibleArm(readings, memory);

        // Семь чередующихся повторов, медиана — одно наблюдённое значение.
        const int repeats = 7;
        var singleBytes = new long[repeats];
        var admissibleBytes = new long[repeats];
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            // Плечо прежнего поиска одного ответа на всех шести чтениях.
            singleBytes[repeat] = MeasureThreadAllocatedBytes(() => RunSingleAnswerArm(readings, memory));
            // Плечо нового выбора с правилом допуска на тех же шести чтениях.
            admissibleBytes[repeat] = MeasureThreadAllocatedBytes(() => RunAdmissibleArm(readings, memory));
        }

        // Медианы обоих плеч.
        var singleMedian = Median(singleBytes);
        var admissibleMedian = Median(admissibleBytes);

        // Измеритель живой у обоих плеч.
        Assert.True(singleMedian > 0, "прежний поиск выделил 0 байт — измеритель не живой");
        Assert.True(admissibleMedian > 0, "новый выбор выделил 0 байт — измеритель не живой");
        // Первый рубеж — абсолютный предел нового пути на ЭТОЙ панели: именно та величина,
        // по которой владелец переноса сверяет свой порог.
        Assert.True(admissibleMedian < AdmissibleSearchBudgetBytesAcceptedPanel,
            $"новый выбор выделил {admissibleMedian} байт на панели переноса, а это не ниже предела {AdmissibleSearchBudgetBytesAcceptedPanel} (наблюдённое 9 864 плюс пять процентов)");
        // Второй, грубый рубеж — тот же предохранитель от обвала.
        Assert.True(admissibleMedian < singleMedian * AdmissibleSearchCatastropheFuseTimes,
            $"новый выбор выделил {admissibleMedian} байт против {singleMedian} у прежнего поиска, это не ниже {AdmissibleSearchCatastropheFuseTimes}-кратного предохранителя");

        // Улика: числа, сравнимые с принятым показанием владельца по шести чтениям.
        WriteReceipt("allocation-admissible-search-accepted-six-reading-panel.json", new
        {
            // Те же два опознавателя: продуктовый путь несёт новое значение, прежний поиск — своё.
            AdmissibleArmAlgorithmIdentity = TranslationPhraseCompositionPlanner.AdmissibleAlgorithmIdentity,
            SingleAnswerArmAlgorithmIdentity = TranslationPhraseCompositionPlanner.AlgorithmIdentity,
            Compared = "PlanAdmissible with the owner-shaped predicate versus Plan, on the accepted transfer's own six readings and its own three learned phrases",
            WhyThisPanel = "the owner's accepted allocation verdict is stated per six readings and as a share of the whole product reading; the dense fourteen-phrase input is an upper bound, not that quantity",
            FirstGuard = $"absolute budget for the NEW path on this panel: {AdmissibleSearchBudgetBytesAcceptedPanel} bytes = observed 9 864 plus the owner's five-percent materiality, rounded up to the next hundred (+5.4 percent)",
            SecondGuard = $"catastrophe fuse: below {AdmissibleSearchCatastropheFuseTimes} times the previous search on the same readings",
            SingleAnswerBytesPerRepeat = singleBytes,
            AdmissibleBytesPerRepeat = admissibleBytes,
            SingleAnswerMedianBytes = singleMedian,
            AdmissibleMedianBytes = admissibleMedian,
            TimesOfSingleAnswer = singleMedian == 0 ? double.NaN : Math.Round(admissibleMedian / (double)singleMedian, 3),
            OwnerPublishedNumbersForContext = "accepted transfer published 9 248 bytes for the installed planner on six readings and 239 488 bytes for the whole product reading of six rows, planner share about 3.3 percent declared as a LOWER bound",
            WhyTheOldBudgetsNoLongerGuard = "the accepted transfer's 18 496-byte six-reading budget is asserted against Plan; once the product calls PlanAdmissible instead, that budget guards a method the product no longer uses, and the owner of that transfer must reassign it",
            Boundary = "this test measures the planner arms only; it does not re-measure the whole product reading path and does not pronounce the owner's five-percent verdict"
        });
    }

    // ── вспомогательное: правило допуска и входы уровня алгоритма ──────────────

    // IsAdmissibleLikeTheOwner — ровно то правило допуска, которое применяет владелец
    // (метод составления из карт фраз в обученных единицах перевода): покрытие не ниже
    // 85 процентов ЛИБО не меньше двух фраз при покрытии не ниже 50 процентов, и ни
    // пустых фраз, ни пустого ответа. Внутри области планировщика у слов нет принятых
    // однословных переводов, поэтому покрытые слова и покрытые фразами слова — одно и
    // то же число; это сказано в самом продукте рядом с вызовом планировщика.
    //
    // Числа здесь живут в ПРОВЕРКЕ, а не во втором владельце продукта: продукт передаёт
    // своё правило планировщику как предикат, и единственный владелец порога остаётся
    // там, где он был.
    private static bool IsAdmissibleLikeTheOwner(TranslationPhrasePlan plan, int sourceTokenCount)
    {
        // Ответ без фраз или без частей не годится ни по одному пути допуска.
        if (plan.PhraseSpanCount == 0 || plan.Parts.Count == 0)
            return false;

        // Доля покрытых слов источника.
        var coverage = plan.CoveredSourceTokens / (double)sourceTokenCount;
        // Второй путь допуска: две и более фразы при покрытии не ниже половины.
        var repeatedPhraseCoverage = plan.PhraseSpanCount >= 2 && coverage >= 0.50;
        // Годится, если прошёл любой из двух путей.
        return repeatedPhraseCoverage || coverage >= 0.85;
    }

    // WideTokens — нормализованные слова входа первого.
    private static string[] WideTokens()
        => ["close", "user", "session", "window", "panel", "list", "item", "now"];

    // WideEdges — три выученные фразы входа первого в виде входа планировщика.
    private static TranslationPhraseEdge[] WideEdges()
        =>
        [
            new(WideShortOneKey, ["close", "user"], WideShortOneTarget),
            new(WideShortTwoKey, ["panel", "list"], WideShortTwoTarget),
            new(WideLongKey, ["user", "session", "window", "panel", "list", "item"], WideLongTarget)
        ];

    // TieTokens — нормализованные слова входа второго.
    private static string[] TieTokens()
        => ["close", "user", "session", "window", "panel", "list"];

    // TieEdges — три выученные фразы входа второго.
    private static TranslationPhraseEdge[] TieEdges()
        =>
        [
            new(TieShortOneKey, ["close", "user"], TieShortOneTarget),
            new(TieShortTwoKey, ["session", "window"], TieShortTwoTarget),
            new(TieLongKey, ["user", "session", "window", "panel"], TieLongTarget)
        ];

    // LessonEdges — три урока принятого переноса покрытия фраз.
    private static TranslationPhraseEdge[] LessonEdges()
        =>
        [
            new("close user", ["close", "user"], "закрой пользователя"),
            new("user session", ["user", "session"], "сеанс пользователя"),
            new("close", ["close"], "закрой")
        ];

    // DenseEdges — плотная память принятого переноса: две длинные фразы, четыре парные и
    // восемь одиночных на одни и те же восемь слов источника.
    private static TranslationPhraseEdge[] DenseEdges()
        =>
        [
            new("a1 a2 a3 a4", ["a1", "a2", "a3", "a4"], "T1234"),
            new("a5 a6 a7 a8", ["a5", "a6", "a7", "a8"], "T5678"),
            new("a1 a2", ["a1", "a2"], "T12"),
            new("a3 a4", ["a3", "a4"], "T34"),
            new("a5 a6", ["a5", "a6"], "T56"),
            new("a7 a8", ["a7", "a8"], "T78"),
            new("a1", ["a1"], "T1"),
            new("a2", ["a2"], "T2"),
            new("a3", ["a3"], "T3"),
            new("a4", ["a4"], "T4"),
            new("a5", ["a5"], "T5"),
            new("a6", ["a6"], "T6"),
            new("a7", ["a7"], "T7"),
            new("a8", ["a8"], "T8")
        ];

    // ── вспомогательное: измерение расхода памяти ──────────────────────────────

    // Приёмник возвращаемого значения плеча: чтобы работу плеча нельзя было счесть мёртвой.
    private static volatile int AllocationSink;

    // RunSingleAnswerArm прогоняет плечо прежнего поиска одного ответа по набору чтений и
    // отдаёт контрольную сумму, чтобы работу плеча нельзя было счесть мёртвой.
    private static int RunSingleAnswerArm(string[][] readings, TranslationPhraseEdge[] memory)
    {
        // Контрольная сумма складывается из покрытий и числа частей ответа.
        var checksum = 0;
        foreach (var tokens in readings)
        {
            // Одно чтение плеча.
            var plan = TranslationPhraseCompositionPlanner.Plan(tokens, memory);
            checksum += plan.CoveredSourceTokens * 31 + plan.Parts.Count;
        }

        return checksum;
    }

    // RunAdmissibleArm прогоняет плечо нового выбора с правилом допуска по тому же набору
    // чтений: правило строится на каждое чтение, потому что оно зависит от длины чтения —
    // ровно так же это будет делать продукт после проводки.
    private static int RunAdmissibleArm(string[][] readings, TranslationPhraseEdge[] memory)
    {
        // Та же контрольная сумма для того же набора чтений.
        var checksum = 0;
        foreach (var tokens in readings)
        {
            // Одно чтение плеча вместе с построением правила допуска для его длины.
            var plan = TranslationPhraseCompositionPlanner.PlanAdmissible(
                tokens, memory, candidate => IsAdmissibleLikeTheOwner(candidate, tokens.Length));
            checksum += plan.CoveredSourceTokens * 31 + plan.Parts.Count;
        }

        return checksum;
    }

    // MeasureThreadAllocatedBytes измеряет байты, выделенные ТЕКУЩИМ потоком внутри
    // действия. Действие синхронно и поток не меняет, иначе показание нельзя приписать ему.
    private static long MeasureThreadAllocatedBytes(Func<int> action)
    {
        // Начальное показание счётчика потока; делегат создан вызывающим кодом до этой строки.
        var start = GC.GetAllocatedBytesForCurrentThread();
        // Само измеряемое действие.
        AllocationSink = action();
        // Разница показаний — байты действия.
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    // Median отдаёт медиану: для нечётного числа значений — среднее по порядку.
    private static long Median(long[] values)
    {
        // Значения упорядочиваются по возрастанию.
        var sorted = values.OrderBy(value => value).ToArray();
        // Середина упорядоченного ряда.
        return sorted[sorted.Length / 2];
    }

    // ── вспомогательное: улики ─────────────────────────────────────────────────

    // Приёмник улик этой попытки: каталог наряда
    // artifacts/fbe/AUDIT-A07-PHRASE-ADMISSION/attempt-0001, подкаталог receipts.
    //
    // Почему ленивое поле заменено фикстурой класса. Ленивое поле рождало приёмник при ПЕРВОЙ записи
    // улики, то есть уже после уроков обучения этого испытания. Фикстура класса создаётся раньше
    // конструктора теста, поэтому захват адреса стал первым действием попытки, а завершение попытки
    // ставится ровно один раз.

    /// <summary>Корень рабочей копии для попытки этого испытания; его отсутствие — настоящий отказ.</summary>
    private static string ReceiptRepositoryRoot()
        // Отсутствие корня означает, что улики писать некуда, и это называется прямо.
        => RepositoryRootOrNull()
            ?? throw new InvalidOperationException("корень хранилища не найден подъёмом от " + AppContext.BaseDirectory);

    /// <summary>Назначенный каталог улик этого наряда: адрес не меняется этим ремонтом.</summary>
    private static string ReceiptDeclaredDirectory()
        // Тот же адрес, что был у ленивого поля: artifacts/fbe/AUDIT-A07-PHRASE-ADMISSION/attempt-0001/receipts.
        => Path.Combine(
            ReceiptRepositoryRoot(), "artifacts", "fbe", "AUDIT-A07-PHRASE-ADMISSION", "attempt-0001", "receipts");

    /// <summary>Захваченная попытка этого испытания: оснастка создаёт её до первого теста.</summary>
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку, ставит её в поле класса и создаёт каталог записи.</summary>
        public AttemptScopeFixture()
            : base(
                ReceiptRepositoryRoot(),
                ReceiptDeclaredDirectory(),
                nameof(TranslationPhraseAdmissionAlignmentTests))
        {
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            AttemptScope = this;

            // Каталог записи создаётся сразу, как это делало прежнее ленивое поле.
            Evidence.EnsureWriteRootDirectory();
        }
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник улик захваченной попытки этого класса.</summary>
    private static ExperimentFixes.ExperimentEvidenceSink ReceiptSink =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(TranslationPhraseAdmissionAlignmentTests) + " не была создана.")).Evidence;

    // RepositoryRootOrNull поднимается от каталога сборки до каталога с продуктом и уликами.
    private static string? RepositoryRootOrNull()
    {
        // Старт — каталог, из которого загружена сборка проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            // Корень опознаётся по двум каталогам сразу: продукт перевода и каталог улик.
            if (Directory.Exists(Path.Combine(directory.FullName, "27.TranslationDomain")) &&
                Directory.Exists(Path.Combine(directory.FullName, "artifacts")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Запись улик: читаемый JSON с кириллицей как есть, перевод строки только «\n».
    private static readonly JsonSerializerOptions ReceiptJsonOptions = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // WriteReceipt записывает одну улику в приёмник этой попытки.
    private static void WriteReceipt(string fileName, object payload)
    {
        // Улика сериализуется целиком до записи.
        var json = JsonSerializer.Serialize(payload, ReceiptJsonOptions);
        // Страховка от возврата каретки внутри строкового значения.
        var normalized = json.Replace("\r\n", "\n");
        // Приёмник пишет байты как есть: UTF-8 без метки порядка байтов.
        ReceiptSink.WriteTextExactly(fileName, normalized + "\n");
    }

    // ── вспомогательное ────────────────────────────────────────────────────────

    // BuildBatch собирает партию из названных фраз: у каждого урока ровно одно
    // фразовое соответствие и ни одного однословного, поэтому разрешённая область
    // планировщика выполняется.
    private static TranslationLearningBatch BuildBatch(
        string batchId,
        params (string Source, string Target)[] lessons)
        => new(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: lessons
                .Select((lesson, index) => BuildPhraseLesson(
                    $"{batchId}-{index + 1:00}", lesson.Source, lesson.Target))
                .ToList(),
            SourceIds: [SourceId]);

    // BuildPhraseLesson строит один урок с настоящей подписью продуктового
    // разборщика, чтобы адрес урока совпадал с адресом чтения.
    private static TranslationLearningExample BuildPhraseLesson(string id, string source, string target)
    {
        // Подпись считает тот же разборщик, которым пользуется продукт при чтении.
        var signature = new TranslationSemanticTokenizer().Tokenize(source, "en").Signature;
        // Принятый ответ урока: один перевод и ровно одно фразовое соответствие.
        var chosen = new ExecutorOutput(
            target, 0.95, [], [new ExecutorPhraseMapping(source, target)]);
        // Отклонённый ответ урока нужен парной записи предпочтения.
        var rejected = JsonSerializer.Serialize(
            new ExecutorOutput("wrong target-script output", 0.31, null), JsonOptions);

        return new TranslationLearningExample(
            ExampleId: id,
            UnitId: TranslationUnitIds.ExecutorFor(TranslationDirection.EN_RU),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(source, "en", "general"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, JsonOptions),
            RejectedOutputJson: rejected.TrimEnd('}')
                + ",\"wrong\":\"[WRONG: phrase_admission_misaligned - admissible plan was not offered.]\"}",
            RejectedType: "phrase_admission_misaligned",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // TranslateEnRuAsync прогоняет строку через продуктовый путь целиком.
    private static async Task<string> TranslateEnRuAsync(TranslationMachine machine, string prompt)
    {
        // Оркестратор продукта — та же точка входа, которой пользуются принятые проверки.
        var orchestratorResult = await machine.GetOrchestratorAsync();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);

        // Один запрос направления английский→русский в области «general».
        var result = await orchestratorResult.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"a07-phrase-admission-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "en",
                ["target_language"] = "ru",
                ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    // Настройки разбора учебных записей: тот же единственный преобразователь перечислений.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
