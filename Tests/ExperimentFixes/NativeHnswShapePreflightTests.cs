// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие проверки формы вектора в штатном указателе ближайших
// соседей (работа R11 плана ремонта 98 из папки docs/plans/
// replanning-possible-diffs/fixes-based-on-experiments, находка §7 аудита 97).
//
// Что здесь проверяется по-человечески. Указатель соседей хранит обученные
// векторы и отвечает на запрос ближайшими из них. Все векторы одного указателя
// обязаны быть одной длины: сравнение идёт покомпонентно. Вопрос простой —
// что происходит, если на вход приходит вектор другой длины.
//
// Что было сломано. Метод добавления первыми четырьмя строками дописывал
// вектор, опознаватель и метаданные в три своих списка и только после этого
// звал генератор случайных чисел за уровнем. Проверки длины не было нигде.
// Поэтому чужеродная запись уже лежала в памяти указателя, счётчик уже вырос,
// выборка генератора уже была израсходована, а падение по выходу за границу
// массива случалось позже, при обходе — то есть уже поверх испорченного
// состояния. Для обученной памяти это тихая порча: вернуть прежнее состояние
// было нечем.
//
// Как это доказывается. Берётся опубликованный замороженный обученный материал
// двадцать первого опыта: восемь векторов размерности 64 с опознавателями
// u1..u8 и двенадцать векторов запроса той же размерности. Байты материала
// закреплены отпечатком прямо в этом файле, поэтому подмена входа видна сразу.
// Из этих восьми векторов строится ОДИН штатный образец восемью обычными
// добавлениями и сохраняется штатным сохранением. Это восстановление
// указателя, а не повтор обучения разборщика или вложений: ни одной подгонки
// словаря, ни одного обновления весов здесь нет. Дальше неверный и верный
// случаи получают по одной независимой штатной загрузке этого образца.
//
// Эталон ответов снимается ДО вмешательства: двенадцать запросов до неверного
// добавления и те же двенадцать после. Между ними нет ни повторной загрузки,
// ни перестройки, ни сохранения-восстановления — иначе проверка просто скрыла
// бы порчу.
//
// Бюджет этой проверки. Ноль подгонок разборщика и вложений, ноль обновлений
// весов; один образец из восьми добавлений; две независимые загрузки образца;
// двенадцать запросов до и двенадцать после для неверного случая плюс один
// поиск принадлежности для верного. Малые указатели на две-три составляющие в
// проверках формы запроса и загрузки обученного материала не используют и
// бюджет обученного материала не тратят.
//
// Чего это покрытие НЕ доказывает. Оно не проверяет не-числа и бесконечности:
// вектор из таких значений нужной длины здесь принимается, и это отдельный
// договор. Оно не доказывает, что указатель владеет переданным массивом:
// вызывающая сторона по-прежнему держит тот же массив. Оно не доказывает
// отката на середине добавления: доказано лишь, что отказ происходит ДО первой
// изменяющей строки, а не то, что прерывание в середине можно откатить.
// Согласованность точки входа и графа с числом узлов при загрузке не
// проверяется — проверены длины векторов и равенство числа строк в четырёх
// основных наборах.
//
// Пометка 01.11.2026 (исполнитель), девятая работа ремонта. Приведённая выше
// фраза — граница ПРЕЖНЕЙ работы R11, она сохранена как было. Сейчас эта
// граница закрыта: загрузка проверяет точку входа, уровни и соседей, и ниже в
// этом же файле добавлен раздел «Загрузка: точка входа, уровни и соседи» с
// семью случаями отказа и одним случаем приёмки настоящего сохранённого
// состояния. Что по-прежнему НЕ проверяется при загрузке: узел глубже верхнего
// уровня, ссылка узла на себя, повторяющиеся соседи, взаимность рёбер,
// достижимость всех узлов обходом, положительность числа соседей и ширин
// поиска, не-числа и бесконечности в сохранённых координатах, повторяющиеся
// опознаватели. Это отдельные договоры, а не часть этой работы.
//
// Пометка 01.11.2026, вторая (исполнитель), девятая работа ремонта — её
// пробелы, найденные аудитом после восемьдесят восьмого опыта (план 116 и
// разбор RF02/RF09 той же даты). Фраза выше «положительность числа соседей и
// ширин поиска не проверяется» сохранена как граница ПРЕЖНЕЙ работы, но
// частично закрыта: ширина ПОСТРОЕНИЯ теперь проверяется и при загрузке, и в
// конструкторе, потому что при нуле следующее добавление не находит ни одного
// соседа и новый узел остаётся отсоединённым от старого знания. Число соседей
// и ширина ПОИСКА по-прежнему не проверяются: у ширины поиска дефекта нет
// (публичный поиск берёт наибольшее из неё и запрошенного числа ответов), а
// числу соседей нужна своя политика допустимых значений.
//
// Второй закрытый пробел — оракул случая приёмки. Прежний случай
// `LoadAcceptsRealSavedStateWithTwoLevelsUnchanged` назван двухуровневым, но
// читает образец, который заново собирается восемью случайными добавлениями, и
// утверждает только количество восемь и одно само-попадание; ни точка входа, ни
// верхний уровень, ни граф не сравниваются, поэтому он прошёл бы и на
// одноуровневом графе. Этот случай оставлен как был, а рядом добавлен
// `LoadPreservesFrozenNavigationAndAnswers`, который грузит проверенные байты
// НАПРЯМУЮ, без перестроения, и сравнивает структурно точку входа, верхний
// уровень, граф, координаты, опознаватели и настройки, а затем двенадцать
// собственных ближайших ответов второго замороженного набора. Прежний ленивый
// образец новые случаи не трогают.
//
// Пометка 01.11.2026, третья (исполнитель), девятая работа ремонта — две её
// оставшиеся межузловые проверки (уточнённый договор ремонта 114 и
// опубликованный аудит границ исполнения 116 той же даты; читались версии
// опубликованной вершины, потому что копия договора на диске с ней расходится).
// Фраза первой пометки выше «что по-прежнему НЕ проверяется при загрузке: узел
// глубже верхнего уровня …» сохранена как граница ПРЕЖНЕЙ работы и теперь
// частично закрыта: добавлены две проверки — сосед обязан доживать до уровня
// ссылки, и объявленный верх обязан равняться глубине самого глубокого узла, а
// не только глубине точки входа. Третьей проверки договора — что точка входа
// принадлежит глобальному верхнему уровню — отдельной строкой нет намеренно: из
// равенства глубины точки входа объявленному верху и равенства ему же
// глобального максимума третье равенство следует, и отдельный код отказа был бы
// недостижим.
//
// Что из той же фразы НЕ закрыто и остаётся отдельными договорами: ссылка узла
// на себя (она законна и проверяется положительным контролем), повторяющиеся
// соседи, взаимность рёбер, достижимость всех узлов обходом, не-числа и
// бесконечности в сохранённых координатах, повторяющиеся опознаватели.
//
// Почему прежняя панель точности этот дефект не ловила — измерено по
// опубликованной улике аудита, а не предположено: обе испорченные копии
// прежняя загрузка принимала, и на обеих все двенадцать проверочных запросов
// возвращали ожидаемые опознаватели. Нулевой уровень этого состояния полный —
// каждый из восьми узлов связан со всеми остальными, — поэтому перебор нижнего
// уровня находит верный ответ независимо от повреждений наверху.
//
// Чего новые случаи НЕ доказывают. Родная квота приёмки плана — три добавления,
// три сохранения, шесть загрузок, двадцать два поиска и чтение свежим процессом
// с квитанцией — здесь НЕ выполнена: сборка и прогон площадки закрыты правами
// сессии, поэтому эта часть помечена как невыполненная, а не как пройденная.
// Чтение свежим процессом в постоянной проверке не делается вовсе: это работа
// родного хозяина опыта.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using VectorSearch;

