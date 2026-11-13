// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка шага «атомарность обучения партии в области перевода и согласованная
// публикация версии состояния». Основание — находка аудита владельца от
// 02.11.2026 (пункт M04 адресного плана ремонта) и выбранное владельцем решение:
// подготовить согласованную новую версию состояния ЦЕЛИКОМ и публиковать её
// целиком, а не менять состояние по частям на ходу.
//
// Что было сломано, по-человечески. Машина перевода учится партиями: в одной
// партии приходит несколько учебных строк. Раньше обучение правило ЖИВУЮ память
// прямо внутри цикла по строкам. Поэтому поздняя строка, негодная по типовому
// договору получателя обучения, рушила партию уже после того, как ранние строки
// успели записаться: оставалась половина партии, принятая молча. Второй разрыв:
// связанный граф машины (поток вывода, обучатели, единицы и уже выданный наружу
// дирижёр) держал ссылки друг на друга, поэтому замена одной части состояния не
// доходила до остальных — старый дирижёр продолжал отвечать по прежнему графу.
//
// Четыре условия приёмки владельца, по одному классу проверок на каждое:
//   1. поздний отказ сохраняет прежнее состояние, причём ДО внешнего
//      восстановления со стороны проверки: состояние не откатывается само в
//      середине и не остаётся полупринятым;
//   2. после успешной публикации и ранее выданный дирижёр, и новый видят новую
//      версию;
//   3. один запрос не смешивает две версии;
//   4. после отказа следующая законная партия обучается нормально.
//
// Чего эти проверки НЕ доказывают. Они не обещают атомарность партии при отмене
// работы, при нехватке памяти площадки или при отказе записи на диск: закрыт
// ровно случай сбоя получателя обучения внутри партии и согласованность
// публикации версии. Они не измеряют качество перевода. По условию 3 доказано,
// что каждый запрос сообщает РОВНО ОДИН номер версии и что ответ согласован с
// этим номером; публикация, которая приходит ровно в середине одного
// незавершённого запроса, здесь не инсценируется — для этого нужна точка
// остановки внутри потока, и этого наряда она не касается.
namespace LlmFromScratch.Tests;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

// ── Общая оснастка приёмки ───────────────────────────────────────────────────

// BatchAtomicPublicationFixture собирает учебные строки, партии, срезы принятого
// состояния и обычные чтения через продуктового потребителя. Оснастка своя, не
// заимствованная у соседней приёмки: иначе правка чужого файла могла бы незаметно
// изменить смысл этих условий.
internal static class BatchAtomicPublicationFixture
{
    // Источник корпуса учебных строк этой приёмки.
    internal const string SourceId = "m04-batch-atomic-version-publication-v1";

    // Тема, направление и языки приёмки: тема general, направление EN_RU.
    internal const string Topic = "general";
    internal const string SourceLang = "en";
    internal const string TargetLang = "ru";
    internal static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Код отказа партии по сбою строки внутри обучения.
    internal const string TrainingFailedRowCode = "BATCH_ROW_TRAINING_FAILED";

    // Прежний код отказа нечитаемого входа: он обязан остаться на своём случае.
    internal const string UnreadableRowCode = "BATCH_ROW_UNREADABLE";

    // Признак необученного ответа машины перевода: обычный ответ без урока
    // несёт этот след, поэтому по нему видно, что урок НЕ был принят.
    internal const string UntrainedAnswerMarker = "BOOTSTRAP";

    // Имя показания, в котором запрос сообщает номер обслужившей его версии
    // состояния. Здесь оно задано СВОИМ литералом, а не ссылкой на константу
    // продукта, намеренно: иначе эта приёмка не компилировалась бы на прежней
    // логике продукта, и обязательный красный прогон «до правки» превратился бы
    // в ошибку сборки вместо падения самих условий. Совпадение литерала с
    // продуктовым именем показания проверяется отдельной проверкой ниже.
    internal const string StateVersionMetricKey = "state_version";

