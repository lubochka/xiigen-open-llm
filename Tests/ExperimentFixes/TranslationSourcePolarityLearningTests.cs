// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «сокращённое отрицание источника не получает
// уверенный противоположный перевод» (назначение «узкое вето знака
// английского высказывания» раздела 5 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 109-p53-p55-transfer-plan-2026-10-30.md; основание — опыт знака источника
// P55-PRIMARY-01, подслучай P55.1).
//
// Что здесь проверяется по-человечески. Переводчик запоминает пары
// «исходный текст — перевод» и на новый запрос берёт перевод самой похожей
// записи. Похожесть считается по набору значимых слов, а не по смыслу. У
// фразы «I can't open the door» набор значимых слов тот же, что у выученной
// записи «I can open the door»: сокращение «can't» распадается на «can» и
// односимвольный остаток, который отбрасывается. Поэтому на запрос «я НЕ могу
// открыть дверь» переводчик уверенно отдавал выученный перевод утвердительной
// записи — «Я могу открыть дверь». Это противоположный смысл, выданный как
// уверенно верный, и это измеренный след дефекта, а не предположение.
//
// Исправление, выбранное планом: перед допуском записи в память сравнивается
// знак английского высказывания запроса и записи. Знак считается по словам:
// сокращения приводятся к полной форме, слова отрицания-всегда («never»,
// «nothing», «nobody», «nowhere», «none») отделяются от простого отрицания
// («not», «cannot», «no»). Разный знак — запись не допускается. Два и более
// признака отрицания (двойное отрицание) права вето НЕ дают: решение
// отдаётся прежнему правилу связочных предложений.
//
// Три главные ловушки, которые здесь проверяются отдельно:
//   1. неоднозначность (двойное отрицание) не создаёт ложного отказа;
//   2. отрицание-всегда и простое отрицание — РАЗНЫЕ знаки, поэтому
//      «I never open the door» не совпадает с «I cannot open the door»;
//   3. знак считается только по ИСХОДНОЙ английской стороне. К русскому
//      целевому тексту этот счёт применять нельзя: русские «не» и «никогда»
//      в списках отсутствуют, поэтому любой русский перевод был бы
//      классифицирован как утверждение и дал бы ложный вывод.
//
// Два плеча измерения:
//   плечо A (база) — положительные контроли, которые обязаны держаться и до
//                    правки, и после неё: холодный отказ по всем четырём
//                    строкам, три точных совпадения источника, фактический
//                    маршрут главной строки, удержание трёх учебных строк и
//                    превышение предела постоянного ответа;
//   плечо B (кандидат) — та же приёмка с требуемым результатом: четыре из
//                    четырёх, включая главную строку, затем сохранение,
//                    свежая загрузка, снова четыре из четырёх и три проверки
//                    чувствительности на ввезённых копиях. Плечо B обязано
//                    падать НА ЗНАЧЕНИЯХ, пока знак источника у исполнителя
//                    выключен, и быть зелёным после включения.
//
// Показания каждого плеча пишутся в файл показаний ДО утверждений, в блоке
// finally, поэтому красный прогон плеча B всё равно оставляет измеренный след
// дефекта, а не обрывает измерение.
//
// Чего эти проверки НЕ доказывают. Они не дают общей теории отрицания и не
// обещают правильного смысла любого английского текста: закрыт ровно случай
// разного знака высказывания у запроса и у записи памяти в направлении с
// английского на русский. Наличие сокращения в словаре не равно его
// корпусной проверке: проверены четыре фразы, а словарь шире. Область
// действия отрицания («not only ... but also»), другие языки, остальные
// варианты сокращений и сам адрес поиска по набору слов остаются за
// пределами доказательства. Свежая загрузка выполняется новым объектом
// переводчика в том же процессе операционной системы, а не отдельным
// холодным процессом. Независимого проверяющего у этого файла в этой сессии
// не было.
namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

public sealed class TranslationSourcePolarityLearningTests
{
    // Имя назначения и номер попытки: по ним складывается путь показаний.
    private const string FixId = "FIX-P55-SOURCE-POLARITY";
    private const string AttemptId = "attempt-0001";

    // Источник корпуса учебных строк этой приёмки задан планом дословно.
    private const string SourceId = "r55-native-acceptance-v1";

    // Тема, направление и языки приёмки заданы планом: тема general, EN_RU.
    private const string Topic = "general";
    private const string SourceLang = "en";
    private const string TargetLang = "ru";
    private static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Вид отвергнутого варианта в учебных строках задан планом дословно.
    private const string RejectedType = "polarity_lost";

    // Назначенная версия поля знака высказывания. Сама строка живёт закрытой
    // постоянной внутри области перевода и из проверок не видна, поэтому здесь
    // она записывается в показания как назначенное значение, а совпадение с
    // продуктовой постоянной подтверждается чтением кода, не этой проверкой.
    private const string AssignedPolarityFieldVersion = "en-source-polarity-field-v2";

    // Прежняя версия правила связочных предложений: она обязана остаться
    // различимой и продолжать работать при выключенном знаке источника.
    private const string LegacyPolicyVersion = "en-simple-copular-polarity-v1";

    // Явная русская метка подмены выученного перевода для пробы
    // чувствительности: её появление в ответе доказывает, что ответ собран из
    // памяти, а не восстановлен счётчиком похожести.
    private const string SentinelTarget = "Я подменённый ответ памяти";

    // Три урока приёмки: исходный текст, правильный перевод, словарь слов и
    // отвергнутый вариант. Данные взяты дословно из карточки опыта знака
    // источника. Словарь слов обязателен: без него меняется и сам набор
    // данных, и путь отката, поэтому убирать его ради «чистого» теста нельзя.
    private static readonly Lesson[] Lessons =
    [
        new(
            "R55-L1",
            "I can open the door",
            "Я могу открыть дверь",
            [("I", "я"), ("can", "могу"), ("open", "открыть"), ("door", "дверь")],
            "Я не могу открыть дверь"),
        new(
            "R55-L2",
            "I cannot open the door",
            "Я не могу открыть дверь",
            [("I", "я"), ("cannot", "не"), ("can", "могу"), ("open", "открыть"), ("door", "дверь")],
            "Я могу открыть дверь"),
        new(
            "R55-L3",
            "I never open the door",
            "Я никогда не открываю дверь",
            [("I", "я"), ("never", "никогда"), ("open", "открываю"), ("door", "дверь")],
            "Я всегда открываю дверь")
    ];

    // Панель четырёх запросов. Эталон задан заранее и в ответ не подставляется.
    // Главная строка фактора — R2: именно она меняется от правки.
    private static readonly PanelRow[] Panel =
    [
        new("R1", "I can open the door", "Я могу открыть дверь",
            "экзамен: утвердительный знак, точное совпадение источника"),
        new("R2", "I can't open the door", "Я не могу открыть дверь",
            "экзамен, ГЛАВНАЯ СТРОКА ФАКТОРА: сокращённое отрицание без точного совпадения источника"),
        new("R3", "I never open the door", "Я никогда не открываю дверь",
            "экзамен: отрицание-всегда, точное совпадение источника"),
        new("R4", "I cannot open the door", "Я не могу открыть дверь",
            "контроль того же исходного текста: полная форма отрицания")
    ];

