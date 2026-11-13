// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Это раздел про сохранение проигравшего обученного кандидата: план сто тридцать девять,
// раздел R117-03; аудит сто тридцать восемь, находка F01.
//
// Что было неправильно, по коду. Отказ публикации по несовпадению основы в
// 27.TranslationDomain/TranslationMachine.cs говорил дословно «обученный кандидат
// сохранён» (прежняя строка 147), а соседний комментарий на прежних строках 375-377
// честно признавал обратное: «обученный кандидат остаётся в закрытой копии и просто
// выходит из области видимости». Закрытая копия обученного состояния (прежние строки
// 281-294) передавалась только в условную публикацию на прежних строках 363-373 и при
// отказе исчезала вместе с кадром вызова. Отказ на прежних строках 378-380 возвращал код
// «версия состояния изменилась» без указателя, дескриптора и пути. То есть сообщение
// обещало контрольную точку, которой никто не создавал, и уже оплаченное обучение
// терялось молча.
//
// Почему обычный путь сохранения здесь непригоден. Обычный SaveCheckpointAsync зовёт
// ExportCheckpoint, а тот читает ТЕКУЩУЮ опубликованную версию. После проигрыша
// сравнения-с-заменой текущей версией является ПОБЕДИТЕЛЬ, поэтому обычный путь записал
// бы победителя под именем кандидата. Запись кандидата обязана строиться из его
// собственного держателя версии и его собственной закрытой копии исполнения.
//
// Что стало. Кандидат назван отдельной величиной до попытки публикации, его фактическое
// состояние собирается тем же чистым сбором, что и обычная выгрузка, и сохраняется
// записью с проверяемым указателем. Слово «сохранён» произносится только после
// подтверждения записи перечитыванием байтов с диска. Отказ хранения — отдельный
// названный отказ CANDIDATE_PERSISTENCE_FAILED, и в этом случае слово «сохранён» не
// произносится вовсе. Проигравший кандидат в победившее состояние не вносится.
//
// О красном прогоне на прежней логике. Прежние правила ниже записаны отдельными
// функциями с приставкой Prior — это ВОСПРОИЗВЕДЕНИЕ прежнего дефекта, а не копия нового
// правила. Каждая красная проверка требует двух вещей разом: прежнее правило на том же
// входе даёт НЕВЕРНЫЙ ответ, а действующий продуктовый путь — верный.
//
// Красные по сути, то есть на прежнем коде они обязаны были падать:
//   1. «проигравший кандидат читается после отказа» — прежний код не создавал записи
//      вовсе, читать было нечего;
//   2. «без хранилища отказ прямо говорит, что кандидат НЕ сохранён» — прежняя фраза
//      говорила «сохранён» на любом пути;
//   3. «ошибка хранения даёт свой код отказа и не говорит «сохранён»» — прежний код
//      такого отказа не имел и всё равно обещал сохранность;
//   4. «запись несёт фактическое состояние, основу, победителя, расход и причину» —
//      прежний код не нёс ничего из этого.
// Положительные контроли «законная одиночная публикация проходит» и «при успехе запись
// кандидата не создаётся» зелёные и до, и после: прежняя логика их тоже пропускала, и
// правка их не ломает.
//
// Управляемая точка синхронизации — не подставной писатель. Поздний конфликт возникает
// только когда чужая публикация проходит МЕЖДУ чтением основы и сравнением-с-заменой.
// Барьер наблюдения даёт эту точку определённо, а не ожиданием по времени; сборка
// кандидата, условная публикация и запись кандидата идут настоящим продуктовым путём.
//
// Чего эти проверки НЕ доказывают, и я этого не заявляю. Первое: ранний отказ по
// названному вызывающим номеру версии — отдельный путь, он здесь не проверяется и
// доказательством позднего конфликта не считается. Второе: путь плоской партии обучения
// памяти единиц не меняет, поэтому записи кандидата там нет по существу, и это проверено
// отдельно как честный отказ, а не как сохранение. Третье: обычный продуктовый
// потребитель собственной конкурентной приёмки из того же раздела плана здесь не создан.
//
// Прогона не было: сборка и тесты в этой среде запрещены политикой, я их не запускал и
// зелёными не называю.
public sealed class TranslationLostCandidateArtifactTests
{
    /// <summary>Учебная пара победителя: она публикуется и обязана остаться живой.</summary>
    private const string WinnerSource = "Он проверил файл";
    private const string WinnerTarget = "He checked the file";

