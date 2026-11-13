// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;
using Xunit;

// ПЕРЕНОС строки очереди переносов 42, идентификатор работы FIX-IDX, карточка
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 52-translation-training-identity-index.md.
//
// ЧТО ИМЕННО ПЕРЕНЕСЕНО ПО-ЧЕЛОВЕЧЕСКИ. Внутри настоящего обучения партии машина перевода
// ведёт журнал учебных опознаний: «такой пример такой единицей уже учили». Раньше на каждый
// урок партии она просматривала этот журнал целиком, чтобы понять, записана ли пара
// «опознание примера + опознание единицы». Теперь рядом с рабочей копией журнала живёт
// локальный набор тех же пар, и ответ берётся из набора за одно обращение.
//
// ЧТО ЭТИ ПРОВЕРКИ ДОКАЗЫВАЮТ. Только ПОВЕДЕНЧЕСКУЮ РАВНОСИЛЬНОСТЬ и сохранность
// инвариантов: тот же журнал, тот же порядок первого появления, те же метаданные первой
// записи, та же регистрозависимость, та же сохранность старых дублей при восстановлении и
// та же неделимость публикации при отказе строки партии.
//
// ЧЕГО ЭТИ ПРОВЕРКИ НЕ ДОКАЗЫВАЮТ. Скорость здесь НЕ измеряется ни одной строкой. Числа
// исходного опыта (обучение 22,2294 → 19,2621 мс) получены один раз в фиксированном порядке
// на другом стенде и к этим проверкам отношения не имеют. Рядом с тем выигрышем по времени
// в опыте стоят ухудшения по памяти: +8 200 байт на обучение, +38 952 байта на
// восстановление, +528 384 байта пика. Называть эту правку экономией памяти запрещено.
public sealed class TrainingIdentityIndexTransferTests
{
    // Тема урока: как в панели карточки — техническая.
    private const string Topic = "technical";

    // Язык источника уроков: английский.
    private const string SourceLanguage = "en";

    // Единица-получатель уроков: исполнитель направления с английского на русский.
    private static readonly string ExecutorUnit = TranslationUnitIds.ExecutorEnRu;

    // Настройки записи учебных текстов: перечисления пишутся словами.
    private static readonly JsonSerializerOptions LessonJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // ПРОВЕРКА 1. Знакомая пара обучается ПОВТОРНО, а первая запись опознания остаётся
    // первой вместе со своими исходными метаданными.
    //
    // Зачем эта проверка нужна. Самая опасная ошибка переноса — перенести вопрос «пара уже
    // записана?» ВЫШЕ вызова обучения и пропустить знакомый урок целиком. Внешне журнал при
    // этом выглядел бы правильно, а обучение молча перестало бы происходить. Здесь знакомый
    // урок предъявляется второй раз с ДРУГИМ переводом, и память исполнителя обязана
    // показать новый перевод.
    [Fact]
    public async Task KnownPairIsStillTrainedWhileItsFirstIdentityRecordKeepsOriginalMetadata()
    {
        // Свежая машина перевода без обученного состояния.
        var machine = new TranslationMachine();

        // Исходный текст урока, который будет предъявлен дважды.
        const string source = "open file";

        // Первый выученный перевод этого текста.
        const string firstTarget = "открой файл";

        // Второй, ИНОЙ перевод того же текста: по нему и видно, состоялось ли обучение.
        const string secondTarget = "открой документ";

        // Первое предъявление урока отдельной партией со своим происхождением.
        var firstBatch = BuildBatch(
            "FIX-IDX-first",
            "fix-idx-source-first",
            BuildLesson("lesson-open-file", source, firstTarget, "fix-idx-source-first"));

        // Партия обязана примениться: иначе дальше проверять нечего.
        var firstResult = await machine.TrainStructuredBatchAsync(firstBatch);
        Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);

        // Состояние памяти соседей сразу после первого урока: это основа для сравнения.
        var knnAfterFirst = ExportCheckpoint(machine).KnnStore.Entries.Count;

        // Второе предъявление ТОЙ ЖЕ пары: опознание примера и единица те же, а перевод,
        // происхождение и партия другие.
        var secondBatch = BuildBatch(
            "FIX-IDX-second",
            "fix-idx-source-second",
            BuildLesson("lesson-open-file", source, secondTarget, "fix-idx-source-second"));

