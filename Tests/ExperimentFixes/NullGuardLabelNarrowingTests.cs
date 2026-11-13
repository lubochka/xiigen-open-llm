// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка назначенного остатка «Проверка типа int не становится проверкой на
// пустое значение» (строка работ о сужении распознавания проверки на пустое
// значение, FIX-NG; карточка 39-null-guard-training-label.md).
//
// ЧТО БЫЛО НЕВЕРНО, ПО-ЧЕЛОВЕЧЕСКИ. Разметчик учебных меток решает, нужен ли
// методу совет «добавь проверку на пустое значение». Он даёт этот совет, когда
// метод обращается к члену объекта и при этом НЕ защищён проверкой на пустое
// значение. Признаком защиты считалось ЛЮБОЕ сопоставление с образцом внутри
// условия. Поэтому обычная проверка величины числа — «если число больше нуля» в
// записи через образец — тоже считалась защитой от пустого значения, совет
// пропадал, и учебная метка выходила ложной. Ложная метка попадает в память
// ученика ДО обучения, то есть модель учится на разметке, которой в коде нет.
//
// ПОЧЕМУ ЭТО ЛОГИЧЕСКАЯ ОШИБКА, А НЕ НАСТРОЙКА. У строки
// `int F(string? x, int n){ if (n is > 0) return x.Length; ... }` значения
// `n = 1, x = пусто` одновременно удовлетворяют условию и дают обращение к члену
// пустого объекта. Значит вывод «такой образец есть, следовательно получатель
// защищён» неверен сам по себе, при любых порогах и любом числе соседей.
//
// ЧТО СУЖЕНО, А ЧТО СОХРАНЕНО. Исключается ровно один предусмотренный образец:
// сравнение величины (relational pattern) по имени параметра, чей тип записан
// как прямое `int`. Сохраняются: ветка пустого литерала; образец с типом
// (`x is string s`); сравнение величины по параметру допускающего пустоту типа
// `int?` — там образец действительно может защищать сам параметр; порядок
// приоритета всех четырёх меток; признаки, число соседей и учебные дозы.
//
// КАКИЕ ПРОВЕРКИ ПАДАЛИ ДО ПРАВКИ (три): «прямое сравнение целого не является
// защитой от пустого значения», «обученная память отвечает на целевой запрос
// через продуктовый писатель и читатель» и «обученная память хранит двенадцать
// строк и меняет ровно семейство постороннего целого». Остальные проходили и до
// правки, и после — это сторона «не ломает законное» и две опоры непустоты.
//
// ОПОРЫ НЕПУСТОТЫ, ПРОХОДЯЩИЕ НА ОБОИХ СОСТОЯНИЯХ ПРОДУКТА. Они доказывают, что
// урок в данных ЕСТЬ и достижим, а прятало его ровно распознавание образца:
//   - то же сравнение величины, записанное обычным оператором `n > 0` вместо
//     образца `n is > 0`, уже даёт совет о проверке на пустое значение и до
//     правки, и после;
//   - то же тело вообще без условия даёт тот же совет на обоих состояниях;
//   - вектор признаков целевого запроса побитово равен векторам его собственного
//     учебного семейства на обоих состояниях, то есть запрос попадает именно в
//     свою тройку соседей и изменение метки обязано изменить ответ.
//
// ЭТАЛОНЫ НЕ ПРИДУМАНЫ. Двенадцать учебных строк, четыре экзаменационных запроса
// и ожидаемые метки взяты дословно из карточки (разделы «Неизменяемые данные» —
// таблицы TRAIN и EXAM). Боевое число соседей, порог допуска читателя и имена
// продуктового писателя и читателя взяты из самого продукта
// (`12.SemanticCompiler/SemanticCompilerFacade.cs`).
//
// ЧЕГО ЭТА ПРИЁМКА НЕ ДОКАЗЫВАЕТ. Это узнавание своего семейства с изменёнными
// именами и константами, а не обобщение на незнакомые семейства. Роста качества
// перевода, разбора или сквозного ответа она не заявляет. Единица не имеет
// сохранения и загрузки состояния, поэтому холодное восстановление, абляция и
// продолжение обучения здесь не проверяются и не утверждаются. Семантическая
// модель не используется: сужение опирается на написание типа параметра, а не на
// разрешение имён, затенение или межпроцедурный анализ пустоты.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using SemanticCompiler;
using SemanticCompiler.Prediction;
using Xunit.Abstractions;

