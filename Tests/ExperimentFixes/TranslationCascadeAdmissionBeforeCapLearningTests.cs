// Порядок допуска в каскаде памяти переводов: полный допуск стоит ДО ограничения числа
// соседей и ДО решения об остановке уровня.
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ. Память переводов ищет готовый ответ по трём адресам
// подряд: сначала по самому подробному, потом по укороченному, потом по частеречному. На
// каждом адресе в ячейке может лежать несколько выученных пар. Раньше уровень сначала
// урезал число кандидатов до разрешённого предела, и только потом у вызывающей стороны
// спрашивали, годится ли кандидат. Из-за такого порядка законный ответ, стоявший в ячейке
// за непригодными, выбрасывался урезанием ДО того, как кто-то спросил, подходит ли он:
// вызывающая сторона получала «ничего не выучено» там, где годный ответ в памяти был.
//
// Теперь полный допуск — закон источника, порог похожести источника и проверка пригодности
// вызывающей стороны — стоит до урезания, а решение «остановиться на этом уровне»
// принимается по результату уже полного допуска.
//
// ГРАНИЦА УТВЕРЖДЕНИЙ. Это правка ПОРЯДКА проверки пригодности, а не новое обучение и не
// рост качества перевода. Ни одна проверка здесь не обучает модель и не измеряет качество
// перевода: все они показывают, какой из уже выученных ответов память способна выдать.
// Значения порогов не менялись ни на одном уровне.
//
// ЧУВСТВИТЕЛЬНОСТЬ. Проверки плеч «Б» и «Г» обязаны падать на опубликованном коде, и
// именно потому, что урезание выбросило годного кандидата до вопроса о пригодности.
// Проверки плеча «А» проходят и до правки, и после: они закрепляют то, что меняться не
// должно, и доказывают, что падение плеч «Б» и «Г» — не поломка сборки и не пустой набор.

namespace LlmFromScratch.Tests;

using TranslationDomain;

public sealed class TranslationCascadeAdmissionBeforeCapLearningTests
{
    // Источник, которому разборщик продукта даёт РАЗНЫЕ подробный и укороченный адреса:
    // расхождение адресов и создаёт разбор по уровням.
    private const string Source = "ValidateToken null ExpiredSession";

    // Непригодная цель: по-английски, то есть для направления английский->русский негодна.
    private const string EnglishTarget = "inspect the expired session";

    // Пригодная цель: по-русски, без латиницы, не копия источника.
    private const string RussianTarget = "проверь истёкшую сессию";

    // Другая пригодная цель, лежащая на укороченном адресе. Нужна, чтобы отличить
    // «остановился на подробном адресе» от «провалился на укороченный».
    private const string OtherRussianTarget = "осмотри завершённый сеанс";

    // Проверка пригодности цели ровно в том виде, в каком её задаёт исполнитель направления
    // английский->русский: цель непустая, в ней есть кириллица и нет латиницы.
    private static bool IsUsableRussianTarget(string _source, string target)
        => !string.IsNullOrWhiteSpace(target)
           && target.Any(ch => ch is >= 'Ѐ' and <= 'ӿ')
           && !target.Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    // ── Плечо А: что обязано остаться прежним (проходит до правки и после) ────────

    // Без переданной проверки урезание и порядок остаются в точности прежними: уровень
    // отвечает своими непригодными целями, и годная запись за ними по-прежнему не видна.
    // Это договор всех ОСТАЛЬНЫХ потребителей памяти, которые проверку не передают.
    [Fact]
    public void WithoutPredicateTheCapAndOrderStayExactlyAsBefore()
    {
        // Ячейка подробного адреса: пять непригодных целей, затем годная шестая.
        var store = BuildFiveUnusableThenOneUsableStore(out var keys);

        // Вызов без проверки пригодности — ровно так вызывают все прочие потребители.
        var found = store.FindNearestCascade(keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 5, Source);

        // Урезание оставило ровно пять записей, и все они непригодны.
        Assert.Equal(5, found.Count);
        Assert.All(found, pair => Assert.False(IsUsableRussianTarget(Source, pair.Target)));

        // Годная шестая запись за пределы урезания не прошла: без проверки её никто не ищет.
        Assert.DoesNotContain(found, pair => pair.Target == RussianTarget);
    }

