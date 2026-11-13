// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «партия обучения применяется целиком или никак».
// Основание — сто шестой опыт, кандидат второй: случай «одна законная строка
// плюс одна со значением вне перечисления в одной партии» остался неизмеренным.
//
// Что здесь проверяется по-человечески. Машина перевода учится партиями: в
// одной партии приходит несколько учебных строк, каждая несёт вход и правильный
// ответ текстом JSON (JavaScript Object Notation — текстовый формат записи
// данных). Перед обучением машина делает предпроход и проверяет, что тексты
// вообще ЧИТАЮТСЯ как документ. Но получатель обучения читает из этого
// документа не просто текст, а типовой договор: у поля «понятие» разрешён
// только список известных значений. Строка, которая как документ разбирается,
// а в поле «понятие» несёт значение вне этого списка, предпроход проходит и
// рушится уже ВНУТРИ цикла обучения — а строки до неё к тому моменту уже
// обучены. Остаётся частичная память, хотя собственный комментарий предпрохода
// прямо говорит: урок нельзя применить ни частично, ни целиком.
//
// Решение, выбранное архитектором: не повторять разбор типов в машине и не
// добавлять крючок проверки договору получателя, а снять снимок обученного
// состояния перед первой обучаемой строкой партии и откатиться к нему при любом
// сбое внутри обучения, вернув ОТКАЗ, а не исключение. Отказ несёт опознание
// строки и настоящую причину исходного сбоя под своим кодом, отличимым от кода
// нечитаемого входа.
//
// Два класса проверок в этом файле:
//   этот класс — НОВОЕ поведение. На неизменённом продуктовом коде он обязан
//     падать на значениях (исключение вместо отказа, изменившаяся память);
//   класс управляющих проверок рядом — ПОЛОЖИТЕЛЬНЫЙ и регрессионный контроль.
//     Он обязан быть зелёным и до правки, и после неё, поэтому доказательством
//     нового поведения он не является и на это не претендует.
//
// Чего эти проверки НЕ доказывают. Они не обещают атомарность партии при
// отмене, при нехватке памяти площадки или при отказе записи на диск: закрыт
// ровно случай сбоя получателя обучения внутри цикла партии. Они не проверяют
// качество перевода и ничего не говорят о метриках обучения. Откат
// восстанавливает НАБОР опознаний учебных строк, но порядок их добавления в
// живом списке после откатa становится упорядоченным; на поведение это не
// влияет, потому что список читается только поиском совпадения, и сравнение
// подписи ведётся между двумя выгрузками, которые обе упорядочены.
namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using LlmFromScratch.Tests.ExperimentFixes;
using TranslationDomain;

// ── Общая оснастка обоих классов проверок ─────────────────────────────────────

// TranslationBatchRollbackFixture собирает учебные строки, партии, снимки
// состояния и обычные чтения через продуктовый поток. Вынесено отдельно, чтобы
// класс нового поведения и класс контроля пользовались ОДНОЙ оснасткой и ни
// одна проверка не получила своей, более удобной версии данных.
internal static class TranslationBatchRollbackFixture
{
    // Источник корпуса учебных строк этой приёмки.
    internal const string SourceId = "p106-candidate-2-batch-rollback-v1";

    // Тема, направление и языки приёмки: тема general, направление EN_RU.
    internal const string Topic = "general";
    internal const string SourceLang = "en";
    internal const string TargetLang = "ru";
    internal static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Прежний код отказа нечитаемого входа: он обязан остаться на своём случае.
    internal const string UnreadableRowCode = "BATCH_ROW_UNREADABLE";

    // Новый код отказа сбоя внутри обучения партии: отличается от прежнего.
    internal const string TrainingFailedRowCode = "BATCH_ROW_TRAINING_FAILED";

    // Опознание строки со значением вне перечисления.
    internal const string OutOfEnumExampleId = "p106-out-of-enum";

