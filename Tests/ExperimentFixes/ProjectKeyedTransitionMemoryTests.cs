// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса опыта 72 «условие проекта в памяти переходов»
// (отчёт experiments-v2/RP01-MICRO/P7201/results/attempt-0001/report.md,
// верхняя запись в docs/plans/replanning-possible-diffs/required-changes.md).
//
// Что здесь проверяется по-человечески. Память переходов учится на уроках
// «текущее действие -> следующее действие» и потом отвечает самым частым
// продолжением. Уроки приходят из файлов разных проектов. Пока ключ памяти —
// только текущее действие, уроки двух проектов складываются в один ключ, и на
// запрос одного проекта память обязана выбрать одно продолжение из двух
// законных, то есть в половине случаев возвращает закон чужого проекта.
// Измеренный след этого дефекта: на запрос «второй проект, сохранение» общая
// таблица вернула «загрузку», а загрузка во втором проекте не встречается
// ни разу.
//
// Три инварианта, по одному на каждую правку наряда:
//   1) каждая извлечённая пара несёт идентификатор своего проекта;
//   2) на данных с намеренным конфликтом каждый проект получает СВОЙ закон;
//   3) при отсутствии записи ответ отличим от законной метки 0.
//
// Почему проверка написана через отражение, а не прямым вызовом. Она обязана
// собираться и падать НА ЗНАЧЕНИЯХ и до правки, и после неё. Прямой вызов
// нового составного ключа не собрался бы на старом коде, и красный прогон
// доказывал бы только отсутствие метода, а не поведение механизма. Поэтому
// проверка находит продуктовые методы по имени, читает их фактическую форму и
// подставляет аргументы по типам параметров. Форму она не предписывает:
// признаком «записи нет» принимается и отдельный выходной параметр, и возврат
// с признаком успеха, и значение вне области меток.
//
// Что эта проверка НЕ доказывает: она не измеряет качество обучения, не
// работает на длинных наборах и не трогает ни выбор при ничье, ни гейт
// неизвестного проекта — это предметы отдельных подслучаев опыта.
namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Collections;
using System.Reflection;

public sealed class ProjectKeyedTransitionMemoryTests
{
    // Метки предикатов продуктового извлекателя
    // (12.SemanticCompiler/Compression/LogicExtractor.cs): 0 — NULL_GUARD
    // («проверка на пустое»), 7 — ASSIGN, 9 — CALL, 11 — RETURN, 20 — LOG_OP.
    // Ноль назван отдельно, потому что именно он сталкивается с отказом.
    private const int NullGuard = 0;
    private const int Assign = 7;
    private const int Call = 9;
    private const int Return = 11;
    private const int LogOp = 20;

    // Два проекта урока. Имена условны: важно лишь, что они различны и что
    // один и тот же текущий предикат имеет в них разные законные продолжения.
    private const string FirstProject = "project-A";
    private const string SecondProject = "project-B";

    // Проверяемый продуктовый тип. Берём его по типу, а не по строке, чтобы
    // переименование класса ломало сборку проверки, а не тихо её пропускало.
    private static readonly Type Adaptor = typeof(ExperimentsV2.E28ProjectAdaptor);

    // Уроки с намеренным конфликтом. Текущий предикат 0 в первом проекте ведёт
    // к 11, а во втором — к 9; счётчики нарочно неравны (3 против 2), чтобы на
    // общем ключе победитель определялся числом наблюдений, а не порядком
    // перечисления словаря: иначе красный прогон зависел бы от порядка уроков.
    // Урок 11 -> 0 добавлен, чтобы метка 0 была законным ОТВЕТОМ этой памяти —
    // без него третий инвариант нечего было бы путать с отказом.
    private static IReadOnlyList<(string Project, int Current, int Next)> ConflictLessons() =>
    [
        (FirstProject, NullGuard, Return),
        (FirstProject, NullGuard, Return),
        (FirstProject, NullGuard, Return),
        (FirstProject, Return, NullGuard),
        (SecondProject, NullGuard, Call),
        (SecondProject, NullGuard, Call),
        (SecondProject, Call, Assign),
    ];

