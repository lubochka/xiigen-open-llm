// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие приёма носителя признаков запроса обученной памятью
// ближайших соседей (карточка переноса «Не терять признаки при чтении
// обученной KNN-памяти», номер 126 в папке docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments; строка работ «Признаки запроса доходят до KNN
// без потери», номер 143 реестра того же каталога).
//
// Что здесь проверяется по-человечески. Обученная память ближайших соседей
// получает вопрос и вместе с ним признаки запроса — короткие пометки о том,
// что именно просят сделать. Эти признаки занимают вторую половину вектора
// запроса, то есть прямо влияют на выбор ближайшего урока. Вопрос простой:
// доходят ли признаки до памяти, если вызывающая сторона передала их любым
// законным способом — массивом, списком или закрытой от записи коллекцией.
//
// Что было сломано. Чтение признаков принимало ровно один носитель — массив
// строк. Те же значения в списке или в закрытой от записи коллекции МОЛЧА
// превращались в пустой набор: ответ выдавался как успешный, признаки при
// этом терялись, и ни вызывающая сторона, ни журнал об этом не узнавали.
// Отдельно терялась и диагностика непригодного входа: элемент-пустышка внутри
// массива ронял чтение безымянной ошибкой «сбой вывода», а тот же
// элемент-пустышка внутри списка исчезал вместе со всеми признаками и давал
// успешный ответ. Тихая потеря данных вместо отказа — это логическая ошибка
// диагностики, а не неудобство.
//
// Как это доказывается. Одна общая обученная память на весь класс: один
// настоящий проход обучения на восьми уроках одной темы — оркестрация работы
// с кодом. Дальше только штатное чтение через обычный открытый вход
// ExecuteAsync; экспериментальный читатель прошлого опыта сюда НЕ переносится
// и продуктом не считается. Главная проверка сравнивает ответ на списке и на
// закрытой от записи коллекции с ответом на массиве ТОГО ЖЕ обученного
// объекта: совпасть обязаны маршрут, шаги, ссылки на доказательства,
// уверенность и предупреждения. Ожидаемые маршрут и шаги выписаны буквально
// из набора уроков, а не прочитаны из самого ответа.
//
// Бюджет этого класса. Ровно одно обучение на восьми уроках, один вложенный
// подбор словаря кусочков слов и одно построение указателя соседей; тридцать
// пять чтений ответа, из них восемь — ранние отказы. Ни одного обучения с
// учителем, ни одной пары предпочтений, ни одного восстановления состояния.
//
// Чего это покрытие НЕ доказывает. Оно не доказывает роста точности: маршрут
// и шаги верны и до правки, и после неё, а исправляется именно сохранность
// признаков и внятность отказа. Оно не доказывает холодного восстановления
// памяти из сохранённого состояния: у этого блока нет своего собственного
// чтения состояния, и здесь обучается один живой объект. Оно не доказывает
// подключения блока к обычному продуктовому маршруту приложения: полный путь
// от промпта до ответа остаётся открытым у другого владельца, и ни один
// тестовый объект его не закрывает. Уверенность здесь — шкала близости
// соседа, а не вероятность правильности ответа.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Collections;
using System.Collections.ObjectModel;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Ответ темы «оркестрация работы с кодом» принадлежит этому покрытию, а не
// продукту: маршрут и изменяемый массив шагов, как в исходном опыте.
internal sealed record OrchestrationAnswer(string Route, string[] Steps);

// Один урок назначенного набора: опознание, вход, маршрут и порядок шагов.
internal sealed record CarrierLesson(string Id, string Input, string Route, string[] Steps);

// Общая обученная память на весь класс проверок. Создаётся ровно один раз,
// поэтому обучение в классе одно, а не по одному на каждую проверку.
public sealed class KnnPredicateCarrierLearningFixture
{
    // Предметная тема обучения: оркестрация работы с кодом.
    internal const string DomainTag = "orchestration-order";

    // Опознание обучаемого блока и его роль в составе логических единиц.
    internal const string UnitId = "p108-carrier-unit";
    internal const string UnitRole = "orchestration-order-selector";

    // Подпись исходника продуктового файла на срезе карточки: если файл
    // изменится, несовпадение видно сразу и номера строк не применяются слепо.
    internal const string SourceSha256 = "3dfc1e9f40f0800e6cb8b431efc1609dbce1d08a9fc35d1e8a5ea298b2b5d70a";

