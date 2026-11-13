// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «проверяющий требует выученное слово целиком, а
// не куском текста» (назначение «целое обязательное слово в signature-правиле»
// раздела 111-P56 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт целого слова
// P5601).
//
// Что здесь проверяется по-человечески. После урока проверяющий качества знает,
// что в переводе этой фразы обязано стоять слово «ready». Проверку наличия он
// делал поиском куска текста: слово «already» содержит внутри себя буквы
// «ready», поэтому кандидат «already» проходил проверку как правильный. Тот же
// кусок текста есть и в «unready» (противоположный смысл), и в «readyState»
// (имя из кода). Это измеренный след дефекта диагностики: проверяющий сообщал
// «прошло» о заведомо неверном переводе, и дальше по потоку этот неверный
// перевод считался проверенным.
//
// Исправление, выбранное планом: в ветви выученных обязательных слов по
// signature требование ищется как последовательность ЦЕЛЫХ слов. Обе строки
// режутся на наибольшие цепочки букв и цифр, поэтому пробел, знак препинания и
// подчёркивание заканчивают слово, и требование обязано совпасть с соседними
// целыми словами. Регистр не учитывается: «READY» по-прежнему удовлетворяет
// требование «ready».
//
// Ровно один фактор. Правка касается ТОЛЬКО ветви `_requiredTermsBySignature`.
// Ветвь приметы источника и ветвь правил понятий — отдельные владельцы и
// отдельные гипотезы, их выражения не менялись. Чтобы измерение действительно
// изолировало один фактор, запрос панели собран так, что обе другие ветви в нём
// не срабатывают вообще: у единственного токена запроса известное понятие
// (LAUNCH_OP, не UNKNOWN) и часть речи VERB, поэтому примета источника пуста, а
// словарь правил понятий остаётся пустым, потому что текст замечания урока не
// называет ни одного понятия.
//
// Три плеча измерения:
//   плечо A (база) — исходы, обязательные и до правки, и после неё: холодный
//                    отказ от требования (ничего не выучено — никаких замечаний)
//                    и пять строк панели, чей исход у старого и нового
//                    выражения одинаков;
//   плечо B (кандидат) — вся панель: четыре из четырёх по основной панели и
//                    четыре из четырёх по краям, затем свежая загрузка и те же
//                    исходы без нового обучения, затем три диагностические
//                    копии выученного состояния;
//   плечо C (опровержение) — то же самое старое выражение поиска куска текста
//                    прогоняется по той же панели и обязано дать 3/4 и 2/4.
//                    Это и есть доказательство чувствительности к мутации:
//                    утверждения плеча B на старом выражении падают.
//
// Показания каждого плеча пишутся в файл показаний ДО утверждений, в блоке
// finally, поэтому красный прогон всё равно оставляет измеренный след.
//
// Чего эти проверки НЕ доказывают. Они не дают теории морфологии и не обещают
// правильного смысла любого текста: закрыт ровно случай «выученное обязательное
// слово найдено куском внутри другого слова» в ветви signature. Приведение
// основ слов, нормализация Unicode, нечёткое сравнение и знак высказывания
// намеренно НЕ добавлены — это была бы вторая правка без измерения. Разбор
// апострофов остаётся отдельным владельцем. Сам signature урока назначен
// вручную по карточке опыта, и его достижимость обычным потоком перевода этими
// проверками не доказана: мост обычного потока — отдельное назначение плана.
// Свежая загрузка выполняется новым объектом проверяющего в том же процессе
// операционной системы, а не отдельным холодным процессом. Независимого
// проверяющего у этого файла в этой сессии не было.
namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;

public sealed class TranslationRequiredWholeTermLearningTests
{
    // Имя назначения и номер попытки: по ним складывается путь показаний.
    private const string FixId = "FIX-P56-WHOLE-TERM";
    private const string AttemptId = "attempt-0001";

    // Источник корпуса учебной строки этой приёмки задан планом дословно.
    private const string SourceId = "r56-native-acceptance-v1";

    // Текст урока, кандидат урока и выученное обязательное слово заданы
    // карточкой опыта дословно и в проверках не переписываются.
    private const string LessonSourceText = "запусти процесс";
    private const string LessonCandidateText = "already";
    private const string RequiredTerm = "ready";