    // Опознание строки с настоящей синтаксической ошибкой.
    internal const string UnreadableExampleId = "p106-unreadable";

    // Заведомо нечитаемый текст: незакрытая скобка документа.
    internal const string BrokenChosenJson = "{";

    // Значение вне перечисления понятий. Такого имени в списке известных
    // понятий нет, поэтому разбор ТИПА обязан отказать, а разбор ДОКУМЕНТА —
    // пройти. Именно этот разрыв и есть измеряемый дефект.
    internal const string OutOfEnumConceptName = "НЕ_ПОНЯТИЕ_ВНЕ_ПЕРЕЧИСЛЕНИЯ";

    // Правильный ответ строки со значением вне перечисления. Текст собран
    // вручную, а не записан из объекта: собрать такой объект нельзя — значения
    // вне перечисления в типе не существует.
    internal const string OutOfEnumChosenJson =
        "{\"Text\":\"открой файл\"," +
        "\"Confidence\":0.95," +
        "\"WordMappings\":[{\"SourceWord\":\"open\"," +
        "\"Concept\":\"" + OutOfEnumConceptName + "\"," +
        "\"TargetWord\":\"открой\"}]," +
        "\"PhraseMappings\":null}";

    // Настроечный урок: он создаёт непустую память ДО измеряемой партии,
    // поэтому сравнение «память до» и «память после» не превращается
    // в тривиальное сравнение двух пустых состояний.
    internal static readonly LessonPair SetupLesson = new("p106-setup", "keep", "сохрани");

    // Восемь законных уроков: доза партии, названная нарядом для измерения цены.
    internal static readonly LessonPair[] EightLessons =
    [
        new("p106-l01", "open file", "открой файл"),
        new("p106-l02", "close file", "закрой файл"),
        new("p106-l03", "save file", "сохрани файл"),
        new("p106-l04", "delete file", "удали файл"),
        new("p106-l05", "read line", "прочитай строку"),
        new("p106-l06", "write line", "запиши строку"),
        new("p106-l07", "start thread", "запусти поток"),
        new("p106-l08", "stop thread", "останови поток")
    ];

