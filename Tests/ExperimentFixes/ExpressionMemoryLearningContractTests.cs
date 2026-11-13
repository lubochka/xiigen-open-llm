// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие договора «память формул отвечает отказом, когда формулу не
// учили» — строка очереди переноса «нативная память формул и явное „не знаю“»
// (номер R32-EX в файле плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md).
//
// Что здесь проверяется по-человечески. В проекте есть маленький обучаемый
// механизм — память формул. Его учат целыми записями вида «Выручка минус
// Затраты равно Прибыль», он запоминает соответствие «левая часть → правая
// часть», а потом по левой части возвращает правую. Вопрос этой проверки
// простой: что он отвечает, когда такую формулу ему не показывали ни разу.
//
// Что было сломано. Сама память отвечала честно: «совпадения нет, значения
// нет». Но обёртка-исполнитель выбрасывала признак совпадения и всё равно
// возвращала вызывающей стороне УСПЕШНЫЙ результат, внутри которого лежало
// пустое значение. То есть «я не знаю такой формулы» и «я выучил такую формулу»
// приходили под одним и тем же успешным конвертом. Отличие было спрятано в поле
// ответа, которое по своему объявленному виду вообще не может быть пустым, и
// компилятору об этом затыкали рот восклицательным знаком.
//
// Чем это хуже честного отказа. Вызывающая сторона обязана заново изобретать
// признак «ответили ли мне», и каждый потребитель изобретает его по-своему.
// Именно так и поступил опыт двадцать девятый: он завёл собственные поля
// «ответил» и «значение пустое», потому что по результату самой единицы
// отличить незнание от выученного ответа было нельзя.
//
// Откуда взяты данные. Из опыта двадцать девятого
// (experiments-v2/RP01-MICRO/P2901) без изменений: четыре формулы, десять
// запросов, второе плечо с циклически сдвинутыми правыми частями.
//
// Бюджет. Два настоящих обучения по четыре урока — всего восемь уроков. Панель
// из десяти запросов на каждое плечо считается ОДИН раз на весь класс: двадцать
// обращений к исполнителю. Плюс одно отдельное обращение с запросом из пробелов,
// которое в панель не входит и посчитано отдельно. Итого 8 уроков и 21 чтение.
// Ни одного обращения к диску, ни одной нейронной правки весов, ни одной пары
// предпочтений, ни одной внешней модели.
//
// Чего это покрытие НЕ доказывает. Оно не доказывает, что механизм умеет
// выводить новые формулы: все восемь отвечаемых запросов совпадают с выученными
// левыми частями, и верный ответ здесь — точный возврат выученного. Обобщения на
// невиданную формулу здесь ноль, и плечо со сдвигом это прямо показывает.
// Цена нового конверта отказа здесь не измеряется.

using AgentInfrastructure;
using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests.ExperimentFixes;

// Одна строка панели: что спросили и что ответил исполнитель.
public sealed record ExpressionPanelRow(
    // Имя плеча: B — настоящие правые части, C — сдвинутые.
    string Arm,
    // Короткое имя запроса, повторяет имена из опыта двадцать девятого.
    string QueryId,
    // Вид запроса: точный, с другой расстановкой пробелов или неизвестный.
    string Kind,
    // Сам текст запроса.
    string Query,
    // Успешным ли пришёл конверт результата.
    bool IsSuccess,
    // Код отказа; у успешного конверта он пустой.
    string ErrorCode,
    // Значение ответа; у отказа его нет вовсе.
    string? Output,
    // Уверенность; читается только у успешного конверта.
    double? Confidence,
    // Сколько предупреждений принёс успешный конверт.
    int WarningsCount);

// Общая панель. xUnit создаёт её один раз на весь класс проверок, поэтому
// назначенный бюджет обучений и чтений расходуется ровно один раз.
public sealed class ExpressionMemoryPanel
{
    // Четыре формулы опыта с настоящими правыми частями.
    public static readonly string[] TrueLessons =
    [
        "Revenue - Cost = Profit",
        "Assets - Liabilities = Equity",
        "Price * Quantity = Revenue",
        "Profit / Revenue = Margin"
    ];

    // Те же четыре левые части, но правые части циклически сдвинуты на один шаг.
    // Это отрицательный контроль: если ответ берётся из выученной памяти, он
    // обязан смениться целиком.
    public static readonly string[] ShiftedLessons =
    [
        "Revenue - Cost = Equity",
        "Assets - Liabilities = Revenue",
        "Price * Quantity = Margin",
        "Profit / Revenue = Profit"
    ];

