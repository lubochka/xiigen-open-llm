// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие формы вектора и владения памятью в простом указателе
// векторов `BruteForceIndex` — родственный обход после двух ремонтов соседа
// (работа R11 плана ремонта 98 из папки docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments и её продолжение по владению памятью).
//
// Что здесь проверяется по-человечески. У договора поиска по векторам
// (`06.VectorSearch/IVectorIndex.cs`) две реализации. Указатель ближайших
// соседей ходит по графу, простой указатель сравнивает запрос со всеми
// хранимыми строками подряд. Обе принимают один и тот же вход: готовый массив
// чисел, опознаватель и необязательный словарь пометок. Вопросы те же самые:
// чьи это массив и словарь после приёма, и что происходит с вектором или
// запросом чужой длины.
//
// Что было сломано. Соседняя реализация получила обе защиты 29 и 30 октября, а
// эта не получила ни одной: её приём был одной строкой, которая клала в запись
// ровно тот массив и ровно тот словарь, которые дала вызывающая сторона, и не
// смотрела на длину вообще. Из этого следовали три беды.
// Первая: вызывающая сторона могла изменить свои же ячейки после приёма, и
// содержимое указателя менялось вместе с ними — ответ на прежний запрос
// становился другим без добавления, без переобучения и без перезагрузки, и
// никакого события, по которому это можно было бы заметить, не происходило.
// Вторая: в один указатель молча ложились строки разной длины.
// Третья, и самая тихая: общая мера косинуса (`06.VectorSearch/Distances.cs`,
// строка 13) идёт по длине ПЕРВОГО аргумента, а первый аргумент здесь —
// запрос. Поэтому запрос КОРОЧЕ хранимых строк считался по началу вектора и
// отвечал неверно без всякой ошибки, а запрос ДЛИННЕЕ падал выходом за границу
// массива уже внутри сравнения.
//
// Почему это был именно пропуск родственника, а не новое решение. Правило
// проекта о родственном обходе требует после добавления защиты одной службе
// обойти все однотипные, принимающие тот же вход (навык целостности проверок,
// раздел «Rule: branch-reachability», подраздел «Sibling audit»). Оба
// предыдущих ремонта этого не сделали, и проверка владения у соседа сама
// назвала пропущенного родственника в своих границах. Теперь один договор даёт
// одно обещание обеими реализациями.
//
// Как это доказывается. Маленькие указатели, собранные прямо здесь, с
// буквально выписанными числами. Сначала обычный ответ. Потом нападение со
// стороны входа: меняются те самые ячейки и те самые записи словаря, которые
// вызывающая сторона передала в приём. Между нападением и повторным чтением
// НЕТ ни добавления, ни перестройки — иначе проверка просто скрыла бы порчу.
// Ожидаемые значения выписаны буквально, и рядом идёт сравнение оценок по
// битам, чтобы сдвиг в последнем разряде не прошёл молча.
//
// Про пометки отдельно, и это главное отличие от соседа. У соседа пометки
// видны через штатное сохранение, и нападение на словарь доказывалось
// сравнением сохранённых байтов. Здесь такой двери нет вовсе: `Save` и `Load`
// у простого указателя бросают `NotImplementedException` по замыслу, а ответ на
// запрос возвращает только опознаватель и оценку. Поэтому пометки читаются
// прямо из закрытого хранилища через отражение. Выбор назван честно: это не
// продуктовая дверь, а чтение внутреннего состояния, и оно доказывает ровно то,
// что у этого класса вообще наблюдаемо, потому что иной поверхности у пометок
// здесь нет. Чувствительность этого способа подтверждается контролем подмены:
// со снятой копией словаря проверка обязана упасть.
//
// Бюджет этой проверки. Ноль обучений, ноль пар предпочтений, ноль обращений к
// модели, ноль обращений к диску. Пятнадцать добавлений всего и двадцать
// запросов на все семь случаев.
//
// Чего это покрытие НЕ доказывает. Оно не закрывает не-числа и бесконечности:
// вектор нужной длины из таких значений по-прежнему принимается. Оно не
// доказывает отката на середине добавления: доказано лишь, что отказ приходит
// ДО единственной изменяющей строки. Оно ничего не говорит о работе из
// нескольких потоков: у класса такого договора нет. Оно не проверяет
// согласованность сохранённого указателя — сохранения у этого класса нет.
// И оно не измеряет цену копирования.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using VectorSearch;

public sealed class NativeBruteForceShapeAndOwnershipTests
{
    // ── Неверная длина вектора: отказ до любого изменения состояния ──────────