    // Восемь уроков набора «U-OR», дословно из исходника опыта P108
    // (experiments-v2/RP01-MICRO/P10801/Program.cs, строки 169–180).
    internal static readonly CarrierLesson[] Lessons =
    [
        new("u1", "review code then implement change", "review-first", ["review", "implement"]),
        new("u2", "inspect code then implement change", "review-first", ["review", "implement"]),
        new("u3", "review module before editing it", "review-first", ["review", "implement"]),
        new("u4", "inspect module before editing it", "review-first", ["review", "implement"]),
        new("u5", "implement change then review code", "build-first", ["implement", "review"]),
        new("u6", "implement change then inspect code", "build-first", ["implement", "review"]),
        new("u7", "edit module before reviewing it", "build-first", ["implement", "review"]),
        new("u8", "edit module before inspecting it", "build-first", ["implement", "review"])
    ];

    // Четыре экзаменационных входа набора «U-OR», дословно из того же
    // исходника (строки 224–229). В обучение они не входят.
    internal static readonly CarrierLesson[] Exams =
    [
        new("e1", "review code before implementing change", "review-first", ["review", "implement"]),
        new("e2", "inspect code before implementing change", "review-first", ["review", "implement"]),
        new("e3", "implement change before reviewing code", "build-first", ["implement", "review"]),
        new("e4", "implement change before inspecting code", "build-first", ["implement", "review"])
    ];

    // Обученный блок, общий для всех проверок класса.
    internal KnnLogicalUnit<string, OrchestrationAnswer> Unit { get; }

    // Одно обучение на весь класс: восемь уроков одной партией.
    public KnnPredicateCarrierLearningFixture()
    {
        // Тип ответа изменяем, поэтому политика владения передаётся явно:
        // память копирует маршрут и массив шагов и на приёме, и на выдаче.
        var answerPolicy = KnnAnswerSnapshot.FromCopy<OrchestrationAnswer>(
            answer => new OrchestrationAnswer(answer.Route, (string[])answer.Steps.Clone()));

        // Опознания признаков урока — это его порядок шагов, как в опыте.
        var examples = Lessons
            .Select(lesson => new LogicalKnnTrainingExample<string, OrchestrationAnswer>(
                lesson.Id,
                lesson.Input,
                new OrchestrationAnswer(lesson.Route, (string[])lesson.Steps.Clone()),
                (string[])lesson.Steps.Clone(),
                [DomainTag],
                [lesson.Id + "-ev"]))
            .ToList();

        // Один настоящий проход обучения: размерность 64, один сосед, порог 0.
        var trained = KnnLogicalUnit<string, OrchestrationAnswer>.Train(
            UnitId,
            UnitRole,
            DomainTag,
            examples,
            input => input,
            answerPolicy,
            vectorDimensions: 64,
            k: 1,
            lessonRequestConfidenceThreshold: 0.0);

        // Отказ обучения останавливает класс здесь же, а не притворяется памятью.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        Unit = trained.Value!;
    }
}