    // Десять запросов панели: четыре точных, четыре с другой расстановкой
    // пробелов и два про формулы, которых в уроках не было.
    private static readonly (string QueryId, string Kind, string Query)[] Queries =
    [
        ("q-exact-f1",   "exact",   "Revenue - Cost ="),
        ("q-exact-f2",   "exact",   "Assets - Liabilities ="),
        ("q-exact-f3",   "exact",   "Price * Quantity ="),
        ("q-exact-f4",   "exact",   "Profit / Revenue ="),
        ("q-format-f1",  "format",  "Revenue-Cost ="),
        ("q-format-f2",  "format",  "Assets-Liabilities ="),
        ("q-format-f3",  "format",  "Price*Quantity ="),
        ("q-format-f4",  "format",  "Profit/Revenue ="),
        ("q-unknown-u1", "unknown", "Revenue+Cost ="),
        ("q-unknown-u2", "unknown", "Price/Quantity =")
    ];

    // Ожидаемые правые части для восьми отвечаемых запросов плеча B, выписанные
    // буквально. Они не берутся из самого ответа, иначе проверка сравнивала бы
    // ответ сам с собой.
    public static readonly string[] ExpectedTrueAnswers =
    [
        "Profit", "Equity", "Revenue", "Margin",
        "Profit", "Equity", "Revenue", "Margin"
    ];

    // Панель плеча с настоящими правыми частями.
    public IReadOnlyList<ExpressionPanelRow> ArmB { get; }

    // Панель плеча со сдвинутыми правыми частями.
    public IReadOnlyList<ExpressionPanelRow> ArmC { get; }

    // Отдельное чтение запроса из одних пробелов на уже обученном плече B.
    public ExpressionPanelRow EmptyQueryRow { get; }

    // Сколько уроков фактически передано в обучение.
    public int LessonsUsed { get; }

    // Сколько раз фактически вызван исполнитель.
    public int ExecuteCalls { get; private set; }

    public ExpressionMemoryPanel()
    {
        // Первое настоящее обучение: четыре урока с правильными правыми частями.
        var armB = ExpressionCompletionUnit.Train(TrueLessons);
        Assert.True(armB.IsSuccess, armB.ErrorMessage);

        // Второе настоящее обучение: те же левые части, сдвинутые правые.
        var armC = ExpressionCompletionUnit.Train(ShiftedLessons);
        Assert.True(armC.IsSuccess, armC.ErrorMessage);

        // Восемь уроков суммарно — ровно назначенный планом бюджет обучения.
        LessonsUsed = TrueLessons.Length + ShiftedLessons.Length;

        // Десять чтений на первом плече.
        ArmB = BuildPanel("B", armB.Value!);

        // Десять чтений на втором плече.
        ArmC = BuildPanel("C", armC.Value!);

        // Одиннадцатое чтение первого плеча: запрос из одних пробелов. Считается
        // отдельно от панели и в назначенные двадцать чтений не входит.
        EmptyQueryRow = Execute("B", "q-blank", "blank", "   ", armB.Value!);

        // Назначенный бюджет израсходован ровно, без тихого расширения: восемь
        // уроков, двадцать чтений панели и одно отдельное чтение пустого запроса.
        Assert.Equal(8, LessonsUsed);
        Assert.Equal(21, ExecuteCalls);
    }

    // Прогоняет десять запросов панели через уже обученный исполнитель.
    private List<ExpressionPanelRow> BuildPanel(string arm, ExpressionCompletionUnit unit)
    {
        var rows = new List<ExpressionPanelRow>(Queries.Length);
        foreach (var (queryId, kind, query) in Queries)
            rows.Add(Execute(arm, queryId, kind, query, unit));
        return rows;
    }

