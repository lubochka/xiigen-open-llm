// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using P11701;

// Что здесь проверяется, по-человечески.
//
// Это раздел про завершение прогона, которое доказывает результат и весь расход: план
// сто тридцать девять, раздел R117-02; аудит сто тридцать восемь, находки F06 (неучтённый
// расход) и F07 (ложная терминальность оболочек запуска).
//
// Что было неправильно, по коду.
//
// Первое — строка вместо логического значения. В оболочке запуска
// experiments-v2/RP01-MICRO/P11701/Run-Experiment.ps1 до этой правки стояло:
//   строка 91: terminal = [bool]$o.terminal
// В этой оболочке ЛЮБАЯ непустая строка приводится к истине, поэтому расписка со
// значением "false" читалась как «прогон завершён». Дальше строка 121 проверяла
// ($existing.usable -and $existing.terminal) и строка 123 выходила кодом из этой же
// расписки, то есть незавершённый прогон объявлялся завершённым. Приёмка на строках
// 70-92 смотрела только НАЛИЧИЕ полей, постоянную строку схемы и числовой код выхода:
// ни вида значений, ни перечня допустимых состояний, ни сверки опыта, попытки, источника
// и фазы с назначением там не было — таких полей расписка вообще не несла.
//
// Второе — пустое значение вместо ошибки. В оболочке опыта сто шестнадцать
// experiments-v2/RP01-MICRO/P11601/Run-Experiment.ps1 до этой правки стояло:
//   строка 331: $outcome = Get-Content -LiteralPath $exitPath -Raw | ConvertFrom-Json
// Разбор буквального null возвращает пустое значение и НЕ бросает ошибку, поэтому весь
// блок объектных проверок на строках 337-348 пропускался целиком, список причин
// оставался пустым, и строка 353 писала состояние COMPLETED_AND_PARSED, тогда как
// строка 354 в той же расписке писала receipt_parsed = ложь. Код выхода оставался нулевым.
//
// Третье — итог из кода возврата последней команды и постоянная терминальность. В
// оболочке опыта сто семнадцать строки 153, 161, 171, 183, 196, 202, 228, 237, 247 и 260
// писали terminal = $true на каждом пути, а итоговая расписка на строках 258-266 выводила
// состояние из $runExit и не содержала ни состава назначенных стадий, ни их отдельных
// состояний, ни полной стоимости. В программе опыта
// experiments-v2/RP01-MICRO/P11701/Program.cs строки 482-483 и 589-590 писали
// execution_status = "COMPLETED" и learning_validity = "VALID_FOR_NAMED_UNIT"
// постоянными строками, а перерасход на строке 520 считался по одной категории и ни разу
// не складывался по всем категориям и физическим попыткам.
//
// Четвёртое — завершение корня принималось за завершение дерева: строки 157, 175, 232 и
// 251 писали process_tree_exited = ...HasExited, то есть потомки и внуки не наблюдались.
//
// Что стало. Исход определяет НАБЛЮДЕНИЕ. Состав исполняемых стадий приходит из
// доверенного паспорта прогона, а не из проверяемой расписки. Стадия принята, когда
// наблюдение совпало с ЗАРАНЕЕ ЗАДАННЫМ ожиданием; у отрицательной стадии ожидание —
// ЛОЖЬ, и она пройдена ровно тогда, когда назначенное действие правильно не состоялось.
// Пригодность отзывают восемь причин, каждая со своим названием. Приёмка расписки
// отвергает ДО преобразований пустое значение, массив, строку, число, повтор
// обязательного поля, пропущенный член, строковое "false" и несовпадение опыта, попытки,
// источника, фазы и стадии с паспортом. Расход складывается по всем категориям и всем
// физическим попыткам, где научный столбец и столбец контроля реализации — разные
// столбцы ОДНОЙ суммы. Дерево процессов опознаётся номером ВМЕСТЕ со временем старта, а
// ошибка перечисления даёт неизвестность, а не пустое успешное множество.
//
// О красном прогоне на прежней логике. Прежние правила жили в оболочке на языке
// командной строки, поэтому ниже каждое из них записано отдельной named-функцией с
// приставкой Prior — это ВОСПРОИЗВЕДЕНИЕ прежнего дефекта, а не копия нового правила.
// Проверка требует двух вещей разом: прежнее правило на том же входе даёт НЕВЕРНЫЙ
// ответ, а действующая функция — верный. Если кто-нибудь вернёт прежнее поведение,
// падает именно это сравнение.
//
// Красные по сути, то есть на прежнем коде они обязаны были падать:
//   1. «строковое false не считается терминальностью» — прежнее приведение строки к
//      логическому значению (строка 91) возвращало истину на строке "false";
//   2. «пустой корень расписки не считается разобранным успехом» — прежний разбор
//      буквального null (строка 331) не бросал ошибку и оставлял список причин пустым;
//   3. «нулевой код возврата не закрывает неисполненную стадию» — прежний итог
//      (строки 258-266) выводился из кода возврата последней команды и не смотрел на
//      состав назначенных стадий;
//   4. «постоянная пригодность отзывается перерасходом» — прежние строки 482-483 и
//      589-590 писали пригодность строковой константой независимо от расхода.
// Положительные контроли «честная полная расписка принята» и «честный полный прогон
// принят по наблюдению» зелёные и до, и после: прежняя логика их тоже пропускала, и
// правка их не ломает. Ожидание ни в одном случае не подгонялось под новый код:
// ожидаемый ответ каждой проверки назван прежде её входа и совпадает с текстом
// требования раздела R117-02.
//
// Чего эта правка НЕ лечит, и я этого не заявляю. Первое: перечисление дерева процессов
// в оболочке опыта сто шестнадцать — я доказал по её коду только разбор пустого значения
// (строка 331) и противоречие состояния и признака разбора (строки 353-354), поэтому её
// наблюдение потомков осталось как было. Второе: отдельный режим проверки уже сохранённой
// попытки (--validate-saved) из того же раздела плана здесь не создан. Третье: это не
// научный пересчёт опыта — сохранённые сырые свидетельства и записанные строки
// результатов прошлых опытов я не переписывал. Четвёртое: живые процессы-пустышки для
// проверки потомка, внука и переиспользования номера здесь заменены снимком дерева,
// потому что запуск процессов в этой среде не выполнялся.
//
// Проверки зовут ДЕЙСТВУЮЩИЕ функции, скомпилированные и в хост опыта, и сюда: тот же
// файл src/RunCompletionFinalizer.cs вызывает программа опыта. Ни одной копии нового
// правила в тесте нет. Обращений к модели и обучению ноль.
//
// Прогона не было: сборка и тесты в этой среде запрещены политикой, я их не запускал и
// зелёными не называю.
public sealed class P11701RunCompletionFinalizerTests
{
    /// <summary>Опознание опыта из паспорта, а не из проверяемого ответа.</summary>
    private const string ExperimentId = "P117-PRIMARY-01";