    /// <summary>Отклонённый вариант победителя: то же предложение в неверном времени.</summary>
    private const string WinnerRejected = "He checks the file [WRONG: tense]";

    /// <summary>Учебная пара проигравшего: её и обязана сохранить запись кандидата.</summary>
    private const string LoserSource = "Он удалил файл";
    private const string LoserTarget = "He deleted the file";

    /// <summary>Отклонённый вариант проигравшего: то же предложение в неверном времени.</summary>
    private const string LoserRejected = "He deletes the file [WRONG: tense]";

    // ── Прежние правила, воспроизведённые для доказательства красноты ────────────

    /// <summary>
    /// ПРЕЖНЯЯ фраза отказа публикации: она утверждала сохранность кандидата на ЛЮБОМ
    /// пути, независимо от того, записал ли кто-нибудь хоть один байт. Воспроизведено
    /// здесь только для сравнения.
    /// </summary>
    private static string PriorRefusalSentence()
        => "Публикация отменена: живое состояние не менялось, обученный кандидат сохранён, " +
           "повторное обучение автоматически не запускается."; // Обещание без записи.

    /// <summary>
    /// ПРЕЖНЕЕ поведение хранения: записей проигравших кандидатов не создавалось вовсе,
    /// поэтому их число всегда было нулём. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static int PriorPersistedCandidateCount()
        => 0; // Кандидат выходил из области видимости и терялся.

    /// <summary>
    /// ПРЕЖНИЙ код отказа при любой судьбе кандидата, включая невозможность сохранить:
    /// отдельного отказа хранения не существовало. Воспроизведено только для сравнения.
    /// </summary>
    private static string PriorErrorCodeForAnyCandidateFate()
        => "STATE_VERSION_CHANGED_DURING_TRAINING"; // Отказ хранения был неотличим от отказа публикации.

    // ── Положительные контроли: зелёные и до правки, и после ────────────────────

    [Fact]
    public async Task Lawful_single_publication_still_passes()
    {
        var machine = new TranslationMachine();
        var baseVersion = machine.CurrentStateVersion;

        // Один писатель без конкурента: публикация обязана пройти как прежде.
        var published = await machine.TrainStructuredBatchAsync(WinnerBatch("candidate-lawful"));

        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.Equal(baseVersion + 1, machine.CurrentStateVersion);

        // Урок на месте и читается обычной дорогой продукта.
        var answer = await AskThroughOrdinaryPath(machine, WinnerSource);
        Assert.Equal(WinnerTarget, answer.FinalHumanResult);
    }

    [Fact]
    public async Task Successful_publication_writes_no_candidate_artifact()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            // Хранилище назначено, но успешной публикации записывать нечего.
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var published = await machine.TrainStructuredBatchAsync(WinnerBatch("candidate-no-artifact"));

            Assert.True(published.IsSuccess, published.ErrorMessage);
            // Ни одной записи проигравшего: запись появляется только по факту отказа.
            Assert.Empty(Directory.GetFiles(directory, "lost-candidate-*.json"));
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Красный случай 1: проигравший кандидат читается после отказа ────────────

