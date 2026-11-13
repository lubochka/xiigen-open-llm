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
// Это оставшиеся случаи приёмки раздела ремонта «конфликт двух писателей без потери
// оплаченного кандидата» (раздел номер 3 плана, файл
// codex-139-post117-addressed-repair-plan-2026-11-03.md). Первый кусок — сама запись
// проигравшего кандидата — проверен отдельно в TranslationLostCandidateArtifactTests.cs.
// Здесь закрываются четыре оставшихся требования приёмки.
//
// 1. Урок применяется РОВНО ОДИН раз, и проигравший не касается победителя. Опасность не
//    в том, что отказ не случится, а в том, что одна и та же работа применится дважды или
//    чужая работа подмешается в победившее состояние.
//
// 2. Измерение держит ОДНУ версию. Что было неправильно: измеренная точность записывалась
//    поверх самой свежей версии, то есть число, снятое со старого состояния, приписывалось
//    новому — качество объявлялось про то, чего никто не измерял. Что стало: запись числа
//    условна по измеренной версии, и завершившееся измерение новую версию не меняет.
//
// 3. Устаревшая названная версия — РАННИЙ отказ без обучения. Этот случай доказывает
//    отдельный путь и НЕ является доказательством позднего конфликта: там ничего не
//    обучалось, значит и оплаченного кандидата записывать не из чего.
//
// 4. Поздняя отмена не публикует ничего. Что было неправильно (в том числе в моём
//    предыдущем куске): отмена проверялась только на входе в метод, поэтому отмена,
//    пришедшая после обучения закрытой копии, публикацию уже не останавливала —
//    отменённая работа всё равно вставала в живое состояние. Что стало: отмена
//    проверяется последним действием перед публикацией, публикации нет вовсе, а уже
//    оплаченный обученный кандидат сохраняется той же записью.
//
// О красном прогоне на прежней логике, и честно о границе этого заявления.
//
// Прежние правила ниже записаны отдельными функциями с приставками Prior и Mutant — это
// ВОСПРОИЗВЕДЕНИЕ прежнего дефекта и требуемого планом мутанта, а не копии нового правила.
//
// ТРИ из них действительно были функциями от входа, и записаны функциями от входа: правило
// записи измерения зависит от того, какую версию измеряли и какая опубликована сейчас;
// правило отмены зависит от того, была ли отмена уже на входе в метод; мутант безусловной
// публикации зависит от порядка публикаций. Для них утверждение честное: прежнее правило на
// ТОМ ЖЕ входе даёт неверный ответ, а живой продуктовый путь — верный, и вход берётся из
// наблюдений этого же прогона.
//
// ЧЕТВЁРТОЕ поведение — мутант раннего отказа, который писал бы запись кандидата на любом
// отказе, — САМО было константой: оно именно тем и плохо, что от входа не зависит. Вход ему
// не придумывается, и заявления «на том же входе» про него НЕТ. Сравнивается его постоянный
// ответ с наблюдённым числом записей живого пути.
//
// Исправление 3 ноября 2026. В первой редакции этого файла все четыре функции входа не
// принимали и возвращали константу, а четыре утверждения о них сравнивали константу с
// собой — такие утверждения зелёные на любом коде, включая сломанный. Заявление о
// воспроизведении прежнего правила при этом было ничем не обеспечено. Действующие
// утверждения о живом поведении от этой правки не ослаблены: они те же.
//
// Мутант «безусловная публикация» при сохранённом раннем заслоне по названной версии
// обязан ломать случаи про один раз и про измерение — это прямое требование раздела
// плана. Он выражен наблюдаемым ответом мутанта и сверкой с живым ответом на том же входе.
//
// Управляемые точки синхронизации — не подставной писатель. Оба барьера наблюдения
// (перед условной публикацией партии и перед условной записью измеренного числа) только
// дают точку, в которой может пройти чужая публикация или прийти отмена. Сами публикации,
// измерение, отказы и запись кандидата идут настоящим продуктовым путём.
//
// Чего эти проверки НЕ доказывают, и я этого не заявляю. Первое: сверка восстановленного
// состояния по каноническому составу остаётся ГИПОТЕЗОЙ с условием опровержения — если
// загрузка контрольной точки не является точным обратным к её сборке по порядку
// исполнителей, падает именно сверка, а не доказательство сохранности записи; в этот файл
// такая сверка не вынесена намеренно. Второе: обычный продуктовый потребитель собственной
// конкурентной приёмки здесь не создан. Третье: «ровно один раз» доказано по НАБЛЮДАЕМОМУ
// следствию — состав обученной памяти и история обучения, — а не перехватом вызовов
// обучателя; так и написано.
//
// Прогона не было: сборка и тесты в этой среде запрещены политикой, я их не запускал и
// зелёными не называю.
public sealed class TranslationWriterConflictAcceptanceTests
{
    /// <summary>Учебная пара победителя.</summary>
    private const string WinnerSource = "Он проверил файл";
    private const string WinnerTarget = "He checked the file";
    private const string WinnerRejected = "He checks the file [WRONG: tense]";