    /// <summary>Попытка, закреплённая каталогом результатов хозяина опыта.</summary>
    private const string AttemptId = "attempt-0001";

    /// <summary>Источник данных прогона из паспорта.</summary>
    private const string Source = "D05 — направление русский→английский";

    /// <summary>Фаза прогона из паспорта.</summary>
    private const string Phase = "P117.1 — шлюз поиска по семантическому понятию";

    /// <summary>Опознание научной стадии прогона.</summary>
    private const string ScientificStage = "scientific-run";

    /// <summary>Состояние принятого научного прогона.</summary>
    private const string StateAccepted = "научный прогон завершён и принят";

    /// <summary>Состояние непринятого научного прогона.</summary>
    private const string StateNotAccepted = "научный прогон не принят";

    /// <summary>Название единицы расхода исполнений.</summary>
    private const string ExecuteUnit = "исполнение единицы (Execute)";

    /// <summary>Название единицы расхода обучений.</summary>
    private const string TrainUnit = "обучение единицы (TrainOnExample)";

    // ── Прежние правила, воспроизведённые для доказательства красноты ────────────

    /// <summary>
    /// ПРЕЖНЕЕ правило строки 91 оболочки запуска: признак терминальности получался
    /// приведением значения к логическому типу, а в той оболочке любая непустая строка
    /// приводится к истине. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorTruthinessOfTerminal(string rawValue)
        => !string.IsNullOrEmpty(rawValue); // Строка "false" тоже непустая, поэтому прежде это была истина.

    /// <summary>
    /// ПРЕЖНЕЕ правило строк 331 и 353 оболочки опыта сто шестнадцать: разбор буквального
    /// пустого значения не бросал ошибку, поэтому список причин оставался пустым, и итог
    /// объявлялся разобранным успехом. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorEmptyRootCountedAsParsedSuccess(string receiptJson)
        => string.Equals(receiptJson.Trim(), "null", StringComparison.Ordinal); // Ни одной причины не добавлялось.

    /// <summary>
    /// ПРЕЖНЕЕ правило строк 258-266 оболочки запуска: состояние итога выводилось из кода
    /// возврата последней команды. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorOutcomeFromLastExitCode(int processExitCode)
        => processExitCode == 0; // Состав назначенных стадий в решение не входил.

    /// <summary>
    /// ПРЕЖНЕЕ правило строк 482-483 и 589-590 программы опыта: пригодность писалась
    /// строковой константой. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static string PriorConstantLearningValidity()
        => "VALID_FOR_NAMED_UNIT"; // Ни расход, ни состав стадий на это не влияли.

    // ── Закрытие пробелов инвентаря по разделу завершения прогона (3 ноября 2026) ─
    //
    // Три пробела первой независимой проверки закрываются здесь, потому что доказываются БЕЗ
    // запуска настоящих процессов: сохранённый ненулевой код выхода без отдельной названной
    // проверки, половина проверки повторного входа и поздний сбой после сохранённой стадии.
    // Остальные пробелы инвентаря требуют либо правки оболочек под узлами опытов, которые мне
    // не назначены, либо настоящего запуска процессов; они названы открытыми в пометке плана и
    // подделок им здесь не изобретается.

