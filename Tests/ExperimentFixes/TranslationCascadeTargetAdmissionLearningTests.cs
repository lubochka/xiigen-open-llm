// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «фильтр допустимости цели между уровнями поиска»
// (адрес P64 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт фильтра
// допустимости цели, запись P6401).
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ.
// Переводчик запоминает пары «исходный текст — перевод» и раскладывает их по
// адресам структуры предложения. На запрос он идёт по трём уровням от самого
// точного адреса к самым общим и отдаёт найденных кандидатов исполнителю.
// Исполнитель затем отбрасывает непригодные: пустую цель, цель на чужом языке
// (для направления английский->русский цель обязана быть по-русски) и цель с
// недостаточной похожестью источника.
//
// В ЧЁМ БЫЛА ОШИБКА. Решающая строка механизма —
// 27.TranslationDomain/TranslationTrainedUnits.cs:376 «return false;». Она
// срабатывала, когда исполнитель отбросил ВСЕХ кандидатов того уровня, который
// ответил. А каскад уже вернулся на первом непустом уровне
// (27.TranslationDomain/TranslationUnitContracts.cs:279), поэтому нижние уровни
// не опрашивались вовсе. Обучение при этом пишет каждую пару под ТРИ адреса
// (строки 434, 437, 438), так что верный перевод часто лежал уровнем ниже — и
// оставался недостижимым. Переводчик отвечал «ничего не выучено» про текст,
// перевод которого он выучил.
//
// ИСПРАВЛЕНИЕ. Поиск получил необязательный последний параметр — проверку
// пригодности одного кандидата. Каждый уровень фильтруется ПОСЛЕ своего
// упорядочивания и ограничения числа соседей, и уровень, опустевший после
// фильтра, ПРОПУСКАЕТ запрос на следующий уровень вместо завершения поиска.
// Переданное «ничего» сохраняет прежнее поведение для всех прочих
// потребителей памяти — именно это свойство используется ниже как доказательство
// чувствительности: прежнюю логику не нужно воспроизводить копией, она
// вызывается тем же продуктовым кодом с непереданной проверкой.
//
// ЧЕГО ЭТА ПРАВКА НЕ ДЕЛАЕТ, и это проверяется отдельным плечом:
// фильтр стоит ПОСЛЕ ограничения числа соседей, поэтому шестую пригодную запись,
// стоящую за пятью непригодными в одном адресе, он не спасает. Кандидаты не
// переупорядочиваются и повторно не допускаются.
//
// ОБЛАСТЬ ПРИМЕНЕНИЯ. Дефект требует адреса, под которым лежат ТОЛЬКО
// непригодные цели. Это возникает, когда объявленная уроком смысловая подпись
// (строка 434) расходится с подписью, которую разборщик считает по тому же
// источнику: тогда верный перевод оказывается только на нижнем уровне. Если
// подписи совпадают, верный перевод лежит на том же уровне и прежняя логика его
// уже находила. Поэтому статус переноса по плану — починка уровня компонента,
// без обещания новой способности обычного пути.
//
// ЧЕГО ЭТОТ ФАЙЛ НЕ ДОКАЗЫВАЕТ: качество перевода, обучение на живых данных,
// время и память; он также не заменяет прогон полного набора тестов проекта.

namespace LlmFromScratch.Tests;

using TranslationDomain;

public sealed class TranslationCascadeTargetAdmissionLearningTests
{
    // Источник, у которого разборщик даёт РАЗНЫЕ полный и укороченный адреса:
    // подробную подпись английскому источнику даёт имя в коде вместе со словом
    // «null». Именно расхождение адресов и создаёт разбор по уровням.
    private const string Source = "ValidateToken null ExpiredSession";

    // Непригодные цели: первая вообще не переведена, вторая по-английски.
    private const string UntranslatedTarget = "ValidateToken null ExpiredSession";
    private const string EnglishTarget = "inspect the expired session";

    // Пригодная цель: по-русски, без латиницы, не копия источника.
    private const string RussianTarget = "проверь истёкшую сессию";

    // Обычный источник положительного контроля и его выученный перевод.
    private const string PlainSource = "open file";
    private const string PlainTarget = "открой файл";

