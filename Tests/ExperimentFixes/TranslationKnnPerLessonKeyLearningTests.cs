// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;
// Общий приёмник доказательств опытов живёт в соседнем пространстве имён этого же каталога.
using LlmFromScratch.Tests.ExperimentFixes;

// КВАЛИФИКАЦИЯ строки очереди переносов 64, идентификатор работы FIX-P60,
// карточка docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 111-post-p55-transfer-plan-2026-11-01.md, раздел «111-P60 — один AddPair на
// каждый различный ключ одного урока».
//
// ЧТО ЭТО ЗА ПРОВЕРКИ И ЧЕГО ОНИ НЕ ДЕЛАЮТ. Продуктовый код здесь НЕ меняется.
// Реестр требует порядок: сначала квалифицировать сохранение обязательного
// слова «пожалуйста» и границу урока на имеющихся данных, и только потом
// рассматривать правку. Поэтому второе плечо (снятие одинаковых адресов)
// собирается здесь же, на уровне хранилища памяти соседей, а не в продукте.
//
// ЧТО СЛОМАНО ПО-ЧЕЛОВЕЧЕСКИ. Запись одного урока зовёт запись в память три
// раза: под каноническим ключом урока, под префиксным и под структурным. Когда
// два из этих трёх адресов совпадают, один и тот же урок занимает в одной
// корзине два места. При выдаче пяти ближайших соседей два места из пяти
// съедаются дублем, и поздний урок становится недостижимым.
//
// ЧТО ЗДЕСЬ ЗАКРЕПЛЕНО КАК НАБЛЮДЁННОЕ, А НЕ КАК ПРЕДПИСАННОЕ. Числа опыта
// (12 записей против 8) измерены ДО того, как первый ключ стал канонизироваться
// коммитом «Единый ключ урока перевода». Поэтому фактическое число различных
// ключей ОДНОГО урока считается здесь заново, настоящие ключи пишутся в
// доказательство, и если сумма этих чисел по панели разойдётся с ожиданием
// карточки, это помечается словом FIXTURE_DRIFT, а не объявляется
// воспроизведённым.
//
// РАЗГРАНИЧЕНИЕ, КОТОРОЕ НЕЛЬЗЯ ТЕРЯТЬ: «различных адресов» и «записей» — две
// разные величины. Ожидание карточки «двенадцать в восемь» — это про число
// ЗАПИСЕЙ: восемь получается как сумма m по четырём урокам, где m — число
// различных адресов ОДНОГО урока. Самих различных адресов по всей панели
// меньше, потому что уроки панели попадают в одни и те же адреса. Называть
// сумму m «числом различных адресов» — ложный ярлык, и он здесь запрещён.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class TranslationKnnPerLessonKeyLearningTests
    : IClassFixture<TranslationKnnPerLessonKeyLearningTests.AttemptScopeFixture>
{
    // Тема урока: как в панели карточки.
    private const string Topic = "general";

    // Язык источника: английский.
    private const string SourceLang = "en";

    // Опознаватель источника уроков этой квалификации.
    private const string SourceId = "fix-p60-qualification-v1";

    // Направление: с английского на русский, как в панели карточки.
    private static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Полярная строка панели: пятый запрос, которого ни один урок не учит целиком.
    private const string PolarSource = "please check yellow file";

    // Строгий эталон пятой строки, назначенный карточкой дословно.
    private const string PolarStrictGold = "пожалуйста, проверь жёлтый файл";

    // Обязательное слово, сохранение которого требует квалифицировать реестр.
    private const string MandatoryWord = "пожалуйста";

    // Четыре урока панели: цвет меняется, построение предложения одно.
    private static readonly (string Source, string Target)[] Panel =
    {
        ("check red file", "проверь красный файл"),
        ("check blue file", "проверь синий файл"),
        ("check green file", "проверь зелёный файл"),
        ("check yellow file", "проверь жёлтый файл")
    };

    // Настройки сериализации для подготовки текстов уроков.
    private static readonly JsonSerializerOptions UnitJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Корень хранилища ищется по файлу правил проекта, вверх от каталога сборки.
    private static string RepositoryRoot()
    {
        // Поиск начинается от каталога, откуда запущена сборка проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        // Подъём вверх до каталога, где лежит файл правил агента.
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        // Без корня хранилища доказательства писать некуда.
        Assert.NotNull(directory);

        // Полный путь корня возвращается вызывающей стороне.
        return directory!.FullName;
    }

    // Каталог доказательств этой работы.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "artifacts", "fbe", "FIX-P60", "attempt-0001");

    // Куда этому прогону разрешено писать: решает общий приёмник доказательств.
    //
    // Почему ленивое поле заменено фикстурой класса. В этом испытании есть настоящие уроки обучения,
    // поэтому поздний захват означал оплату обучения до того, как станет известен отказ. Фикстура
    // класса создаётся раньше конструктора теста, поэтому захват адреса стал первым действием
    // попытки, а завершение попытки ставится ровно один раз.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(RepositoryRoot(), AttemptDirectory(), nameof(TranslationKnnPerLessonKeyLearningTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(TranslationKnnPerLessonKeyLearningTests) + " не была создана.")).Evidence;

    // Доказательства пишутся в пригодном для хранилища виде: без метки порядка
    // байтов, перевод строки одним знаком, без пробелов в конце строк.
    private static void WriteEvidence(string relativePath, string text)
    {
        // Нормализация оставлена здесь: приёмник пишет байты как есть.
        var normalized = string.Join("\n",
            text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));

        // Запись выполняется приёмником, который и выбрал разрешённый каталог.
        Evidence.WriteTextExactly(relativePath, normalized);
    }

    // Три адреса, под которые продуктовая запись урока кладёт один и тот же урок.
    // Первый ключ берётся КАНОНИЗИРОВАННЫМ — ровно так, как это делает живой код
    // после переноса «Единый ключ урока перевода». Брать его сырым, как написано
    // в карточке от 1 ноября, означало бы откатить тот принятый перенос.
    private static string[] ThreeAddressesOf(string source)
    {
        // Разбор источника настоящим токенизатором предметной области.
        var tokens = new TranslationSemanticTokenizer().Tokenize(source, SourceLang);

        // Те же три адреса и в том же порядке, что в продуктовом методе записи.
        return new[]
        {
            TranslationSemanticKey.Canonicalize(tokens.Signature),
            tokens.PrefixSignature,
            tokens.StructureSignature
        };
    }

    [Fact]
    public void ObservedDistinctAddressCountPerLessonIsRecordedNotAssumed()
    {
        // Накопитель человекочитаемых строк доказательства.
        var lines = new List<string>
        {
            "# Фактические адреса одного урока после канонизации ключа",
            "",
            "Строка очереди переносов 64, идентификатор работы FIX-P60.",
            "Первый адрес берётся канонизированным, как в живом коде.",
            ""
        };

        // Сумма m по урокам: это число ЗАПИСЕЙ после снятия одинаковых адресов
        // внутри урока, а НЕ число различных адресов панели.
        var entriesAfterPerLessonDedup = 0;

        // Сами различные адреса панели: по их числу видно, что уроки делят ячейки
        // памяти ближайших соседей, а не стоят каждый в своей.
        var panelDistinctAddresses = new HashSet<string>(StringComparer.Ordinal);

        // Каждый урок панели разбирается отдельно.
        foreach (var (source, target) in Panel)
        {
            // Три адреса этого урока в продуктовом порядке.
            var addresses = ThreeAddressesOf(source);

            // Число различных адресов — это и есть m из карточки.
            var distinct = addresses.Distinct(StringComparer.Ordinal).Count();

            // Карточка ограничивает m снизу единицей и сверху тройкой.
            Assert.InRange(distinct, 1, 3);

            // Сумма пополняется: каждый различный адрес урока даст одну запись.
            entriesAfterPerLessonDedup += distinct;

            // Адреса этого урока складываются в общий набор адресов панели.
            foreach (var address in addresses)
                panelDistinctAddresses.Add(address);

            // Настоящие адреса пишутся в доказательство дословно.
            lines.Add($"## урок «{source}» -> «{target}»");
            lines.Add($"- различных адресов (m): {distinct}");
            lines.Add($"- канонический ключ урока: {addresses[0]}");
            lines.Add($"- префиксный адрес: {addresses[1]}");
            lines.Add($"- структурный адрес: {addresses[2]}");
            lines.Add("");
        }

        // Ожидание карточки: по два различных адреса на урок, то есть восемь
        // ЗАПИСЕЙ после снятия одинаковых адресов внутри урока.
        var cardExpectation = Panel.Length * 2;

        // Итог называет две разные величины ОТДЕЛЬНО. Прежняя запись складывала m
        // по урокам и называла сумму «различными адресами» — это противоречило
        // напечатанным выше адресам, где все уроки совпадают.
        lines.Add($"Итог, величина первая: различных адресов по всей панели — {panelDistinctAddresses.Count}.");
        lines.Add($"Все {Panel.Length} урока панели попадают в одни и те же адреса: "
            + string.Join(", ", panelDistinctAddresses.OrderBy(address => address, StringComparer.Ordinal))
            + ".");
        lines.Add("");

        // Вторая величина: сумма m, то есть число записей после снятия дублей.
        lines.Add($"Итог, величина вторая: сумма m по {Panel.Length} урокам — {entriesAfterPerLessonDedup}. "
            + "Это число ЗАПИСЕЙ после снятия одинаковых адресов внутри урока, "
            + "а не число различных адресов.");
        lines.Add("");

        // Расхождение с ожиданием карточки помечается словом, а не замалчивается.
        lines.Add(entriesAfterPerLessonDedup == cardExpectation
            ? "Ожидание карточки «двенадцать в восемь» задано через m=2 на урок, то есть "
              + $"{cardExpectation} записей, и по записям оно воспроизводится. "
              + "К числу различных адресов это ожидание не относится."
            : "FIXTURE_DRIFT: записей после снятия одинаковых адресов внутри урока "
              + $"{entriesAfterPerLessonDedup}, ожидание карточки {cardExpectation} записей. "
              + "Числа опыта 12 против 8 измерены до канонизации первого ключа и воспроизведёнными не объявляются.");
        lines.Add("");

        // Пояснение механизма: без него смена достижимости выглядит необъяснимой.
        lines.Add("Почему это меняет достижимость позднего урока. Адреса совпадают не только");
        lines.Add("внутри урока, но и между уроками: вся панель лежит в "
            + $"{panelDistinctAddresses.Count} ячейках памяти ближайших соседей.");
        lines.Add("Значит уроки не стоят каждый в своей ячейке, а конкурируют за одни и те же");
        lines.Add("места в выдаче ближайших соседей. Пока один урок занимает в общей ячейке два");
        lines.Add("места вместо одного, мест на все уроки не хватает, и поздний урок в выдачу не");
        lines.Add("попадает. Снятие одинаковых адресов внутри урока освобождает эти места —");
        lines.Add("именно поэтому поздний урок становится достижимым.");
        lines.Add("");

        // Доказательство записывается до конца проверки.
        WriteEvidence("observed-addresses.md", string.Join("\n", lines));
    }

    [Fact]
    public void ProductTrainingWritesThreeEntriesPerLessonEvenWhenAddressesCollide()
    {
        // Хранилище памяти соседей для продуктового плеча.
        var store = new TranslationKnnStore();

        // Продуктовая единица-исполнитель, которую обучаем без изменений кода.
        var unit = new ExecutorUnit(Direction, store);

        // Все четыре урока панели подаются продуктовому методу записи.
        foreach (var (source, target) in Panel)
            unit.TrainOnExample(BuildExample(source, target, "panel"));

        // Запись в память зовётся три раза на урок, а счётчик считает пары, а не
        // корзины, поэтому совпадение адресов даёт лишнюю запись в той же корзине.
        var expectedEntries = Panel.Length * 3;

        // Наблюдённое число записей закрепляется как продуктовое поведение.
        Assert.Equal(expectedEntries, store.Count);
    }

    [Fact]
    public void RemovingIdenticalAddressesSavesExactlyThreeMinusDistinctPerLesson()
    {
        // Продуктовое плечо: три записи на урок.
        var productStore = new TranslationKnnStore();

        // Продуктовая единица обучается без единой правки продукта.
        var productUnit = new ExecutorUnit(Direction, productStore);

        // Второе плечо собирается здесь же, на уровне хранилища: один урок — один
        // раз на каждый РАЗЛИЧНЫЙ адрес. Продукт при этом не меняется.
        var dedupStore = new TranslationKnnStore();

        // Ожидаемая экономия складывается из «три минус m» по каждому уроку.
        var expectedSaving = 0;

        // Оба плеча получают одни и те же уроки в одном порядке.
        foreach (var (source, target) in Panel)
        {
            // Продуктовое плечо обучается продуктовым методом.
            productUnit.TrainOnExample(BuildExample(source, target, "panel"));

            // Три адреса этого урока в продуктовом порядке.
            var addresses = ThreeAddressesOf(source);

            // Различные адреса с сохранением первого порядка — как требует карточка.
            var distinctAddresses = addresses.Distinct(StringComparer.Ordinal).ToList();

            // Второе плечо кладёт урок по одному разу на каждый различный адрес.
            foreach (var address in distinctAddresses)
                dedupStore.AddPair(address, source, target);

            // Экономия этого урока: три вызова против числа различных адресов.
            expectedSaving += addresses.Length - distinctAddresses.Count;
        }

        // Фактическая разница числа записей между плечами.
        var observedSaving = productStore.Count - dedupStore.Count;

        // Экономия обязана совпасть с суммой «три минус m», и ничем иным.
        Assert.Equal(expectedSaving, observedSaving);

        // Доказательство с обоими числами записывается человеческим текстом.
        WriteEvidence("entries-both-arms.md", string.Join("\n", new[]
        {
            "# Число записей памяти соседей: продуктовое плечо против снятия одинаковых адресов",
            "",
            "Строка очереди переносов 64, идентификатор работы FIX-P60.",
            "Продукт НЕ изменён: второе плечо собрано на уровне хранилища внутри проверки.",
            "",
            $"- записей в продуктовом плече: {productStore.Count}",
            $"- записей при снятии одинаковых адресов: {dedupStore.Count}",
            $"- экономия записей: {observedSaving}",
            $"- уроков в панели: {Panel.Length}",
            "",
            "Экономия равна сумме «три минус число различных адресов» по каждому уроку.",
            ""
        }));
    }

    [Fact]
    public void MandatoryPleaseIsNotTaughtByAnyPanelLessonSoTheRegistryGateCannotPass()
    {
        // Обязательное слово ищется во всех выученных ответах панели.
        var targetsTeachingTheWord = Panel
            .Where(lesson => lesson.Target.Contains(MandatoryWord, StringComparison.Ordinal))
            .ToList();

        // Наблюдение, которое и есть ответ на гейт реестра: слова нет ни в одном
        // выученном ответе, поэтому никакая расстановка адресов его не вернёт.
        Assert.Empty(targetsTeachingTheWord);

        // Строгий эталон пятой строки не равен ни одному выученному ответу.
        Assert.DoesNotContain(PolarStrictGold, Panel.Select(lesson => lesson.Target));

        // Продуктовое плечо обучается, чтобы показать достижимость на уровне памяти.
        var store = new TranslationKnnStore();

        // Единица-исполнитель обучается продуктовым методом, без правок продукта.
        var unit = new ExecutorUnit(Direction, store);

        // Все четыре урока панели записываются в память.
        foreach (var (source, target) in Panel)
            unit.TrainOnExample(BuildExample(source, target, "panel"));

        // Префиксный адрес полярной строки: по нему читается её окружение.
        var polarPrefix = new TranslationSemanticTokenizer().Tokenize(PolarSource, SourceLang).PrefixSignature;

        // Пять ближайших соседей полярной строки в продуктовом плече.
        var productNeighbours = store.FindNearest(polarPrefix, 5);

        // Второе плечо: тот же набор без одинаковых адресов.
        var dedupStore = new TranslationKnnStore();

        // Урок кладётся по одному разу на каждый различный адрес.
        foreach (var (source, target) in Panel)
            foreach (var address in ThreeAddressesOf(source).Distinct(StringComparer.Ordinal))
                dedupStore.AddPair(address, source, target);

        // Пять ближайших соседей полярной строки во втором плече.
        var dedupNeighbours = dedupStore.FindNearest(polarPrefix, 5);

        // Сколько РАЗНЫХ уроков попало в выдачу каждого плеча. Это и есть
        // восстановительная сторона правки, и её надо измерить, а не выводить из
        // порядка вставки: дубли съедают места, поэтому поздний урок в выдачу не
        // попадает, пока одинаковые адреса не сняты.
        var productDistinctLessons = productNeighbours.Select(pair => pair.Source).Distinct(StringComparer.Ordinal).Count();

        // То же число для плеча со снятыми одинаковыми адресами.
        var dedupDistinctLessons = dedupNeighbours.Select(pair => pair.Source).Distinct(StringComparer.Ordinal).Count();

        // Достижим ли поздний четвёртый урок панели в каждом плече: измеряется
        // прямым поиском его источника среди выданных соседей.
        var lateLessonSource = Panel[^1].Source;

        // Достижимость позднего урока в продуктовом плече.
        var lateReachableInProduct = productNeighbours.Any(pair => string.Equals(pair.Source, lateLessonSource, StringComparison.Ordinal));

        // Достижимость позднего урока при снятии одинаковых адресов.
        var lateReachableInDedup = dedupNeighbours.Any(pair => string.Equals(pair.Source, lateLessonSource, StringComparison.Ordinal));

        // НАЗВАННАЯ ДЕГРАДАЦИЯ, закреплённая наблюдением, а не спрятанная.
        // Снятие дубля освобождает места в выдаче и делает поздний урок
        // достижимым. Но обязательное слово «пожалуйста» не содержится ни в одном
        // уроке панели, поэтому строгий эталон пятой строки недостижим В ПРИНЦИПЕ:
        // ответ без этого слова карточка называет ложным принятием, а не ответом
        // на новую строку. Гейт реестра «сначала доказать сохранение
        // обязательного please» на этих данных не проходит, и причина лежит в
        // составе панели, а не в снятии одинаковых адресов.
        WriteEvidence("mandatory-word-gate.md", string.Join("\n", new[]
        {
            "# Гейт реестра: сохраняется ли обязательное слово «пожалуйста»",
            "",
            "Строка очереди переносов 64, идентификатор работы FIX-P60.",
            "",
            $"- полярная строка: {PolarSource}",
            $"- строгий эталон карточки: {PolarStrictGold}",
            $"- уроков панели, учащих слово «{MandatoryWord}»: {targetsTeachingTheWord.Count} из {Panel.Length}",
            $"- ближайших соседей полярной строки в продуктовом плече: {productNeighbours.Count}",
            $"- ближайших соседей полярной строки при снятии одинаковых адресов: {dedupNeighbours.Count}",
            $"- РАЗНЫХ уроков в выдаче продуктового плеча: {productDistinctLessons} из {Panel.Length}",
            $"- РАЗНЫХ уроков в выдаче при снятии одинаковых адресов: {dedupDistinctLessons} из {Panel.Length}",
            $"- поздний урок «{lateLessonSource}» достижим в продуктовом плече: {(lateReachableInProduct ? "да" : "НЕТ")}",
            $"- поздний урок «{lateLessonSource}» достижим при снятии адресов: {(lateReachableInDedup ? "да" : "НЕТ")}",
            "",
            "ВЫВОД, наблюдённый: обязательное слово не встречается ни в одном выученном",
            "ответе панели, поэтому строгий эталон пятой строки недостижим при любой",
            "расстановке адресов. Ответ без этого слова карточка называет ложным",
            "принятием. Гейт реестра на этих данных НЕ ПРОХОДИТ, и причина — состав",
            "панели, а не снятие одинаковых адресов.",
            "",
            "ГРАНИЦА НАБЛЮДЕНИЯ: непроходимость гейта измерена НА ЭТОЙ ПАНЕЛИ из четырёх",
            "уроков, которые слова «" + MandatoryWord + "» не содержат. Это НЕ доказывает, что приём",
            "«один адрес — одна запись» вреден вообще: про другие наборы уроков здесь не",
            "измерено ничего.",
            "",
            "Это закрепление наблюдения, а не предписания: проверка фиксирует то, что",
            "измерено, и не прячет деградацию.",
            ""
        }));
    }

    [Fact]
    public void RepeatedLessonIsNotMergedWhenOnlyItsIdentifierDiffers()
    {
        // Изолированная проверка границы урока из карточки: источник, которому
        // полный адрес назначен вручную, а префиксный и структурный — родные.
        const string isolatedSource = "open file";

        // Выученный перевод этого источника.
        const string isolatedTarget = "открой файл";

        // Вручную назначенный полный адрес, как велит карточка.
        const string manualFullKey = "P60-NONALIAS";

        // Разбор источника настоящим токенизатором.
        var tokens = new TranslationSemanticTokenizer().Tokenize(isolatedSource, SourceLang);

        // Три адреса: назначенный вручную плюс два родных.
        var addresses = new[] { manualFullKey, tokens.PrefixSignature, tokens.StructureSignature };

        // Все три обязаны различаться, иначе проверка границы бессмысленна.
        Assert.Equal(3, addresses.Distinct(StringComparer.Ordinal).Count());

        // Хранилище изолированной проверки.
        var store = new TranslationKnnStore();

        // Первое предъявление урока: по одной записи на каждый различный адрес.
        foreach (var address in addresses)
            store.AddPair(address, isolatedSource, isolatedTarget);

        // После первого урока записей ровно три.
        Assert.Equal(3, store.Count);

        // Тот же урок предъявляется ВТОРОЙ раз отдельной партией: различие только
        // в опознавателе урока и расписке, а содержание то же.
        foreach (var address in addresses)
            store.AddPair(address, isolatedSource, isolatedTarget);

        // Повторное содержание дедуплицировать нельзя: записей стало шесть.
        // Эта проверка краснеет, если снятие дублей сделано слишком широко —
        // например глобальным Distinct по источнику и цели, который карточка
        // прямо запрещает: тогда второй урок не добавит ни одной записи.
        Assert.Equal(6, store.Count);
    }

    // Построение законной учебной строки исполнителя для панели.
    private static TranslationLearningExample BuildExample(string source, string target, string marker)
    {
        // Подпись берётся у настоящего токенизатора, а не придумывается.
        var signature = new TranslationSemanticTokenizer().Tokenize(source, SourceLang).Signature;

        // Строка собирается ровно по форме учебной записи предметной области.
        return new TranslationLearningExample(
            ExampleId: $"fix-p60-{marker}-{source.Replace(' ', '-')}",
            UnitId: TranslationUnitIds.ExecutorFor(Direction),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(source, SourceLang, Topic), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(target, 0.95, null, null), UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("[WRONG: missing translation]", 0.1, null), UnitJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }
}