    /// <summary>
    /// ПРЕЖНЕЕ правило чтения кода выхода в оболочке: отсутствующее или нечитаемое значение
    /// приводилось к нулю, то есть к успеху. Настоящая функция от входа: ответ считается из
    /// поданного текста значения. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static int PriorAbsentOrUnreadableExitCountedAsZero(string? rawExitCode)
        => int.TryParse(rawExitCode, out var parsed) ? parsed : 0; // Нечитаемое становилось нулём.

    [Fact]
    public void Saved_nonzero_exit_code_is_read_as_nonzero_and_a_text_exit_code_is_refused()
    {
        // Прежнее правило на входе «значения нет» отвечает НЕВЕРНО: ноль, то есть успех.
        Assert.Equal(0, PriorAbsentOrUnreadableExitCountedAsZero(null));
        // И на нечитаемом значении — тоже ноль.
        Assert.Equal(0, PriorAbsentOrUnreadableExitCountedAsZero("не число"));
        // Зависимость от входа: на настоящем числе его ответ другой, значит дефект именно в
        // приведении отсутствующего и нечитаемого к нулю, а не в самом чтении.
        Assert.Equal(3, PriorAbsentOrUnreadableExitCountedAsZero("3"));

        // Действующая приёмка читает СОХРАНЁННЫЙ ненулевой код как ненулевой, а не как ноль.
        var savedNonzero = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(state: StateNotAccepted, exitCode: "3"));
        Assert.Equal(3, savedNonzero.ExitCode);

        // И состояние «научный прогон не принят» остаётся непринятым состоянием: ненулевой код
        // выхода не превращается в успех и не теряется.
        Assert.Equal(StateNotAccepted, savedNonzero.State);

