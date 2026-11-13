// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие инварианта «большинство голосов считается по фактически
// найденному числу соседей» (перенос тридцать пятого опыта, план переноса 100
// из папки docs/plans/replanning-possible-diffs/fixes-based-on-experiments).
//
// Что здесь проверяется по-человечески. Маршрутизатор наборов данных выбирает
// корзину данных большинством голосов ближайших соседей: он берёт несколько
// самых похожих выученных примеров и смотрит, за какие корзины они голосуют.
// Вопрос простой: от чего считается это «большинство» — от заданного числа
// соседей или от того, сколько соседей на самом деле нашлось в памяти.
//
// Что было сломано. Делителем было ЗАДАННОЕ число соседей. Когда в памяти
// лежало меньше примеров, чем задано, планка большинства оставалась высокой, а
// голосов физически не могло набраться столько. При заданных трёх соседях и
// одном выученном примере порог требовал не меньше двух голосов, а голос был
// ровно один: единственный верный выученный ответ не проходил, и маршрутизатор
// отвечал отказом — пустым набором корзин. Ответ в памяти был, его теряли при
// чтении.
//
// Как это доказывается. Те же четыре памяти, что и в опыте, при одном и том же
// заданном числе соседей три: пустая память, один урок, два встречных урока и
// три урока. Ожидаемые значения выписаны буквально, а не взяты из самого же
// ответа. Главный случай — память из одного урока: именно он падает на старом
// правиле и проходит на новом. Остальные три случая — контроли: они обязаны
// отвечать одинаково и до переноса, и после, иначе перенос задел бы больше,
// чем один делитель.
//
// Бюджет этой проверки. Четыре подгонки памяти и четыре чтения ответа, каждое
// на своём экземпляре маршрутизатора. Ни одного обучения с учителем, ни одной
// пары предпочтений, ни одного прохода через модель.
//
// Чего это покрытие НЕ доказывает. Проверено одно значение заданного числа
// соседей — три. У всех учебных строк здесь одинаковые признаки, поэтому все
// расстояния равны нулю и порядок соседей есть порядок их добавления; про
// память с разными признаками эта проверка не говорит ничего. Запрос чтения
// буквально совпадает с признаками учебных строк, то есть верный ответ здесь —
// точный возврат выученного, а не обобщение на новый вход. Цена изменения на
// больших памятях здесь не измеряется.

using LogicExtractor = SemanticCompiler.LogicExtractor;

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class DatasetRouterNeighbourVoteDivisorTests
{
    // Заданное число соседей во всех четырёх случаях одно и то же — три.
    private const int ConfiguredNeighbours = 3;

    // Признак запроса и всех учебных строк: ветвление. Один и тот же везде,
    // поэтому отбор соседей не вносит своей разницы в результат.
    private static readonly int[] Query = [LogicExtractor.BRANCH];

    // Номера корзин берутся по имени из настоящего продуктового списка,
    // а не записываются числом: порядок имён принадлежит продукту.
    private static readonly int CSharpBucket =
        Array.IndexOf(AgentAlgorithms.PredicateDatasetRouter.DatasetNames, "CSharp");

    private static readonly int BugFixBucket =
        Array.IndexOf(AgentAlgorithms.PredicateDatasetRouter.DatasetNames, "BugFix");

    // ГЛАВНЫЙ СЛУЧАЙ переноса. В памяти один урок, а задано три соседа.
    // Голос ровно один. На старом правиле планка была «больше трёх пополам»,
    // то есть не меньше двух голосов, и единственный выученный ответ терялся.
    // На новом правиле планка считается от одного найденного соседа, и ответ
    // возвращается.
    [Fact]
    public void OneLessonAndThreeConfiguredNeighbours_ReturnsTheSingleLearnedLabel()
    {
        // Маршрутизатор с заданными тремя соседями и памятью из одного урока.
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);
        router.Train([(Query, Bucket(CSharpBucket))]);

        // Чтение ответа на том же признаке, что и у учебной строки.
        bool[] answer = router.Predict(Query);

        // Ожидание выписано буквально: истинна ровно корзина «CSharp».
        Assert.Equal(Bucket(CSharpBucket), answer);
    }

    // КОНТРОЛЬ НИЧЬЕЙ. Два встречных урока: один голос за «CSharp», один за
    // «BugFix». Планка — «строго больше половины», половина от двух есть
    // единица, значит ни одна корзина не проходит. Отказ остаётся отказом и
    // после переноса: целочисленное деление не менялось.
    [Fact]
    public void TwoOpposingLessons_StayARefusalBecauseNeitherBucketHasAMajority()
    {
        // Память из двух уроков с одинаковыми признаками и разными корзинами.
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);
        router.Train(
        [
            (Query, Bucket(CSharpBucket)),
            (Query, Bucket(BugFixBucket))
        ]);

        // Чтение ответа на том же признаке.
        bool[] answer = router.Predict(Query);

        // Ожидание выписано буквально: ни одна корзина не выбрана.
        Assert.Equal(Bucket(), answer);
    }

    // КОНТРОЛЬ ВЕТКИ ПУСТОЙ ПАМЯТИ. Эта ветка переносом не трогалась вовсе:
    // до подсчёта голосов дело не доходит. Необученный маршрутизатор по-прежнему
    // отвечает умолчанием «брать все корзины», и это не предсказание.
    [Fact]
    public void EmptyMemory_StillAnswersWithTheUnchangedTakeEverythingDefault()
    {
        // Маршрутизатор без единого урока.
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);

        // Чтение ответа на пустой памяти.
        bool[] answer = router.Predict(Query);

        // Ожидание выписано буквально: истинны все корзины продуктового списка.
        Assert.Equal(
            Enumerable.Repeat(true, AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length).ToArray(),
            answer);
    }

    // КОНТРОЛЬ ПОЛНОЙ ПАМЯТИ. Уроков столько же, сколько задано соседей, значит
    // фактическое число равно заданному и делитель совпадает со старым. Ответ
    // обязан остаться прежним — иначе перенос задел бы и этот случай.
    [Fact]
    public void MemoryAsLargeAsConfiguredNeighbours_KeepsTheAnswerItGaveBeforeTheTransfer()
    {
        // Три урока: два за «CSharp», один за «BugFix».
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);
        router.Train(
        [
            (Query, Bucket(CSharpBucket)),
            (Query, Bucket(CSharpBucket)),
            (Query, Bucket(BugFixBucket))
        ]);

        // Чтение ответа на том же признаке.
        bool[] answer = router.Predict(Query);

        // Ожидание выписано буквально: два голоса против одного дают «CSharp».
        Assert.Equal(Bucket(CSharpBucket), answer);
    }

    // Строит вектор корзин продуктовой длины: истинны только названные номера.
    private static bool[] Bucket(params int[] trueBuckets)
    {
        // Длина вектора принадлежит продукту, а не проверке.
        var label = new bool[AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length];

        // Отмечаем истиной только те корзины, которые назвал вызывающий случай.
        foreach (var bucket in trueBuckets)
            label[bucket] = true;

        return label;
    }
}
