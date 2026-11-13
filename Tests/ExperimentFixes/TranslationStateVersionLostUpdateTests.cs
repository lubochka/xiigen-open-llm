// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Самая опасная находка аудита по последствиям: она теряет уже выполненную работу МОЛЧА.
//
// Что было неправильно. Публикация новой версии состояния была слепой заменой ссылки.
// Согласованность каждой ОТДЕЛЬНОЙ публикации при этом сохранялась — читатель никогда не видел
// полуготового графа, — но потерю обновления это не предотвращало. Два писателя прочитали версию
// семь; каждый подготовил свою версию восемь; каждый опубликовал её согласованно. Результат
// первого писателя исчезал без следа и без отказа. Атомарная замена ссылки сама по себе от
// потери обновления не спасает.
//
// Вторая половина находки отдельная. Точность, измеренная на ОДНОЙ версии, записывалась поверх
// самой свежей. Довод был разумным — не отбрасывать назад публикацию, прошедшую во время
// измерения, — но следствие хуже болезни: число приписывалось версии, которую никто не измерял.
//
// Что стало. Публикация условна по прочитанной основе: она происходит только если версия всё ещё
// та, от которой писатель готовил изменение. Иначе отказ с названием обеих версий, живое
// состояние не меняется, и повторное обучение автоматически НЕ запускается — это отдельное
// решение вызывающего. Запись измерения привязана к той версии, которую измеряли.
//
// Поправка 3 ноября 2026 (раздел ремонта R117-03). Прежде здесь и в самом отказе стояло
// «обученный кандидат сохранён». Это было обещание без записи: закрытая копия обученного
// состояния просто выходила из области видимости. Теперь судьба кандидата называется отдельной
// фразой и только по факту подтверждённой записи; сохранение проигравшего кандидата и его
// восстановление проверяются в TranslationLostCandidateArtifactTests.cs. Эти четыре проверки
// доказывают отказ публикации и неизменность живого состояния, но НЕ доказывают сохранность
// кандидата.
//
// Отрицательные проверки идут НАСТОЯЩИМ механизмом публикации. Подставного шва здесь нет: номер
// версии читается открытым свойством машины, и условие по версии — обычный необязательный довод
// продуктовых методов обучения и оценки, которым может воспользоваться любой вызывающий, а не
// только проверка. Иначе проверка доказывала бы только саму себя.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
// проекта, и обходить запрет оболочкой-посредником я не стал.
public sealed class TranslationStateVersionLostUpdateTests
{
    // Перечисления в учебных примерах пишутся словами, как их пишет продуктовый конструктор.
    private static readonly JsonSerializerOptions JsonOptions =
        new() { Converters = { new JsonStringEnumConverter() } };

    // Учебная строка, по которой потом отвечает обычный читатель.
    private const string FirstWriterSource = "Он проверил файл";
    private const string FirstWriterTarget = "He checked the file";

    // ── Проверка 1: два писателя от одной версии ──────────────────────────────

    [Fact]
    public async Task SecondWriterFromTheSameVersion_IsRefusedAndTheFirstWritersWorkSurvives()
    {
        var machine = new TranslationMachine();

        // Оба писателя читают ОДНУ и ту же версию состояния.
        var sharedBaseVersion = machine.CurrentStateVersion;

        // Первый писатель публикует свою версию: он успевает первым.
        var firstWriter = await machine.TrainStructuredBatchAsync(
            BatchOf("lost-update-first", FirstWriterSource, FirstWriterTarget),
            expectedStateVersion: sharedBaseVersion);
        Assert.True(firstWriter.IsSuccess, firstWriter.ErrorMessage);

        // Версия ушла вперёд ровно на одну публикацию.
        Assert.Equal(sharedBaseVersion + 1, machine.CurrentStateVersion);

        // Второй писатель готовил свою работу от ТОЙ ЖЕ прочитанной версии и публикует её позже.
        var secondWriter = await machine.TrainStructuredBatchAsync(
            BatchOf("lost-update-second", "Он удалил файл", "He deleted the file"),
            expectedStateVersion: sharedBaseVersion);

        // Отказ, а не молчаливое затирание работы первого.
        Assert.False(secondWriter.IsSuccess);
        Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", secondWriter.ErrorCode);
        // В причине названы ОБЕ версии: прочитанная основа и та, что опубликована сейчас.
        Assert.Contains(sharedBaseVersion.ToString(), secondWriter.ErrorMessage);
        Assert.Contains((sharedBaseVersion + 1).ToString(), secondWriter.ErrorMessage);

        // Работа первого писателя ЦЕЛА и читается ОБЫЧНЫМ читателем, а не проверкой состояния.
        var answer = await AskThroughOrdinaryPath(machine, FirstWriterSource);
        Assert.Equal(FirstWriterTarget, answer.FinalHumanResult);
        // Версия не сдвинулась отказом второго писателя: публикации не было.
        Assert.Equal(sharedBaseVersion + 1, machine.CurrentStateVersion);
    }

