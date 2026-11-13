// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TensorCore.Layers;
using Tokenizer;
// Имя пары предпочтений есть в двух пространствах имён; в этих проверках оно
// всегда означает пару непрерывного обучения, а не материал родного тренера.
using DpoPair = ContinuousLearning.DpoPair;

/// <summary>
/// Проверки переноса «обучение с учителем не возвращает текст, который
/// исправляющее сравнение предпочтений отвергло».
///
/// Что проверяется по-человечески. Пользователь исправил ответ модели: плохой
/// текст стал отвергнутым, хороший — выбранным. Дальше идёт один шаг прямого
/// сравнения предпочтений (DPO — Direct Preference Optimization), который
/// уводит модель от отвергнутого текста, и затем уроки обучения с учителем
/// (SFT — supervised fine-tuning). Если ответом урока всё ещё стоит тот самый
/// отвергнутый текст, последний шаг учит модель именно ему и гасит только что
/// полученное исправление. Проверки доказывают, что в обучение уходит
/// выбранный текст, а не отвергнутый, и что ничего другого при этом не
/// поменялось.
///
/// Чего эти проверки НЕ доказывают. Ни одного настоящего обновления весов тут
/// нет: шпион учебного шага веса не двигает, а исполнитель сравнения
/// предпочтений только выкладывает требуемые файлы. Это проверки подключения
/// и договора, а не измеренное обучение.
/// </summary>
public sealed class DpoSftCorrectionCollisionTests
{
    // Домен один и тот же во всех проверках: по нему сверяются урок и пара.
    private const string Domain = "BUG";