public sealed class NativeHnswShapePreflightTests
{
    // Отпечатки замороженного материала, записанные до запуска. Если байты
    // поменяются, проверка остановится на них, а не выдаст тихо другой ответ.
    private const string FrozenIndexSha = "f9fc76519f366d13e81925e77c9e47966beb934bf948feea9d67726a29544c4f";
    private const string FrozenArmSha = "2bdcf0745f34f64bbbcab561a2b4c92b0450b786eccf83f69d39eabf3eacc3a1";

    // Второй замороженный набор запросов того же опыта: его двенадцать ответов
    // проверяет сохранность навигации ниже. Отпечаток закреплён так же, как у
    // двух соседних наборов.
    private const string FrozenArmBSha = "253eba7b0231d2164a776397ea6fe1ed9ab8f91cba5cb9aae6d4f9a4fe8d89a6";

    // Принятая размерность замороженного обученного материала.
    private const int FrozenDimensions = 64;

    // Один образец на весь прогон: восемь добавлений замороженных векторов и
    // одно штатное сохранение. Дальше каждый случай грузит его заново.
    private static readonly Lazy<string> NativeFixture = new(BuildNativeFixtureOnce);

    // ── Неверная форма: отказ до любого изменения состояния ──────────────────

    [Fact]
    public void InvalidVectorIsRejectedBeforeAnyStateChange()
    {
        // Независимая штатная загрузка образца номер один.
        var index = HnswIndex.Load(NativeFixture.Value);
        // Образец содержит ровно восемь обученных строк.
        Assert.Equal(8, index.Count);

        // Эталон ответов снимается ДО вмешательства: двенадцать запросов.
        var queries = FrozenQueryVectors();
        Assert.Equal(12, queries.Count);
        var answersBefore = queries.Select(q => Answer(index, q.Vector)).ToList();

        // Полный снимок состояния до вмешательства: он включает счётчик,
        // опознаватели, метаданные, граф, точку входа и верхний уровень.
        var stateBefore = SaveToBytes(index, "before");

        // Неверный случай опыта: обученная строка u1, усечённая с 64 до 63.
        var source = FrozenStoredVectors().Single(row => row.Id == "u1").Vector;
        var truncated = source.Take(63).ToArray();
        Assert.Equal(63, truncated.Length);

        // Отказ приходит нарушением договора вызова с названной причиной.
        var rejection = Assert.Throws<ArgumentException>(() => index.Add(truncated, "bad"));
        Assert.Contains("VECTOR_DIMENSION_MISMATCH: expected 64, actual 63", rejection.Message);

        // Счётчик не сдвинулся.
        Assert.Equal(8, index.Count);

        // Байт в байт тот же снимок: опознаватели, метаданные, граф, точка
        // входа и верхний уровень остались прежними. Новый опознаватель "bad"
        // в состоянии отсутствует.
        var stateAfter = SaveToBytes(index, "after");
        Assert.Equal(stateBefore, stateAfter);
        Assert.DoesNotContain("\"bad\"", System.Text.Encoding.UTF8.GetString(stateAfter));

        // Те же двенадцать запросов после вмешательства: между отказом и этим
        // чтением не было ни загрузки, ни перестройки, ни восстановления.
        var answersAfter = queries.Select(q => Answer(index, q.Vector)).ToList();
        Assert.Equal(answersBefore, answersAfter);
    }

    // ── Верная форма: принимается и находится ────────────────────────────────