    // Проверка пригодности цели для направления английский->русский ровно в том
    // виде, в каком её передаёт исполнитель после правки: цель непустая и
    // пригодна для направления. Здесь она выражена через тот же наблюдаемый
    // признак — наличие кириллицы и отсутствие латиницы.
    private static bool IsUsableRussianTarget(string _source, string target)
        => !string.IsNullOrWhiteSpace(target)
           && target.Any(ch => ch is >= 'Ѐ' and <= 'ӿ')
           && !target.Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    // ── Плечо A: что обязано сохраниться ──────────────────────────────────────

    // Непереданная проверка сохраняет прежнее поведение: самый точный адрес
    // отвечает своими непригодными целями, и поиск на нём заканчивается. Это и
    // есть прежняя логика, вызванная тем же продуктовым кодом.
    [Fact]
    public void ArmA_WithoutPredicateTheDetailedAddressStillAnswersWithUnusableTargets()
    {
        // Память, где верный перевод лежит только на укороченном адресе.
        var store = BuildSplitAddressStore(out var keys);

        // Вызов без новой проверки: ровно так вызывают все прочие потребители.
        var found = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 5, Source);

        // Ответ пришёл, и в нём только непригодные цели.
        Assert.NotEmpty(found);
        Assert.All(found, pair => Assert.False(IsUsableRussianTarget(Source, pair.Target)));
        Assert.DoesNotContain(found, pair => pair.Target == RussianTarget);
    }

    // Проверка, которая допускает всё, не меняет ничего: значит сам фильтр не
    // двигает ни порядок, ни число соседей, ни пороги уровней.
    [Fact]
    public void ArmA_PredicateThatAcceptsEverythingChangesNothing()
    {
        // Одна и та же память для двух вызовов.
        var store = BuildSplitAddressStore(out var keys);

        // Без проверки и с проверкой, допускающей всё.
        var withoutPredicate = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 5, Source);
        var withAcceptAll = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source, candidatePredicate: (_, _) => true);

        // Совпадают и число записей, и сами записи в том же порядке.
        Assert.Equal(withoutPredicate.Count, withAcceptAll.Count);
        for (var i = 0; i < withoutPredicate.Count; i++)
        {
            Assert.Equal(withoutPredicate[i].Source, withAcceptAll[i].Source);
            Assert.Equal(withoutPredicate[i].Target, withAcceptAll[i].Target);
        }
    }

    // Положительный контроль продуктового пути: выученная пригодная пара
    // по-прежнему отвечает через настоящий вход исполнителя.
    [Fact]
    public void ArmA_PositiveControlThroughExecutorStillAnswers()
    {
        // Память с одной обычной выученной парой под всеми тремя адресами.
        var store = new TranslationKnnStore();
        var tokenizer = new TranslationSemanticTokenizer();
        var plain = tokenizer.Tokenize(PlainSource, "en");
        store.AddPair(plain.Signature, PlainSource, PlainTarget);
        store.AddPair(plain.PrefixSignature, PlainSource, PlainTarget);
        store.AddPair(plain.StructureSignature, PlainSource, PlainTarget);

        // Настоящий исполнитель направления английский->русский.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);

        // Запрос совпадает с выученным источником.
        var candidate = executor.Execute(tokenizer.Tokenize(PlainSource, "en"), "general");

        // Ответ — выученный перевод, и он пришёл путём поиска по памяти.
        Assert.Equal(PlainTarget, candidate.Text);
        Assert.Contains("knn", candidate.AdapterId, StringComparison.Ordinal);
    }

    // ── Плечо B: сам множитель ────────────────────────────────────────────────

    // Уровень, опустевший после фильтра, пропускает запрос на следующий уровень,
    // и оттуда приходит верный перевод.
    [Fact]
    public void ArmB_EmptiedDetailedAddressFallsThroughToThePrefixAddress()
    {
        // Память, где верный перевод лежит только на укороченном адресе.
        var store = BuildSplitAddressStore(out var keys);

        // Тот же вызов, но с проверкой пригодности цели.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source, candidatePredicate: IsUsableRussianTarget);

        // Вернулась ровно пригодная запись с нижнего уровня.
        var selected = Assert.Single(found);
        Assert.Equal(RussianTarget, selected.Target);
    }

    // Худший край: оба верхних адреса непригодны, отвечает третий уровень.
    [Fact]
    public void ArmB_WhenTwoAddressesAreUnusableTheStructureAddressAnswers()
    {
        // Память, где пригодная запись лежит на частеречном адресе.
        var store = BuildWorstEdgeStore(out var keys);

        // Поиск с проверкой пригодности обязан дойти до третьего уровня.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source, candidatePredicate: IsUsableRussianTarget);

        // Пригодная запись найдена.
        var selected = Assert.Single(found);
        Assert.Equal(RussianTarget, selected.Target);

        // Без проверки тот же вызов отвечает непригодными целями верхнего уровня.
        var withoutPredicate = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 5, Source);
        Assert.NotEmpty(withoutPredicate);
        Assert.All(withoutPredicate, pair => Assert.False(IsUsableRussianTarget(Source, pair.Target)));
    }

    // Честный отказ сохраняется: если пригодных целей нет ни на одном адресе,
    // поиск возвращает пустой ответ, а не чужой перевод.
    [Fact]
    public void ArmB_AllAddressesUnusableStillRefuses()
    {
        // Память, где все три адреса содержат только непригодные цели.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        foreach (var address in new[] { keys.Full, keys.Prefix, keys.Structure })
        {
            store.AddPair(address, Source, UntranslatedTarget);
            store.AddPair(address, Source, EnglishTarget);
        }

        // Поиск с проверкой пригодности.
        var found = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source, candidatePredicate: IsUsableRussianTarget);

        // Отказ: ни одной записи.
        Assert.Empty(found);
    }

    // Продуктовый путь целиком: через настоящий вход исполнителя проблемная
    // строка теперь получает выученный русский перевод.
    [Fact]
    public void ArmB_ProblemRowThroughExecutorNowReturnsTheLearnedRussianTranslation()
    {
        // Память, где верный перевод лежит только на укороченном адресе.
        var store = BuildSplitAddressStore(out _);

        // Настоящий исполнитель направления английский->русский.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);

        // Запрос — тот самый источник, перевод которого выучен.
        var candidate = executor.Execute(new TranslationSemanticTokenizer().Tokenize(Source, "en"), "general");

        // Пришёл выученный русский перевод, а не отказ и не чужой язык.
        Assert.Equal(RussianTarget, candidate.Text);
        Assert.Contains("knn", candidate.AdapterId, StringComparison.Ordinal);
    }

    // ── Плечо C: чувствительность к подмене логики ────────────────────────────

    // Те же утверждения плеча B на той же памяти падают, если проверку не
    // передать: это и есть прежняя логика, вызванная продуктовым кодом. Копия
    // прежней реализации здесь не нужна.
    [Fact]
    public void ArmC_SameAssertionsFailWithoutThePredicateProvingMutationSensitivity()
    {
        // Та же память, что в плече B.
        var store = BuildSplitAddressStore(out var keys);

        // Прежнее поведение: проверка не передана.
        var previous = store.FindNearestCascade(keys.Full, keys.Prefix, keys.Structure, 5, Source);

        // Верного перевода среди ответа нет: утверждение плеча B на нём падает.
        Assert.NotEmpty(previous);
        Assert.DoesNotContain(previous, pair => pair.Target == RussianTarget);

        // А с проверкой он есть. Один и тот же вызов, одна и та же память.
        var repaired = store.FindNearestCascade(
            keys.Full, keys.Prefix, keys.Structure, 5, Source, candidatePredicate: IsUsableRussianTarget);
        Assert.Contains(repaired, pair => pair.Target == RussianTarget);
    }

    // ── Плечо D: граница первого переноса, закрытая назначенным остатком ──────
    //
    // ИСТОРИЧЕСКАЯ ЗАПИСЬ (первый перенос, сохраняется как пройденная граница).
    // Первый перенос сознательно поставил фильтр пригодности ПОСЛЕ ограничения
    // числа соседей и честно закрепил два следствия этого порядка: «шестая
    // пригодная запись за пятью непригодными не спасается» и «фильтр никогда не
    // поднимает кандидата на место первого». Оба следствия были настоящими и
    // были названы прямо, а не скрыты.
    //
    // ТЕКУЩИЙ ДОГОВОР. Назначенный остаток перенёс полный допуск ДО ограничения
    // числа соседей и ДО решения об остановке уровня. Поэтому ожидания двух
    // проверок ниже перевёрнуты: теперь первые k берутся уже из пригодных
    // записей. Это изменение ДОГОВОРА по назначенному остатку, а не подгонка
    // ожидания под дефект продукта — прежний результат был именно дефектом, и
    // его прежняя формулировка сохранена абзацем выше.

    // Шестая пригодная запись, стоящая за пятью непригодными в одном адресе,
    // теперь достижима: пригодность спрашивают до ограничения числа соседей.
    [Fact]
    public void ArmD_SixthUsableEntryBehindFiveUnusableOnesIsNowReached()
    {
        // Один адрес: пять непригодных целей, затем пригодная.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        for (var i = 1; i <= 5; i++)
            store.AddPair(keys.Full, Source, $"unusable target {i}");
        store.AddPair(keys.Full, Source, RussianTarget);

        // Более общие адреса пусты: отвечает сам подробный адрес или никто.
        var found = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 5, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Ответ пришёл, и это именно пригодная шестая запись.
        Assert.Equal(RussianTarget, Assert.Single(found).Target);
    }

    // Допуск стоит ДО ограничения числа соседей: при ограничении в одного соседа
    // единственное место достаётся пригодной записи, а не первой по порядку.
    [Fact]
    public void ArmD_AdmissionRunsBeforeTheNeighbourCapSoOneSlotGoesToTheUsablePair()
    {
        // Один адрес: сначала непригодная цель, затем пригодная.
        var store = new TranslationKnnStore();
        var keys = KeysOf(Source);
        store.AddPair(keys.Full, Source, EnglishTarget);
        store.AddPair(keys.Full, Source, RussianTarget);

        // Ограничение в одного соседа применяется уже к пригодным записям.
        var found = store.FindNearestCascade(
            keys.Full, "ABSENT|PREFIX", "ABSENT|STRUCTURE", 1, Source,
            candidatePredicate: IsUsableRussianTarget);

        // Место занял пригодный кандидат: порядок проверки исправлен.
        Assert.Equal(RussianTarget, Assert.Single(found).Target);
    }

    // ── Построение памяти ─────────────────────────────────────────────────────

    // Три адреса одного текста, посчитанные настоящим разборщиком продукта.
    private static (string Full, string Prefix, string Structure) KeysOf(string text)
    {
        // Источник английский, как его передаёт продукт.
        var tokens = new TranslationSemanticTokenizer().Tokenize(text, "en");

        // Полный, укороченный и частеречный адреса.
        return (tokens.Signature, tokens.PrefixSignature, tokens.StructureSignature);
    }

    // Память, повторяющая проблемную строку опыта: на полном адресе лежат только
    // непригодные цели, а верный перевод того же источника — на укороченном.
    // Так бывает, когда объявленная уроком смысловая подпись расходится с
    // подписью разборщика того же источника.
    private static TranslationKnnStore BuildSplitAddressStore(
        out (string Full, string Prefix, string Structure) keys)
    {
        // Адреса считает настоящий разборщик.
        keys = KeysOf(Source);

        // Полный и укороченный адреса обязаны различаться, иначе разбора по
        // уровням не получится и проверка потеряет смысл.
        Assert.NotEqual(keys.Full, keys.Prefix);

        // Настоящее продуктовое хранилище.
        var store = new TranslationKnnStore();

        // Полный адрес: только непригодные цели, в порядке добавления.
        store.AddPair(keys.Full, Source, UntranslatedTarget);
        store.AddPair(keys.Full, Source, EnglishTarget);

        // Укороченный адрес: верный перевод того же источника.
        store.AddPair(keys.Prefix, Source, RussianTarget);

        // Готовая память проблемной строки.
        return store;
    }

    // Память худшего края: непригодные цели на полном и укороченном адресах,
    // верный перевод — только на частеречном.
    private static TranslationKnnStore BuildWorstEdgeStore(
        out (string Full, string Prefix, string Structure) keys)
    {
        // Адреса считает настоящий разборщик.
        keys = KeysOf(Source);

        // Все три адреса обязаны различаться для этого плеча.
        Assert.NotEqual(keys.Full, keys.Prefix);
        Assert.NotEqual(keys.Prefix, keys.Structure);

        // Настоящее продуктовое хранилище.
        var store = new TranslationKnnStore();

        // Два верхних адреса непригодны.
        store.AddPair(keys.Full, Source, UntranslatedTarget);
        store.AddPair(keys.Prefix, Source, EnglishTarget);

        // Пригодная запись лежит на третьем уровне.
        store.AddPair(keys.Structure, Source, RussianTarget);

        // Готовая память худшего края.
        return store;
    }
}