    // ── Правка 1: принадлежность проекта доходит до пар ───────────────────────
    [Fact]
    public void ExtractedPairsCarryTheirOwnProject()
    {
        // Два отдельных вызова, по одному файлу на проект: тогда ожидаемый
        // идентификатор каждой пары известен точно, без догадок о том, какой
        // предикат пришёл из какого файла.
        var firstPairs = Extract(("21.AgentInfrastructure/Logical/Unit.cs", SampleSource, FirstProject));
        var secondPairs = Extract(("22.AgentAlgorithms/Context/Router.cs", SampleSource, SecondProject));

        Assert.True(firstPairs.Count > 0, "извлекатель не дал ни одной пары на учебном тексте первого проекта");
        Assert.True(secondPairs.Count > 0, "извлекатель не дал ни одной пары на учебном тексте второго проекта");

        // Каждая пара обязана нести идентификатор своего и только своего проекта.
        var firstSeen = ProjectsOf(firstPairs);
        var secondSeen = ProjectsOf(secondPairs);

        Assert.All(firstSeen, p => Assert.Equal(FirstProject, p));
        Assert.All(secondSeen, p => Assert.Equal(SecondProject, p));
        Assert.Equal(firstPairs.Count, firstSeen.Count);
        Assert.Equal(secondPairs.Count, secondSeen.Count);
    }

    // ── Правка 2: на конфликте каждый проект получает свой закон ──────────────
    [Fact]
    public void EachProjectGetsItsOwnLawOnAConflictingKey()
    {
        var memory = Train(ConflictLessons());

        // Первый проект: после «проверки на пустое» законен возврат.
        var first = Ask(memory, FirstProject, NullGuard);
        // Второй проект: после той же «проверки на пустое» законен вызов.
        var second = Ask(memory, SecondProject, NullGuard);

        Assert.True(first.Found, $"память не ответила на запрос ({FirstProject}, {NullGuard}): {first.Shape}");
        Assert.True(second.Found, $"память не ответила на запрос ({SecondProject}, {NullGuard}): {second.Shape}");

        Assert.Equal(Return, first.Label);

        // Главная строка красного прогона: на старом общем ключе здесь
        // возвращается закон первого проекта (11), потому что его наблюдений
        // больше, хотя во втором проекте продолжение 11 не встречается ни разу.
        Assert.True(
            second.Label == Call,
            $"на запрос ({SecondProject}, {NullGuard}) память вернула {second.Label}, " +
            $"ожидался {Call}; значение {Return} — это дословно закон проекта {FirstProject}, " +
            $"которого во втором проекте нет ни в одном уроке. Ключ памяти не разделён по проекту.");
    }

    // ── Правка 3: отказ отличим от законной метки «проверка на пустое» ────────
    [Fact]
    public void MissingRecordIsDistinguishableFromTheLegalZeroLabel()
    {
        var lessons = ConflictLessons();
        var memory = Train(lessons);

        // Метка 0 — законный ОТВЕТ этой памяти: урок 11 -> 0 ей преподан.
        Assert.Contains(lessons, l => l.Next == NullGuard);

        // Предикат 20 не встречается как текущий ни в одном уроке, поэтому
        // записи для него нет ни у своего проекта, ни на запасном пути.
        Assert.DoesNotContain(lessons, l => l.Current == LogOp);

        var answer = Ask(memory, FirstProject, LogOp);

        Assert.False(
            !answer.HasFoundFlag && answer.Label == NullGuard,
            $"при отсутствии записи механизм вернул {answer.Label} и не вернул признака «записи нет» " +
            $"({answer.Shape}); {NullGuard} — законная метка «проверка на пустое», которой эта же память " +
            "отвечает на урок 11 -> 0, поэтому отказ неотличим от предсказания.");

        Assert.False(answer.Found, $"записи нет, но механизм сообщил, что ответ найден: {answer.Shape}");
    }

    // ── Опора проверки: вызов продуктовых методов по их фактической форме ─────