public sealed class NullGuardLabelNarrowingTests
{
    // Вывод в журнал прогона: по нему снимаются показания обучения до и после.
    private readonly ITestOutputHelper _output;

    // xUnit передаёт приёмник вывода; он нужен для показаний обучения.
    public NullGuardLabelNarrowingTests(ITestOutputHelper output) => _output = output;

    // ── Неизменяемые данные карточки ────────────────────────────────────────

    // Семейство постороннего целого: сравнение величины по прямому int-параметру.
    // Именно здесь метка была ложной: совет о проверке на пустое значение пропадал.
    private const string T01 = "class L1{int F(string? x,int n){if(n is >0)return x.Length;return 0;}}";
    private const string T02 = "class L2{int G(string? a,int count){if(count is >1)return a.Length;return 1;}}";
    private const string T03 = "class L3{int H(string? text,int size){if(size is >2)return text.Length;return 2;}}";

    // Семейство пустого литерала: настоящая защита, совет обязан пропадать.
    private const string T04 = "class L4{int F(string? x){if(x==null)return 0;return x.Length;}}";
    private const string T05 = "class L5{int G(string? a){if(a==null)return 1;return a.Length;}}";
    private const string T06 = "class L6{int H(string? text){if(text==null)return 2;return text.Length;}}";

    // Семейство образца с типом: совет обязан пропадать и после сужения.
    private const string T07 = "class L7{int F(object? x){if(x is string s)return s.Length;return 0;}}";
    private const string T08 = "class L8{int G(object? a){if(a is string b)return b.Length;return 1;}}";
    private const string T09 = "class L9{int H(object? value){if(value is string text)return text.Length;return 2;}}";

    // Семейство допускающего пустоту целого: образец защищает сам параметр,
    // поэтому сужение обязано его НЕ затронуть. Это главная защита от пересужения.
    private const string T10 = "class L10{int F(int? x){if(x is >0)return x.Value;return 0;}}";
    private const string T11 = "class L11{int G(int? a){if(a is >1)return a.Value;return 1;}}";
    private const string T12 = "class L12{int H(int? value){if(value is >2)return value.Value;return 2;}}";

    // Целевой запрос: то же семейство постороннего целого с другими именами.
    private const string Q1 = "class Q1{int Read(string? item,int limit){if(limit is >3)return item.Length;return 3;}}";
    // Три контрольных запроса: по одному на каждое сохраняемое семейство.
    private const string Q2 = "class Q2{int Read(string? item){if(item==null)return 3;return item.Length;}}";
    private const string Q3 = "class Q3{int Read(object? item){if(item is string content)return content.Length;return 3;}}";
    private const string Q4 = "class Q4{int Read(int? item){if(item is >3)return item.Value;return 3;}}";

    // Двенадцать учебных строк в том же порядке, в каком их принимает писатель.
    private static readonly string[] TrainingSources =
        [T01, T02, T03, T04, T05, T06, T07, T08, T09, T10, T11, T12];

    // Независимый эталон меток: три строки постороннего целого требуют совета о
    // проверке на пустое значение, остальные девять его не требуют.
    private static readonly int[] GoldTrainingLabels =
    [
        ActionLabelGenerator.ADD_NULL_CHECK, ActionLabelGenerator.ADD_NULL_CHECK, ActionLabelGenerator.ADD_NULL_CHECK,
        ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD,
        ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD,
        ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.ADD_METHOD
    ];

    // Боевой порог читателя: ниже четырёх обученных строк он молчит (facade:383).
    private const int ReaderTrainingFloor = 4;

    // ── Решающая проверка: сама суть сужения ────────────────────────────────

    // Падала до правки: все четыре строки получали совет «добавь метод» вместо
    // совета о проверке на пустое значение, потому что сравнение величины по
    // прямому целому считалось защитой от пустого значения.
    [Fact]
    public void DirectIntRelationalPattern_IsNotANullGuard_SoTheLessonSurvives()
    {
        // Показания собираются по всем строкам семейства ДО любого сравнения,
        // иначе журнал обрывался бы на первой же ложной метке.
        var actual = new List<string>();
        // Каждая строка семейства постороннего целого размечается по отдельности.
        foreach (var (name, source) in new[] { ("T01", T01), ("T02", T02), ("T03", T03), ("Q1", Q1) })
        {
            // Разметчик вызывается ровно один раз на строку: это и есть учитель.
            var label = ActionLabelGenerator.Label(source);
            // В журнал попадает фактическая метка, а не только итог сравнения.
            _output.WriteLine($"{name}: метка={ActionLabelGenerator.Labels[label]}");
            // Показание сохраняется для сравнения всей панели целиком.
            actual.Add(ActionLabelGenerator.Labels[label]);
        }

        // Ожидание независимое: обращение к члену не защищено от пустого значения,
        // поэтому все четыре строки обязаны получить совет о проверке.
        var expected = Enumerable.Repeat(
            ActionLabelGenerator.Labels[ActionLabelGenerator.ADD_NULL_CHECK], actual.Count).ToList();
        // Сравнение панелями целиком: в отчёте о падении видны все четыре метки.
        Assert.Equal(expected, actual);
    }