    // Настройки записи учебных текстов: без отступов, с безопасным для
    // кириллицы кодировщиком и строковыми значениями перечислений.
    internal static readonly JsonSerializerOptions UnitJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки устойчивой подписи состояния: без отступов, чтобы подпись
    // не зависела от переносов строк.
    internal static readonly JsonSerializerOptions StableJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Построение законной учебной строки предметной области.
    internal static TranslationLearningExample BuildExample(LessonPair pair)
    {
        // Подпись берётся у настоящего токенизатора, а не придумывается.
        var signature = new TranslationSemanticTokenizer().Tokenize(pair.Source, SourceLang).Signature;

        // Строка собирается ровно по форме учебной записи предметной области.
        return new TranslationLearningExample(
            ExampleId: pair.ExampleId,
            UnitId: TranslationUnitIds.ExecutorFor(Direction),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(pair.Source, SourceLang, Topic), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(pair.Target, 0.95, null, null), UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("[WRONG: missing translation]", 0.1, null), UnitJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // Строка со значением вне перечисления: вход остаётся законным, портится
    // только поле «понятие» внутри правильного ответа.
    internal static TranslationLearningExample BuildOutOfEnumExample(LessonPair pair)
        => BuildExample(pair) with { ExampleId = OutOfEnumExampleId, ChosenOutputJson = OutOfEnumChosenJson };

    // Строка с настоящей синтаксической ошибкой: документ не закрыт.
    internal static TranslationLearningExample BuildUnreadableExample(LessonPair pair)
        => BuildExample(pair) with { ExampleId = UnreadableExampleId, ChosenOutputJson = BrokenChosenJson };

    // Вызов продуктового обучения партии. Исключение не скрывается, а
    // записывается как наблюдаемый исход: на неизменённом коде именно оно и
    // составляет измеренный след дефекта.
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
            // Исходное поведение неизменённого кода: сбой получателя обучения
            // прерывает партию исключением уже после обучения ранних строк.
            return new BatchOutcome(
                IsSuccess: false,
                ErrorCode: "",
                ErrorMessage: "",
                Value: null,
                ThrewException: true,
                ExceptionText: $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // Срез принятого обученного содержимого живой машины перевода.
    internal static StateSnapshot Snapshot(TranslationMachine machine)
    {
        // Снимок берётся продуктовым выгрузчиком состояния.
        var exported = machine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var checkpoint = exported.Value!;

        // Исполнитель нужного направления разбирается отдельно: именно его
        // память правит обучение этой приёмки.
        var executor = checkpoint.Executors.FirstOrDefault(entry => entry.Direction == Direction);

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

    // Устойчивая подпись принятого обученного содержимого.
    //
    // Исключённые поля названы явно: отметка времени выгрузки ExportedAtUtc и
    // временные поля истории обучения CpuMs, PeakRamMb, ElapsedMs. Без этого
    // подпись менялась бы без единой новой записи в память. Файловое равенство
    // всей контрольной точки этим сравнением не подменяется.
    internal static string StableSha(TranslationMachineCheckpoint checkpoint)
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

        // Подпись берётся от устойчивого текста проекции.
        var text = JsonSerializer.Serialize(projection, StableJson);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    // Обычное чтение одной строки через продуктового потребителя: оркестратор.
    internal static string ReadOne(TranslationMachine machine, string sourceText)
    {
        // Оркестратор берётся продуктовым способом, а не собирается в обход.
        var orchestratorResult = machine.GetOrchestratorAsync().GetAwaiter().GetResult();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);

        // Запрос строится ровно так, как его строит продуктовый вызов.
        var request = new LogicalOrchestratorRequest(
            RunId: $"p106-read-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLang,
                ["target_language"] = TargetLang,
                ["topic_domain"] = Topic
            });

        var run = orchestratorResult.Value!.RunAsync(request).GetAwaiter().GetResult();
        Assert.True(run.IsSuccess, run.ErrorMessage);

        // Эталон в ответ не подставляется: возвращается фактический текст.
        return run.Value!.FinalHumanResult ?? string.Empty;
    }

    // Каталог запускателей этой приёмки: туда же, где лежат скрипт и выводы.
    // Запись за пределы каталога проверок этим нарядом не разрешена.
    internal static string EvidenceDirectory()
    {
        // Корень дерева ищется подъёмом от каталога сборки проекта проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Tests", "ExperimentFixes");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        // Корень не найден: показания пишутся рядом со сборкой, и это видно по пути.
        var fallback = Path.Combine(AppContext.BaseDirectory, "p106-batch-rollback-evidence");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    // Корень хранилища для общего приёмника доказательств опытов. Приёмнику нужен
    // корень, чтобы спросить хранилище, отслеживает ли оно файлы внутри назначенного
    // каталога показаний; без корня такой вопрос задать нельзя.
    internal static string RepositoryRootForEvidence()
    {
        // Подъём идёт от каталога сборки проекта проверок, как и у каталога показаний.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            // Корнем считается тот каталог, внутри которого лежит каталог проверок опытов.
            if (Directory.Exists(Path.Combine(directory.FullName, "Tests", "ExperimentFixes")))
                return directory.FullName;

            // Не этот каталог — поднимаемся на уровень выше.
            directory = directory.Parent;
        }

        // Корень не найден: отдаём каталог сборки, и тогда приёмник решает по содержимому.
        return AppContext.BaseDirectory;
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
}

// ── Новое поведение: партия целиком или никак ─────────────────────────────────

// Эти проверки обязаны УПАСТЬ на неизменённом продуктовом коде и пройти после
// правки. Проверка, которая прошла до правки, ничего не доказывает.
public sealed class TranslationBatchTypeMismatchRollbackTests
{
    // Главная проверка покрытия, названная нарядом дословно: партия «одна
    // законная строка плюс одна со значением вне перечисления» отвергается
    // целиком, и память после отказа РАВНА памяти до партии.
    [Fact]
    public void LegalRowPlusOutOfEnumRow_RejectsWholeBatch_AndLeavesMemoryEqualToBefore()
    {
        // Сначала доказывается сам разрыв: текст со значением вне перечисления
        // КАК ДОКУМЕНТ читается. Если бы он не читался, его отвергал бы прежний
        // предпроход, и измеряемого дефекта здесь не было бы вовсе.
        using (var document = JsonDocument.Parse(TranslationBatchRollbackFixture.OutOfEnumChosenJson))
        {
            // Поле «понятие» на месте и несёт значение вне списка известных.
            var concept = document.RootElement
                .GetProperty("WordMappings")[0]
                .GetProperty("Concept")
                .GetString();
            Assert.Equal(TranslationBatchRollbackFixture.OutOfEnumConceptName, concept);
        }

        // Машина перевода создаётся продуктовым способом, с полным составом единиц.
        var machine = new TranslationMachine();

        // Настроечная партия: один законный урок создаёт непустую память.
        var setup = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-setup",
            [TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Подпись памяти ДО измеряемой партии.
        var before = TranslationBatchRollbackFixture.Snapshot(machine);

        // Память до партии действительно непуста: иначе сравнение ничего не стоит.
        Assert.True(before.KnnEntries > 0, "настроечный урок обязан был записать память");
        Assert.True(before.IdentityRows > 0, "настроечный урок обязан был записать опознание");

        // Измеряемая партия: первая строка законна, вторая несёт значение вне
        // перечисления. Порядок именно такой: на неизменённом коде первая строка
        // к моменту сбоя уже обучена.
        var mixed = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-mixed",
            [
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[0]),
                TranslationBatchRollbackFixture.BuildOutOfEnumExample(TranslationBatchRollbackFixture.EightLessons[1])
            ]);

        // Подпись памяти ПОСЛЕ отказа, до любой починки.
        var after = TranslationBatchRollbackFixture.Snapshot(machine);

        // Отказ обязан быть управляемым: конверт результата, а не исключение.
        Assert.False(
            mixed.ThrewException,
            $"сбой внутри обучения обязан давать отказ, а не исключение: {mixed.ExceptionText}");
        Assert.False(mixed.IsSuccess, "партия со строкой вне перечисления не может быть успешной");

        // Код отказа — новый и отличается от кода нечитаемого входа.
        Assert.Equal(TranslationBatchRollbackFixture.TrainingFailedRowCode, mixed.ErrorCode);
        Assert.NotEqual(TranslationBatchRollbackFixture.UnreadableRowCode, mixed.ErrorCode);

        // Память после отказа равна памяти до партии: доказано подписью
        // состояния, а не словами. Счётчики перечислены отдельно, чтобы
        // сообщение о падении называло, что именно уехало.
        Assert.Equal(before.StableSha, after.StableSha);
        Assert.Equal(before.Version, after.Version);
        Assert.Equal(before.HistoryRows, after.HistoryRows);
        Assert.Equal(before.IdentityRows, after.IdentityRows);
        Assert.Equal(before.KnnEntries, after.KnnEntries);
        Assert.Equal(before.TopicSourceEntries, after.TopicSourceEntries);
        Assert.Equal(before.WordMapEntries, after.WordMapEntries);
        Assert.Equal(before.PhraseMapEntries, after.PhraseMapEntries);

        // Продуктовое наблюдение того же факта: урок первой строки отвергнутой
        // партии в ответах не появился. Этот урок до партии не преподавался,
        // поэтому обычное чтение обязано остаться в исходной обёртке заготовки.
        var answer = TranslationBatchRollbackFixture.ReadOne(
            machine,
            TranslationBatchRollbackFixture.EightLessons[0].Source);
        Assert.NotEqual(
            TranslationBatchRollbackFixture.EightLessons[0].Target,
            answer.Trim());
        Assert.Contains("BOOTSTRAP", answer, StringComparison.Ordinal);
    }

    // Отказ по сбою несёт опознание строки и настоящую причину исходного сбоя,
    // а не общую фразу и не переименование в «нечитаемый вход».
    [Fact]
    public void TrainingFailureRefusal_CarriesRowIdentityAndRealReason()
    {
        // Машина перевода и настроечный урок.
        var machine = new TranslationMachine();
        var setup = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-reason-setup",
            [TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Партия с одной законной и одной негодной по типу строкой.
        var mixed = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-reason",
            [
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[0]),
                TranslationBatchRollbackFixture.BuildOutOfEnumExample(TranslationBatchRollbackFixture.EightLessons[1])
            ]);

        // Управляемый отказ, а не исключение.
        Assert.False(mixed.ThrewException, mixed.ExceptionText);
        Assert.False(mixed.IsSuccess);

        // Опознание именно плохой строки присутствует в сообщении.
        Assert.Contains(
            TranslationBatchRollbackFixture.OutOfEnumExampleId,
            mixed.ErrorMessage,
            StringComparison.Ordinal);

        // Настоящая причина передана, а не проглочена: в сообщении видно и род
        // исходного сбоя, и само значение, которого в перечислении нет.
        Assert.Contains("JsonException", mixed.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains(
            TranslationBatchRollbackFixture.OutOfEnumConceptName,
            mixed.ErrorMessage,
            StringComparison.Ordinal);

        // Сообщение не притворяется нечитаемым входом: это другая болезнь.
        Assert.DoesNotContain("unreadable", mixed.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // Порядок строк соблюдён: отказ наступает по той строке, которая
    // действительно плохая, а не по первой и не по последней строке партии.
    [Fact]
    public void BatchOfFour_RefusesOnTheThirdRow_WhichIsTheActuallyBadOne()
    {
        // Машина перевода и настроечный урок.
        var machine = new TranslationMachine();
        var setup = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-order-setup",
            [TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Подпись памяти до партии из четырёх строк.
        var before = TranslationBatchRollbackFixture.Snapshot(machine);

        // Партия: законная, законная, негодная по типу, законная.
        var ordered = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-order",
            [
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[0]),
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[1]),
                TranslationBatchRollbackFixture.BuildOutOfEnumExample(TranslationBatchRollbackFixture.EightLessons[2]),
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[3])
            ]);

        // Подпись памяти сразу после отказа.
        var after = TranslationBatchRollbackFixture.Snapshot(machine);

        // Управляемый отказ с новым кодом.
        Assert.False(ordered.ThrewException, ordered.ExceptionText);
        Assert.False(ordered.IsSuccess);
        Assert.Equal(TranslationBatchRollbackFixture.TrainingFailedRowCode, ordered.ErrorCode);

        // Названа ровно третья строка — та, что действительно плохая.
        Assert.Contains(
            TranslationBatchRollbackFixture.OutOfEnumExampleId,
            ordered.ErrorMessage,
            StringComparison.Ordinal);

        // Опознания законных строк в сообщении отказа не называются: отказ
        // обязан указывать на виновную строку, а не на всю партию списком.
        foreach (var index in new[] { 0, 1, 3 })
            Assert.DoesNotContain(
                TranslationBatchRollbackFixture.EightLessons[index].ExampleId,
                ordered.ErrorMessage,
                StringComparison.Ordinal);

        // Память после отказа равна памяти до партии, включая две законные
        // строки, которые цикл обучения успел пройти до плохой.
        Assert.Equal(before.StableSha, after.StableSha);
        Assert.Equal(before.IdentityRows, after.IdentityRows);
        Assert.Equal(before.KnnEntries, after.KnnEntries);
        Assert.Equal(before.TopicSourceEntries, after.TopicSourceEntries);
    }
}

// ── Положительный и регрессионный контроль ────────────────────────────────────

// Эти проверки обязаны быть зелёными и ДО правки, и ПОСЛЕ неё. Они доказывают
// отсутствие регрессии и измеряют цену снимка, но доказательством нового
// поведения не являются и на это не претендуют.
public sealed class TranslationBatchTypeMismatchRollbackControlTests
{
    // Законная партия восьми строк по-прежнему обучает: регрессии нет.
    [Fact]
    public void LegalBatchOfEightRows_StillTeachesEveryRow()
    {
        // Машина перевода создаётся продуктовым способом.
        var machine = new TranslationMachine();

        // Подпись памяти до партии: у новой машины память пуста.
        var before = TranslationBatchRollbackFixture.Snapshot(machine);
        Assert.Equal(0, before.KnnEntries);

        // Законная партия ровно из восьми строк.
        var batch = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-legal-eight",
            TranslationBatchRollbackFixture.EightLessons
                .Select(TranslationBatchRollbackFixture.BuildExample)
                .ToArray());

