// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса FIX-P83 — чтение выражения-тела метода в наборе
// признаков предсказателя вида следующего метода. Исходный опыт — о методе,
// написанном одной строкой через стрелку (восемьдесят третий опыт, 1 ноября
// 2026); карточка кандидата лежит рядом с прогоном опыта.
//
// Что здесь проверяется по-человечески. Предсказатель отвечает на вопрос «какого
// вида будет следующий метод» так: он смотрит на набор признаков метода-вопроса,
// находит в выученных парах самые похожие методы и голосует видами их
// продолжений. Признаки метода — это сокращённые имена видов узлов в его теле.
// До переноса метод, написанный одной строкой через стрелку, не имел ни одного
// признака: код читал только блочное тело в фигурных скобках, а выражение-тело
// не читал вовсе. Пустой набор признаков имеет два последствия, и оба проверены
// здесь: такой метод оказывается идеально похожим на любое объявление вообще без
// тела — абстрактное, в интерфейсе, внешнее, — и максимально далёким от своего же
// блочного смыслового двойника, то есть от метода, который делает ровно то же
// самое, но в фигурных скобках.
//
// Главный случай. Асинхронный однострочник обязан получить признаки ожидания и
// вызова и вид «асинхронный», а не пустой набор и вид «простой». Именно этот
// случай падает на прежнем правиле и проходит на новом.
//
// Контроли, которые обязаны отвечать одинаково и до переноса, и после. Блочный
// метод читается как раньше. Настоящие объявления без тела по-прежнему не имеют
// признаков: у них действительно нет ни блока, ни выражения, и пустой набор для
// них честен.
//
// Зачем здесь проверка на неразличимость двух однострочников. Самая вероятная
// будущая поломка этого переноса — «выравнивание» двух строк чтения узлов ради
// красоты: у блочного тела читается поддерево БЕЗ самого узла, у выражения-тела —
// ВМЕСТЕ с самим выражением, и эта несимметричность несущая. Если её убрать,
// асинхронный однострочник схлопывается в тот же набор, что обычный, а обычный
// возвращается к пустому набору. Поэтому здесь отдельно утверждается, что наборы
// двух разных однострочников различны и что у обычного однострочника набор не
// пуст: эти два утверждения падают ровно на выровненной логике.
//
// Бюджет этой проверки. Разбор нескольких коротких исходников и один прогон
// обучения предсказателя на двадцати четырёх парах при продуктовом числе соседей
// пять. Ни одного обучения с учителем, ни одной пары предпочтений, ни одного
// прохода через модель.
//
// Чего это покрытие НЕ доказывает. Оно не доказывает, что выигрыш доходит до
// работающего продукта: обучение этого предсказателя в хранилище не вызывает ни
// одна продуктовая строка, только проверки, а без обучения продуктовый путь
// возвращает «нет ответа». Оно не доказывает поведение при настоящем проектном
// объёме и разнообразии форм методов, не измеряет время и память и ничего не
// говорит о правиле «оба набора пусты — расстояние ноль», которое перенос
// сознательно не трогает.

