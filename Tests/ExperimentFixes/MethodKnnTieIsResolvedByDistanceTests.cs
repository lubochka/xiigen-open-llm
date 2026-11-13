// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка переноса FIX-KNN-TIE-01: ровный голос методного предсказателя
// разрешается расстоянием до ближайшего соседа, а не написанием метки.
//
// Что было неверно. Голос считал только число голосов, а при равенстве решало
// порядковое сравнение имени метки. Боевой маршрут создаёт предсказатель с пятью
// соседями (`12.SemanticCompiler/SemanticCompilerFacade.cs`, строка 46) и
// отвечает начиная с четырёх обученных пар (строка 370). На наборе «по одной
// паре на метку» голос выходит РОВНЫМ — по одному голосу на метку, — и побеждает
// первая по алфавиту. Измерено восемьдесят пятым опытом на боевой настройке:
// все четыре экзаменационных запроса получили `ErrorHandling`, хотя ближайшая
// пара у них разная, и старая точность метки упала с четырёх из четырёх при
// одном соседе до НУЛЯ из четырёх при пяти.
//
// Почему это логическая ошибка, а не настройка. У всех четырёх запросов
// ближайшая пара совпадала с запросом ПОЛНОСТЬЮ — расстояние ноль по сырым
// показаниям опыта, — и голос всё равно отдавал ответ чужой метке. Написание
// метки к запросу отношения не имеет.
//
// Эталоны взяты из существующих источников, а не придуманы:
//   - число соседей боевого маршрута и его адрес — из существующей проверки
//     `Tests/ExperimentFixes/MethodKnnExpressionBodyLearningTests.cs`, где та же
//     постоянная `ProductionNeighbours` равна пяти;
//   - инвариант «двадцать четыре пары отвечают своим же продолжением» — из той
//     же проверки, метод `AtProductionNeighbours_EveryLearnedPairAnswersItsOwnTarget`,
//     вместе с её учебными формами;
//   - четыре обучающих текста и четыре запроса с эталонными метками — дословно
//     из замороженного хозяина восемьдесят пятого опыта
//     (`experiments-v2/RP01-MICRO/P8501/Program.cs`, строки 61–71 и 458–464);
//   - ожидаемые ответы продукта до правки — из его сырых показаний
//     (`.../results/attempt-0001/panel-production.json`): при пяти соседях
//     `ErrorHandling` на всех четырёх запросах, при одном соседе — `Guard`,
//     `Guard`, `Loop`, `Loop`.
//
// Какие проверки падали ДО правки: три первые. Две последние проходили и до неё
// и обязаны проходить после — это сторона «не ломает законное».
//
// Чего эта приёмка НЕ доказывает. Она не измеряет качество на отложенной
// выборке: четыре запроса и четыре обучающие пары — это панель опыта, а не
// отложенный набор. Обученной способности она не заявляет: ни контрольной
// точки, ни абляции, ни свежей загрузки, ни продолжения обучения у этой
// единицы нет. Она не вводит вес голоса по расстоянию — это отдельная
// непроверенная гипотеза, записанная в реестре предложением карточки опыта.

namespace LlmFromScratch.Tests.ExperimentFixes;

using SemanticCompiler.Prediction;

public sealed class MethodKnnTieIsResolvedByDistanceTests
{
    // Продуктовое число соседей: тот же адрес и то же значение, что в соседней
    // существующей проверке этого предсказателя.
    private const int ProductionNeighbours = 5;

    // Четыре обучающих текста боевой панели восемьдесят пятого опыта, дословно.
    // Каждый файл даёт ровно одну пару «метод — следующий метод», поэтому на
    // четырёх файлах выходит четыре пары — по одной на метку.
    private const string TrainGuardAsync =
        "class GA{void A(int x){if(x<0)return;} async System.Threading.Tasks.Task B(int x){if(x<0)return;await System.Threading.Tasks.Task.Delay(1);}}";
    private const string TrainLoop =
        "class LP{void A(){for(int i=0;i<2;i++){}} void B(){for(int i=0;i<3;i++){}}}";
    private const string TrainErrorHandling =
        "class EH{void A(int x){try{}catch{}} void B(int x){try{}catch{}}}";
    private const string TrainSimple =
        "class SP{void A(){int a=1;} void B(){int b=2;}}";

    // Четыре экзаменационных запроса панели с эталонными метками, дословно.
    private static (string Name, string Source, string Gold)[] ExamQueries() =>
    [
        ("guard-0", "class Q{void Q1(int n){if(n<0)return;}}",     "Guard"),
        ("guard-1", "class Q{void Q2(int n){if(n<1)return;}}",     "Guard"),
        ("loop-0",  "class Q{void Q3(){for(int j=0;j<4;j++){}}}",  "Loop"),
        ("loop-1",  "class Q{void Q4(){for(int j=1;j<5;j++){}}}",  "Loop")
    ];

    // Обучающий набор боевой панели: четыре файла, по своему номеру у каждого.
    private static List<PackedMethod> ProductionTraining()
    {
        var methods = new List<PackedMethod>();
        short fileId = 0;
        foreach (var source in new[] { TrainGuardAsync, TrainLoop, TrainErrorHandling, TrainSimple })
        {
            var extracted = MethodExtractor.Extract(source, fileId++);
            Assert.True(extracted.IsSuccess, extracted.ErrorMessage);
            methods.AddRange(extracted.Value!);
        }

        return methods;
    }