    // Значение вне перечисления понятий. Такого имени в списке известных понятий
    // нет, поэтому разбор ТИПА у получателя обучения обязан отказать, а разбор
    // ДОКУМЕНТА — пройти. Именно этот разрыв и ловит поздний отказ.
    internal const string OutOfEnumConceptName = "НЕ_ПОНЯТИЕ_ВНЕ_ПЕРЕЧИСЛЕНИЯ";

    // Правильный ответ строки со значением вне перечисления. Текст собран
    // вручную, а не записан из объекта: собрать такой объект нельзя, потому что
    // значения вне перечисления в типе не существует.
    internal const string OutOfEnumChosenJson =
        "{\"Text\":\"открой файл\"," +
        "\"Confidence\":0.95," +
        "\"WordMappings\":[{\"SourceWord\":\"open\"," +
        "\"Concept\":\"" + OutOfEnumConceptName + "\"," +
        "\"TargetWord\":\"открой\"}]," +
        "\"PhraseMappings\":null}";

    // Заведомо нечитаемый текст: незакрытая скобка документа.
    internal const string BrokenChosenJson = "{";

    // Настройки записи текстов учебной строки. Имена перечислений пишутся
    // словами: ровно так их читает получатель обучения.
    internal static readonly JsonSerializerOptions UnitJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки устойчивой подписи среза состояния.
    internal static readonly JsonSerializerOptions StableJson = new()
    {
        WriteIndented = false
    };

    // Настроечный урок: он создаёт непустую память ДО измеряемой партии, поэтому
    // сравнение «состояние до» и «состояние после» не превращается в сравнение
    // двух пустых состояний.
    internal static readonly LessonPair SetupLesson = new("m04-setup", "keep file", "сохрани файл");

    // Законные уроки приёмки.
    internal static readonly LessonPair[] Lessons =
    [
        new("m04-l01", "open file", "открой файл"),
        new("m04-l02", "close file", "закрой файл"),
        new("m04-l03", "delete file", "удали файл"),
        new("m04-l04", "read line", "прочитай строку")
    ];