    [Fact]
    public void ValidVectorWithNewIdIsAcceptedAndFound()
    {
        // Независимая штатная загрузка образца номер два.
        var index = HnswIndex.Load(NativeFixture.Value);
        Assert.Equal(8, index.Count);

        // Верный вектор нужной размерности из того же замороженного материала,
        // с новым опознавателем: среди восьми хранимых такого вектора нет.
        var admitted = FrozenQueryVectors().Single(q => q.Id == "e1").Vector;
        Assert.Equal(FrozenDimensions, admitted.Length);

        // Обычное добавление проходит.
        index.Add(admitted, "valid-64");
        Assert.Equal(9, index.Count);

        // Один поиск принадлежности: сам добавленный вектор возвращает себя.
        var hits = index.Search(admitted, topK: 1);
        Assert.Single(hits);
        Assert.Equal("valid-64", hits[0].id);
        Assert.Equal(1f, hits[0].score, precision: 5);
    }

    // ── Пустой вектор: отказ даже самому первому ─────────────────────────────

    [Fact]
    public void EmptyVectorIsRejectedEvenAsTheVeryFirstOne()
    {
        // Пустой указатель: принятой размерности ещё нет.
        var index = new HnswIndex();

        // Отсутствующий массив — тоже нарушение договора вызова, а не ответ.
        Assert.Throws<ArgumentNullException>(() => index.Add(null!, "missing"));
        Assert.Equal(0, index.Count);

        // Пустой вектор первым зафиксировал бы размерность ноль и запер бы
        // указатель навсегда, поэтому он отвергается и здесь.
        var rejection = Assert.Throws<ArgumentException>(() => index.Add([], "empty"));
        Assert.Contains("VECTOR_EMPTY", rejection.Message);
        Assert.Equal(0, index.Count);

        // Отказ не испортил указатель: обычный первый вектор по-прежнему принят,
        // и именно он задаёт принятую размерность.
        index.Add([1f, 0f], "first");
        Assert.Equal(1, index.Count);
        Assert.Equal("first", index.Search([1f, 0f], topK: 1)[0].id);
    }

    // ── Запрос другой длины ──────────────────────────────────────────────────

    [Fact]
    public void SearchRejectsQueryOfAnotherLength()
    {
        // Пустой указатель отвечает пустым списком на запрос любой длины:
        // прежнее поведение сохранено, ранний возврат стоит раньше проверки.
        Assert.Empty(new HnswIndex().Search([1f, 0f, 0f], topK: 5));

        // Указатель с принятой размерностью два.
        var index = new HnswIndex(m: 4, efConstruction: 10, efSearch: 10);
        index.Add([1f, 0f], "a");
        index.Add([0f, 1f], "b");

        // Запрос на три составляющие отвергается до первого сравнения.
        var rejection = Assert.Throws<ArgumentException>(() => index.Search([1f, 0f, 0f], topK: 1));
        Assert.Contains("QUERY_DIMENSION_MISMATCH: expected 2, actual 3", rejection.Message);

        // Отсутствующий запрос отвергается там же, до первого сравнения.
        Assert.Throws<ArgumentNullException>(() => index.Search(null!, 1));

        // Запрос нужной длины отвечает по-прежнему.
        Assert.Equal("a", index.Search([1f, 0.1f], topK: 1)[0].id);
    }

    // ── Загрузка: отсутствующий набор и сохранённый вектор без составляющих ──

    [Fact]
    public void LoadRejectsMissingCollectionAndEmptyStoredVector()
    {
        // Набор векторов отсутствует целиком: обязательного поля нет.
        var missing = WriteIndexJson(vectors: "null", ids: "[]", meta: "[]", graph: "[]");
        Assert.Contains(
            "INDEX_COLLECTION_MISSING",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(missing)).Message);