    // Signature урока назначен вручную по карточке опыта. Достижимость этого
    // адреса обычным потоком перевода здесь не утверждается.
    private const string AssignedSignature = "IMPERATIVE_V|LAUNCH_OP";

    // Подменное обязательное слово для пробы чувствительности: ни один кандидат
    // панели его не содержит, поэтому его появление в состоянии обязано
    // обрушить всю основную панель в отказ.
    private const string SentinelRequiredTerm = "zzqq";

    // Основная панель: четыре кандидата с одним и тем же контролируемым
    // запросом. Ожидание задано заранее и в ответ не подставляется.
    // Главные строки фактора — P2 «already»: именно она меняется от правки.
    private static readonly PanelRow[] MainPanel =
    [
        new("P1", RequiredTerm, true,
            "положительный контроль: ровно выученное слово целиком"),
        new("P2", "already", false,
            "ГЛАВНАЯ СТРОКА ФАКТОРА: выученное слово стоит куском внутри другого слова"),
        new("P3", "not ready", true,
            "выученное слово стоит целым словом рядом с другим словом; отрицание этот опыт не изучает, поэтому строка остаётся принятой"),
        new("P4", "", false,
            "пустой кандидат не несёт выученного слова")
    ];

    // Края: четыре кандидата, проверяющие границы слова и регистр.
    private static readonly PanelRow[] EdgePanel =
    [
        new("E1", "READY", true,
            "регистр не учитывается: верхний регистр удовлетворяет требование"),
        new("E2", "ready.", true,
            "знак препинания заканчивает слово, поэтому требование найдено"),
        new("E3", "readyState", false,
            "имя из кода: выученное слово стоит куском, целым словом не стоит"),
        new("E4", "unready", false,
            "противоположный смысл: выученное слово стоит куском, целым словом не стоит")
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

    // Настройки записи файла показаний: с отступами, чтобы человек читал его
    // глазами без дополнительной обработки.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Плечо A: исходы, обязательные и до правки, и после неё ────────────────
    //
    // Назначенные дозы плеча A: 1 настоящий урок продуктовой единицы,
    // 16 прямых проверок (8 холодных + 8 после урока).
    //
    // Это плечо НЕ утверждает исход главной строки фактора: до правки она
    // отвечает «прошло» о неверном переводе. Оно утверждает ровно то, что
    // обязано держаться в обоих состояниях продукта.
    [Fact]
    public void BaselineArm_OutcomesThatMustHoldBeforeAndAfterFix()
    {
        // Журнал показаний плеча заполняется по ходу и пишется в finally.
        var meter = new ArmMeter("A", "база: исходы, обязательные в обоих состояниях продукта");

        try
        {
            // Подготовка: создание продуктовой единицы проверяющего качества.
            var unit = meter.Measure("подготовка: продуктовая единица проверяющего", () => new QualityVerifierUnit());

            // Холодная панель: ничего не выучено, поэтому требований нет и все
            // восемь кандидатов обязаны пройти без замечаний. Это изоляция:
            // позднейшие отказы могут прийти только из выученного состояния.
            var cold = meter.Measure("холодная панель (8 прямых проверок)", () => ReadAllRows(unit));
            meter.RecordPanel("холодная панель", cold);
            Assert.All(cold, answer => Assert.True(
                answer.Pass,
                $"строка {answer.RowId}: до обучения требований нет, но проверяющий дал отказ: {string.Join("; ", answer.Issues)}"));
            Assert.Equal(0, cold.Count(answer => answer.SignatureIssueRaised));

            // Настоящее обучение: один вызов продуктового обучения единицы.
            var lesson = meter.Measure("обучение: один настоящий урок единицы", () => TrainLesson(unit));
            meter.RecordLesson("учебная строка приёмки", lesson);
            Assert.True(lesson.Stored, "урок не записал обязательное слово в состояние проверяющего");
            Assert.Equal(new[] { RequiredTerm }, lesson.StoredRequiredTerms);

            // Панель после урока: фактические исходы записываются целиком.
            var after = meter.Measure("панель после урока (8 прямых проверок)", () => ReadAllRows(unit));
            meter.RecordPanel("панель после урока", after);

            // Положительный контроль: пять строк, чей исход у старого и нового
            // выражения одинаков, обязаны держаться и здесь.
            foreach (var rowId in new[] { "P1", "P3", "P4", "E1", "E2" })
            {
                var row = AllRows().Single(candidate => candidate.RowId == rowId);
                var answer = after.Single(candidate => candidate.RowId == rowId);
                Assert.Equal(row.ExpectedPass, answer.Pass);
            }

            // Изоляция фактора: обе другие ветви проверяющего в этой панели
            // молчат, поэтому каждое замечание пришло из ветви signature.
            Assert.All(after, answer => Assert.All(
                answer.Issues,
                issue => Assert.Contains("for signature", issue, StringComparison.Ordinal)));

            // Фактические дозы плеча сверяются с назначенными планом.
            meter.AssertAssignedDoses(lessons: 1, directVerifyCalls: 16);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Плечо B: кандидат, требуемый результат переноса ───────────────────────
    //
    // Назначенные дозы плеча B: 1 настоящий урок продуктовой единицы,
    // 24 прямых проверки (8 холодных + 8 после урока + 8 после свежей загрузки)
    // и ОТДЕЛЬНО 12 проверок чувствительности (3 копии по 4 основные строки).
    //
    // На старом выражении поиска куска текста это плечо обязано падать на
    // значениях строк «already», «readyState» и «unready».
    [Fact]
    public void CandidateArm_LearnedTermMustMatchWholeWords_AndSurvivesFreshLoad()
    {
        // Журнал показаний плеча B.
        var meter = new ArmMeter("B", "кандидат: выученное обязательное слово ищется целыми словами");

        try
        {
            // Подготовка: создание продуктовой единицы проверяющего качества.
            var unit = meter.Measure("подготовка: продуктовая единица проверяющего", () => new QualityVerifierUnit());

            // Холодная панель: без выученных требований отказов быть не может.
            var cold = meter.Measure("холодная панель (8 прямых проверок)", () => ReadAllRows(unit));
            meter.RecordPanel("холодная панель", cold);
            Assert.Equal(0, cold.Count(answer => answer.SignatureIssueRaised));

            // Настоящее обучение: тот же один урок.
            var lesson = meter.Measure("обучение: один настоящий урок единицы", () => TrainLesson(unit));
            meter.RecordLesson("учебная строка приёмки", lesson);
            Assert.True(lesson.Stored, "урок не записал обязательное слово в состояние проверяющего");

            // Панель после урока: требуется четыре из четырёх по основной
            // панели и четыре из четырёх по краям.
            var after = meter.Measure("панель после урока (8 прямых проверок)", () => ReadAllRows(unit));
            meter.RecordPanel("панель после урока", after);
            AssertPanelMatchesOracle(after, MainPanel, "основная панель после урока");
            AssertPanelMatchesOracle(after, EdgePanel, "края после урока");

            // Утрата правильных исходов обязана быть нулевой: ни одна строка,
            // принятая до правки верно, не стала неверной.
            Assert.Equal(0, after.Count(answer =>
                !answer.MatchesOracle && OldSubstringOutcomeMatchesOracle(answer.RowId)));

            // Фактическое принятое состояние снимается продуктовым выгрузчиком:
            // именно его копии правятся в пробах чувствительности.
            var accepted = ExportCheckpoint(unit);
            Assert.Single(accepted.RequiredTermsBySignature);
            Assert.Equal(AssignedSignature, accepted.RequiredTermsBySignature[0].Key);

            // Одна свежая загрузка: НОВЫЙ объект проверяющего получает только
            // выгруженное состояние и ни одного урока. Это не отдельный
            // холодный процесс операционной системы — записано честно.
            var reloaded = meter.Measure(
                "свежая загрузка состояния новым объектом проверяющего",
                () =>
                {
                    var fresh = new QualityVerifierUnit();
                    ImportCheckpoint(fresh, accepted);
                    return fresh;
                });

            // Панель после свежей загрузки: те же исходы без нового обучения.
            var afterLoad = meter.Measure("панель после свежей загрузки (8 прямых проверок)", () => ReadAllRows(reloaded));
            meter.RecordPanel("панель после свежей загрузки", afterLoad);
            AssertPanelMatchesOracle(afterLoad, MainPanel, "основная панель после свежей загрузки");
            AssertPanelMatchesOracle(afterLoad, EdgePanel, "края после свежей загрузки");

            // Выученное правило после загрузки обязано быть тем же самым.
            var afterLoadState = ExportCheckpoint(reloaded);
            Assert.Equal(new[] { RequiredTerm }, afterLoadState.RequiredTermsBySignature[0].Values);

            // Назначенные дозы самой приёмки сверяются до проб чувствительности.
            meter.AssertAssignedDoses(lessons: 1, directVerifyCalls: 24);

            // ── Три диагностические копии выученного состояния ─────────────────
            //
            // Живое принятое состояние не портится: каждая проба ввозит КОПИЮ
            // выгруженного состояния в отдельный новый объект проверяющего.
            // Обучение в пробах не вызывается.

            // Копия 1: словарь обязательных слов по signature удалён целиком.
            // Отказы обязаны исчезнуть: иначе они приходят не из выученного
            // состояния, и это провал изоляции.
            var withoutDictionary = accepted with { RequiredTermsBySignature = [] };
            var probeOne = meter.MeasureSensitivity(
                "проба 1: словарь обязательных слов по signature удалён",
                () => ReadMainRows(ImportCopy(withoutDictionary)));
            meter.RecordSensitivity("проба 1: удалённый словарь требований", probeOne);
            Assert.All(probeOne, answer => Assert.True(
                answer.Pass,
                $"строка {answer.RowId}: отказ сохранился при удалённом словаре требований — провал изоляции"));

            // Копия 2: то же требование заменено на подменное слово. Вся
            // основная панель обязана уйти в отказ: это доказывает, что
            // требование читается из выученного состояния, а не вписано в код.
            var withSentinel = accepted with
            {
                RequiredTermsBySignature =
                [
                    new QualityVerifierStringListCheckpoint(AssignedSignature, [SentinelRequiredTerm])
                ]
            };
            var probeTwo = meter.MeasureSensitivity(
                "проба 2: обязательное слово заменено подменным значением",
                () => ReadMainRows(ImportCopy(withSentinel)));
            meter.RecordSensitivity("проба 2: подменённое обязательное слово", probeTwo);
            Assert.All(probeTwo, answer => Assert.False(
                answer.Pass,
                $"строка {answer.RowId}: кандидат прошёл подменное требование «{SentinelRequiredTerm}» — требование берётся не из состояния"));

            // Копия 3: состояние оставлено исходным. Основная панель обязана
            // воспроизвести те же четыре из четырёх.
            var probeThree = meter.MeasureSensitivity(
                "проба 3: состояние оставлено исходным",
                () => ReadMainRows(ImportCopy(accepted)));
            meter.RecordSensitivity("проба 3: исходное состояние", probeThree);
            AssertPanelMatchesOracle(probeThree, MainPanel, "основная панель на исходной копии");

            // Назначенные дозы чувствительности: ноль уроков, 12 проверок.
            meter.AssertSensitivityDoses(sensitivityVerifyCalls: 12);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Плечо C: опровержение — старое выражение падает на этой же панели ─────
    //
    // Назначенные дозы: ноль уроков и ноль продуктовых проверок. Здесь
    // выполняется ровно одно прежнее продуктовое выражение проверки наличия
    // требования — поиск куска текста без учёта регистра — и измеряется, сколько
    // строк панели оно закрывает верно.
    //
    // Это и есть доказательство чувствительности к мутации: утверждения плеча B
    // на прежнем выражении падают именно на названных строках, а не проходят.
    // Копия прежнего выражения здесь НЕ заменяет продуктовый код и не является
    // заглушкой: она служит эталоном опровержения.
    [Fact]
    public void OldSubstringLogic_FailsTheSameOracle_ProvingTheNewAssertionsAreMutationSensitive()
    {
        // Исходы прежнего выражения считаются по каждой строке отдельно.
        var records = new List<object>();
        var mainCorrect = 0;
        var edgeCorrect = 0;

        // Основная панель: прежнее выражение закрывает верно только три строки.
        foreach (var row in MainPanel)
        {
            var oldPass = OldSubstringContains(row.CandidateText, RequiredTerm);
            if (oldPass == row.ExpectedPass) mainCorrect++;
            records.Add(DescribeOldOutcome("основная панель", row, oldPass));
        }

        // Края: прежнее выражение закрывает верно только две строки.
        foreach (var row in EdgePanel)
        {
            var oldPass = OldSubstringContains(row.CandidateText, RequiredTerm);
            if (oldPass == row.ExpectedPass) edgeCorrect++;
            records.Add(DescribeOldOutcome("края", row, oldPass));
        }

        // Измеренные показания опыта: основная панель 3 из 4, края 2 из 4.
        Assert.Equal(3, mainCorrect);
        Assert.Equal(2, edgeCorrect);

        // Названные строки обязаны переворачиваться: именно они доказывают, что
        // утверждения плеча B чувствительны к возврату прежнего выражения.
        foreach (var rowId in new[] { "P2", "E3", "E4" })
        {
            var row = AllRows().Single(candidate => candidate.RowId == rowId);
            Assert.True(
                OldSubstringContains(row.CandidateText, RequiredTerm),
                $"строка {rowId}: прежнее выражение обязано было ошибочно принять «{row.CandidateText}»");
            Assert.False(
                row.ExpectedPass,
                $"строка {rowId}: ожидание панели обязано быть отказом");
        }

        // Показания опровержения пишутся отдельным файлом.
        WriteReceipt("mutation-oracle-receipt.json", new
        {
            Назначение = FixId,
            Попытка = AttemptId,
            ЧтоЭтоЗаПроверка = "опровержение: прежнее продуктовое выражение поиска куска текста прогнано по той же панели",
            ВремяЗаписиUtc = DateTimeOffset.UtcNow,
            ВыученноеОбязательноеСлово = RequiredTerm,
            ПрежнееВыражение = "candidateText.Contains(required, StringComparison.OrdinalIgnoreCase)",
            НовоеВыражение = "требование совпадает как последовательность целых слов (буквы и цифры), регистр не учитывается",
            ЧестноеОграничение = new[]
            {
                "это копия прежнего выражения в проверках: продуктовый код она не заменяет и заглушкой не является",
                "сравнение идёт по тем же восьми строкам панели и тому же выученному слову",
                "ни одного урока и ни одной продуктовой проверки здесь не выполняется"
            },
            ФактическиеИсходы = new
            {
                ОсновнаяПанельВерно = mainCorrect,
                ОсновнаяПанельВсего = MainPanel.Length,
                КраяВерно = edgeCorrect,
                КраяВсего = EdgePanel.Length
            },
            Строки = records
        });
    }

    // ── Утверждения наборов исходов ───────────────────────────────────────────

    // Все строки набора обязаны совпасть с заранее заданным ожиданием.
    private static void AssertPanelMatchesOracle(
        IReadOnlyList<VerifyAnswer> answers,
        PanelRow[] rows,
        string panelName)
    {
        // Каждая строка проверяется отдельно, чтобы сообщение о падении
        // называло конкретный фактический исход и замечания проверяющего.
        foreach (var row in rows)
        {
            var answer = answers.Single(candidate => candidate.RowId == row.RowId);
            Assert.True(
                answer.Pass == row.ExpectedPass,
                $"{panelName}, строка {row.RowId} «{row.CandidateText}» ({row.Meaning}): ожидалось {(row.ExpectedPass ? "принято" : "отказ")}, получено {(answer.Pass ? "принято" : "отказ")}, замечания: {string.Join("; ", answer.Issues)}");
        }

        // Итоговый счёт набора назван явно.
        Assert.Equal(rows.Length, answers.Count(answer =>
            rows.Any(row => row.RowId == answer.RowId) && answer.MatchesOracle));
    }

    // Прежнее выражение, прогнанное по одной строке: совпал ли её исход с
    // ожиданием панели. Используется для счёта утраты правильных исходов.
    private static bool OldSubstringOutcomeMatchesOracle(string rowId)
    {
        // Строка ищется в объединённой панели по своему номеру.
        var row = AllRows().Single(candidate => candidate.RowId == rowId);
        return OldSubstringContains(row.CandidateText, RequiredTerm) == row.ExpectedPass;
    }

    // Копия прежнего продуктового выражения проверки наличия требования.
    // Служит эталоном опровержения и продуктовый код не подменяет.
    private static bool OldSubstringContains(string candidateText, string required)
        => candidateText.Contains(required, StringComparison.OrdinalIgnoreCase);

    // Человеческое описание одного исхода прежнего выражения для показаний.
    private static object DescribeOldOutcome(string panel, PanelRow row, bool oldPass)
        => new
        {
            Панель = panel,
            Код = row.RowId,
            Кандидат = row.CandidateText,
            ОжидалсяИсход = row.ExpectedPass ? "принято" : "отказ",
            ИсходПрежнегоВыражения = oldPass ? "принято" : "отказ",
            ПрежнееВыражениеВерно = oldPass == row.ExpectedPass,
            Смысл = row.Meaning
        };

    // ── Учебная строка и её запись в состояние ────────────────────────────────

    // Один настоящий урок продуктовой единицы проверяющего качества.
    private static LessonOutcome TrainLesson(QualityVerifierUnit unit)
    {
        // Правильный ответ урока взят из карточки опыта дословно: оценка 0.42,
        // одно замечание, отказ и подсказка исправления.
        var chosen = new VerifierOutput(
            Score: 0.42,
            Issues: [$"expected '{RequiredTerm}'"],
            Pass: false,
            CorrectionHint: RequiredTerm);

        // Отвергнутый вариант несёт сам дефект: кандидат принят как верный.
        var rejected = new VerifierOutput(
            Score: 0.95,
            Issues: [],
            Pass: true,
            CorrectionHint: null);

        // Строка собирается ровно по форме учебной записи предметной области.
        var example = new TranslationLearningExample(
            ExampleId: "R56-L1",
            UnitId: TranslationUnitIds.QualityVerifier,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new VerifierInput(LessonSourceText, LessonCandidateText), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, UnitJson),
            RejectedType: "required_term_substring_accepted",
            SemanticSignature: AssignedSignature,
            SourceId: SourceId,
            HeldOut: false);

        // Продуктовый путь обучения единицы вызывается напрямую.
        unit.TrainOnExample(example);

        // Фактическая запись в состояние читается продуктовым выгрузчиком.
        var state = ExportCheckpoint(unit);
        var entry = state.RequiredTermsBySignature
            .FirstOrDefault(row => string.Equals(row.Key, AssignedSignature, StringComparison.Ordinal));
        return new LessonOutcome(
            Stored: entry is not null,
            StoredRequiredTerms: entry?.Values.ToArray() ?? []);
    }

    // ── Прямые проверки через продуктовый метод Verify ────────────────────────

    // Запрос панели: один и тот же контролируемый набор токенов для всех строк.
    //
    // Собран так, что обе другие ветви проверяющего в нём молчат: у токена
    // известное понятие LAUNCH_OP и часть речи VERB, поэтому примета источника
    // пуста и её ветвь пропускается целиком.
    private static SemanticTokenSequence BuildControlledQuery()
        => new(
            Pattern: SentencePattern.IMPERATIVE_V,
            Tokens:
            [
                new SemanticToken(
                    Word: "запусти",
                    Lemma: "запустить",
                    Pos: TranslationPos.VERB,
                    Role: "IMPERATIVE",
                    Concept: SemanticConcept.LAUNCH_OP)
            ],
            PosBigrams: [],
            Signature: AssignedSignature,
            PrefixSignature: AssignedSignature,
            StructureSignature: "IMPERATIVE_V|VERB",
            RuleTags: []);

    // Объединённая панель: основные строки и края в одном порядке.
    private static PanelRow[] AllRows() => [.. MainPanel, .. EdgePanel];

    // Чтение всех восьми строк панели через продуктовый метод Verify.
    private static IReadOnlyList<VerifyAnswer> ReadAllRows(QualityVerifierUnit unit)
        => ReadRows(unit, AllRows());

    // Чтение только четырёх основных строк: так считаются пробы чувствительности.
    private static IReadOnlyList<VerifyAnswer> ReadMainRows(QualityVerifierUnit unit)
        => ReadRows(unit, MainPanel);

    // Чтение набора строк: один и тот же запрос, разные кандидаты.
    private static IReadOnlyList<VerifyAnswer> ReadRows(QualityVerifierUnit unit, PanelRow[] rows)
    {
        // Запрос собирается один раз: фактор изолирован на тексте кандидата.
        var query = BuildControlledQuery();

        // Исходы собираются по порядку строк набора.
        var answers = new List<VerifyAnswer>(rows.Length);
        foreach (var row in rows)
        {
            // Продуктовый метод проверки вызывается напрямую, без копий единицы.
            var result = unit.Verify(row.CandidateText, query);
            answers.Add(new VerifyAnswer(
                RowId: row.RowId,
                CandidateText: row.CandidateText,
                ExpectedPass: row.ExpectedPass,
                Pass: result.Pass,
                Score: result.Score,
                Issues: result.Issues,
                SignatureIssueRaised: result.Issues.Any(issue =>
                    issue.Contains("for signature", StringComparison.Ordinal)),
                MatchesOracle: result.Pass == row.ExpectedPass));
        }

        return answers;
    }

    // ── Узкий доступ к существующим внутренним методам состояния ──────────────
    //
    // Выгрузка и ввоз состояния единицы объявлены внутренними для своей области.
    // План разрешает узкий доступ по точным именам и запрещает добавлять
    // публичный изменяемый интерфейс ради проверок, поэтому здесь ровно два
    // вызова по точным именам и ничего больше.

    // Выгрузка фактического состояния продуктовым методом единицы.
    private static QualityVerifierUnitCheckpoint ExportCheckpoint(QualityVerifierUnit unit)
    {
        // Метод ищется по точному имени в самой единице.
        var method = typeof(QualityVerifierUnit).GetMethod(
            "ExportCheckpoint",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        // Результат приводится к публичной записи состояния.
        var exported = method!.Invoke(unit, null);
        Assert.NotNull(exported);
        return (QualityVerifierUnitCheckpoint)exported!;
    }

    // Ввоз состояния продуктовым методом единицы.
    private static void ImportCheckpoint(QualityVerifierUnit unit, QualityVerifierUnitCheckpoint checkpoint)
    {
        // Метод ищется по точному имени в самой единице.
        var method = typeof(QualityVerifierUnit).GetMethod(
            "ImportCheckpoint",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        // Ввоз выполняется ровно продуктовым путём восстановления состояния.
        method!.Invoke(unit, [checkpoint]);
    }

    // Ввоз копии состояния в отдельный новый объект единицы. Обучение не
    // вызывается, живое принятое состояние не трогается.
    private static QualityVerifierUnit ImportCopy(QualityVerifierUnitCheckpoint copy)
    {
        // Новый объект получает только ввезённую копию состояния.
        var unit = new QualityVerifierUnit();
        ImportCheckpoint(unit, copy);
        return unit;
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
        private readonly List<object> _lessons = [];
        private readonly List<object> _sensitivity = [];
        private readonly Process _process = Process.GetCurrentProcess();
        private int _lessonCount;
        private int _directVerifyCalls;
        private int _sensitivityVerifyCalls;

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
                ВыделеноПотокомБайт: GC.GetAllocatedBytesForCurrentThread() - allocatedStart));

            return value;
        }

        // MeasureSensitivity измеряет одну пробу чувствительности и отдельно
        // считает её проверки, чтобы не смешивать с бюджетом самой приёмки.
        public IReadOnlyList<VerifyAnswer> MeasureSensitivity(
            string phaseName,
            Func<IReadOnlyList<VerifyAnswer>> action)
        {
            // Проба измеряется той же стадийной меркой.
            var answers = Measure(phaseName, action);

            // Проверки пробы учитываются в отдельном счётчике.
            _sensitivityVerifyCalls += answers.Count;
            return answers;
        }

        // RecordPanel записывает фактические исходы одного набора.
        public void RecordPanel(string phaseName, IReadOnlyList<VerifyAnswer> answers)
        {
            // Каждая проверка учитывается в фактическом бюджете проверок.
            _directVerifyCalls += answers.Count;
            _panels.Add(new
            {
                Стадия = phaseName,
                СовпалоСОжиданием = answers.Count(answer => answer.MatchesOracle),
                ВсегоПроверок = answers.Count,
                Исходы = answers
            });
        }

        // RecordLesson записывает фактический исход одного урока.
        public void RecordLesson(string phaseName, LessonOutcome outcome)
        {
            // Фактические дозы считаются по наблюдаемому исходу, а не по плану.
            _lessonCount += 1;
            _lessons.Add(new
            {
                Стадия = phaseName,
                ТребованиеЗаписано = outcome.Stored,
                ЗаписанныеТребования = outcome.StoredRequiredTerms
            });
        }

        // RecordSensitivity записывает фактический исход одной пробы.
        public void RecordSensitivity(string phaseName, IReadOnlyList<VerifyAnswer> answers)
        {
            // Проба описывается фактическими исходами основной панели.
            _sensitivity.Add(new
            {
                Стадия = phaseName,
                ПринятоСтрок = answers.Count(answer => answer.Pass),
                ВсегоПроверок = answers.Count,
                Исходы = answers
            });
        }

        // AssertAssignedDoses сверяет фактические дозы приёмки с назначенными.
        public void AssertAssignedDoses(int lessons, int directVerifyCalls)
        {
            // Расхождение дозы — это нарушение бюджета, а не мелочь оформления.
            Assert.Equal(lessons, _lessonCount);
            Assert.Equal(directVerifyCalls, _directVerifyCalls);
        }

        // AssertSensitivityDoses сверяет дозы проб чувствительности отдельно.
        public void AssertSensitivityDoses(int sensitivityVerifyCalls)
        {
            // Пробы не добавляют ни одного урока — только названные проверки.
            Assert.Equal(sensitivityVerifyCalls, _sensitivityVerifyCalls);
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
                НазначенныйSignatureУрока = AssignedSignature,
                ВыученноеОбязательноеСлово = RequiredTerm,
                ЧестноеОграничениеИзмерений = new[]
                {
                    "процессорное время относится ко всему процессу, не только к стадии",
                    "пик рабочего набора относится ко всему процессу и по стадиям не делится",
                    "свежая загрузка выполняется новым объектом проверяющего в том же процессе операционной системы",
                    "signature урока назначен вручную: достижимость этого адреса обычным потоком перевода здесь не доказана",
                    "одна короткая временная пара не разрешает вопрос о скорости: порядок, разогрев и шум не отделены",
                    "неизвестные значения записаны пустыми и не превращены в ноль"
                },
                ФактическиеДозы = new
                {
                    Уроков = _lessonCount,
                    ПрямыхПроверок = _directVerifyCalls,
                    ПроверокЧувствительности = _sensitivityVerifyCalls
                },
                ПикРабочегоНабораБайт = peakWorkingSet,
                СтадииИзмерения = _phases,
                Уроки = _lessons,
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

    // Строка панели: номер, текст кандидата, ожидаемый исход и человеческое
    // пояснение, зачем эта строка стоит в панели.
    private sealed record PanelRow(
        string RowId,
        string CandidateText,
        bool ExpectedPass,
        string Meaning);

    // Фактический исход одной прямой проверки.
    private sealed record VerifyAnswer(
        string RowId,
        string CandidateText,
        bool ExpectedPass,
        bool Pass,
        double Score,
        IReadOnlyList<string> Issues,
        bool SignatureIssueRaised,
        bool MatchesOracle);

    // Наблюдаемый исход одного урока: записалось ли требование и какое.
    private sealed record LessonOutcome(bool Stored, IReadOnlyList<string> StoredRequiredTerms);

    // Показания одной стадии измерения.
    private sealed record PhaseMeasurement(
        string Стадия,
        double ВремяПоСтеннымЧасамМс,
        double? ПроцессорноеВремяПроцессаМс,
        long ВыделеноПотокомБайт);
}