        // Партия обязана пройти и обучить все восемь строк.
        Assert.False(batch.ThrewException, batch.ExceptionText);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(8, batch.Value!.RowsTrained);

        // Память действительно изменилась: партия записала уроки, а не только
        // вернула успешный конверт результата.
        var after = TranslationBatchRollbackFixture.Snapshot(machine);
        Assert.NotEqual(before.StableSha, after.StableSha);
        Assert.True(after.KnnEntries >= 8, $"ожидалось не меньше восьми пар памяти, получено {after.KnnEntries}");
        Assert.Equal(8, after.IdentityRows);
        Assert.Equal(8, after.TopicSourceEntries);
    }

    // Настоящая синтаксическая ошибка по-прежнему отвергается ПРЕЖНИМ кодом
    // отказа: существующий предпроход читаемости сохранён как есть.
    [Fact]
    public void TrulyUnreadableRow_IsStillRefusedByThePreviousRefusalCode()
    {
        // Машина перевода и настроечный урок.
        var machine = new TranslationMachine();
        var setup = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-unreadable-setup",
            [TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Подпись памяти до негодной партии.
        var before = TranslationBatchRollbackFixture.Snapshot(machine);

        // Текст со незакрытой скобкой как документ действительно не читается.
        Assert.ThrowsAny<JsonException>(
            () => JsonDocument.Parse(TranslationBatchRollbackFixture.BrokenChosenJson));

        // Партия: законная строка плюс строка с настоящей синтаксической ошибкой.
        var broken = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-unreadable",
            [
                TranslationBatchRollbackFixture.BuildExample(TranslationBatchRollbackFixture.EightLessons[0]),
                TranslationBatchRollbackFixture.BuildUnreadableExample(TranslationBatchRollbackFixture.EightLessons[1])
            ]);

        // Отказ прежний: тот же код и опознание нечитаемой строки.
        Assert.False(broken.ThrewException, broken.ExceptionText);
        Assert.False(broken.IsSuccess);
        Assert.Equal(TranslationBatchRollbackFixture.UnreadableRowCode, broken.ErrorCode);
        Assert.Contains(
            TranslationBatchRollbackFixture.UnreadableExampleId,
            broken.ErrorMessage,
            StringComparison.Ordinal);

        // Память не изменилась: предпроход отказывает до первой записи.
        var after = TranslationBatchRollbackFixture.Snapshot(machine);
        Assert.Equal(before.StableSha, after.StableSha);
    }

