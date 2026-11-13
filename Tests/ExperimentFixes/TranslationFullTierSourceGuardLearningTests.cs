// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «страж источника полной подписи» (назначение
// «порог источника самого точного уровня поиска» плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт стража источника
// полной подписи, запись P6201).
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ.
// Переводчик запоминает пары «исходный текст — перевод». Записи раскладываются
// по адресам: адрес — это строка структуры предложения («подпись»). На новый
// запрос переводчик идёт по трём уровням от самого точного адреса к самым
// общим: полная подпись, затем укороченная подпись, затем подпись из одних
// частей речи. На каждом уровне есть защита: слова запроса должны достаточно
// совпадать со словами запомненной записи, иначе запись не допускается.
//
// В ЧЁМ БЫЛА ОШИБКА. Защита по словам стояла НЕ на всех уровнях. На самом
// точном уровне порог совпадения слов был равен нулю, если подпись выглядела
// подробной. Но подробная подпись говорит лишь о том, что два предложения
// одинаково УСТРОЕНЫ, а не о том, что они про одно и то же. Поэтому на самом
// точном уровне переводчик отдавал первую попавшуюся запись из этого адреса
// независимо от её слов. Последствия ровно два, и оба — выдача неверного
// ответа как уверенно верного:
//   1. запрос про один предмет получал перевод, выученный для ДРУГОГО
//      предмета («green vehicle» получал «синий виджет»);
//   2. запрос, про который вообще ничего не выучено, получал тот же чужой
//      перевод вместо честного отказа («purple gadget» получал «синий виджет»).
//
// ИСПРАВЛЕНИЕ. Порог самого точного уровня теперь такой же, как у широких
// уровней: слова запроса и записи должны совпадать не меньше чем на 0,35.
// Самый точный уровень больше не может быть мягче, чем уровни под ним.
// Изменён ровно один множитель. Порог укороченного уровня (0,25), порог
// уровня частей речи (0,35), способ подсчёта совпадения слов, число
// возвращаемых соседей, порядок уровней и необязательный признак знака
// английского высказывания не менялись — и это проверяется плечом A.
//
// ПОЧЕМУ ЗАКОН ИСТОЧНИКА ЗДЕСЬ НИЧЕГО НЕ РЕШАЕТ (важно для чистоты опыта).
// Перед порогом совпадения слов у записи есть ещё одна проверка — закон
// источника, который отсекает заведомо противоположные по смыслу записи. Он
// распознаёт ровно одну узкую форму предложения: артикль, затем предмет из
// закрытого списка, затем «is» или «was», затем признак из закрытого списка.
// Ни один текст этого файла такой форме не отвечает, поэтому закон на всех
// строках возвращает «неизвестно», а «неизвестно» допускается. Значит ни один
// результат здесь не объясняется законом источника: решает только порог.
//
// ПЛЕЧИ ИЗМЕРЕНИЯ.
//   плечо A (база)      — то, что обязано держаться и до правки, и после неё:
//                         точное совпадение источника по-прежнему отвечает,
//                         порог укороченного уровня по-прежнему 0,25, порог
//                         уровня частей речи по-прежнему 0,35, число соседей
//                         и порядок записей сохранены;
//   плечо B (кандидат)  — сам множитель: три строки панели дают 3 из 3 верных
//                         исходов и ноль чужих переводов, а самый точный
//                         уровень становится строго строже укороченного;
//   плечо C (чувствительность) — дословная копия ПРЕЖНЕГО выбора порога на той
//                         же панели даёт 1 из 3 и два чужих перевода. Это
//                         доказывает, что утверждения плеча B падают на старом
//                         коде, а не подогнаны под новый. Копия дополнительно
//                         сверяется с настоящим хранилищем на тех уровнях,
//                         которые НЕ менялись: иначе копии нельзя доверять;
//   плечо D (продуктовое обучение) — те же проверки на обучении, как в
//                         продукте: пара кладётся под ТРИ ключа. Здесь
//                         измерено, что настоящий пересказ НЕ теряется:
//                         самый точный уровень его отклоняет, но каскад не
//                         останавливается и укороченный уровень отдаёт верный
//                         перевод. Здесь же закреплена оставшаяся дыра,
//                         которую этот перенос НЕ закрывает;
//   плечо E (следствие) — вызов без текста запроса теперь не получает ответа с
//                         самого точного уровня. Ни один продуктовый вызывающий
//                         так не вызывает: все передают текст запроса;
//   плечо F (выигрыш)   — запрос, в котором после отбрасывания служебных слов
//                         не осталось ни одного значимого слова, раньше
//                         получал уверенный чужой перевод; теперь получает
//                         отказ.
//
// ИСПРАВЛЕНИЕ ОШИБКИ ДОКАЗАТЕЛЬСТВА (важно для чтения этого файла).
// В первой редакции здесь стояло плечо с утверждением «настоящий пересказ
// теперь получает отказ». Это утверждение ОШИБОЧНО, и оно снято. Оно
// появилось потому, что панель учила пару под ОДНИМ ключом, а продукт кладёт
// каждую выученную пару под ТРИ ключа:
// 27.TranslationDomain/TranslationTrainedUnits.cs строка 434 — полная
// подпись, строка 437 — укороченная, строка 438 — частеречная. Поэтому отказ
// самого точного уровня НЕ останавливает поиск: в коде стоит
// «if (full.Count > 0) return full;», и пустой результат пропускает запрос на
// укороченный уровень, где лежит та же пара и где порог 0,25. Плечи A, B и C
// намеренно оставлены на ОДНОМ ключе: они измеряют отдельно взятый самый
// точный уровень (более общие адреса в них заведомо отсутствуют), а плечо D
// измеряет продукт целиком.
//
// ЧЕГО ЭТОТ ФАЙЛ НЕ ДОКАЗЫВАЕТ. Он не доказывает качество перевода, не
// доказывает обучение на живых данных, не измеряет время и память и не
// заменяет прогон полного набора тестов проекта.