        // Сохранённый вектор без составляющих задал бы принятую размерность ноль
        // и запер бы указатель: ни один запрос такой длины не приходит.
        var empty = WriteIndexJson(vectors: "[[]]", ids: "[\"a\"]", meta: "[{}]", graph: "[[[]]]");
        Assert.Contains(
            "INDEX_VECTOR_EMPTY: stored vector 0",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(empty)).Message);

        // Отсутствующий сохранённый вектор отвергается там же, а не падением
        // по пустой ссылке при чтении его длины.
        var hole = WriteIndexJson(vectors: "[[1.0,0.0],null]", ids: "[\"a\",\"b\"]", meta: "[{},{}]", graph: "[[[1]],[[0]]]");
        Assert.Contains(
            "INDEX_VECTOR_EMPTY: stored vector 1",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(hole)).Message);
    }

    // ── Загрузка: разные длины сохранённых векторов ──────────────────────────

    [Fact]
    public void LoadRejectsStoredVectorsOfDifferentLength()
    {
        // Сохранённый указатель, где второй вектор на одну составляющую длиннее.
        var path = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0,0.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]");

        // Непригодный экземпляр наружу не выдаётся.
        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(path));
        Assert.Contains("INDEX_VECTOR_DIMENSION_MISMATCH", rejection.Message);
    }

    // ── Загрузка: разошедшиеся счётчики основных наборов ─────────────────────

    [Fact]
    public void LoadRejectsInconsistentCollectionCounts()
    {
        // Два вектора, но только один опознаватель: наборы разошлись.
        var path = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(path));
        Assert.Contains("INDEX_COUNT_MISMATCH", rejection.Message);
    }

    // ── Загрузка: точка входа, уровни и соседи ───────────────────────────────
    //
    // Девятая работа ремонта. До неё загрузка копировала точку входа, верхний
    // уровень и весь граф соседей без проверки, поэтому подменённый или обрезанный
    // файл становился пригодным указателем и ломался позже: выход точки входа за
    // число узлов — исключением по индексу при первом же обходе, сосед за числом
    // узлов — тем же исключением, но только когда запрос доходил до этого узла,
    // отсутствующий список уровней — невнятным обращением к пустой ссылке в
    // копирующем цикле, а два самых тихих состояния (точка входа −1 при
    // сохранённых векторах и узел без нулевого уровня) не давали исключения вовсе
    // и просто отвечали неверно. Каждый случай ниже назван своим кодом отказа при
    // загрузке; у каждого есть близнец с одним исправленным полем, который
    // по-прежнему грузится и отвечает — так видно, что отказ про поле, а не про
    // файл целиком.

    [Fact]
    public void LoadRejectsEntryPointOutsideStoredNodes()
    {
        // Два сохранённых узла, а точка входа называет седьмой.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]",
            entryPoint: 7);

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point 7 is not one of the 2 stored nodes.", rejection.Message);

        // Тот же файл с единственным исправленным полем грузится и отвечает.
        var repaired = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]",
            entryPoint: 0);
        Assert.Equal("a", HnswIndex.Load(repaired).Search([1f, 0f], topK: 1)[0].id);
    }

    [Fact]
    public void LoadRejectsMissingEntryPointOverStoredNodes()
    {
        // Самое тихое состояние: векторы сохранены, а точки входа нет. Прежняя
        // загрузка принимала такой файл, и дальше любой запрос получал пустой
        // список при непустом счётчике — уверенно неверный ответ без исключения.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]",
            entryPoint: -1);

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_ENTRY_POINT_OUT_OF_RANGE: entry point -1 is not one of the 2 stored nodes.", rejection.Message);
    }

    [Fact]
    public void LoadRejectsNavigationStateOnAnIndexWithNoVectors()
    {
        // Пустой набор векторов, но навигация утверждает нулевой узел. Прежняя
        // загрузка принимала это, и затем любой запрос получал жалобу на длину
        // запроса — сообщение не про настоящую причину.
        var broken = WriteIndexJson(vectors: "[]", ids: "[]", meta: "[]", graph: "[]", entryPoint: 0, maxLevel: 0);

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_EMPTY_STATE_INCONSISTENT", rejection.Message);

        // Настоящий пустой сохранённый указатель по-прежнему грузится и отвечает
        // пустым списком: эта работа пустой случай не запрещает.
        var empty = WriteIndexJson(vectors: "[]", ids: "[]", meta: "[]", graph: "[]", entryPoint: -1, maxLevel: -1);
        var loaded = HnswIndex.Load(empty);
        Assert.Equal(0, loaded.Count);
        Assert.Empty(loaded.Search([1f, 0f], topK: 3));
    }

    [Fact]
    public void LoadRejectsNodeWithoutItsListOfLevels()
    {
        // У первого узла нет списка уровней. Прежняя загрузка падала здесь
        // обращением к пустой ссылке в копирующем цикле, без названной причины.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[null,[[0]]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_GRAPH_NODE_MISSING: node 0 has no list of levels.", rejection.Message);
    }

    [Fact]
    public void LoadRejectsNodeWithoutLevelZero()
    {
        // У первого узла список уровней пуст. Прежняя загрузка принимала такой
        // файл, и узел становился бесплодным: обход через него никуда не ведёт.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[],[[0]]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_GRAPH_NODE_EMPTY: node 0 has no level zero", rejection.Message);
    }

    [Fact]
    public void LoadRejectsLevelWithoutItsListOfNeighbours()
    {
        // Уровень есть, а списка соседей на нём нет: то же обращение к пустой
        // ссылке в прежнем копирующем цикле.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[null],[[0]]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_GRAPH_LEVEL_MISSING: node 0 level 0 has no list of neighbours.", rejection.Message);
    }

    [Fact]
    public void LoadRejectsNeighbourOutsideStoredNodes()
    {
        // Первый узел ссылается на пятый при двух сохранённых. Прежняя загрузка
        // принимала это, а исключение по индексу приходило позже и только тогда,
        // когда запрос доходил до этой ссылки.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[5]],[[0]]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_NEIGHBOUR_OUT_OF_RANGE: node 0 level 0 points at node 5", rejection.Message);

        // Отрицательная ссылка отвергается тем же кодом.
        var negative = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[-1]],[[0]]]");
        Assert.Contains(
            "INDEX_NEIGHBOUR_OUT_OF_RANGE: node 0 level 0 points at node -1",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(negative)).Message);
    }

    [Fact]
    public void LoadRejectsTopLevelThatDisagreesWithTheEntryPoint()
    {
        // У точки входа один уровень, а верхний уровень в файле третий. Прежняя
        // загрузка принимала это, и спуск молча ничего не делал: обход начинался
        // с уровня, которого у точки входа нет.
        var broken = WriteIndexJson(
            vectors: "[[1.0,0.0],[0.0,1.0]]",
            ids: "[\"a\",\"b\"]",
            meta: "[{},{}]",
            graph: "[[[1]],[[0]]]",
            entryPoint: 0,
            maxLevel: 3);

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains("INDEX_MAX_LEVEL_MISMATCH: top level 3 disagrees with the 1 levels of entry point 0.", rejection.Message);
    }

    [Fact]
    public void LoadAcceptsRealSavedStateWithTwoLevelsUnchanged()
    {
        // Главная защита от ложного отказа: настоящее сохранённое состояние
        // двадцать первого опыта, собранное обычными добавлениями, имеет точку
        // входа пятого узла и верхний уровень первый при восьми узлах. Новые
        // проверки обязаны принять его без изменений.
        var index = HnswIndex.Load(NativeFixture.Value);
        Assert.Equal(8, index.Count);

        // Хранимый вектор находит себя: граф скопирован целиком и обход работает.
        var stored = FrozenStoredVectors().Single(row => row.Id == "u1").Vector;
        var hits = index.Search(stored, topK: 1);
        Assert.Single(hits);
        Assert.Equal("u1", hits[0].id);
        Assert.Equal(1f, hits[0].score, precision: 5);
    }

    // ── Загрузка: параметр продолжения, а не состояние ───────────────────────
    //
    // Девятая работа ремонта, второй пробел (план 116, разбор RF02/RF09 от
    // 1 ноября 2026). Прежняя девятая работа закрыла состояние навигации и НЕ
    // закрыла её параметры: ширина построения хранится обычным целым, поэтому
    // отсутствующее поле даёт ноль при разборе, и загрузка передавала этот ноль
    // конструктору как есть. При нулевой ширине следующее добавление не находит
    // ни одного соседа, новый узел остаётся несвязанным, а указатель считается
    // годным и отвечает. Три случая ниже — ровно три назначенные планом копии
    // проверенного начального состояния: поле убрано, ноль, минус один.

    [Fact]
    public void LoadRejectsMissingOrNonPositiveConstructionBeam()
    {
        // Поле убрано целиком: разбор оставляет ноль, и отказ называет этот ноль.
        var removed = WriteFrozenIndexWithBeam(null);
        Assert.Contains(
            "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam 0 is not positive",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(removed)).Message);

        // Поле записано нулём явно: тот же отказ, то же названное значение.
        var zero = WriteFrozenIndexWithBeam("0");
        Assert.Contains(
            "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam 0 is not positive",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(zero)).Message);

        // Отрицательное значение отвергается тем же кодом с его значением.
        var negative = WriteFrozenIndexWithBeam("-1");
        Assert.Contains(
            "INDEX_CONSTRUCTION_BEAM_INVALID: construction beam -1 is not positive",
            Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(negative)).Message);

        // Главная защита от ложного отказа: то же проверенное состояние с явной
        // шириной 64 по-прежнему грузится и отвечает. Отказ про поле, а не про файл.
        var untouched = CopyFrozenIndexToTemp();
        var index = HnswIndex.Load(untouched);
        Assert.Equal(8, index.Count);

        // И прямое создание экземпляра отвергает непригодную ширину там же, где
        // она попадает в экземпляр, — договор вызова, а не ответ.
        var direct = Assert.Throws<ArgumentOutOfRangeException>(
            () => new HnswIndex(m: 8, efConstruction: 0, efSearch: 32));
        Assert.Contains("INDEX_CONSTRUCTION_BEAM_INVALID", direct.Message);
    }

    // ── Загрузка: межузловые согласования уровней ────────────────────────────
    //
    // Девятая работа ремонта, две оставшиеся межузловые проверки (уточнённый
    // договор ремонта 114-post88-addressed-repair-plan-2026-11-01, раздел
    // согласованного графа; опубликованный аудит границ исполнения
    // 116-post89-execution-boundary-audit-2026-11-01, раздел про два отдельных
    // договора загрузки).
    //
    // Прежние проверки видели, что каждый сосед — один из сохранённых узлов, и
    // что объявленный верх совпадает с глубиной ТОЧКИ ВХОДА. Не видели двух
    // вещей: что сосед, названный на уровне l, сам доживает до уровня l, и что
    // объявленный верх равен глубине САМОГО ГЛУБОКОГО узла. Оба повреждения
    // тихие: обе ветви навигации спрашивают наличие уровня у текущего узла,
    // поэтому ребро на отсутствующий уровень не падает ошибкой, а укорачивает
    // спуск, а узел глубже объявленного верха вообще не посещается.
    //
    // Почему панель точности этого не поймала — измерено, а не предположено. В
    // опубликованной улике аудита обе испорченные копии прежняя загрузка
    // ПРИНИМАЛА, и на обеих все двенадцать проверочных запросов возвращали
    // ожидаемые опознаватели: нулевой уровень этого состояния полный, каждый из
    // восьми узлов связан со всеми остальными, поэтому перебор нижнего уровня
    // находит верный ответ независимо от повреждений наверху.

    [Fact]
    public void LoadRejectsUpperEdgeToNodeWithoutThatLevel()
    {
        // Ровно одно изменение проверенного состояния: верхний уровень точки
        // входа, который в исправном состоянии пуст, начинает ссылаться на узел
        // 0 — а у того есть только нулевой уровень.
        var broken = WriteFrozenIndexWithGraphEdit("[[4,0,1,2,3,6,7],[]]", "[[4,0,1,2,3,6,7],[0]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains(
            "INDEX_NEIGHBOUR_LEVEL_MISSING: node 5 level 1 points at node 0, which has only 1 levels and therefore no level 1",
            rejection.Message);

        // Главная защита от ложного отказа: то же состояние без этого изменения
        // по-прежнему грузится и отвечает. Отказ про ребро, а не про файл.
        var index = HnswIndex.Load(CopyFrozenIndexToTemp());
        Assert.Equal(8, index.Count);
        var stored = FrozenStoredVectors().Single(row => row.Id == "u1").Vector;
        Assert.Equal("u1", index.Search(stored, topK: 1)[0].id);
    }

    [Fact]
    public void LoadRejectsNodeDeeperThanTheDeclaredTopLevel()
    {
        // Ровно одно изменение: узел 0, не являющийся точкой входа, получает два
        // дополнительных пустых уровня, и его глубина становится второй при
        // объявленном верхнем уровне первом. Прежняя проверка этого не видела:
        // она сверяет верх только с глубиной точки входа, а та не изменилась.
        var broken = WriteFrozenIndexWithGraphEdit("[[1,2,3,4,5,6,7]]", "[[1,2,3,4,5,6,7],[],[]]");

        var rejection = Assert.Throws<InvalidOperationException>(() => HnswIndex.Load(broken));
        Assert.Contains(
            "INDEX_MAX_LEVEL_NOT_GLOBAL: top level 1 disagrees with the deepest stored node 0, whose top level is 2",
            rejection.Message);

        // Та же защита от ложного отказа на неизменённом состоянии.
        Assert.Equal(8, HnswIndex.Load(CopyFrozenIndexToTemp()).Count);
    }

    [Fact]
    public void LoadAcceptsEmptyAndSingleNodeIndexes()
    {
        // Положительные контроли, названные договором отдельно: пустой и
        // одноузловой указатели новые проверки принимать обязаны.
        //
        // Настоящий пустой сохранённый указатель: политика пустого значения —
        // точка входа минус один и верх минус один — сохранена без изменений.
        var empty = HnswIndex.Load(WriteIndexJson(vectors: "[]", ids: "[]", meta: "[]", graph: "[]", entryPoint: -1, maxLevel: -1));
        Assert.Equal(0, empty.Count);
        Assert.Empty(empty.Search([1f, 0f], topK: 3));

        // Одноузловой указатель с одним уровнем: глобальный максимум глубин ноль
        // равен объявленному верху, соседей нет вовсе.
        var flat = HnswIndex.Load(WriteIndexJson(vectors: "[[1.0,0.0]]", ids: "[\"a\"]", meta: "[{}]", graph: "[[[]]]", entryPoint: 0, maxLevel: 0));
        Assert.Equal(1, flat.Count);
        Assert.Equal("a", flat.Search([1f, 0f], topK: 1)[0].id);

        // Одноузловой указатель с двумя уровнями: единственный узел сам и есть
        // самый глубокий, поэтому равенство соблюдено и здесь.
        var tall = HnswIndex.Load(WriteIndexJson(vectors: "[[1.0,0.0]]", ids: "[\"a\"]", meta: "[{}]", graph: "[[[],[]]]", entryPoint: 0, maxLevel: 1));
        Assert.Equal(1, tall.Count);
        Assert.Equal("a", tall.Search([1f, 0f], topK: 1)[0].id);

        // Ссылка узла на себя на верхнем уровне законна: уровень у него есть.
        // Взаимность и отсутствие само-ссылок эта работа не требует.
        var self = HnswIndex.Load(WriteIndexJson(vectors: "[[1.0,0.0]]", ids: "[\"a\"]", meta: "[{}]", graph: "[[[],[0]]]", entryPoint: 0, maxLevel: 1));
        Assert.Equal(1, self.Count);
        Assert.Equal("a", self.Search([1f, 0f], topK: 1)[0].id);
    }

    // ── Загрузка: сохранность замороженной навигации и её ответов ────────────
    //
    // Девятая работа ремонта, третий пробел того же разбора: прежний случай
    // приёмки назван двухуровневым, но читает образец, который заново собирают
    // восемью случайными добавлениями, поэтому ни точка входа, ни верхний
    // уровень, ни граф не сравниваются вовсе, и случай проходит даже на
    // одноуровневом графе. Здесь сохранённое состояние грузится НАПРЯМУЮ, без
    // перестроения, и сравнивается структурно. Прежний ленивый образец этот
    // случай не трогает.

    [Fact]
    public void LoadPreservesFrozenNavigationAndAnswers()
    {
        // Замороженные байты копируются во временную папку: сам файл опыта не
        // редактируется ни при каких условиях.
        var frozenText = System.Text.Encoding.UTF8.GetString(ReadFrozen("S0/index.json", FrozenIndexSha));
        var source = CopyFrozenIndexToTemp();

        // Одна штатная загрузка и одно штатное сохранение: ни одного добавления.
        var index = HnswIndex.Load(source);
        var savedPath = Path.Combine(TempDirectory(), $"frozen-roundtrip-{Guid.NewGuid():N}.json");
        index.Save(savedPath);

        using var before = JsonDocument.Parse(frozenText);
        using var after = JsonDocument.Parse(File.ReadAllText(savedPath));

        // Исходные значения навигации и настроек выписаны буквально, а не взяты
        // из того же файла: подмена оснастки видна сразу.
        Assert.Equal(5, before.RootElement.GetProperty("EntryPoint").GetInt32());
        Assert.Equal(1, before.RootElement.GetProperty("MaxLevel").GetInt32());
        Assert.Equal(8, before.RootElement.GetProperty("M").GetInt32());
        Assert.Equal(64, before.RootElement.GetProperty("EfConstruction").GetInt32());
        Assert.Equal(32, before.RootElement.GetProperty("EfSearch").GetInt32());

        // Те же пять полей после загрузки и сохранения равны исходным.
        foreach (var field in new[] { "EntryPoint", "MaxLevel", "M", "EfConstruction", "EfSearch" })
            Assert.Equal(
                before.RootElement.GetProperty(field).GetInt32(),
                after.RootElement.GetProperty(field).GetInt32());

        // Опознаватели, координаты и весь граф соседей сравниваются структурно,
        // а не условием «количество осталось восемь».
        Assert.Equal(ReadIds(before), ReadIds(after));
        Assert.Equal(ReadVectors(before), ReadVectors(after));
        Assert.Equal(ReadGraph(before), ReadGraph(after));

        // Двенадцать собственных ближайших ответов второго замороженного набора:
        // ожидаемый опознаватель берётся из сохранённых наблюдений, а не из
        // текущего ответа указателя.
        var expected = FrozenQueryAnswersB();
        Assert.Equal(12, expected.Count);
        foreach (var (id, query, expectedFirst) in expected)
        {
            var hits = index.Search(query, topK: 1);
            Assert.Single(hits);
            Assert.Equal(expectedFirst, hits[0].id);
        }
    }

    // ── Продолженное добавление поверх восстановленного состояния ───────────
    //
    // Отдельный случай, как требует план: он проверяет другой механизм, чем
    // сохранность замороженной навигации, и поэтому не сливается с ним.

    [Fact]
    public void SaveLoadPreservesFurtherNativeAdd()
    {
        // Небольшой указатель назначенной планом настройки.
        var index = new HnswIndex(m: 4, efConstruction: 20, efSearch: 10);

        // До добавлений оба запроса не находят ничего.
        Assert.Empty(index.Search([1f, 0f, 0f], topK: 1));
        Assert.Empty(index.Search([0f, 1f, 0f], topK: 1));

        // Два обычных добавления и два само-попадания.
        index.Add([1f, 0f, 0f], "a");
        index.Add([0f, 1f, 0f], "b");
        Assert.Equal("a", index.Search([1f, 0f, 0f], topK: 1)[0].id);
        Assert.Equal("b", index.Search([0f, 1f, 0f], topK: 1)[0].id);

        // Сохранение и штатная загрузка: состояние переживает круг.
        var first = Path.Combine(TempDirectory(), $"further-first-{Guid.NewGuid():N}.json");
        index.Save(first);
        var restored = HnswIndex.Load(first);

        // Продолженное добавление поверх восстановленного состояния.
        restored.Add([0f, 0f, 1f], "c");
        Assert.Equal(3, restored.Count);

        // Все три само-попадания верны: старое знание и новое связаны.
        Assert.Equal("a", restored.Search([1f, 0f, 0f], topK: 1)[0].id);
        Assert.Equal("b", restored.Search([0f, 1f, 0f], topK: 1)[0].id);
        Assert.Equal("c", restored.Search([0f, 0f, 1f], topK: 1)[0].id);

        // Второе сохранение: продолженное состояние тоже записывается штатно.
        var second = Path.Combine(TempDirectory(), $"further-second-{Guid.NewGuid():N}.json");
        restored.Save(second);
        Assert.True(new FileInfo(second).Length > 0);
    }

    // ── Вспомогательное ──────────────────────────────────────────────────────

    // Один ответ указателя в виде строки: опознаватели и точные биты оценок.
    // Сравнение идёт по битам, чтобы сдвиг в последнем разряде не прошёл молча.
    private static string Answer(HnswIndex index, float[] query) =>
        string.Join(
            ";",
            index.Search(query, topK: 3)
                .Select(hit => $"{hit.id}:{BitConverter.SingleToInt32Bits(hit.score)}"));

    // Полный снимок состояния через штатное сохранение.
    private static byte[] SaveToBytes(HnswIndex index, string label)
    {
        var path = Path.Combine(TempDirectory(), $"state-{label}-{Guid.NewGuid():N}.json");
        index.Save(path);
        return File.ReadAllBytes(path);
    }

    // Восемь замороженных обученных векторов двадцать первого опыта.
    private static List<(string Id, float[] Vector)> FrozenStoredVectors()
    {
        using var document = JsonDocument.Parse(ReadFrozen("S0/index.json", FrozenIndexSha));
        var root = document.RootElement;
        var ids = root.GetProperty("Ids").EnumerateArray().Select(id => id.GetString()!).ToList();
        var vectors = root.GetProperty("Vectors").EnumerateArray()
            .Select(vector => vector.EnumerateArray().Select(value => value.GetSingle()).ToArray())
            .ToList();
        // Материал обязан быть тем самым: восемь строк размерности 64.
        Assert.Equal(8, ids.Count);
        Assert.Equal(8, vectors.Count);
        Assert.All(vectors, vector => Assert.Equal(FrozenDimensions, vector.Length));
        return ids.Zip(vectors, (id, vector) => (id, vector)).ToList();
    }

    // Двенадцать замороженных векторов запроса того же опыта.
    private static List<(string Id, float[] Vector)> FrozenQueryVectors()
    {
        using var document = JsonDocument.Parse(ReadFrozen("A.arm.json", FrozenArmSha));
        var rows = document.RootElement.GetProperty("Observations").EnumerateArray()
            .Select(observation => (
                Id: observation.GetProperty("Id").GetString()!,
                Vector: observation.GetProperty("QueryVector").EnumerateArray()
                    .Select(value => value.GetSingle()).ToArray()))
            .ToList();
        Assert.Equal(12, rows.Count);
        Assert.All(rows, row => Assert.Equal(FrozenDimensions, row.Vector.Length));
        return rows;
    }

    // Копия замороженного начального состояния во временной папке. Сам файл
    // опыта не редактируется: все случаи загрузки читают копию.
    private static string CopyFrozenIndexToTemp()
    {
        var path = Path.Combine(TempDirectory(), $"frozen-copy-{Guid.NewGuid():N}.json");
        File.WriteAllBytes(path, ReadFrozen("S0/index.json", FrozenIndexSha));
        return path;
    }

    // Копия того же состояния с изменённым ровно одним полем — шириной
    // построения. Пустое значение означает «поле убрано целиком». Остальные
    // поля не трогаются, поэтому отказ относится к этому полю, а не к файлу.
    private static string WriteFrozenIndexWithBeam(string? beamLiteral)
    {
        var text = System.Text.Encoding.UTF8.GetString(ReadFrozen("S0/index.json", FrozenIndexSha));

        // Образец поля выписан буквально. Если формат сохранения когда-нибудь
        // изменится, эта проверка упадёт здесь — вместо того чтобы молча
        // проверять неизменённый файл и выдавать это за пройденный случай.
        const string marker = "\"EfConstruction\":64";
        Assert.Contains(marker, text);

        var changed = beamLiteral is null
            // Поле вместе со своей запятой удаляется: при разборе остаётся ноль.
            ? text.Replace(marker + ",", string.Empty)
            // Либо значение заменяется на назначенное случаем.
            : text.Replace(marker, "\"EfConstruction\":" + beamLiteral);

        // Замена обязана была произойти: иначе случай ничего не проверяет.
        Assert.NotEqual(text, changed);

        var path = Path.Combine(TempDirectory(), $"frozen-beam-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, changed);
        return path;
    }

    // Копия того же состояния с изменённым ровно одним участком графа. Образец
    // текста выписан буквально и проверен на уникальность: если формат
    // сохранения или сам замороженный граф когда-нибудь изменится, случай
    // упадёт здесь — вместо того чтобы молча проверять неизменённый файл и
    // выдавать это за пройденный случай.
    private static string WriteFrozenIndexWithGraphEdit(string marker, string replacement)
    {
        var text = System.Text.Encoding.UTF8.GetString(ReadFrozen("S0/index.json", FrozenIndexSha));

        // Образец обязан быть и обязан быть единственным: иначе замена попала бы
        // не туда, и случай проверял бы не то повреждение, которое назван.
        Assert.Contains(marker, text);
        Assert.Equal(1, CountOccurrences(text, marker));

        var changed = text.Replace(marker, replacement);
        // Замена обязана была произойти: иначе случай ничего не проверяет.
        Assert.NotEqual(text, changed);

        var path = Path.Combine(TempDirectory(), $"frozen-graph-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, changed);
        return path;
    }

    // Сколько раз образец встречается в тексте: нужен для проверки уникальности
    // образца выше, а не для самой замены.
    private static int CountOccurrences(string text, string marker)
    {
        int count = 0;
        int at = text.IndexOf(marker, StringComparison.Ordinal);
        while (at >= 0)
        {
            count++;
            at = text.IndexOf(marker, at + marker.Length, StringComparison.Ordinal);
        }
        return count;
    }

    // Двенадцать запросов второго замороженного набора вместе с ожидаемым
    // первым попаданием, взятым из сохранённых наблюдений того же опыта.
    private static List<(string Id, float[] Query, string ExpectedFirstId)> FrozenQueryAnswersB()
    {
        using var document = JsonDocument.Parse(ReadFrozen("B.arm.json", FrozenArmBSha));
        var rows = document.RootElement.GetProperty("Observations").EnumerateArray()
            .Select(observation => (
                Id: observation.GetProperty("Id").GetString()!,
                Query: observation.GetProperty("QueryVector").EnumerateArray()
                    .Select(value => value.GetSingle()).ToArray(),
                ExpectedFirstId: observation.GetProperty("Hits")[0].GetProperty("Id").GetString()!))
            .ToList();

        // Материал обязан быть тем самым: двенадцать строк нужной размерности.
        Assert.All(rows, row => Assert.Equal(FrozenDimensions, row.Query.Length));
        return rows;
    }

    // Опознаватели сохранённого состояния в их исходном порядке.
    private static List<string> ReadIds(JsonDocument document) =>
        document.RootElement.GetProperty("Ids").EnumerateArray()
            .Select(id => id.GetString()!)
            .ToList();

    // Координаты сохранённого состояния построчно.
    private static List<List<float>> ReadVectors(JsonDocument document) =>
        document.RootElement.GetProperty("Vectors").EnumerateArray()
            .Select(vector => vector.EnumerateArray().Select(value => value.GetSingle()).ToList())
            .ToList();

    // Весь граф соседей: узел, уровень, список соседей — в исходном порядке.
    private static List<List<List<int>>> ReadGraph(JsonDocument document) =>
        document.RootElement.GetProperty("Graph").EnumerateArray()
            .Select(node => node.EnumerateArray()
                .Select(level => level.EnumerateArray().Select(value => value.GetInt32()).ToList())
                .ToList())
            .ToList();

    // Чтение замороженного файла с проверкой отпечатка: подмена входа видна сразу.
    private static byte[] ReadFrozen(string relativePath, string expectedSha)
    {
        var full = Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "P2101", "results", "attempt-0001",
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var bytes = File.ReadAllBytes(full);
        var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        Assert.Equal(expectedSha, actual);
        return bytes;
    }

    // Один образец: восемь обычных добавлений замороженных векторов и одно
    // штатное сохранение. Ни словарь, ни веса здесь не трогаются.
    private static string BuildNativeFixtureOnce()
    {
        var index = new HnswIndex(m: 8, efConstruction: 64, efSearch: 32);
        foreach (var (id, vector) in FrozenStoredVectors())
            index.Add(vector, id);

        var path = Path.Combine(TempDirectory(), "native-fixture.json");
        index.Save(path);
        return path;
    }

    // Небольшой сохранённый указатель, собранный буквально, для случаев загрузки.
    // Точка входа и верхний уровень по умолчанию ноль и ноль — ровно то, что было
    // у прежних случаев этого файла, поэтому их вызовы не поменялись. Девятая
    // работа ремонта добавила возможность задать эти два поля, потому что именно
    // они и проверяются.
    private static string WriteIndexJson(
        string vectors,
        string ids,
        string meta,
        string graph,
        int entryPoint = 0,
        int maxLevel = 0)
    {
        var path = Path.Combine(TempDirectory(), $"saved-{Guid.NewGuid():N}.json");
        File.WriteAllText(
            path,
            $"{{\"Vectors\":{vectors},\"Ids\":{ids},\"Meta\":{meta},\"Graph\":{graph}," +
            $"\"EntryPoint\":{entryPoint},\"MaxLevel\":{maxLevel}," +
            "\"M\":4,\"EfConstruction\":10,\"EfSearch\":10}");
        return path;
    }

    // Отдельная временная папка прогона: в хранилище эта проверка ничего не пишет.
    private static readonly Lazy<string> RunDirectory = new(() =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"xiigen-r11-hnsw-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    });

    private static string TempDirectory() => RunDirectory.Value;

    // Поднимается от папки сборки до корня хранилища, чтобы пути не зависели
    // от места запуска.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("NATIVE_HNSW_SHAPE_PREFLIGHT_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }
}