    // Измерение цены снимка и откатa при партии восьми строк.
    //
    // Это измерение, а не утверждение о дешевизне. Честные ограничения названы
    // в файле показаний: процессорное время относится ко всему процессу,
    // время по стенным часам зависит от загрузки площадки, выделения потоком
    // относятся к измеряемому вызову.
    [Fact]
    public void SnapshotAndRollbackCost_IsMeasuredOnBatchOfEightRows()
    {
        // Машина перевода, обученная ровно восемью законными строками: именно
        // такое состояние снимается и откатывается в продуктовом пути.
        var machine = new TranslationMachine();
        var batch = TranslationBatchRollbackFixture.TrainBatch(
            machine,
            "p106-cost-eight",
            TranslationBatchRollbackFixture.EightLessons
                .Select(TranslationBatchRollbackFixture.BuildExample)
                .ToArray());
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Equal(8, batch.Value!.RowsTrained);

        // Прогрев: первый вызов платит за разовую подготовку среды выполнения,
        // и без прогрева он выдавал бы цену подготовки за цену снимка.
        var warmup = machine.ExportCheckpoint();
        Assert.True(warmup.IsSuccess, warmup.ErrorMessage);
        Assert.True(machine.ImportCheckpoint(warmup.Value!).IsSuccess);

        // Число повторов измерения: по одному вызову цену назвать нельзя.
        const int repeats = 25;
        var snapshotMs = new List<double>(repeats);
        var snapshotBytes = new List<long>(repeats);
        var rollbackMs = new List<double>(repeats);
        var rollbackBytes = new List<long>(repeats);

        for (var attempt = 0; attempt < repeats; attempt++)
        {
            // Цена СНЯТИЯ снимка: ровно выгрузка обученного состояния в памяти.
            var snapshotAllocatedStart = GC.GetAllocatedBytesForCurrentThread();
            var snapshotWatch = Stopwatch.StartNew();
            var snapshot = machine.ExportCheckpoint();
            snapshotWatch.Stop();
            snapshotBytes.Add(GC.GetAllocatedBytesForCurrentThread() - snapshotAllocatedStart);
            snapshotMs.Add(snapshotWatch.Elapsed.TotalMilliseconds);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);

            // Цена ОТКАТA: ровно обратная загрузка того же снимка в памяти.
            var rollbackAllocatedStart = GC.GetAllocatedBytesForCurrentThread();
            var rollbackWatch = Stopwatch.StartNew();
            var restored = machine.ImportCheckpoint(snapshot.Value!);
            rollbackWatch.Stop();
            rollbackBytes.Add(GC.GetAllocatedBytesForCurrentThread() - rollbackAllocatedStart);
            rollbackMs.Add(rollbackWatch.Elapsed.TotalMilliseconds);
            Assert.True(restored.IsSuccess, restored.ErrorMessage);
        }