namespace LlmFromScratch.Tests;

using System.Text.RegularExpressions;
using TranslationDomain;

public sealed class TranslationFullTierSourceGuardLearningTests
{
    // Подробный адрес: четыре составляющие, и последняя не является названием
    // части речи. Именно такая подпись считалась «подробной» и именно она
    // раньше оставалась без защиты по словам.
    private const string DetailedKey = "TYPE|NOUN|VERB|RULE_SYNTAX";

    // Адреса, которых в хранилище заведомо нет: так пустой ответ означает
    // именно отказ защиты, а не находку другого уровня.
    private const string AbsentFullKey = "ABSENT|FULL|ADDRESS";
    private const string AbsentPrefixKey = "ABSENT|PREFIX|ADDRESS";
    private const string AbsentStructureKey = "ABSENT|STRUCTURE|ADDRESS";

    // Два пороговых значения продукта, записанные здесь только для копии
    // прежней логики в плече C.
    private const double BroadFloor = 0.35;
    private const double PrefixFloor = 0.25;

    // Панель из двух выученных записей под одним подробным адресом.
    private const string BlueSource = "blue widget";
    private const string BlueTarget = "синий виджет";
    private const string GreenSource = "green vehicle";
    private const string GreenTarget = "зелёное транспортное средство";

    // Запрос, про который ничего не выучено: правильный исход — отказ.
    private const string UnknownSource = "purple gadget";

    // Запрос с совпадением слов ровно 0,25: четыре значимых слова запроса,
    // одно из них совпадает с записью «blue widget».
    private const string QuarterOverlapSource = "purple gadget near blue";

    // ── Панель продуктового обучения (плечи D и F) ────────────────────────────
    //
    // Две записи, у которых настоящий разборщик даёт ОДИНАКОВЫЕ три подписи,
    // поэтому они живут в одних и тех же адресах памяти и могут подменять друг
    // друга. Подробную полную подпись у английского источника даёт единственный
    // доступный путь: имя в коде плюс слово «null» (обычные английские слова
    // понятий не дают, а правила-пометки строятся только для русского
    // источника). У русского источника подробные подписи возникают постоянно,
    // поэтому там дефект был шире — это область применения, а не измерение.
    private const string FirstSource = "ValidateToken null ExpiredSession";
    private const string FirstTarget = "ValidateToken возвращает null для истёкшей сессии";
    private const string SecondSource = "ParseRequest null MissingHeader";
    private const string SecondTarget = "ParseRequest возвращает null при отсутствующем заголовке";

