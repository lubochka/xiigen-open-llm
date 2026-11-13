// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Это раздел ремонта про настоящую память перевода и честный смысл результата (шестой по
// порядку разделов в файле codex-139-post117-addressed-repair-plan-2026-11-03.md).
//
// Что было неправильно. Приёмка состояния памяти довольствовалась двумя показателями: числом
// записей («девять») и свёрткой ожидаемого журнала. Ни один из них о памяти ничего не говорит.
// Число девять совпадает у множества совершенно разных состояний: испортить один перевод,
// переставить две записи внутри ячейки, переименовать ключ, перенести запись из одной ячейки в
// другую — число останется девять. А свёртка ожидаемого журнала, сравниваемая сама с собой,
// равна себе ВСЕГДА: такая проверка не может упасть ни на одном состоянии, то есть не является
// проверкой вовсе. Это и есть самоподтверждение: если ожидаемое строится тем же кодом, что и
// проверяемое, их согласие — согласие кода с самим собой.
//
// Что стало. Журнал ожидаемых записей приходит в сверку как ДАННЫЕ, объявленные тем, кто учил.
// В случаях ниже эти данные — литералы самой проверки: они не вычислены ни хранилищем памяти,
// ни разборщиком текста, ни построителем ключей. Сверка сравнивает состав ячеек, источник и
// перевод каждой записи, кратности и порядок внутри ячейки, и каждое расхождение называет своим
// кодом.
//
// Прежнее правило воспроизведено функциями ОТ ВХОДА: PriorCountAndJournalHashAccepts принимает
// фактическое число записей и журнал, PriorJournalHashComparedWithItself принимает журнал.
// Каждое красное утверждение двойное: прежнее правило на том же входе отвечает НЕВЕРНО
// (принимает испорченную память), действующая сверка — верно. Зависимость от входа показана
// вторым входом.
//
// Чего эти проверки НЕ доказывают, и я этого не заявляю. Первое: фактическое состояние
// наблюдается через продуктовую выгрузку состояния, а не чтением живых структур в памяти
// процесса, поэтому сверка видит картину выгрузки; если бы сама выгрузка теряла записи, эти
// проверки такого не заметили бы — это ненаблюдённая граница. Второе: сверка доказывает, что
// обучение ЗАПИСАЛО ожидаемое, и не доказывает, что чтение этим пользуется. Третье: правило
// построения ключа разборщиком здесь намеренно не перепроверяется, ключи приходят литералами.
// Четвёртое: в живом случае ниже журнал снят с ЧИСТОЙ выгрузки, и он годится ТОЛЬКО как проба
// чувствительности к порче — доказательством совпадения он быть не может, потому что снят с
// самого проверяемого состояния, и я называю это прямо.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений этой
// среды, я их не запускал и зелёными не называю.
public sealed class TranslationKnnStateAuditTests
{
    /// <summary>Ключ первой ячейки памяти в журнале проверок.</summary>
    private const string CellOne = "IMPERATIVE_V|ADD_OP";

    /// <summary>Ключ второй ячейки памяти в журнале проверок.</summary>
    private const string CellTwo = "IMPERATIVE_V|VALIDATE_CHECK";

    /// <summary>Источник первого урока.</summary>
    private const string SourceOne = "Он проверил файл";

    /// <summary>Перевод первого урока.</summary>
    private const string TargetOne = "He checked the file";

    /// <summary>Источник второго урока.</summary>
    private const string SourceTwo = "Он удалил файл";

    /// <summary>Перевод второго урока.</summary>
    private const string TargetTwo = "He deleted the file";

    // ── Прежние правила, воспроизведённые для доказательства красноты ───────────