    // ── Сохраняемые семейства: проходят на обоих состояниях продукта ────────

    // Пустой литерал — настоящая защита: совет обязан пропадать и до, и после.
    [Fact]
    public void NullLiteralGuard_StillSuppressesTheLesson()
    {
        // Три учебные строки и её контрольный запрос проверяются одним списком.
        foreach (var source in new[] { T04, T05, T06, Q2 })
            // Ожидается отсутствие совета о проверке на пустое значение.
            Assert.Equal(ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.Label(source));
    }

    // Образец с типом не является сравнением величины, значит сужение его не берёт.
    [Fact]
    public void TypePattern_StillSuppressesTheLesson()
    {
        // Три учебные строки и контрольный запрос этого семейства.
        foreach (var source in new[] { T07, T08, T09, Q3 })
            // Совет обязан отсутствовать: образец с типом исключает пустое значение.
            Assert.Equal(ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.Label(source));
    }

    // Главная защита от пересужения: у допускающего пустоту целого сравнение
    // величины действительно отсекает пустое значение, и его исключать нельзя.
    [Fact]
    public void NullableIntRelationalPattern_StillSuppressesTheLesson()
    {
        // Три учебные строки и контрольный запрос с типом параметра `int?`.
        foreach (var source in new[] { T10, T11, T12, Q4 })
            // Совет обязан отсутствовать: иначе сужение задело законную защиту.
            Assert.Equal(ActionLabelGenerator.ADD_METHOD, ActionLabelGenerator.Label(source));
    }

    // ── Опоры непустоты: доказывают, что урок есть и прятало его распознавание ──

    // Первая опора: то же сравнение величины, записанное обычным оператором, уже
    // даёт совет на ОБОИХ состояниях продукта. Значит данные и ветка приоритета
    // на месте, а скрывало совет ровно распознавание записи через образец.
    [Fact]
    public void NonPatternIntComparison_AlreadyKeepsTheLesson_OnBothProductStates()
    {
        // Та же строка, но условие записано как `n>0`, а не как `n is >0`.
        const string sameCheckWithoutPattern = "class L1{int F(string? x,int n){if(n>0)return x.Length;return 0;}}";
        // Совет обязан быть и до правки, и после: сопоставления с образцом здесь нет.
        Assert.Equal(ActionLabelGenerator.ADD_NULL_CHECK, ActionLabelGenerator.Label(sameCheckWithoutPattern));
    }

    // Вторая опора: то же тело вообще без условия даёт тот же совет на обоих
    // состояниях. Отличие от учебной строки ровно одно — обёртка условия.
    [Fact]
    public void SameBodyWithoutAnyCondition_KeepsTheLesson_OnBothProductStates()
    {
        // То же обращение к члену, но без всякого условия вокруг него.
        const string sameBodyNoCondition = "class L1{int F(string? x,int n){return x.Length;}}";
        // Совет обязан быть на обоих состояниях продукта.
        Assert.Equal(ActionLabelGenerator.ADD_NULL_CHECK, ActionLabelGenerator.Label(sameBodyNoCondition));
    }

    // Третья опора: вектор признаков целевого запроса побитово равен векторам его
    // собственного учебного семейства. Значит запрос попадает именно в свою
    // тройку соседей, и исправление их меток обязано изменить его ответ.
    [Fact]
    public void QueryVectorMatchesItsOwnFamily_OnBothProductStates()
    {
        // Вектор целевого запроса считается продуктовым построителем признаков.
        var queryVector = ActionLabelGenerator.FeatureVector(Q1);
        // Сверяется с каждой из трёх строк собственного семейства.
        foreach (var (name, source) in new[] { ("T01", T01), ("T02", T02), ("T03", T03) })
        {
            // Вектор учебной строки считается тем же построителем.
            var trainVector = ActionLabelGenerator.FeatureVector(source);
            // Равенство поэлементное: признаки правкой метки не затрагиваются.
            Assert.Equal(queryVector.Length, trainVector.Length);
            // Побитовое равенство каждого признака доказывает попадание в своё семейство.
            for (var index = 0; index < queryVector.Length; index++)
                Assert.Equal(
                    BitConverter.SingleToInt32Bits(queryVector[index]),
                    BitConverter.SingleToInt32Bits(trainVector[index]));
            // В журнал попадает факт совпадения с конкретной учебной строкой.
            _output.WriteLine($"вектор Q1 побитово равен вектору {name}");
        }
    }

