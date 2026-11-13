// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие инварианта «запрос без признаков не выбирает произвольную
// выученную корзину» (работа R32-DQ плана дополнений 99 из папки
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments, основание —
// тридцать седьмой опыт «запрос без признаков получает метку первого уложенного
// урока», P37-PRIMARY-01).
//
// Что здесь проверяется по-человечески. Маршрутизатор наборов данных запоминает
// уроки вида «такие признаки → такая корзина данных» и на запрос отдаёт метку
// ближайшего урока. Вопрос простой: что он должен ответить, если в запросе нет
// ни одного признака? Сравнивать не с чем — от пустого запроса все уроки
// одинаково далеки.
//
// Что было сломано. Он всё равно выдавал чью-то метку, и это была метка того
// урока, который положили в память первым. Расстояние Жаккара от пустого набора
// признаков до любой непустой учебной строки равно единице, поэтому все уроки
// оказывались «одинаково близкими», а отбор соседей использует устойчивую
// сортировку: при равных расстояниях она сохраняет порядок хранения. В итоге
// «ближайшим» становился просто первый уложенный урок. В опыте это видно прямо:
// на одних и тех же трёх уроках прямая укладка давала «CSharp», обратная —
// «Tests». Для вызывающей стороны это хуже, чем отказ: она получает вид
// осмысленного решения там, где никакого основания для решения не было.
//
// Как это доказывается. Два чистых маршрутизатора обучаются одними и теми же
// тремя уроками, но в прямом и обратном порядке укладки, при одном заданном
// соседе — ровно как в опыте. Сначала проверяются три осведомительных запроса на
// каждом плече: они обязаны читаться правильно и до переноса, и после. Это
// защита от ложной красноты: если бы проверка падала из-за необученной памяти, а
// не из-за пустого запроса, эти шесть утверждений упали бы первыми. И только
// затем проверяется сам пустой запрос.
//
// Бюджет этой проверки. Два обучения, шесть предъявленных уроков, восемь чтений
// ответа в главной проверке и одно чтение в контроле холодной памяти. Ни одного
// обучения с учителем, ни одной пары предпочтений, ни одного прохода через
// модель, ни одной записи на диск.
//
// Чего это покрытие НЕ доказывает. Заданное число соседей здесь ровно одно, как
// в опыте; при больших значениях на пустом запросе выбиралось бы несколько
// первых строк и включалось бы голосование — это не проверяется. Три урока
// попарно не пересекаются; дубликаты признаков, пересекающиеся наборы и память
// большего размера не проверяются. Обобщение не проверяется вовсе: каждый
// осведомительный запрос буквально совпадает с признаками одного урока, поэтому
// верный ответ здесь — точный возврат выученного, а не ответ на новый вход.
// Поведение при пустой ссылке вместо массива признаков эта проверка не трогает и
// ничего о нём не утверждает.

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class DatasetRouterLearningStateTests
{
    // Заданное число соседей — одно, ровно как в тридцать седьмом опыте.
    private const int ConfiguredNeighbours = 1;

    // Номера корзин берутся по имени из настоящего продуктового списка,
    // а не записываются числом: порядок имён принадлежит продукту.
    private static readonly int CSharpBucket =
        Array.IndexOf(AgentAlgorithms.PredicateDatasetRouter.DatasetNames, "CSharp");

    private static readonly int BugFixBucket =
        Array.IndexOf(AgentAlgorithms.PredicateDatasetRouter.DatasetNames, "BugFix");

    private static readonly int TestsBucket =
        Array.IndexOf(AgentAlgorithms.PredicateDatasetRouter.DatasetNames, "Tests");

    // Признаки трёх уроков опыта: по одному числу на урок, наборы не пересекаются.
    private static readonly int[] CSharpFeatures = [10];
    private static readonly int[] BugFixFeatures = [20];
    private static readonly int[] TestsFeatures = [30];

    // Запрос без единого признака — тот самый вход, вокруг которого весь перенос.
    private static readonly int[] NoFeatures = [];

    // ГЛАВНАЯ ПРОВЕРКА переноса. На старом правиле она падает на первом же
    // утверждении о пустом запросе: вместо отказа возвращается метка первого
    // уложенного урока, и у двух порядков укладки эта метка разная.
    [Fact]
    public void EmptyQuery_DoesNotDependOnLessonOrder()
    {
        // Первое плечо: те же три урока, уложенные в прямом порядке.
        var forwardOrder = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);
        forwardOrder.Train(
        [
            (CSharpFeatures, Bucket(CSharpBucket)),
            (BugFixFeatures, Bucket(BugFixBucket)),
            (TestsFeatures, Bucket(TestsBucket))
        ]);

        // Второе плечо: те же три урока, уложенные в обратном порядке.
        var reverseOrder = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);
        reverseOrder.Train(
        [
            (TestsFeatures, Bucket(TestsBucket)),
            (BugFixFeatures, Bucket(BugFixBucket)),
            (CSharpFeatures, Bucket(CSharpBucket))
        ]);

        // Шесть осведомительных чтений идут ПЕРВЫМИ: они доказывают, что память
        // действительно обучена и что перенос не задел обычные запросы. Если бы
        // учебного состояния не было, упали бы именно они, а не пустой запрос.
        Assert.Equal(Mask(CSharpBucket), Read(forwardOrder, CSharpFeatures));
        Assert.Equal(Mask(BugFixBucket), Read(forwardOrder, BugFixFeatures));
        Assert.Equal(Mask(TestsBucket), Read(forwardOrder, TestsFeatures));
        Assert.Equal(Mask(CSharpBucket), Read(reverseOrder, CSharpFeatures));
        Assert.Equal(Mask(BugFixBucket), Read(reverseOrder, BugFixFeatures));
        Assert.Equal(Mask(TestsBucket), Read(reverseOrder, TestsFeatures));

        // Сам инвариант: на запросе без признаков обученный маршрутизатор
        // отказывается выбирать корзину, и ответ одинаков при любой укладке.
        Assert.Equal(Mask(), Read(forwardOrder, NoFeatures));
        Assert.Equal(Mask(), Read(reverseOrder, NoFeatures));
    }

    // КОНТРОЛЬ ГРАНИЦЫ ДВУХ ВЕТОК. Пустой запрос к НЕобученному маршрутизатору
    // обязан по-прежнему получать прежнее умолчание «брать все корзины»: отказ
    // от выбора относится только к обученной памяти. Если бы новая проверка
    // встала выше ветки пустой памяти, этот контроль упал бы.
    [Fact]
    public void EmptyQueryOnEmptyMemory_KeepsTheUnchangedTakeEverythingDefault()
    {
        // Маршрутизатор без единого урока: учебной памяти нет вовсе.
        var untrained = new AgentAlgorithms.PredicateDatasetRouter(k: ConfiguredNeighbours);

        // Чтение ответа на запросе без признаков по холодной памяти.
        string answer = Read(untrained, NoFeatures);

        // Ожидание выписано буквально: истинны все корзины продуктового списка.
        Assert.Equal(new string('1', AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length), answer);
    }

    // Читает ответ маршрутизатора и переводит его в человеческую маску вида
    // «010000»: так в сообщении о расхождении сразу видно, какая корзина выбрана.
    private static string Read(AgentAlgorithms.PredicateDatasetRouter router, int[] queryPredicates)
    {
        // Настоящее продуктовое чтение обученной памяти.
        bool[] answer = router.Predict(queryPredicates);

        // Каждая корзина превращается в один знак: единица — выбрана, ноль — нет.
        return string.Concat(answer.Select(selected => selected ? '1' : '0'));
    }

    // Строит ожидаемую маску продуктовой длины: единицы стоят только у названных корзин.
    private static string Mask(params int[] selectedBuckets)
    {
        // Длина маски принадлежит продукту, а не проверке.
        var mask = new char[AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length];

        // По умолчанию ни одна корзина не выбрана.
        Array.Fill(mask, '0');

        // Отмечаем единицей только те корзины, которые назвал вызывающий случай.
        foreach (var bucket in selectedBuckets)
            mask[bucket] = '1';

        return new string(mask);
    }

    // Строит вектор меток урока продуктовой длины: истинны только названные корзины.
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