    /// <summary>
    /// ПРЕЖНЕЕ правило приёмки состояния: сверялось число записей и свёртка ожидаемого журнала.
    /// Настоящая функция от входа: ответ считается из фактического числа записей и журнала.
    /// Свёртка журнала в ответ не входит намеренно — ниже отдельно показано, почему она не
    /// добавляет ничего. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorCountAndJournalHashAccepts(
        int actualEntryCount,
        IReadOnlyList<TranslationKnnExpectedWrite> journal)
        => actualEntryCount == journal.Count; // Совпало число — и прежней приёмке этого хватало.

    /// <summary>
    /// ПРЕЖНЯЯ вторая половина приёмки: свёртка ожидаемого журнала сравнивалась сама с собой.
    /// Функция от входа, и её ответ — всегда истина на любом входе: именно это и есть
    /// самоподтверждение. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorJournalHashComparedWithItself(
        IReadOnlyList<TranslationKnnExpectedWrite> journal)
        => JournalFingerprint(journal) == JournalFingerprint(journal); // Равно себе всегда.

    /// <summary>Свёртка журнала: порядок, ключи, источники и переводы в одной строке.</summary>
    private static string JournalFingerprint(IReadOnlyList<TranslationKnnExpectedWrite> journal)
        => string.Join("\u0001", journal.Select(write =>
            write.Signature + "\u0002" + write.Source + "\u0002" + write.Target));

    // ── Случай 1: испорчен один перевод при том же числе записей ────────────────

    [Fact]
    public void OneCorruptedTarget_PassedThePriorCountCheck_AndIsRefusedByTheAudit()
    {
        var journal = HonestJournal();                 // Объявленное ожидаемое: три записи.
        var honest = MemoryOf(journal);                // Честная память, совпадающая с журналом.
        var corrupted = MemoryOf(                      // Та же память с ОДНИМ испорченным переводом.
            (CellOne, SourceOne, TargetOne),
            (CellOne, SourceTwo, "He removed the file"), // Было «deleted», стало «removed».
            (CellTwo, SourceOne, TargetOne));

        // Число записей у честной и у испорченной памяти одно и то же.
        Assert.Equal(honest.Entries.Count, corrupted.Entries.Count);

        // Прежнее правило на том же входе отвечает НЕВЕРНО: оно принимает испорченную память,
        // потому что число записей совпало.
        Assert.True(PriorCountAndJournalHashAccepts(corrupted.Entries.Count, journal));

        // И оно даёт ОДИНАКОВЫЙ ответ на честной и на испорченной памяти — то есть различить их
        // не способно в принципе.
        Assert.Equal(
            PriorCountAndJournalHashAccepts(honest.Entries.Count, journal),
            PriorCountAndJournalHashAccepts(corrupted.Entries.Count, journal));

        // Действующая сверка отвечает верно: испорченный перевод назван своим кодом.
        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(corrupted, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.TargetMismatchCode, refused.ErrorCode);

        // А честную память она принимает: отказ не огульный.
        var accepted = TranslationKnnStateAudit.RequireMemoryMatchesJournal(honest, journal);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);

        // Прежнее правило действительно зависит от входа: при другом числе записей его ответ
        // меняется. Беда не в том, что оно отвечает всегда одно, а в том, что оно смотрит только
        // на число.
        Assert.False(PriorCountAndJournalHashAccepts(corrupted.Entries.Count - 1, journal));
    }

    // ── Случай 2: свёртка ожидаемого журнала не может упасть вовсе ──────────────

    [Fact]
    public void JournalHashComparedWithItself_IsTrueEvenForMemoryThatSharesNothing()
    {
        var journal = HonestJournal();   // Объявленное ожидаемое.
        var foreign = MemoryOf(          // Память, не имеющая с журналом ничего общего.
            ("OTHER|CELL", "Совсем другой источник", "Totally different target"),
            ("OTHER|CELL", "И ещё один", "And one more"),
            ("OTHER|CELL", "И третий", "And a third"));

        // Свёртка журнала, сравниваемая сама с собой, истинна при любом состоянии памяти.
        Assert.True(PriorJournalHashComparedWithItself(journal));
        // И число записей здесь тоже совпадает, поэтому прежняя приёмка принимает и это.
        Assert.True(PriorCountAndJournalHashAccepts(foreign.Entries.Count, journal));

        // Действующая сверка отвергает: в памяти нет объявленной журналом ячейки.
        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(foreign, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.MissingSignatureCode, refused.ErrorCode);

        // Зависимость от входа у воспроизведённой свёртки: на другом журнале значение свёртки
        // другое, то есть сама свёртка от входа зависит — не зависит от входа только её
        // сравнение с собой, и именно это делало прежнюю приёмку пустой.
        Assert.NotEqual(JournalFingerprint(journal), JournalFingerprint(ShuffledJournal()));
    }

    // ── Случай 3: переименован ключ ячейки при том же числе записей ─────────────

    [Fact]
    public void MutatedSignature_PassedThePriorCountCheck_AndIsRefusedByTheAudit()
    {
        var journal = HonestJournal();
        var mutated = MemoryOf(
            (CellOne, SourceOne, TargetOne),
            (CellOne, SourceTwo, TargetTwo),
            ("IMPERATIVE_V|VALIDATE_CHECK_MUTATED", SourceOne, TargetOne)); // Ключ подменён.

        Assert.True(PriorCountAndJournalHashAccepts(mutated.Entries.Count, journal));

        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(mutated, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.MissingSignatureCode, refused.ErrorCode);
    }

    // ── Случай 4: переставлены записи внутри одной ячейки ───────────────────────

    [Fact]
    public void SwappedOrderInsideOneCell_PassedThePriorCountCheck_AndIsRefusedByTheAudit()
    {
        var journal = HonestJournal();
        var swapped = MemoryOf(
            (CellOne, SourceTwo, TargetTwo),   // Записи ячейки стоят в обратном порядке.
            (CellOne, SourceOne, TargetOne),
            (CellTwo, SourceOne, TargetOne));

        // Состав тот же, число то же — прежняя приёмка принимает.
        Assert.True(PriorCountAndJournalHashAccepts(swapped.Entries.Count, journal));

        // Порядок памяти значим: легшая позже запись несёт более сильный учебный сигнал.
        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(swapped, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.OrderMismatchCode, refused.ErrorCode);
    }

    // ── Случай 5: запись перенесена из одной ячейки в другую ───────────────────

    [Fact]
    public void MovedEntryBetweenCells_KeepsTheTotalCount_AndIsRefusedByMultiplicity()
    {
        var journal = HonestJournal();
        var moved = MemoryOf(
            (CellOne, SourceOne, TargetOne),
            (CellTwo, SourceTwo, TargetTwo),   // Вторая запись первой ячейки ушла во вторую.
            (CellTwo, SourceOne, TargetOne));

        // Общее число записей не изменилось — прежняя приёмка переноса не замечает.
        Assert.True(PriorCountAndJournalHashAccepts(moved.Entries.Count, journal));

        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(moved, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.MultiplicityMismatchCode, refused.ErrorCode);
    }

    // ── Случай 6: подменён источник при том же переводе и числе ────────────────

    [Fact]
    public void ReplacedSource_WithSameTargetAndCount_IsRefusedBySource()
    {
        var journal = HonestJournal();
        var replaced = MemoryOf(
            (CellOne, "Он открыл файл", TargetOne), // Источник подменён, перевод прежний.
            (CellOne, SourceTwo, TargetTwo),
            (CellTwo, SourceOne, TargetOne));

        Assert.True(PriorCountAndJournalHashAccepts(replaced.Entries.Count, journal));

        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(replaced, journal);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.SourceMismatchCode, refused.ErrorCode);
    }

    // ── Случай 7: пустой журнал не является доказательством совпадения ─────────

    [Fact]
    public void EmptyJournalIsRefused_InsteadOfSilentlyCountingAsAMatch()
    {
        var honest = MemoryOf(HonestJournal());

        // Пустой журнал: прежняя приёмка приняла бы пустую память по равенству нулей.
        Assert.True(PriorCountAndJournalHashAccepts(0, []));

        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(honest, []);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.EmptyJournalCode, refused.ErrorCode);
    }

    // ── Случай 8: живое обучение, продуктовая выгрузка и проба чувствительности ─

    [Fact]
    public async Task LiveLessonWritesItsSourceAndTargetIntoMemory_AndTheAuditNoticesCorruption()
    {
        var machine = new TranslationMachine();

        // Обучение идёт обычным продуктовым путём структурной партии.
        var trained = await machine.TrainStructuredBatchAsync(LessonBatch("knn-audit-live"));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Состояние наблюдается продуктовой выгрузкой. Это и есть названная граница: живые
        // структуры в памяти процесса напрямую здесь не читаются.
        var exported = machine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var memory = exported.Value!.KnnStore;

        // НЕЗАВИСИМЫЙ инвариант, не вычисленный ни разборщиком, ни построителем ключей: урок
        // записал в память только свои источник и перевод, и записал их хотя бы однажды.
        Assert.NotEmpty(memory.Entries);
        Assert.All(memory.Entries, entry => Assert.Equal(SourceOne, entry.Source));
        Assert.All(memory.Entries, entry => Assert.Equal(TargetOne, entry.Target));

        // Числа записей на урок эта проверка намеренно НЕ закрепляет: по коду исполнителя их три
        // на учебную строку (ключ урока, приставка и структура), но сколько учебных строк даёт
        // сборщик партии — его дело, и переписывать сборщик здесь значило бы проверять код его же
        // кодом.

        // Дальше — ПРОБА ЧУВСТВИТЕЛЬНОСТИ, и только она. Журнал ниже снят с ЧИСТОЙ выгрузки,
        // поэтому доказательством совпадения он быть не может: он снят с самого проверяемого
        // состояния. Годен он ровно для одного: показать, что сверка замечает порчу настоящих
        // выгруженных байтов.
        var journalFromCleanExport = memory.Entries
            .Select(entry => new TranslationKnnExpectedWrite(entry.Signature, entry.Source, entry.Target))
            .ToList();

        var corrupted = new TranslationKnnStoreCheckpoint(
            memory.Entries
                .Select((entry, index) => index == 0
                    ? new TranslationKnnStoreEntryCheckpoint(entry.Signature, entry.Source, "He removed the file")
                    : entry)
                .ToList());

        // Число записей порча не изменила — прежняя приёмка её не заметила бы.
        Assert.Equal(memory.Entries.Count, corrupted.Entries.Count);
        Assert.True(PriorCountAndJournalHashAccepts(corrupted.Entries.Count, journalFromCleanExport));

        // А сверка замечает и называет причину.
        var refused = TranslationKnnStateAudit.RequireMemoryMatchesJournal(corrupted, journalFromCleanExport);
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationKnnStateAudit.TargetMismatchCode, refused.ErrorCode);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>Честный журнал ожидаемого: литералы проверки, а не вычисленное состояние.</summary>
    private static List<TranslationKnnExpectedWrite> HonestJournal()
        =>
        [
            new TranslationKnnExpectedWrite(CellOne, SourceOne, TargetOne),
            new TranslationKnnExpectedWrite(CellOne, SourceTwo, TargetTwo),
            new TranslationKnnExpectedWrite(CellTwo, SourceOne, TargetOne)
        ];

    /// <summary>Журнал с другим составом: нужен для доказательства зависимости свёртки от входа.</summary>
    private static List<TranslationKnnExpectedWrite> ShuffledJournal()
        =>
        [
            new TranslationKnnExpectedWrite(CellTwo, SourceOne, TargetOne),
            new TranslationKnnExpectedWrite(CellOne, SourceTwo, TargetTwo),
            new TranslationKnnExpectedWrite(CellOne, SourceOne, TargetOne)
        ];

    /// <summary>Выгрузка памяти, собранная ровно из объявленного журнала.</summary>
    private static TranslationKnnStoreCheckpoint MemoryOf(
        IReadOnlyList<TranslationKnnExpectedWrite> journal)
        => new([.. journal.Select(write =>
            new TranslationKnnStoreEntryCheckpoint(write.Signature, write.Source, write.Target))]);

    /// <summary>Выгрузка памяти, собранная из перечисленных троек.</summary>
    private static TranslationKnnStoreCheckpoint MemoryOf(
        params (string Signature, string Source, string Target)[] entries)
        => new([.. entries.Select(entry =>
            new TranslationKnnStoreEntryCheckpoint(entry.Signature, entry.Source, entry.Target))]);

    /// <summary>Учебная партия одного урока, собранная настоящим продуктовым сборщиком.</summary>
    private static TranslationLearningBatch LessonBatch(string batchId)
        => TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            batchId,
            [
                new TranslationTrainReadyPair(
                    Id: batchId + "-p01",
                    Source: "simulation",
                    Direction: "RU_EN",
                    PairFamily: "tense",
                    Prompt: "Переведи",
                    ASide: SourceOne,
                    Chosen: TargetOne,
                    Rejected: "He checks the file [WRONG: tense]",
                    ValidationOutcome: "train_ready",
                    ValidationErrors: [],
                    VerifiedRuleTags: ["TENSE_PAST"])
            ],
            signatureMode: TranslationSignatureMode.ExactPair);
}