    // ── Проверка 2: измерение не приписывается другой версии ──────────────────

    [Fact]
    public async Task MeasurementTakenForOneVersion_IsNotRecordedAgainstAnotherVersion()
    {
        var machine = new TranslationMachine();

        // Вызывающий собирается измерить КОНКРЕТНУЮ версию и запоминает её номер.
        var versionToMeasure = machine.CurrentStateVersion;

        // Между решением измерить и самим измерением проходит чужая публикация.
        var publication = await machine.TrainStructuredBatchAsync(
            BatchOf("measure-shift", FirstWriterSource, FirstWriterTarget));
        Assert.True(publication.IsSuccess, publication.ErrorMessage);
        Assert.NotEqual(versionToMeasure, machine.CurrentStateVersion);

        // Измерение той версии, которую собирались измерять, больше невозможно.
        //
        // Почему здесь названо измерение С ВЕРСИЕЙ, а не обычное. Эта проверка была написана
        // против прежней формы, где номер версии принимал сам член договора обучаемой машины
        // третьим необязательным доводом. Ту форму продукт убрал намеренно: метод с лишним
        // доводом НЕ реализует член интерфейса, и из-за этого область перевода перестала
        // собираться. Поэтому договорный член остался ровно двухдоводным, а измерение с
        // названной версией получило собственное имя. Ожидания ниже не тронуты ни одно:
        // проверяется тот же отказ, тот же код причины и те же два номера версии в сообщении.
        var evaluation = await machine.EvaluateAtVersionAsync(
            new LearningEvaluationSet<TranslationDomain>(
                EvaluationId: "measure-shift-eval",
                DomainTag: TranslationMachine.DomainTag,
                EvaluationItems: [new LearningEvaluationItem("i1", FirstWriterSource, FirstWriterTarget)]),
            expectedStateVersion: versionToMeasure);

        // Отказ с названием ОБЕИХ личностей версии, а не число, приписанное не тому состоянию.
        Assert.False(evaluation.IsSuccess);
        Assert.Equal("MEASURED_STATE_VERSION_CHANGED", evaluation.ErrorCode);
        Assert.Contains(versionToMeasure.ToString(), evaluation.ErrorMessage);
        Assert.Contains(machine.CurrentStateVersion.ToString(), evaluation.ErrorMessage);
    }

    // ── Проверка 3: при отказе живое состояние цело и обучения заново не было ──
    // Сохранность самого кандидата этой проверкой не доказывается: она проверяется записью
    // проигравшего кандидата в TranslationLostCandidateArtifactTests.cs.

    [Fact]
    public async Task WhenPublicationIsRefused_LiveStateIsUntouchedAndNoRetrainingHappens()
    {
        var machine = new TranslationMachine();
        var baseVersion = machine.CurrentStateVersion;

        // Одна законная публикация: с неё состояние и будет сравниваться.
        var accepted = await machine.TrainStructuredBatchAsync(
            BatchOf("refusal-base", FirstWriterSource, FirstWriterTarget));
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);