        // Цена всей законной партии восьми строк на свежей машине: она даёт
        // масштаб, с которым цену снимка и откатa вообще можно сравнивать.
        var scaleMachine = new TranslationMachine();
        var scaleExamples = TranslationBatchRollbackFixture.EightLessons
            .Select(TranslationBatchRollbackFixture.BuildExample)
            .ToArray();
        var scaleAllocatedStart = GC.GetAllocatedBytesForCurrentThread();
        var scaleWatch = Stopwatch.StartNew();
        var scaleBatch = TranslationBatchRollbackFixture.TrainBatch(scaleMachine, "p106-cost-scale", scaleExamples);
        scaleWatch.Stop();
        var scaleBytes = GC.GetAllocatedBytesForCurrentThread() - scaleAllocatedStart;
        Assert.True(scaleBatch.IsSuccess, scaleBatch.ErrorMessage);

        // Показания пишутся на диск рядом со скриптом прогона: словам о цене
        // веры нет, а файл показаний можно прочитать и перепроверить.
        var payload = new
        {
            Назначение = "p106-candidate-2-batch-rollback",
            ЧтоИзмерено = "цена снятия снимка обученного состояния и цена откатa при партии восьми строк",
            ВремяЗаписиUtc = DateTimeOffset.UtcNow,
            ЧестныеОграниченияИзмерения = new[]
            {
                "время по стенным часам зависит от загрузки площадки и не является нижней границей",
                "выделенные байты считаются по текущему потоку и относятся к измеряемому вызову",
                "процессорное время по стадиям не делится и здесь не приводится",
                "измерено на машине перевода, обученной ровно восемью законными строками",
                "цена откатa измерена на успешной обратной загрузке, а не на пути отказа"
            },
            ПовторовИзмерения = repeats,
            СостояниеПодСнимком = new
            {
                СтрокОбучено = batch.Value!.RowsTrained,
                ПарПамяти = TranslationBatchRollbackFixture.Snapshot(machine).KnnEntries,
                Опознаний = TranslationBatchRollbackFixture.Snapshot(machine).IdentityRows
            },
            СнятиеСнимка = new
            {
                СреднееВремяМс = Math.Round(snapshotMs.Average(), 4),
                МинимальноеВремяМс = Math.Round(snapshotMs.Min(), 4),
                МаксимальноеВремяМс = Math.Round(snapshotMs.Max(), 4),
                СреднееВыделеноБайт = (long)snapshotBytes.Average(),
                МинимальноВыделеноБайт = snapshotBytes.Min(),
                МаксимальноВыделеноБайт = snapshotBytes.Max()
            },
            Откат = new
            {
                СреднееВремяМс = Math.Round(rollbackMs.Average(), 4),
                МинимальноеВремяМс = Math.Round(rollbackMs.Min(), 4),
                МаксимальноеВремяМс = Math.Round(rollbackMs.Max(), 4),
                СреднееВыделеноБайт = (long)rollbackBytes.Average(),
                МинимальноВыделеноБайт = rollbackBytes.Min(),
                МаксимальноВыделеноБайт = rollbackBytes.Max()
            },
            МасштабДляСравнения = new
            {
                ЧтоЭто = "одна законная партия восьми строк целиком, на свежей машине перевода",
                ВремяМс = Math.Round(scaleWatch.Elapsed.TotalMilliseconds, 4),
                ВыделеноБайт = scaleBytes
            }
        };