    /// <summary>
    /// Решающая проверка: при допущенной исправляющей паре в обучение уходит
    /// выбранный текст, а не отвергнутый. Против прежней логики, которая
    /// кодировала ответ урока как есть, эта проверка падает.
    /// </summary>
    [Fact]
    public async Task AdmittedCorrection_TrainsChosenTextInsteadOfRejectedText()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableCorrectionTransaction(
            fixture,
            "collision-main");
        var pair = Assert.Single(pairs);
        // Урок дословно повторяет отвергнутый текст пары: это и есть столкновение.
        var staleLesson = fixture.Example(Domain, pair.Prompt, pair.Rejected);
        var spy = new RecordingSupervisedStepExecutorN63C();
        var dpo = new CompletingDpoExecutorN63C();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageN63C(),
            fixture.CreateFactory(),
            CreateAuthorityN63C(),
            dpo,
            spy,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                Domain,
                [staleLesson],
                pairs,
                transaction,
                CancellationToken.None);

        // Шаг сравнения предпочтений выполнен ровно один раз и до уроков.
        Assert.Equal(1, dpo.CallCount);
        // Шпион веса не двигает, поэтому тренер честно отказывается публиковать
        // переходник: именно этот отказ доказывает отсутствие настоящих обновлений.
        Assert.Equal("NO_ADAPTER_UPDATE", result.ErrorCode);
        var captured = Assert.Single(spy.CapturedTokenSequences);
        var tokenizer = LoadFixtureTokenizerN63C(fixture);
        // В обучение ушёл выбранный текст в формате учебной строки.
        Assert.Equal(
            SupervisedTextSequence.EncodeJointLf(
                tokenizer,
                pair.Prompt,
                pair.Chosen),
            captured);
        // Отвергнутый текст в обучение не ушёл: это решающее отличие от прежней логики.
        Assert.NotEqual(
            SupervisedTextSequence.EncodeJointLf(
                tokenizer,
                pair.Prompt,
                pair.Rejected),
            captured);
        // Вес урока передан без изменения.
        Assert.Equal(1f, Assert.Single(spy.CapturedWeights));
    }

    /// <summary>
    /// Отрицательный контроль: без дословного столкновения урок уходит в
    /// обучение без изменения. Проверяются три соседних случая — другой ответ,
    /// другой запрос и пустой список пар.
    /// </summary>
    [Theory]
    [InlineData("other-response")]
    [InlineData("other-prompt")]
    [InlineData("no-pairs")]
    public async Task LessonWithoutExactCollision_IsTrainedUnchanged(string shape)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableCorrectionTransaction(
            fixture,
            $"no-collision-{shape}");
        var pair = Assert.Single(pairs);
        // Урок строится так, чтобы дословного совпадения не было.
        var lessonPrompt = shape == "other-prompt" ? "other ask" : pair.Prompt;
        var lessonResponse = shape == "other-response"
            ? "near " + pair.Rejected
            : pair.Rejected;
        var lesson = fixture.Example(Domain, lessonPrompt, lessonResponse);
        var spy = new RecordingSupervisedStepExecutorN63C();
        var dpo = new CompletingDpoExecutorN63C();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageN63C(),
            fixture.CreateFactory(),
            CreateAuthorityN63C(),
            dpo,
            spy,
            fixture.AdapterDirectory);

        // В случае «нет пар» путь сравнения предпочтений не вызывается вовсе.
        var result = shape == "no-pairs"
            ? await trainer.TrainAsync(Domain, [lesson], [])
            : await ((ITransactionalIncrementalTrainer)trainer)
                .TrainTransactionAsync(
                    Domain,
                    [lesson],
                    pairs,
                    transaction,
                    CancellationToken.None);

        Assert.Equal("NO_ADAPTER_UPDATE", result.ErrorCode);
        Assert.Equal(shape == "no-pairs" ? 0 : 1, dpo.CallCount);
        var captured = Assert.Single(spy.CapturedTokenSequences);
        var tokenizer = LoadFixtureTokenizerN63C(fixture);
        // Ушли ровно те знаки, что и до переноса: ответ урока не подменён.
        Assert.Equal(
            SupervisedTextSequence.EncodeJointLf(
                tokenizer,
                lessonPrompt,
                lessonResponse),
            captured);
    }

    /// <summary>
    /// Неоднозначность: один и тот же устаревший урок исправляют две допущенные
    /// пары. Проекция обязана отказаться явной ошибкой и не выбрать одну из
    /// сторон; ни один урок при отказе не меняется.
    /// </summary>
    [Fact]
    public void AmbiguousCorrections_AreRefusedWithoutChoosingASide()
    {
        var lesson = Lesson("fix abs", "wrong abs");
        var first = Candidate("pair-1", "fix abs", "right abs", "wrong abs");
        var second = Candidate("pair-2", "fix abs", "other right abs", "wrong abs");

        var projected = DpoSupervisedCorrectionProjection.Project(
            Domain,
            [lesson],
            [first, second],
            new string('a', 64));

        // Отказ назван точным словом, а не тихим выбором одной из пар.
        Assert.False(projected.IsSuccess);
        Assert.Equal(
            "DPO_SUPERVISED_CORRECTION_AMBIGUOUS",
            projected.ErrorCode);
        Assert.Equal(
            DpoSupervisedCorrectionProjection.AmbiguousErrorCode,
            projected.ErrorCode);
        // Исходный урок не затронут: проекция возвращает копию, а не правит вход.
        Assert.Equal("wrong abs", lesson.Response);
        // Кандидат на исправление при этом действительно виден: отказ не из-за
        // того, что столкновение не нашлось.
        Assert.True(DpoSupervisedCorrectionProjection.HasCorrectionCandidate(
            Domain,
            [lesson],
            [first.Pair, second.Pair]));
    }

    /// <summary>
    /// Исправленный урок сохраняет обе личности источника: прежняя остаётся в
    /// происхождении, производная каноничная и тоже в происхождении, а номер
    /// пары берётся из записи транзакции. Запрос, домен, вес, время снятия,
    /// позиция и число уроков не меняются.
    /// </summary>
    [Fact]
    public void CorrectedLesson_KeepsBothSourceIdentitiesAndEveryOtherField()
    {
        var stale = Lesson("fix abs", "wrong abs");
        var untouched = Lesson("fix sign", "right sign");
        var candidate = Candidate("pair-42", "fix abs", "right abs", "wrong abs");
        var originalBatch = BuildV1BatchDigestByProduct([stale, untouched]);

        var projected = DpoSupervisedCorrectionProjection.Project(
            Domain,
            [stale, untouched],
            [candidate],
            originalBatch);

        Assert.True(projected.IsSuccess, $"{projected.ErrorCode}: {projected.ErrorMessage}");
        var value = projected.Value!;
        // Число уроков и их порядок сохранены.
        Assert.Equal(2, value.Examples.Count);
        var corrected = value.Examples[0];
        // Заменён ровно ответ: он стал выбранным текстом пары.
        Assert.Equal("right abs", corrected.Response);
        // Запрос, домен, вес и время снятия сохранены дословно.
        Assert.Equal(stale.Prompt, corrected.Prompt);
        Assert.Equal(stale.Domain, corrected.Domain);
        Assert.Equal(stale.Weight, corrected.Weight);
        Assert.Equal(stale.CapturedAt, corrected.CapturedAt);
        // Второй урок не тронут вообще: тот же объект.
        Assert.Same(untouched, value.Examples[1]);
        // Производная личность источника отличается от прежней и канонична.
        Assert.NotEqual(stale.SourceId, corrected.SourceId);
        Assert.Equal(
            TrainingSourceProvenance.CanonicalSourceIdLength,
            corrected.SourceId!.Length);
        // Производная личность строится ровно по правилу карточки: имя версии,
        // прежний источник, номер пары и отпечаток набора пар, а содержимым —
        // точные байты выбранного ответа.
        Assert.Equal(
            TrainingSourceProvenance.CreateSourceId(
                string.Concat(
                    DpoSupervisedCorrectionProjection.Version,
                    ":",
                    stale.SourceId,
                    ":",
                    "pair-42",
                    ":",
                    DpoSupervisedCorrectionProjection
                        .ComputeAdmittedPairBatchSha256([candidate])),
                "right abs"),
            corrected.SourceId);
        // В происхождении лежат обе личности источника заменённого урока.
        Assert.Contains(stale.SourceId, value.TrainingSources.SourceIds);
        Assert.Contains(corrected.SourceId, value.TrainingSources.SourceIds);
        // Нетронутый урок тоже остаётся в происхождении.
        Assert.Contains(untouched.SourceId, value.TrainingSources.SourceIds);
        // Родословная не создаёт дополнительных учебных строк: три личности на
        // два урока — это один заменённый плюс один нетронутый.
        Assert.Equal(3, value.TrainingSources.SourceIds.Count);
        // Запись замены называет позицию, номер пары и отпечатки обоих текстов.
        var binding = Assert.Single(value.Corrections);
        Assert.Equal(0, binding.ExampleOrdinal);
        Assert.Equal(stale.SourceId, binding.OriginalSourceId);
        Assert.Equal(corrected.SourceId, binding.CorrectedSourceId);
        Assert.Equal("pair-42", binding.TransactionPairId);
        Assert.Equal(Sha256Hex("wrong abs"), binding.OriginalResponseSha256);
        Assert.Equal(Sha256Hex("right abs"), binding.CorrectedResponseSha256);
        // Отпечаток набора до замены сохранён как есть.
        Assert.Equal(originalBatch, value.OriginalBatchSha256);
    }

    /// <summary>
    /// Границы кандидата: пара другого домена, пара с другим запросом, пара с
    /// другим отвергнутым текстом и пара, у которой выбранный текст равен
    /// отвергнутому, исправления не дают. Ни одного «близкого» совпадения.
    /// </summary>
    [Fact]
    public void NonCorrectingPairs_NeverTouchTheLesson()
    {
        var lesson = Lesson("fix abs", "wrong abs");
        var otherDomain = new DpoSupervisedCorrectionCandidatePair(
            "pair-other-domain",
            new DpoPair("fix abs", "right abs", "wrong abs", "PLAN", Captured));
        var otherPrompt = Candidate("pair-other-prompt", "fix sign", "right abs", "wrong abs");
        var otherRejected = Candidate("pair-other-rejected", "fix abs", "right abs", "wrong sign");
        var degenerate = Candidate("pair-degenerate", "fix abs", "wrong abs", "wrong abs");
        // Пара с обрезкой пробелов тоже не совпадает: сравнение только дословное.
        var trimmed = Candidate("pair-trimmed", "fix abs", "right abs", " wrong abs ");
        DpoSupervisedCorrectionCandidatePair[] candidates =
            [otherDomain, otherPrompt, otherRejected, degenerate, trimmed];

        // Кандидата на исправление нет ни по одной из этих пар.
        Assert.False(DpoSupervisedCorrectionProjection.HasCorrectionCandidate(
            Domain,
            [lesson],
            candidates.Select(static value => value.Pair).ToArray()));

        var projected = DpoSupervisedCorrectionProjection.Project(
            Domain,
            [lesson],
            candidates,
            new string('b', 64));

        Assert.True(projected.IsSuccess, $"{projected.ErrorCode}: {projected.ErrorMessage}");
        // Замен нет, урок тот же объект, ответ не менялся.
        Assert.Empty(projected.Value!.Corrections);
        Assert.Same(lesson, Assert.Single(projected.Value!.Examples));
        Assert.Equal("wrong abs", lesson.Response);
    }

    /// <summary>
    /// Отпечаток учебного набора первой версии переносом не изменён. Ожидаемое
    /// значение считается в проверке независимо, по описанному порядку полей, а
    /// не берётся из продукта; поэтому любое изменение прежнего правила эту
    /// проверку ломает. Исправленный набор получает отличный отпечаток второй
    /// версии, который зависит и от отпечатка набора до замены.
    /// </summary>
    [Fact]
    public void FirstVersionBatchDigest_IsUnchanged_AndCorrectedDigestDiffers()
    {
        var stale = Lesson("fix abs", "wrong abs");
        var candidate = Candidate("pair-7", "fix abs", "right abs", "wrong abs");
        var originalBatch = BuildV1BatchDigestByProduct([stale]);

        // Независимый расчёт прежнего отпечатка по описанному порядку полей.
        Assert.Equal(
            BuildV1BatchDigestIndependently([stale]),
            originalBatch);

        var projected = DpoSupervisedCorrectionProjection.Project(
            Domain,
            [stale],
            [candidate],
            originalBatch);
        Assert.True(projected.IsSuccess, $"{projected.ErrorCode}: {projected.ErrorMessage}");
        var correctedExamples = projected.Value!.Examples;

        // Отпечаток исправленного набора второй версии отличается и от прежнего
        // отпечатка исходного набора, и от первой версии того же набора после
        // замены: исправленный набор нельзя принять за неисправленный.
        var correctedDigest = BuildCorrectedBatchDigestByProduct(
            correctedExamples,
            projected.Value!);
        Assert.NotEqual(originalBatch, correctedDigest);
        Assert.NotEqual(
            BuildV1BatchDigestByProduct(correctedExamples),
            correctedDigest);
        // Отпечаток остаётся шестидесяти четырёх шестнадцатеричных знаков.
        Assert.Equal(64, correctedDigest.Length);
        // Повторный расчёт на тех же данных даёт то же значение: восстановление
        // попадает в того же записанного кандидата, а не во второй проход.
        Assert.Equal(
            correctedDigest,
            BuildCorrectedBatchDigestByProduct(
                correctedExamples,
                projected.Value!));
    }

    /// <summary>
    /// Точное сравнение предпочтений и формат учебной строки переносом не
    /// затронуты: имена версий те же, разделителя в паре нет, а формат учебной
    /// строки по-прежнему даёт один вызов словаря на склейке «запрос, перевод
    /// строки, ответ».
    /// </summary>
    [Fact]
    public void ExactDpoFramingAndJointLfFormat_RemainUnchanged()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abab abab abab abab", 260);
        const string prompt = "a";
        const string chosen = "b";
        const string rejected = "c";

        // Формат учебной строки: тот же один вызов словаря на той же склейке.
        Assert.Equal("sft-joint-lf-v1", SupervisedTextSequence.ProfileVersion);
        Assert.Equal("\n", SupervisedTextSequence.Separator);
        Assert.Equal(
            tokenizer.Encode(prompt + "\n" + chosen),
            SupervisedTextSequence.EncodeJointLf(tokenizer, prompt, chosen));

        var batch = DpoExactPairMaterial.CreateBatch(
            tokenizer,
            prompt,
            chosen,
            rejected,
            DpoTrainerPairCustoms.CreateAdmitted(
                prompt,
                chosen,
                rejected,
                DpoPairSideAuthorKind.DeterministicGenerator,
                DpoPairSideAuthorKind.DeterministicGenerator,
                "fix-n63c-exact-dpo-unchanged").Admission);

        // Имена версий формата и цели обучения по предпочтениям не изменились.
        Assert.Equal("segmented-byte-BPE-v2", DpoExactPairMaterial.Version);
        Assert.Equal(
            "prompt-response-segments-no-separator-v2",
            DpoExactPairMaterial.FramingVersion);
        Assert.Equal(
            "response-sum-softplus-signal-weighted-v3",
            DpoExactPairMaterial.ObjectiveVersion);
        // Знаки пары — раздельные знаки запроса и ответа, склеенные встык.
        Assert.Equal(
            tokenizer.Encode(prompt)
                .Concat(tokenizer.Encode(chosen))
                .Select(static value => (float)value),
            batch.ChosenIds.Data);
        // Разделителя учебной строки в паре нет ни на одной позиции.
        Assert.DoesNotContain(
            (float)tokenizer.Encode(SupervisedTextSequence.Separator)[0],
            batch.ChosenIds.Data);
        // Длина запроса в паре считается без разделителя.
        Assert.Equal(
            tokenizer.Encode(prompt).Count,
            batch.ExactMaterial!.PromptLength);
    }

    // ─── оснастка проверок ────────────────────────────────────────────────────

    // Одно и то же время снятия во всех построенных уроках и парах: оно входит
    // в отпечатки, поэтому не должно плавать между расчётами одной проверки.
    private static readonly DateTimeOffset Captured =
        new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    // Учебная строка с канонической личностью источника.
    private static TrainingExample Lesson(string prompt, string response)
        => new(
            prompt,
            response,
            Domain,
            1d,
            Captured,
            TrainingSourceProvenance.CreateSourceId(
                $"fix-n63c/{Domain}/{prompt}",
                response));

    // Допущенная пара вместе со своим номером из записи транзакции.
    private static DpoSupervisedCorrectionCandidatePair Candidate(
        string pairId,
        string prompt,
        string chosen,
        string rejected)
        => new(
            pairId,
            new DpoPair(prompt, chosen, rejected, Domain, Captured));

    // Долговечная транзакция с одной исправляющей парой: ответ модели попадает
    // в отвергнутую сторону, правка пользователя — в выбранную.
    private static (
        FileDpoPairStore.TransactionHandle Transaction,
        IReadOnlyList<DpoPair> Pairs) PrepareDurableCorrectionTransaction(
            TinyTrainingFixtureCL fixture,
            string uniqueStoreSuffix)
    {
        var router = new FeedbackRouter(
            new NullSessionStorageN63C(),
            new AcceptingDpoCacheN63C(),
            Path.Combine(
                fixture.RootDirectory,
                $"durable-dpo-pairs-{uniqueStoreSuffix}"));
        var routed = router.Route(new FeedbackEvent(
            "fix abs",
            "wrong abs",
            "right abs",
            FeedbackSignal.Edited,
            Domain)
        {
            TeacherProvenance = QwenProvenanceN63C(uniqueStoreSuffix),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [$"sha256:{new string('a', 64)}"]),
        });
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var prepared = ((IDpoPairTransactionCoordinator)router)
            .PrepareDpoTransaction([Domain]);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        return (
            transaction,
            snapshot.Value!.Pairs.Select(static value => value.Pair).ToArray());
    }

    // Действующая серверная власть допуска учителя для этих проверок.
    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthorityN63C()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray(),
            StableDpoTrainingExecutor.DefaultDpoStepCount);
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);
        var capability = ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value!,
                static () => true);
        Assert.True(capability.IsAvailable);
        return capability;
    }

    // Паспорт местного открытого учителя: тот же, что в действующих проверках.
    private static TrainingTeacherProvenance QwenProvenanceN63C(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            new string('d', 64),
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = new string('c', 64),
        };

    // Словарь учебной оснастки читается тем же файлом, что отдан тренеру.
    private static BpeTokenizer LoadFixtureTokenizerN63C(
        TinyTrainingFixtureCL fixture)
    {
        using var stream = File.OpenRead(fixture.TokenizerPath);
        return BpeTokenizer.Load(stream);
    }

    // Прежний отпечаток набора считается самим продуктом: так проверка видит
    // ровно то значение, которое уйдёт в запись кандидата.
    private static string BuildV1BatchDigestByProduct(
        IReadOnlyList<TrainingExample> examples)
    {
        var method = typeof(IncrementalTrainer).GetMethod(
            "BuildTrainingExampleBatchSha256",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var result = (OperationResult<string>)method.Invoke(
            null,
            [examples])!;
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    // Отпечаток исправленного набора второй версии считается самим продуктом.
    private static string BuildCorrectedBatchDigestByProduct(
        IReadOnlyList<TrainingExample> examples,
        ProjectedSupervisedBatch correction)
    {
        var method = typeof(IncrementalTrainer).GetMethod(
            "BuildCorrectedTrainingExampleBatchSha256",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var result = (OperationResult<string>)method.Invoke(
            null,
            [examples, correction])!;
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    // Независимый расчёт прежнего отпечаток набора по описанному порядку полей:
    // число уроков, имя первой версии, затем на каждый урок запрос, ответ,
    // домен, личность источника, вес и время снятия с его смещением.
    private static string BuildV1BatchDigestIndependently(
        IReadOnlyList<TrainingExample> examples)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIntN63C(hash, examples.Count);
        AppendTextN63C(hash, "xiigen.dpo-supervised-example-batch.v1");
        foreach (var example in examples)
        {
            AppendTextN63C(hash, example.Prompt);
            AppendTextN63C(hash, example.Response);
            AppendTextN63C(hash, example.Domain);
            AppendTextN63C(hash, example.SourceId);
            AppendLongN63C(hash, BitConverter.DoubleToInt64Bits(example.Weight));
            AppendLongN63C(hash, example.CapturedAt.Ticks);
            AppendLongN63C(hash, example.CapturedAt.Offset.Ticks);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    // Длина строки и её байты: отсутствующая строка помечается длиной минус один.
    private static void AppendTextN63C(IncrementalHash hash, string? value)
    {
        if (value is null)
        {
            AppendIntN63C(hash, -1);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        AppendIntN63C(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    // Целое число в младшем-первым порядке байтов.
    private static void AppendIntN63C(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    // Длинное целое число в младшем-первым порядке байтов.
    private static void AppendLongN63C(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    // Отпечаток одной строки текста в кодировке UTF-8.
    private static string Sha256Hex(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}

// Шпион учебного шага: сохраняет фактические знаки и вес и не вызывает родной
// тренер, поэтому ни одного настоящего обновления весов не происходит.
file sealed class RecordingSupervisedStepExecutorN63C
    : ISupervisedTrainingStepExecutor
{
    public List<IReadOnlyList<int>> CapturedTokenSequences { get; } = [];

    public List<float> CapturedWeights { get; } = [];

    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<Parameter> adapterParameters)
    {
        // Знаки сохраняются копией, чтобы позже их нельзя было подменить.
        CapturedTokenSequences.Add(tokenIds.ToArray());
        // Вес учебной строки сохраняется как есть.
        CapturedWeights.Add(weight);
        // Возвращается конечная потеря: тренер обязан принять её как число.
        return 0.5f;
    }
}

// Исполнитель шага сравнения предпочтений для проверок: он выкладывает
// требуемые файлы и возвращает полную расписку, но весов не меняет. Настоящего
// обучения здесь нет.
file sealed class CompletingDpoExecutorN63C : IDpoTrainingExecutor
{
    public int CallCount { get; private set; }

    public Task<OperationResult<DpoTrainingExecutionArtifact>> ExecuteAsync(
        DpoTrainingExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        var permit = request.LegalTeacherAdmissionLease.AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
            return Task.FromResult(permit.MapError<DpoTrainingExecutionArtifact>());
        DpoTrainingExecutionArtifact artifact;
        using (permit.Value)
        {
            var steps = request.DpoStepCount;
            var policyPath = Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(steps));
            var metricsPath = Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(steps));
            Directory.CreateDirectory(request.CheckpointDirectory);
            File.WriteAllBytes(policyPath, [1]);
            File.WriteAllBytes(metricsPath, [1]);
            artifact = new DpoTrainingExecutionArtifact(
                request.Pairs.Count,
                steps,
                policyPath,
                metricsPath,
                request.LegalTeacherAdmissionLease.Admission.PolicyIdentityRef);
        }

        return Task.FromResult(
            OperationResult<DpoTrainingExecutionArtifact>.Ok(artifact));
    }
}

// Кэш доменного набора-заглушка: обратной связи он нужен как зависимость.
file sealed class AcceptingDpoCacheN63C : IDomainDatasetCache
{
    public OperationResult<bool> Add(TrainingExample example)
        => OperationResult<bool>.Ok(true);

    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok([]);

    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(0);

    public Task<OperationResult<bool>> FlushAsync()
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

// Хранилище сессий-заглушка: тренеру оно нужно только как зависимость.
file sealed class NullSessionStorageN63C : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Состояние сессии не сохраняется в этой проверке.",
            "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}