    // BuildExample собирает одну законную учебную строку для исполнителя EN_RU.
    internal static TranslationLearningExample BuildExample(LessonPair pair)
    {
        // Структурная подпись строки: по ней урок попадает в память поиска.
        var signature = "SVO|HANDLE_OP|DATA_ENTITY";

        // Строка несёт вход, правильный ответ и отвергнутый ответ с пометкой.
        return new TranslationLearningExample(
            ExampleId: pair.ExampleId,
            UnitId: TranslationUnitIds.ExecutorFor(Direction),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new ExecutorInput(pair.Source, SourceLang, Topic), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput(pair.Target, 0.95, null, null), UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput("[WRONG: missing translation]", 0.1, null), UnitJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // BuildOutOfEnumExample делает строку негодной по ТИПУ: вход остаётся
    // законным, портится только поле «понятие» внутри правильного ответа.
    internal static TranslationLearningExample BuildOutOfEnumExample(LessonPair pair)
        => BuildExample(pair) with { ChosenOutputJson = OutOfEnumChosenJson };

    // BuildUnreadableExample делает строку с настоящей синтаксической ошибкой.
    internal static TranslationLearningExample BuildUnreadableExample(LessonPair pair)
        => BuildExample(pair) with { ChosenOutputJson = BrokenChosenJson };

    // TrainBatch вызывает продуктовое обучение партии. Исключение не скрывается,
    // а записывается как наблюдаемый исход: на прежней логике именно оно и
    // составляло измеренный след дефекта.
    internal static BatchOutcome TrainBatch(
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
            // Исход переносится в наблюдаемую запись как есть.
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
            // Исключение наружу — это и есть прежнее поведение: партия рвалась
            // уже после обучения ранних строк. Оно записывается, а не глотается.
            return new BatchOutcome(
                IsSuccess: false,
                ErrorCode: "",
                ErrorMessage: "",
                Value: null,
                ThrewException: true,
                ExceptionText: $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // Snapshot снимает срез принятого обученного содержимого продуктовым
    // выгрузчиком состояния. Никакого восстановления этот срез не делает.
    internal static StateSnapshot Snapshot(TranslationMachine machine)
    {
        // Срез берётся обычной выгрузкой состояния машины.
        var exported = machine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var checkpoint = exported.Value!;

        // Исполнитель нужного направления разбирается отдельно: именно его
        // память правит обучение этой приёмки.
        var executor = checkpoint.Executors.FirstOrDefault(entry => entry.Direction == Direction);

        // Срез несёт устойчивую подпись и счётчики всех затронутых частей.
        return new StateSnapshot(
            StableSha: StableSha(checkpoint),
            Version: checkpoint.Version,
            HistoryRows: checkpoint.TrainingHistory.Count,
            IdentityRows: checkpoint.StructuredTrainingIdentities?.Count ?? 0,
            KnnEntries: checkpoint.KnnStore.Entries.Count,
            TopicSourceEntries: executor?.TopicSourceMap?.Count ?? 0,
            WordMapEntries: executor?.WordMap.Count ?? 0,
            PhraseMapEntries: executor?.PhraseMap.Count ?? 0);
    }

    // StableSha даёт устойчивую подпись принятого содержимого.
    //
    // Исключённые поля названы явно: отметка времени выгрузки и временные поля
    // истории обучения (процессорное время, память, затраченное время). Без этого
    // подпись менялась бы без единой новой записи в состояние. Файловым
    // равенством всей контрольной точки это сравнение не подменяется.
    internal static string StableSha(TranslationMachineCheckpoint checkpoint)
    {
        // Проекция переносит в подпись только содержательные части состояния.
        var projection = new
        {
            checkpoint.SchemaVersion,
            checkpoint.MachineId,
            checkpoint.DomainTag,
            checkpoint.Version,
            checkpoint.AccuracyAfterLastBatch,
            checkpoint.HasStructuredTraining,
            ПамятьПоиска = checkpoint.KnnStore.Entries,
            Токенизатор = checkpoint.SemanticTokenizer,
            Исполнители = checkpoint.Executors,
            Проверяющий = checkpoint.QualityVerifier,
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

        // Подпись берётся от устойчивого текста проекции.
        var text = JsonSerializer.Serialize(projection, StableJson);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    // IssueOrchestrator берёт дирижёра продуктовым способом, а не в обход.
    internal static ILogicalOrchestrator IssueOrchestrator(TranslationMachine machine)
    {
        // Дирижёр запрашивается у самой машины: это обычный продуктовый путь.
        var orchestratorResult = machine.GetOrchestratorAsync().GetAwaiter().GetResult();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        // Наружу отдаётся именно выданный дирижёр, чтобы его можно было
        // переспросить ПОСЛЕ публикации новой версии.
        return orchestratorResult.Value!;
    }

    // Ask задаёт один обычный запрос переданному дирижёру и возвращает ответ
    // вместе с номером версии, которую этот запрос сам сообщил о себе.
    internal static AnswerWithVersion Ask(ILogicalOrchestrator orchestrator, string sourceText)
    {
        // Запрос строится ровно так, как его строит продуктовый вызов.
        var request = new LogicalOrchestratorRequest(
            RunId: $"m04-read-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLang,
                ["target_language"] = TargetLang,
                ["topic_domain"] = Topic
            });

        // Запрос исполняется обычным путём дирижёра.
        var run = orchestrator.RunAsync(request).GetAwaiter().GetResult();
        Assert.True(run.IsSuccess, run.ErrorMessage);

        // Показание версии обязано присутствовать в ответе: без него нельзя
        // сказать, какая версия обслужила запрос.
        var metrics = run.Value!.FlowResult.Metrics;
        Assert.True(
            metrics.ContainsKey(StateVersionMetricKey),
            "ответ запроса обязан сообщать номер обслужившей его версии состояния");

        // Номер версии читается как целое число: текст без числа не годится.
        var versionText = metrics[StateVersionMetricKey];
        Assert.True(
            long.TryParse(versionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version),
            $"номер версии состояния обязан быть целым числом, получено '{versionText}'");

        // Эталон в ответ не подставляется: возвращается фактический текст.
        return new AnswerWithVersion(run.Value!.FinalHumanResult ?? string.Empty, version);
    }

    // AskMachine задаёт запрос через ВНОВЬ выданного дирижёра: короткая запись
    // для случаев, где важен только текущий ответ машины.
    internal static AnswerWithVersion AskMachine(TranslationMachine machine, string sourceText)
        // Дирижёр берётся заново, поэтому ответ приходит от текущей версии.
        => Ask(IssueOrchestrator(machine), sourceText);

    // ReadAnswer читает только ТЕКСТ ответа и номера версии не требует.
    //
    // Зачем отдельное чтение. Условия про сохранность состояния обязаны падать
    // на прежней логике продукта по САМОМУ дефекту состояния, а не по тому, что
    // показания версии там ещё нет. Поэтому там, где важен только ответ,
    // используется это чтение, а показание версии проверяется отдельными
    // условиями, которым оно и нужно по смыслу.
    internal static string ReadAnswer(TranslationMachine machine, string sourceText)
    {
        // Дирижёр берётся обычным продуктовым способом.
        var orchestrator = IssueOrchestrator(machine);

        // Запрос строится ровно так, как его строит продуктовый вызов.
        var request = new LogicalOrchestratorRequest(
            RunId: $"m04-plain-read-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLang,
                ["target_language"] = TargetLang,
                ["topic_domain"] = Topic
            });

        // Запрос исполняется обычным путём дирижёра.
        var run = orchestrator.RunAsync(request).GetAwaiter().GetResult();
        Assert.True(run.IsSuccess, run.ErrorMessage);

        // Эталон в ответ не подставляется: возвращается фактический текст.
        return run.Value!.FinalHumanResult ?? string.Empty;
    }

    // Учебная пара: опознание строки, источник и правильный перевод.
    internal sealed record LessonPair(string ExampleId, string Source, string Target);

    // Наблюдаемый исход одного вызова обучения партии.
    internal sealed record BatchOutcome(
        bool IsSuccess,
        string ErrorCode,
        string ErrorMessage,
        LearningBatchResult? Value,
        bool ThrewException,
        string ExceptionText);

    // Срез принятого обученного содержимого и его счётчики.
    internal sealed record StateSnapshot(
        string StableSha,
        string Version,
        int HistoryRows,
        int IdentityRows,
        int KnnEntries,
        int TopicSourceEntries,
        int WordMapEntries,
        int PhraseMapEntries);

    // Ответ запроса вместе с номером версии, которую он сам о себе сообщил.
    internal sealed record AnswerWithVersion(string Answer, long StateVersion);
}

// ── Условие 1: поздний отказ сохраняет прежнее состояние ─────────────────────

// Эти проверки обязаны падать на ПРЕЖНЕЙ логике продукта: там ранние строки
// партии уже менялись в живой памяти, а поздняя негодная строка выходила
// исключением. Проверка состояния делается СРАЗУ после отказа и БЕЗ внешнего
// восстановления: именно это требовал владелец.
public sealed class TranslationBatchAtomicVersionPublicationTests
{
    [Fact]
    public void LateRowFailure_KeepsPreviousState_WithoutAnyExternalRestore()
    {
        // Машина перевода и настроечный урок: память до партии непустая.
        var machine = new TranslationMachine();
        var setup = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-late-setup",
            [BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Настроечный урок обязан был действительно записаться, иначе сравнение
        // «до» и «после» было бы сравнением двух пустых состояний.
        var before = BatchAtomicPublicationFixture.Snapshot(machine);
        Assert.True(before.KnnEntries > 0, "настроечный урок обязан был записать память поиска");
        Assert.True(before.IdentityRows > 0, "настроечный урок обязан был записать опознание");

        // Ответ на урок первой строки партии до обучения: он необучен.
        var firstLesson = BatchAtomicPublicationFixture.Lessons[0];
        var answerBefore = BatchAtomicPublicationFixture.ReadAnswer(machine, firstLesson.Source);
        Assert.Contains(
            BatchAtomicPublicationFixture.UntrainedAnswerMarker,
            answerBefore,
            StringComparison.Ordinal);

        // Партия: первая строка законна, ошибка во второй.
        var mixed = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-late",
            [
                BatchAtomicPublicationFixture.BuildExample(firstLesson),
                BatchAtomicPublicationFixture.BuildOutOfEnumExample(
                    BatchAtomicPublicationFixture.Lessons[1])
            ]);

        // Срез состояния снимается СРАЗУ после отказа. Никакого восстановления,
        // загрузки контрольной точки или иного внешнего вмешательства до этой
        // строки не происходит: состояние обязано быть прежним само по себе.
        var after = BatchAtomicPublicationFixture.Snapshot(machine);

        // Управляемый отказ, а не исключение наружу.
        Assert.False(mixed.ThrewException, mixed.ExceptionText);
        Assert.False(mixed.IsSuccess, "партия со строкой, негодной по типу, не может быть успешной");
        Assert.Equal(BatchAtomicPublicationFixture.TrainingFailedRowCode, mixed.ErrorCode);
        Assert.NotEqual(BatchAtomicPublicationFixture.UnreadableRowCode, mixed.ErrorCode);

        // Состояние после отказа равно состоянию до партии по всем частям.
        Assert.Equal(before.StableSha, after.StableSha);
        Assert.Equal(before.Version, after.Version);
        Assert.Equal(before.HistoryRows, after.HistoryRows);
        Assert.Equal(before.IdentityRows, after.IdentityRows);
        Assert.Equal(before.KnnEntries, after.KnnEntries);
        Assert.Equal(before.TopicSourceEntries, after.TopicSourceEntries);
        Assert.Equal(before.WordMapEntries, after.WordMapEntries);
        Assert.Equal(before.PhraseMapEntries, after.PhraseMapEntries);

        // Урок ПЕРВОЙ, законной строки партии не стал ответом: половина партии
        // не принята. На прежней логике он бы уже отвечал.
        var answerAfter = BatchAtomicPublicationFixture.ReadAnswer(machine, firstLesson.Source);
        Assert.Contains(
            BatchAtomicPublicationFixture.UntrainedAnswerMarker,
            answerAfter,
            StringComparison.Ordinal);
        Assert.DoesNotContain(firstLesson.Target, answerAfter, StringComparison.Ordinal);

        // Ответ до партии и ответ после отказа совпадают дословно: обычный
        // потребитель не заметил ни одной принятой строки неудавшейся партии.
        Assert.Equal(answerBefore, answerAfter);
    }

    [Fact]
    public void BatchOfFour_RefusesOnTheThirdRow_AndKeepsTheTwoEarlierRowsOutOfState()
    {
        // Машина перевода и настроечный урок.
        var machine = new TranslationMachine();
        var setup = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-order-setup",
            [BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Срез состояния до партии из четырёх строк.
        var before = BatchAtomicPublicationFixture.Snapshot(machine);

        // Партия: законная, законная, негодная по типу, законная.
        var ordered = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-order",
            [
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[0]),
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[1]),
                BatchAtomicPublicationFixture.BuildOutOfEnumExample(
                    BatchAtomicPublicationFixture.Lessons[2]),
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[3])
            ]);

        // Срез состояния сразу после отказа, без внешнего восстановления.
        var after = BatchAtomicPublicationFixture.Snapshot(machine);

        // Управляемый отказ с кодом сбоя строки внутри обучения.
        Assert.False(ordered.ThrewException, ordered.ExceptionText);
        Assert.False(ordered.IsSuccess);
        Assert.Equal(BatchAtomicPublicationFixture.TrainingFailedRowCode, ordered.ErrorCode);

        // Названа ровно третья строка — та, что действительно негодна.
        Assert.Contains(
            BatchAtomicPublicationFixture.Lessons[2].ExampleId,
            ordered.ErrorMessage,
            StringComparison.Ordinal);

        // Опознания законных строк в сообщении отказа не называются: отказ
        // указывает на виновную строку, а не на всю партию списком.
        foreach (var index in new[] { 0, 1, 3 })
            Assert.DoesNotContain(
                BatchAtomicPublicationFixture.Lessons[index].ExampleId,
                ordered.ErrorMessage,
                StringComparison.Ordinal);

        // Состояние после отказа равно состоянию до партии, включая две законные
        // строки, которые обучение успело пройти до негодной.
        Assert.Equal(before.StableSha, after.StableSha);
        Assert.Equal(before.IdentityRows, after.IdentityRows);
        Assert.Equal(before.KnnEntries, after.KnnEntries);
        Assert.Equal(before.TopicSourceEntries, after.TopicSourceEntries);

        // Ни одна из двух ранних законных строк не отвечает переводом.
        foreach (var index in new[] { 0, 1 })
        {
            var lesson = BatchAtomicPublicationFixture.Lessons[index];
            var answer = BatchAtomicPublicationFixture.ReadAnswer(machine, lesson.Source);
            Assert.DoesNotContain(lesson.Target, answer, StringComparison.Ordinal);
        }
    }

    // ── Условие 2: новую версию видят и прежний дирижёр, и новый ─────────────

    // Эта проверка обязана падать на НАИВНОЙ правке, где новая версия состояния
    // подменяется без решателя версии у дирижёра: тогда дирижёр, выданный ДО
    // публикации, навсегда остаётся на прежнем графе и нового урока не видит.
    [Fact]
    public void AfterPublication_PreviouslyIssuedAndNewOrchestrator_BothSeeTheNewVersion()
    {
        // Машина перевода. Дирижёр берётся ДО всякого обучения: именно он потом
        // и должен увидеть новую версию.
        var machine = new TranslationMachine();
        var orchestratorIssuedBefore = BatchAtomicPublicationFixture.IssueOrchestrator(machine);

        // Урок, который партия научит отвечать.
        var lesson = BatchAtomicPublicationFixture.Lessons[0];

        // До обучения прежний дирижёр отвечает необученным ответом.
        var beforeTraining = BatchAtomicPublicationFixture.Ask(orchestratorIssuedBefore, lesson.Source);
        Assert.Contains(
            BatchAtomicPublicationFixture.UntrainedAnswerMarker,
            beforeTraining.Answer,
            StringComparison.Ordinal);

        // Законная партия обучается и публикует новую версию состояния.
        var published = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-visibility",
            [BatchAtomicPublicationFixture.BuildExample(lesson)]);
        Assert.False(published.ThrewException, published.ExceptionText);
        Assert.True(published.IsSuccess, published.ErrorMessage);

        // РАНЕЕ ВЫДАННЫЙ дирижёр обязан увидеть новую версию: тот же объект,
        // новый ответ и новый номер версии.
        var afterTrainingOld = BatchAtomicPublicationFixture.Ask(orchestratorIssuedBefore, lesson.Source);
        Assert.Contains(lesson.Target, afterTrainingOld.Answer, StringComparison.Ordinal);
        Assert.DoesNotContain(
            BatchAtomicPublicationFixture.UntrainedAnswerMarker,
            afterTrainingOld.Answer,
            StringComparison.Ordinal);
        Assert.True(
            afterTrainingOld.StateVersion > beforeTraining.StateVersion,
            $"номер версии обязан вырасти после публикации: было {beforeTraining.StateVersion}, " +
            $"стало {afterTrainingOld.StateVersion}");

        // ВНОВЬ выданный дирижёр обязан видеть ту же самую новую версию.
        var afterTrainingNew = BatchAtomicPublicationFixture.AskMachine(machine, lesson.Source);
        Assert.Contains(lesson.Target, afterTrainingNew.Answer, StringComparison.Ordinal);
        Assert.Equal(afterTrainingOld.StateVersion, afterTrainingNew.StateVersion);
        Assert.Equal(afterTrainingOld.Answer, afterTrainingNew.Answer);
    }