        // Код выхода ТЕКСТОМ отвергается до преобразований, а не приводится к нулю молча.
        var textExit = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(exitCode: "\"3\""));
        Assert.False(textExit.Admitted);
        Assert.NotEmpty(textExit.RejectionReasons);
    }

    [Fact]
    public void Re_entry_admission_accepts_an_honest_terminal_receipt_and_refuses_a_text_false_one()
    {
        // Повторный вход в оболочке читает уже существующую расписку и решает, выходить ли её
        // исходом. Здесь закрывается РЕШАЮЩАЯ ЧАСТЬ этого решения: приёмка расписки. Сама ветвь
        // повторного входа написана на языке оболочки под узлами опытов, она мне не назначена и
        // этой проверкой НЕ закрывается — так сказано и в пометке плана.

        // Прежнее правило на том же входе отвечает НЕВЕРНО: строка «false» считалась истиной,
        // поэтому повторный вход объявлял незавершённый прогон завершённым.
        Assert.True(PriorTruthinessOfTerminal("false"));
        // Зависимость от входа: на пустом значении его ответ другой.
        Assert.False(PriorTruthinessOfTerminal(string.Empty));

        // Действующая приёмка принимает честную терминальную расписку: повторный вход вправе
        // выйти её исходом, не запуская прогон заново.
        var honest = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        Assert.True(honest.Admitted);
        Assert.True(honest.TerminalObserved);

        // И отвергает расписку, где терминальность записана ТЕКСТОМ «false».
        var textFalse = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(terminal: "\"false\""));
        Assert.False(textFalse.Admitted);
        Assert.False(textFalse.TerminalObserved);

        // Текст «true» тоже не терминальность: повторный вход не должен выходить и по нему.
        var textTrue = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(terminal: "\"true\""));
        Assert.False(textTrue.Admitted);
    }

    [Fact]
    public void Late_failure_after_a_saved_stage_keeps_that_stage_accepted_and_does_not_close_the_run()
    {
        // Прежнее правило на том же входе отвечает НЕВЕРНО: итог выводился из кода возврата
        // последней команды, поэтому нулевой выход ПОСЛЕ позднего сбоя закрывал прогон.
        Assert.True(PriorOutcomeFromLastExitCode(0));
        // Зависимость от входа: на ненулевом коде его ответ другой.
        Assert.False(PriorOutcomeFromLastExitCode(1));

        // Наблюдения: ранние стадии наблюдены как состоявшиеся — их результат уже сохранён, —
        // а научная стадия позже провалилась.
        var lateFailure = new List<RunStageObservation>
        {
            new("cold-memory-silent", true, true, "из пустой памяти отвечено строк: 0"),
            new("copy-faithful-all-rows", true, true, "точных строк копии: 5 из 5"),
            new("blindness-control", true, true, "протёкших терминов: 0"),
            new("unrelated-row-false-hit", false, false, "ложного попадания не наблюдено"),
            new(ScientificStage, true, false, "после сохранения состояния запись отчёта упала")
        };

        var verdict = RunCompletionFinalizer.Finalize(
            Passport(),
            lateFailure,
            HonestCost(),
            RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson()),
            ExitedTree());

        // Поздний сбой назван своей стадией, а прогон не закрыт.
        Assert.Contains(ScientificStage, verdict.FailedStageIds);
        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(RunCompletionFinalizer.InvalidLearningValidity, verdict.LearningValidity);

        // А уже сохранённые ранние стадии задним числом НЕ обесцениваются: их нет ни среди
        // проваленных, ни среди отсутствующих, ни среди неизвестных.
        Assert.DoesNotContain("copy-faithful-all-rows", verdict.FailedStageIds);
        Assert.DoesNotContain("copy-faithful-all-rows", verdict.MissingStageIds);
        Assert.DoesNotContain("copy-faithful-all-rows", verdict.UnknownStageIds);

        // Названная граница этой проверки: она закрывает УЧЁТ позднего сбоя. Фактическая запись
        // сохранённого состояния на диск до сбоя происходит в программе опыта под узлами
        // опытов, она мне не назначена и этой проверкой не наблюдается.
    }

    // ── Доверенный паспорт и честные наблюдения ─────────────────────────────────

    /// <summary>
    /// Доверенный паспорт прогона ровно того состава, что в программе опыта: семь
    /// положительных стадий, одна отрицательная и сама научная стадия.
    /// </summary>
    private static RunPassport Passport()
        => new(
            ExperimentId,
            AttemptId,
            Source,
            Phase,
            new RunStageExpectation[]
            {
                new("cold-memory-silent", true, new[] { "наблюдено" }),
                new("copy-faithful-all-rows", true, new[] { "наблюдено" }),
                new("blindness-control", true, new[] { "наблюдено" }),
                new("unrelated-row-false-hit", false, new[] { "наблюдено" }),
                new(ScientificStage, true, new[] { StateAccepted, StateNotAccepted })
            },
            new RunCostAssignment[]
            {
                new(ExecuteUnit, 23),
                new(TrainUnit, 9)
            });

    /// <summary>
    /// Наблюдения честного прогона. У отрицательной стадии ожидание ложь, и наблюдение
    /// ложь её проходит: правильное неисполнение — успех.
    /// </summary>
    private static List<RunStageObservation> HonestObservations()
        => new()
        {
            new RunStageObservation("cold-memory-silent", true, true, "из пустой памяти отвечено строк: 0"),
            new RunStageObservation("copy-faithful-all-rows", true, true, "точных строк копии: 5 из 5"),
            new RunStageObservation("blindness-control", true, true, "протёкших терминов: 0"),
            new RunStageObservation("unrelated-row-false-hit", false, false, "ложного попадания не наблюдено"),
            new RunStageObservation(ScientificStage, true, true, "программа дошла до конечного состояния")
        };

    /// <summary>
    /// Записи расхода честного прогона: восемнадцать научных исполнений плюс пять
    /// исполнений контроля реализации и шесть научных обучений плюс три обучения контроля
    /// реализации. Это сохранённые поправки плана для этого опыта.
    /// </summary>
    private static List<RunCostRecord> HonestCost()
        => new()
        {
            new RunCostRecord(AttemptId, ScientificStage, ExecuteUnit, RunCompletionFinalizer.ScientificColumn, 18, true),
            new RunCostRecord(AttemptId, ScientificStage, ExecuteUnit, RunCompletionFinalizer.ImplementationControlColumn, 5, true),
            new RunCostRecord(AttemptId, ScientificStage, TrainUnit, RunCompletionFinalizer.ScientificColumn, 6, true),
            new RunCostRecord(AttemptId, ScientificStage, TrainUnit, RunCompletionFinalizer.ImplementationControlColumn, 3, true)
        };

    /// <summary>Дерево процессов, наблюдённое завершённым.</summary>
    private static RunProcessTreeVerdict ExitedTree()
        => RunProcessTreeObserver.Classify(
            1000,
            new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc),
            true,
            Array.Empty<ObservedProcessRecord>());

    /// <summary>
    /// Собирает текст расписки по частям, чтобы каждая проверка портила ровно одно место.
    /// Значения терминальности и кода выхода подставляются как ТЕКСТ, поэтому проверка
    /// может передать и настоящее логическое значение, и строку, и дробное число.
    /// </summary>
    private static string ReceiptJson(
        string schema = "p117-receipt-v1",
        string experiment = ExperimentId,
        string attempt = AttemptId,
        string source = Source,
        string phase = Phase,
        string stage = ScientificStage,
        string terminal = "true",
        string state = StateAccepted,
        string exitCode = "0",
        string extraMembers = "")
        => "{"
            + "\"receipt_schema\": \"" + schema + "\", "
            + "\"experiment\": \"" + experiment + "\", "
            + "\"attempt\": \"" + attempt + "\", "
            + "\"source\": \"" + source + "\", "
            + "\"phase\": \"" + phase + "\", "
            + "\"stage\": \"" + stage + "\", "
            + "\"terminal\": " + terminal + ", "
            + "\"state\": \"" + state + "\", "
            + "\"exit_code\": " + exitCode
            + extraMembers
            + "}";

    // ── Положительные контроли: зелёные и до правки, и после ────────────────────

    [Fact]
    public void Honest_full_receipt_is_admitted()
    {
        // Ожидаемый ответ назван прежде входа: честная полная расписка принимается.
        var verdict = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());

        Assert.True(verdict.Admitted);
        Assert.Empty(verdict.RejectionReasons);
        Assert.True(verdict.TerminalObserved);
        Assert.Equal(StateAccepted, verdict.State);
        Assert.Equal(0, verdict.ExitCode);

        // Прежнее правило тоже принимало этот вход: положительный контроль зелёный до и после.
        Assert.True(PriorTruthinessOfTerminal("true"));
    }

    [Fact]
    public void Honest_full_run_is_accepted_by_observation()
    {
        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());

        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), receipt, ExitedTree());

        Assert.True(verdict.RunCompletionValid);
        Assert.Equal(RunCompletionFinalizer.ValidLearningValidity, verdict.LearningValidity);
        Assert.Empty(verdict.MissingStageIds);
        Assert.Empty(verdict.UnknownStageIds);
        Assert.Empty(verdict.FailedStageIds);
        Assert.Empty(verdict.DuplicateStageIds);
        Assert.Empty(verdict.UnassignedStageIds);
        Assert.True(verdict.ScientificReceiptAdmitted);
        Assert.Equal(RunProcessTreeObserver.TreeExited, verdict.ProcessTreeState);

        // Прежний итог по нулевому коду возврата тоже был успехом: контроль зелёный до и после.
        Assert.True(PriorOutcomeFromLastExitCode(0));
    }

    // ── Красный случай 1: строковое «false» ─────────────────────────────────────

    [Fact]
    public void Terminal_as_string_false_is_rejected_while_prior_rule_accepted_it()
    {
        // Прежнее правило строки 91: непустая строка приводилась к истине.
        Assert.True(PriorTruthinessOfTerminal("false"));

        // Действующее правило: это не логическое значение, а строка.
        var verdict = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(terminal: "\"false\""));

        Assert.False(verdict.Admitted);
        Assert.False(verdict.TerminalObserved);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("терминальность не логическое значение"));
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("строка"));
    }

    [Fact]
    public void Terminal_as_string_true_is_also_rejected()
    {
        // Строка «true» столь же недопустима: проверяется ВИД значения, а не его текст.
        var verdict = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(terminal: "\"true\""));

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("терминальность не логическое значение"));
    }

    // ── Красный случай 2: пустой корень вместо ошибки ───────────────────────────

    [Fact]
    public void Null_root_is_rejected_while_prior_rule_counted_it_as_parsed_success()
    {
        // Прежнее правило строк 331 и 353: разбор пустого значения не давал ни одной причины.
        Assert.True(PriorEmptyRootCountedAsParsedSuccess("null"));

        // Действующее правило: корень обязан быть объектом, и это проверяется до полей.
        var verdict = RunReceiptAdmission.Admit(Passport(), ScientificStage, "null");

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("корень расписки не объект"));
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("пустое значение"));
    }

    [Fact]
    public void Array_root_is_rejected()
    {
        var verdict = RunReceiptAdmission.Admit(Passport(), ScientificStage, "[]");

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("массив"));
    }

    [Fact]
    public void String_root_is_rejected()
    {
        var verdict = RunReceiptAdmission.Admit(Passport(), ScientificStage, "\"готово\"");

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("строка"));
    }

    [Fact]
    public void Missing_receipt_is_rejected_and_is_not_success()
    {
        var absent = RunReceiptAdmission.Admit(Passport(), ScientificStage, null);
        var empty = RunReceiptAdmission.Admit(Passport(), ScientificStage, "   ");

        Assert.False(absent.Admitted);
        Assert.False(empty.Admitted);
        Assert.Contains(absent.RejectionReasons, reason => reason.Contains("расписки нет"));
    }

    // ── Красный случай 3: нулевой код возврата при неисполненной стадии ─────────

    [Fact]
    public void Zero_exit_code_does_not_close_an_unobserved_stage()
    {
        // Прежний итог выводился из кода возврата последней команды и был успехом.
        Assert.True(PriorOutcomeFromLastExitCode(0));

        // Действующее правило: назначенная стадия не наблюдалась, значит прогон не принят.
        var withoutBlindness = HonestObservations()
            .Where(observation => observation.StageId != "blindness-control")
            .ToList();

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), withoutBlindness, HonestCost(), receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(RunCompletionFinalizer.InvalidLearningValidity, verdict.LearningValidity);
        Assert.Contains("blindness-control", verdict.MissingStageIds);

        // Прежняя пригодность была строковой константой и совпала бы с принятой.
        Assert.NotEqual(PriorConstantLearningValidity(), verdict.LearningValidity);
    }

    [Fact]
    public void Successful_process_exit_without_scientific_receipt_stays_not_accepted()
    {
        // Расписки нет вовсе: успешный выход процесса её не заменяет.
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), null, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.False(verdict.ScientificReceiptAdmitted);
        Assert.Contains(verdict.ReceiptRejectionReasons, reason => reason.Contains("научной расписки нет"));
    }

    // ── Красный случай 4: перерасход отзывает постоянную пригодность ────────────

    [Fact]
    public void Cost_overrun_revokes_validity_while_prior_validity_was_a_constant()
    {
        // Прежняя пригодность была строковой константой независимо от расхода.
        Assert.Equal("VALID_FOR_NAMED_UNIT", PriorConstantLearningValidity());

        // Шестнадцать лишних исполнений сверх назначенных двадцати трёх.
        var overrunCost = HonestCost();
        overrunCost.Add(new RunCostRecord(
            AttemptId, ScientificStage, ExecuteUnit, RunCompletionFinalizer.ScientificColumn, 16, true));

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), overrunCost, receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(RunCompletionFinalizer.InvalidLearningValidity, verdict.LearningValidity);
        Assert.Contains(verdict.Cost.OverrunUnits, unit => unit.Contains(ExecuteUnit));
        Assert.Contains(verdict.Cost.OverrunUnits, unit => unit.Contains("39 против назначенных 23"));
    }

    // ── Паспорт: опыт, попытка, источник, фаза и стадия ────────────────────────

    [Fact]
    public void Wrong_attempt_source_and_phase_are_each_rejected_against_the_passport()
    {
        var wrongAttempt = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(attempt: "attempt-0002"));
        var wrongSource = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(source: "D09 — другое направление"));
        var wrongPhase = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(phase: "P117.9 — чужая фаза"));
        var wrongExperiment = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(experiment: "P116-PRIMARY-01"));

        Assert.False(wrongAttempt.Admitted);
        Assert.False(wrongSource.Admitted);
        Assert.False(wrongPhase.Admitted);
        Assert.False(wrongExperiment.Admitted);
        Assert.Contains(wrongAttempt.RejectionReasons, reason => reason.Contains("attempt"));
        Assert.Contains(wrongSource.RejectionReasons, reason => reason.Contains("source"));
        Assert.Contains(wrongPhase.RejectionReasons, reason => reason.Contains("phase"));
        Assert.Contains(wrongExperiment.RejectionReasons, reason => reason.Contains("experiment"));
    }

    [Fact]
    public void Build_receipt_cannot_close_the_scientific_stage()
    {
        // Расписка стадии сборки не завершает опыт, даже будучи вполне исправной.
        var verdict = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(stage: "build", state: StateAccepted));

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("не завершает стадию"));
    }

    [Fact]
    public void State_outside_the_allowed_list_is_rejected()
    {
        // Произвольный человеческий текст состоянием нужной стадии не считается.
        var verdict = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(state: "всё хорошо"));

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("не входит в допустимый перечень"));
    }

    [Fact]
    public void Repeated_required_member_and_non_integer_exit_code_are_rejected()
    {
        var duplicated = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(extraMembers: ", \"terminal\": false"));
        var fractional = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(exitCode: "0.5"));
        var textualExit = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(exitCode: "\"0\""));

        Assert.False(duplicated.Admitted);
        Assert.Contains(duplicated.RejectionReasons, reason => reason.Contains("повторён"));
        Assert.False(fractional.Admitted);
        Assert.False(textualExit.Admitted);
        Assert.Contains(textualExit.RejectionReasons, reason => reason.Contains("код выхода не целое число"));
    }

    [Fact]
    public void Missing_typed_member_is_rejected_before_any_conversion()
    {
        // Расписка без члена стадии: отказ наступает на составе, до преобразований.
        var withoutStage = "{"
            + "\"receipt_schema\": \"p117-receipt-v1\", "
            + "\"experiment\": \"" + ExperimentId + "\", "
            + "\"attempt\": \"" + AttemptId + "\", "
            + "\"source\": \"" + Source + "\", "
            + "\"phase\": \"" + Phase + "\", "
            + "\"terminal\": true, "
            + "\"state\": \"" + StateAccepted + "\", "
            + "\"exit_code\": 0"
            + "}";

        var verdict = RunReceiptAdmission.Admit(Passport(), ScientificStage, withoutStage);

        Assert.False(verdict.Admitted);
        Assert.Contains(verdict.RejectionReasons, reason => reason.Contains("нет обязательного члена stage"));
    }

    // ── Заранее заданное ожидание и отрицательная стадия ───────────────────────

    [Fact]
    public void Negative_stage_passes_on_false_and_fails_on_true()
    {
        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());

        // Правильное неисполнение проходит: ожидание отрицательной стадии — ложь.
        var correct = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), receipt, ExitedTree());
        Assert.DoesNotContain("unrelated-row-false-hit", correct.FailedStageIds);

        // Ложное попадание отрицательную стадию проваливает, хотя наблюдение «истина».
        var withFalseHit = HonestObservations();
        withFalseHit.RemoveAll(observation => observation.StageId == "unrelated-row-false-hit");
        withFalseHit.Add(new RunStageObservation(
            "unrelated-row-false-hit", false, true, "наблюдено ложное попадание в память"));

        var broken = RunCompletionFinalizer.Finalize(
            Passport(), withFalseHit, HonestCost(), receipt, ExitedTree());

        Assert.False(broken.RunCompletionValid);
        Assert.Contains("unrelated-row-false-hit", broken.FailedStageIds);
    }

    [Fact]
    public void All_true_fields_is_not_the_rule()
    {
        // Требование «все логические поля истинны» сломало бы отрицательную стадию:
        // её наблюдение ложь, и именно это — успех.
        var negative = HonestObservations()
            .Single(observation => observation.StageId == "unrelated-row-false-hit");

        Assert.False(negative.ObservedCompleted);
        Assert.True(negative.Accepted);
        Assert.True(negative.Observed);
    }

    [Fact]
    public void Unknown_observation_is_not_success_and_is_named_separately()
    {
        var unknown = HonestObservations();
        unknown.RemoveAll(observation => observation.StageId == "copy-faithful-all-rows");
        unknown.Add(new RunStageObservation(
            "copy-faithful-all-rows", true, null, "наблюдение не состоялось"));

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), unknown, HonestCost(), receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Contains("copy-faithful-all-rows", verdict.UnknownStageIds);
        Assert.DoesNotContain("copy-faithful-all-rows", verdict.FailedStageIds);
        Assert.DoesNotContain("copy-faithful-all-rows", verdict.MissingStageIds);
    }

    [Fact]
    public void Duplicate_and_unassigned_stages_are_named_separately()
    {
        var messy = HonestObservations();
        messy.Add(new RunStageObservation("blindness-control", true, true, "повтор того же опознания"));
        messy.Add(new RunStageObservation("tidy-up", true, true, "стадии нет в паспорте"));

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), messy, HonestCost(), receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Contains("blindness-control", verdict.DuplicateStageIds);
        Assert.Contains("tidy-up", verdict.UnassignedStageIds);
        Assert.Empty(verdict.MissingStageIds);
    }

    // ── Полная стоимость ───────────────────────────────────────────────────────

    [Fact]
    public void Full_cost_sums_every_category_and_every_physical_attempt()
    {
        // Две физические попытки обязаны войти в одну сумму.
        var twoAttempts = HonestCost();
        twoAttempts.Add(new RunCostRecord(
            "attempt-0002", ScientificStage, ExecuteUnit, RunCompletionFinalizer.ScientificColumn, 4, true));

        var summary = RunCompletionFinalizer.SummarizeCost(Passport(), twoAttempts);

        Assert.Equal(28, summary.ScientificTotal);           // 18 + 6 + 4.
        Assert.Equal(8, summary.ImplementationControlTotal);  // 5 + 3.
        Assert.Equal(36, summary.GrandTotal);                 // Обязательная сумма всех категорий.
        Assert.Equal(2, summary.AttemptIds.Count);
        Assert.Contains("attempt-0002", summary.AttemptIds);
    }

    [Fact]
    public void Implementation_control_is_a_separate_column_of_one_sum_not_a_way_to_zero_the_price()
    {
        var summary = RunCompletionFinalizer.SummarizeCost(Passport(), HonestCost());
        var execute = summary.Units.Single(unit => unit.Unit == ExecuteUnit);

        Assert.Equal(18, execute.ScientificCount);
        Assert.Equal(5, execute.ImplementationControlCount);
        Assert.Equal(23, execute.TotalCount);     // Оба столбца складываются в одну сумму.
        Assert.Equal(23, execute.AssignedCount);
        Assert.Equal(0, execute.Overrun);         // Ровно в пределах назначенного.
    }

    [Fact]
    public void Unobserved_cost_is_not_silently_counted_as_zero()
    {
        var partlyObserved = HonestCost();
        partlyObserved.Add(new RunCostRecord(
            AttemptId, ScientificStage, TrainUnit, RunCompletionFinalizer.ScientificColumn, 0, false));

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), partlyObserved, receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Contains(TrainUnit, verdict.Cost.UnobservedUnits);
        Assert.Contains(verdict.Cost.Units, unit => unit.Unit == TrainUnit && !unit.FullyObserved);
    }

    [Fact]
    public void A_rejected_receipt_does_not_increase_the_train_or_execute_ledger()
    {
        // Отказ приёмки и отказ финализатора не добавляют ни одного прохода обучения или
        // исполнения: сумма после отказа равна сумме входа.
        var before = RunCompletionFinalizer.SummarizeCost(Passport(), HonestCost());

        var rejected = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(terminal: "\"false\""));
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), rejected, ExitedTree());

        Assert.False(rejected.Admitted);
        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(before.GrandTotal, verdict.Cost.GrandTotal);
        Assert.Equal(before.ScientificTotal, verdict.Cost.ScientificTotal);
        Assert.Equal(before.ImplementationControlTotal, verdict.Cost.ImplementationControlTotal);
        Assert.Equal(32, verdict.Cost.GrandTotal); // 18 + 5 + 6 + 3, ни одного лишнего прохода.
    }

    // ── Дерево процессов ──────────────────────────────────────────────────────

    [Fact]
    public void Root_exit_does_not_prove_a_live_grandchild_has_exited()
    {
        // Корень мёртв, потомок мёртв, а внук жив: прежний признак выхода корня объявил бы
        // дерево завершённым.
        var start = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);
        var snapshot = new[]
        {
            new ObservedProcessRecord(2000, 1, start, false),
            new ObservedProcessRecord(2001, 2000, start, false),
            new ObservedProcessRecord(2002, 2001, start, true)
        };

        var verdict = RunProcessTreeObserver.Classify(2000, start, true, snapshot);

        Assert.Equal(RunProcessTreeObserver.TreeAlive, verdict.State);
        Assert.Contains(2002, verdict.AliveProcessIds);
    }

    [Fact]
    public void Enumeration_failure_is_unknown_and_never_an_empty_success()
    {
        var failed = RunProcessTreeObserver.Classify(
            3000, new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc), false, null);
        var absentSnapshot = RunProcessTreeObserver.Classify(
            3000, new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc), true, null);

        Assert.Equal(RunProcessTreeObserver.TreeUnknown, failed.State);
        Assert.Equal(RunProcessTreeObserver.TreeUnknown, absentSnapshot.State);
        Assert.Empty(failed.AliveProcessIds);
        Assert.Contains(failed.Reasons, reason => reason.Contains("неизвестно"));
    }

    [Fact]
    public void Reused_process_number_is_not_mistaken_for_the_former_root()
    {
        // Тот же номер, другое время старта: это другой процесс, и выход он не доказывает.
        var rootStart = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);
        var laterStart = new DateTime(2026, 11, 3, 11, 30, 0, DateTimeKind.Utc);
        var snapshot = new[] { new ObservedProcessRecord(4000, 1, laterStart, true) };

        var verdict = RunProcessTreeObserver.Classify(4000, rootStart, true, snapshot);

        Assert.NotEqual(RunProcessTreeObserver.TreeExited, verdict.State);
        Assert.Contains(verdict.Reasons, reason => reason.Contains("переиспользован"));
    }

    [Fact]
    public void An_unexited_tree_revokes_run_completion()
    {
        var start = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);
        var aliveTree = RunProcessTreeObserver.Classify(
            5000, start, true, new[] { new ObservedProcessRecord(5000, 1, start, true) });

        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), receipt, aliveTree);

        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(RunProcessTreeObserver.TreeAlive, verdict.ProcessTreeState);
    }

    [Fact]
    public void Absent_tree_observation_is_unknown_and_revokes_run_completion()
    {
        var receipt = RunReceiptAdmission.Admit(Passport(), ScientificStage, ReceiptJson());
        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), receipt, null);

        Assert.False(verdict.RunCompletionValid);
        Assert.Equal(RunProcessTreeObserver.TreeUnknown, verdict.ProcessTreeState);
    }

    // ── Расписка не принята значит прогон не закрыт ───────────────────────────

    [Fact]
    public void A_not_accepted_scientific_state_revokes_run_completion()
    {
        // Расписка исправна по форме, но её состояние — «не принят».
        var receipt = RunReceiptAdmission.Admit(
            Passport(), ScientificStage, ReceiptJson(state: StateNotAccepted, exitCode: "7"));

        Assert.True(receipt.Admitted);              // Форма верна: отказа приёмки нет.
        Assert.Equal(StateNotAccepted, receipt.State);
        Assert.Equal(7, receipt.ExitCode);

        // А вот наблюдение научной стадии при этом обязано быть ложью, и прогон не принят.
        var notCompleted = HonestObservations();
        notCompleted.RemoveAll(observation => observation.StageId == ScientificStage);
        notCompleted.Add(new RunStageObservation(
            ScientificStage, true, false, "научная стадия не закрылась"));

        var verdict = RunCompletionFinalizer.Finalize(
            Passport(), notCompleted, HonestCost(), receipt, ExitedTree());

        Assert.False(verdict.RunCompletionValid);
        Assert.Contains(ScientificStage, verdict.FailedStageIds);
    }

    [Fact]
    public void Stage_ledger_and_full_completion_share_one_rule()
    {
        // Учёт состава и расхода — общая часть решения для писателя и читателя расписки.
        // Полное завершение строго сильнее: оно добавляет приёмку расписки и дерево.
        var ledger = RunCompletionFinalizer.FinalizeStages(
            Passport(), HonestObservations(), HonestCost());

        Assert.True(ledger.StagesAndCostValid);
        Assert.Equal(RunCompletionFinalizer.ValidLearningValidity, ledger.LearningValidity);

        var withoutReceipt = RunCompletionFinalizer.Finalize(
            Passport(), HonestObservations(), HonestCost(), null, ExitedTree());

        Assert.False(withoutReceipt.RunCompletionValid);
        Assert.Equal(ledger.Cost.GrandTotal, withoutReceipt.Cost.GrandTotal);
    }
}