    /// <summary>Учебная пара проигравшего.</summary>
    private const string LoserSource = "Он удалил файл";
    private const string LoserTarget = "He deleted the file";
    private const string LoserRejected = "He deletes the file [WRONG: tense]";

    // ── Прежние правила и мутант, воспроизведённые для доказательства красноты ───

    // Три из четырёх прежних правил ниже БЫЛИ функциями от входа, и записаны функциями от
    // входа — по образцу P11701RunCompletionFinalizerTests.cs:137-153. Четвёртое прежнее
    // поведение само было КОНСТАНТОЙ, и про него заявление «на том же входе» НЕ делается:
    // вход ему не придумывается. Так честнее, и образец такого честного случая уже есть в
    // TranslationLostCandidateArtifactTests.cs:97-113.

    /// <summary>
    /// МУТАНТ, которого требует раздел плана: безусловная публикация при сохранённом раннем
    /// заслоне по названной версии. Это НАСТОЯЩАЯ функция от входа: слепая запись ссылки
    /// ничего не сравнивает, поэтому в живом состоянии садится тот, кто публиковался
    /// ПОСЛЕДНИМ. Вход — наблюдённый порядок публикаций этого же прогона.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static string MutantUnconditionalPublishSeats(IReadOnlyList<string> publicationOrder)
        => publicationOrder.Count == 0
            ? "никто"                                  // Никто не публиковался — садиться некому.
            : publicationOrder[publicationOrder.Count - 1]; // Последняя слепая запись затирает предыдущую.

    /// <summary>
    /// ПРЕЖНЕЕ правило записи измерения: число записывалось поверх САМОЙ СВЕЖЕЙ версии, даже
    /// если измеряли другую. Возвращает номер версии, которой садится число, или нуль как
    /// «никакой версии».
    ///
    /// Оба довода прежнему правилу ДОСТУПНЫ, и то, что оно ИГНОРИРУЕТ измеренную версию, —
    /// это и есть сам дефект, а не придуманный вход: ответ прежнего правила меняется только
    /// от свежей версии. Живое правило, наоборот, зависит от обоих доводов: оно пишет число
    /// тогда и только тогда, когда измеренная версия всё ещё самая свежая.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    /// <remarks>
    /// Типы доводов и ответа — ДЛИННОЕ целое: номер версии состояния читается открытым свойством
    /// машины, а оно отдаёт длинное. Обычное целое здесь не собралось бы, а приведение молча
    /// усекало бы номер после предела обычного целого.
    /// </remarks>
    private static long PriorMeasuredNumberLandsOnVersion(long measuredVersion, long freshestVersion)
        => freshestVersion; // Измеренная версия прежним правилом не учитывалась — в этом дефект.

    /// <summary>
    /// ПРЕЖНЕЕ правило отмены: она проверялась ТОЛЬКО на входе в метод. Это НАСТОЯЩАЯ функция
    /// от входа: публикация происходила тогда и только тогда, когда на входе отмены ещё не
    /// было, а когда отмена приходила позже, она публикацию уже не останавливала.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorPublicationHappened(bool cancellationAlreadyRequestedAtEntry)
        => !cancellationAlreadyRequestedAtEntry; // Поздняя отмена прежде ничего не останавливала.