    // Прочие метки сужением не затронуты: совет о цикле и совет о выделении.
    [Fact]
    public void OtherLabelFamilies_AreNotTouchedByTheNarrowing()
    {
        // Коллекция в параметрах без цикла в теле даёт совет о цикле.
        Assert.Equal(
            ActionLabelGenerator.ADD_LOOP,
            ActionLabelGenerator.Label("class C{void P(System.Collections.Generic.IEnumerable<int> items){var x=1;}}"));
        // Обращение к члену без всякой защиты по-прежнему даёт совет о проверке:
        // это дословное ожидание уже существующей проверки продукта.
        Assert.Equal(
            ActionLabelGenerator.ADD_NULL_CHECK,
            ActionLabelGenerator.Label("public sealed class C { public void Process(string s) { var len = s.Length; } }"));
        // Строка без обращения к члену и без коллекции даёт совет о методе.
        Assert.Equal(
            ActionLabelGenerator.ADD_METHOD,
            ActionLabelGenerator.Label("class C{int P(int n){return n;}}"));
    }

    // ── Прогон обучения: настоящий продуктовый писатель и настоящий читатель ──

    // Падала до правки: соседями целевого запроса были три строки с ложной меткой,
    // поэтому обученная память отвечала «добавь метод» вместо совета о проверке.
    [Fact]
    public void TrainedMemoryAnswersTheTargetQueryThroughProductWriterAndReader()
    {
        // Один холодный фасад на прогон: опция чтения метки файла включена явно.
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions
        {
            UseFileActionKnn = true,   // включает читателя метки файла
            UseRegexPreprocessor = false, // посторонние преобразования выключены
            UseMethodKnn = false,      // чужая единица в прогоне не участвует
            UseStructuralKnn = false   // чужая единица в прогоне не участвует
        });

        // Настоящий продуктовый читатель берётся по точной подписи, а не копируется.
        var reader = typeof(SemanticCompilerFacade).GetMethod(
            "PredictFileAction",
            BindingFlags.Instance | BindingFlags.NonPublic,
            [typeof(string), typeof(StackLabel)]);
        // Отсутствие читателя — ошибка постановки, а не провал продукта.
        Assert.NotNull(reader);

        // Показания до обучения собираются целиком, затем сравниваются.
        var beforeTraining = new List<string>();
        // До обучения читатель обязан молчать: обученных строк меньше порога.
        foreach (var (name, query) in new[] { ("Q1", Q1), ("Q2", Q2), ("Q3", Q3), ("Q4", Q4) })
        {
            // Тот же читатель на том же фасаде, язык задан явно.
            var before = reader!.Invoke(facade, [query, StackLabel.CSharp]) as string;
            // Показание до обучения уходит в журнал прогона.
            _output.WriteLine($"ДО обучения {name}: {before ?? "(молчит)"}");
            // Показание сохраняется для сравнения всей панели целиком.
            beforeTraining.Add(before ?? "(молчит)");
        }

        // Молчание до обучения — это защита фасада, а не обученный ответ.
        Assert.Equal(Enumerable.Repeat("(молчит)", 4).ToList(), beforeTraining);

        // Ровно один настоящий прогон обучения на двенадцати учебных строках.
        facade.TrainFileActionKnn(TrainingSources);

        // Память ученика читается только для снимка, без записи в неё.
        var predictor = PrivateField<FileActionKnnPredictor>(facade, "_fileActionKnn");
        // Двенадцать строк обязаны лежать в памяти: писатель не отбросил ни одной.
        Assert.Equal(TrainingSources.Length, predictor.TrainingSize);
        // Порог читателя заведомо пройден, иначе ответ был бы молчанием.
        Assert.True(predictor.TrainingSize >= ReaderTrainingFloor);

