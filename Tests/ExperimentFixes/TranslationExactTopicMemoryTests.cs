// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «известные значения слова не перезаписывают
// друг друга» (назначение FIX-P51-TOPIC раздела 3 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 105-late-learning-transfer-plan-2026-10-30.md; основание — опыт о теме
// слова P51-PRIMARY-01, подслучай P51.1).
//
// Что здесь проверяется по-человечески. Переводчик держит точную память
// «этот исходный текст переводится так». Адресом ячейки раньше был ТОЛЬКО
// текст источника. Поэтому урок «bank в теме финансов — банк» и урок
// «bank в теме реки — берег» попадали в одну ячейку: второй урок затирал
// первый, и запрос в теме финансов отвечал «берег» — то есть выдавал
// противоположное известное значение как верное. Тема при чтении вообще не
// участвовала, хотя приходила в запросе и даже передавалась в метод чтения.
//
// Исправление, выбранное планом: адресом ячейки становится ПАРА «тема и
// источник». Оба урока про одно слово живут рядом, запрос выбирает свою
// ячейку. Тема не угадывается по тексту — она уже приходит в запросе.
//
// Чего этот перенос НЕ доказывает (границы названы прямо):
//   * он не вводит общий запрет переводить незнакомую тему. Пара «тема и
//     источник» снимает только конфликт точной памяти; на многословном
//     источнике обороты, слова и ближайшие соседи по-прежнему могут выдать
//     ответ, потому что они темой не разделены;
//   * чтение уже известного перевода — это сохранение и повторное
//     использование памяти, а не обобщение на невиданные предложения;
//   * предел постоянного ответа на основной панели — 1 из 4: тот, кто всегда
//     отвечает одной и той же строкой, набирает не больше одного верного
//     исхода, поэтому 4 из 4 не достигаются угадыванием.

namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;
using Xunit;

// Набор проверок точной памяти, адресованной парой «тема и источник».
public sealed class TranslationExactTopicMemoryTests
{
    // Назначение и попытка — ими же названы каталог запускателя и каталог показаний.
    private const string FixId = "FIX-P51-TOPIC";
    private const string AttemptId = "attempt-0001";

    // Направление переноса: с английского на русский, как в опыте о теме слова.
    private const string SourceLanguage = "en";
    private const string TargetLanguage = "ru";

    // Направление исполнителя, чью точную память правит этот перенос.
    private const TranslationDirection Direction = TranslationDirection.EN_RU;

    // Признак отказа переводить: обёртка заготовки. За перевод она НЕ принимается.
    private const string BootstrapMarker = "[EN_RU:BOOTSTRAP]";

    // Единые настройки разбора учебных примеров: перечисления пишутся словами.
    private static readonly JsonSerializerOptions LessonJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки файлов показаний: читаемый отступ и русский текст как есть.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Урок — это исходный текст, тема и правильный перевод.
    private sealed record Lesson(string ExampleId, string Source, string Topic, string Target);

    // Строка панели — это исходный текст, тема и независимый эталон.
    // Эталон читается ТОЛЬКО утверждением и только после получения фактического
    // ответа, поэтому он не может попасть в сам ответ.
    private sealed record PanelRow(string Code, string Source, string Topic, string? Gold);

    // Три назначенных планом урока в назначенном порядке. Незнакомая тема в
    // обучение НЕ входит — иначе отказ на ней перестал бы быть отказом.
    private static readonly Lesson[] MainLessons =
    [
        new("P51-TOPIC-t1", "bank", "finance", "банк"),
        new("P51-TOPIC-t2", "bank", "river",   "берег"),
        new("P51-TOPIC-t3", "file", "general", "файл")
    ];

    // Контрольные уроки смены тем: темы переставлены ТОЛЬКО у двух уроков про
    // «bank», третий урок про «file» не тронут. Это единственный отличающийся
    // фактор второй проверки.
    private static readonly Lesson[] SwappedTopicLessons =
    [
        new("P51-TOPIC-t1", "bank", "river",   "банк"),
        new("P51-TOPIC-t2", "bank", "finance", "берег"),
        new("P51-TOPIC-t3", "file", "general", "файл")
    ];

    // Панель четырёх проверок. У строки незнакомой темы эталон — отказ, поэтому
    // независимого текста перевода у неё нет: конкретный перевод для этой строки
    // запрещён планом.
    private static readonly PanelRow[] Panel =
    [
        new("E1", "bank", "finance", "банк"),
        new("E2", "bank", "river",   "берег"),
        new("E3", "file", "general", "файл"),
        new("E4", "bank", "unknown", null)
    ];