    // Одно обращение к исполнителю; результат записывается как есть, без правки.
    private ExpressionPanelRow Execute(
        string arm,
        string queryId,
        string kind,
        string query,
        ExpressionCompletionUnit unit)
    {
        // Счётчик чтений ведётся явно, чтобы бюджет в отчёте был измеренным.
        ExecuteCalls++;

        // Конверт запроса собирается ровно так же, как его собирают соседние
        // проверки этой единицы.
        var request = new LogicalUnitRequest<string>(
            "transfer-expression-memory-refusal",
            ExpressionCompletionUnit.Id,
            ExpressionCompletionUnit.Role,
            ExpressionCompletionUnit.DomainTag,
            query,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

        // Исполнитель внутри синхронный, поэтому ожидание здесь детерминировано.
        var result = unit.ExecuteAsync(request).GetAwaiter().GetResult();

        // У отказа значения ответа нет вовсе, поэтому читаются только те поля,
        // которые у отказа действительно существуют.
        return new ExpressionPanelRow(
            arm,
            queryId,
            kind,
            query,
            result.IsSuccess,
            result.ErrorCode,
            result.IsSuccess ? result.Value!.Output : null,
            result.IsSuccess ? result.Value!.Confidence : null,
            result.IsSuccess ? result.Value!.Warnings.Length : 0);
    }
}

public sealed class ExpressionMemoryLearningContractTests
    : IClassFixture<ExpressionMemoryPanel>
{
    // Код отказа, назначенный планом переноса для случая «формулы нет в памяти».
    private const string NotInMemoryCode = "EXPRESSION_NOT_IN_MEMORY";

    private readonly ExpressionMemoryPanel _panel;

    public ExpressionMemoryLearningContractTests(ExpressionMemoryPanel panel)
        => _panel = panel;

    // ── Главная проверка переноса ────────────────────────────────────────────
    // Незнакомая формула обязана прийти отказом, а не успешным пустым ответом.
    [Fact]
    public void UnknownFormula_IsTypedRefusal()
    {
        var row = _panel.ArmB.Single(r => r.QueryId == "q-unknown-u1");

        Assert.False(
            row.IsSuccess,
            $"Формула '{row.Query}' в уроках не встречалась, поэтому результат " +
            "обязан быть отказом, а не успешным конвертом.");
        Assert.Null(row.Output);
    }

    // Отказ обязан нести читаемый код причины, а не просто быть неуспешным.
    [Fact]
    public void UnknownFormula_RefusalCarriesTypedCode()
    {
        var row = _panel.ArmB.Single(r => r.QueryId == "q-unknown-u1");

        Assert.Equal(NotInMemoryCode, row.ErrorCode);
    }

    // Запрос из одних пробелов — такое же незнание, с тем же кодом причины.
    [Fact]
    public void EmptyQuery_IsTypedRefusal()
    {
        var row = _panel.EmptyQueryRow;

        Assert.False(
            row.IsSuccess,
            "Запрос без формулы не даёт оснований для ответа и обязан быть отказом.");
        Assert.Equal(NotInMemoryCode, row.ErrorCode);
    }

    // Ни один успешный конверт не имеет права нести пустое значение ответа:
    // поле ответа в договоре объявлено как непустая строка.
    [Fact]
    public void NoSuccessfulAnswerIsNull()
    {
        var successfulButNull = _panel.ArmB
            .Concat(_panel.ArmC)
            .Count(r => r.IsSuccess && r.Output is null);

        Assert.Equal(0, successfulButNull);
    }

    // ── Контроли сохранности уже работающего умения ──────────────────────────
    // Восемь отвечаемых запросов плеча B дают выученные правые части.
    [Fact]
    public void TrainedAnswers_EightOfEight()
    {
        // Берутся только те запросы, формулы которых в уроках действительно были.
        var answerable = _panel.ArmB.Where(r => r.Kind != "unknown").ToList();

        Assert.Equal(8, answerable.Count);

        for (int i = 0; i < answerable.Count; i++)
        {
            var row = answerable[i];

            Assert.True(row.IsSuccess, $"Выученная формула '{row.Query}' не ответила.");
            Assert.Equal(ExpressionMemoryPanel.ExpectedTrueAnswers[i], row.Output);
            Assert.Equal(1.0, row.Confidence);
            Assert.Equal(0, row.WarningsCount);
        }
    }

    // Сдвинутые уроки меняют все восемь ответов: значит ответ берётся из
    // выученной памяти, а не из неизменной формулы внутри кода.
    [Fact]
    public void ShiftedLessons_ChangeEveryAnswer()
    {
        var trueArm    = _panel.ArmB.Where(r => r.Kind != "unknown").ToList();
        var shiftedArm = _panel.ArmC.Where(r => r.Kind != "unknown").ToList();

        // Совпадением считается одинаковый ответ на одном и том же запросе.
        int agreements = trueArm
            .Zip(shiftedArm, (b, c) => b.Output == c.Output)
            .Count(same => same);

        Assert.Equal(0, agreements);
    }
}