        // Куда писать показания, решает общий приёмник доказательств опытов, а не это
        // испытание. Назначенный каталог тот же, что и раньше, но если хранилище уже
        // отслеживает внутри него файлы, каталог признан опубликованным наблюдением:
        // тогда прогон заметно уходит в игнорируемый каталог результатов испытаний, а
        // опубликованная расписка остаётся нетронутой ни на байт. Ровно эта расписка и
        // была молча перезаписана прогоном девятого ноября: измеренное среднее время
        // снятия снимка 0.0472 подменилось на 0.054, минимальное 0.0152 на 0.0393,
        // максимальное 0.4976 на 0.153.
        var evidenceSink = ExperimentEvidenceSink.ForDeclaredDirectory(
            TranslationBatchRollbackFixture.RepositoryRootForEvidence(),
            TranslationBatchRollbackFixture.EvidenceDirectory());

        // Приёмник отдаёт разрешённый путь: само испытание пишет своей сериализацией.
        var path = evidenceSink.ResolvePath("run-batch-type-mismatch-rollback-gate-cost-receipt.json");

        // Запись показаний: кодировка UTF-8 без метки порядка байт, переводы
        // строк в стиле LF, чтобы проверка пробелов перед коммитом молчала.
        var json = JsonSerializer
            .Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        if (!json.EndsWith('\n')) json += "\n";
        File.WriteAllText(path, json, new UTF8Encoding(false));

        // Показания обязаны существовать на диске: иначе измерения не было.
        Assert.True(File.Exists(path), $"файл показаний цены не записан: {path}");

        // Измеренные значения обязаны быть настоящими числами, а не пустотой.
        Assert.All(snapshotMs, value => Assert.True(double.IsFinite(value)));
        Assert.All(rollbackMs, value => Assert.True(double.IsFinite(value)));
        Assert.All(snapshotBytes, value => Assert.True(value > 0, "снимок не может стоить ноль байт"));
    }
}