public sealed class KnnPredicateCarrierLearningTests(KnnPredicateCarrierLearningFixture fixture)
    : IClassFixture<KnnPredicateCarrierLearningFixture>
{
    // Общая обученная память класса: подменять её внутри проверок нельзя.
    private readonly KnnLogicalUnit<string, OrchestrationAnswer> _unit = fixture.Unit;

    // PC-LEARNED. Приобретённое знание ненулевое: все восемь уроков дают свой
    // полный правильный ответ, тогда как постоянная метка не проходит.
    // Восемь чтений ответа.
    [Fact]
    public async Task PcLearned_AllEightLessonsAnswerFully_AndConstantLabelFails()
    {
        // Счётчик полностью правильных ответов обученной памяти.
        var trainedCorrect = 0;

        foreach (var lesson in KnnPredicateCarrierLearningFixture.Lessons)
        {
            // Признаки урока передаются законным массивом строк.
            var answer = await ReadAsync(lesson.Input, true, (string[])lesson.Steps.Clone());

            // Чтение обязано пройти через общий конверт результата.
            Assert.True(answer.IsSuccess, answer.ErrorMessage);

            // Ожидания выписаны буквально из набора уроков, а не из ответа.
            Assert.Equal(lesson.Route, answer.Value!.Output.Route);
            Assert.Equal(lesson.Steps, answer.Value!.Output.Steps);

            // Полный правильный ответ — это маршрут И все шаги вместе.
            trainedCorrect++;
        }

        // Обученная память отвечает полностью правильно на всех восьми уроках.
        Assert.Equal(8, trainedCorrect);

        // Вырожденная постоянная метка считается без единого обращения к памяти:
        // в наборе четыре урока одного маршрута и четыре другого.
        var constantLabelCorrect = KnnPredicateCarrierLearningFixture.Lessons
            .Count(lesson => string.Equals(lesson.Route, "review-first", StringComparison.Ordinal));

        // Постоянная метка даёт 4 из 8 и потому не объясняет результат памяти.
        Assert.Equal(4, constantLabelCorrect);
        Assert.True(trainedCorrect > constantLabelCorrect);
    }

    // PC-CARRIERS. Главный закон переноса: массив, список и закрытая от записи
    // коллекция с ОДИНАКОВЫМИ значениями дают одинаковый запрос и потому
    // одинаковый ответ того же обученного объекта. Двенадцать чтений ответа.
    [Fact]
    public async Task PcCarriers_ListAndReadOnlyCollection_AnswerExactlyLikeTheArray()
    {
        // Счётчик полностью правильных ответов по всем носителям.
        var correct = 0;

        foreach (var exam in KnnPredicateCarrierLearningFixture.Exams)
        {
            // Признаки экзамена задаются отдельными литералами, эталон шагов
            // в построение запроса не передаётся.
            var predicates = PredicatesFor(exam.Id);

            // Носитель-массив того же обученного объекта — независимый образец
            // сравнения для двух других носителей.
            var viaArray = await ReadAsync(exam.Input, true, predicates.ToArray());
            Assert.True(viaArray.IsSuccess, viaArray.ErrorMessage);

            // Носитель-список: отдельный настоящий тип времени исполнения.
            var viaList = await ReadAsync(exam.Input, true, new List<string>(predicates));
            Assert.True(viaList.IsSuccess, viaList.ErrorMessage);

            // Носитель «закрытая от записи коллекция»: тоже отдельный тип.
            var viaReadOnly = await ReadAsync(
                exam.Input,
                true,
                new ReadOnlyCollection<string>(new List<string>(predicates)));
            Assert.True(viaReadOnly.IsSuccess, viaReadOnly.ErrorMessage);

            // Буквальный эталон маршрута и шагов для всех трёх носителей.
            foreach (var answer in new[] { viaArray.Value!, viaList.Value!, viaReadOnly.Value! })
            {
                Assert.Equal(exam.Route, answer.Output.Route);
                Assert.Equal(exam.Steps, answer.Output.Steps);
                correct++;
            }

            // Список обязан дать ровно тот же ответ, что массив того же объекта.
            AssertSameAnswer(viaArray.Value!, viaList.Value!, exam.Id + ": список против массива");

            // И закрытая от записи коллекция тоже обязана дать тот же ответ.
            AssertSameAnswer(viaArray.Value!, viaReadOnly.Value!, exam.Id + ": закрытая коллекция против массива");
        }

        // Двенадцать полностью правильных ответов: четыре входа на три носителя.
        Assert.Equal(12, correct);
    }

    // PC-NULL-ITEM. Элемент-пустышка внутри любого из трёх носителей — это
    // непригодный вход, и отказ называет и носитель, и место элемента, а не
    // прячет потерю признаков за успешным ответом. Три чтения ответа.
    [Fact]
    public async Task PcNullItem_IsRefusedByNameAndIndex_ForAllThreeCarriers()
    {
        // Первый экзаменационный вход; второй элемент признаков — пустышка.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];

        // Носитель-массив с пустышкой на месте номер один.
        var viaArray = await ReadAsync(exam.Input, true, new[] { "review", null! });
        AssertNullElementRefusal(viaArray, 1, "массив строк");

        // Носитель-список с пустышкой на том же месте.
        var viaList = await ReadAsync(exam.Input, true, new List<string> { "review", null! });
        AssertNullElementRefusal(viaList, 1, "список строк");

        // Носитель «закрытая от записи коллекция» с пустышкой на том же месте.
        var viaReadOnly = await ReadAsync(
            exam.Input,
            true,
            new ReadOnlyCollection<string>(new List<string> { "review", null! }));
        AssertNullElementRefusal(viaReadOnly, 1, "закрытая от записи коллекция строк");
    }

    // PC-ABSENT. Законное отсутствие признаков сохраняет прежнее поведение:
    // отсутствующий ключ, значение-пустышка, пустой массив и массив из одной
    // пробельной строки означают одно и то же. Четыре чтения ответа.
    [Fact]
    public async Task PcAbsent_FourLawfulWaysToSendNoPredicates_StayCompatible()
    {
        // Первый экзаменационный вход, маршрут которого известен буквально.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];

        // Ключа признаков в метаданных нет вовсе.
        var absentKey = await ReadAsync(exam.Input, false, null);
        Assert.True(absentKey.IsSuccess, absentKey.ErrorMessage);

        // Ключ есть, но значение — пустышка: это по-прежнему отсутствие данных.
        var nullValue = await ReadAsync(exam.Input, true, null);
        Assert.True(nullValue.IsSuccess, nullValue.ErrorMessage);

        // Пустой массив признаков допустим и отказом не является.
        var emptyArray = await ReadAsync(exam.Input, true, Array.Empty<string>());
        Assert.True(emptyArray.IsSuccess, emptyArray.ErrorMessage);

        // Массив из одной пробельной строки отбрасывается прежней нормализацией.
        var blankOnly = await ReadAsync(exam.Input, true, new[] { "   " });
        Assert.True(blankOnly.IsSuccess, blankOnly.ErrorMessage);

        // Буквальный маршрут известен заранее и не зависит от признаков.
        Assert.Equal(exam.Route, absentKey.Value!.Output.Route);

        // Все четыре законных способа «признаков нет» дают один и тот же ответ.
        AssertSameAnswer(absentKey.Value!, nullValue.Value!, "значение-пустышка против отсутствующего ключа");
        AssertSameAnswer(absentKey.Value!, emptyArray.Value!, "пустой массив против отсутствующего ключа");
        AssertSameAnswer(absentKey.Value!, blankOnly.Value!, "пробельная строка против отсутствующего ключа");
    }

    // PC-UNSUPPORTED. Неподдержанный носитель прекращает вызов именованным
    // отказом: ни строка целиком, ни массив чисел не становятся списком строк.
    // Два чтения ответа.
    [Fact]
    public async Task PcUnsupported_SingleStringAndIntArray_AreRefusedByName()
    {
        // Первый экзаменационный вход.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];

        // Строка целиком — это последовательность букв, а не признаков.
        var viaBareString = await ReadAsync(exam.Input, true, "review");
        Assert.False(viaBareString.IsSuccess);
        Assert.Equal("PREDICATE_IDS_CARRIER_UNSUPPORTED", viaBareString.ErrorCode);
        Assert.Contains(typeof(string).FullName!, viaBareString.ErrorMessage);

        // Массив чисел не превращается в признаки приведением к тексту.
        var viaIntArray = await ReadAsync(exam.Input, true, new[] { 1, 2 });
        Assert.False(viaIntArray.IsSuccess);
        Assert.Equal("PREDICATE_IDS_CARRIER_UNSUPPORTED", viaIntArray.ErrorCode);
        Assert.Contains(typeof(int[]).FullName!, viaIntArray.ErrorMessage);
    }

    // PC-OWNERSHIP. Запрос снимает собственную копию последовательности:
    // поздняя правка списка вызывающей стороной не меняет ни обученную память,
    // ни прежний ответ. Два чтения ответа.
    [Fact]
    public async Task PcOwnership_CallerMutatesItsListAfterTheRead_MemoryAndAnswerStayPut()
    {
        // Первый экзаменационный вход и его признаки отдельными литералами.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];
        var callerList = new List<string>(PredicatesFor(exam.Id));

        // Первое чтение: признаки пришли списком вызывающей стороны.
        var viaList = await ReadAsync(exam.Input, true, callerList);
        Assert.True(viaList.IsSuccess, viaList.ErrorMessage);
        Assert.Equal(exam.Route, viaList.Value!.Output.Route);
        Assert.Equal(exam.Steps, viaList.Value!.Output.Steps);

        // Нападение со стороны входа: вызывающая сторона портит свой список
        // уже ПОСЛЕ прочитанного ответа, прямо в тех же ячейках.
        callerList[0] = "СЛОМАНО-ПРИЗНАК";
        callerList.Add("СЛОМАНО-ДОБАВЛЕНО");

        // Второе чтение: тот же вход, те же исходные значения свежим массивом.
        // Между нападением и чтением нет ни обучения, ни перестройки указателя.
        var viaFreshArray = await ReadAsync(exam.Input, true, PredicatesFor(exam.Id).ToArray());
        Assert.True(viaFreshArray.IsSuccess, viaFreshArray.ErrorMessage);

        // Обученная память и ответ прежние: порча списка никуда не просочилась.
        AssertSameAnswer(viaList.Value!, viaFreshArray.Value!, "свежий массив после порчи списка");

        // Ответ не делит массив признаков ни с вызывающей стороной, ни с прошлым
        // ответом: это отдельные объекты, а не одна общая ячейка.
        Assert.NotSame(viaList.Value!.PredicateIds, viaFreshArray.Value!.PredicateIds);
        Assert.DoesNotContain("СЛОМАНО-ПРИЗНАК", viaFreshArray.Value!.PredicateIds);
    }

    // PC-EARLY-REFUSAL. Прежние ранние отказы остаются первыми: отменённый
    // признак работы и отсутствующий вход отвечают своими кодами даже тогда,
    // когда носитель признаков заведомо непригоден. Два чтения ответа.
    [Fact]
    public async Task PcEarlyRefusal_CancelledAndMissingInput_KeepTheirOwnCodes()
    {
        // Первый экзаменационный вход.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];

        // Признак работы отменён заранее.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        // Отмена обязана обогнать новую проверку носителя признаков.
        var afterCancel = await _unit.ExecuteAsync(
            BuildRequest(exam.Input, true, new[] { 1, 2 }),
            cancelled.Token);
        Assert.False(afterCancel.IsSuccess);
        Assert.Equal("CANCELLED", afterCancel.ErrorCode);

        // Отсутствующий вход тоже обязан обогнать проверку носителя признаков.
        var withoutInput = await _unit.ExecuteAsync(BuildRequest(null!, true, new[] { 1, 2 }));
        Assert.False(withoutInput.IsSuccess);
        Assert.Equal("INPUT_REQUIRED", withoutInput.ErrorCode);
    }

    // PC-ENUMERATION. Последовательность перечисляется ровно один раз, а
    // исключение во время перечисления остаётся отказом прежнего конверта и не
    // превращается в частичный успешный ответ. Два чтения ответа.
    [Fact]
    public async Task PcEnumeration_OneShotSequenceIsReadOnce_AndThrowingSequenceRefuses()
    {
        // Первый экзаменационный вход и его признаки отдельными литералами.
        var exam = KnnPredicateCarrierLearningFixture.Exams[0];

        // Одноразовая последовательность: второе перечисление само себя роняет.
        var oneShot = new OneShotPredicateSequence(PredicatesFor(exam.Id));
        var viaOneShot = await ReadAsync(exam.Input, true, oneShot);

        // Один проход достаточен, поэтому чтение успешно и ответ правильный.
        Assert.True(viaOneShot.IsSuccess, viaOneShot.ErrorMessage);
        Assert.Equal(exam.Route, viaOneShot.Value!.Output.Route);
        Assert.Equal(exam.Steps, viaOneShot.Value!.Output.Steps);

        // Прямое доказательство однократности: перечислили ровно один раз.
        Assert.Equal(1, oneShot.EnumerationCount);

        // Вторая последовательность обрывается исключением на втором элементе.
        var throwing = await ReadAsync(exam.Input, true, new ThrowingPredicateSequence());

        // Это прежний отказ конверта вывода, а не частичный успешный ответ.
        Assert.False(throwing.IsSuccess);
        Assert.Equal("KNN_LOGICAL_UNIT_INFERENCE_FAILED", throwing.ErrorCode);
    }

    // Признаки запроса отдельными литералами: эталонные шаги экзамена в
    // построение запроса не передаются, иначе эталон подсказывал бы ответ.
    private static string[] PredicatesFor(string examId)
    {
        // Первые два экзамена просят сначала осмотр, потом правку.
        if (string.Equals(examId, "e1", StringComparison.Ordinal)
            || string.Equals(examId, "e2", StringComparison.Ordinal))
            return ["review", "implement"];

        // Остальные два просят сначала правку, потом осмотр.
        return ["implement", "review"];
    }

    // Один штатный запрос к обученной памяти через обычный открытый вход.
    private Task<OperationResult<LogicalUnitResult<OrchestrationAnswer>>> ReadAsync(
        string input,
        bool keyPresent,
        object? predicateValue)
    {
        // Признак работы не отменён: ранние отказы проверяются отдельно.
        return _unit.ExecuteAsync(BuildRequest(input, keyPresent, predicateValue));
    }

    // Собрать конверт запроса с нужным носителем признаков или вовсе без ключа.
    private static LogicalUnitRequest<string> BuildRequest(
        string input,
        bool keyPresent,
        object? predicateValue)
    {
        // Метаданные собираются отдельно, чтобы отличить отсутствующий ключ от
        // присутствующего ключа со значением-пустышкой.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        if (keyPresent)
            metadata["predicate_ids"] = predicateValue!;

        // Признаки оценки низкой уверенности выключены, поэтому предупреждений нет.
        return new LogicalUnitRequest<string>(
            "run-p108-carrier",
            KnnPredicateCarrierLearningFixture.UnitId,
            KnnPredicateCarrierLearningFixture.UnitRole,
            KnnPredicateCarrierLearningFixture.DomainTag,
            input,
            new LogicalExecutionFlags(),
            metadata);
    }

    // Два ответа обязаны совпасть целиком: маршрут, шаги, ссылки, уверенность и
    // предупреждения. Именно здесь видна потеря признаков у другого носителя.
    private static void AssertSameAnswer(
        LogicalUnitResult<OrchestrationAnswer> expected,
        LogicalUnitResult<OrchestrationAnswer> actual,
        string because)
    {
        // Маршрут и порядок шагов совпадают.
        Assert.Equal(expected.Output.Route, actual.Output.Route);
        Assert.Equal(expected.Output.Steps, actual.Output.Steps);

        // Ссылки на доказательства указывают на того же соседа.
        Assert.Equal(expected.EvidenceIds, actual.EvidenceIds);

        // Уверенность — та же шкала близости: расхождение означает другой вектор
        // запроса, то есть потерянные признаки.
        Assert.True(
            expected.Confidence == actual.Confidence,
            $"{because}: уверенность {actual.Confidence} вместо {expected.Confidence} — вектор запроса отличается, признаки потеряны.");

        // Предупреждения и признаки выбранного соседа тоже совпадают.
        Assert.Equal(expected.Warnings, actual.Warnings);
        Assert.Equal(expected.PredicateIds, actual.PredicateIds);
    }

    // Отказ на элементе-пустышке называет код, место элемента и носитель.
    private static void AssertNullElementRefusal(
        OperationResult<LogicalUnitResult<OrchestrationAnswer>> result,
        int expectedIndex,
        string carrierName)
    {
        // Успешный ответ здесь был бы тихой потерей признаков.
        Assert.False(result.IsSuccess, $"{carrierName}: ожидался отказ, а чтение вернуло успешный ответ.");

        // Код отказа именованный, а не общий «сбой вывода».
        Assert.Equal("PREDICATE_IDS_NULL_ELEMENT", result.ErrorCode);

        // В сообщении названо место непригодного элемента.
        Assert.Contains($"index {expectedIndex}", result.ErrorMessage);
    }

    // Одноразовая последовательность признаков: считает перечисления и роняет
    // второе, поэтому повторное чтение входа скрыть невозможно.
    private sealed class OneShotPredicateSequence(IReadOnlyList<string> values) : IEnumerable<string>
    {
        // Сколько раз у последовательности просили перечислитель.
        public int EnumerationCount { get; private set; }

        // Выдать перечислитель и запомнить обращение.
        public IEnumerator<string> GetEnumerator()
        {
            // Каждое обращение учитывается до выдачи перечислителя.
            EnumerationCount++;

            // Второе обращение недопустимо: источник одноразовый.
            if (EnumerationCount > 1)
                throw new InvalidOperationException("Эта последовательность признаков одноразовая и уже прочитана.");

            // Значения отдаются как есть, без своей копии.
            return values.GetEnumerator();
        }

        // Старый необобщённый вход ведёт в тот же самый перечислитель.
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Последовательность, обрывающаяся исключением уже во время перечисления.
    private sealed class ThrowingPredicateSequence : IEnumerable<string>
    {
        // Перечислитель отдаёт первый элемент и падает на втором.
        public IEnumerator<string> GetEnumerator() => Iterate();

        // Источник признаков обрывается после первого значения.
        private static IEnumerator<string> Iterate()
        {
            // Первое значение ещё приходит нормально.
            yield return "review";

            // Второе значение получить уже нельзя: источник оборвался.
            throw new InvalidOperationException("Источник признаков оборвался на втором элементе.");
        }

        // Старый необобщённый вход ведёт в тот же самый перечислитель.
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