    // Учебный текст одного файла. Содержит «== null» и «.Fail(», на которые у
    // продуктового извлекателя есть прямые текстовые правила, поэтому предикатов
    // гарантированно не меньше двух и хотя бы одна пара существует.
    private const string SampleSource =
        "public OperationResult<int> Handle(string input)\n" +
        "{\n" +
        "    if (input == null) return OperationResult<int>.Fail(\"empty\");\n" +
        "    var value = Convert(input);\n" +
        "    return OperationResult<int>.Ok(value);\n" +
        "}\n";

    // Ищем продуктовый метод по части имени. Часть имени, а не точное имя,
    // нужна потому, что локальная функция компилируется в метод с искажённым
    // именем вида «<Run>g__Predict|0_0», и проверка обязана находить её тоже.
    private static (MethodInfo Method, object? Target) Find(string nameFragment)
    {
        const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        var found = Adaptor.GetMethods(Any)
            .Where(m => m.Name.Contains(nameFragment, StringComparison.Ordinal))
            .OrderBy(m => m.Name.Length)
            .ToList();

        Assert.True(
            found.Count > 0,
            $"в типе {Adaptor.FullName} нет метода с именем, содержащим «{nameFragment}»");

        var method = found[0];
        // Статический метод вызывается без получателя; экземплярный — на новом
        // объекте: механизм не держит состояния между вызовами.
        return (method, method.IsStatic ? null : Activator.CreateInstance(Adaptor));
    }

    // Собирает кортеж нужной формы: значения подставляются по числу полей, а
    // форму задаёт сам продуктовый метод, а не эта проверка.
    private static object MakeTuple(Type tupleType, params object[] values)
    {
        int arity = tupleType.GetGenericArguments().Length;
        Assert.True(
            arity == values.Length,
            $"кортеж {tupleType} ожидает {arity} полей, а проверка подготовила {values.Length}");
        return Activator.CreateInstance(tupleType, values)!;
    }

    // Вызывает продуктовое извлечение пар на заданных файлах.
    private static List<object> Extract(params (string Path, string Text, string Project)[] files)
    {
        var (method, target) = Find("ExtractPredPairs");
        var parameters = method.GetParameters();
        Assert.True(parameters.Length == 1, $"извлечение пар ожидалось с одним параметром, а их {parameters.Length}");

        var fileTuple = parameters[0].ParameterType.GetGenericArguments()[0];
        int arity = fileTuple.GetGenericArguments().Length;
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(fileTuple))!;

        foreach (var file in files)
        {
            // Старая форма файла — (путь, текст); новая — (путь, текст, проект).
            list.Add(arity >= 3
                ? MakeTuple(fileTuple, file.Path, file.Text, file.Project)
                : MakeTuple(fileTuple, file.Path, file.Text));
        }