    /// <summary>
    /// МУТАНТ раннего отказа: он писал бы запись кандидата на ЛЮБОМ отказе, не глядя, было ли
    /// обучение. Это поведение само по себе КОНСТАНТА — оно именно тем и плохо, что от входа
    /// не зависит, — поэтому вход ему не придумывается и заявления «на том же входе» про него
    /// нет. Утверждение о нём сравнивает его постоянный ответ с НАБЛЮДЁННЫМ числом записей
    /// живого пути.
    /// </summary>
    private const int MutantEarlyRefusalArtifactCountIsConstant = 1; // Запись без обученного кандидата — ложь о данных.

    // ── Случай 1: урок применяется ровно один раз ───────────────────────────────

    [Fact]
    public async Task Late_conflict_applies_each_lesson_exactly_once_and_the_loser_never_touches_the_winner()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            // Снимок живого состояния СРАЗУ после победителя: он снимается внутри барьера,
            // то есть до того, как проигравший дошёл до сравнения-с-заменой.
            string? identityRightAfterWinner = null;

            // НАБЛЮДЁННЫЙ порядок попыток публикации этого же прогона. Он и есть вход для
            // мутанта безусловной публикации: мутант садит того, кто писал последним.
            var publicationOrder = new List<string>();

            var refused = await TrainLosingLate(machine, afterWinner: () =>
            {
                publicationOrder.Add("победитель"); // Победитель опубликовался первым.
                var justAfterWinner = machine.ExportCheckpoint();
                Assert.True(justAfterWinner.IsSuccess, justAfterWinner.ErrorMessage);
                identityRightAfterWinner =
                    TranslationAtomicJsonWriter.CanonicalStateIdentity(justAfterWinner.Value!);
            });
            publicationOrder.Add("проигравший"); // Проигравший пытался публиковаться вторым.

            Assert.False(refused.IsSuccess);
            Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", refused.ErrorCode);
            Assert.NotNull(identityRightAfterWinner);

            // Живое состояние не тронуто отказом НИ НА ЧТО: тот же канонический состав, что
            // сразу после победителя. Значит урок победителя применён ровно один раз, и
            // второго применения — ни своего, ни чужого — не случилось.
            var live = machine.ExportCheckpoint();
            Assert.True(live.IsSuccess, live.ErrorMessage);
            Assert.Equal(
                identityRightAfterWinner,
                TranslationAtomicJsonWriter.CanonicalStateIdentity(live.Value!));

            // В живом состоянии сидит ПОБЕДИТЕЛЬ, а не проигравший: прямой ответ на мутанта.
            var liveWho = live.Value!.KnnStore.Entries.Any(entry => entry.Source == WinnerSource)
                ? "победитель"
                : (live.Value!.KnnStore.Entries.Any(entry => entry.Source == LoserSource)
                    ? "проигравший"
                    : "никто");
            Assert.Equal("победитель", liveWho);

            // Двойное утверждение на ТОМ ЖЕ входе: наблюдённый порядок попыток публикации
            // подаётся мутанту, и его ответ противоположен живому. Мутант садит того, кто
            // писал последним, то есть проигравшего; живой продуктовый путь — победителя.
            Assert.Equal("проигравший", MutantUnconditionalPublishSeats(publicationOrder));
            Assert.NotEqual(MutantUnconditionalPublishSeats(publicationOrder), liveWho);

            // И мутант действительно зависит от входа, а не отвечает одно и то же: на обратном
            // порядке публикаций его ответ другой. Без этой проверки нельзя было отличить
            // функцию от входа от замаскированной константы.
            var reversedOrder = publicationOrder.AsEnumerable().Reverse().ToList();
            Assert.Equal("победитель", MutantUnconditionalPublishSeats(reversedOrder));
            Assert.NotEqual(
                MutantUnconditionalPublishSeats(publicationOrder),
                MutantUnconditionalPublishSeats(reversedOrder));

            // Ни одной строки проигравшего в победившем состоянии, ни по памяти, ни по
            // опознаниям учебных строк.
            Assert.DoesNotContain(live.Value!.KnnStore.Entries, entry => entry.Source == LoserSource);
            Assert.DoesNotContain(
                live.Value!.StructuredTrainingIdentities,
                identity => identity.BatchId == "candidate-loser");