    [Fact]
    public async Task Losing_candidate_is_persisted_and_restorable_while_prior_code_only_promised_it()
    {
        // Прежнее правило: записей не существовало, а фраза обещала сохранность.
        Assert.Equal(0, PriorPersistedCandidateCount());
        Assert.Contains("обученный кандидат сохранён", PriorRefusalSentence());

        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            // Поздний конфликт: победитель публикуется ровно между чтением основы
            // проигравшим и его сравнением-с-заменой.
            var refused = await TrainLosingLate(machine);

            // Публикация отказана, но теперь отказ несёт проверяемый указатель.
            Assert.False(refused.IsSuccess);
            Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", refused.ErrorCode);
            Assert.Contains("Обученный кандидат сохранён и доступен для чтения", refused.ErrorMessage);

            // Запись действительно лежит на диске, а не обещана словом.
            var files = Directory.GetFiles(directory, "lost-candidate-*.json");
            Assert.Single(files);
            // Указатель в сообщении ведёт именно на этот файл. Сверяется имя файла, а не вся
            // строка пути: у временного каталога может быть несколько равноправных написаний.
            Assert.Contains(Path.GetFileName(files[0]), refused.ErrorMessage);

            // Действующее правило против прежнего на том же входе: записей стало не ноль.
            Assert.NotEqual(PriorPersistedCandidateCount(), files.Length);

            // Проигравший кандидат читается НОВОЙ машиной и отвечает своим уроком.
            var artifact = ReadArtifact(files[0]);
            var restored = new TranslationMachine();
            var imported = restored.ImportCheckpoint(artifact.CandidateCheckpoint);
            Assert.True(imported.IsSuccess, imported.ErrorMessage);

            var restoredAnswer = await AskThroughOrdinaryPath(restored, LoserSource);
            Assert.Equal(LoserTarget, restoredAnswer.FinalHumanResult);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Красный случай 2: без хранилища отказ не обещает сохранности ────────────

    [Fact]
    public async Task Refusal_without_a_store_says_not_saved_instead_of_promising_a_checkpoint()
    {
        // Прежняя фраза обещала сохранность даже когда сохранять было нечем.
        Assert.Contains("обученный кандидат сохранён", PriorRefusalSentence());

        var machine = new TranslationMachine(); // Хранилище намеренно не назначено.

        var refused = await TrainLosingLate(machine);

        Assert.False(refused.IsSuccess);
        Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", refused.ErrorCode);
        // Действующее правило говорит прямо: кандидат НЕ сохранён, и почему.
        Assert.Contains("Обученный кандидат НЕ сохранён", refused.ErrorMessage);
        Assert.Contains("хранилище записей проигравших кандидатов не назначено", refused.ErrorMessage);
        // Прежнего обещания в сообщении больше нет ни в каком виде.
        Assert.DoesNotContain("кандидат сохранён, повторное обучение", refused.ErrorMessage);
    }

    // ── Красный случай 3: ошибка хранения имеет свой названный отказ ────────────

    [Fact]
    public async Task Storage_failure_returns_candidate_persistence_failed_and_never_claims_saved()
    {
        // Прежний код отказа был одним и тем же при любой судьбе кандидата.
        Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", PriorErrorCodeForAnyCandidateFate());

        // Настоящая, а не поддельная ошибка хранения: путь каталога занят ФАЙЛОМ,
        // поэтому создать каталог невозможно, и настоящее хранилище отказывает само.
        var occupiedPath = Path.Combine(Path.GetTempPath(), "lost-candidate-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(occupiedPath, "этот путь занят файлом, а не каталогом");
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(occupiedPath)).IsSuccess);

            // Ноль назван ДЛИННЫМ: номер версии состояния читается открытым свойством машины, а
            // оно отдаёт длинное целое. Без этого вывод типа взял бы обычное целое из литерала, и
            // присваивание длинного номера в него не собралось бы.
            var winnerVersionBefore = 0L; // Номер победившей версии до отказанного писателя.
            var refused = await TrainLosingLate(machine, afterWinner: () => winnerVersionBefore = machine.CurrentStateVersion);

            // Отдельный названный отказ, отличный от отказа публикации.
            Assert.False(refused.IsSuccess);
            Assert.Equal("CANDIDATE_PERSISTENCE_FAILED", refused.ErrorCode);
            Assert.NotEqual(PriorErrorCodeForAnyCandidateFate(), refused.ErrorCode);

            // Слово «сохранён» не произносится, и причина названа.
            Assert.Contains("Обученный кандидат НЕ сохранён", refused.ErrorMessage);
            Assert.DoesNotContain("Обученный кандидат сохранён и доступен", refused.ErrorMessage);

            // Живое состояние цело: победитель на месте, версия не сдвинулась отказом.
            Assert.Equal(winnerVersionBefore, machine.CurrentStateVersion);
            var answer = await AskThroughOrdinaryPath(machine, WinnerSource);
            Assert.Equal(WinnerTarget, answer.FinalHumanResult);
        }
        finally
        {
            if (File.Exists(occupiedPath)) File.Delete(occupiedPath);
        }
    }

    // ── Красный случай 4: запись несёт фактическое состояние и всю обстановку ───

    [Fact]
    public async Task Artifact_carries_actual_state_base_winner_cost_and_refusal_reason()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var baseVersion = machine.CurrentStateVersion;
            long casWinner = 0;
            string? thirdState = null;
            var refused = await TrainLosingLate(machine, afterWinner: () =>
            {
                casWinner = machine.CurrentStateVersion;
                machine.AfterConditionalPublishConflictBarrier = observed =>
                {
                    machine.AfterConditionalPublishConflictBarrier = null;
                    Assert.Equal(casWinner, observed);
                    // C публикуется ПОСЛЕ actual CAS A, до создания сообщения/артефакта A. Train C=0.
                    Assert.True(machine.ImportCheckpoint(machine.ExportCheckpoint().Value!).IsSuccess);
                    thirdState = TranslationAtomicJsonWriter.CanonicalStateIdentity(machine.ExportCheckpoint().Value!);
                };
            });
            Assert.False(refused.IsSuccess);

            var artifact = ReadArtifact(Directory.GetFiles(directory, "lost-candidate-*.json")[0]);

            // Основа и победитель названы числами, а не словами «что-то изменилось». Здесь
            // победитель ЕСТЬ, поэтому его поле заполнено; пустым оно бывает только когда
            // победителя не было вовсе — на пути отмены.
            Assert.Equal(baseVersion, artifact.BaseStateVersionNumber);
            Assert.NotNull(artifact.WinningStateVersionNumber);
            Assert.Equal(casWinner, artifact.WinningStateVersionNumber!.Value);
            Assert.Equal(casWinner + 1, machine.CurrentStateVersion);
            Assert.Contains($"CAS наблюдал опубликованную версию номер {casWinner}.", artifact.RefusalReason);
            Assert.Equal(thirdState, TranslationAtomicJsonWriter.CanonicalStateIdentity(machine.ExportCheckpoint().Value!));
            Assert.NotEqual(artifact.BaseStateVersionNumber, artifact.WinningStateVersionNumber!.Value);

            // Партия, происхождение и причина отказа на месте.
            Assert.Equal("candidate-loser", artifact.CandidateBatchId);
            Assert.NotEmpty(artifact.LineageExampleIdentities);
            Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", artifact.RefusalErrorCode);
            Assert.Contains("Публикация отменена", artifact.RefusalReason);

            // Уже оплаченный расход назван числом, а не признаком «было обучение».
            Assert.True(artifact.Cost.RowsTrained > 0);

            // ФАКТИЧЕСКОЕ состояние, а не журнал уроков: обученная память непуста и несёт
            // именно урок проигравшего.
            Assert.NotNull(artifact.CandidateCheckpoint);
            Assert.Contains(
                artifact.CandidateCheckpoint.StructuredTrainingIdentities,
                identity => string.Equals(identity.BatchId, "candidate-loser", StringComparison.Ordinal));

            // Победившее живое состояние урока проигравшего НЕ содержит: кандидат в
            // победителя не внесён.
            var live = machine.ExportCheckpoint();
            Assert.True(live.IsSuccess, live.ErrorMessage);
            Assert.DoesNotContain(
                live.Value!.StructuredTrainingIdentities,
                identity => string.Equals(identity.BatchId, "candidate-loser", StringComparison.Ordinal));
            Assert.Contains(
                live.Value!.StructuredTrainingIdentities,
                identity => string.Equals(identity.BatchId, "candidate-winner", StringComparison.Ordinal));
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Красный случай 5: сверка восстановленного состояния без времени выгрузки ─

    [Fact]
    public async Task Restored_state_parity_ignores_export_time_and_reading_the_artifact_starts_no_training()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var refused = await TrainLosingLate(machine);
            Assert.False(refused.IsSuccess);

            var liveVersionAfterRefusal = machine.CurrentStateVersion;
            var artifact = ReadArtifact(Directory.GetFiles(directory, "lost-candidate-*.json")[0]);

            // Опознание состояния в записи считается БЕЗ времени выгрузки.
            Assert.Equal(
                TranslationAtomicJsonWriter.CanonicalStateIdentity(artifact.CandidateCheckpoint),
                artifact.CandidateStateIdentity);

            // Восстановленное состояние совпадает с кандидатом по каноническому составу,
            // хотя время выгрузки у него новое.
            var restored = new TranslationMachine();
            Assert.True(restored.ImportCheckpoint(artifact.CandidateCheckpoint).IsSuccess);
            var restoredExport = restored.ExportCheckpoint();
            Assert.True(restoredExport.IsSuccess, restoredExport.ErrorMessage);
            Assert.NotEqual(artifact.CandidateCheckpoint.ExportedAtUtc, restoredExport.Value!.ExportedAtUtc);
            Assert.Equal(
                artifact.CandidateStateIdentity,
                TranslationAtomicJsonWriter.CanonicalStateIdentity(restoredExport.Value!));

            // Чтение записи обучения не запускает: ни версия, ни история живой машины не
            // сдвинулись от самого чтения.
            Assert.Equal(liveVersionAfterRefusal, machine.CurrentStateVersion);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Отдельно: плоский путь обучения не создаёт ложной записи кандидата ─────

    [Fact]
    public async Task Flat_training_path_never_writes_a_candidate_artifact()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var baseVersion = machine.CurrentStateVersion;

            // Плоский путь обучения памяти единиц не меняет: исполнение остаётся тем же живым
            // объектом. Поэтому записывать «контрольную точку кандидата» здесь было бы ложью о
            // данных — она содержала бы состояние ПОБЕДИТЕЛЯ под именем кандидата.
            var flat = await machine.TrainBatchAsync(new LearningBatch<TranslationDomain>(
                BatchId: "flat-batch",
                DomainTag: TranslationMachine.DomainTag,
                TrainingItems: [new LearningTrainingItem("i1", "Переведи", LoserTarget, LoserRejected)],
                SourceIds: ["simulation"]));

            // Ни одной записи кандидата этот путь не создаёт ни при успехе, ни при отказе.
            Assert.Empty(Directory.GetFiles(directory, "lost-candidate-*.json"));

            // Версия двигается только законной публикацией; отказ её не двигает.
            if (flat.IsSuccess)
                Assert.Equal(baseVersion + 1, machine.CurrentStateVersion);
            else
                Assert.Equal(baseVersion, machine.CurrentStateVersion);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>
    /// TrainLosingLate создаёт ПОЗДНИЙ конфликт настоящим продуктовым путём: проигравший
    /// писатель читает основу, обучает свою закрытую копию, и ровно перед его условной
    /// публикацией через барьер наблюдения публикуется победитель. Подставного писателя
    /// здесь нет: обе публикации идут обычным продуктовым методом обучения.
    /// </summary>
    private static Task<OperationResult<LearningBatchResult>> TrainLosingLate(
        TranslationMachine machine,
        Action? afterWinner = null)
    {
        // Барьер одноразовый: он снимается ДО публикации победителя, иначе публикация
        // победителя снова вошла бы в ту же точку и вызвала бы бесконечный повтор.
        machine.BeforeConditionalPublishBarrier = () =>
        {
            machine.BeforeConditionalPublishBarrier = null; // Одноразовость барьера.

            // Победитель публикуется обычным продуктовым методом. Метод завершается без
            // настоящего ожидания, поэтому синхронное получение результата здесь безопасно.
            var winner = machine.TrainStructuredBatchAsync(WinnerBatch("candidate-winner"))
                .GetAwaiter().GetResult();
            Assert.True(winner.IsSuccess, winner.ErrorMessage);

            afterWinner?.Invoke(); // Наблюдение состояния сразу после победителя.
        };

        // Проигравший писатель: условие по версии не называется, поэтому раннего отказа
        // нет и конфликт случается именно на сравнении-с-заменой.
        return machine.TrainStructuredBatchAsync(LoserBatch("candidate-loser"));
    }

    /// <summary>Читает сохранённую запись кандидата тем же общим правилом текста.</summary>
    private static TranslationLostCandidateArtifact ReadArtifact(string path)
    {
        var json = File.ReadAllText(path); // Фактические байты записи с диска.
        var artifact = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(
            json, TranslationCheckpointJsonOptions.Standard); // Одно правило чтения и записи.
        Assert.NotNull(artifact);
        return artifact!;
    }

    /// <summary>
    /// BatchOf собирает партию настоящим продуктовым конструктором. Отклонённый вариант
    /// передаётся явно: он обязан быть настоящим близким промахом того же предложения, а
    /// не механической припиской к верному ответу.
    /// </summary>
    private static TranslationLearningBatch BatchOf(
        string batchId,
        string source,
        string target,
        string rejected)
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
                    Rejected: rejected,
                    ValidationOutcome: "train_ready",
                    ValidationErrors: [],
                    VerifiedRuleTags: ["TENSE_PAST"])
            ],
            signatureMode: TranslationSignatureMode.ExactPair);

    /// <summary>Партия победителя одним названием, чтобы вызовы читались короче.</summary>
    private static TranslationLearningBatch WinnerBatch(string batchId)
        => BatchOf(batchId, WinnerSource, WinnerTarget, WinnerRejected);

    /// <summary>Партия проигравшего одним названием, чтобы вызовы читались короче.</summary>
    private static TranslationLearningBatch LoserBatch(string batchId)
        => BatchOf(batchId, LoserSource, LoserTarget, LoserRejected);

    /// <summary>
    /// AskThroughOrdinaryPath спрашивает машину её собственным дирижёром: отвечает именно
    /// проверяемое состояние, а не отдельный заново обученный объект.
    /// </summary>
    private static async Task<LogicalOrchestratorResult> AskThroughOrdinaryPath(
        TranslationMachine machine,
        string userRequest)
    {
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var answer = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: "lost-candidate-" + Guid.NewGuid().ToString("N"),
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

    /// <summary>Создаёт отдельный временный каталог записей для одной проверки.</summary>
    private static string NewTemporaryDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "lost-candidate-tests-" + Guid.NewGuid().ToString("N")); // Уникальное имя на проверку.
        Directory.CreateDirectory(directory);                        // Каталог создаётся заранее.
        return directory;                                            // Путь возвращается вызывающей проверке.
    }

    /// <summary>Убирает временный каталог после проверки, не роняя её при отказе уборки.</summary>
    private static void DeleteDirectory(string directory)
    {
        try { Directory.Delete(directory, recursive: true); } // Обычная уборка временного каталога.
        catch (IOException) { }                               // Занятый файл не должен ронять проверку.
        catch (UnauthorizedAccessException) { }               // Отказ доступа тоже не роняет проверку.
    }
}
