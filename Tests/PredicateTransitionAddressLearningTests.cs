// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using ExperimentsV2;

namespace LlmFromScratch.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ПОСТОЯННАЯ ПРОВЕРКА: текущий предикат обязан участвовать в адресе памяти
// следующего перехода.
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ
// В проекте есть память-подсказчик «какое действие в коде пойдёт следующим».
// Она работает как записная книжка: по адресу лежит ответ, при вопросе берётся
// строка с самым похожим адресом. Дефект был в том, что адресом служил ТОЛЬКО
// портрет файла — доли видов действий в этом файле. Портрет один на весь файл, а
// вопросов к файлу много: «что после проверки на пустую ссылку?» и «что после
// возврата значения?» попадали в ОДНУ строку книжки. Записывалось несколько
// ответов, а отвечал всегда самый ранний: остальные становились недостижимыми.
// Починка добавляет к адресу отметку «а какое действие сейчас?».
//
// ОТКУДА ДАННЫЕ
// Ровно данные опыта «адрес памяти кэша предсказания следующего предиката»
// (номер опыта P47-PRIMARY-01, подслучай P47.1): четыре обучающих файла, двенадцать
// переходов, четыре экзаменационных запроса. Опыт показал: учебные контроли
// 8 из 12 против 12 из 12, разрушительных коллизий адресов 4 против 0, ни одной
// потерянной верной строки.
//
// ДВЕ НЕЗАВИСИМЫЕ ОПОРЫ ЭТОЙ ПРОВЕРКИ
// 1. Точные данные опыта (проверки 1-7). Писатель и читатель памяти здесь
//    настоящие продуктовые: `PredicateCache.Add` и `PredicateCache.Lookup` с
//    неизменённым порогом похожести 0.85. Сборка адреса берётся у продукта.
// 2. Настоящий путь продукта (проверка 8). Через отражение вызывается закрытый
//    производитель пар `E30PredicateCache.ExtractPairsWithVecs` на настоящем
//    тексте C#, разобранном настоящим разборчиком кода. Это доказывает, что
//    починка действительно подключена в рабочем пути, а не просто лежит рядом.
//
// ЧЕСТНАЯ ОГОВОРКА ПРО ПОРТРЕТ ФАЙЛА
// Продуктовый вход построения портрета `PredicateCache.BuildVector(string)`
// принимает ТЕКСТ и разбирает его настоящим разборчиком кода. Словарь набора
// опыта (0 — проверка на пустую ссылку, 1 — возврат значения, 2 — загрузка,
// 3 — сохранение, 4 — вызов, 5 — ожидание) — локальный словарь опыта, а не
// продуктовые номера, и настоящий разборчик такие последовательности по тексту
// не выдаёт. Поэтому для точных данных опыта портрет считается здесь местной
// копией той же арифметики (22 разряда, отсев номеров вне границ, деление на
// сумму) — ровно так же, как это сделал и сам опыт, с полной шапкой. Настоящая
// продуктовая сборка портрета по тексту проверяется отдельно, в проверке 8.
//
// ЧЕГО ЭТА ПРОВЕРКА НЕ ДОКАЗЫВАЕТ
// Она не доказывает продуктовую готовность: это исправление экспериментального
// потребителя. Она не доказывает загрузку памяти в новом процессе — у этой
// памяти вообще нет сохранения состояния. Она не доказывает межпроектную
// изоляцию: у опыта похожесть чужих адресов 0.833333 при пороге 0.85, то есть
// разделение держится на пороге, а не на устройстве адреса, и здесь оно не
// утверждается.
// ─────────────────────────────────────────────────────────────────────────────

public sealed class PredicateTransitionAddressLearningTests
{
    // Разрядность портрета файла. Продуктовая постоянная `new float[22]`
    // в файле 35.ExperimentsV2/Infrastructure/PredicateCache.cs, строка 35.
    private const int FileVectorSlots = 22;

    // Порог похожести. Берётся продуктовый по умолчанию и НЕ меняется:
    // подгонка порога запрещена условием переноса.
    private const float SimilarityThreshold = 0.85f;

    // ── Локальный словарь набора опыта (не продуктовые обозначения) ──────────
    private const int NullCheck = 0;  // проверка на пустую ссылку
    private const int Return    = 1;  // возврат значения
    private const int Load      = 2;  // загрузка
    private const int Save      = 3;  // сохранение
    private const int Call      = 4;  // вызов
    private const int Await     = 5;  // ожидание