    // Проверка, допускающая всё, не меняет ничего: значит сам допуск не двигает ни порядок,
    // ни число соседей, ни пороги уровней — он только отбрасывает негодное.
    [Fact]
    public void PredicateThatAcceptsEverythingChangesNothing()
    {
        // Одна и та же память для двух вызовов.
        var store = BuildFiveUnusableThenOneUsableStore(out var keys);

        // Без проверки и с проверкой, допускающей всё.
        var withoutPredicate = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 5, Source);
        var withAcceptAll = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 5, Source, candidatePredicate: (_, _) => true);

        // Совпадают и число записей, и сами записи в том же порядке.
        Assert.Equal(withoutPredicate.Count, withAcceptAll.Count);
        for (var i = 0; i < withoutPredicate.Count; i++)
        {
            Assert.Equal(withoutPredicate[i].Source, withAcceptAll[i].Source);
            Assert.Equal(withoutPredicate[i].Target, withAcceptAll[i].Target);
        }
    }

    // ОПОРА ДЛЯ ЧУВСТВИТЕЛЬНОСТИ: годная запись в памяти ЕСТЬ и достижима обычным чтением,
    // если поднять предел числа соседей так, чтобы урезание до неё не дотянулось. Эта
    // проверка проходит и на опубликованном коде, и после правки. Поэтому падение проверок
    // ниже на опубликованном коде означает именно «урезание выбросило годного кандидата», а
    // не «кандидата в памяти не было» и не «сборка сломана».
    [Fact]
    public void TheNeighbourCapWasTheCauseProvenByRaisingTheLimit()
    {
        // Та же ячейка: пять непригодных целей, затем годная шестая.
        var store = BuildFiveUnusableThenOneUsableStore(out var keys);

        // Предел шесть: урезание до годной записи не дотягивается ни до правки, ни после.
        var withRoomForSix = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 6, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Годная запись найдена, и она ровно одна.
        Assert.Equal(RussianTarget, Assert.Single(withRoomForSix).Target);
    }

    // ── Плечо Б: что правка делает (падает на опубликованном коде) ────────────────

    // Законная шестая запись, стоящая за пятью непригодными в одной ячейке, теперь
    // достижима: пригодность спрашивают до урезания, поэтому первые k берутся уже из
    // годных записей.
    //
    // ИСТОРИЧЕСКАЯ ГРАНИЦА. Первый перенос сознательно оставил эту запись недостижимой и
    // закрепил это проверкой «шестая запись не спасается»; та проверка переписана вместе с
    // этой правкой, а её прежний смысл сохранён здесь как запись о пройденной границе.
    [Fact]
    public void SixthUsableEntryBehindFiveUnusableOnesIsNowReached()
    {
        // Ячейка подробного адреса: пять непригодных целей, затем годная шестая.
        var store = BuildFiveUnusableThenOneUsableStore(out var keys);

        // Более общие адреса пусты: подхватить запись больше некому, отвечает сам уровень.
        var found = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 5, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Ответ пришёл, и это именно годная шестая запись.
        Assert.Equal(RussianTarget, Assert.Single(found).Target);
    }

    // Предел в одного соседа тоже перестал прятать годную запись: допуск стоит раньше
    // урезания, поэтому единственное место достаётся годному кандидату, а не первому.
    [Fact]
    public void AdmissionBeforeTheCapMakesOneNeighbourSlotEnough()
    {
        // Ячейка: сначала непригодная цель, затем годная.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        store.AddPair(keys.Full, Source, EnglishTarget);
        store.AddPair(keys.Full, Source, RussianTarget);

        // Предел — один сосед.
        var found = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 1, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Единственное место занял годный кандидат.
        Assert.Equal(RussianTarget, Assert.Single(found).Target);
    }

    // ── Плечо В: решение об остановке уровня ─────────────────────────────────────

    // Решение «остановиться на этом уровне» принимается ПОСЛЕ полного допуска. Поэтому
    // подробный адрес, у которого годная запись есть, оставляет ответ себе и не
    // проваливается на укороченный с другим ответом.
    //
    // Эта проверка различающая: до правки подробный уровень опустошался допуском уже после
    // урезания и возвращал ответ УКОРОЧЕННОГО адреса.
    [Fact]
    public void TierStopIsDecidedAfterFullAdmissionSoTheDetailedAddressKeepsItsAnswer()
    {
        // Подробный адрес: пять непригодных целей и годная шестая.
        var store = BuildFiveUnusableThenOneUsableStore(out var keys);

        // Укороченный адрес: ДРУГОЙ годный ответ того же источника.
        store.AddPair(keys.Prefix, Source, OtherRussianTarget);

        // Оба адреса годный ответ имеют, различается только уровень.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Ответил подробный адрес — тот, на котором годная запись действительно есть.
        Assert.Equal(RussianTarget, Assert.Single(found).Target);

        // И это не ответ укороченного адреса: провала на уровень ниже не произошло.
        Assert.DoesNotContain(found, pair => pair.Target == OtherRussianTarget);
    }

    // Уровень, на котором годных записей НЕТ ВОВСЕ, по-прежнему проваливается на следующий,
    // а не заканчивает поиск. Это сохранённое поведение первого переноса.
    [Fact]
    public void TierWithoutAnyAdmissibleEntryStillFallsThroughToTheNextAddress()
    {
        // Подробный адрес: только непригодная цель. Укороченный: годный ответ.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        store.AddPair(keys.Full, Source, EnglishTarget);
        store.AddPair(keys.Prefix, Source, OtherRussianTarget);

        // Поиск по всем трём адресам.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Ответил укороченный адрес.
        Assert.Equal(OtherRussianTarget, Assert.Single(found).Target);
    }

    // Когда годных записей нет ни на одном адресе, память по-прежнему ОТКАЗЫВАЕТ, а не
    // выдаёт негодный ответ. Сохранённый отрицательный контроль.
    [Fact]
    public void AllAddressesInadmissibleStillRefuses()
    {
        // Все три адреса несут только непригодные по направлению цели.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        store.AddPair(keys.Full, Source, EnglishTarget);
        store.AddPair(keys.Prefix, Source, Source);
        store.AddPair(keys.Structure, Source, EnglishTarget);

        // Поиск по всем трём адресам.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Отказ, а не чужой ответ.
        Assert.Empty(found);
    }

    // ── Плечо Г: нижний порог источника у исполнителя согласован с тем же порядком ─

    // Нижний порог похожести источника у исполнительной единицы прежде оставался ПОСЛЕ
    // каскада, и от этого та же болезнь возвращалась на другом условии: уровень приходил
    // непустым, порог отвергал на нём все записи, а спуститься на более общий уровень было
    // уже нельзя. Теперь порог спрашивают вместе с остальным допуском — до урезания.
    //
    // Падает на опубликованном коде: там исполнитель получал пять записей с долей общих
    // слов 0,40, отвергал их своим порогом 0,60 и сообщал «ничего не выучено», хотя запись
    // с долей 0,80 лежала в той же ячейке.
    [Fact]
    public void ExecutorSourceFloorIsAskedBeforeTheCapSoTheLawfulCandidateIsReached()
    {
        // Память: пять годных по языку целей с далёким источником и одна с близким.
        var store = BuildDistantThenNearSourceStore();

        // Исполнитель обычного направления английский->русский.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);

        // Запрос без известных смысловых понятий: работает ветка с порогом 0,60.
        var answer = executor.Execute(QueryWithoutKnownConcepts(), "general");

        // Ответ пришёл, и это перевод той записи, чей источник прошёл порог направления.
        Assert.Equal(NearSourceTarget, answer.Text);
        Assert.Equal("executor-en_ru-ordinary-source-knn", answer.AdapterId);
    }

    // Значение порога не менялось: когда в памяти есть ТОЛЬКО далёкая запись, исполнитель
    // по-прежнему отказывается её брать. Проходит и до правки, и после.
    [Fact]
    public void ExecutorSourceFloorValueUnchangedSoADistantOnlyCandidateIsStillRefused()
    {
        // Память: одна годная по языку цель с далёким источником, доля общих слов 0,40.
        var store = new TranslationKnnStore();
        store.AddPair(QueryFullKey, "alpha beta omega", "далёкий выученный ответ");

        // Тот же исполнитель и тот же запрос.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);
        var answer = executor.Execute(QueryWithoutKnownConcepts(), "general");

        // Порог 0,60 отверг долю 0,40: ответа из памяти нет, работает запасной путь.
        Assert.NotEqual("далёкий выученный ответ", answer.Text);
        Assert.Equal("executor-en_ru-fallback", answer.AdapterId);
    }

    // Законная смысловая ветвь сохраняет свой пол ноль: при известном смысловом понятии
    // порог не спрашивается вовсе, и далёкая запись остаётся доступной. Проходит и до
    // правки, и после — доказывает, что ноль не был подменён порогом направления.
    [Fact]
    public void LawfulZeroSourceFloorOfTheSemanticBranchIsUnchanged()
    {
        // Память: одна годная по языку цель с далёким источником.
        var store = new TranslationKnnStore();
        store.AddPair(QueryFullKey, "alpha beta omega", "далёкий выученный ответ");

        // Тот же исполнитель, но в разборе есть известное смысловое понятие.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);
        var answer = executor.Execute(QueryWithOneKnownConcept(), "general");

        // Пол ноль сохранён: далёкая запись отвечает через смысловую ветвь.
        Assert.Equal("далёкий выученный ответ", answer.Text);
        Assert.Equal("executor-en_ru-knn", answer.AdapterId);
    }

    // ── Построение памяти и запросов ─────────────────────────────────────────────

    // Три адреса одного текста, посчитанные настоящим разборщиком продукта.
    private static (string Full, string Prefix, string Structure) KeysOf(string text)
    {
        // Источник английский, как его передаёт продукт.
        var tokens = new TranslationSemanticTokenizer().Tokenize(text, "en");

        // Подробный, укороченный и частеречный адреса.
        return (tokens.Signature, tokens.PrefixSignature, tokens.StructureSignature);
    }

    // Ячейка подробного адреса: пять непригодных целей, затем годная шестая. Ровно та
    // расстановка, на которой урезание выбрасывало законный ответ до вопроса о пригодности.
    private static TranslationKnnStore BuildFiveUnusableThenOneUsableStore(
        out (string Full, string Prefix, string Structure) keys)
    {
        // Адреса считает настоящий разборщик продукта.
        keys = KeysOf(Source);

        // Подробный и укороченный адреса обязаны различаться, иначе разбора по уровням не
        // получится и проверка потеряет смысл.
        Assert.NotEqual(keys.Full, keys.Prefix);

        // Настоящее продуктовое хранилище.
        var store = new TranslationKnnStore();

        // Пять непригодных по направлению целей — все по-английски, в порядке добавления.
        for (var i = 1; i <= 5; i++)
            store.AddPair(keys.Full, Source, $"{EnglishTarget} {i}");

        // Годная шестая запись: она и была недостижима.
        store.AddPair(keys.Full, Source, RussianTarget);

        // Готовая память.
        return store;
    }

    // Запрос исполнителя: пять значимых слов. Доля общих слов с «alpha beta omega» — 0,40,
    // с «alpha beta gamma delta» — 0,80.
    private const string QuerySource = "alpha beta gamma delta epsilon";

    // Подробный адрес запроса в том виде, в каком его ищет исполнитель: канонический ключ.
    private static readonly string QueryFullKey = TranslationSemanticKey.Canonicalize("SVO|UNKNOWN");

    // Перевод записи с близким источником — законный ответ этой ячейки.
    private const string NearSourceTarget = "близкий выученный ответ";

    // Память исполнителя: пять записей с далёким источником и годной по языку целью, затем
    // запись с близким источником. Все шесть лежат в ОДНОЙ ячейке подробного адреса.
    //
    // Доля общих значимых слов у первых пяти — 0,40: это выше порога допуска самого каскада
    // (0,35), поэтому каскад их пропускал, и ниже порога обычного направления (0,60),
    // поэтому исполнитель отвергал их все. Шестая запись с долей 0,80 проходит оба порога.
    private static TranslationKnnStore BuildDistantThenNearSourceStore()
    {
        // Настоящее продуктовое хранилище.
        var store = new TranslationKnnStore();

        // Пять далёких источников: у каждого ровно два общих значимых слова с запросом.
        var distantSources = new[]
        {
            "alpha beta omega", "alpha beta sigma", "alpha beta kappa",
            "alpha beta lambda", "alpha beta zeta"
        };

        // Цели по-русски, то есть по языку пригодны: отвергать их может только порог.
        for (var i = 0; i < distantSources.Length; i++)
            store.AddPair(QueryFullKey, distantSources[i], $"далёкий выученный ответ {i + 1}");

        // Шестая запись: источник близкий, доля общих слов 0,80.
        store.AddPair(QueryFullKey, "alpha beta gamma delta", NearSourceTarget);

        // Готовая память.
        return store;
    }

    // Разбор БЕЗ известных смысловых понятий: именно такой и попадает под порог обычного
    // направления английский->русский.
    private static SemanticTokenSequence QueryWithoutKnownConcepts()
        => BuildQuerySequence(SemanticConcept.UNKNOWN);

    // Тот же разбор, но с одним известным смысловым понятием: выбирается законная
    // смысловая ветвь с полом ноль.
    private static SemanticTokenSequence QueryWithOneKnownConcept()
        => BuildQuerySequence(SemanticConcept.DATA_ENTITY);

    // Разбор запроса с заданным понятием первого слова. Укороченный и частеречный адреса
    // намеренно уникальны: ответ обязан прийти или не прийти по ПОДРОБНОМУ адресу, а не
    // через более широкую ступень каскада.
    private static SemanticTokenSequence BuildQuerySequence(SemanticConcept firstWordConcept)
    {
        // Слова запроса становятся токенами без особой роли.
        var words = QuerySource.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokens = words
            .Select((word, index) => new SemanticToken(
                Word: word,
                Lemma: word,
                Pos: TranslationPos.NOUN,
                Role: "DIRECT_OBJ",
                // Понятие назначается только первому слову: так ветвь выбирается ровно одним
                // различием между двумя запросами.
                Concept: index == 0 ? firstWordConcept : SemanticConcept.UNKNOWN))
            .ToArray();

        // Готовый разбор запроса.
        return new SemanticTokenSequence(
            Pattern: SentencePattern.SVO,
            Tokens: tokens,
            PosBigrams: [],
            Signature: "SVO|UNKNOWN",
            PrefixSignature: "SVO|PREFIX-NOT-STORED",
            StructureSignature: "SVO|STRUCTURE-NOT-STORED",
            RuleTags: []);
    }
}