        // Повторная партия тоже обязана примениться: знакомый урок отказом не является.
        var secondResult = await machine.TrainStructuredBatchAsync(secondBatch);
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);

        // Состояние после повторного урока читается одной выгрузкой.
        var checkpoint = ExportCheckpoint(machine);

        // ПЕРВОЕ доказательство состоявшегося обучения: точная память исполнителя несёт
        // НОВЫЙ перевод. Если бы проверку пары перенесли до обучения, здесь остался бы старый.
        var executor = SingleExecutor(checkpoint);
        var targets = (executor.TopicSourceMap ?? []).Select(entry => entry.Target).ToArray();
        Assert.Contains(secondTarget, targets);
        Assert.DoesNotContain(firstTarget, targets);

        // ВТОРОЕ доказательство: память ближайших соседей выросла, потому что обучение
        // единицы складывает записи списком и повтор не снимает.
        Assert.True(
            checkpoint.KnnStore.Entries.Count > knnAfterFirst,
            $"Повторный урок не добавил записей памяти соседей: было {knnAfterFirst}, " +
            $"стало {checkpoint.KnnStore.Entries.Count}.");

        // А вот журнал опознаний повторной записи НЕ получает: пара в нём ровно одна.
        var journal = checkpoint.StructuredTrainingIdentities;
        Assert.Single(journal);

        // И эта единственная запись — ПЕРВАЯ: её происхождение и партия остались исходными.
        Assert.Equal("lesson-open-file", journal[0].ExampleId);
        Assert.Equal(ExecutorUnit, journal[0].UnitId);
        Assert.Equal("fix-idx-source-first", journal[0].SourceId);
        Assert.Equal("FIX-IDX-first", journal[0].BatchId);
    }

    // ПРОВЕРКА 2. Для последовательности уроков с повторами журнал совпадает с тем, что
    // давал прежний полный перебор.
    //
    // Ожидание здесь не выписано руками, а ВЫЧИСЛЕНО отдельной образцовой реализацией
    // прежнего правила «первое появление пары выигрывает». Поэтому проверка сравнивает две
    // независимые реализации одного договора, а не подгоняет ожидание под продукт.
    [Fact]
    public async Task JournalOfRepeatedLessonsMatchesFirstAppearanceRuleOfTheOldFullScan()
    {
        // Свежая машина перевода.
        var machine = new TranslationMachine();

        // Три партии с умышленными повторами: внутри одной партии, между партиями и
        // совершенно новый урок последним.
        var batches = new[]
        {
            BuildBatch(
                "FIX-IDX-journal-1",
                "fix-idx-journal-source-1",
                BuildLesson("lesson-save", "save draft", "сохрани черновик", "fix-idx-journal-source-1"),
                BuildLesson("lesson-upload", "upload image", "загрузи картинку", "fix-idx-journal-source-1"),
                // Повтор ВНУТРИ одной партии: пара уже занята первой строкой этой же партии.
                BuildLesson("lesson-save", "save draft", "сохрани набросок", "fix-idx-journal-source-1")),
            BuildBatch(
                "FIX-IDX-journal-2",
                "fix-idx-journal-source-2",
                // Повтор МЕЖДУ партиями: пара уже записана прошлой партией.
                BuildLesson("lesson-upload", "upload image", "загрузи изображение", "fix-idx-journal-source-2"),
                BuildLesson("lesson-archive", "archive log", "заархивируй журнал", "fix-idx-journal-source-2")),
            BuildBatch(
                "FIX-IDX-journal-3",
                "fix-idx-journal-source-3",
                BuildLesson("lesson-download", "download package", "скачай пакет", "fix-idx-journal-source-3"))
        };

        // Все три партии применяются по порядку: отказа здесь быть не должно.
        foreach (var batch in batches)
        {
            var applied = await machine.TrainStructuredBatchAsync(batch);
            Assert.True(applied.IsSuccess, applied.ErrorMessage);
        }

        // Фактический экспортируемый журнал.
        var actual = ExportCheckpoint(machine).StructuredTrainingIdentities;

        // Образцовый журнал прежнего правила, посчитанный независимо от продукта.
        var expected = FirstAppearanceJournal(batches);

        // Число записей совпадает: повторы не удвоились и новые уроки не пропали.
        Assert.Equal(expected.Count, actual.Count);

        // Совпадает КАЖДАЯ запись целиком, в том же порядке экспорта: опознание примера,
        // происхождение первой записавшей строки, единица и партия первого появления.
        for (var position = 0; position < expected.Count; position++)
        {
            Assert.Equal(expected[position].ExampleId, actual[position].ExampleId);
            Assert.Equal(expected[position].SourceId, actual[position].SourceId);
            Assert.Equal(expected[position].UnitId, actual[position].UnitId);
            Assert.Equal(expected[position].BatchId, actual[position].BatchId);
        }
    }

    // ПРОВЕРКА 3. Пары, отличающиеся ТОЛЬКО регистром опознания примера или опознания
    // единицы, считаются разными парами.
    //
    // Карточка запрещает приведение к нижнему регистру, нормализацию псевдонимов и
    // сравнение только по свёртке. Регистр опознания примера проверяется обучением двух
    // строк, а регистр опознания единицы — через журнал, восстановленный из контрольной
    // точки: единицы с иным регистром имени в машине нет, и обучением её не предъявить.
    [Fact]
    public async Task PairsDifferingOnlyByLetterCaseAreTreatedAsDifferentPairs()
    {
        // Первая часть: регистр опознания ПРИМЕРА.
        var machine = new TranslationMachine();

        // Две строки одной партии, различающиеся только регистром опознания примера.
        var caseBatch = BuildBatch(
            "FIX-IDX-case",
            "fix-idx-case-source",
            BuildLesson("lesson-open", "open file", "открой файл", "fix-idx-case-source"),
            BuildLesson("LESSON-OPEN", "open file", "открой файл", "fix-idx-case-source"));

        // Партия применяется: разный регистр законен, это просто две разные строки.
        var applied = await machine.TrainStructuredBatchAsync(caseBatch);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);

        // В журнале ДВЕ записи: регистрозависимое сравнение считает эти пары разными.
        var journal = ExportCheckpoint(machine).StructuredTrainingIdentities;
        Assert.Equal(2, journal.Count);

        // Оба опознания примера присутствуют дословно, без приведения регистра.
        Assert.Contains("lesson-open", journal.Select(identity => identity.ExampleId));
        Assert.Contains("LESSON-OPEN", journal.Select(identity => identity.ExampleId));

        // Вторая часть: регистр опознания ЕДИНИЦЫ.
        //
        // Журнал восстановленной машины содержит запись той же пары, но с именем единицы в
        // другом регистре. Если бы ключ сравнивался без учёта регистра, эта запись подавила
        // бы новый урок и журнал остался бы с одной записью.
        var seeded = await MachineWithSeededJournal(
            new TranslationStructuredTrainingIdentityCheckpoint(
                ExampleId: "lesson-open",
                SourceId: "fix-idx-seeded-source",
                UnitId: ExecutorUnit.ToUpperInvariant(),
                BatchId: "FIX-IDX-seeded"));

        // Урок предъявляется настоящей единице, имя которой записано в обычном регистре.
        var seededApplied = await seeded.TrainStructuredBatchAsync(BuildBatch(
            "FIX-IDX-case-unit",
            "fix-idx-case-unit-source",
            BuildLesson("lesson-open", "open file", "открой файл", "fix-idx-case-unit-source")));
        Assert.True(seededApplied.IsSuccess, seededApplied.ErrorMessage);

        // Записей стало две: восстановленная с именем единицы в верхнем регистре и новая с
        // настоящим именем единицы.
        var seededJournal = ExportCheckpoint(seeded).StructuredTrainingIdentities;
        Assert.Equal(2, seededJournal.Count);

        // Оба имени единицы присутствуют дословно.
        Assert.Contains(ExecutorUnit, seededJournal.Select(identity => identity.UnitId));
        Assert.Contains(ExecutorUnit.ToUpperInvariant(), seededJournal.Select(identity => identity.UnitId));
    }

    // ПРОВЕРКА 4. Устаревшие дубли журнала при восстановлении СОХРАНЯЮТСЯ.
    //
    // Нынешняя схема допускает в сохранённом журнале несколько записей с одной парой —
    // например у состояний, записанных до появления правила первого появления. Загрузка не
    // имеет права дедуплицировать их или переписывать метаданные: это чужие сохранённые
    // данные. Восстановление этой правкой не менялось, и проверка это закрепляет.
    [Fact]
    public async Task RestoreKeepsLegacyDuplicateJournalRecordsInsteadOfDeduplicatingThem()
    {
        // Две записи ОДНОЙ пары с разным происхождением и разными партиями.
        var firstDuplicate = new TranslationStructuredTrainingIdentityCheckpoint(
            ExampleId: "lesson-legacy",
            SourceId: "fix-idx-legacy-source-1",
            UnitId: ExecutorUnit,
            BatchId: "FIX-IDX-legacy-1");
        var secondDuplicate = new TranslationStructuredTrainingIdentityCheckpoint(
            ExampleId: "lesson-legacy",
            SourceId: "fix-idx-legacy-source-2",
            UnitId: ExecutorUnit,
            BatchId: "FIX-IDX-legacy-2");

        // Машина, восстановленная из контрольной точки с этими двумя записями.
        var machine = await MachineWithSeededJournal(firstDuplicate, secondDuplicate);

        // Экспортируемый журнал восстановленной машины.
        var journal = ExportCheckpoint(machine).StructuredTrainingIdentities;

        // Обе записи на месте: снятия дублей при восстановлении нет.
        Assert.Equal(2, journal.Count);

        // Метаданные обоих дублей сохранены дословно, ни одна запись не переписана другой.
        Assert.Contains("fix-idx-legacy-source-1", journal.Select(identity => identity.SourceId));
        Assert.Contains("fix-idx-legacy-source-2", journal.Select(identity => identity.SourceId));
        Assert.Contains("FIX-IDX-legacy-1", journal.Select(identity => identity.BatchId));
        Assert.Contains("FIX-IDX-legacy-2", journal.Select(identity => identity.BatchId));
    }

    // ПРОВЕРКА 5. При сбое обучения строки партии журнал живой версии не меняется ни одной
    // записью.
    //
    // Это главный инвариант неделимости публикации, из-за которого набор сделан ЛОКАЛЬНЫМ, а
    // не полем машины. Партия несёт две строки: первая законна и дописала бы опознание,
    // вторая несёт число там, где единица ждёт текст, и обучение этой строки падает. Отказ
    // обязан оставить журнал ровно таким, каким он был до партии.
    [Fact]
    public async Task FailedBatchRowLeavesTheLiveJournalWithoutASingleNewRecord()
    {
        // Свежая машина перевода.
        var machine = new TranslationMachine();

        // Один законный урок, чтобы журнал был непустым до испорченной партии.
        var prepared = await machine.TrainStructuredBatchAsync(BuildBatch(
            "FIX-IDX-atomic-prepare",
            "fix-idx-atomic-source",
            BuildLesson("lesson-prepared", "save draft", "сохрани черновик", "fix-idx-atomic-source")));
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);

        // Журнал ДО испорченной партии: ровно одна запись подготовленного урока.
        var journalBefore = ExportCheckpoint(machine).StructuredTrainingIdentities;
        Assert.Single(journalBefore);

        // Строка с негодным ТИПОМ поля: текст документа читается, а разбор в типовой договор
        // единицы падает, потому что на месте исходного текста стоит число.
        var brokenLesson = BuildLesson(
            "lesson-broken",
            "upload image",
            "загрузи картинку",
            "fix-idx-atomic-source") with
        {
            InputJson = "{\"SourceText\":123,\"SourceLang\":\"en\",\"TopicDomain\":\"technical\"}"
        };

        // Партия из двух строк: первая законна и идёт первой, вторая падает.
        var failing = await machine.TrainStructuredBatchAsync(BuildBatch(
            "FIX-IDX-atomic-fail",
            "fix-idx-atomic-source",
            BuildLesson("lesson-new", "download package", "скачай пакет", "fix-idx-atomic-source"),
            brokenLesson));

        // Партия отвергнута целиком, и код отказа — именно сбой строки обучения.
        Assert.False(failing.IsSuccess);
        Assert.Equal("BATCH_ROW_TRAINING_FAILED", failing.ErrorCode);

        // Журнал живой версии не изменился: ни записи законной строки той же партии, ни
        // записи упавшей строки в нём нет.
        var journalAfter = ExportCheckpoint(machine).StructuredTrainingIdentities;
        Assert.Single(journalAfter);
        Assert.Equal(journalBefore[0].ExampleId, journalAfter[0].ExampleId);
        Assert.Equal(journalBefore[0].SourceId, journalAfter[0].SourceId);
        Assert.Equal(journalBefore[0].BatchId, journalAfter[0].BatchId);
        Assert.DoesNotContain("lesson-new", journalAfter.Select(identity => identity.ExampleId));
        Assert.DoesNotContain("lesson-broken", journalAfter.Select(identity => identity.ExampleId));
    }

    // ── вспомогательные построения ────────────────────────────────────────────────

    // BuildLesson собирает один законный учебный урок исполнителя по форме предметной области.
    private static TranslationLearningExample BuildLesson(
        string exampleId,
        string source,
        string target,
        string sourceId)
    {
        // Смысловая подпись берётся у настоящего разборщика, а не придумывается.
        var signature = new TranslationSemanticTokenizer().Tokenize(source, SourceLanguage).Signature;

        // Строка собирается ровно по форме учебной записи области перевода.
        return new TranslationLearningExample(
            ExampleId: exampleId,
            UnitId: ExecutorUnit,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new ExecutorInput(source, SourceLanguage, Topic), LessonJson),
            ChosenOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput(target, 0.95, null, null), LessonJson),
            RejectedOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput("[WRONG: missing translation]", 0.1, null), LessonJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: sourceId,
            HeldOut: false);
    }

    // BuildBatch собирает партию обучения из готовых уроков.
    private static TranslationLearningBatch BuildBatch(
        string batchId,
        string sourceId,
        params TranslationLearningExample[] examples)
        // Опознание партии, предметная область, сами уроки и список происхождений.
        => new(batchId, TranslationMachine.DomainTag, examples, new[] { sourceId });

    // ExportCheckpoint читает полное состояние машины одной выгрузкой.
    private static TranslationMachineCheckpoint ExportCheckpoint(TranslationMachine machine)
    {
        // Выгрузка обязана состояться: иначе проверять состояние нечем.
        var exported = machine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        return exported.Value!;
    }

    // SingleExecutor возвращает контрольную точку того исполнителя, которого учили уроки.
    private static ExecutorUnitCheckpoint SingleExecutor(TranslationMachineCheckpoint checkpoint)
        // Направление с английского на русский: именно его единица получает уроки.
        => checkpoint.Executors.Single(executor => executor.Direction == TranslationDirection.EN_RU);

    // MachineWithSeededJournal создаёт машину, ЖУРНАЛ которой восстановлен из контрольной
    // точки с заданными записями. Остальное состояние берётся у настоящей обученной машины,
    // поэтому контрольная точка проходит полную проверку структуры как обычная.
    private static async Task<TranslationMachine> MachineWithSeededJournal(
        params TranslationStructuredTrainingIdentityCheckpoint[] identities)
    {
        // Донорская машина с одним настоящим уроком: её состояние законно и полно.
        var donor = new TranslationMachine();
        var trained = await donor.TrainStructuredBatchAsync(BuildBatch(
            "FIX-IDX-seed-donor",
            "fix-idx-seed-donor-source",
            BuildLesson("lesson-seed", "archive log", "заархивируй журнал", "fix-idx-seed-donor-source")));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Контрольная точка донора с подменённым журналом опознаний: меняется только журнал.
        var seededCheckpoint = ExportCheckpoint(donor) with { StructuredTrainingIdentities = identities };

        // Принимающая машина восстанавливает это состояние обычной загрузкой записи.
        var receiver = new TranslationMachine();
        var imported = receiver.ImportCheckpoint(seededCheckpoint);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return receiver;
    }

    // FirstAppearanceJournal — ОБРАЗЦОВАЯ реализация прежнего правила журнала, написанная
    // независимо от продукта: полный просмотр уже записанных пар и дописывание только
    // первого появления, затем порядок экспорта по опознанию примера и единице.
    //
    // ГРАНИЦЫ этой образцовой реализации, названные прямо, чтобы её не переиспользовали
    // неверно. Она двойник продукта ТОЛЬКО для данных той проверки, которая её вызывает, и
    // общим двойником обучения партии НЕ является по двум причинам.
    // Первая: она не воспроизводит пропуск строки с неизвестным опознанием единицы — продукт
    // такую строку не обучает и в журнал не пишет, а здесь она дала бы запись. Для партии с
    // чужой единицей этот образец применять нельзя; такой случай проверяется отдельной
    // проверкой про неизвестную единицу.
    // Вторая: она начинает с пустого журнала и ничего не знает про уже существующие записи
    // восстановленной машины. Для машины, журнал которой восстановлен из контрольной точки,
    // этот образец тоже применять нельзя.
    private static IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint> FirstAppearanceJournal(
        IEnumerable<TranslationLearningBatch> batches)
    {
        // Накапливаемый журнал: порядок добавления здесь такой же, как у продукта.
        var journal = new List<TranslationStructuredTrainingIdentityCheckpoint>();

        // Партии и строки внутри них перебираются в порядке предъявления.
        foreach (var batch in batches)
        {
            foreach (var example in batch.Examples)
            {
                // Отложенные строки в обучение не идут и журнал не занимают.
                if (example.HeldOut)
                    continue;

                // Прежнее правило: ПОЛНЫЙ просмотр журнала регистрозависимым сравнением.
                var alreadyRecorded = journal.Any(identity =>
                    string.Equals(identity.ExampleId, example.ExampleId, StringComparison.Ordinal) &&
                    string.Equals(identity.UnitId, example.UnitId, StringComparison.Ordinal));

                // Знакомая пара журнал не меняет: первая запись остаётся первой.
                if (alreadyRecorded)
                    continue;

                // Новая пара дописывается с метаданными ЭТОЙ, первой записавшей строки.
                journal.Add(new TranslationStructuredTrainingIdentityCheckpoint(
                    example.ExampleId,
                    example.SourceId,
                    example.UnitId,
                    batch.BatchId));
            }
        }

        // Порядок экспорта продукта: по опознанию примера, затем по единице, ordinal.
        return journal
            .OrderBy(identity => identity.ExampleId, StringComparer.Ordinal)
            .ThenBy(identity => identity.UnitId, StringComparer.Ordinal)
            .ToList();
    }

    // ПРОВЕРКА 6. Строка партии с неизвестным опознанием единицы пропускается и в журнал
    // опознаний не попадает НИКОГДА.
    //
    // Зачем эта проверка нужна. Пропуск неизвестной единицы стоит ВЫШЕ работы с набором и
    // журналом: такую строку никто не обучает. Если перенести добавление в набор и дописывание
    // журнала выше этого пропуска, продукт начнёт записывать опознание урока, которого ни одна
    // единица не получила, — то есть журнал будет утверждать обучение, которого не было. Пять
    // прежних проверок эту мутацию не ловят, потому что все их уроки идут на настоящую единицу.
    [Fact]
    public async Task UnknownUnitRowIsSkippedAndNeverEntersTheIdentityJournal()
    {
        // Свежая машина перевода без обученного состояния.
        var machine = new TranslationMachine();

        // Опознание единицы, которого среди обучателей машины нет.
        const string unknownUnit = "executor-no-such-unit";

        // Неизвестность единицы доказывается самим продуктом, а не предполагается: открытый
        // читатель машины отвечает, есть ли у неё получатель обучения с таким опознанием.
        Assert.False(machine.HasStructuredTrainerFor(unknownUnit));

        // Настоящая единица, наоборот, получателем обучения является.
        Assert.True(machine.HasStructuredTrainerFor(ExecutorUnit));

        // Партия из двух строк: первая на настоящую единицу, вторая на неизвестную.
        var batch = BuildBatchWithSources(
            "FIX-IDX-unknown-unit",
            new[] { "fix-idx-unknown-unit-source" },
            BuildLessonForUnit(
                "lesson-real-unit", ExecutorUnit, "open file", "открой файл", "fix-idx-unknown-unit-source"),
            BuildLessonForUnit(
                "lesson-unknown-unit", unknownUnit, "save draft", "сохрани черновик", "fix-idx-unknown-unit-source"));

        // Нынешний договор такую строку просто пропускает, поэтому партия обязана примениться.
        var applied = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);

        // Продвинутой считается только настоящая единица: неизвестная ничего не получила.
        Assert.Equal(new[] { ExecutorUnit }, applied.Value!.PromotedUnitIds);

        // Экспортируемый журнал опознаний после партии.
        var journal = ExportCheckpoint(machine).StructuredTrainingIdentities;

        // Запись в журнале ровно одна — та, которую действительно обучили. Этими тремя
        // утверждениями журнал зажат полностью, и отдельного утверждения «неизвестной единицы
        // в журнале нет» не требуется: на единственной записи с уже зафиксированными
        // опознанием примера и единицей оно было бы тождественно истинным. Если продукт
        // запишет пропущенную строку, красным станет количество записей ниже; если запишет её
        // ВМЕСТО настоящей — красным станет равенство опознания примера.
        Assert.Single(journal);
        Assert.Equal("lesson-real-unit", journal[0].ExampleId);
        Assert.Equal(ExecutorUnit, journal[0].UnitId);
    }

    // ПРОВЕРКА 7. Каждая запись журнала несёт происхождение СВОЕГО урока, а не первое
    // происхождение партии.
    //
    // Зачем эта проверка нужна. Запись опознания берёт происхождение у урока. Если взять его
    // из списка происхождений партии, то при партии с одним происхождением разницы не видно, и
    // пять прежних проверок остаются зелёными: в их данных происхождение урока и единственное
    // происхождение партии — одна и та же строка. Здесь происхождения разные, поэтому подмена
    // источника поля становится видимой.
    //
    // Третий случай установлен ПО КОДУ, а не угадан: путь обучения партии не читает список
    // происхождений партии ни одной строкой, поэтому урок с происхождением, которого в списке
    // партии нет вовсе, этот договор допускает, и его запись тоже несёт происхождение урока.
    //
    // ВТОРОЙ ИНВАРИАНТ ЭТОЙ ЖЕ ПРОВЕРКИ: страж дублей работает и внутри многоисточниковой
    // партии. Повтор того же ключа «опознание примера плюс опознание единицы» даёт в журнале
    // ОДНУ запись, и эта запись несёт происхождение ПЕРВОГО появления.
    //
    // Зачем это здесь. Сам смысл переноса — страж от дублей учебных опознаний, и держится он
    // ровно одной строкой продукта: добавление пары в рабочий набор само отвечает, была ли
    // пара уже записана. Обход этого стража в отдельной ветке — например «дописывать запись
    // также тогда, когда в списке происхождений партии больше одного значения» — ни одной из
    // шести остальных проверок не ловился: партии проверок 1–5 строятся прежним строителем,
    // который кладёт в список происхождений ровно одно значение, а проверка 6 передаёт массив
    // из одного элемента. Многоисточниковая партия есть только здесь, поэтому повтор ключа
    // поставлен именно в неё: без повтора страж в этой партии вообще не срабатывает и журнал
    // равен двум записям и с обходом, и без него.
    [Fact]
    public async Task EachJournalRecordCarriesItsOwnLessonOriginNotTheFirstBatchOrigin()
    {
        // Свежая машина перевода без обученного состояния.
        var machine = new TranslationMachine();

        // Первое законное происхождение, перечисленное в списке партии.
        const string firstOrigin = "fix-idx-origin-first";

        // Второе законное происхождение, тоже перечисленное в списке партии.
        const string secondOrigin = "fix-idx-origin-second";

        // Происхождение урока, которого в списке происхождений партии нет вовсе.
        const string unlistedOrigin = "fix-idx-origin-not-in-batch-list";

        // Партия с ТРЕМЯ уроками и двумя происхождениями в списке партии: два разных урока
        // разного происхождения и третьим — ПОВТОР ключа первого урока с ДРУГИМ происхождением
        // и другим переводом. Повтор стоит последним, поэтому первым появлением ключа остаётся
        // первый урок со своим происхождением, и подмена «первого появления» на «последнее»
        // стала бы видна на равенстве происхождения ниже.
        var batch = BuildBatchWithSources(
            "FIX-IDX-origins",
            new[] { firstOrigin, secondOrigin },
            BuildLessonForUnit("lesson-origin-a", ExecutorUnit, "open file", "открой файл", firstOrigin),
            BuildLessonForUnit("lesson-origin-b", ExecutorUnit, "save draft", "сохрани черновик", secondOrigin),
            BuildLessonForUnit("lesson-origin-a", ExecutorUnit, "open file", "открой документ", secondOrigin));

        // Партия обязана примениться: все три урока законны, идут настоящей единице, а повтор
        // знакомой пары отказом не является — он обучается заново, но опознание не дописывает.
        var applied = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);

        // Журнал после партии: по записи на каждый РАЗНЫЙ ключ, а не на каждый урок. Трёх
        // уроков здесь два ключа, поэтому записей ровно две. Это и есть страж дублей внутри
        // многоисточниковой партии: обход стража дописал бы третью запись.
        var journal = ExportCheckpoint(machine).StructuredTrainingIdentities;
        Assert.Equal(2, journal.Count);

        // Каждая запись несёт происхождение именно своего урока, а не первое по списку партии.
        // Для повторённого ключа это утверждение работает дважды: выбор единственной записи
        // падает, если повтор дописал вторую, а равенство происхождения падает, если запись
        // взяла происхождение повтора вместо первого появления.
        Assert.Equal(
            firstOrigin,
            journal.Single(identity => string.Equals(
                identity.ExampleId, "lesson-origin-a", StringComparison.Ordinal)).SourceId);
        Assert.Equal(
            secondOrigin,
            journal.Single(identity => string.Equals(
                identity.ExampleId, "lesson-origin-b", StringComparison.Ordinal)).SourceId);

        // Третий случай: урок с происхождением, которого в списке партии нет.
        var unlistedBatch = BuildBatchWithSources(
            "FIX-IDX-origin-unlisted",
            new[] { firstOrigin },
            BuildLessonForUnit(
                "lesson-origin-unlisted", ExecutorUnit, "archive log", "заархивируй журнал", unlistedOrigin));

        // Договор этого пути такую партию допускает, поэтому она тоже применяется.
        var unlistedApplied = await machine.TrainStructuredBatchAsync(unlistedBatch);
        Assert.True(unlistedApplied.IsSuccess, unlistedApplied.ErrorMessage);

        // Журнал после третьей записи.
        var journalAfter = ExportCheckpoint(machine).StructuredTrainingIdentities;
        Assert.Equal(3, journalAfter.Count);

        // Запись несёт происхождение урока, хотя в списке партии стояло другое значение.
        Assert.Equal(
            unlistedOrigin,
            journalAfter.Single(identity => string.Equals(
                identity.ExampleId, "lesson-origin-unlisted", StringComparison.Ordinal)).SourceId);

        // И первые две записи своих происхождений от этой партии не потеряли.
        Assert.Equal(
            firstOrigin,
            journalAfter.Single(identity => string.Equals(
                identity.ExampleId, "lesson-origin-a", StringComparison.Ordinal)).SourceId);
        Assert.Equal(
            secondOrigin,
            journalAfter.Single(identity => string.Equals(
                identity.ExampleId, "lesson-origin-b", StringComparison.Ordinal)).SourceId);
    }

    // BuildLessonForUnit — то же законное построение урока исполнителя, но с НАЗВАННЫМ
    // опознанием единицы. Отдельный строитель, а не новый довод прежнего: прежний строитель
    // используют пять уже принятых проверок, и менять его в этом наряде нельзя.
    private static TranslationLearningExample BuildLessonForUnit(
        string exampleId,
        string unitId,
        string source,
        string target,
        string sourceId)
    {
        // Смысловая подпись берётся у настоящего разборщика, а не придумывается.
        var signature = new TranslationSemanticTokenizer().Tokenize(source, SourceLanguage).Signature;

        // Строка собирается ровно по форме учебной записи области перевода.
        return new TranslationLearningExample(
            ExampleId: exampleId,
            UnitId: unitId,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new ExecutorInput(source, SourceLanguage, Topic), LessonJson),
            ChosenOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput(target, 0.95, null, null), LessonJson),
            RejectedOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput("[WRONG: missing translation]", 0.1, null), LessonJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: sourceId,
            HeldOut: false);
    }

    // BuildBatchWithSources — партия, у которой в списке происхождений МОЖЕТ стоять больше
    // одного значения. Прежний строитель партии кладёт туда ровно один аргумент, поэтому на нём
    // подмену происхождения урока происхождением партии увидеть невозможно.
    private static TranslationLearningBatch BuildBatchWithSources(
        string batchId,
        IReadOnlyList<string> sourceIds,
        params TranslationLearningExample[] examples)
        // Опознание партии, предметная область, сами уроки и названный список происхождений.
        => new(batchId, TranslationMachine.DomainTag, examples, sourceIds);
}