    // Человеческие названия номеров — нужны, чтобы сообщения о падении читались.
    private static string Name(int predicate) => predicate switch
    {
        NullCheck => "проверка на пустую ссылку",
        Return    => "возврат значения",
        Load      => "загрузка",
        Save      => "сохранение",
        Call      => "вызов",
        Await     => "ожидание",
        -1        => "отказ памяти",
        _         => $"номер {predicate}",
    };

    // Один вопрос к памяти: в каком файле, какого проекта, какое действие сейчас
    // и какое действие обязано быть названо следующим.
    private sealed record Question(string File, string Project, int Current, int Expected);

    // Один ответ памяти на один вопрос.
    private sealed record Answer(Question Question, int Predicted, bool Found)
    {
        public bool Ok => Predicted == Question.Expected;

        // Читаемая строка для сообщения о падении.
        public string Human =>
            $"файл {Question.File} (проект {Question.Project}), сейчас "
            + $"{Name(Question.Current)} -> ожидалось {Name(Question.Expected)}, "
            + $"память ответила {Name(Predicted)}";
    }

    // Четыре обучающих файла опыта в том же порядке подачи. Порядок важен:
    // именно он определял, чья ранняя запись занимала общий адрес.
    private static readonly (string File, string Project, int[] Sequence)[] TrainingFiles =
    [
        ("f1", "A", [NullCheck, Return, NullCheck, Return]),
        ("f2", "A", [Load, Save, Load, Save]),
        ("f3", "B", [NullCheck, Save, NullCheck, Save]),
        ("f4", "B", [Call, Await, Call, Await]),
    ];

    // Четыре экзаменационных файла опыта. С обучающими по источнику не пересекаются.
    private static readonly (string File, string Project, int[] Sequence)[] ExamFiles =
    [
        ("t1", "A", [NullCheck, Return]),
        ("t2", "A", [Load, Save]),
        ("t3", "B", [NullCheck, Save]),
        ("t4", "B", [Call, Await]),
    ];

    // Четыре строки, которые в опыте исправились поимённо: старое правило во всех
    // четырёх повторило ответ ПЕРВОГО перехода своего файла.
    private static readonly Question[] FourNamedRows =
    [
        new("f1", "A", Return, NullCheck),  // f1: после возврата значения -> проверка на пустую ссылку
        new("f2", "A", Save,   Load),       // f2: после сохранения        -> загрузка
        new("f3", "B", Save,   NullCheck),  // f3: после сохранения        -> проверка на пустую ссылку
        new("f4", "B", Await,  Call),       // f4: после ожидания          -> вызов
    ];

    // ── Сборка адреса: продуктовая и прежняя ────────────────────────────────