    [Fact]
    public void InvalidVectorLengthIsRejectedBeforeAnyStateChange()
    {
        // Указатель с принятой размерностью два: первый вектор её и задал.
        var index = new BruteForceIndex();
        index.Add([1f, 0f], "первая");
        index.Add([0f, 1f], "вторая");
        Assert.Equal(2, index.Count);

        // Эталон ответа снимается ДО вмешательства, в двух видах сразу:
        // человекочитаемом и побитовом.
        var readableBefore = Answer(index, [1f, 0f]);
        Assert.Equal("первая:1;вторая:0", readableBefore);
        var exactBefore = ExactAnswer(index, [1f, 0f]);

        // Вектор длиннее принятой размерности отвергается с названной причиной.
        var longer = Assert.Throws<ArgumentException>(() => index.Add([1f, 0f, 0f], "лишняя"));
        Assert.Contains("VECTOR_DIMENSION_MISMATCH: expected 2, actual 3", longer.Message);

        // Вектор короче принятой размерности отвергается там же.
        var shorter = Assert.Throws<ArgumentException>(() => index.Add([1f], "короткая"));
        Assert.Contains("VECTOR_DIMENSION_MISMATCH: expected 2, actual 1", shorter.Message);

        // Счётчик не сдвинулся: это и есть доказательство «до изменения
        // состояния» числом, а не рассуждением.
        Assert.Equal(2, index.Count);

        // Ни один отвергнутый опознаватель внутрь не попал: спрашиваем с запасом
        // по числу ответов, чтобы увидеть весь указатель целиком.
        var everything = index.Search([1f, 0f], topK: 100).Select(hit => hit.id).ToArray();
        Assert.Equal(["первая", "вторая"], everything);

        // Те же два чтения после отказа: между отказом и ними не было ни
        // добавления, ни перестройки указателя.
        Assert.Equal(readableBefore, Answer(index, [1f, 0f]));
        Assert.Equal(exactBefore, ExactAnswer(index, [1f, 0f]));
    }

    // ── Пустой и отсутствующий вектор: отказ даже самому первому ─────────────

    [Fact]
    public void EmptyOrMissingVectorIsRejectedEvenAsTheVeryFirstOne()
    {
        // Пустой указатель: принятой размерности ещё нет.
        var index = new BruteForceIndex();

        // Отсутствующий массив — нарушение договора вызова, а не ответ.
        Assert.Throws<ArgumentNullException>(() => index.Add(null!, "отсутствует"));
        Assert.Equal(0, index.Count);

        // Пустой вектор первым зафиксировал бы размерность ноль и запер бы
        // указатель навсегда, поэтому он отвергается и здесь.
        var rejection = Assert.Throws<ArgumentException>(() => index.Add([], "пустая"));
        Assert.Contains("VECTOR_EMPTY", rejection.Message);
        Assert.Equal(0, index.Count);

        // Отказ не испортил указатель: обычный первый вектор по-прежнему принят,
        // и именно он задаёт принятую размерность.
        index.Add([1f, 0f], "первая");
        Assert.Equal(1, index.Count);
        Assert.Equal("первая", index.Search([1f, 0f], topK: 1)[0].id);
    }

    // ── Запрос другой длины: отказ до первого сравнения ──────────────────────

    [Fact]
    public void SearchRejectsQueryOfAnotherLength()
    {
        // Пустой указатель отвечает пустым списком на запрос любой длины:
        // прежнее поведение сохранено, ранний возврат стоит раньше проверки.
        Assert.Empty(new BruteForceIndex().Search([1f, 0f, 0f], topK: 5));

        // Указатель с принятой размерностью два.
        var index = new BruteForceIndex();
        index.Add([1f, 0f], "первая");
        index.Add([0f, 1f], "вторая");

        // Запрос короче стоит первым намеренно: это самый тихий случай. Раньше
        // он считался по началу вектора и отвечал «первая» с оценкой 1, не
        // подавая никакого знака, поэтому со снятой защитой здесь не будет
        // вообще никакого исключения — и это видно прямо в журнале контроля.
        var shorter = Assert.Throws<ArgumentException>(() => index.Search([1f], topK: 1));
        Assert.Contains("QUERY_DIMENSION_MISMATCH: expected 2, actual 1", shorter.Message);

        // Запрос длиннее раньше падал выходом за границу массива уже внутри
        // сравнения; теперь он отвергается до первого сравнения.
        var longer = Assert.Throws<ArgumentException>(() => index.Search([1f, 0f, 0f], topK: 1));
        Assert.Contains("QUERY_DIMENSION_MISMATCH: expected 2, actual 3", longer.Message);

        // Отсутствующий запрос отвергается там же, до первого сравнения.
        Assert.Throws<ArgumentNullException>(() => index.Search(null!, 1));

        // Запрос нужной длины отвечает по-прежнему.
        Assert.Equal("первая", index.Search([1f, 0.1f], topK: 1)[0].id);
    }

    // ── Нападение на массив со стороны входа ─────────────────────────────────