        // Отпечаток живого состояния ПОСЛЕ законной публикации.
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var fingerprintBefore = Fingerprint(before.Value!);

        // Отказанная попытка: условие по версии уже не выполняется.
        var refused = await machine.TrainStructuredBatchAsync(
            BatchOf("refusal-attempt", "Он удалил файл", "He deleted the file"),
            expectedStateVersion: baseVersion);
        Assert.False(refused.IsSuccess);
        Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", refused.ErrorCode);

        // Живое состояние целиком то же: ни одного урока отказанной партии в нём нет.
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(fingerprintBefore, Fingerprint(after.Value!));

        // Повторного обучения не было: история обучения не выросла, номер версии не сдвинулся.
        Assert.Single(after.Value!.TrainingHistory);
        Assert.Equal(baseVersion + 1, machine.CurrentStateVersion);

        // Урок законной публикации по-прежнему читается обычным читателем.
        var answer = await AskThroughOrdinaryPath(machine, FirstWriterSource);
        Assert.Equal(FirstWriterTarget, answer.FinalHumanResult);
    }

    // ── Проверка 4: законная одиночная публикация проходит (зелёная до и после) ─

    [Fact]
    public async Task LawfulSinglePublication_StillPasses()
    {
        var machine = new TranslationMachine();
        var baseVersion = machine.CurrentStateVersion;

        // Один писатель, названное условие по версии выполняется.
        var published = await machine.TrainStructuredBatchAsync(
            BatchOf("lawful-single", FirstWriterSource, FirstWriterTarget),
            expectedStateVersion: baseVersion);

        Assert.True(published.IsSuccess, published.ErrorMessage);
        // Версия выросла ровно на одну публикацию.
        Assert.Equal(baseVersion + 1, machine.CurrentStateVersion);
        // Урок на месте и читается обычной дорогой.
        var answer = await AskThroughOrdinaryPath(machine, FirstWriterSource);
        Assert.Equal(FirstWriterTarget, answer.FinalHumanResult);
        // Прежний вызов без условия по версии тоже проходит: договор не сломан.
        var withoutCondition = await machine.TrainStructuredBatchAsync(
            BatchOf("lawful-no-condition", "Он открыл файл", "He opened the file"));
        Assert.True(withoutCondition.IsSuccess, withoutCondition.ErrorMessage);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // BatchOf собирает партию настоящим продуктовым конструктором из одной готовой пары.
    private static TranslationLearningBatch BatchOf(string batchId, string source, string target)
        => TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            batchId,
            [
                new TranslationTrainReadyPair(
                    Id: batchId + "-p01",
                    Source: "simulation",
                    Direction: "RU_EN",
                    PairFamily: "tense",
                    Prompt: "Переведи",
                    ASide: source,
                    Chosen: target,
                    Rejected: target.Replace("checked", "checks") + " [WRONG: tense]",
                    ValidationOutcome: "train_ready",
                    ValidationErrors: [],
                    VerifiedRuleTags: ["TENSE_PAST"])
            ],
            signatureMode: TranslationSignatureMode.ExactPair);

    // AskThroughOrdinaryPath спрашивает машину её собственным оркестратором: отвечает именно
    // проверяемое состояние, а не отдельный заново обученный объект.
    private static async Task<LogicalOrchestratorResult> AskThroughOrdinaryPath(
        TranslationMachine machine,
        string userRequest)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var answer = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: "lost-update-" + Guid.NewGuid().ToString("N"),
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: userRequest,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "ru",
                ["target_language"] = "en",
                ["topic_domain"] = "general"
            }));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        return answer.Value!;
    }

    // Fingerprint снимает отпечаток всего состояния; время записи файла исключено намеренно —
    // это стенные часы, а не обученное состояние.
    private static string Fingerprint(TranslationMachineCheckpoint checkpoint)
        => JsonSerializer.Serialize(
            checkpoint with { ExportedAtUtc = DateTimeOffset.UnixEpoch },
            JsonOptions);
}