        // Ожидаемые ответы после обучения: целевой запрос получает совет о
        // проверке на пустое значение, три контроля остаются при совете о методе.
        var expected = new (string Name, string Query, int Label)[]
        {
            ("Q1", Q1, ActionLabelGenerator.ADD_NULL_CHECK),
            ("Q2", Q2, ActionLabelGenerator.ADD_METHOD),
            ("Q3", Q3, ActionLabelGenerator.ADD_METHOD),
            ("Q4", Q4, ActionLabelGenerator.ADD_METHOD)
        };

        // Показания после обучения собираются целиком, затем сравниваются.
        var afterTraining = new List<string>();
        // Каждый запрос проходит через тот же продуктовый читатель после обучения.
        foreach (var (name, query, _) in expected)
        {
            // Фактический ответ обученной памяти через боевой маршрут читателя.
            var answer = reader!.Invoke(facade, [query, StackLabel.CSharp]) as string;
            // Показание после обучения уходит в журнал прогона.
            _output.WriteLine($"ПОСЛЕ обучения {name}: {answer ?? "(молчит)"}");
            // Показание сохраняется для сравнения всей панели целиком.
            afterTraining.Add(answer ?? "(молчит)");
        }

        // Сравнение панелями целиком: в отчёте о падении видны все четыре ответа.
        Assert.Equal(
            expected.Select(row => ActionLabelGenerator.Labels[row.Label]).ToList(),
            afterTraining);
    }

    // Падала до правки: в памяти лежали девять верных меток из двенадцати.
    [Fact]
    public void TrainedMemoryKeepsTwelveRowsAndChangesOnlyTheUnrelatedIntFamily()
    {
        // Отдельный холодный фасад: прогон обучения этой проверки независим.
        var facade = new SemanticCompilerFacade(options: new SemanticCompilerOptions { UseFileActionKnn = true });
        // Ровно один настоящий прогон обучения на тех же двенадцати строках.
        facade.TrainFileActionKnn(TrainingSources);

        // Память читается только для снимка фактически записанных строк.
        var predictor = PrivateField<FileActionKnnPredictor>(facade, "_fileActionKnn");
        // Снимок берётся из того же приватного поля, что хранит обученные пары.
        var rows = PrivateField<List<(float[] Features, int Label)>>(predictor, "_train");
        // Двенадцать строк: ни одна учебная строка не потеряна и не удвоена.
        Assert.Equal(GoldTrainingLabels.Length, rows.Count);

        // Вся панель фактических меток собирается ДО сравнения, чтобы показания
        // обучения попали в журнал целиком и на старом, и на новом коде.
        var actualLabels = new List<string>();
        // Фактические метки читаются из памяти по учебному порядку.
        for (var index = 0; index < rows.Count; index++)
        {
            // Имя строки восстанавливается по её месту в учебном порядке.
            var rowName = $"T{index + 1:00}";
            // Фактическая метка этой строки в обученной памяти.
            var actual = rows[index].Label;
            // Показание уходит в журнал: так видно, какие метки изменились.
            _output.WriteLine($"{rowName}: метка в памяти={ActionLabelGenerator.Labels[actual]}");
            // Признаков должно быть ровно тридцать: правка метки их не меняет.
            Assert.Equal(30, rows[index].Features.Length);
            // Показание сохраняется для сравнения всей панели целиком.
            actualLabels.Add(ActionLabelGenerator.Labels[actual]);
        }

        // Сравнение панелями целиком: отчёт о падении показывает все двенадцать меток.
        Assert.Equal(GoldTrainingLabels.Select(label => ActionLabelGenerator.Labels[label]).ToList(), actualLabels);

        // Девять сохраняемых строк обязаны остаться при совете о методе: это и есть
        // доказательство, что сужение изменило ровно семейство постороннего целого.
        for (var index = 3; index < rows.Count; index++)
            // Ни одна из строк T04–T12 не получила совета о проверке на пустое значение.
            Assert.NotEqual(ActionLabelGenerator.ADD_NULL_CHECK, rows[index].Label);
    }

    // Чтение приватного поля только для снимка: запись в поля не выполняется.
    private static T PrivateField<T>(object owner, string fieldName)
    {
        // Поле ищется по точному имени на фактическом типе владельца.
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        // Отсутствие поля — ошибка постановки проверки, а не провал продукта.
        Assert.NotNull(field);
        // Значение приводится к ожидаемому типу; подмена типа была бы ошибкой.
        return (T)field!.GetValue(owner)!;
    }
}