        var result = method.Invoke(target, [list]);
        Assert.NotNull(result);
        return ((IEnumerable)result!).Cast<object>().ToList();
    }

    // Читает идентификатор проекта из каждой извлечённой пары. Если в паре нет
    // ни одного строкового поля, принадлежность проекту до пар не дошла — и
    // сообщение называет фактическую форму пары, а не отсутствующий метод.
    private static List<string> ProjectsOf(List<object> pairs)
    {
        var projects = new List<string>();
        foreach (var pair in pairs)
        {
            var fields = pair.GetType().GetFields();
            var text = fields.FirstOrDefault(f => f.FieldType == typeof(string));
            Assert.True(
                text is not null,
                "в извлечённой паре нет строкового поля с идентификатором проекта — принадлежность файла " +
                "к проекту потеряна до построения памяти; фактические поля пары: " +
                string.Join(", ", fields.Select(f => $"{f.Name}:{f.FieldType.Name}")));
            projects.Add((string)text!.GetValue(pair)!);
        }
        return projects;
    }

    // Обучает продуктовую память на уроках. Если продуктовый ключ ещё не несёт
    // проекта, проект отбрасывается — ровно та же потеря, что происходит
    // сегодня в продуктовом пути, и обучение идёт на тех же уроках.
    private static (object Model, Type ModelType) Train(
        IReadOnlyList<(string Project, int Current, int Next)> lessons)
    {
        var (method, target) = Find("BuildBigram");
        var pairTuple = method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
        int arity = pairTuple.GetGenericArguments().Length;

        if (arity >= 3)
        {
            Assert.Equal(typeof(string), pairTuple.GetGenericArguments()[0]);
        }

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(pairTuple))!;
        foreach (var lesson in lessons)
        {
            list.Add(arity >= 3
                ? MakeTuple(pairTuple, lesson.Project, lesson.Current, lesson.Next)
                : MakeTuple(pairTuple, lesson.Current, lesson.Next));
        }

        var model = method.Invoke(target, [list]);
        Assert.NotNull(model);
        return (model!, method.ReturnType);
    }

    // Один ответ памяти: метка, признак «запись найдена» и фактическая форма
    // ответа — форма попадает в сообщение о падении как доказательство.
    private readonly record struct Answer(bool HasFoundFlag, bool Found, int Label, string Shape);

    // Спрашивает продуктовое предсказание. Аргументы подставляются по типам
    // параметров: память и запасной путь получают одну и ту же память (так её
    // вызывает и продуктовый базовый проход), строка — проект, число — текущий
    // предикат. Признак «записи нет» читается из любой из трёх разрешённых
    // форм: выходной параметр, возврат с признаком успеха, кортеж.
    private static Answer Ask((object Model, Type ModelType) memory, string project, int current)
    {
        var (method, target) = Find("Predict");
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];
        var byRef = new List<int>();

        for (int i = 0; i < parameters.Length; i++)
        {
            var type = parameters[i].ParameterType;
            if (type.IsByRef) { byRef.Add(i); args[i] = null; continue; }
            if (type == memory.ModelType) { args[i] = memory.Model; continue; }
            if (type == typeof(string)) { args[i] = project; continue; }
            if (type == typeof(int)) { args[i] = current; continue; }

            Assert.Fail($"у предсказания параметр {parameters[i].Name}:{type} — проверка не умеет его заполнить");
        }

        var returned = method.Invoke(target, args);

        bool hasFlag = false;
        bool found = true;
        int label = int.MinValue;
        bool labelRead = false;
        var shape = new List<string>();

        // Выходные параметры: логический — признак «найдено», числовой — метка.
        foreach (int i in byRef)
        {
            var element = parameters[i].ParameterType.GetElementType()!;
            if (element == typeof(bool))
            {
                hasFlag = true;
                found = (bool)args[i]!;
                shape.Add($"выходной признак {parameters[i].Name}={found}");
            }
            else if (element == typeof(int))
            {
                label = (int)args[i]!;
                labelRead = true;
                shape.Add($"выходная метка {parameters[i].Name}={label}");
            }
        }

        if (method.ReturnType == typeof(bool))
        {
            hasFlag = true;
            found = (bool)returned!;
            shape.Add($"возврат признака успеха={found}");
        }
        else if (method.ReturnType == typeof(int))
        {
            if (!labelRead) { label = (int)returned!; labelRead = true; }
            shape.Add($"возврат метки={label}");
        }
        else
        {
            // Кортеж: логическое поле — признак, числовое — метка.
            foreach (var field in method.ReturnType.GetFields())
            {
                var value = field.GetValue(returned);
                if (field.FieldType == typeof(bool))
                {
                    hasFlag = true;
                    found = (bool)value!;
                    shape.Add($"поле возврата {field.Name}={found}");
                }
                else if (field.FieldType == typeof(int))
                {
                    label = (int)value!;
                    labelRead = true;
                    shape.Add($"поле возврата {field.Name}={label}");
                }
            }
        }

        Assert.True(labelRead, $"из ответа предсказания не удалось прочитать метку; форма: {method}");

        // Значение вне области меток извлекателя (0..21) — тоже законный
        // признак «записи нет», и тогда отдельного флага не требуется.
        if (!hasFlag && (label < 0 || label > 21))
        {
            hasFlag = true;
            found = false;
            shape.Add("метка вне области 0..21 — это признак «записи нет»");
        }

        return new Answer(hasFlag, found, label, string.Join("; ", shape));
    }
}