    // Удержания повторяют первые три входа панели с теми же эталонами. Это
    // ПОВТОРНЫЕ чтения уже известных уроков, а не три новые самостоятельные
    // задачи, и в отчёте они так и называются.
    private static readonly PanelRow[] Retentions =
    [
        new("C1", "bank", "finance", "банк"),
        new("C2", "bank", "river",   "берег"),
        new("C3", "file", "general", "файл")
    ];

    // ── Проверка 1: обучение, удержание и свежая загрузка ────────────────────
    //
    // Назначенный планом бюджет этой проверки: 3 урока исполнителя одной
    // партией, 18 чтений обычного потока, одно сохранение, одна свежая
    // загрузка. Внешних моделей, обучения предпочтений, порождения, скрытого
    // прогрева и повторов — ноль. Внутренние действия токенизатора и
    // проверяющего внутри этих чтений — вложенные операции тех же 18 чтений, а
    // не отсутствие вычислений.
    [Fact]
    public async Task TopicExactMemory_LearnsRetainsAndFreshLoads()
    {
        // Используется обычная продуктовая машина, а не конструктор с подставленным потоком.
        var machine = new TranslationMachine();

        // ── Холодные чтения: 4 обращения до любого обучения ──────────────────
        // Холодная машина ничего не знает, поэтому здесь ожидается отказ на всех
        // четырёх строках. Это точка отсчёта, а не требование к продукту.
        var cold = new Dictionary<string, (string Text, string AdapterId)>();
        foreach (var row in Panel)
            cold[row.Code] = await ReadThroughOrdinaryFlowAsync(machine, row);

        // Ни одна холодная строка не может быть верной: памяти ещё нет.
        foreach (var row in Panel)
            Assert.StartsWith(BootstrapMarker, cold[row.Code].Text, StringComparison.Ordinal);

        // ── Обучение: три урока ОДНОЙ партией обычным путём ──────────────────
        var trainResult = await machine.TrainStructuredBatchAsync(
            BuildBatch("P51-TOPIC-main-v1", MainLessons));
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);

        // Партия обязана сообщить ровно три обученные строки — ни одна не потеряна.
        Assert.Equal(MainLessons.Length, trainResult.Value!.RowsTrained);

        // ── Чтения после обучения: 4 строки панели ───────────────────────────
        // Эта группа утверждений стоит ПЕРВОЙ сознательно: именно здесь виден
        // сам дефект. До правки запрос в теме финансов отвечал «берег» вместо
        // «банк», и красный прогон обязан упасть на этом значении, а не на
        // номере версии формы сохранения.
        var afterTraining = new Dictionary<string, (string Text, string AdapterId)>();
        foreach (var row in Panel)
            afterTraining[row.Code] = await ReadThroughOrdinaryFlowAsync(machine, row);

        // Все четыре строки обязаны соблюсти свой эталон: три перевода и отказ.
        AssertPanelFullyCorrect(afterTraining, "после обучения");

        // ── Удержания: 3 повторных чтения известных уроков ───────────────────
        var retentionAfterTraining = new Dictionary<string, (string Text, string AdapterId)>();
        foreach (var row in Retentions)
            retentionAfterTraining[row.Code] = await ReadThroughOrdinaryFlowAsync(machine, row);

        // Ни один известный урок не потерян повторным чтением.
        AssertRetentionsHeld(retentionAfterTraining, "после обучения");

        // ── Три урока обязаны лежать по ТРЁМ разным адресам ──────────────────
        // Это и есть ядро переноса: раньше адресов было два, потому что второй
        // урок про «bank» затирал первый.
        var trainedState = ReadExecutorState(machine);
        Assert.Equal(TranslationExactTopicMemoryVersions.ContextualTopic, trainedState.ExactSourceKeyVersion);
        Assert.NotNull(trainedState.TopicSourceMap);
        Assert.Equal(MainLessons.Length, trainedState.TopicSourceMap!.Count);

        // Старая карта источника у версии 2 обязана быть пустой: авторитетный
        // набор должен быть один, а не два конкурирующих.
        Assert.Empty(trainedState.ExactSourceMap);

        // Каждый урок обязан найтись по своей паре «тема и источник».
        foreach (var lesson in MainLessons)
            Assert.Contains(
                trainedState.TopicSourceMap!,
                entry => entry.TopicDomain == lesson.Topic &&
                         entry.Source == lesson.Source &&
                         entry.Target == lesson.Target);