    // Продуктовая сборка адреса перехода, если она в продукте уже есть.
    // Ищется по имени, поэтому файл проверки одинаково собирается и до починки,
    // и после неё: до починки метода нет и проверка падает на ЗНАЧЕНИЯХ, а не на
    // сборке.
    private static readonly MethodInfo? ProductionAddressBuilder =
        typeof(E30PredicateCache).GetMethod(
            "BuildTransitionAddress",
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(float[]), typeof(int)],
            null);

    // Адрес, который строит продукт сейчас. Пока продуктовой сборки нет, продукт
    // по факту использует один портрет файла на все его пары — это и есть
    // дефект, зафиксированный в файле 35.ExperimentsV2/Experiments/E30PredicateCache.cs,
    // строка 100 (один `BuildVector` на файл) и строки 45 и 50 (запись и чтение
    // по этому одному адресу).
    private static float[] ProductionAddress(float[] fileVector, int currentPredicate)
        => ProductionAddressBuilder is null
            ? fileVector
            : (float[])ProductionAddressBuilder.Invoke(
                null, new object[] { fileVector, currentPredicate })!;

    // Прежнее правило адреса, сохранённое как опорный образец: адрес равен
    // портрету файла, текущее действие в него не входит. Оно остаётся в проверке
    // навсегда, чтобы было видно, что набор данных действительно показывает
    // дефект, и чтобы проверка не превратилась в пустую.
    private static float[] FileOnlyAddress(float[] fileVector, int currentPredicate)
        => fileVector;

    // ── Местная копия арифметики портрета файла ─────────────────────────────
    // Построчно повторяет `PredicateCache.BuildVector`, строки 35-41: массив ровно
    // из 22 разрядов, отсев номеров вне границ, прибавление единицы за вхождение,
    // деление каждого разряда на сумму. Не перенесён только вызов разборчика кода:
    // набор опыта задаёт последовательности действий напрямую.
    private static float[] BuildFilePortrait(IReadOnlyList<int> predicates)
    {
        var vec = new float[FileVectorSlots];
        if (predicates.Count == 0) return vec;
        foreach (var p in predicates) if (p >= 0 && p < FileVectorSlots) vec[p]++;
        float total = vec.Sum();
        if (total > 0) for (int i = 0; i < FileVectorSlots; i++) vec[i] /= total;
        return vec;
    }

    // Переходы файла: из последовательности действий получаются пары
    // «сейчас -> следующее».
    private static IEnumerable<Question> Transitions(
        (string File, string Project, int[] Sequence) file)
    {
        for (int i = 0; i < file.Sequence.Length - 1; i++)
            yield return new Question(
                file.File, file.Project, file.Sequence[i], file.Sequence[i + 1]);
    }

    // Итог одного полного прогона памяти при заданной сборке адреса.
    private sealed record MemoryRun(
        List<Answer> Retention,          // чтения двенадцати учебных переходов
        List<Answer> Exam,               // четыре экзаменационных чтения
        int Writes,                      // сколько настоящих записей сделано
        int DistinctAddresses,           // сколько различных адресов занято
        int Collisions,                  // сколько записей легло в занятый адрес
        int DestructiveCollisions,       // из них с ДРУГИМ ответом у ранней записи
        HashSet<int> AddressLengths);    // все встреченные длины адресов

    // Полный прогон: обучение настоящими продуктовыми записями, затем чтение
    // настоящим продуктовым потребителем. Эталонный ответ в ответ памяти нигде
    // не подставляется — он участвует только в сравнении уже полученного ответа.
    private static MemoryRun RunMemory(Func<float[], int, float[]> address)
    {
        // Настоящая продуктовая память с неизменённым порогом похожести.
        var cache = new PredicateCache(threshold: SimilarityThreshold);

        // Список уже занятых адресов вместе с ответом САМОЙ РАННЕЙ записи:
        // именно ранняя запись закрывает собой адрес при равной похожести.
        var occupied = new List<(float[] Address, int FirstAnswer)>();

        int writes = 0, collisions = 0, destructive = 0;
        var lengths = new HashSet<int>();

        // ── Обучение: двенадцать настоящих записей ──────────────────────────
        foreach (var file in TrainingFiles)
        {
            // Портрет файла считается один раз на файл — так же, как в продукте.
            var portrait = BuildFilePortrait(file.Sequence);

            foreach (var q in Transitions(file))
            {
                // Адрес строится НА КАЖДУЮ пару: в этом и состоит проверяемая разница.
                var addr = address(portrait, q.Current);
                lengths.Add(addr.Length);

                // Учёт коллизий: совпал ли адрес с уже занятым и несёт ли ранняя
                // запись другой ответ.
                int existing = occupied.FindIndex(e => SameAddress(e.Address, addr));
                if (existing >= 0)
                {
                    collisions++;
                    if (occupied[existing].FirstAnswer != q.Expected) destructive++;
                }
                else occupied.Add((addr, q.Expected));

                // Настоящий продуктовый писатель.
                cache.Add(addr, q.Expected);
                writes++;
            }
        }

        // ── Чтение учебных переходов: удержание выученного ─────────────────
        var retention = new List<Answer>();
        foreach (var file in TrainingFiles)
        {
            var portrait = BuildFilePortrait(file.Sequence);
            foreach (var q in Transitions(file))
            {
                var query = address(portrait, q.Current);
                lengths.Add(query.Length);
                var (found, predicted) = cache.Lookup(query);
                retention.Add(new Answer(q, predicted, found));
            }
        }

        // ── Чтение экзаменационных запросов ────────────────────────────────
        var exam = new List<Answer>();
        foreach (var file in ExamFiles)
        {
            var portrait = BuildFilePortrait(file.Sequence);
            foreach (var q in Transitions(file))
            {
                var query = address(portrait, q.Current);
                lengths.Add(query.Length);
                var (found, predicted) = cache.Lookup(query);
                exam.Add(new Answer(q, predicted, found));
            }
        }

        return new MemoryRun(
            retention, exam, writes, occupied.Count, collisions, destructive, lengths);
    }

    // Точное совпадение адресов: сначала длина, затем каждый разряд.
    private static bool SameAddress(float[] a, float[] b)
        => a.Length == b.Length && a.SequenceEqual(b);

    // Читаемый перечень неверных ответов для сообщения о падении.
    private static string Failures(IEnumerable<Answer> answers)
        => string.Join("; ", answers.Where(a => !a.Ok).Select(a => a.Human));

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 1. Удержание выученного: двенадцать переходов из двенадцати.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void AddressWithCurrentPredicate_HoldsAllTwelveLearnedTransitions()
    {
        var run = RunMemory(ProductionAddress);

        // Все двенадцать настоящих записей обучения состоялись.
        Assert.Equal(12, run.Writes);

        // Все двенадцать учебных переходов прочитаны обратно верно.
        int correct = run.Retention.Count(a => a.Ok);
        Assert.True(
            correct == 12,
            $"Удержание выученного: верно {correct} из 12. Неверные строки: "
            + $"{Failures(run.Retention)}. Пока текущее действие не входит в адрес, "
            + "разные вопросы к одному файлу попадают в одну строку памяти и "
            + "отвечает самая ранняя запись.");

        // Отказов памяти быть не должно: адрес находится всегда.
        Assert.Empty(run.Retention.Where(a => !a.Found));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 2. Экзамен: четыре запроса из четырёх.
    // ЧЕСТНО: эта проверка НЕ различает старое и новое правило — в опыте оба
    // плеча дали 4 из 4, потому что у старого правила на этой панели не осталось
    // запаса. Здесь она стоит как защита от ухудшения, а не как доказательство
    // выигрыша.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void AddressWithCurrentPredicate_AnswersAllFourExamQueries()
    {
        var run = RunMemory(ProductionAddress);

        int correct = run.Exam.Count(a => a.Ok);
        Assert.True(
            correct == 4,
            $"Экзамен: верно {correct} из 4. Неверные строки: {Failures(run.Exam)}.");

        Assert.Empty(run.Exam.Where(a => !a.Found));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 3. Разрушительные коллизии адресов исчезают: 4 против 0.
    // Безвредные совпадения одинаковых ответов ошибками НЕ объявляются.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void AddressWithCurrentPredicate_RemovesTheFourDestructiveCollisions()
    {
        var oldRule = RunMemory(FileOnlyAddress);
        var current = RunMemory(ProductionAddress);

        // Опорный образец: прежнее правило действительно даёт четыре
        // разрушительные коллизии на этих данных.
        Assert.Equal(4, oldRule.DestructiveCollisions);

        // Починка: ни одной разрушительной коллизии.
        Assert.True(
            current.DestructiveCollisions == 0,
            $"Разрушительных коллизий адресов {current.DestructiveCollisions} "
            + "вместо 0. Разрушительная коллизия — это когда занявшая адрес ранняя "
            + "запись несёт ДРУГОЙ ответ, и правильная запись становится "
            + "недостижимой.");

        // Адресов становится по одному на пару «файл и текущее действие»: 8 вместо 4.
        Assert.Equal(4, oldRule.DistinctAddresses);
        Assert.Equal(8, current.DistinctAddresses);

        // Четыре оставшихся совпадения безвредны: они повторяют уже записанный
        // ответ (третий переход каждого файла совпадает с первым). Это не ошибка.
        Assert.Equal(4, current.Collisions);
        Assert.Equal(8, oldRule.Collisions);

        // Ни одна строка не потеряна: записей по-прежнему двенадцать.
        Assert.Equal(12, current.Writes);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 4. Ни один прежде верный ответ не потерян.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void AddressWithCurrentPredicate_KeepsEveryAnswerTheOldRuleAlreadyGotRight()
    {
        var oldRule = RunMemory(FileOnlyAddress);
        var current = RunMemory(ProductionAddress);

        // Прежде верные строки — те восемь, которые старое правило уже отвечало верно.
        var previouslyCorrect = oldRule.Retention.Where(a => a.Ok).Select(a => a.Question).ToList();
        Assert.Equal(8, previouslyCorrect.Count);

        // Каждая из них обязана остаться верной.
        var lost = new List<string>();
        foreach (var q in previouslyCorrect)
        {
            var now = current.Retention.First(a => a.Question == q);
            if (!now.Ok) lost.Add(now.Human);
        }

        Assert.True(
            lost.Count == 0,
            $"Потеряно прежде верных строк: {lost.Count}. Это: {string.Join("; ", lost)}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 5. Четыре исправленные строки поимённо.
    // Это главная проверка переноса: именно эти четыре вопроса старое правило
    // отвечало ответом первого перехода своего файла.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void FourNamedRows_WrongUnderTheOldRule_AreAnsweredCorrectlyNow()
    {
        var oldRule = RunMemory(FileOnlyAddress);
        var current = RunMemory(ProductionAddress);

        var stillWrong = new List<string>();
        foreach (var expectedRow in FourNamedRows)
        {
            // Строка обязана присутствовать в наборе: иначе данные разошлись с опытом.
            var before = oldRule.Retention.First(a => a.Question == expectedRow);
            var after  = current.Retention.First(a => a.Question == expectedRow);

            // Опорный образец: при старом правиле эта строка была неверной.
            Assert.False(
                before.Ok,
                $"Ожидалось, что при старом правиле строка «{before.Human}» неверна, "
                + "но она верна. Значит набор данных больше не показывает дефект.");

            // Починка: теперь она верна.
            if (!after.Ok) stillWrong.Add(after.Human);
        }

        Assert.True(
            stillWrong.Count == 0,
            $"Из четырёх поимённо исправляемых строк остались неверными "
            + $"{stillWrong.Count}: {string.Join("; ", stillWrong)}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 6. Опорный образец дефекта: прежнее правило даёт 8 из 12.
    // Эта проверка верна и до починки, и после неё. Она существует, чтобы
    // проверка 1 не могла стать пустой: если набор данных перестанет показывать
    // дефект, здесь это будет видно сразу.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void OldFileOnlyRule_StillShowsTheDefectOnExactlyThisData()
    {
        var oldRule = RunMemory(FileOnlyAddress);

        Assert.Equal(12, oldRule.Writes);
        Assert.Equal(8, oldRule.Retention.Count(a => a.Ok));
        Assert.Equal(4, oldRule.Retention.Count(a => !a.Ok));

        // На экзамене старое правило тоже даёт 4 из 4 — различения здесь нет.
        Assert.Equal(4, oldRule.Exam.Count(a => a.Ok));

        // Каждая из четырёх неверных строк — это ответ первого перехода файла.
        foreach (var expectedRow in FourNamedRows)
            Assert.False(oldRule.Retention.First(a => a.Question == expectedRow).Ok);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 7. Все адреса одного прогона одной длины.
    // Мера похожести `CosineSim` идёт циклом по длине ПЕРВОГО вектора, поэтому
    // адрес другой длины в том же хранилище означал бы чтение за границей массива.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void EveryAddressInOneRun_HasExactlyOneLength()
    {
        var current = RunMemory(ProductionAddress);

        Assert.True(
            current.AddressLengths.Count == 1,
            "В одном прогоне встретились адреса разной длины: "
            + $"{string.Join(", ", current.AddressLengths.OrderBy(x => x))}. "
            + "Мера похожести идёт по длине первого вектора, поэтому разные длины "
            + "в одном хранилище дают чтение за границей массива.");

        // При подключённой продуктовой сборке длина обязана быть 44: 22 разряда
        // портрета файла и 22 разряда блока текущего действия.
        if (ProductionAddressBuilder is not null)
            Assert.Equal(44, current.AddressLengths.Single());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 8. Починка подключена в настоящем пути продукта.
    // Через отражение вызывается закрытый производитель пар на настоящем тексте
    // C#, разобранном настоящим разборчиком кода. Требование: внутри одного файла
    // разные текущие действия обязаны давать РАЗНЫЕ адреса, а одинаковые
    // текущие действия — один и тот же адрес.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void ProductionPairProducer_GivesOneAddressPerCurrentPredicate()
    {
        // Два настоящих файла C#. Каждый даёт последовательность действий, в
        // которой есть несколько РАЗНЫХ текущих действий.
        const string readerSource = """
            public sealed class Reader
            {
                public int Read(string? source)
                {
                    if (source == null) { return 0; }
                    var length = source.Length;
                    Log(length);
                    return length;
                }

                private static void Log(int value) { }
            }
            """;

        const string writerSource = """
            public sealed class Writer
            {
                public int Save(string? payload)
                {
                    if (payload == null) { return 0; }
                    var prepared = payload.Trim();
                    Store(prepared);
                    return prepared.Length;
                }

                private static void Store(string value) { }
            }
            """;

        var files = new List<(string Path, string Text)>
        {
            ("Reader.cs", readerSource),
            ("Writer.cs", writerSource),
        };

        // Закрытый производитель пар продукта. Он есть и до починки, и после неё,
        // поэтому эта проверка падает именно на ЗНАЧЕНИЯХ.
        var producer = typeof(E30PredicateCache).GetMethod(
            "ExtractPairsWithVecs", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(producer);

        var produced = producer!.Invoke(null, new object[] { files });
        Assert.NotNull(produced);

        // Пары читаются отражением: тип пары закрытый, читаем поля по именам.
        var rows = new List<(int Index, float[] Vec, int Current, int Next)>();
        int index = 0;
        foreach (var item in (System.Collections.IEnumerable)produced!)
        {
            var type    = item!.GetType();
            var vec     = (float[])type.GetProperty("Vec")!.GetValue(item)!;
            var current = (int)type.GetProperty("Current")!.GetValue(item)!;
            var next    = (int)type.GetProperty("Next")!.GetValue(item)!;
            rows.Add((index++, vec, current, next));
        }

        Assert.NotEmpty(rows);

        // Пары одного файла идут подряд, поэтому файл определяется по портрету:
        // внутри файла портрет один и тот же. Группируем по номеру файла, считая
        // границу там, где меняется длина или содержимое неизменной части.
        // Проще и надёжнее: проверяем попарно все строки с одинаковым набором
        // «тот же файл» через равенство первых 22 разрядов адреса.
        int filesWithTwoDistinctCurrents = 0;
        var sameFileGroups = rows
            .GroupBy(r => string.Join(",", r.Vec.Take(FileVectorSlots)))
            .ToList();

        foreach (var group in sameFileGroups)
        {
            var list = group.ToList();
            var distinctCurrents = list.Select(r => r.Current).Distinct().ToList();
            if (distinctCurrents.Count >= 2) filesWithTwoDistinctCurrents++;

            foreach (var a in list)
                foreach (var b in list)
                {
                    if (a.Index == b.Index) continue;

                    if (a.Current == b.Current)
                        Assert.True(
                            SameAddress(a.Vec, b.Vec),
                            "Один файл и одно текущее действие "
                            + $"({Name(a.Current)}) дали РАЗНЫЕ адреса. Тогда одно и "
                            + "то же выученное соответствие легло бы в две строки памяти.");
                    else
                        Assert.True(
                            !SameAddress(a.Vec, b.Vec),
                            "Один файл, но разные текущие действия "
                            + $"({Name(a.Current)} и {Name(b.Current)}) дали ОДИН адрес. "
                            + "Значит текущее действие не участвует в адресе: разные "
                            + "вопросы к файлу попадут в одну строку памяти и ответит "
                            + "самая ранняя запись.");
                }
        }

        // Защита от пустой проверки: хотя бы один файл обязан дать два разных
        // текущих действия, иначе сравнивать было нечего.
        Assert.True(
            filesWithTwoDistinctCurrents >= 1,
            "Ни один файл набора не дал двух разных текущих действий, поэтому "
            + "проверка подключения ничего не проверила. Нужен другой текст файла.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Проверка 9. Номер текущего действия вне диапазона фиксируется, а не
    // отбрасывается молча.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void CurrentPredicateOutOfRange_IsReportedNotSilentlyDropped()
    {
        Assert.NotNull(ProductionAddressBuilder);

        // Признак «номер уложился в блок» обязан быть доступен вызывающему,
        // иначе такой номер нельзя посчитать и показать в отчёте.
        var encodable = typeof(E30PredicateCache).GetMethod(
            "IsCurrentPredicateEncodable",
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(int)],
            null);
        Assert.NotNull(encodable);

        // Номер внутри собственного диапазона механизма укладывается.
        Assert.True((bool)encodable!.Invoke(null, new object[] { Save })!);

        // Номера вне диапазона 0..21 не укладываются и обязаны быть видны как
        // отдельный случай.
        Assert.False((bool)encodable.Invoke(null, new object[] { 22 })!);
        Assert.False((bool)encodable.Invoke(null, new object[] { -1 })!);

        // Адрес при этом остаётся той же длины, а блок текущего действия — нулевым:
        // строка не теряется, но и ложного различения не появляется.
        var portrait = BuildFilePortrait(TrainingFiles[0].Sequence);
        var address  = ProductionAddress(portrait, 99);
        Assert.Equal(44, address.Length);
        for (int i = FileVectorSlots; i < address.Length; i++)
            Assert.Equal(0f, address[i]);
    }
}