    // Удержание: три учебные строки читаются обратно отдельной стадией.
    private static readonly PanelRow[] Retention =
    [
        new("T1", "I can open the door", "Я могу открыть дверь", "удержание первого урока"),
        new("T2", "I cannot open the door", "Я не могу открыть дверь", "удержание второго урока"),
        new("T3", "I never open the door", "Я никогда не открываю дверь", "удержание третьего урока")
    ];

    // Прежняя матрица допуска из показаний опыта: двенадцать строк «запрос —
    // запись». Ожидание записано как «допущена» или «отклонена» при
    // ВКЛЮЧЁННОМ знаке источника.
    private static readonly LawProbe[] AdmissionMatrix =
    [
        new("M01", "I can open the door", "I can open the door", true, "утверждение против утверждения"),
        new("M02", "I can open the door", "I cannot open the door", false, "утверждение против простого отрицания"),
        new("M03", "I can open the door", "I never open the door", false, "утверждение против отрицания-всегда"),
        new("M04", "I can't open the door", "I can open the door", false, "сокращённое отрицание против утверждения"),
        new("M05", "I can't open the door", "I cannot open the door", true, "сокращение и полная форма — один знак"),
        new("M06", "I can't open the door", "I never open the door", false, "простое отрицание против отрицания-всегда"),
        new("M07", "I never open the door", "I can open the door", false, "отрицание-всегда против утверждения"),
        new("M08", "I never open the door", "I cannot open the door", false, "отрицание-всегда против простого отрицания"),
        new("M09", "I never open the door", "I never open the door", true, "отрицание-всегда против себя же"),
        new("M10", "I cannot open the door", "I can open the door", false, "простое отрицание против утверждения"),
        new("M11", "I cannot open the door", "I cannot open the door", true, "простое отрицание против себя же"),
        new("M12", "I cannot open the door", "I never open the door", false, "простое отрицание против отрицания-всегда")
    ];

    // Пять краевых случаев из показаний опыта: имена сохранены дословно.
    private static readonly LawProbe[] EdgeCases =
    [
        new("E1-never-vs-not", "I never open the door", "I cannot open the door", false,
            "отрицание-всегда не подменяется простым отрицанием"),
        new("E2-cant-equals-cannot", "I can't open the door", "I cannot open the door", true,
            "сокращение can't и полная форма cannot равнозначны в этом наборе"),
        new("E3-double-negation-abstains", "I cannot never open the door", "I can open the door", true,
            "двойное отрицание требует контекстного эталона: знак права вето не даёт"),
        new("E4-known-copular-clause-preserved", "the box is not empty", "the box is empty", false,
            "прежнее узкое правило связочных предложений не сломано"),
        new("E5-same-polarity-positive-control", "I can open the door", "I can open the door", true,
            "одинаковый утвердительный знак не создаёт ложного отказа")
    ];

    // Три проверки ВЫКЛЮЧЕННОГО знака источника: прежний допуск по умолчанию
    // обязан остаться ровно прежним для всех нынешних вызывающих.
    private static readonly LawProbe[] LegacyDefaultProbes =
    [
        new("D1-cant-vs-cannot-default", "I can't open the door", "I cannot open the door", true,
            "по умолчанию запись допускается: прежний закон этих фраз не узнаёт"),
        new("D2-never-vs-cannot-default", "I never open the door", "I cannot open the door", true,
            "по умолчанию запись допускается — это и есть прежнее поведение, которое новый знак меняет только по включению"),
        new("D3-copular-veto-default", "the box is not empty", "the box is empty", false,
            "прежнее вето связочного предложения продолжает работать без включения знака")
    ];