    [Fact]
    public void StoredVectorKeepsItsOwnMemory_WhenCallerChangesTheSameArray()
    {
        // Массив вызывающей стороны: именно его ячейки потом и будут менять.
        var callerVector = new[] { 1f, 0f };

        // Две строки: первая совпадает с запросом, вторая заметно в стороне,
        // поэтому порядок ответа однозначен и устойчив.
        var index = new BruteForceIndex();
        index.Add(callerVector, "совпадает");
        index.Add([0.6f, 0.8f], "в-стороне");

        // Чтение первое: обычный ответ до любого нападения. Ожидание выписано
        // буквально — сначала точное совпадение с оценкой 1, затем дальняя
        // строка с оценкой 0,6.
        var before = Answer(index, [1f, 0f]);
        Assert.Equal("совпадает:1;в-стороне:0.6", before);
        var exactBefore = ExactAnswer(index, [1f, 0f]);

        // Нападение со стороны входа: вызывающая сторона разворачивает свой
        // массив в другую сторону, прямо в тех же ячейках.
        callerVector[0] = 0f;
        callerVector[1] = 1f;

        // Чтение второе: между нападением и этим чтением не было ни добавления,
        // ни перестройки указателя.
        var after = Answer(index, [1f, 0f]);

        // Ответ обязан быть прежним и по виду, и по битам. При общем массиве
        // первая строка стала бы перпендикулярной запросу, её оценка упала бы
        // с 1 до 0, и первой оказалась бы «в-стороне» — то есть сменились бы и
        // порядок, и числа.
        Assert.Equal(before, after);
        Assert.Equal("совпадает:1;в-стороне:0.6", after);
        Assert.Equal(exactBefore, ExactAnswer(index, [1f, 0f]));

        // И прямо в хранилище лежит уже не тот массив, который держит
        // вызывающая сторона.
        Assert.NotSame(callerVector, StoredEntry(index, 0).Vector);
        Assert.Equal([1f, 0f], StoredEntry(index, 0).Vector);
    }

    // ── Нападение на словарь пометок со стороны входа ────────────────────────

    [Fact]
    public void StoredMetadataKeepsItsOwnValues_WhenCallerChangesTheSameDictionary()
    {
        // Словарь вызывающей стороны со своим сравнивателем ключей: его и будут
        // менять после приёма. Сравниватель выбран нечувствительным к регистру
        // намеренно — так видно, что выбор вызывающей стороны уехал вместе с
        // копией, а не заменился сравнивателем по умолчанию.
        var callerMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["source"] = "original",
            ["lock"] = "original"
        };

        var index = new BruteForceIndex();
        index.Add([1f, 0f], "строка", callerMetadata);

        // Единственная дверь к пометкам у этого класса — закрытое хранилище:
        // ответ на запрос их не отдаёт, а сохранение отказывает по замыслу.
        var stored = StoredEntry(index, 0).Metadata;

        // Значения на месте до нападения.
        Assert.Equal("original", stored["source"]);
        Assert.Equal("original", stored["lock"]);

        // Сравниватель ключей вызывающей стороны уехал вместе с копией: поиск
        // по ключу в другом регистре находит ту же запись.
        Assert.Same(StringComparer.OrdinalIgnoreCase, stored.Comparer);
        Assert.Equal("original", stored["SOURCE"]);

        // Нападение со стороны входа: меняется прежнее значение и добавляется
        // ключ, которого при приёме не было.
        callerMetadata["source"] = "BROKEN";
        callerMetadata["added-key"] = "BROKEN";

        // Чтение после нападения: между ними не было ни добавления, ни
        // перестройки указателя.
        var storedAfter = StoredEntry(index, 0).Metadata;

        // Ни изменённое значение, ни новый ключ внутрь не прошли. Проверка
        // значения стоит первой намеренно: при общем словаре она покажет само
        // поехавшее значение — «original» против «BROKEN», — а не только тот
        // факт, что объект один и тот же.
        Assert.Equal("original", storedAfter["source"]);
        Assert.Equal("original", storedAfter["lock"]);
        Assert.Equal(2, storedAfter.Count);
        Assert.False(storedAfter.ContainsKey("added-key"));
        Assert.DoesNotContain(
            "BROKEN",
            string.Join(";", storedAfter.Select(pair => pair.Key + "=" + pair.Value)));

        // И только теперь — опорное сравнение: в хранилище лежит не тот самый
        // словарь, который держит вызывающая сторона.
        Assert.NotSame(callerMetadata, stored);