        // ── Сохранение контрольной точки ────────────────────────────────────
        var checkpointPath = Path.Combine(
            Path.GetTempPath(), $"p51-topic-{Guid.NewGuid():N}.json");
        try
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            // Сохранённый файл обязан быть непустым и разбираемым: пустая
            // сериализация выглядела бы как успешное сохранение ничего.
            Assert.True(new FileInfo(checkpointPath).Length > 0, "файл контрольной точки пуст");
            var savedJson = await File.ReadAllTextAsync(checkpointPath);
            var parsed = JsonSerializer.Deserialize<TranslationMachineCheckpoint>(
                savedJson, TranslationCheckpointJsonOptions.Standard);
            Assert.NotNull(parsed);

            // В файле обязаны лежать именно три тематические записи нужного направления.
            var savedExecutor = parsed!.Executors.Single(entry => entry.Direction == Direction);
            Assert.Equal(MainLessons.Length, savedExecutor.TopicSourceMap?.Count);

            // ── Свежая загрузка: НОВЫЙ объект машины, обучения ноль ──────────
            var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            var restored = load.Value!.Machine;

            // Загруженная машина — это другой объект, а не та же самая ссылка.
            Assert.NotSame(machine, restored);

            // ── Чтения восстановленной машины: 4 строки панели ──────────────
            var afterFreshLoad = new Dictionary<string, (string Text, string AdapterId)>();
            foreach (var row in Panel)
                afterFreshLoad[row.Code] = await ReadThroughOrdinaryFlowAsync(restored, row);

            // Восстановленная машина обязана отвечать так же, как обученная.
            AssertPanelFullyCorrect(afterFreshLoad, "после свежей загрузки");

            // ── Удержания восстановленной машины: 3 повторных чтения ────────
            var retentionAfterFreshLoad = new Dictionary<string, (string Text, string AdapterId)>();
            foreach (var row in Retentions)
                retentionAfterFreshLoad[row.Code] = await ReadThroughOrdinaryFlowAsync(restored, row);

            // Ни один известный урок не потерян новым процессом чтения.
            AssertRetentionsHeld(retentionAfterFreshLoad, "после свежей загрузки");

            // Три адреса обязаны уцелеть в состоянии восстановленной машины.
            var restoredState = ReadExecutorState(restored);
            Assert.Equal(MainLessons.Length, restoredState.TopicSourceMap?.Count);
            Assert.Empty(restoredState.ExactSourceMap);

            // ── Отказ импорта не стирает ПРИНЯТУЮ ОБУЧЕНИЕМ память ──────────
            // Эта проверка стоит здесь, а не отдельной проверкой, ровно по
            // назначению плана: подменённое сохранённое состояние импортируется
            // «в уже обученную основную машину». Отдельная проверка потребовала
            // бы третьей партии и ещё трёх уроков, то есть вышла бы за
            // назначенный учебный бюджет. Здесь обучения и чтений потока ноль.
            var fingerprintBeforeRefusal = CanonicalExecutorFingerprint(machine);

            // Старое сохранение: версия 1 с непустой картой источника без темы.
            var refusal = machine.ImportCheckpoint(BuildLegacyCheckpointWithSourceOnlyMemory());
            Assert.False(refusal.IsSuccess);
            Assert.Equal("CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED", refusal.ErrorCode);

            // Каноническое содержимое состояний обязано остаться тем же байт в
            // байт. Временное поле отметки времени выгрузки из сравнения
            // исключено явно — иначе отпечаток менялся бы сам по себе.
            var fingerprintAfterRefusal = CanonicalExecutorFingerprint(machine);
            Assert.Equal(fingerprintBeforeRefusal, fingerprintAfterRefusal);

            // Прежнюю модель нельзя подменять новым пустым экземпляром, поэтому
            // три обученных адреса обязаны остаться на месте.
            Assert.Equal(MainLessons.Length, ReadExecutorState(machine).TopicSourceMap?.Count);

            // Показания отдельным файлом: доказательство неразрушающего отказа.
            WriteReceipt("import-refusal-does-not-erase-memory.json", new
            {
                назначение = FixId,
                состояние_машины = "обучена тремя уроками обычным путём в этой же проверке",
                обучения_для_этой_проверки = 0,
                чтений_потока_для_этой_проверки = 0,
                старое_сохранение = "версия 1, карта источника без темы: bank -> берег",
                ожидаемый_код_отказа = "CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED",
                наблюдаемый_код_отказа = refusal.ErrorCode,
                отпечаток_до_попытки = Sha256OfText(fingerprintBeforeRefusal),
                отпечаток_после_попытки = Sha256OfText(fingerprintAfterRefusal),
                отпечатки_совпали = string.Equals(
                    fingerprintBeforeRefusal, fingerprintAfterRefusal, StringComparison.Ordinal),
                исключено_из_сравнения = "временное поле отметки времени выгрузки ExportedAtUtc",
                адресов_точной_памяти_осталось = ReadExecutorState(machine).TopicSourceMap?.Count ?? 0,
                смысл =
                    "Отказ импорта оставил принятую обучением модель без изменений. " +
                    "Утраченную тему нельзя восстановить из пары «bank → берег», поэтому " +
                    "приписывание темы general, угадывание темы по переводу и скрытое " +
                    "переобучение запрещены."
            });