    // ── Условие 3: один запрос не смешивает две версии ───────────────────────

    // Доказывается ровно это: каждый запрос сообщает РОВНО ОДИН номер версии, и
    // его ответ согласован именно с этим номером. Запрос, разорванный публикацией
    // ровно в своей середине, здесь не инсценируется: для этого нужна точка
    // остановки внутри потока, и её этот наряд не добавляет.
    [Fact]
    public void OneRequest_ReportsExactlyOneStateVersion_AndItsAnswerAgreesWithThatVersion()
    {
        // Машина перевода и один дирижёр на всю проверку.
        var machine = new TranslationMachine();
        var orchestrator = BatchAtomicPublicationFixture.IssueOrchestrator(machine);

        // Два урока: первый публикуется раньше, второй позже.
        var firstLesson = BatchAtomicPublicationFixture.Lessons[0];
        var secondLesson = BatchAtomicPublicationFixture.Lessons[1];

        // Версия до всякого обучения.
        var zeroVersion = BatchAtomicPublicationFixture.Ask(orchestrator, firstLesson.Source).StateVersion;

        // Публикация первой версии с первым уроком.
        var firstBatch = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-mix-first",
            [BatchAtomicPublicationFixture.BuildExample(firstLesson)]);
        Assert.True(firstBatch.IsSuccess, firstBatch.ErrorMessage);