using SemanticCompiler.Prediction;

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class MethodKnnExpressionBodyLearningTests
{
    // Продуктовое число соседей предсказателя: SemanticCompilerFacade.cs строка 46.
    private const int ProductionNeighbours = 5;

    // Фактические учебные тексты восемьдесят третьего опыта. Вызовы R() и Calc()
    // нигде не определены: разборщик строит дерево синтаксиса, и извлечению
    // признаков это не мешает.
    private const string AsyncArrow = "class E1{async Task<int> M()=>await R();}";
    private const string AsyncBlockTwin = "class E2{async Task<int> M(){return await R();}}";
    private const string PlainArrow = "class E3{int M()=>Calc();}";
    private const string AbstractNoBody = "abstract class E4{abstract int M();}";
    private const string InterfaceDeclaration = "interface E5{int M();}";
    private const string ExternNoBody = "class E6{extern int M();}";

    // Главный случай: однострочник через стрелку обязан иметь признаки своего
    // выражения, а не пустой набор.
    [Fact]
    public void ArrowBodiedMethod_GetsFeaturesOfItsExpression_NotAnEmptySet()
    {
        var method = Single(AsyncArrow);

        Assert.Equal(["Await", "Call"], Sorted(method.KindBag));
        Assert.Equal("Async", MethodKnnPredictor.CanonicalType(method.KindBag));
    }

    // Та же правка на обычном однострочнике: один признак вызова, вид «смешанный».
    // Падает, если две строки чтения узлов выровнять между собой.
    [Fact]
    public void PlainArrowBodiedMethod_GetsTheCallFeature_NotAnEmptySet()
    {
        var method = Single(PlainArrow);

        Assert.Equal(["Call"], Sorted(method.KindBag));
        Assert.NotEmpty(method.KindBag);
    }

    // Два разных однострочника обязаны остаться различимыми. Это утверждение
    // падает ровно на выровненной логике: там оба дают один и тот же набор.
    [Fact]
    public void TwoDifferentArrowBodiedMethods_StayDistinguishable()
    {
        var asyncBag = Single(AsyncArrow).KindBag;
        var plainBag = Single(PlainArrow).KindBag;

        Assert.NotEqual(Sorted(asyncBag), Sorted(plainBag));
        Assert.Equal(0.5, MethodKnnPredictor.JaccardDistance(asyncBag, plainBag), precision: 9);
    }

    // Контроль: блочный метод читается ровно как раньше, перенос его не задел.
    [Fact]
    public void BlockBodiedMethod_IsReadExactlyAsBefore()
    {
        var method = Single(AsyncBlockTwin);

        Assert.Equal(["Await", "Call", "Ret"], Sorted(method.KindBag));
        Assert.Equal("Async", MethodKnnPredictor.CanonicalType(method.KindBag));
    }

    // Контроль: у настоящего объявления без тела признаков нет и быть не должно.
    [Theory]
    [InlineData(AbstractNoBody)]
    [InlineData(InterfaceDeclaration)]
    [InlineData(ExternNoBody)]
    public void DeclarationWithoutAnyBody_StaysWithoutFeatures(string source)
    {
        var method = Single(source);

        Assert.Empty(method.KindBag);
        Assert.Equal("Simple", MethodKnnPredictor.CanonicalType(method.KindBag));
    }

    // Первая половина двустороннего дефекта расстояния: однострочник больше не
    // лежит на максимальном расстоянии от своего же блочного двойника.
    [Fact]
    public void ArrowBodiedMethod_IsNoLongerMaximallyFarFromItsBlockTwin()
    {
        var distance = MethodKnnPredictor.JaccardDistance(
            Single(AsyncArrow).KindBag,
            Single(AsyncBlockTwin).KindBag);

        Assert.Equal(1.0 / 3.0, distance, precision: 9);
        Assert.True(distance < 1.0, "однострочник не должен быть максимально далёк от своего двойника");
    }

    // Вторая половина того же дефекта: однострочник больше не выглядит идеальным
    // совпадением с объявлением, у которого тела нет вообще.
    [Theory]
    [InlineData(AbstractNoBody)]
    [InlineData(InterfaceDeclaration)]
    [InlineData(ExternNoBody)]
    public void ArrowBodiedMethod_IsNoLongerAPerfectMatchForABodilessDeclaration(string source)
    {
        var distance = MethodKnnPredictor.JaccardDistance(
            Single(AsyncArrow).KindBag,
            Single(source).KindBag);

        Assert.Equal(1.0, distance, precision: 9);
    }

    // Продуктовая настройка: пять соседей и обучающий объём больше числа соседей.
    // На каждой выученной паре задаём вопрос её же методом-вопросом и ждём вид её
    // же продолжения. До переноса такой контроль давал 18 верных из 24, потому что
    // два однострочника с пустыми наборами портили голос; после переноса — 24 из 24.
    [Fact]
    public void AtProductionNeighbours_EveryLearnedPairAnswersItsOwnTarget()
    {
        var methods = new List<PackedMethod>();
        short fileId = 0;
        for (var copy = 1; copy <= 6; copy++)
        {
            foreach (var source in VolumeForms(copy))
            {
                var extracted = MethodExtractor.Extract(source, fileId++);
                Assert.True(extracted.IsSuccess);
                methods.AddRange(extracted.Value!);
            }
        }

        var predictor = new MethodKnnPredictor(k: ProductionNeighbours);
        predictor.Train(methods);
        Assert.Equal(24, predictor.TrainingSize);

        var correct = 0;
        for (var index = 0; index < methods.Count - 1; index++)
        {
            if (methods[index].FileId != methods[index + 1].FileId ||
                methods[index + 1].IndexInFile != methods[index].IndexInFile + 1)
            {
                continue;
            }

            var expected = MethodKnnPredictor.CanonicalType(methods[index + 1].KindBag);
            if (predictor.Predict(methods[index]) == expected)
                correct++;
        }

        Assert.Equal(24, correct);
    }

    // Четыре формы учебного файла опыта: однострочник с блочным двойником и оба
    // блочных, в асинхронном и обычном виде. Второй метод каждого файла блочный,
    // поэтому вид цели этим переносом не меняется.
    private static string[] VolumeForms(int copy) =>
    [
        $"class A{copy}{{async Task<int> M()=>await R();async Task<int> N(){{return await R();}}}}",
        $"class B{copy}{{async Task<int> M(){{return await R();}}async Task<int> N(){{return await R();}}}}",
        $"class C{copy}{{int M()=>Calc();int N(){{return Calc();}}}}",
        $"class D{copy}{{int M(){{return Calc();}}int N(){{return Calc();}}}}"
    ];

    // Разобрать исходник с одним методом и вернуть этот метод.
    private static PackedMethod Single(string source)
    {
        var extracted = MethodExtractor.Extract(source);
        Assert.True(extracted.IsSuccess, extracted.ErrorMessage);
        return Assert.Single(extracted.Value!);
    }

    // Набор признаков в устойчивом порядке: сам набор порядка не хранит.
    private static string[] Sorted(HashSet<string> bag) =>
        bag.OrderBy(kind => kind, StringComparer.Ordinal).ToArray();
}