            // История обучения живого состояния — ровно одна запись: один победитель, один раз.
            Assert.Single(live.Value!.TrainingHistory);

            // А урок проигравшего целиком лежит в записи кандидата, и ровно один раз.
            var artifact = ReadArtifact(Directory.GetFiles(directory, "lost-candidate-*.json")[0]);
            Assert.Single(artifact.CandidateCheckpoint.TrainingHistory);
            Assert.Contains(
                artifact.CandidateCheckpoint.KnnStore.Entries,
                entry => entry.Source == LoserSource);
            Assert.DoesNotContain(
                artifact.CandidateCheckpoint.KnnStore.Entries,
                entry => entry.Source == WinnerSource);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Случай 2: измерение держит одну версию ──────────────────────────────────

    [Fact]
    public async Task Measurement_that_held_one_version_does_not_change_the_newly_published_one()
    {
        var machine = new TranslationMachine();

        // Обученное состояние нужно, иначе измерять нечего и записи не происходит.
        Assert.True((await machine.TrainStructuredBatchAsync(WinnerBatch("measure-base"))).IsSuccess);

        // Точность свежей версии ДО измерения: это её собственное число от её публикации.
        var beforeMeasurement = machine.ExportCheckpoint();
        Assert.True(beforeMeasurement.IsSuccess, beforeMeasurement.ErrorMessage);
        var accuracyOfOwnPublication = beforeMeasurement.Value!.AccuracyAfterLastBatch;

        // Версия, которую измеряем: вход прежнего правила записи измерения.
        var measuredVersion = machine.CurrentStateVersion;

        // Чужая публикация проходит РОВНО между снятием числа и его записью.
        machine.BeforeMeasuredAccuracyPublishBarrier = () =>
        {
            machine.BeforeMeasuredAccuracyPublishBarrier = null; // Барьер одноразовый.
            var competitor = machine.TrainStructuredBatchAsync(LoserBatch("measure-competitor"))
                .GetAwaiter().GetResult();
            Assert.True(competitor.IsSuccess, competitor.ErrorMessage);
        };

        // Измерение заведомо даёт НУЛЕВУЮ точность: ожидаемый ответ назван заведомо неверным.
        var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "measure-shifted",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems: [new LearningEvaluationItem("i1", WinnerSource, "заведомо не тот ответ")]));

        // Число к другой версии не приписывается: отказ с названием обеих величин.
        Assert.False(evaluation.IsSuccess);
        Assert.Equal("MEASURED_STATE_VERSION_CHANGED", evaluation.ErrorCode);

        // Свежая версия, опубликованная конкурентом, измерением НЕ изменена: её точность не
        // стала нулём, который снят со СТАРОЙ версии. Это и есть ответ прежнему правилу,
        // которое записало бы этот ноль поверх самой свежей версии.
        var afterMeasurement = machine.ExportCheckpoint();
        Assert.True(afterMeasurement.IsSuccess, afterMeasurement.ErrorMessage);
        Assert.NotEqual(0.0, afterMeasurement.Value!.AccuracyAfterLastBatch);
        // Точность осталась числом собственной публикации, а не привнесённым нулём: сравнение
        // идёт с числом, снятым до измерения, только как с ориентиром ненулевого значения.
        Assert.True(accuracyOfOwnPublication > 0.0);

        // Двойное утверждение на ТОМ ЖЕ входе: измеренная версия и самая свежая версия
        // подаются прежнему правилу, и его ответ противоположен живому.
        var freshestVersion = machine.CurrentStateVersion;
        Assert.NotEqual(measuredVersion, freshestVersion); // Конкурент действительно сдвинул версию.

        // Куда число село НА САМОМ ДЕЛЕ: ноль означает «ни к какой версии не приписано». Ноль
        // назван длинным, потому что номер версии длинный: смешивать обычное целое с длинным в
        // одном сравнении нельзя.
        var liveNumberLandedOnVersion =
            afterMeasurement.Value!.AccuracyAfterLastBatch == 0.0 ? freshestVersion : 0L;

        Assert.Equal(freshestVersion, PriorMeasuredNumberLandsOnVersion(measuredVersion, freshestVersion));
        Assert.Equal(0L, liveNumberLandedOnVersion);
        Assert.NotEqual(
            PriorMeasuredNumberLandsOnVersion(measuredVersion, freshestVersion),
            liveNumberLandedOnVersion);

        // Прежнее правило действительно зависит от входа, а не отвечает одно и то же: на входе
        // «измеренная версия и есть самая свежая» его ответ другой. И именно в этом случае
        // живое правило число пишет — то есть расхождение возникает ровно тогда, когда версия
        // за время измерения сдвинулась.
        Assert.NotEqual(
            PriorMeasuredNumberLandsOnVersion(measuredVersion, freshestVersion),
            PriorMeasuredNumberLandsOnVersion(measuredVersion, measuredVersion));

        // Урок конкурента на месте: его публикация отказом измерения не откатилась.
        Assert.Contains(
            afterMeasurement.Value!.StructuredTrainingIdentities,
            identity => identity.BatchId == "measure-competitor");
    }

    [Fact]
    public async Task Uncontended_measurement_lawfully_publishes_its_number()
    {
        // Положительный контроль: без конкурента измерение законно записывает своё число.
        // Зелёный и до правки, и после: условная запись одиночному писателю не мешает.
        var machine = new TranslationMachine();
        Assert.True((await machine.TrainStructuredBatchAsync(WinnerBatch("measure-alone"))).IsSuccess);

        var before = machine.ExportCheckpoint().Value!;
        var version = machine.CurrentStateVersion;
        var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            EvaluationId: "measure-alone-eval",
            DomainTag: TranslationMachine.DomainTag,
            EvaluationItems: [new LearningEvaluationItem("i1", WinnerSource, WinnerTarget)]));

        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.Equal(1.0, evaluation.Value!.Accuracy);
        Assert.Equal(version + 1, machine.CurrentStateVersion);
        var after = machine.ExportCheckpoint().Value!;
        Assert.Equal(1.0, after.AccuracyAfterLastBatch);
        Assert.Equal(TranslationAtomicJsonWriter.CanonicalStateIdentity(before with { AccuracyAfterLastBatch = 1.0 }),
            TranslationAtomicJsonWriter.CanonicalStateIdentity(after));
    }

    [Fact]
    public async Task Late_evaluation_cancellation_preserves_version_accuracy_and_full_state()
    {
        var machine = new TranslationMachine();
        Assert.True((await machine.TrainStructuredBatchAsync(WinnerBatch("late-eval-base"))).IsSuccess);
        var before = machine.ExportCheckpoint().Value!;
        var version = machine.CurrentStateVersion;
        using var cancellation = new CancellationTokenSource();
        machine.BeforeMeasuredAccuracyPublishBarrier = () => cancellation.Cancel();
        var result = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            "late-eval-cancel", TranslationMachine.DomainTag,
            [new LearningEvaluationItem("i1", WinnerSource, WinnerTarget)]), cancellation.Token);
        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal(version, machine.CurrentStateVersion);
        Assert.Equal(TranslationAtomicJsonWriter.CanonicalStateIdentity(before),
            TranslationAtomicJsonWriter.CanonicalStateIdentity(machine.ExportCheckpoint().Value!));
    }

    [Fact]
    public async Task Ordinary_incorrect_answer_is_a_successfully_measured_zero_not_cancellation()
    {
        var machine = new TranslationMachine();
        Assert.True((await machine.TrainStructuredBatchAsync(WinnerBatch("wrong-eval-base"))).IsSuccess);
        var before = machine.ExportCheckpoint().Value!;
        var version = machine.CurrentStateVersion;
        var result = await machine.EvaluateAsync(new LearningEvaluationSet<TranslationDomain>(
            "wrong-eval", TranslationMachine.DomainTag,
            [new LearningEvaluationItem("i1", WinnerSource, "заведомо неверный ожидаемый ответ")]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0.0, result.Value!.Accuracy);
        Assert.Equal(version + 1, machine.CurrentStateVersion);
        Assert.Equal(TranslationAtomicJsonWriter.CanonicalStateIdentity(before with { AccuracyAfterLastBatch = 0.0 }),
            TranslationAtomicJsonWriter.CanonicalStateIdentity(machine.ExportCheckpoint().Value!));
    }

    // ── Случай 3: устаревшая названная версия — ранний отказ без обучения ───────

    [Fact]
    public async Task Stale_expected_version_is_an_early_refusal_with_no_training_and_no_candidate_artifact()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var staleBase = machine.CurrentStateVersion;

            // Законная публикация уводит версию вперёд.
            Assert.True((await machine.TrainStructuredBatchAsync(WinnerBatch("early-base"))).IsSuccess);

            var beforeRefusal = machine.ExportCheckpoint();
            Assert.True(beforeRefusal.IsSuccess, beforeRefusal.ErrorMessage);
            var identityBefore = TranslationAtomicJsonWriter.CanonicalStateIdentity(beforeRefusal.Value!);
            var versionBefore = machine.CurrentStateVersion;

            // Писатель называет УСТАРЕВШУЮ версию: отказ наступает ДО обучения.
            var refused = await machine.TrainStructuredBatchAsync(
                LoserBatch("early-loser"),
                expectedStateVersion: staleBase);

            Assert.False(refused.IsSuccess);
            Assert.Equal("STATE_VERSION_CHANGED_DURING_TRAINING", refused.ErrorCode);

            // Обучения не было ни на одну строку: состав состояния и версия не сдвинулись.
            var afterRefusal = machine.ExportCheckpoint();
            Assert.True(afterRefusal.IsSuccess, afterRefusal.ErrorMessage);
            Assert.Equal(identityBefore, TranslationAtomicJsonWriter.CanonicalStateIdentity(afterRefusal.Value!));
            Assert.Equal(versionBefore, machine.CurrentStateVersion);
            Assert.Single(afterRefusal.Value!.TrainingHistory);

            // И записи кандидата НЕТ: платить было не за что, поэтому и терять нечего.
            //
            // Сравнение с мутантом здесь честно названо: его поведение САМО константа — он
            // писал бы запись на любом отказе, не глядя на вход, — поэтому заявления «на том
            // же входе» про него нет. Сравнивается его постоянный ответ с НАБЛЮДЁННЫМ числом
            // записей живого пути.
            var artifacts = Directory.GetFiles(directory, "lost-candidate-*.json");
            Assert.Empty(artifacts);
            Assert.NotEqual(MutantEarlyRefusalArtifactCountIsConstant, artifacts.Length);

            // Это ранний путь. Поздний конфликт им НЕ доказывается: там обучение состоялось.
            Assert.DoesNotContain("отменена ПОСЛЕ обучения", refused.ErrorMessage);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Случай 4: поздняя отмена не публикует ничего ────────────────────────────

    [Fact]
    public async Task Late_cancellation_publishes_nothing_and_still_keeps_the_paid_candidate()
    {
        var directory = NewTemporaryDirectory();
        try
        {
            var machine = new TranslationMachine();
            Assert.True(machine.UseLostCandidateStore(
                new TranslationFileLostCandidateStore(directory)).IsSuccess);

            var versionBefore = machine.CurrentStateVersion;
            var beforeCancel = machine.ExportCheckpoint();
            Assert.True(beforeCancel.IsSuccess, beforeCancel.ErrorMessage);
            var identityBefore = TranslationAtomicJsonWriter.CanonicalStateIdentity(beforeCancel.Value!);

            using var cancellation = new CancellationTokenSource();

            // Отмена приходит РОВНО после обучения закрытой копии и ДО публикации.
            machine.BeforeConditionalPublishBarrier = () =>
            {
                machine.BeforeConditionalPublishBarrier = null; // Барьер одноразовый.
                cancellation.Cancel();                          // Поздняя отмена именно в этой точке.
            };

            var cancelled = await machine.TrainStructuredBatchAsync(
                LoserBatch("cancel-late"),
                cancellation.Token);

            // Отмена названа своим кодом, а не отказом конфликта версий.
            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Contains("отменена ПОСЛЕ обучения закрытой копии и ДО публикации", cancelled.ErrorMessage);

            // Публикации НЕ было: это прямой ответ прежнему правилу, которое публиковало.
            Assert.Equal(versionBefore, machine.CurrentStateVersion);
            var afterCancel = machine.ExportCheckpoint();
            Assert.True(afterCancel.IsSuccess, afterCancel.ErrorMessage);
            Assert.Equal(identityBefore, TranslationAtomicJsonWriter.CanonicalStateIdentity(afterCancel.Value!));
            Assert.DoesNotContain(afterCancel.Value!.KnnStore.Entries, entry => entry.Source == LoserSource);

            // Двойное утверждение на ТОМ ЖЕ входе. Вход прежнего правила — «была ли отмена уже
            // запрошена на входе в метод». В этом прогоне на входе отмены не было: она пришла
            // позже, в точке наблюдения. Прежнее правило на таком входе публиковало, живой
            // продуктовый путь — не публикует.
            const bool cancellationAlreadyRequestedAtEntry = false;
            var livePublishedAfterLateCancellation = machine.CurrentStateVersion != versionBefore;

            Assert.True(PriorPublicationHappened(cancellationAlreadyRequestedAtEntry));
            Assert.False(livePublishedAfterLateCancellation);
            Assert.NotEqual(
                PriorPublicationHappened(cancellationAlreadyRequestedAtEntry),
                livePublishedAfterLateCancellation);

            // А на входе «отмена была уже на входе» прежнее правило тоже не публиковало —
            // значит функция действительно зависит от входа, а не отвечает одно и то же.
            Assert.False(PriorPublicationHappened(cancellationAlreadyRequestedAtEntry: true));

            // Частичного состояния не возникает: живое состояние осталось ровно прежним целиком.
            Assert.Empty(afterCancel.Value!.TrainingHistory);

            // Оплаченный обученный кандидат при этом не потерян: запись одна и её причина —
            // именно отмена, а не конфликт версий.
            var files = Directory.GetFiles(directory, "lost-candidate-*.json");
            Assert.Single(files);
            var artifact = ReadArtifact(files[0]);
            Assert.Equal("CANCELLED", artifact.RefusalErrorCode);
            Assert.Contains("отменена ПОСЛЕ обучения", artifact.RefusalReason);

            // Запись не лжёт о том, что произошло: победителя НЕ было, и поле победителя
            // пустое, а не заполнено номером основы. Основа при этом названа своим полем.
            Assert.Null(artifact.WinningStateVersionNumber);
            Assert.Equal(versionBefore, artifact.BaseStateVersionNumber);
            Assert.Contains(artifact.CandidateCheckpoint.KnnStore.Entries, entry => entry.Source == LoserSource);
            Assert.True(artifact.Cost.RowsTrained > 0);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>
    /// TrainLosingLate создаёт ПОЗДНИЙ конфликт настоящим продуктовым путём: проигравший
    /// читает основу и обучает свою закрытую копию, и ровно перед его условной публикацией
    /// через барьер наблюдения публикуется победитель.
    /// </summary>
    private static Task<OperationResult<LearningBatchResult>> TrainLosingLate(
        TranslationMachine machine,
        Action? afterWinner = null)
    {
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

    /// <summary>BatchOf собирает партию настоящим продуктовым конструктором.</summary>
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

    /// <summary>Партия победителя одним названием.</summary>
    private static TranslationLearningBatch WinnerBatch(string batchId)
        => BatchOf(batchId, WinnerSource, WinnerTarget, WinnerRejected);

    /// <summary>Партия проигравшего одним названием.</summary>
    private static TranslationLearningBatch LoserBatch(string batchId)
        => BatchOf(batchId, LoserSource, LoserTarget, LoserRejected);

    /// <summary>Создаёт отдельный временный каталог записей для одной проверки.</summary>
    private static string NewTemporaryDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "writer-conflict-tests-" + Guid.NewGuid().ToString("N")); // Уникальное имя на проверку.
        Directory.CreateDirectory(directory);                         // Каталог создаётся заранее.
        return directory;                                             // Путь возвращается проверке.
    }

    /// <summary>Убирает временный каталог после проверки, не роняя её при отказе уборки.</summary>
    private static void DeleteDirectory(string directory)
    {
        try { Directory.Delete(directory, recursive: true); } // Обычная уборка временного каталога.
        catch (IOException) { }                               // Занятый файл не должен ронять проверку.
        catch (UnauthorizedAccessException) { }               // Отказ доступа тоже не роняет проверку.
    }
}