    // Пересказ первой записи: другие имена, та же форма, совпадает одно слово
    // из трёх, то есть 0,3333 — ниже порога 0,35 и выше порога 0,25.
    private const string ParaphraseOfFirst = "VerifyToken null StaleSession";

    // Посторонний предмет той же формы: про него не выучено ничего.
    private const string UnrelatedSameShape = "RenderView null BrokenTemplate";

    // Запрос, в котором после отбрасывания служебных слов не остаётся ни одного
    // значимого слова длиной от двух знаков.
    private const string NoInformativeWordsQuery = "the is not";

    // ── Плечо A: то, что обязано держаться до и после правки ──────────────────

    // Точное совпадение источника по-прежнему отвечает выученным переводом.
    // Это защита от обратной ошибки: порог не должен ломать прямое попадание.
    [Theory]
    [InlineData(BlueSource, BlueTarget)]
    [InlineData(GreenSource, GreenTarget)]
    public void ArmA_ExactLearnedSourceStillAnswers(string query, string expectedTarget)
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Более общие адреса пусты: отвечать обязан самый точный уровень.
        var found = store.FindNearestCascade(DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3, query);

        // Ответ есть, и это перевод именно запрошенной записи.
        var selected = Assert.Single(found);
        Assert.Equal(query, selected.Source);
        Assert.Equal(expectedTarget, selected.Target);
    }

    // Порог укороченного уровня не менялся: совпадение ровно 0,25 допускается.
    // Один и тот же адрес передан и как точный, и как укороченный, поэтому
    // проверка показывает сразу две вещи: точный уровень запись отверг, а
    // укороченный — допустил.
    [Fact]
    public void ArmA_PrefixFloorStillAdmitsQuarterOverlap()
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Адрес один и тот же на двух уровнях: разница только в пороге.
        var found = store.FindNearestCascade(DetailedKey, DetailedKey, AbsentStructureKey, 3, QuarterOverlapSource);

        // Ответ пришёл с укороченного уровня: его порог 0,25 не изменился.
        var selected = Assert.Single(found);
        Assert.Equal(BlueSource, selected.Source);
        Assert.Equal(BlueTarget, selected.Target);
    }

    // Порог уровня частей речи не менялся: 0,25 отвергается, 1,00 допускается.
    [Fact]
    public void ArmA_StructureFloorUnchangedAtBroadValue()
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Совпадение 0,25 ниже порога 0,35 — уровень частей речи отказывает.
        var refused = store.FindNearestCascade(
            AbsentFullKey, AbsentPrefixKey, DetailedKey, 3, QuarterOverlapSource);
        Assert.Empty(refused);

        // Совпадение 1,00 выше порога — тот же уровень отвечает.
        var admitted = store.FindNearestCascade(
            AbsentFullKey, AbsentPrefixKey, DetailedKey, 3, BlueSource);
        var selected = Assert.Single(admitted);
        Assert.Equal(BlueTarget, selected.Target);
    }

    // Число возвращаемых соседей и порядок записей сохранены: при трёх
    // подходящих записях и ограничении в две возвращаются первые две в порядке
    // добавления.
    [Fact]
    public void ArmA_NeighbourCountAndInsertionOrderPreserved()
    {
        // Три записи с одним и тем же источником и разными переводами.
        var store = new TranslationKnnStore();
        store.AddPair(DetailedKey, BlueSource, "перевод один");
        store.AddPair(DetailedKey, BlueSource, "перевод два");
        store.AddPair(DetailedKey, BlueSource, "перевод три");

        // Ограничение в двух соседей при трёх подходящих записях.
        var found = store.FindNearestCascade(DetailedKey, AbsentPrefixKey, AbsentStructureKey, 2, BlueSource);

        // Вернулись ровно две записи, и именно первые две по порядку добавления.
        Assert.Equal(2, found.Count);
        Assert.Equal("перевод один", found[0].Target);
        Assert.Equal("перевод два", found[1].Target);
    }

    // ── Плечо B: сам множитель ────────────────────────────────────────────────

    // Главная панель из трёх строк на ОДНОМ ключе: более общие адреса заведомо
    // отсутствуют, поэтому измеряется отдельно взятый самый точный уровень.
    // Это НЕ утверждение о продукте целиком: в продукте пара лежит под тремя
    // ключами, и отказ этого уровня пропускает запрос на укороченный. Поведение
    // продукта целиком измеряется плечом D ниже.
    [Theory]
    [InlineData(BlueSource, BlueTarget)]
    [InlineData(GreenSource, GreenTarget)]
    [InlineData(UnknownSource, null)]
    public void ArmB_DetailedAddressNoLongerReturnsForeignTranslation(string query, string? expectedTarget)
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Более общие адреса пусты: решает только самый точный уровень.
        var found = store.FindNearestCascade(DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3, query);

        // Невыученный предмет обязан получить отказ, а не чужой перевод.
        if (expectedTarget is null)
        {
            Assert.Empty(found);
            return;
        }

        // Выученный предмет обязан получить свой собственный перевод.
        var selected = Assert.Single(found);
        Assert.Equal(query, selected.Source);
        Assert.Equal(expectedTarget, selected.Target);
    }

    // Вся панель целиком на ОДНОМ ключе: три из трёх верных исходов и ноль
    // чужих переводов для отдельно взятого самого точного уровня. Это мера
    // уровня, а не мера продукта: продуктовую меру даёт плечо D.
    [Fact]
    public void ArmB_WholePanelIsThreeOfThreeWithZeroForeignTranslations()
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Ожидаемый исход каждой строки панели.
        var panel = new (string Query, string? Expected)[]
        {
            (BlueSource, BlueTarget),
            (GreenSource, GreenTarget),
            (UnknownSource, null)
        };

        // Счётчик верных исходов панели.
        var correct = 0;

        // Счётчик выдач перевода, выученного для другого источника.
        var foreign = 0;

        // Каждая строка панели проходит через настоящий продуктовый поиск.
        foreach (var (query, expected) in panel)
        {
            var found = store.FindNearestCascade(DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3, query);

            // Строка без выученного ответа верна тогда и только тогда, когда пуста.
            if (expected is null)
            {
                if (found.Count == 0) correct++;
                else foreign++;
                continue;
            }

            // Строка с выученным ответом верна при собственном переводе записи.
            if (found.Count == 1 && found[0].Target == expected) correct++;

            // Любой другой непустой ответ — это перевод чужого источника.
            else if (found.Count > 0) foreign++;
        }

        // Три из трёх верных исходов.
        Assert.Equal(3, correct);

        // Ни одного чужого перевода.
        Assert.Equal(0, foreign);
    }

    // Самый точный уровень теперь строго строже укороченного, а не мягче:
    // одно и то же совпадение 0,25 он отвергает, а укороченный допускает.
    [Fact]
    public void ArmB_DetailedAddressIsNoLongerMorePermissiveThanPrefix()
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Только самый точный уровень: совпадение 0,25 ниже его порога 0,35.
        var detailedOnly = store.FindNearestCascade(
            DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3, QuarterOverlapSource);
        Assert.Empty(detailedOnly);

        // Тот же адрес на укороченном уровне: его порог 0,25 запись допускает.
        var prefixOnly = store.FindNearestCascade(
            AbsentFullKey, DetailedKey, AbsentStructureKey, 3, QuarterOverlapSource);
        Assert.NotEmpty(prefixOnly);
    }

    // ── Плечо C: чувствительность к подмене логики ────────────────────────────

    // Дословная копия ПРЕЖНЕГО выбора порога на той же панели обязана дать
    // 1 из 3 и два чужих перевода. Если бы утверждения плеча B были подогнаны
    // под новый код, эта проверка бы не прошла.
    [Fact]
    public void ArmC_OldThresholdChoiceFailsTheSamePanelProvingMutationSensitivity()
    {
        // Те же две записи, что и в настоящем хранилище.
        var entries = BuildReplicaEntries();

        // Та же панель с теми же ожидаемыми исходами.
        var panel = new (string Query, string? Expected)[]
        {
            (BlueSource, BlueTarget),
            (GreenSource, GreenTarget),
            (UnknownSource, null)
        };

        // Счётчик верных исходов прежней логики.
        var correct = 0;

        // Счётчик выдач чужого перевода прежней логикой.
        var foreign = 0;

        // Каждая строка проходит через копию прежней логики.
        foreach (var (query, expected) in panel)
        {
            var found = OldCascade(entries, DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3, query);

            // Прежняя логика на подробном адресе отдавала первую запись всегда.
            if (expected is null)
            {
                if (found.Count == 0) correct++;
                else foreign++;
                continue;
            }

            // Верным считается только собственный перевод записи.
            if (found.Count > 0 && found[0].Target == expected) correct++;
            else if (found.Count > 0) foreign++;
        }

        // Прежняя логика давала один верный исход из трёх.
        Assert.Equal(1, correct);

        // И два чужих перевода вместо верного ответа и отказа.
        Assert.Equal(2, foreign);
    }

    // Копии прежней логики можно доверять только если она совпадает с настоящим
    // продуктом там, где ничего не менялось. Здесь отвечают укороченный и
    // частеречный уровни, порог которых не трогали, поэтому копия и продукт
    // обязаны дать один и тот же ответ.
    [Theory]
    [InlineData(QuarterOverlapSource)]
    [InlineData(BlueSource)]
    [InlineData(UnknownSource)]
    public void ArmC_ReplicaAgreesWithProductOnTheTiersThatDidNotChange(string query)
    {
        // Настоящее хранилище и его копия получают одни и те же записи.
        var store = BuildTwoEntryStore();
        var entries = BuildReplicaEntries();

        // Самый точный адрес заведомо отсутствует, поэтому изменённый порог
        // не участвует: решают только неизменённые уровни.
        var product = store.FindNearestCascade(AbsentFullKey, DetailedKey, DetailedKey, 3, query);
        var replica = OldCascade(entries, AbsentFullKey, DetailedKey, DetailedKey, 3, query);

        // Одинаковое число найденных записей.
        Assert.Equal(product.Count, replica.Count);

        // И одинаковые записи в одинаковом порядке.
        for (var i = 0; i < product.Count; i++)
        {
            Assert.Equal(product[i].Source, replica[i].Source);
            Assert.Equal(product[i].Target, replica[i].Target);
        }
    }

    // ── Плечо D: продуктовое обучение под три ключа ───────────────────────────

    // Укороченная подпись пересказа и укороченная подпись выученной пары — одна
    // и та же строка. Именно поэтому отказ самого точного уровня не означает
    // отказ продукта: та же пара лежит под этим адресом по строке 437 обучения.
    [Fact]
    public void ArmD_ParaphraseAndLearnedPairShareThePrefixAddress()
    {
        // Подписи считает настоящий разборщик продукта, а не копия.
        var learned = KeysOf(FirstSource);
        var paraphrase = KeysOf(ParaphraseOfFirst);

        // Укороченные адреса совпадают.
        Assert.Equal(learned.Prefix, paraphrase.Prefix);

        // Полные адреса тоже совпадают: форма предложения одна и та же.
        Assert.Equal(learned.Full, paraphrase.Full);
    }

    // Настоящий пересказ НЕ теряется. Самый точный уровень его отклоняет, потому
    // что совпадает одно значимое слово из трёх, то есть 0,3333 ниже порога
    // 0,35. Но каскад не останавливается: пустой результат пропускает запрос на
    // укороченный уровень, где лежит та же пара и где порог 0,25 это совпадение
    // проходит. Прежнее утверждение «пересказ теперь получает отказ» было
    // следствием панели на одном ключе и снято как ошибочное.
    [Fact]
    public void ArmD_GenuineParaphraseIsAnsweredFromThePrefixTierNotRefused()
    {
        // Обучение как в продукте: каждая пара под три ключа.
        var store = BuildProductTrainedStore();

        // Адреса запроса считает настоящий разборщик.
        var keys = KeysOf(ParaphraseOfFirst);

        // Поиск идёт по всем трём уровням с их настоящими порогами.
        var found = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 3, ParaphraseOfFirst);

        // Ответ есть, и это верный перевод именно пересказанной пары.
        Assert.NotEmpty(found);
        Assert.Equal(FirstTarget, found[0].Target);
    }

    // Продуктовый выигрыш: точный запрос второй выученной пары получает СВОЙ
    // перевод, а не перевод первой пары. Прежняя логика отдавала первую запись
    // адреса, потому что на подробной подписи порог был нулевой.
    [Fact]
    public void ArmD_ExactSecondPairGetsItsOwnTranslationWhereOldLogicGaveTheFirst()
    {
        // Обучение как в продукте и та же память в виде списка для копии.
        var store = BuildProductTrainedStore();
        var entries = BuildProductTrainedReplicaEntries();

        // Адреса точного запроса второй пары.
        var keys = KeysOf(SecondSource);

        // Новая логика: отвечает сама вторая пара.
        var found = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 3, SecondSource);
        Assert.NotEmpty(found);
        Assert.Equal(SecondTarget, found[0].Target);

        // Прежняя логика на той же трёхключевой памяти отдавала перевод ПЕРВОЙ
        // пары: это и есть устранённая выдача чужого перевода.
        var old = OldCascade(entries, keys.Full, keys.Prefix, keys.Structure, 3, SecondSource);
        Assert.NotEmpty(old);
        Assert.Equal(FirstTarget, old[0].Target);
    }

    // Оставшаяся дыра, которую этот перенос НЕ закрывает, закреплена здесь как
    // факт, чтобы будущий перенос имел точку отсчёта: посторонний предмет той же
    // формы со совпадением 0,3333 по-прежнему получает чужой перевод — теперь с
    // укороченного уровня, порог которого этот перенос не трогал. Механически
    // это тот же случай, что и пересказ: порог 0,25 их не различает, и пересказ
    // получает верный ответ лишь потому, что нужная пара добавлена первой.
    [Fact]
    public void ArmD_UnrelatedSubjectOfTheSameShapeStillGetsForeignTranslationFromPrefixTier()
    {
        // Обучение как в продукте.
        var store = BuildProductTrainedStore();

        // Адреса постороннего запроса совпадают с адресами выученных пар.
        var keys = KeysOf(UnrelatedSameShape);

        // Ответ приходит и он чужой: это не выигрыш и не регресс, а граница.
        var found = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 3, UnrelatedSameShape);
        Assert.NotEmpty(found);
        Assert.Equal(FirstTarget, found[0].Target);
    }

    // ── Плечо E: следствие для вызова без текста запроса ──────────────────────

    // Без текста запроса сравнивать нечего, поэтому самый точный уровень теперь
    // не отвечает. Все продуктовые вызывающие передают текст запроса, поэтому
    // это следствие ни одного из них не затрагивает.
    [Fact]
    public void ArmE_WithoutQuerySourceTextTheDetailedAddressNoLongerAnswers()
    {
        // Хранилище получает обе записи под одним подробным адресом.
        var store = BuildTwoEntryStore();

        // Вызов без текста запроса: параметр остаётся необязательным.
        var found = store.FindNearestCascade(DetailedKey, AbsentPrefixKey, AbsentStructureKey, 3);

        // Ответа нет: сравнивать слова не с чем.
        Assert.Empty(found);
    }

    // ── Плечо F: запрос без значимых слов ─────────────────────────────────────

    // Запрос, в котором после отбрасывания служебных слов не осталось ни одного
    // значимого слова, раньше получал уверенный чужой перевод с самого точного
    // уровня: нулевой порог отдавал первую запись адреса, ничего не сравнивая.
    // Теперь сравнивать нечего, и все три уровня отказывают. Адреса берутся от
    // выученной пары, потому что измеряется поведение стража, а не отсутствие
    // адреса в памяти.
    [Fact]
    public void ArmF_QueryWithNoInformativeWordsIsRefusedWhereOldLogicAnswered()
    {
        // Обучение как в продукте и та же память в виде списка для копии.
        var store = BuildProductTrainedStore();
        var entries = BuildProductTrainedReplicaEntries();

        // Адреса выученной пары.
        var keys = KeysOf(FirstSource);

        // Новая логика: отказ на всех трёх уровнях.
        var found = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 3, NoInformativeWordsQuery);
        Assert.Empty(found);

        // Прежняя логика отвечала чужим переводом, не сравнив ни одного слова.
        var old = OldCascade(entries, keys.Full, keys.Prefix, keys.Structure, 3, NoInformativeWordsQuery);
        Assert.NotEmpty(old);
        Assert.Equal(FirstTarget, old[0].Target);
    }

    // ── Построение панели ─────────────────────────────────────────────────────

    // Три адреса одного текста, посчитанные НАСТОЯЩИМ разборщиком продукта.
    private static (string Full, string Prefix, string Structure) KeysOf(string text)
    {
        // Источник английский: язык передаётся так же, как его передаёт продукт.
        var tokens = new TranslationSemanticTokenizer().Tokenize(text, "en");

        // Полная, укороченная и частеречная подписи этого текста.
        return (tokens.Signature, tokens.PrefixSignature, tokens.StructureSignature);
    }

    // Обучение как в продукте: каждая пара кладётся под ТРИ ключа — ровно так,
    // как это делают строки 434, 437 и 438 файла TranslationTrainedUnits.cs.
    private static TranslationKnnStore BuildProductTrainedStore()
    {
        // Настоящее продуктовое хранилище без подмен.
        var store = new TranslationKnnStore();

        // Обе записи проходят одинаковое обучение в порядке добавления.
        foreach (var (source, target) in new[] { (FirstSource, FirstTarget), (SecondSource, SecondTarget) })
        {
            var keys = KeysOf(source);

            // Полная подпись: строка 434.
            store.AddPair(keys.Full, source, target);

            // Укороченная подпись: строка 437.
            store.AddPair(keys.Prefix, source, target);

            // Частеречная подпись: строка 438.
            store.AddPair(keys.Structure, source, target);
        }

        // Готовая продуктовая память для плеч D и F.
        return store;
    }

    // Та же трёхключевая память в виде простого списка для копии прежней логики.
    private static IReadOnlyList<(string Signature, string Source, string Target)> BuildProductTrainedReplicaEntries()
    {
        // Порядок добавления сохраняется: он определяет, какая запись первая.
        var entries = new List<(string Signature, string Source, string Target)>();

        // Те же две записи и те же три адреса на каждую.
        foreach (var (source, target) in new[] { (FirstSource, FirstTarget), (SecondSource, SecondTarget) })
        {
            var keys = KeysOf(source);
            entries.Add((keys.Full, source, target));
            entries.Add((keys.Prefix, source, target));
            entries.Add((keys.Structure, source, target));
        }

        // Список для прохода копии прежней логики.
        return entries;
    }

    // Настоящее продуктовое хранилище с двумя записями под одним адресом.
    private static TranslationKnnStore BuildTwoEntryStore()
    {
        // Хранилище продукта без каких-либо подмен.
        var store = new TranslationKnnStore();

        // Первая запись добавляется первой: прежняя логика отдавала именно её.
        store.AddPair(DetailedKey, BlueSource, BlueTarget);

        // Вторая запись живёт под тем же адресом, но про другой предмет.
        store.AddPair(DetailedKey, GreenSource, GreenTarget);

        // Готовая панель для всех плеч.
        return store;
    }

    // Те же записи в виде простого списка для копии прежней логики.
    private static IReadOnlyList<(string Signature, string Source, string Target)> BuildReplicaEntries()
        =>
        [
            (DetailedKey, BlueSource, BlueTarget),
            (DetailedKey, GreenSource, GreenTarget)
        ];

    // ── Копия ПРЕЖНЕЙ логики: только доказательство, не продукт ───────────────
    //
    // Ниже — дословная копия удалённого выбора порога и прежнего стража
    // источника. Это НЕ продуктовый код, НЕ заглушка и НЕ второй путь
    // исполнения: копия существует исключительно для доказательства того, что
    // утверждения плеча B падают на прежней логике. Закон источника в копии не
    // воспроизводится намеренно: на всех текстах этого файла он возвращает
    // «неизвестно» и ничего не решает, что подтверждается плечом C через
    // сверку копии с настоящим продуктом на неизменённых уровнях.

    // Прежний проход по трём уровням с прежним выбором порога.
    private static IReadOnlyList<(string Source, string Target)> OldCascade(
        IReadOnlyList<(string Signature, string Source, string Target)> entries,
        string fullSignature,
        string prefixSignature,
        string structureSignature,
        int k,
        string? querySourceText)
    {
        // Прежний выбор: подробная подпись получала порог ноль, то есть
        // никакой защиты по словам.
        var fullThreshold = OldIsLowInformationSignature(fullSignature)
            ? BroadFloor
            : 0.0;

        // Самый точный уровень.
        var full = OldGuard(entries, fullSignature, k, querySourceText, fullThreshold);
        if (full.Count > 0) return full;

        // Укороченный уровень с неизменным порогом.
        var prefix = OldGuard(entries, prefixSignature, k, querySourceText, PrefixFloor);
        if (prefix.Count > 0) return prefix;

        // Уровень частей речи с неизменным порогом.
        var structure = OldGuard(entries, structureSignature, k, querySourceText, BroadFloor);
        if (structure.Count > 0) return structure;

        // Ни один уровень не ответил.
        return [];
    }

    // Прежний страж источника одного уровня.
    private static IReadOnlyList<(string Source, string Target)> OldGuard(
        IReadOnlyList<(string Signature, string Source, string Target)> entries,
        string signature,
        int k,
        string? querySourceText,
        double minimumSourceSimilarity)
    {
        // Записи этого адреса в порядке добавления.
        var bucket = entries
            .Where(entry => string.Equals(entry.Signature, signature, StringComparison.Ordinal))
            .Select(entry => (entry.Source, entry.Target))
            .ToList();

        // Пустой адрес не отвечает.
        if (bucket.Count == 0) return [];

        // Нулевой порог — ровно та дыра, которую закрыл перенос: отдаётся
        // первая запись адреса без всякой проверки слов.
        if (minimumSourceSimilarity <= 0.0) return bucket.Take(k).ToList();

        // Значимые слова запроса.
        var queryTokens = ReplicaSourceTokens(querySourceText);

        // Без значимых слов сравнивать нечего.
        if (queryTokens.Count == 0) return [];

        // Записи, слова которых совпадают с запросом не ниже порога.
        return bucket
            .Where(pair => ReplicaSourceSimilarity(queryTokens, ReplicaSourceTokens(pair.Source)) >= minimumSourceSimilarity)
            .Take(k)
            .ToList();
    }

    // Дословная копия удалённой проверки «подпись выглядит общей».
    private static bool OldIsLowInformationSignature(string signature)
    {
        // Составляющие подписи без пустых значений.
        var components = (signature ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries);

        // Две составляющие и меньше — подпись считалась общей.
        if (components.Length <= 2)
            return true;

        // Подпись считалась общей и тогда, когда всё после первой составляющей
        // было названиями частей речи.
        return components
            .Skip(1)
            .All(component => Enum.GetNames<TranslationPos>()
                .Any(pos => string.Equals(pos, component, StringComparison.Ordinal)));
    }

    // Выражение значимых слов источника: буквы и цифры, длина не меньше двух,
    // без служебных слов.
    private static readonly Regex ReplicaTokenRegex = new(@"[\p{L}\p{N}_$]+", RegexOptions.Compiled);

    // Служебные слова, которые не считаются значимыми.
    private static readonly HashSet<string> ReplicaLowInformationWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
        "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
        "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those"
    };

    // Копия подсчёта значимых слов.
    private static HashSet<string> ReplicaSourceTokens(string? sourceText)
        => ReplicaTokenRegex
            .Matches(sourceText ?? string.Empty)
            .Select(match => match.Value.Trim().ToLowerInvariant())
            .Where(token => token.Length >= 2)
            .Where(token => !ReplicaLowInformationWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // Копия подсчёта совпадения: доля слов запроса, найденных в записи.
    private static double ReplicaSourceSimilarity(HashSet<string> queryTokens, HashSet<string> candidateTokens)
    {
        // Без слов с любой стороны совпадения нет.
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
            return 0.0;

        // Число общих слов.
        var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();

        // Доля считается от числа слов ЗАПРОСА, как в продукте.
        return overlap / (double)queryTokens.Count;
    }
}