            // Показания: фактические ответы и фактические пути на всех трёх
            // стадиях. Эталоны здесь только для чтения человеком — утверждения
            // выше уже выполнены, и файл показаний на них не влияет.
            WriteReceipt("main-panel-receipt.json", new
            {
                назначение = FixId,
                бюджет_назначенный = new { партии = 2, уроки = 6, чтения_потока = 22 },
                бюджет_этой_проверки = new { партии = 1, уроки = 3, чтения_потока = 18, сохранений = 1, свежих_загрузок = 1 },
                обученных_строк_сообщено = trainResult.Value!.RowsTrained,
                разных_адресов_после_трёх_записей = trainedState.TopicSourceMap!.Count,
                версия_адреса_точной_памяти = trainedState.ExactSourceKeyVersion,
                старая_карта_источника_пуста = trainedState.ExactSourceMap.Count == 0,
                предел_постоянного_ответа_на_панели = ConstantAnswerCeiling(),
                до_обучения = DescribeStage(Panel, cold),
                после_обучения = DescribeStage(Panel, afterTraining),
                удержания_после_обучения = DescribeStage(Retentions, retentionAfterTraining),
                после_свежей_загрузки = DescribeStage(Panel, afterFreshLoad),
                удержания_после_свежей_загрузки = DescribeStage(Retentions, retentionAfterFreshLoad),
                тематические_записи_в_файле = savedExecutor.TopicSourceMap!
                    .Select(entry => new { entry.TopicDomain, entry.Source, entry.Target })
                    .ToList(),
                граница_вывода =
                    "Пара «тема и источник» снимает конфликт точной памяти и ничего не " +
                    "доказывает о выборе среди остальных путей отката. Глобальный запрет " +
                    "перевода неизвестной темы НЕ заявляется."
            });
        }
        finally
        {
            // Временный файл контрольной точки удаляется в любом исходе.
            if (File.Exists(checkpointPath)) File.Delete(checkpointPath);
        }
    }

    // ── Проверка 2: контроль смены тем ──────────────────────────────────────
    //
    // Назначенный планом бюджет: ещё 3 урока и 4 чтения. Если переставить темы
    // у двух уроков про «bank», правильность обязана упасть с 4 из 4 до 2 из 4.
    // Это доказывает, что ответ действительно берётся из записанного урока, а
    // не из подогнанного под эталон текста.
    [Fact]
    public async Task TopicLabels_ControlChangesKnownAnswers()
    {
        // Свежая машина: контроль не должен видеть памяти первой проверки.
        var machine = new TranslationMachine();

        // Обучение теми же тремя уроками, но с переставленными темами у «bank».
        var trainResult = await machine.TrainStructuredBatchAsync(
            BuildBatch("P51-TOPIC-control-v1", SwappedTopicLessons));
        Assert.True(trainResult.IsSuccess, trainResult.ErrorMessage);
        Assert.Equal(SwappedTopicLessons.Length, trainResult.Value!.RowsTrained);

        // Четыре чтения той же панели с теми же эталонами.
        var actual = new Dictionary<string, (string Text, string AdapterId)>();
        foreach (var row in Panel)
            actual[row.Code] = await ReadThroughOrdinaryFlowAsync(machine, row);

        // Тема финансов теперь хранит «берег» — ответ обязан стать НЕверным.
        Assert.Equal("берег", actual["E1"].Text);
        Assert.NotEqual("банк", actual["E1"].Text);

        // Тема реки теперь хранит «банк» — ответ обязан стать НЕверным.
        Assert.Equal("банк", actual["E2"].Text);
        Assert.NotEqual("берег", actual["E2"].Text);

        // Урок про «file» темами не переставлялся — он обязан уцелеть.
        Assert.Equal("файл", actual["E3"].Text);

        // Незнакомая тема обязана давать тот же отказ, что и раньше.
        Assert.StartsWith(BootstrapMarker, actual["E4"].Text, StringComparison.Ordinal);

        // Полная правильность падает ровно до 2 из 4: уцелели «file» и отказ.
        var correct = Panel.Count(row => IsRowCorrect(row, actual[row.Code].Text));
        Assert.Equal(2, correct);

        // Показания контроля: переставленные темы и фактические ответы на них.
        WriteReceipt("topic-swap-control-receipt.json", new
        {
            назначение = FixId,
            бюджет_этой_проверки = new { партии = 1, уроки = 3, чтения_потока = 4 },
            переставленные_уроки = SwappedTopicLessons
                .Select(lesson => new { lesson.Source, lesson.Topic, lesson.Target })
                .ToList(),
            фактические_ответы = DescribeStage(Panel, actual),
            верных_исходов = correct,
            верных_исходов_без_перестановки = Panel.Length,
            смысл =
                "Перестановка тем у двух уроков про одно слово роняет полную " +
                "правильность с 4 из 4 до 2 из 4. Значит ответ действительно берётся " +
                "из записанного урока, а не из подогнанного под эталон текста."
        });
    }

    // ── Дешёвые проверки формы сохранения: обучения ноль ────────────────────
    //
    // Эти четыре проверки работают на ПОСТРОЕННЫХ записях состояния и ничего не
    // обучают. Они проверяют, что загрузка отказывает ДО изменения принятой
    // модели и отдаёт назначенный планом код отказа.
    [Theory]
    // Неизвестная версия адреса точной памяти: продукт не умеет её читать.
    [InlineData("unknown_version", "CHECKPOINT_EXACT_SOURCE_SCHEMA_UNSUPPORTED")]
    // Повторный адрес после нормализации: две записи претендуют на одну ячейку.
    [InlineData("duplicate_key", "CHECKPOINT_EXACT_SOURCE_KEY_DUPLICATE")]
    // Несовпадение версии: заявлена старая версия, а тематическая карта непуста.
    [InlineData("version_mismatch", "CHECKPOINT_EXACT_SOURCE_STATE_INVALID")]
    // Одновременно две непустые авторитетные карты: старая и новая.
    [InlineData("both_maps_non_empty", "CHECKPOINT_EXACT_SOURCE_STATE_INVALID")]
    public void ExactTopicMemorySchema_RejectsBrokenStateWithAssignedCode(
        string caseName,
        string expectedErrorCode)
    {
        // Построенная запись состояния: обучение не выполняется ни на один случай.
        var checkpoint = BuildBrokenCheckpoint(caseName);

        // Загрузка в чистую машину обязана отказать назначенным кодом.
        var machine = new TranslationMachine();
        var import = machine.ImportCheckpoint(checkpoint);

        Assert.False(import.IsSuccess, $"случай {caseName} обязан быть отвергнут");
        Assert.Equal(expectedErrorCode, import.ErrorCode);

        // Показания дешёвой проверки: обучения ноль, наблюдаемый код отказа.
        WriteReceipt($"cheap-schema-check-{caseName}.json", new
        {
            назначение = FixId,
            случай = caseName,
            обучения_выполнено = 0,
            чтений_потока_выполнено = 0,
            ожидаемый_код_отказа = expectedErrorCode,
            наблюдаемый_код_отказа = import.ErrorCode,
            наблюдаемое_сообщение = import.ErrorMessage
        });
    }

    // ── Старое сохранение с непустой картой источника ОТКАЗЫВАЕТ намеренно ───
    //
    // Это назначенное планом ужесточение совместимости, а не недоработка.
    // Утраченную тему нельзя восстановить из пары «bank → берег»: приписать
    // тему «general», угадать её по переводу или скрыто переобучиться
    // запрещено, потому что любое из этих действий назвало бы выдуманную тему
    // проверенным учебным фактом.
    // Обучения здесь ноль: случай проверяется на построенной записи состояния.
    // Доказательство, что такой отказ не стирает ПРИНЯТУЮ ОБУЧЕНИЕМ память,
    // выполняется внутри первой проверки на уже обученной машине, чтобы не
    // потратить третью партию уроков сверх назначенного учебного бюджета.
    [Fact]
    public void LegacySourceOnlyMemory_IsRefusedWithContextRequiredCode()
    {
        // Старое сохранение: версия 1 с непустой картой источника без темы.
        var legacy = BuildLegacyCheckpointWithSourceOnlyMemory();

        // Импорт обязан отказать назначенным кодом «нужен контекст темы».
        var machine = new TranslationMachine();
        var import = machine.ImportCheckpoint(legacy);
        Assert.False(import.IsSuccess);
        Assert.Equal("CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED", import.ErrorCode);

        // Показания: наблюдаемый код отказа и нулевой учебный расход.
        WriteReceipt("legacy-source-only-refusal.json", new
        {
            назначение = FixId,
            обучения_выполнено = 0,
            чтений_потока_выполнено = 0,
            старое_сохранение = "версия 1, карта источника без темы: bank -> берег",
            ожидаемый_код_отказа = "CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED",
            наблюдаемый_код_отказа = import.ErrorCode,
            наблюдаемое_сообщение = import.ErrorMessage,
            смысл =
                "Это намеренное ужесточение совместимости, назначенное планом. " +
                "Утраченную тему нельзя восстановить из пары «bank → берег», поэтому " +
                "приписывание темы general, угадывание темы по переводу и скрытое " +
                "переобучение запрещены."
        });
    }

    // ── Старое сохранение с ПУСТОЙ картой источника переносится ─────────────
    //
    // Пустая старая карта не содержит утраченных тем, поэтому её можно принять
    // как пустую тематическую карту. Обучение при этом не выполняется.
    [Fact]
    public void LegacyEmptyMemory_LoadsAsEmptyContextualMemoryWithoutTraining()
    {
        // Старое сохранение необученного исполнителя: обе карты пусты.
        var legacy = BuildLegacyCheckpointWithEmptyMemory();

        // Импорт обязан пройти: переносить нечего, терять нечего.
        var machine = new TranslationMachine();
        var import = machine.ImportCheckpoint(legacy);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        // Тематическая память обязана остаться пустой — обучения не было.
        var state = ReadExecutorState(machine);
        Assert.Empty(state.TopicSourceMap ?? []);
        Assert.Empty(state.ExactSourceMap);
    }

    // ── Общие вспомогательные действия ──────────────────────────────────────

    // ReadThroughOrdinaryFlowAsync выполняет ОДНО чтение обычным продуктовым
    // путём: распорядитель, запуск, поток маршрутизации перевода, обученный
    // переходник. Фактический идентификатор переходника возвращается рядом с
    // текстом, как требует план.
    private static async Task<(string Text, string AdapterId)> ReadThroughOrdinaryFlowAsync(
        TranslationMachine machine,
        PanelRow row)
    {
        // Распорядитель берётся у самой машины — обходных путей нет.
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        // Обычный запрос несёт исходный текст, языки и ТЕМУ.
        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"p51-topic-{row.Code}-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: row.Source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLanguage,
                ["target_language"] = TargetLanguage,
                ["topic_domain"] = row.Topic
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Идентификатор переходника читается из показаний потока, не угадывается.
        var adapterId = result.Value!.FlowResult.Metrics.TryGetValue("adapter_id", out var id)
            ? id
            : "НЕ_СООБЩЁН";

        return (result.Value!.FinalHumanResult, adapterId);
    }

    // IsRowCorrect сравнивает фактический ответ с независимым эталоном строки.
    // Для строки незнакомой темы верным исходом является именно отказ.
    private static bool IsRowCorrect(PanelRow row, string actual)
        => row.Gold is null
            ? actual.StartsWith(BootstrapMarker, StringComparison.Ordinal)
            : string.Equals(actual, row.Gold, StringComparison.Ordinal);

    // AssertPanelFullyCorrect требует все четыре строки панели верными.
    private static void AssertPanelFullyCorrect(
        IReadOnlyDictionary<string, (string Text, string AdapterId)> actual,
        string stage)
    {
        foreach (var row in Panel)
        {
            var observed = actual[row.Code].Text;

            if (row.Gold is null)
            {
                // Незнакомая тема: конкретный перевод для этой строки запрещён.
                Assert.StartsWith(BootstrapMarker, observed, StringComparison.Ordinal);
                continue;
            }

            // Известная тема: требуется именно нужный текст, обёртка заготовки
            // за перевод НЕ принимается.
            Assert.DoesNotContain(BootstrapMarker, observed);
            Assert.Equal(row.Gold, observed);
        }

        // Полная правильность панели на этой стадии обязана быть 4 из 4.
        var correct = Panel.Count(row => IsRowCorrect(row, actual[row.Code].Text));
        Assert.True(
            correct == Panel.Length,
            $"стадия «{stage}»: верных исходов {correct} из {Panel.Length}");
    }

    // AssertRetentionsHeld требует, чтобы повторное чтение известных уроков
    // возвращало те же переводы — ни один урок не потерян.
    private static void AssertRetentionsHeld(
        IReadOnlyDictionary<string, (string Text, string AdapterId)> actual,
        string stage)
    {
        foreach (var row in Retentions)
        {
            var observed = actual[row.Code].Text;
            Assert.DoesNotContain(BootstrapMarker, observed);
            Assert.Equal(row.Gold, observed);
        }

        // Удержано обязано быть 3 из 3.
        var held = Retentions.Count(row => IsRowCorrect(row, actual[row.Code].Text));
        Assert.True(
            held == Retentions.Length,
            $"стадия «{stage}»: удержано {held} из {Retentions.Length}");
    }

    // DescribeStage складывает фактические ответы и фактические пути одной стадии
    // рядом с независимым эталоном каждой строки. Эталон читается только после
    // получения фактического ответа и в сам ответ попасть не может.
    private static IReadOnlyList<object> DescribeStage(
        IReadOnlyList<PanelRow> rows,
        IReadOnlyDictionary<string, (string Text, string AdapterId)> actual)
        => rows.Select(row => (object)new
        {
            код = row.Code,
            источник = row.Source,
            тема = row.Topic,
            эталон = row.Gold ?? "отказ; конкретный перевод для этой строки запрещён",
            фактический_ответ = actual[row.Code].Text,
            фактический_путь_ответа = actual[row.Code].AdapterId,
            верно = IsRowCorrect(row, actual[row.Code].Text)
        }).ToList();

    // ConstantAnswerCeiling считает предел того, кто всегда отвечает одной и той
    // же строкой. Он назначен планом равным 1 из 4, и это число проверяется, а не
    // переписывается: иначе 4 из 4 можно было бы набрать угадыванием.
    private static int ConstantAnswerCeiling()
    {
        // Перебираются все эталонные строки панели плюс обёртка заготовки.
        var candidates = Panel
            .Select(row => row.Gold)
            .Where(gold => gold is not null)
            .Select(gold => gold!)
            .Append(BootstrapMarker + " bank")
            .Distinct(StringComparer.Ordinal);

        // Берётся лучший постоянный ответ: сколько строк он закрывает.
        var best = candidates
            .Select(constant => Panel.Count(row => IsRowCorrect(row, constant)))
            .Max();

        // Назначенный планом предел обязан совпасть с посчитанным.
        Assert.Equal(1, best);
        return best;
    }

    // Sha256OfText даёт короткий отпечаток текста для файла показаний.
    private static string Sha256OfText(string text)
        => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    // ReceiptDirectory — каталог показаний этой попытки внутри каталога запускателей.
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

    // WriteReceipt пишет один файл показаний в кодировке UTF-8 без метки порядка
    // байтов и только с переводом строки Unix.
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

    // BuildBatch строит партию уроков точно по реальной форме опыта о теме слова.
    private static TranslationLearningBatch BuildBatch(string batchId, IReadOnlyList<Lesson> lessons)
    {
        // Подпись строится настоящим токенизатором области, не выдуманной строкой.
        var tokenizer = new TranslationSemanticTokenizer();

        var examples = lessons.Select(lesson => new TranslationLearningExample(
            ExampleId: lesson.ExampleId,
            UnitId: TranslationUnitIds.ExecutorFor(Direction),
            FlowId: TranslationRoutingFlow.FlowId,
            // Вход несёт источник, язык источника и ТЕМУ урока.
            InputJson: JsonSerializer.Serialize(
                new ExecutorInput(lesson.Source, SourceLanguage, lesson.Topic), LessonJson),
            // Выход несёт перевод и уверенность 0,95 без словарей и оборотов.
            ChosenOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput(lesson.Target, 0.95, null, null), LessonJson),
            // Отвергнутый вариант — назначенная планом отметка отсутствия перевода.
            RejectedOutputJson: JsonSerializer.Serialize(
                new ExecutorOutput("[WRONG: missing translation]", 0.1, null, null), LessonJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: tokenizer.Tokenize(lesson.Source, SourceLanguage).Signature,
            SourceId: "p51-simulation-v1",
            // Ни одна строка не отложена: все три обучают.
            HeldOut: false)).ToList();

        return new TranslationLearningBatch(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: ["p51-simulation-v1"]);
    }

    // ReadExecutorState выгружает состояние исполнителя нужного направления.
    private static ExecutorUnitCheckpoint ReadExecutorState(TranslationMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!.Executors.Single(entry => entry.Direction == Direction);
    }

    // CanonicalExecutorFingerprint даёт отпечаток принятого обученного
    // содержимого состояний исполнителей и пар памяти. Временное поле отметки
    // времени выгрузки исключено ЯВНО: иначе отпечаток менялся бы сам по себе.
    private static string CanonicalExecutorFingerprint(TranslationMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        // Проекция: только состояния исполнителей и пары памяти, без времени выгрузки.
        var projection = new
        {
            executors = export.Value!.Executors,
            knn = export.Value!.KnnStore,
            identities = export.Value!.StructuredTrainingIdentities
        };

        return JsonSerializer.Serialize(projection, TranslationCheckpointJsonOptions.Standard);
    }

    // BuildBrokenCheckpoint строит запись состояния с одним названным дефектом.
    private static TranslationMachineCheckpoint BuildBrokenCheckpoint(string caseName)
    {
        // Основой берётся настоящая выгрузка чистой машины, чтобы все остальные
        // поля были заведомо правильными и проверялся ровно один дефект.
        var baseline = new TranslationMachine().ExportCheckpoint();
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        var executors = baseline.Value!.Executors
            .Select(executor => executor.Direction != Direction
                ? executor
                : caseName switch
                {
                    // Версия, которой продукт не знает.
                    "unknown_version" => executor with
                    {
                        ExactSourceKeyVersion = 99,
                        TopicSourceMap = []
                    },
                    // Два раза одна и та же ячейка после нормализации адреса.
                    "duplicate_key" => executor with
                    {
                        ExactSourceKeyVersion = TranslationExactTopicMemoryVersions.ContextualTopic,
                        ExactSourceMap = [],
                        TopicSourceMap =
                        [
                            new ExecutorTopicSourceMapEntryCheckpoint("finance", "bank", "банк"),
                            new ExecutorTopicSourceMapEntryCheckpoint("finance", "bank", "берег")
                        ]
                    },
                    // Заявлена старая версия, а тематическая карта непуста.
                    "version_mismatch" => executor with
                    {
                        ExactSourceKeyVersion = TranslationExactTopicMemoryVersions.LegacySourceOnly,
                        ExactSourceMap = [],
                        TopicSourceMap =
                        [
                            new ExecutorTopicSourceMapEntryCheckpoint("finance", "bank", "банк")
                        ]
                    },
                    // Две непустые авторитетные карты одновременно.
                    "both_maps_non_empty" => executor with
                    {
                        ExactSourceKeyVersion = TranslationExactTopicMemoryVersions.ContextualTopic,
                        ExactSourceMap = [new ExecutorStringMapEntryCheckpoint("bank", "берег")],
                        TopicSourceMap =
                        [
                            new ExecutorTopicSourceMapEntryCheckpoint("finance", "bank", "банк")
                        ]
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "неизвестный случай")
                })
            .ToList();

        return baseline.Value! with { HasStructuredTraining = true, Executors = executors };
    }

    // BuildLegacyCheckpointWithSourceOnlyMemory строит старое сохранение, где
    // точная память адресована только источником и темы в ней нет.
    private static TranslationMachineCheckpoint BuildLegacyCheckpointWithSourceOnlyMemory()
        => BuildLegacyCheckpoint([new ExecutorStringMapEntryCheckpoint("bank", "берег")]);

    // BuildLegacyCheckpointWithEmptyMemory строит старое сохранение необученного
    // исполнителя: старая карта пуста, терять нечего.
    private static TranslationMachineCheckpoint BuildLegacyCheckpointWithEmptyMemory()
        => BuildLegacyCheckpoint([]);

    // BuildLegacyCheckpoint помечает ВСЕ состояния исполнителей старой версией
    // адреса и кладёт переданную старую карту нужному направлению.
    private static TranslationMachineCheckpoint BuildLegacyCheckpoint(
        IReadOnlyList<ExecutorStringMapEntryCheckpoint> legacyExactSourceMap)
    {
        var baseline = new TranslationMachine().ExportCheckpoint();
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);

        var executors = baseline.Value!.Executors
            .Select(executor => executor with
            {
                // Старая версия адреса и отсутствующая тематическая карта —
                // ровно так выглядит файл, записанный до этого переноса.
                ExactSourceKeyVersion = TranslationExactTopicMemoryVersions.LegacySourceOnly,
                TopicSourceMap = null,
                ExactSourceMap = executor.Direction == Direction
                    ? legacyExactSourceMap
                    : []
            })
            .ToList();

        return baseline.Value! with { HasStructuredTraining = true, Executors = executors };
    }
}

// Номера версий адреса точной памяти, названные человеческими именами.
// Продуктовый тип адреса закрыт внутри области перевода, поэтому проверки
// держат те же два числа отдельно и сравнивают их с наблюдаемым полем.
internal static class TranslationExactTopicMemoryVersions
{
    // Старый адрес: только текст источника. Так выглядят файлы до переноса.
    internal const int LegacySourceOnly = 1;

    // Новый адрес: пара «тема и источник».
    internal const int ContextualTopic = 2;
}