        // Отсутствующий словарь по-прежнему означает пустые пометки, а не отказ.
        index.Add([0f, 1f], "без-пометок");
        Assert.Empty(StoredEntry(index, 1).Metadata);
    }

    // ── Выдача: наружу не уходит ничего из памяти указателя ──────────────────

    [Fact]
    public void AnswerIsNotTheIndexOwnState_AndPublicSurfaceIsLocked()
    {
        // Маленький указатель из двух строк.
        var index = new BruteForceIndex();
        index.Add([1f, 0f], "первая");
        index.Add([0f, 1f], "вторая");

        // Обычный ответ: это список пар «опознаватель и оценка».
        var hits = index.Search([1f, 0f], topK: 2);
        Assert.Equal(2, hits.Count);

        // Нападение со стороны выхода: вызывающая сторона распоряжается
        // полученным списком как своим — меняет и опустошает его.
        hits[0] = ("СЛОМАНО", -1f);
        hits.Clear();

        // Повторное чтение без перестройки даёт прежний ответ: отданный список
        // был собственным списком вызывающей стороны, а не внутренним
        // состоянием указателя.
        Assert.Equal("первая:1;вторая:0", Answer(index, [1f, 0f]));

        // Два разных ответа не делят между собой один и тот же список.
        Assert.NotSame(index.Search([1f, 0f], topK: 2), index.Search([1f, 0f], topK: 2));

        // Больше у указателя наружу ничего и не выходит. Открытых членов ровно
        // четыре: приём, запрос, сохранение и загрузка. Приём ничего не
        // возвращает; запрос возвращает новый список из неизменяемых строк и
        // чисел; сохранение и загрузка отказывают по замыслу. Ни один из них не
        // отдаёт наружу ни хранимый массив, ни словарь пометок, ни внутренний
        // список — поэтому на выдаче закрывать нечего.
        var surface = typeof(BruteForceIndex)
            .GetMethods(BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        // Если у класса появится новый открытый член, эта проверка остановит
        // работу и потребует заново решить вопрос владения на выдаче.
        Assert.Equal(["Add", "Load", "Save", "Search"], surface);

        // Открытое свойство ровно одно, и это число: счётчик хранимых строк.
        var properties = typeof(BruteForceIndex)
            .GetProperties(BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.DeclaredOnly);
        Assert.Equal(["Count"], properties.Select(property => property.Name).ToArray());
        Assert.Equal(typeof(int), properties[0].PropertyType);

        // Открытых полей у класса нет вовсе: вся память спрятана.
        Assert.Empty(typeof(BruteForceIndex).GetFields(BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly));
    }

    // ── Сохранения нет по замыслу, поэтому проверять его нечему ──────────────

    [Fact]
    public void PersistenceRefusesByDesign_SoThereIsNoStoredShapeToCheck()
    {
        // У соседа проверка формы закрывала и загрузку с диска: сохранённый
        // указатель с разной длиной векторов наружу не выдаётся. Здесь такой
        // работы нет и быть не может — обе операции отказывают по замыслу, и
        // эта проверка закрепляет именно это, чтобы расхождение двух реализаций
        // было названным решением, а не забытым пробелом.
        var index = new BruteForceIndex();
        index.Add([1f, 0f], "первая");

        var save = Assert.Throws<NotImplementedException>(() => index.Save("не-используется.json"));
        Assert.Contains("in-memory only", save.Message);

        var load = Assert.Throws<NotImplementedException>(() => BruteForceIndex.Load("не-используется.json"));
        Assert.Contains("in-memory only", load.Message);

        // Отказ хранилища состояние не тронул.
        Assert.Equal(1, index.Count);
    }

    // ── Вспомогательное ──────────────────────────────────────────────────────

    // Один ответ указателя в виде строки: опознаватели и оценки. Оценка
    // печатается до пятого знака и всегда с точкой как разделителем, чтобы
    // запись читалась человеком и не зависела от языка машины.
    private static string Answer(BruteForceIndex index, float[] query) =>
        string.Join(
            ";",
            index.Search(query, topK: 3)
                .Select(hit => hit.id + ":" + hit.score.ToString(
                    "0.#####", System.Globalization.CultureInfo.InvariantCulture)));

    // Тот же ответ, но оценки в точных битах: сдвиг в последнем разряде, который
    // печать до пятого знака скрыла бы, здесь виден.
    private static string ExactAnswer(BruteForceIndex index, float[] query) =>
        string.Join(
            ";",
            index.Search(query, topK: 3)
                .Select(hit => $"{hit.id}:{BitConverter.SingleToInt32Bits(hit.score)}"));

    // Чтение закрытого хранилища: единственная дверь к пометкам у этого класса.
    // Это внутреннее состояние, а не продуктовая поверхность, и названо так
    // честно: у пометок здесь просто нет иного наблюдаемого выхода.
    private static IndexEntry StoredEntry(BruteForceIndex index, int position)
    {
        var field = typeof(BruteForceIndex).GetField(
            "_entries", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("BRUTE_FORCE_ENTRIES_FIELD_NOT_FOUND");
        var entries = (List<IndexEntry>)field.GetValue(index)!;
        return entries[position];
    }
}