        // На этой версии первый урок уже знают, а второй — ещё нет. Оба запроса
        // обязаны сообщить ОДИН и тот же номер версии.
        var firstKnown = BatchAtomicPublicationFixture.Ask(orchestrator, firstLesson.Source);
        var secondUnknown = BatchAtomicPublicationFixture.Ask(orchestrator, secondLesson.Source);
        Assert.Equal(firstKnown.StateVersion, secondUnknown.StateVersion);
        Assert.True(
            firstKnown.StateVersion > zeroVersion,
            "номер версии обязан вырасти после первой публикации");
        Assert.Contains(firstLesson.Target, firstKnown.Answer, StringComparison.Ordinal);
        Assert.DoesNotContain(secondLesson.Target, secondUnknown.Answer, StringComparison.Ordinal);

        // Публикация следующей версии со вторым уроком.
        var secondBatch = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-mix-second",
            [BatchAtomicPublicationFixture.BuildExample(secondLesson)]);
        Assert.True(secondBatch.IsSuccess, secondBatch.ErrorMessage);

        // Теперь оба урока знают, и оба запроса снова сообщают один номер версии,
        // который строго больше прежнего. Ответ каждого запроса согласован именно
        // с тем номером, который этот запрос о себе сообщил: смешения «версия,
        // знающая второй урок» и «ответ, его не знающий» не возникает.
        var bothFirst = BatchAtomicPublicationFixture.Ask(orchestrator, firstLesson.Source);
        var bothSecond = BatchAtomicPublicationFixture.Ask(orchestrator, secondLesson.Source);
        Assert.Equal(bothFirst.StateVersion, bothSecond.StateVersion);
        Assert.True(
            bothFirst.StateVersion > firstKnown.StateVersion,
            "номер версии обязан вырасти после второй публикации");
        Assert.Contains(firstLesson.Target, bothFirst.Answer, StringComparison.Ordinal);
        Assert.Contains(secondLesson.Target, bothSecond.Answer, StringComparison.Ordinal);

        // Прежний ответ прежней версии задним числом не переписывается: номер
        // версии того запроса остался своим.
        Assert.NotEqual(bothSecond.StateVersion, secondUnknown.StateVersion);
    }

    // ── Условие 4: после отказа следующая законная партия обучается ──────────

    [Fact]
    public void AfterRefusedBatch_TheNextLawfulBatch_TrainsNormallyAndKeepsEarlierLessons()
    {
        // Машина перевода и настроечный урок: его сохранность проверяется ниже.
        var machine = new TranslationMachine();
        var setup = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-next-setup",
            [BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Настроечный урок отвечает переводом уже сейчас.
        var setupAnswerBefore = BatchAtomicPublicationFixture.ReadAnswer(
            machine, BatchAtomicPublicationFixture.SetupLesson.Source);
        Assert.Contains(
            BatchAtomicPublicationFixture.SetupLesson.Target,
            setupAnswerBefore,
            StringComparison.Ordinal);

        // Отказавшая партия: первая строка законна, вторая негодна по типу.
        var refused = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-next-refused",
            [
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[0]),
                BatchAtomicPublicationFixture.BuildOutOfEnumExample(
                    BatchAtomicPublicationFixture.Lessons[1])
            ]);
        Assert.False(refused.IsSuccess);
        Assert.Equal(BatchAtomicPublicationFixture.TrainingFailedRowCode, refused.ErrorCode);

        // Срез состояния после отказа: от него и считается следующая партия.
        var afterRefusal = BatchAtomicPublicationFixture.Snapshot(machine);

        // Следующая законная партия из трёх строк.
        var lawful = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-next-lawful",
            [
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[0]),
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[1]),
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[2])
            ]);

        // Партия принята полностью: отказ предыдущей партии ничего не испортил.
        Assert.False(lawful.ThrewException, lawful.ExceptionText);
        Assert.True(lawful.IsSuccess, lawful.ErrorMessage);
        Assert.Equal(3, lawful.Value!.RowsTrained);

        // Состояние действительно изменилось, и опознания выросли ровно на три.
        var afterLawful = BatchAtomicPublicationFixture.Snapshot(machine);
        Assert.NotEqual(afterRefusal.StableSha, afterLawful.StableSha);
        Assert.Equal(afterRefusal.IdentityRows + 3, afterLawful.IdentityRows);
        Assert.Equal(afterRefusal.HistoryRows + 1, afterLawful.HistoryRows);
        Assert.Equal(afterRefusal.TopicSourceEntries + 3, afterLawful.TopicSourceEntries);
        Assert.NotEqual(afterRefusal.Version, afterLawful.Version);

        // Все три новых урока отвечают переводом через обычного потребителя.
        foreach (var index in new[] { 0, 1, 2 })
        {
            var lesson = BatchAtomicPublicationFixture.Lessons[index];
            var answer = BatchAtomicPublicationFixture.ReadAnswer(machine, lesson.Source);
            Assert.Contains(lesson.Target, answer, StringComparison.Ordinal);
        }

        // Прежний настроечный урок не потерян: подготовка новой версии переносит
        // ранее принятое состояние целиком, а не начинает с чистого листа.
        var setupAnswerAfter = BatchAtomicPublicationFixture.ReadAnswer(
            machine, BatchAtomicPublicationFixture.SetupLesson.Source);
        Assert.Equal(setupAnswerBefore, setupAnswerAfter);
        Assert.Contains(
            BatchAtomicPublicationFixture.SetupLesson.Target,
            setupAnswerAfter,
            StringComparison.Ordinal);
    }

    // ── Граница: нечитаемый вход остаётся на своём прежнем коде отказа ───────

    // Эта проверка охраняет границу правки: новый код отказа не должен поглотить
    // прежний случай настоящей синтаксической ошибки входа.
    [Fact]
    public void TrulyUnreadableRow_KeepsItsOwnPreviousRefusalCode()
    {
        // Машина перевода и настроечный урок.
        var machine = new TranslationMachine();
        var setup = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-unreadable-setup",
            [BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Срез состояния до партии с нечитаемой строкой.
        var before = BatchAtomicPublicationFixture.Snapshot(machine);

        // Партия: законная строка и строка с незакрытым документом.
        var broken = BatchAtomicPublicationFixture.TrainBatch(
            machine,
            "m04-unreadable",
            [
                BatchAtomicPublicationFixture.BuildExample(BatchAtomicPublicationFixture.Lessons[0]),
                BatchAtomicPublicationFixture.BuildUnreadableExample(
                    BatchAtomicPublicationFixture.Lessons[1])
            ]);

        // Срез состояния сразу после отказа.
        var after = BatchAtomicPublicationFixture.Snapshot(machine);

        // Прежний код отказа сохранён, состояние не изменилось.
        Assert.False(broken.ThrewException, broken.ExceptionText);
        Assert.False(broken.IsSuccess);
        Assert.Equal(BatchAtomicPublicationFixture.UnreadableRowCode, broken.ErrorCode);
        Assert.Equal(before.StableSha, after.StableSha);
    }
}