    // Настройки записи учебных текстов JSON повторяют настройки опыта:
    // без отступов, с безопасным для кириллицы кодировщиком и строковыми
    // значениями перечислений.
    private static readonly JsonSerializerOptions UnitJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки устойчивого отпечатка состояния: без отступов, чтобы отпечаток
    // не зависел от переносов строк.
    private static readonly JsonSerializerOptions StableJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки записи файла показаний: с отступами, чтобы человек читал его
    // глазами без дополнительной обработки.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Плечо A: положительные контроли, зелёные до правки и после неё ────────
    //
    // Назначенные дозы плеча A: 1 вызов продуктового обучения партии,
    // 3 назначенные строки, 3 настоящих успешных урока, 11 обычных чтений
    // (4 холодных + 4 панели + 3 удержания).
    //
    // Это плечо НЕ утверждает правильность главной строки фактора: до правки
    // она отвечает противоположным смыслом. Оно утверждает ровно то, что
    // обязано держаться в обоих состояниях продукта, и записывает фактические
    // ответы, маршруты и вердикты всех четырёх строк.
    [Fact]
    public void BaselineArm_PreservedPositiveControls_HoldBeforeAndAfterFix()
    {
        // Журнал показаний плеча заполняется по ходу и пишется в finally,
        // поэтому измерение сохраняется даже при падении утверждений.
        var meter = new ArmMeter("A", "база: положительные контроли, обязательные в обоих состояниях");

        try
        {
            // Подготовка: создание переводчика и загрузка его зависимостей.
            var machine = meter.Measure(
                "подготовка: структуры данных и зависимости",
                () => new TranslationMachine());

            // Холодная панель: до обучения все четыре строки обязаны остаться
            // в исходном отказе, то есть в обёртке заготовки.
            var cold = meter.Measure(
                "холодная панель (4 чтения)",
                () => ReadRows(machine, Panel));
            meter.RecordPanel("холодная панель", cold);
            Assert.Equal(0, cold.Count(answer => answer.ExactCorrect));
            Assert.All(cold, answer => Assert.Contains("BOOTSTRAP", answer.Answer, StringComparison.Ordinal));

            // Настоящее обучение: одна партия из трёх назначенных уроков.
            var trained = meter.Measure(
                "обучение: одна партия (3 строки)",
                () => TrainBatch(machine, "r55-native-acceptance-001", BuildExamples()));
            meter.RecordBatch("учебная партия приёмки", trained, assignedRows: 3);
            Assert.True(trained.IsSuccess, trained.ErrorMessage);
            Assert.Equal(3, trained.Value!.RowsTrained);

            // Панель после обучения: фактические ответы записываются целиком.
            var panel = meter.Measure(
                "панель после обучения (4 чтения)",
                () => ReadRows(machine, Panel));
            meter.RecordPanel("панель после обучения", panel);

            // Положительный контроль 1: три строки с точным совпадением
            // источника обязаны отвечать верно и идти маршрутом точного
            // совпадения. Эти три строки правка не трогает.
            foreach (var rowId in new[] { "R1", "R3", "R4" })
            {
                var answer = panel.Single(candidate => candidate.RowId == rowId);
                Assert.True(
                    answer.ExactCorrect,
                    $"строка {rowId} «{answer.Source}»: ожидалось «{answer.Gold}», получено «{answer.Answer}»");
                Assert.Contains("-exact-source", answer.AdapterId, StringComparison.Ordinal);
            }

            // Положительный контроль 2: главная строка фактора действительно
            // доходит до обычного допуска по похожести и отвечает ИЗ ПАМЯТИ, а
            // не обёрткой заготовки. Правильность её ответа здесь не
            // утверждается: до правки он противоположен по смыслу.
            var mainRow = panel.Single(candidate => candidate.RowId == "R2");
            Assert.Contains("-ordinary-source-knn", mainRow.AdapterId, StringComparison.Ordinal);
            Assert.DoesNotContain("BOOTSTRAP", mainRow.Answer);
            Assert.NotEqual("TRANSLATION_LOW_CONFIDENCE", mainRow.Verdict);
            Assert.Contains(
                mainRow.Answer.Trim(),
                Lessons.Select(lesson => lesson.TargetText).ToArray(),
                StringComparer.OrdinalIgnoreCase);

            // Положительный контроль 3: удержание трёх учебных строк.
            var retention = meter.Measure(
                "удержание трёх учебных строк (3 чтения)",
                () => ReadRows(machine, Retention));
            meter.RecordPanel("удержание трёх учебных строк", retention);
            AssertWholeSetAnswered(retention, Retention);

            // Положительный контроль 4: предел постоянного ответа превышен.
            // Лучший один и тот же ответ закрывает 2 строки из 4; панель
            // обязана давать больше, иначе измерение ничего не значит.
            var ceiling = ConstantAnswerCeiling();
            meter.RecordConstantCeiling(ceiling);
            Assert.Equal(2, ceiling.Hits);
            Assert.True(
                panel.Count(answer => answer.ExactCorrect) > ceiling.Hits,
                $"панель не превысила предел постоянного ответа: верных {panel.Count(answer => answer.ExactCorrect)} из {Panel.Length}, предел {ceiling.Hits}");

            // Фактические дозы плеча сверяются с назначенными планом.
            meter.AssertAssignedDoses(batchCalls: 1, assignedRows: 3, successfulLessons: 3, ordinaryReads: 11);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Плечо B: кандидат, требуемый результат переноса ───────────────────────
    //
    // Назначенные дозы плеча B: 1 вызов продуктового обучения партии,
    // 3 назначенные строки, 3 настоящих успешных урока, 18 обычных чтений
    // (4 холодных + 4 панели + 3 удержания + 4 панели после загрузки +
    // 3 удержания после загрузки) и ОТДЕЛЬНО 3 чтения чувствительности.
    //
    // Пока знак источника у исполнителя выключен, это плечо обязано падать на
    // значениях главной строки: выученный перевод утвердительной записи
    // выдаётся на запрос с отрицанием.
    [Fact]
    public void CandidateArm_ContractedNegationKeepsNegativeMeaning_AndSurvivesFreshLoad()
    {
        // Журнал показаний плеча B.
        var meter = new ArmMeter("B", "кандидат: узкое вето знака английского источника");

        // Путь контрольной точки принадлежит показаниям этого плеча.
        var checkpointPath = Path.Combine(ReceiptDirectory(), "candidate-checkpoint.json");

        try
        {
            // Подготовка: создание переводчика и загрузка его зависимостей.
            var machine = meter.Measure(
                "подготовка: структуры данных и зависимости",
                () => new TranslationMachine());

            // Холодная панель: исходный отказ по всем четырём строкам.
            var cold = meter.Measure(
                "холодная панель (4 чтения)",
                () => ReadRows(machine, Panel));
            meter.RecordPanel("холодная панель", cold);
            Assert.Equal(0, cold.Count(answer => answer.ExactCorrect));

            // Настоящее обучение: та же одна партия из трёх уроков.
            var trained = meter.Measure(
                "обучение: одна партия (3 строки)",
                () => TrainBatch(machine, "r55-native-acceptance-001", BuildExamples()));
            meter.RecordBatch("учебная партия приёмки", trained, assignedRows: 3);
            Assert.True(trained.IsSuccess, trained.ErrorMessage);
            Assert.Equal(3, trained.Value!.RowsTrained);

            // Панель после обучения: требуется четыре верных ответа из четырёх.
            var panel = meter.Measure(
                "панель после обучения (4 чтения)",
                () => ReadRows(machine, Panel));
            meter.RecordPanel("панель после обучения", panel);

            // Маршрут главной строки обязан быть настоящим: ответ приходит
            // через обычный допуск по похожести, а не через точное совпадение
            // источника и не из обёртки заготовки.
            var mainRow = panel.Single(candidate => candidate.RowId == "R2");
            Assert.Contains("-ordinary-source-knn", mainRow.AdapterId, StringComparison.Ordinal);
            Assert.DoesNotContain("BOOTSTRAP", mainRow.Answer);

            // Четыре из четырёх, включая главную строку фактора.
            AssertWholeSetAnswered(panel, Panel);

            // Удержание трёх учебных строк: три из трёх.
            var retention = meter.Measure(
                "удержание трёх учебных строк (3 чтения)",
                () => ReadRows(machine, Retention));
            meter.RecordPanel("удержание трёх учебных строк", retention);
            AssertWholeSetAnswered(retention, Retention);

            // Предел постоянного ответа превышен с запасом: 4 из 4 против 2.
            var ceiling = ConstantAnswerCeiling();
            meter.RecordConstantCeiling(ceiling);
            Assert.True(
                panel.Count(answer => answer.ExactCorrect) > ceiling.Hits,
                "панель не превысила предел постоянного ответа");

            // Фактическое принятое состояние снимается продуктовым выгрузчиком:
            // именно его копии правятся в пробах чувствительности.
            var accepted = machine.ExportCheckpoint();
            Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
            var acceptedCheckpoint = accepted.Value!;
            var acceptedSha = StableSha(acceptedCheckpoint);

            // Одно сохранение штатной контрольной точки.
            var saved = meter.Measure(
                "сохранение контрольной точки",
                () => machine.SaveCheckpointAsync(checkpointPath).GetAwaiter().GetResult());
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            meter.RecordCheckpointBytes(checkpointPath);

            // Одна свежая загрузка: новый объект переводчика читает файл с диска
            // и не выполняет ни одного обучения. Это не отдельный холодный
            // процесс операционной системы — записано честно.
            var loaded = meter.Measure(
                "свежая загрузка контрольной точки новым объектом переводчика",
                () => TranslationMachine.LoadCheckpointAsync(checkpointPath).GetAwaiter().GetResult());
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            var reloaded = loaded.Value!.Machine;

            // Панель после свежей загрузки: те же четыре из четырёх без обучения.
            var panelAfterLoad = meter.Measure(
                "панель после свежей загрузки (4 чтения)",
                () => ReadRows(reloaded, Panel));
            meter.RecordPanel("панель после свежей загрузки", panelAfterLoad);
            Assert.Contains(
                "-ordinary-source-knn",
                panelAfterLoad.Single(candidate => candidate.RowId == "R2").AdapterId,
                StringComparison.Ordinal);
            AssertWholeSetAnswered(panelAfterLoad, Panel);

            // Удержание после свежей загрузки: три из трёх.
            var retentionAfterLoad = meter.Measure(
                "удержание после свежей загрузки (3 чтения)",
                () => ReadRows(reloaded, Retention));
            meter.RecordPanel("удержание после свежей загрузки", retentionAfterLoad);
            AssertWholeSetAnswered(retentionAfterLoad, Retention);

            // Назначенные дозы самой приёмки сверяются до проб чувствительности.
            meter.AssertAssignedDoses(batchCalls: 1, assignedRows: 3, successfulLessons: 3, ordinaryReads: 18);

            // ── Три пробы чувствительности на ввезённых копиях ─────────────────
            //
            // Живое принятое состояние не портится: каждая проба ввозит КОПИЮ
            // фактической контрольной точки в отдельный новый переводчик.
            // Обучение в пробах не вызывается, копии не публикуются как новое
            // принятое состояние. Главная строка читается по одному разу.
            var negativeLesson = Lessons.Single(lesson => lesson.LessonId == "R55-L2");
            var mainQuery = Panel.Single(row => row.RowId == "R2");

            // Проба 1: удалить пары второго урока и его запись точного
            // совпадения. Правильный ответ обязан исчезнуть: иначе он берётся
            // не из памяти, и это провал изоляции.
            var probeOneCheckpoint = RemoveLessonFromCopy(acceptedCheckpoint, negativeLesson, out var removedPairs, out var removedExact);
            Assert.Equal(3, removedPairs);
            Assert.Equal(1, removedExact);
            var probeOne = meter.MeasureSensitivity(
                "проба 1: удалены пары второго урока и его точное совпадение",
                () => ReadSingleRow(ImportCopy(probeOneCheckpoint), mainQuery));
            meter.RecordSensitivity("проба 1: удалённая память второго урока", probeOne, removedPairs, removedExact);
            Assert.False(
                probeOne.ExactCorrect,
                $"правильный ответ возник при удалённой памяти второго урока — провал изоляции: «{probeOne.Answer}»");

            // Проба 2: заменить переводы тех же трёх пар явной русской меткой.
            // Ответ обязан стать меткой: это и доказывает, что он собран из
            // выученного значения, а не восстановлен счётчиком похожести.
            var probeTwoCheckpoint = ReplaceLessonTargetInCopy(acceptedCheckpoint, negativeLesson, SentinelTarget, out var replacedPairs);
            Assert.Equal(3, replacedPairs);
            var probeTwo = meter.MeasureSensitivity(
                "проба 2: переводы тех же пар заменены явной русской меткой",
                () => ReadSingleRow(ImportCopy(probeTwoCheckpoint), mainQuery));
            meter.RecordSensitivity("проба 2: подменённое выученное значение", probeTwo, replacedPairs, 0);
            Assert.False(
                probeTwo.ExactCorrect,
                $"правильный ответ возник при подменённом выученном значении — провал изоляции: «{probeTwo.Answer}»");
            Assert.Contains(SentinelTarget, probeTwo.Answer, StringComparison.Ordinal);

            // Проба 3: очистить словари и пары целиком. Ответ обязан перестать
            // быть переводом из памяти.
            var probeThreeCheckpoint = ClearLearnedMemoryInCopy(acceptedCheckpoint);
            var probeThree = meter.MeasureSensitivity(
                "проба 3: словари и пары очищены",
                () => ReadSingleRow(ImportCopy(probeThreeCheckpoint), mainQuery));
            meter.RecordSensitivity("проба 3: пустая память", probeThree, 0, 0);
            Assert.False(
                probeThree.ExactCorrect,
                $"правильный ответ возник при пустой памяти — провал изоляции: «{probeThree.Answer}»");

            // Живое принятое состояние после трёх проб обязано остаться тем же.
            var liveAgain = machine.ExportCheckpoint();
            Assert.True(liveAgain.IsSuccess, liveAgain.ErrorMessage);
            Assert.Equal(acceptedSha, StableSha(liveAgain.Value!));

            // Назначенные дозы чувствительности: ноль партий, ноль уроков,
            // три чтения главной строки.
            meter.AssertSensitivityDoses(sensitivityReads: 3);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Двадцать чистых проверок закона допуска ───────────────────────────────
    //
    // Назначенные дозы: 20 вызовов допуска, 0 уроков, 0 обычных чтений потока.
    //
    // Честная оговорка об охвате вызова. Сам закон источника закрыт внутри
    // области перевода, поэтому из проверок он недостижим напрямую. Единственный
    // продуктовый вызывающий закона — каскад поиска похожих пар, и проверки
    // идут через него: в хранилище кладётся РОВНО ОДНА запись под адресом
    // запроса, поэтому непустой ответ означает «запись допущена», а пустой —
    // «запись отклонена». Различие между «не распознано» и «совпало» этим
    // способом не наблюдается; оба исхода означают допуск, и в показаниях
    // ожидание записано именно как допуск или отказ.
    [Fact]
    public void SourceLaw_TwentyPureAdmissionProbes_MatchExperimentMatrixAndKeepLegacyDefault()
    {
        // Накопитель показаний чистых проверок.
        var records = new List<object>();
        var calls = 0;

        // Двенадцать строк прежней матрицы допуска при ВКЛЮЧЁННОМ знаке.
        foreach (var probe in AdmissionMatrix)
        {
            var admitted = ProbeAdmission(probe.Query, probe.Candidate, enableEnglishPolarityField: true);
            calls++;
            records.Add(DescribeProbe("матрица допуска", probe, admitted, fieldEnabled: true));
            Assert.Equal(probe.ExpectedAdmitted, admitted);
        }

        // Пять краевых случаев из показаний опыта при ВКЛЮЧЁННОМ знаке.
        foreach (var probe in EdgeCases)
        {
            var admitted = ProbeAdmission(probe.Query, probe.Candidate, enableEnglishPolarityField: true);
            calls++;
            records.Add(DescribeProbe("краевые случаи опыта", probe, admitted, fieldEnabled: true));
            Assert.Equal(probe.ExpectedAdmitted, admitted);
        }

        // Три проверки ВЫКЛЮЧЕННОГО знака: вызов идёт БЕЗ нового параметра,
        // то есть ровно так, как его делают все нынешние продуктовые вызывающие.
        foreach (var probe in LegacyDefaultProbes)
        {
            var admitted = ProbeAdmissionWithDefault(probe.Query, probe.Candidate);
            calls++;
            records.Add(DescribeProbe("прежний допуск по умолчанию", probe, admitted, fieldEnabled: false));
            Assert.Equal(probe.ExpectedAdmitted, admitted);
        }

        // Назначенное число вызовов обязано совпасть с фактическим.
        Assert.Equal(20, calls);

        // Показания чистых проверок пишутся отдельным файлом.
        WriteReceipt("law-probes-receipt.json", new
        {
            Назначение = FixId,
            Попытка = AttemptId,
            ЧтоЭтоЗаПроверки = "чистые проверки закона допуска: ни одного урока и ни одного чтения потока перевода",
            ВремяЗаписиUtc = DateTimeOffset.UtcNow,
            НазначеннаяВерсияЗнакаИсточника = AssignedPolarityFieldVersion,
            ПрежняяВерсияПравилаСвязочныхПредложений = LegacyPolicyVersion,
            ЧестноеОграничениеОхвата = new[]
            {
                "закон закрыт внутри области перевода: проверки идут через его единственного продуктового вызывающего",
                "наблюдается допуск или отказ; «не распознано» и «совпало» этим способом не различаются",
                "версия знака источника записана как назначенная: закрытая постоянная из проверок не видна"
            },
            ФактическиеДозы = new
            {
                ВызововДопуска = calls,
                Уроков = 0,
                ОбычныхЧтенийПотока = 0
            },
            Проверки = records
        });
    }

    // ── Утверждения наборов ответов ───────────────────────────────────────────

    // Все строки набора обязаны быть переведены точно.
    private static void AssertWholeSetAnswered(IReadOnlyList<PanelAnswer> answers, PanelRow[] rows)
    {
        // Каждая строка проверяется отдельно, чтобы сообщение о падении
        // называло конкретный фактический ответ и маршрут.
        foreach (var row in rows)
        {
            var answer = answers.Single(candidate => candidate.RowId == row.RowId);
            Assert.True(
                answer.ExactCorrect,
                $"строка {row.RowId} «{row.Source}» ({row.Meaning}): ожидалось «{row.Gold}», получено «{answer.Answer}», маршрут {answer.AdapterId}, вердикт {answer.Verdict}");
        }

        // Итоговый счёт набора назван явно.
        Assert.Equal(rows.Length, answers.Count(answer => answer.ExactCorrect));
    }

    // Предел постоянного ответа: лучший один и тот же ответ на всю панель.
    private static ConstantCeiling ConstantAnswerCeiling()
    {
        // Ответы группируются по эталону, берётся самая частая группа.
        var best = Panel
            .GroupBy(row => row.Gold, StringComparer.Ordinal)
            .Select(group => new { Answer = group.Key, Hits = group.Count() })
            .OrderByDescending(entry => entry.Hits)
            .First();

        // Предел выражается и числом строк, и долей от всей панели.
        return new ConstantCeiling(best.Answer, best.Hits, Panel.Length, best.Hits / (double)Panel.Length);
    }

    // ── Учебные строки и партии ───────────────────────────────────────────────

    // Построение трёх учебных строк приёмки из назначенных уроков.
    private static IReadOnlyList<TranslationLearningExample> BuildExamples()
    {
        // Один разборщик на все три строки: подпись и понятия берутся у него.
        var tokenizer = new TranslationSemanticTokenizer();

        // Строки собираются по порядку назначенных уроков.
        var examples = new List<TranslationLearningExample>(Lessons.Length);
        foreach (var lesson in Lessons)
        {
            // Подпись берётся у настоящего разборщика, а не придумывается.
            var sequence = tokenizer.Tokenize(lesson.SourceText, SourceLang);

            // Понятие каждого слова берётся у настоящего разборщика: ищется
            // токен с тем же нормализованным ключом, и берётся его понятие.
            var wordMappings = new List<ExecutorWordMapping>(lesson.Words.Count);
            foreach (var (sourceWord, targetWord) in lesson.Words)
            {
                var token = sequence.Tokens.FirstOrDefault(candidate =>
                    string.Equals(
                        FixtureMapKey(candidate.Word),
                        FixtureMapKey(sourceWord),
                        StringComparison.OrdinalIgnoreCase));
                var concept = token?.Concept ?? SemanticConcept.UNKNOWN;
                wordMappings.Add(new ExecutorWordMapping(sourceWord, concept, targetWord));
            }

            // Правильный ответ несёт словарь слов; выражения оборотов пусты.
            var chosen = new ExecutorOutput(lesson.TargetText, 0.95, wordMappings, null);

            // Отвергнутый вариант несёт противоположный смысл и пояснение.
            var rejected = new ExecutorOutput(
                lesson.WrongTargetText + " [WRONG: знак высказывания потерян - отрицание источника не сохранено - читатель отдаёт противоположный смысл]",
                0.20,
                null,
                null);

            // Строка собирается ровно по форме учебной записи предметной области.
            examples.Add(new TranslationLearningExample(
                ExampleId: lesson.LessonId,
                UnitId: TranslationUnitIds.ExecutorFor(Direction),
                FlowId: TranslationRoutingFlow.FlowId,
                InputJson: JsonSerializer.Serialize(new ExecutorInput(lesson.SourceText, SourceLang, Topic), UnitJson),
                ChosenOutputJson: JsonSerializer.Serialize(chosen, UnitJson),
                RejectedOutputJson: JsonSerializer.Serialize(rejected, UnitJson),
                RejectedType: RejectedType,
                SemanticSignature: sequence.Signature,
                SourceId: SourceId,
                HeldOut: false));
        }

        return examples;
    }

    // Вызов продуктового обучения партии. Исключение не скрывается, а
    // записывается как наблюдаемый исход.
    private static BatchOutcome TrainBatch(
        TranslationMachine machine,
        string batchId,
        IReadOnlyList<TranslationLearningExample> examples)
    {
        // Партия собирается из переданных строк с общим источником корпуса.
        var batch = new TranslationLearningBatch(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: [SourceId]);

        try
        {
            // Продуктовый путь обучения вызывается напрямую, без копий обучаемого.
            var result = machine.TrainStructuredBatchAsync(batch).GetAwaiter().GetResult();
            return new BatchOutcome(
                IsSuccess: result.IsSuccess,
                ErrorCode: result.ErrorCode,
                ErrorMessage: result.ErrorMessage,
                Value: result.Value,
                ThrewException: false,
                ExceptionText: "");
        }
        catch (Exception ex)
        {
            // Отказ разборщика записывается как наблюдаемый исход, не как падение.
            return new BatchOutcome(
                IsSuccess: false,
                ErrorCode: "",
                ErrorMessage: "",
                Value: null,
                ThrewException: true,
                ExceptionText: $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ── Обычные чтения через продуктовый поток ────────────────────────────────

    // Чтение набора строк через обычного потребителя: оркестратор переводчика.
    private static IReadOnlyList<PanelAnswer> ReadRows(TranslationMachine machine, PanelRow[] rows)
    {
        // Оркестратор берётся продуктовым способом, а не собирается в обход.
        var orchestratorResult = machine.GetOrchestratorAsync().GetAwaiter().GetResult();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        var orchestrator = orchestratorResult.Value!;

        // Ответы собираются по порядку строк набора.
        var answers = new List<PanelAnswer>(rows.Length);
        foreach (var row in rows)
            answers.Add(AskOrdinaryConsumer(orchestrator, row));

        return answers;
    }

    // Чтение одной строки через того же обычного потребителя.
    private static PanelAnswer ReadSingleRow(TranslationMachine machine, PanelRow row)
    {
        // Отдельное чтение использует тот же продуктовый путь, что и набор.
        var orchestratorResult = machine.GetOrchestratorAsync().GetAwaiter().GetResult();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        return AskOrdinaryConsumer(orchestratorResult.Value!, row);
    }

    // Один запрос обычного потребителя: текст ответа, маршрут, вердикт, уверенность.
    private static PanelAnswer AskOrdinaryConsumer(ILogicalOrchestrator orchestrator, PanelRow row)
    {
        // Запрос строится ровно так, как его строит продуктовый вызов.
        var request = new LogicalOrchestratorRequest(
            RunId: $"r55-{row.RowId}-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: row.Source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLang,
                ["target_language"] = TargetLang,
                ["topic_domain"] = Topic
            });

        // Поток исполняется продуктовым оркестратором.
        var run = orchestrator.RunAsync(request).GetAwaiter().GetResult();
        Assert.True(run.IsSuccess, run.ErrorMessage);
        var value = run.Value!;

        // Служебные показания ответа читаются из метрик потока.
        var metrics = value.FlowResult?.Metrics ?? new Dictionary<string, string>(StringComparer.Ordinal);
        var adapterId = metrics.TryGetValue("adapter_id", out var adapter) ? adapter : "не сообщено";
        var confidence = metrics.TryGetValue("confidence", out var raw) &&
            double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0.0;

        // Эталон в ответ не подставляется: сравнивается фактический текст.
        var answerText = value.FinalHumanResult ?? string.Empty;
        return new PanelAnswer(
            RowId: row.RowId,
            Source: row.Source,
            Gold: row.Gold,
            Meaning: row.Meaning,
            Answer: answerText,
            AdapterId: adapterId,
            Verdict: value.OverallVerdict,
            Confidence: confidence,
            ExactCorrect: string.Equals(answerText.Trim(), row.Gold, StringComparison.OrdinalIgnoreCase));
    }

    // ── Чистая проверка допуска ───────────────────────────────────────────────

    // Одна проверка допуска при явно ВКЛЮЧЁННОМ знаке источника.
    private static bool ProbeAdmission(string query, string candidate, bool enableEnglishPolarityField)
    {
        // Хранилище получает ровно одну запись под адресом запроса.
        var store = BuildSingleEntryStore(query, candidate, out var tokens);

        // Адреса более общих уровней намеренно пусты: так пустой ответ
        // означает именно отказ закона, а не находку другого уровня.
        var found = store.FindNearestCascade(
            tokens.Signature,
            string.Empty,
            string.Empty,
            1,
            query,
            enableEnglishPolarityField);

        // Непустой ответ означает допуск записи.
        return found.Count > 0;
    }

    // Та же проверка, но БЕЗ нового параметра: ровно так вызывают все нынешние
    // продуктовые вызывающие, поэтому она измеряет прежний допуск по умолчанию.
    private static bool ProbeAdmissionWithDefault(string query, string candidate)
    {
        // Хранилище получает ровно одну запись под адресом запроса.
        var store = BuildSingleEntryStore(query, candidate, out var tokens);

        // Вызов без нового параметра: прежняя подпись, прежний смысл.
        var found = store.FindNearestCascade(tokens.Signature, string.Empty, string.Empty, 1, query);

        // Непустой ответ означает допуск записи.
        return found.Count > 0;
    }

    // Хранилище с единственной записью под адресом запроса.
    private static TranslationKnnStore BuildSingleEntryStore(
        string query,
        string candidate,
        out SemanticTokenSequence tokens)
    {
        // Адрес берётся у настоящего разборщика по тексту запроса.
        tokens = new TranslationSemanticTokenizer().Tokenize(query, SourceLang);

        // Целевой текст записи для чистой проверки допуска значения не имеет:
        // проверяется сам допуск, а не качество перевода.
        var store = new TranslationKnnStore();
        store.AddPair(tokens.Signature, candidate, "целевой текст для чистой проверки допуска");
        return store;
    }

    // Человеческое описание одной чистой проверки для файла показаний.
    private static object DescribeProbe(string group, LawProbe probe, bool admitted, bool fieldEnabled)
        => new
        {
            Группа = group,
            Код = probe.ProbeId,
            Запрос = probe.Query,
            ЗаписьПамяти = probe.Candidate,
            ЗнакИсточникаВключён = fieldEnabled,
            ОжидалсяДопуск = probe.ExpectedAdmitted,
            ФактическийДопуск = admitted,
            Смысл = probe.Meaning
        };

    // ── Копии состояния для проб чувствительности ─────────────────────────────

    // Копия контрольной точки без пар одного урока и без его точного совпадения.
    private static TranslationMachineCheckpoint RemoveLessonFromCopy(
        TranslationMachineCheckpoint source,
        Lesson lesson,
        out int removedPairs,
        out int removedExact)
    {
        // Пары урока отбираются по его исходному тексту.
        var keptEntries = source.KnnStore.Entries
            .Where(entry => !string.Equals(entry.Source, lesson.SourceText, StringComparison.OrdinalIgnoreCase))
            .ToList();
        removedPairs = source.KnnStore.Entries.Count - keptEntries.Count;

        // Запись точного совпадения урока отбирается по нормализованному ключу.
        var lessonKey = FixtureMapKey(lesson.SourceText);
        var executors = new List<ExecutorUnitCheckpoint>(source.Executors.Count);
        var removedExactEntries = 0;
        foreach (var executor in source.Executors)
        {
            // Правится только исполнитель нужного направления.
            if (executor.Direction != Direction)
            {
                executors.Add(executor);
                continue;
            }

            // Запись точного совпадения этого урока удаляется. Адресом точной
            // памяти стала пара «тема и источник», поэтому отбор идёт по половине
            // адреса, несущей источник; старый список по-прежнему чистится, чтобы
            // копия устаревшего состояния тоже оставалась без этой записи.
            var keptExact = executor.ExactSourceMap
                .Where(entry => !string.Equals(entry.Key, lessonKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var keptTopicExact = (executor.TopicSourceMap ?? [])
                .Where(entry => !string.Equals(entry.Source, lessonKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
            removedExactEntries += executor.ExactSourceMap.Count - keptExact.Count;
            removedExactEntries += (executor.TopicSourceMap?.Count ?? 0) - keptTopicExact.Count;
            executors.Add(executor with
            {
                ExactSourceMap = keptExact,
                TopicSourceMap = keptTopicExact
            });
        }

        removedExact = removedExactEntries;
        return source with
        {
            KnnStore = new TranslationKnnStoreCheckpoint(keptEntries),
            Executors = executors
        };
    }

    // Копия контрольной точки, где переводы пар одного урока заменены меткой.
    private static TranslationMachineCheckpoint ReplaceLessonTargetInCopy(
        TranslationMachineCheckpoint source,
        Lesson lesson,
        string sentinel,
        out int replacedPairs)
    {
        // Переводы пар именно этого урока заменяются явной русской меткой.
        var replaced = 0;
        var entries = new List<TranslationKnnStoreEntryCheckpoint>(source.KnnStore.Entries.Count);
        foreach (var entry in source.KnnStore.Entries)
        {
            // Пары других уроков остаются нетронутыми.
            if (!string.Equals(entry.Source, lesson.SourceText, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(entry);
                continue;
            }

            // Перевод этой пары подменяется меткой, адрес и источник сохраняются.
            entries.Add(entry with { Target = sentinel });
            replaced++;
        }

        replacedPairs = replaced;
        return source with { KnnStore = new TranslationKnnStoreCheckpoint(entries) };
    }

    // Копия контрольной точки с полностью очищенными парами и словарями.
    private static TranslationMachineCheckpoint ClearLearnedMemoryInCopy(TranslationMachineCheckpoint source)
    {
        // Все словари исполнителей обнуляются, пары памяти удаляются целиком.
        var executors = source.Executors
            .Select(executor => executor with
            {
                WordMap = [],
                SurfaceWordMap = [],
                PhraseMap = [],
                ExactSourceMap = [],
                // Точная память живёт по адресу «тема и источник», поэтому полная
                // очистка обязана обнулить и её: иначе «очищенная» копия всё ещё
                // отвечала бы из выученных ячеек.
                TopicSourceMap = []
            })
            .ToList();

        return source with
        {
            KnnStore = new TranslationKnnStoreCheckpoint([]),
            Executors = executors
        };
    }

    // Ввоз копии состояния в отдельный новый переводчик. Обучение не вызывается,
    // живое принятое состояние не трогается, копия никуда не публикуется.
    private static TranslationMachine ImportCopy(TranslationMachineCheckpoint copy)
    {
        // Новый объект переводчика получает только ввезённую копию.
        var machine = new TranslationMachine();
        var imported = machine.ImportCheckpoint(copy);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    // ── Отпечаток принятого обученного содержимого ────────────────────────────

    // Устойчивый отпечаток принятого обученного содержимого.
    //
    // Исключённые поля названы явно: отметка времени выгрузки ExportedAtUtc и
    // временные поля истории обучения. Без этого отпечаток менялся бы без
    // единой новой записи в память. Файловое равенство всей контрольной точки
    // этим сравнением не подменяется.
    private static string StableSha(TranslationMachineCheckpoint checkpoint)
    {
        // Проекция состояния: только принятое обученное содержимое.
        var projection = new
        {
            checkpoint.Version,
            checkpoint.AccuracyAfterLastBatch,
            checkpoint.HasStructuredTraining,
            checkpoint.KnnStore,
            checkpoint.SemanticTokenizer,
            checkpoint.Executors,
            checkpoint.QualityVerifier,
            История = checkpoint.TrainingHistory
                .Select(entry => new
                {
                    entry.BatchId,
                    entry.RowsTrained,
                    entry.AccuracyBefore,
                    entry.AccuracyAfter,
                    entry.RewardMargin,
                    entry.PromotedUnitIds
                })
                .ToArray(),
            Опознания = checkpoint.StructuredTrainingIdentities
        };

        // Отпечаток берётся от устойчивого текста проекции.
        var text = JsonSerializer.Serialize(projection, StableJson);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    // ── Нормализация ключа только для сборки набора данных ────────────────────

    // FixtureMapKey повторяет продуктовую нормализацию ключа словаря. Продуктовый
    // метод закрыт внутри области перевода и из проверок недостижим, поэтому
    // здесь лежит его копия, и она используется ТОЛЬКО для сборки набора данных
    // и для отбора записей в копиях состояния. Продуктовое решение о допуске
    // она не принимает.
    private static string FixtureMapKey(string value)
    {
        // Пробелы по краям убираются до любого разбора.
        var trimmed = (value ?? string.Empty).Trim();

        // Остаются буквы, цифры, подчёркивание, знак доллара и пробелы; всё
        // остальное, включая апостроф, отбрасывается — ровно как в продукте.
        return string.Join(" ", new string(trimmed
                .ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$' || char.IsWhiteSpace(ch))
                .ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    // ── Путь показаний ────────────────────────────────────────────────────────

    // Каталог показаний этой попытки внутри каталога запускателей.
    private static string ReceiptDirectory() => AllowedReceiptDirectory.Value;

    // Корень хранилища: подъём от каталога сборки проверок до каталога с запускателями опытов.
    private static string? RepositoryRootOrNull()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "experiments-v2", "_runners")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Куда этому прогону разрешено писать показания. Решение принимает общий приёмник
    // доказательств ОДИН раз за прогон, по одному факту: отслеживает ли хранилище файлы
    // внутри назначенного каталога показаний. Пока каталог не опубликован, прогон пишет в
    // него и остаётся в хранилище — это запись опыта. После публикации тот же прогон
    // становится проверочным и заметно уходит в игнорируемый каталог результатов испытаний.
    private static readonly Lazy<string> AllowedReceiptDirectory = new(() =>
    {
        var root = RepositoryRootOrNull();

        // Корень не найден: показания пишутся рядом со сборкой, и это видно по пути.
        if (root is null)
        {
            var fallback = Path.Combine(AppContext.BaseDirectory, $"{FixId}-{AttemptId}", "receipts");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        // Назначенный карточкой каталог показаний этой попытки.
        var declared = Path.Combine(root, "experiments-v2", "_runners", $"{FixId}-{AttemptId}", "receipts");

        // Разрешённый каталог выдаёт приёмник, он же сообщает род прогона и точный путь.
        return ExperimentFixes.ExperimentEvidenceSink
            .ForDeclaredDirectory(root, declared)
            .EnsureWriteRootDirectory();
    });

    // Запись одного файла показаний в кодировке UTF-8 без метки порядка байтов
    // и только с переводом строки Unix.
    private static void WriteReceipt(string fileName, object payload)
    {
        // Путь складывается из каталога показаний этой попытки и имени файла.
        var path = Path.Combine(ReceiptDirectory(), fileName);

        // Переводы строк Windows заменяются, иначе проверка пробелов перед
        // коммитом ругается на возврат каретки.
        var json = JsonSerializer.Serialize(payload, ReceiptJson)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        if (!json.EndsWith('\n')) json += "\n";
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }

    // ── Измеритель плеча ──────────────────────────────────────────────────────

    // ArmMeter — измеритель одного плеча. Каждая стадия измеряется отдельной
    // строкой; неизвестное значение остаётся пустым и не превращается в ноль.
    private sealed class ArmMeter
    {
        private readonly string _armId;
        private readonly string _description;
        private readonly List<PhaseMeasurement> _phases = [];
        private readonly List<object> _panels = [];
        private readonly List<object> _batches = [];
        private readonly List<object> _sensitivity = [];
        private readonly Process _process = Process.GetCurrentProcess();
        private object? _constantCeiling;
        private long? _checkpointBytes;
        private int _batchCalls;
        private int _assignedRows;
        private int _successfulLessons;
        private int _ordinaryReads;
        private int _sensitivityReads;

        // Плечо создаётся с человеческим описанием своего назначения.
        public ArmMeter(string armId, string description)
        {
            _armId = armId;
            _description = description;
        }

        // Measure выполняет одну стадию и записывает её показания отдельной строкой.
        public T Measure<T>(string phaseName, Func<T> action)
        {
            // Начальные показания окна измерения.
            var allocatedStart = GC.GetAllocatedBytesForCurrentThread();
            var processAllocatedStart = GC.GetTotalAllocatedBytes(precise: false);
            var cpuStart = ReadProcessorTime();
            var watch = Stopwatch.StartNew();

            // Сама стадия выполняется синхронно в том же потоке, иначе
            // потоковые выделения нельзя было бы приписать этой стадии.
            var value = action();

            // Конечные показания окна измерения.
            watch.Stop();
            var cpuEnd = ReadProcessorTime();

            _phases.Add(new PhaseMeasurement(
                Стадия: phaseName,
                ВремяПоСтеннымЧасамМс: Math.Round(watch.Elapsed.TotalMilliseconds, 3),
                ПроцессорноеВремяПроцессаМс: cpuStart is null || cpuEnd is null
                    ? null
                    : Math.Round((cpuEnd.Value - cpuStart.Value).TotalMilliseconds, 3),
                ВыделеноПотокомБайт: GC.GetAllocatedBytesForCurrentThread() - allocatedStart,
                ВыделеноПроцессомБайт: GC.GetTotalAllocatedBytes(precise: false) - processAllocatedStart));

            return value;
        }

        // MeasureSensitivity измеряет одну пробу чувствительности и отдельно
        // считает её чтение, чтобы не смешивать с бюджетом самой приёмки.
        public PanelAnswer MeasureSensitivity(string phaseName, Func<PanelAnswer> action)
        {
            // Проба измеряется той же стадийной меркой.
            var answer = Measure(phaseName, action);

            // Чтение пробы учитывается в отдельном счётчике.
            _sensitivityReads += 1;
            return answer;
        }

        // RecordPanel записывает фактические ответы одного набора.
        public void RecordPanel(string phaseName, IReadOnlyList<PanelAnswer> answers)
        {
            // Каждое чтение учитывается в фактическом бюджете чтений.
            _ordinaryReads += answers.Count;
            _panels.Add(new
            {
                Стадия = phaseName,
                ВерныхОтветов = answers.Count(answer => answer.ExactCorrect),
                ВсегоЧтений = answers.Count,
                Ответы = answers
            });
        }

        // RecordBatch записывает фактический исход одного вызова обучения партии.
        public void RecordBatch(string phaseName, BatchOutcome outcome, int assignedRows)
        {
            // Фактические дозы считаются по наблюдаемому исходу, а не по плану.
            _batchCalls += 1;
            _assignedRows += assignedRows;
            _successfulLessons += outcome.Value?.RowsTrained ?? 0;
            _batches.Add(new
            {
                Стадия = phaseName,
                НазначеноСтрок = assignedRows,
                Успех = outcome.IsSuccess,
                КодОтказа = outcome.ErrorCode,
                СообщениеОтказа = outcome.ErrorMessage,
                ВыброшеноИсключение = outcome.ThrewException,
                ТекстИсключения = outcome.ExceptionText,
                ОбученоСтрокПоОтчёту = outcome.Value?.RowsTrained,
                ПроцессорноеВремяПоОтчётуМашиныМс = outcome.Value?.CpuMs
            });
        }

        // RecordSensitivity записывает фактический исход одной пробы.
        public void RecordSensitivity(string phaseName, PanelAnswer answer, int changedPairs, int changedExact)
        {
            // Проба описывается фактическим ответом и объёмом правки копии.
            _sensitivity.Add(new
            {
                Стадия = phaseName,
                ИзмененоПар = changedPairs,
                ИзмененоЗаписейТочногоСовпадения = changedExact,
                Ответ = answer
            });
        }

        // RecordConstantCeiling записывает предел постоянного ответа.
        public void RecordConstantCeiling(ConstantCeiling ceiling)
        {
            // Предел записывается и числом строк, и долей, и самим ответом.
            _constantCeiling = new
            {
                ЛучшийПостоянныйОтвет = ceiling.Answer,
                ЗакрываетСтрок = ceiling.Hits,
                ВсегоСтрок = ceiling.Rows,
                Доля = ceiling.Share
            };
        }

        // RecordCheckpointBytes записывает фактический размер контрольной точки.
        public void RecordCheckpointBytes(string checkpointPath)
        {
            // Размер читается с диска: это наблюдаемое значение, а не оценка.
            _checkpointBytes = File.Exists(checkpointPath)
                ? new FileInfo(checkpointPath).Length
                : null;
        }

        // AssertAssignedDoses сверяет фактические дозы приёмки с назначенными.
        public void AssertAssignedDoses(int batchCalls, int assignedRows, int successfulLessons, int ordinaryReads)
        {
            // Расхождение дозы — это нарушение бюджета, а не мелочь оформления.
            Assert.Equal(batchCalls, _batchCalls);
            Assert.Equal(assignedRows, _assignedRows);
            Assert.Equal(successfulLessons, _successfulLessons);
            Assert.Equal(ordinaryReads, _ordinaryReads);
        }

        // AssertSensitivityDoses сверяет дозы проб чувствительности отдельно.
        public void AssertSensitivityDoses(int sensitivityReads)
        {
            // Пробы не добавляют ни партий, ни уроков — только названные чтения.
            Assert.Equal(sensitivityReads, _sensitivityReads);
        }

        // Write пишет файл показаний плеча. Вызывается всегда, в том числе при
        // падении утверждений, чтобы красный прогон сохранял измерение.
        public void Write()
        {
            // Пик рабочего набора относится ко всему процессу и не делится по стадиям.
            long? peakWorkingSet;
            try
            {
                _process.Refresh();
                peakWorkingSet = _process.PeakWorkingSet64;
            }
            catch (PlatformNotSupportedException)
            {
                peakWorkingSet = null;
            }

            var payload = new
            {
                Назначение = FixId,
                Попытка = AttemptId,
                Плечо = _armId,
                ЧтоЭтоЗаПлечо = _description,
                ВремяЗаписиUtc = DateTimeOffset.UtcNow,
                НазначеннаяВерсияЗнакаИсточника = AssignedPolarityFieldVersion,
                ПрежняяВерсияПравилаСвязочныхПредложений = LegacyPolicyVersion,
                ЧестноеОграничениеИзмерений = new[]
                {
                    "процессорное время относится ко всему процессу, не только к стадии",
                    "пик рабочего набора относится ко всему процессу и по стадиям не делится",
                    "выделения потоком относятся к стадии, выделения процессом включают посторонние потоки",
                    "свежая загрузка выполняется новым объектом переводчика в том же процессе операционной системы",
                    "одна короткая временная пара не разрешает вопрос о скорости: порядок, разогрев и шум не отделены",
                    "неизвестные значения записаны пустыми и не превращены в ноль"
                },
                ФактическиеДозы = new
                {
                    ВызововОбученияПартии = _batchCalls,
                    НазначенныхСтрок = _assignedRows,
                    УспешныхУроковПоОтчёту = _successfulLessons,
                    ОбычныхЧтений = _ordinaryReads,
                    ЧтенийЧувствительности = _sensitivityReads
                },
                ПределПостоянногоОтвета = _constantCeiling,
                ПикРабочегоНабораБайт = peakWorkingSet,
                РазмерКонтрольнойТочкиБайт = _checkpointBytes,
                СтадииИзмерения = _phases,
                ВызовыОбучения = _batches,
                Панели = _panels,
                ПробыЧувствительности = _sensitivity
            };

            // Запись показаний: перевод строк Unix, чтобы проверка пробелов
            // перед коммитом не ругалась на переводы строк Windows.
            WriteReceipt($"arm-{_armId}-receipt.json", payload);
        }

        // ReadProcessorTime читает процессорное время процесса; на площадке без
        // такой возможности возвращает пустое значение вместо нуля.
        private TimeSpan? ReadProcessorTime()
        {
            try
            {
                _process.Refresh();
                return _process.TotalProcessorTime;
            }
            catch (PlatformNotSupportedException)
            {
                return null;
            }
        }
    }

    // ── Записи данных ────────────────────────────────────────────────────────

    // Один урок приёмки: номер, источник, правильный перевод, словарь слов и
    // отвергнутый вариант.
    private sealed record Lesson(
        string LessonId,
        string SourceText,
        string TargetText,
        IReadOnlyList<(string Source, string Target)> Words,
        string WrongTargetText);

    // Строка набора чтений: номер, источник, эталон и человеческое пояснение.
    private sealed record PanelRow(string RowId, string Source, string Gold, string Meaning);

    // Фактический ответ одного чтения: текст, маршрут, вердикт, уверенность, верность.
    private sealed record PanelAnswer(
        string RowId,
        string Source,
        string Gold,
        string Meaning,
        string Answer,
        string AdapterId,
        string Verdict,
        double Confidence,
        bool ExactCorrect);

    // Наблюдаемый исход одного вызова обучения партии.
    private sealed record BatchOutcome(
        bool IsSuccess,
        string ErrorCode,
        string ErrorMessage,
        LearningBatchResult? Value,
        bool ThrewException,
        string ExceptionText);

    // Одна чистая проверка допуска: номер, запрос, запись, ожидание и смысл.
    private sealed record LawProbe(
        string ProbeId,
        string Query,
        string Candidate,
        bool ExpectedAdmitted,
        string Meaning);

    // Предел постоянного ответа: сам ответ, сколько строк он закрывает и доля.
    private sealed record ConstantCeiling(string Answer, int Hits, int Rows, double Share);

    // Показания одной стадии измерения.
    private sealed record PhaseMeasurement(
        string Стадия,
        double ВремяПоСтеннымЧасамМс,
        double? ПроцессорноеВремяПроцессаМс,
        long ВыделеноПотокомБайт,
        long ВыделеноПроцессомБайт);
}