    // Запрос — первый метод своего файла, как и в хозяине опыта.
    private static PackedMethod Query(string source)
    {
        var extracted = MethodExtractor.Extract(source, fileId: 99);
        Assert.True(extracted.IsSuccess, extracted.ErrorMessage);
        return extracted.Value![0];
    }

    // Обученный предсказатель на заданном числе соседей.
    private static MethodKnnPredictor Trained(int neighbours, List<PackedMethod> methods)
    {
        var predictor = new MethodKnnPredictor(k: neighbours);
        predictor.Train(methods);
        return predictor;
    }

    [Fact]
    public void AtProductionNeighbours_EachExamQueryGetsItsNearestPairLabel()
    {
        // ДО ПРАВКИ ЭТА ПРОВЕРКА ПАДАЛА на первом же запросе: голос был ровным, и
        // продукт отдавал `ErrorHandling` всем четырём запросам, потому что «E»
        // идёт первой среди `ErrorHandling`, `Guard`, `Loop`, `Mixed`.
        var methods = ProductionTraining();
        var predictor = Trained(ProductionNeighbours, methods);

        // Обученность ровно та, при которой боевой маршрут уже отвечает.
        Assert.Equal(4, predictor.TrainingSize);

        foreach (var exam in ExamQueries())
        {
            Assert.Equal(exam.Gold, predictor.Predict(Query(exam.Source)));
        }
    }

    [Fact]
    public void AtProductionNeighbours_TheAnswerEqualsTheOneNeighbourAnswer()
    {
        // Второй способ сказать то же самое, и он не зависит от эталонных меток:
        // когда сосед один, ровного голоса не бывает вовсе, и ответ заведомо
        // принадлежит ближайшей паре. Правка обязана привести ответ при пяти
        // соседях к ответу при одном — на наборе, где голос ровный.
        var methods = ProductionTraining();
        var atFive = Trained(ProductionNeighbours, methods);
        var atOne = Trained(1, methods);

        foreach (var exam in ExamQueries())
        {
            var query = Query(exam.Source);
            Assert.Equal(atOne.Predict(query), atFive.Predict(query));
        }
    }

    [Fact]
    public void ALabelThatSortsLastStillWinsWhenItIsNearest()
    {
        // Инвариант «написание метки не решает», проверенный с другой стороны:
        // здесь ближайшая пара несёт метку ПОСЛЕДНЮЮ по алфавиту из четырёх.
        // До правки побеждала первая по алфавиту, то есть ровно противоположная.
        var methods = ProductionTraining();
        var query = Query("class Q{void Q5(){int z=9;}}");
        var nearestLabel = Trained(1, methods).Predict(query);

        // Сначала убеждаемся, что оснастка действительно про последнюю метку:
        // иначе проверка проходила бы по случайной причине.
        Assert.Equal("Mixed", nearestLabel);
        Assert.Equal("ErrorHandling", new[] { "ErrorHandling", "Guard", "Loop", "Mixed" }
            .OrderBy(label => label, StringComparer.Ordinal).First());

        Assert.Equal(nearestLabel, Trained(ProductionNeighbours, methods).Predict(query));
    }

    [Fact]
    public void AnUntiedVoteIsNotChanged()
    {
        // Сторона «не ломает законное». Эта проверка проходила и до правки:
        // инвариант взят из существующей постоянной проверки площадки — на
        // двадцати четырёх парах при пяти соседях каждая пара отвечает своим же
        // продолжением, потому что голос там НЕ ровный.
        var methods = new List<PackedMethod>();
        short fileId = 0;
        for (var copy = 1; copy <= 6; copy++)
        {
            foreach (var source in VolumeForms(copy))
            {
                var extracted = MethodExtractor.Extract(source, fileId++);
                Assert.True(extracted.IsSuccess, extracted.ErrorMessage);
                methods.AddRange(extracted.Value!);
            }
        }

        var predictor = Trained(ProductionNeighbours, methods);
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

    [Fact]
    public void EmptyTrainedMemoryStillAnswersTheAbsenceCode()
    {
        // Инвариант, который правка не имеет права задеть: при пустой обученной
        // памяти ответ остаётся прежним кодом отсутствия. Обученность ничем не
        // подменяется, и правка в этот путь не входит.
        var predictor = new MethodKnnPredictor(k: ProductionNeighbours);

        Assert.Equal(0, predictor.TrainingSize);
        Assert.Equal("Mixed", predictor.Predict(Query("class Q{void Q6(){int z=9;}}")));
    }

    // Четыре формы учебного файла из существующей проверки площадки: однострочник
    // с блочным двойником и оба блочных, в асинхронном и обычном виде.
    private static string[] VolumeForms(int copy) =>
    [
        $"class A{copy}{{async Task<int> M()=>await R();async Task<int> N(){{return await R();}}}}",
        $"class B{copy}{{async Task<int> M(){{return await R();}}async Task<int> N(){{return await R();}}}}",
        $"class C{copy}{{int M()=>Calc();int N(){{return Calc();}}}}",
        $"class D{copy}{{int M(){{return Calc();}}int N(){{return Calc();}}}}"
    ];
}
