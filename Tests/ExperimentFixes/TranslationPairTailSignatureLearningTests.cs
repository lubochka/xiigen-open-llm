// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;
using Xunit.Abstractions;

// Q02 / R09: происхождение PairId отделено от рабочего ключа всех читателей.
// Новый builder пишет проверенное RULE:TENSE=PAST; Canonicalize старого ключа
// сохраняет предъявленное RULE:TENSE_PAST и удаляет только сегменты PAIR.
// Проверки ниже различают форму памяти, достижение кандидата до source-floor
// и обычный ответ самой загруженной машины без повторного обучения.
// Старые source-only карты без темы по-прежнему отвергаются: legacy-контроль
// относится только к форме semantic keys, а TopicSourceMap сохраняет версию 2.
// Сырой parser-guess не становится проверенной аннотацией этой правкой.
public sealed class TranslationPairTailSignatureLearningTests
{
    private readonly ITestOutputHelper _output;

    // Только журнал наблюдений; конструктор ничего не обучает и не загружает.
    public TranslationPairTailSignatureLearningTests(ITestOutputHelper output) => _output = output;

    // Учебная строка в прошедшем времени: та же форма, на которой измерялся сто шестой опыт.
    private const string TrainedSourceText = "Он проверил файл";

    // Номер учебной пары: именно он больше не имеет права попадать в рабочий ключ.
    private const string TrainedPairId = "P106-TAIL-t01";

    // Перечисления в учебных примерах пишутся словами, как их пишет продуктовый конструктор партий.
    private static readonly JsonSerializerOptions JsonOptions =
        new() { Converters = { new JsonStringEnumConverter() } };

    // Готовая к обучению пара для настоящего конструктора партий домена перевода.
    private static TranslationTrainReadyPair TrainedPair() => new(
        Id: TrainedPairId,
        Source: "simulation",
        Direction: "RU_EN",
        PairFamily: "tense",
        Prompt: "Переведи",
        ASide: TrainedSourceText,
        Chosen: "He checked the file",
        Rejected: "He checks the file [WRONG: tense]",
        ValidationOutcome: "train_ready",
        ValidationErrors: [],
        VerifiedRuleTags: ["TENSE_PAST"]);

    // Партия, собранная продуктовым конструктором именно в режиме подписи «пара».
    private static TranslationLearningBatch BuildPairModeBatch()
        => TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "P106-TAIL-main-v1",
            [TrainedPair()],
            signatureMode: TranslationSignatureMode.ExactPair);

    // Урок разборщика смысла той партии: его ключ и есть предмет всей этой работы.
    private static TranslationLearningExample TokenizerLessonOf(TranslationLearningBatch batch)
        => batch.Examples.Single(
            example => example.UnitId == TranslationUnitIds.SemanticTokenizer);

    // ── Группа 1: один канонический ключ у всех читателей ──────────────────────

    // Требование 1 владельца: единый ключ пары обучения используется согласованно всеми пятью
    // читателями. Здесь проверяется самое ядро: ключ, под которым урок ЗАПИСАН, совпадает с
    // ключом, по которому его будут ИСКАТЬ при обычном чтении.
    [Fact]
    public async Task OneCanonicalKey_IsUsedByTheWriterTheMemoryAndTheReaderAlike()
    {
        // Разбор времени исполнения той же строки: из него берутся адреса, по которым чтение
        // действительно ищет урок.
        var runtime = new TranslationSemanticTokenizer().Tokenize(TrainedSourceText, "ru");

        // Ключ, который собрал писатель — конструктор учебных партий.
        var batch = BuildPairModeBatch();
        var lessonKey = TokenizerLessonOf(batch).SemanticSignature;

        // Писатель собрал ключ КАНОНИЧЕСКИМ: канонизация этого ключа ничего в нём не меняет,
        // то есть отрезка происхождения в нём нет ни в каком виде.
        Assert.Equal(TranslationSemanticKey.Canonicalize(lessonKey), lessonKey);
        Assert.DoesNotContain("PAIR:", lessonKey);

        // Обучение настоящей продуктовой машиной настоящей партией.
        var machine = new TranslationMachine();
        var train = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        // Память разборщика смысла хранит РОВНО тот ключ, который собрал писатель, и ни одного
        // ключа с происхождением: адрес записи и адрес писателя — одна и та же величина.
        var patternKeys = export.Value!.SemanticTokenizer.PatternCorrections
            .Select(entry => entry.Signature)
            .ToArray();
        Assert.Contains(lessonKey, patternKeys);
        Assert.DoesNotContain(patternKeys, key => key.Contains("PAIR:", StringComparison.Ordinal));

        // Память ближайших соседей хранит тот же ключ писателя и, кроме него, адреса времени
        // исполнения — префиксный и структурный. Именно по этим двум адресам чтение доходит до
        // урока через ступени каскада, поэтому их наличие и есть доказательство достижимости.
        var knnKeys = export.Value!.KnnStore.Entries
            .Select(entry => entry.Signature)
            .ToArray();
        Assert.Contains(lessonKey, knnKeys);
        Assert.Contains(runtime.PrefixSignature, knnKeys);
        Assert.Contains(runtime.StructureSignature, knnKeys);
        Assert.DoesNotContain(knnKeys, key => key.Contains("PAIR:", StringComparison.Ordinal));
    }

    // Новый builder и старый ключ имеют разные допустимые записи RULE.
    // Удаление PAIR не должно ни терять, ни переименовывать предъявленное правило.
    [Fact]
    public void VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone()
    {
        // Ключ урока, собранный настоящим конструктором партий.
        var lessonKey = TokenizerLessonOf(BuildPairModeBatch()).SemanticSignature;

        // Проверенная грамматическая пометка на месте — в той форме, в которой её пишет продукт.
        Assert.Contains("RULE:TENSE=PAST", lessonKey);
        // Номера учебной пары в рабочем ключе нет.
        Assert.DoesNotContain("PAIR:", lessonKey);

        // Отдельно закреплено, что порядок отрезков значения не имеет: ключ прежней формы, где
        // пометка стоит ПОСЛЕ номера, приводится к канонической форме без потери пометки.
        Assert.Equal(
            "SVO|VALIDATE_CHECK|RULE:TENSE_PAST",
            TranslationSemanticKey.Canonicalize("SVO|VALIDATE_CHECK|PAIR:P-X|RULE:TENSE_PAST"));
        // И ключ прежней формы, где пометка стоит ДО номера, тоже не теряет её.
        Assert.Equal(
            "SVO|VALIDATE_CHECK|RULE:TENSE_PAST",
            TranslationSemanticKey.Canonicalize("SVO|VALIDATE_CHECK|RULE:TENSE_PAST|PAIR:P-X"));
    }

    // Требование 3 владельца: происхождение пары хранится отдельно, а не внутри ключа.
    [Fact]
    public void LessonProvenance_LivesInItsOwnFieldNotInsideTheWorkingKey()
    {
        var lesson = TokenizerLessonOf(BuildPairModeBatch());

        // Происхождение названо прямо и полностью.
        Assert.Equal(TrainedPairId, lesson.PairId);
        // Форма ключа названа номером версии, чтобы загрузчик отличал старую память от новой.
        Assert.Equal(TranslationSemanticKey.SchemaVersion, lesson.KeySchemaVersion);
        // В самом ключе номера пары нет ни в каком виде.
        Assert.DoesNotContain(TrainedPairId, lesson.SemanticSignature);
        Assert.DoesNotContain("PAIR:", lesson.SemanticSignature);
    }

    // ── Группа 2: столкновение ключей и порог уверенности ──────────────────────

    // Требование 4 владельца: при столкновении ключей с несовместимым содержимым — отказ ДО
    // публикации, с человеческой причиной и кодом отказа, и доказанным фактом, что публикации
    // не было.
    //
    // Две строки отличаются ТОЛЬКО номером пары. На прежней логике это два разных ключа, и
    // партия проходила: второй урок просто ложился рядом. После миграции оба ключа становятся
    // одним, а содержимое у них несовместимое — разные образцы предложения. Такое молча
    // затирать последним уроком нельзя.
    [Fact]
    public async Task TwoRowsCollapsingToOneKeyWithIncompatiblePayload_AreRefusedBeforeAnyPublication()
    {
        var machine = new TranslationMachine();
        var versionBefore = machine.CurrentStateVersion;

        // Отпечаток состояния ДО попытки: с ним сравнивается состояние после отказа.
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var fingerprintBefore = CheckpointFingerprint(before.Value!);

        var collidingBatch = new TranslationLearningBatch(
            BatchId: "P106-TAIL-collision-v1",
            DomainTag: TranslationMachine.DomainTag,
            Examples:
            [
                TokenizerRow("P106-TAIL-c01", "SVO|VALIDATE_CHECK|PAIR:P106-TAIL-c01", SentencePattern.SVO),
                TokenizerRow("P106-TAIL-c02", "SVO|VALIDATE_CHECK|PAIR:P106-TAIL-c02", SentencePattern.SOV)
            ],
            SourceIds: ["simulation"]);

        var train = await machine.TrainStructuredBatchAsync(collidingBatch);

        // Отказ, а не исключение: это обычный результат операции.
        Assert.False(train.IsSuccess);
        // Код отказа назван точно, чтобы его можно было отличить от любого другого отказа партии.
        Assert.Equal("TRANSLATION_SEMANTIC_KEY_COLLISION", train.ErrorCode);
        // Причина человеческая: в ней видно, какой ключ столкнулся и какие строки его дали.
        Assert.Contains("SVO|VALIDATE_CHECK", train.ErrorMessage);
        Assert.Contains("P106-TAIL-c01", train.ErrorMessage);
        Assert.Contains("P106-TAIL-c02", train.ErrorMessage);

        // Публикации НЕ БЫЛО: состояние машины целиком то же, что до попытки.
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(fingerprintBefore, CheckpointFingerprint(after.Value!));
        Assert.Equal(versionBefore, machine.CurrentStateVersion);
    }

    // Требование 5 владельца: порог уверенности 0.60 остаётся привязан к обычной паре языков и
    // НЕ превращается в общий запрет.
    //
    // Почему именно 0.60. Это порог доли общих значимых слов между запросом и сохранённым
    // источником для ОБЫЧНОГО направления английский→русский, когда в разборе нет ни одного
    // известного смыслового понятия. В этой ветке у поиска нет никакой другой опоры, кроме
    // совпадения слов: структура «подлежащее-сказуемое-дополнение» сама по себе не говорит, что
    // две фразы об одном и том же. Ниже 0.60 выученный ответ про один предмет начинал отвечать
    // на вопрос про другой предмет. Там же, где в разборе ЕСТЬ известные смысловые понятия,
    // опора есть, и уже разрешённый пол 0.0 сохраняется — этот порог не глобальный.
    //
    // Что проверяется здесь. Чужое происхождение в ключе не имеет права открывать ветку без
    // порога. На прежней логике исполнитель искал подстроку «|PAIR:» в рабочем ключе и уводил
    // такую строку в ветку с полом 0.0, то есть чужой номер пары фактически отменял порог.
    // ПОРЯДОК ПРОВЕРКИ ИСПРАВЛЕН (находка аудита по моей же работе). Прежде эта проверка сразу
    // утверждала «ответа из памяти нет» на входе ниже порога. Такое утверждение проходит и тогда,
    // когда кандидата в памяти НЕ БЫЛО ВОВСЕ: сравнивать не с чем, и зелёный результат ничего не
    // доказывает. Теперь проверка сначала ДОКАЗЫВАЕТ, что кандидат существует и достижим обычным
    // чтением, и только потом показывает, что ниже порога он отвергается.
    [Fact]
    public void ForeignProvenanceInTheKey_DoesNotLiftTheOrdinaryEnRuSourceFloor()
    {
        // Общая память ближайших соседей: ровно один выученный урок.
        const string learnedAnswer = "другой выученный ответ";
        var store = new TranslationKnnStore();
        store.AddPair("SVO|UNKNOWN", "alpha beta omega", learnedAnswer);

        // Исполнитель обычного направления английский→русский.
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);
        var observations = new List<(IReadOnlyList<(string Source, string Target)> Candidates, double Floor)>();
        executor.BeforeKnnSourceFloor = (candidates, floor) => observations.Add((candidates, floor));

        // ШАГ 1 — СУЩЕСТВОВАНИЕ И ДОСТИЖИМОСТЬ. Запрос с полным совпадением значимых слов:
        // доля общих слов равна единице, порог пройден. Если здесь ответа нет, значит кандидата
        // в памяти не было или он недостижим этим путём — и тогда шаг 2 ничего не проверяет.
        var reachable = executor.Execute(
            SequenceWithoutKnownConcepts("alpha beta", "SVO|UNKNOWN|PAIR:P-FOREIGN"),
            "general");
        Assert.Equal(learnedAnswer, reachable.Text);
        Assert.Equal("executor-en_ru-ordinary-source-knn", reachable.AdapterId);
        Assert.Single(observations);
        Assert.Equal(("alpha beta omega", learnedAnswer), Assert.Single(observations[0].Candidates));
        Assert.Equal(0.60, observations[0].Floor);

        // ШАГ 2 — ПОРОГ. Тот же урок, тот же вид разбора, но доля общих значимых слов 0,40:
        // это выше порога допуска самого каскада (0,35) и ниже порога обычного направления
        // (0,60). Значит отвергает именно порог направления, а не пустота набора и не каскад.
        //
        // ГДЕ ИМЕННО ОТВЕРГАЕТ — ИЗМЕНИЛОСЬ ВМЕСТЕ С ПОРЯДКОМ ДОПУСКА. Прежде порог
        // направления стоял ПОСЛЕ каскада, и наблюдение ниже видело запись, которую порог
        // ещё не смотрел. Назначенный остаток перенёс полный допуск, включая этот порог,
        // ВНУТРЬ уровня — до ограничения числа соседей. Поэтому запись отвергается раньше, и
        // наблюдение видит пустой набор. Само ЗНАЧЕНИЕ порога не менялось: оно по-прежнему
        // 0,60 и по-прежнему принадлежит обычному направлению, что и проверяется строкой
        // про floor ниже. Непустота проверки держится ШАГОМ 1: он уже доказал, что запись в
        // памяти есть и достижима обычным чтением, поэтому пустой набор здесь означает
        // «порог отверг единственного кандидата», а не «кандидата не было».
        var belowFloor = executor.Execute(
            SequenceWithoutKnownConcepts("alpha beta gamma delta epsilon", "SVO|UNKNOWN|PAIR:P-FOREIGN"),
            "general");
        Assert.NotEqual(learnedAnswer, belowFloor.Text);
        Assert.Equal("executor-en_ru-fallback", belowFloor.AdapterId);
        Assert.Equal(2, observations.Count);
        Assert.Empty(observations[1].Candidates);
        Assert.Equal(0.60, observations[1].Floor);
        _output.WriteLine(JsonSerializer.Serialize(new
        {
            stage = "candidate_refused_at_admission_by_direction_floor",
            candidate_count = observations[1].Candidates.Count,
            floor = observations[1].Floor, positive = reachable.Text, negative = belowFloor.Text
        }));

        // Чужой номер учебной пары стоял в рабочем ключе на ОБОИХ шагах и ни на одном из них не
        // отменил порог: ветвь выбирается по смысловым понятиям разбора, а не по происхождению.
        // Утверждения об этом здесь нет намеренно — его несут сами два шага выше, а сравнение
        // литерала с литералом не доказывало бы ничего.
    }

    // Требование аудита: объявленная форма ключа обязана соответствовать данным.
    //
    // Прежде поле формы ключа имело значение по умолчанию «каноническая», и строка со СТАРЫМ
    // ключом, несущим отрезок происхождения, молча помечалась канонической. Это ложь о данных:
    // загрузчик считал бы такую строку уже перенесённой. Ни вспомогательный вычислитель, ни само
    // новое поле записи переносом не являются.
    [Fact]
    public async Task RowClaimingCanonicalFormWhileCarryingProvenance_IsRefusedWithBothValuesNamed()
    {
        var machine = new TranslationMachine();

        // Отпечаток состояния ДО попытки: с ним сравнивается состояние после отказа.
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var fingerprintBefore = CheckpointFingerprint(before.Value!);

        // Строка лжёт о себе: ключ несёт отрезок происхождения, а форма объявлена канонической.
        var lyingRow = TokenizerRow("P106-TAIL-lie", "SVO|VALIDATE_CHECK|PAIR:P106-TAIL-lie", SentencePattern.SVO)
            with { KeySchemaVersion = TranslationSemanticKey.SchemaVersion };

        var train = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            BatchId: "P106-TAIL-schema-lie-v1",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [lyingRow],
            SourceIds: ["simulation"]));

        // Отказ, а не исключение и не молчаливый приём.
        Assert.False(train.IsSuccess);
        Assert.Equal("BATCH_ROW_KEY_SCHEMA_VERSION_MISMATCH", train.ErrorCode);
        // Причина называет ОБЕ величины: объявленную форму и выведенную из самого ключа.
        Assert.Contains(TranslationSemanticKey.SchemaVersion.ToString(), train.ErrorMessage);
        Assert.Contains(TranslationSemanticKey.LegacySchemaVersion.ToString(), train.ErrorMessage);

        // Публикации не было: состояние переводчика целиком то же, что до попытки.
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(fingerprintBefore, CheckpointFingerprint(after.Value!));
    }

    // Прежний договор писателей не ломается: строка, которая форму ключа НЕ называет, проходит,
    // и её форма выводится из содержимого ключа.
    [Fact]
    public void RowThatDoesNotStateItsKeyForm_HasTheFormDerivedFromTheKeyItself()
    {
        // Ключ прежней формы: отрезок происхождения внутри него.
        Assert.Equal(
            TranslationSemanticKey.LegacySchemaVersion,
            TranslationSemanticKey.DeriveSchemaVersion("SVO|VALIDATE_CHECK|PAIR:P-X"));

        // Канонический ключ: отрезка происхождения нет.
        Assert.Equal(
            TranslationSemanticKey.SchemaVersion,
            TranslationSemanticKey.DeriveSchemaVersion("SVO|VALIDATE_CHECK|RULE:TENSE_PAST"));

        // Строка настоящего конструктора партий форму называет сама, и она совпадает с выведенной.
        var lesson = TokenizerLessonOf(BuildPairModeBatch());
        Assert.Equal(
            TranslationSemanticKey.DeriveSchemaVersion(lesson.SemanticSignature),
            lesson.KeySchemaVersion);
    }

    // Требование 6 владельца: отрицательный перевод без обязательного термина после миграции
    // по-прежнему отвергается.
    //
    // Проверяющий качества пишет свои требования по учебному ключу, а читает по ключу времени
    // исполнения. На прежней логике ключ записи несёт номер пары, ключ чтения его не несёт,
    // поэтому требование не находилось НИКОГДА и плохой перевод проходил проверку.
    [Fact]
    public void RequiredTermOfTheVerifier_IsFoundByTheRuntimeKeyAndRejectsTheBadTranslation()
    {
        // Канонический ключ: он и есть ключ чтения.
        const string canonicalKey = "SVO|VALIDATE_CHECK|DATA_ENTITY";

        var verifier = new QualityVerifierUnit();
        // Урок прежней формы: требование записано по ключу С номером пары.
        verifier.TrainOnExample(VerifierRow(
            "P106-TAIL-v01",
            canonicalKey + "|PAIR:P106-TAIL-v01",
            requiredTerm: "thread"));

        // Разбор времени исполнения: без списка слов, чтобы сработала ровно одна ветка
        // проверяющего — требование по ключу, а не по слову-признаку источника.
        var runtimeTokens = EmptyTokenSequenceWithKey(canonicalKey);

        // Плохой перевод: обязательного термина в нём нет.
        var bad = verifier.Verify("some unrelated answer", runtimeTokens);
        Assert.False(bad.Pass);
        Assert.Contains(bad.Issues, issue => issue.Contains("thread", StringComparison.Ordinal));

        // Хороший перевод: обязательный термин на месте, претензий нет.
        var good = verifier.Verify("start the thread now", runtimeTokens);
        Assert.True(good.Pass, string.Join("; ", good.Issues));

        // В этом отдельном уроке карта правил действительно непуста. Переносим её
        // искусственную старую форму в нового проверяющего без второго обучения.
        var savedVerifier = verifier.ExportCheckpoint();
        var savedRule = Assert.Single(savedVerifier.RequiredTermsBySignature);
        Assert.Equal(canonicalKey, savedRule.Key);
        Assert.Contains("thread", savedRule.Values);
        var restoredVerifier = new QualityVerifierUnit();
        restoredVerifier.ImportCheckpoint(savedVerifier with
        {
            RequiredTermsBySignature = [savedRule with { Key = canonicalKey + "|PAIR:Q02-legacy-verifier" }]
        });
        var restoredRule = Assert.Single(restoredVerifier.ExportCheckpoint().RequiredTermsBySignature);
        Assert.Equal(canonicalKey, restoredRule.Key);
        Assert.Contains("thread", restoredRule.Values);

        var restoredBad = restoredVerifier.Verify("some unrelated answer", runtimeTokens);
        Assert.False(restoredBad.Pass);
        Assert.Contains(restoredBad.Issues, issue => issue.Contains("thread", StringComparison.Ordinal));
        var restoredGood = restoredVerifier.Verify("start the thread now", runtimeTokens);
        Assert.True(restoredGood.Pass, string.Join("; ", restoredGood.Issues));
        _output.WriteLine(JsonSerializer.Serialize(new
        {
            stage = "nonempty_legacy_verifier_import", restored_key = restoredRule.Key,
            required = restoredRule.Values, writer_bad_pass = bad.Pass, writer_good_pass = good.Pass,
            restored_bad_pass = restoredBad.Pass, restored_good_pass = restoredGood.Pass
        }));
    }

    // ── Общие сборщики учебных строк и разборов ────────────────────────────────

    // TokenizerRow собирает одну учебную строку разборщика смысла с заданным ключом прежней
    // формы: номер пары стоит внутри ключа, поэтому форма ключа названа прежней версией.
    private static TranslationLearningExample TokenizerRow(
        string pairId,
        string legacyKey,
        SentencePattern chosenPattern)
        => new(
            ExampleId: pairId + ":semantic",
            UnitId: TranslationUnitIds.SemanticTokenizer,
            FlowId: "ru-en-general",
            InputJson: JsonSerializer.Serialize(
                new TokenizerInput("Он проверил " + pairId, "ru"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(
                new TokenizerOutput(chosenPattern, legacyKey), JsonOptions),
            RejectedOutputJson: JsonSerializer.Serialize(
                new TokenizerOutput(SentencePattern.UNKNOWN, "WRONG|" + legacyKey), JsonOptions),
            RejectedType: "wrong_sentence_pattern",
            SemanticSignature: legacyKey,
            SourceId: "simulation",
            HeldOut: false,
            PairId: pairId,
            KeySchemaVersion: TranslationSemanticKey.LegacySchemaVersion);

    // VerifierRow собирает одну учебную строку проверяющего качества, которая действительно
    // пишет требование по ключу: в причине нет слов «translated quality marker», а ключ несёт
    // известное смысловое понятие — оба условия продуктового обучателя выполнены.
    private static TranslationLearningExample VerifierRow(
        string pairId,
        string legacyKey,
        string requiredTerm)
        => new(
            ExampleId: pairId + ":verifier",
            UnitId: TranslationUnitIds.QualityVerifier,
            FlowId: "ru-en-general",
            InputJson: JsonSerializer.Serialize(
                new VerifierInput("Он проверил файл", "some unrelated answer"), JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(
                new VerifierOutput(
                    0.42,
                    [$"expected '{requiredTerm}' for signature {legacyKey}"],
                    false,
                    "start the thread now"),
                JsonOptions),
            RejectedOutputJson: JsonSerializer.Serialize(
                new VerifierOutput(0.91, [], true, null), JsonOptions),
            RejectedType: "missing_required_term",
            SemanticSignature: legacyKey,
            SourceId: "simulation",
            HeldOut: false,
            PairId: pairId,
            KeySchemaVersion: TranslationSemanticKey.LegacySchemaVersion);

    // SequenceWithoutKnownConcepts строит разбор, в котором НЕТ ни одного известного смыслового
    // понятия: именно такой разбор и должен попадать под порог обычного направления.
    private static SemanticTokenSequence SequenceWithoutKnownConcepts(
        string sourceText,
        string signature)
    {
        // Слова источника становятся токенами без известного понятия и без особой роли.
        var tokens = sourceText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => new SemanticToken(
                Word: word,
                Lemma: word,
                Pos: TranslationPos.NOUN,
                Role: "DIRECT_OBJ",
                Concept: SemanticConcept.UNKNOWN))
            .ToArray();

        return new SemanticTokenSequence(
            Pattern: SentencePattern.SVO,
            Tokens: tokens,
            PosBigrams: [],
            Signature: signature,
            // Префиксный и структурный адреса намеренно уникальны: ответ обязан прийти или не
            // прийти по ПОЛНОМУ адресу, а не через более широкую ступень каскада.
            PrefixSignature: "SVO|PREFIX-NOT-STORED",
            StructureSignature: "SVO|STRUCTURE-NOT-STORED",
            RuleTags: []);
    }

    // EmptyTokenSequenceWithKey строит разбор с заданным ключом и БЕЗ слов: так у проверяющего
    // качества молчит ветка слова-признака источника и работает ровно ветка ключа.
    private static SemanticTokenSequence EmptyTokenSequenceWithKey(string signature)
        => new(
            Pattern: SentencePattern.SVO,
            Tokens: [],
            PosBigrams: [],
            Signature: signature,
            PrefixSignature: signature,
            StructureSignature: signature,
            RuleTags: []);

    // ── Группа 3: восстановленный потребитель отвечает сам ─────────────────────
    //
    // ЧТО ИСПРАВЛЕНО В ДОГОВОРЕ ПРИЁМКИ 2 ноября 2026 (ремонт «тест восстановления читает
    // третий, заново обученный объект»; человеческое имя пункта — «исправить доказательство
    // восстановления», пункт M03c адресного плана).
    //
    // Чем прежний договор был негоден. Прежняя проверка импортировала состояние в целевую
    // машину, читала обратно её повторный экспорт — а затем создавала ТРЕТИЙ разборщик,
    // обучала его заново тем же уроком и проверяла ответ ЭТОГО НОВОГО объекта. Таким
    // способом нельзя доказать, что загруженная машина применяет восстановленное
    // состояние: заново обученный объект ответил бы точно так же и при полностью
    // сломанной загрузке. Третий объект доказывал обучение, а не восстановление.
    //
    // Что стало договором. Настоящая цепочка: обучить источник, сохранить, создать НОВУЮ
    // целевую машину, загрузить в неё состояние, взять оркестратор ИМЕННО У НЕЁ и задать
    // запрос обычной продуктовой дорогой. В этом плече чтения ноль обучений и ноль
    // обновлений: тождество состояния до и после чтения сравнивается целиком. Отвечает
    // загруженная машина, а не свежеобученный двойник.
    //
    // Различающий пример. Сам текст примера сам по себе ничего не доказывает, поэтому
    // рядом стоит свежая необученная машина на том же запросе: она обязана ответить
    // иначе. Без этого сравнения проверка прошла бы и на правилах, без всякой загрузки.
    //
    // Свежий отрицательный контроль CorruptingRestoredAnswerMap_BreaksLearnedAnswer
    // меняет одну использованную ячейку карты и повторяет обычный запрос той же машине.
    // Исторический отдельный broken-reader здесь не считается новым выполнением.
    //
    // ЧТО ИЗМЕНИЛОСЬ 2 ноября 2026 в этой группе. Снято единственное требование —
    // «сохранённая подпись обязана нести номер учебной пары». Оно закрепляло тот самый
    // дефект, который чинит миграция. Остальная часть группы не менялась: обе проверки
    // обязаны остаться зелёными и после миграции, потому что ответ здесь приходит ранним
    // точным путём по адресу «тема плюс источник», а этот адрес миграция не затрагивает.

    // Ответ, которому обучен источник: он и обязан прийти от загруженной машины.
    private const string TrainedTargetText = "He checked the file";

    // Имя ветви каскада, которая обязана ответить на этот пример у загруженной машины.
    // Значение не угадано: оно снято с фактического прогона и закреплено, чтобы
    // отключение именно этой восстановленной карты роняло поведенческую проверку.
    private const string ExpectedAnsweringAdapterId = "executor-ru_en-exact-source";

    // Запрос обычной продуктовой дорогой: те же поля, которыми пользуется сам домен.
    private static LogicalOrchestratorRequest TranslationRequest(string userRequest) => new(
        RunId: "P106-TAIL-restore-" + Guid.NewGuid().ToString("N"),
        ScenarioId: TranslationRoutingFlow.ScenarioId,
        UserRequest: userRequest,
        Flags: new LogicalExecutionFlags(),
        Metadata: new Dictionary<string, object>
        {
            ["source_language"] = "ru",
            ["target_language"] = "en",
            ["topic_domain"] = "general"
        });

    // Отпечаток всего сохранённого состояния: нужен, чтобы доказать ноль обучений и
    // обновлений в плече чтения. Сравнивается состояние целиком, а не одна запись.
    //
    // Время записи файла из отпечатка ИСКЛЮЧЕНО намеренно. Поле `ExportedAtUtc` — это
    // по его собственному описанию «стенные часы в момент записи файла», то есть не
    // обученное состояние: два экспорта одного и того же состояния отличаются в нём
    // всегда. Это измерено на собственной ошибке: первый вариант отпечатка сравнивал
    // часы и падал на разнице в миллисекундах, ничего не говоря о состоянии. Прибор
    // исправлен, ожидание не ослаблено: всё остальное состояние сравнивается точно.
    private static string CheckpointFingerprint(TranslationMachineCheckpoint checkpoint)
        => JsonSerializer.Serialize(
            checkpoint with { ExportedAtUtc = DateTimeOffset.UnixEpoch },
            JsonOptions);

    // Задаёт запрос машине её собственным оркестратором и возвращает результат.
    private static async Task<LogicalOrchestratorResult> AskThroughOrdinaryPath(
        TranslationMachine machine,
        string userRequest)
    {
        // Оркестратор берётся ИМЕННО У ЭТОЙ машины: это и есть проверяемая связь.
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        // Обычный запрос обычной дорогой домена.
        var answer = await orchestrator.Value!.RunAsync(TranslationRequest(userRequest));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);

        return answer.Value!;
    }

    // ПРОВЕРКА СХЕМЫ, отделённая от проверки поведения намеренно (находка аудита по моей же
    // работе). Прежде это утверждение стояло внутри поведенческой проверки и останавливало её
    // раньше, чем дело доходило до восстановленного читателя. Теперь падение схемы не скрывает
    // поведение, а падение поведения не скрывает схему: это две независимые проверки.
    [Fact]
    public async Task ImportedCheckpoint_StoresTheLessonUnderACanonicalKeyWithoutProvenance()
    {
        var source = new TranslationMachine();
        var train = await source.TrainStructuredBatchAsync(BuildPairModeBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var export = source.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        var target = new TranslationMachine();
        var import = target.ImportCheckpoint(export.Value!);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        var reloaded = target.ExportCheckpoint();
        Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);

        // Запись урока ищется по прежнему адресу — учебному тексту со сжатыми пробелами.
        var storedAfterImport = reloaded.Value!.SemanticTokenizer.SignatureCorrections
            .Single(entry => string.Equals(
                entry.NormalizedRawText, TrainedSourceText, StringComparison.OrdinalIgnoreCase));

        // Ключ записи прежний: адрес урока миграция не меняет.
        Assert.Equal(TrainedSourceText, storedAfterImport.NormalizedRawText);
        // Сохранённая подпись урока не несёт происхождения: оно живёт отдельным полем.
        Assert.DoesNotContain("PAIR:", storedAfterImport.TokenizerSignature);
        // Законная грамматическая пометка писателя при этом сохранена — она не происхождение.
        Assert.Contains("RULE:TENSE=PAST", storedAfterImport.TokenizerSignature);
    }

    [Fact]
    public async Task ImportedCheckpointReExportsExactlyWhatWasSaved()
    {
        // Сторона ХРАНЕНИЯ, отделённая от стороны ПОВЕДЕНИЯ намеренно. Эта проверка
        // говорит только одно: загрузка не теряет и не меняет сохранённые байты
        // состояния. Она обязана оставаться зелёной и тогда, когда применение
        // загруженной карты отключено, — именно этим она доказывает, что соседняя
        // поведенческая проверка смотрит на поведение, а не на хранение.
        var source = new TranslationMachine();
        var train = await source.TrainStructuredBatchAsync(BuildPairModeBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var export = source.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        var target = new TranslationMachine();
        var import = target.ImportCheckpoint(export.Value!);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        var reloaded = target.ExportCheckpoint();
        Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);

        // Отпечаток повторного экспорта совпадает с отпечатком сохранённого состояния.
        Assert.Equal(
            CheckpointFingerprint(export.Value!),
            CheckpointFingerprint(reloaded.Value!));
    }

    [Fact]
    public async Task SavedLesson_SurvivesCheckpointRoundTrip_AndIsAnsweredByTheLoadedMachineItself()
    {
        // Источник обучается продуктовой машиной и сохраняется.
        var source = new TranslationMachine();
        var train = await source.TrainStructuredBatchAsync(BuildPairModeBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);

        var export = source.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        // НОВАЯ целевая машина: она ничему не обучалась и всё знает только из загрузки.
        var target = new TranslationMachine();
        var import = target.ImportCheckpoint(export.Value!);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        // Сторона хранения здесь НЕ утверждается. Проверка схемы живёт отдельным методом
        // ниже, и вот почему: утверждение о форме сохранённой подписи, поставленное здесь,
        // остановило бы этот метод ДО обращения к восстановленному читателю, и поведение
        // загруженного объекта осталось бы непроверенным. Эту ошибку нашёл аудит у меня же.
        // Прежняя неделимая проверка не доказывает новую: схема и поведение — две независимые
        // проверки, и падение одной не имеет права скрывать другую.
        var reloaded = target.ExportCheckpoint();
        Assert.True(reloaded.IsSuccess, reloaded.ErrorMessage);

        // Отпечаток состояния ДО чтения: с ним сравнивается состояние после ответа.
        var fingerprintBeforeRead = CheckpointFingerprint(reloaded.Value!);

        // ГЛАВНОЕ: отвечает сама загруженная машина, своим оркестратором, без обучения.
        var answer = await AskThroughOrdinaryPath(target, TrainedSourceText);

        // Ответ пришёл из восстановленного урока, а не из правил.
        Assert.Equal(TrainedTargetText, answer.FinalHumanResult);
        // Итог обычного пути — переведено, а не низкая уверенность.
        Assert.Equal("TRANSLATED", answer.OverallVerdict);
        // Ответ выдала ИМЕННО та восстановленная карта, на которую рассчитан этот
        // пример, а не какая-то другая ветвь каскада с тем же текстом. Точное имя
        // ветви здесь обязательно: без него отрицательный контроль не кусает —
        // это измерено на собственной ошибке, когда проверка с общим началом имени
        // прошла и при отключённой карте, потому что ответ пришёл из другой ветви.
        Assert.Equal(ExpectedAnsweringAdapterId, answer.FlowResult.Metrics["adapter_id"]);
        // Уверенность — настоящее конечное число, а не пропуск поля.
        Assert.True(
            double.TryParse(
                answer.FlowResult.Metrics["confidence"],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var confidence) && double.IsFinite(confidence),
            "уверенность обычного пути не является конечным числом");

        // РАЗЛИЧАЮЩИЙ ПРИМЕР: свежая необученная машина на том же запросе обязана
        // ответить иначе. Иначе проверка прошла бы и на правилах, без всякой загрузки.
        var fresh = await AskThroughOrdinaryPath(new TranslationMachine(), TrainedSourceText);
        Assert.NotEqual(TrainedTargetText, fresh.FinalHumanResult);

        // НОЛЬ обучений и обновлений в плече чтения: состояние целиком то же самое.
        var afterRead = target.ExportCheckpoint();
        Assert.True(afterRead.IsSuccess, afterRead.ErrorMessage);
        Assert.Equal(fingerprintBeforeRead, CheckpointFingerprint(afterRead.Value!));
    }

    [Fact]
    public async Task UnknownKeySchema_IsRejectedBeforeAnyPublication()
    {
        var machine = new TranslationMachine();
        var before = machine.ExportCheckpoint();
        Assert.True(before.IsSuccess, before.ErrorMessage);
        var versionBefore = machine.CurrentStateVersion;
        var row = TokenizerRow("Q02-unknown", "SVO|VALIDATE_CHECK", SentencePattern.SVO)
            with { KeySchemaVersion = 999 };

        var result = await machine.TrainStructuredBatchAsync(new TranslationLearningBatch(
            "Q02-unknown-schema", TranslationMachine.DomainTag, [row], ["simulation"]));

        Assert.False(result.IsSuccess);
        Assert.Equal("BATCH_ROW_KEY_SCHEMA_VERSION_MISMATCH", result.ErrorCode);
        Assert.Contains("999", result.ErrorMessage);
        Assert.Contains("2", result.ErrorMessage);
        var after = machine.ExportCheckpoint();
        Assert.True(after.IsSuccess, after.ErrorMessage);
        Assert.Equal(CheckpointFingerprint(before.Value!), CheckpointFingerprint(after.Value!));
        Assert.Equal(versionBefore, machine.CurrentStateVersion);
    }

    [Fact]
    public async Task LegacyCheckpoint_WithPairSegments_LoadsAndAnswersThroughTarget()
    {
        var source = new TranslationMachine();
        var train = await source.TrainStructuredBatchAsync(BuildPairModeBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var exported = source.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var saved = exported.Value!;

        // Искусственная старая форма ТОЛЬКО semantic keys, не исторический артефакт.
        // Тема точной памяти не теряется: TopicSourceMap и её версия 2 остаются прежними.
        static string OldKey(string key) => key + "|PAIR:Q02-legacy";
        var legacy = saved with
        {
            KnnStore = saved.KnnStore with
            {
                Entries = saved.KnnStore.Entries.Select(entry => entry with
                    { Signature = OldKey(entry.Signature) }).ToArray()
            },
            SemanticTokenizer = saved.SemanticTokenizer with
            {
                PatternCorrections = saved.SemanticTokenizer.PatternCorrections.Select(entry => entry with
                    { Signature = OldKey(entry.Signature) }).ToArray(),
                SignatureCorrections = saved.SemanticTokenizer.SignatureCorrections.Select(entry => entry with
                    { TokenizerSignature = OldKey(entry.TokenizerSignature) }).ToArray()
            }
        };
        Assert.NotEmpty(legacy.KnnStore.Entries);
        Assert.NotEmpty(legacy.SemanticTokenizer.PatternCorrections);
        Assert.NotEmpty(legacy.SemanticTokenizer.SignatureCorrections);
        // General-builder marker не создаёт signature-rule: его legacy import
        // проверяет непустой отдельный RequiredTermOfTheVerifier выше.
        Assert.Empty(legacy.QualityVerifier.RequiredTermsBySignature);
        Assert.All(legacy.KnnStore.Entries, entry => Assert.Contains("PAIR:Q02-legacy", entry.Signature));
        Assert.All(legacy.SemanticTokenizer.PatternCorrections,
            entry => Assert.Contains("PAIR:Q02-legacy", entry.Signature));
        Assert.All(legacy.SemanticTokenizer.SignatureCorrections,
            entry => Assert.Contains("PAIR:Q02-legacy", entry.TokenizerSignature));
        var target = new TranslationMachine();
        var imported = target.ImportCheckpoint(legacy);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        var restored = target.ExportCheckpoint();
        Assert.True(restored.IsSuccess, restored.ErrorMessage);
        Assert.Equal(CheckpointFingerprint(saved), CheckpointFingerprint(restored.Value!));

        var versionBeforeRead = target.CurrentStateVersion;
        var answer = await AskThroughOrdinaryPath(target, TrainedSourceText);
        Assert.Equal(TrainedTargetText, answer.FinalHumanResult);
        Assert.Equal("TRANSLATED", answer.OverallVerdict);
        Assert.Equal(ExpectedAnsweringAdapterId, answer.FlowResult.Metrics["adapter_id"]);
        Assert.Equal(versionBeforeRead, target.CurrentStateVersion);
        Assert.Equal(CheckpointFingerprint(restored.Value!), CheckpointFingerprint(target.ExportCheckpoint().Value!));
        _output.WriteLine(JsonSerializer.Serialize(new
        {
            stage = "synthetic_legacy_semantic_keys_import", target_answer = answer.FinalHumanResult,
            adapter = answer.FlowResult.Metrics["adapter_id"], target_training_calls = "absent_in_test_source",
            knn_entries = legacy.KnnStore.Entries.Count
        }));
    }

    [Fact]
    public void KnownSemanticCandidate_RetainsLawfulZeroSourceFloor()
    {
        const string learnedAnswer = "другой выученный ответ";
        var store = new TranslationKnnStore();
        store.AddPair("SVO|UNKNOWN", "alpha beta omega", learnedAnswer);
        var executor = new ExecutorUnit(TranslationDirection.EN_RU, store);
        var observations = new List<(IReadOnlyList<(string Source, string Target)> Candidates, double Floor)>();
        executor.BeforeKnnSourceFloor = (candidates, floor) => observations.Add((candidates, floor));
        var tokens = SequenceWithoutKnownConcepts(
            "alpha beta gamma delta epsilon", "SVO|UNKNOWN|PAIR:Q02-semantic");
        // Доля общих слов 2/5=0,40 проходит каскад 0,35, но не ordinary floor 0,60.
        // Одно известное понятие выбирает законную смысловую ветвь с floor 0.
        tokens = tokens with { Tokens = tokens.Tokens.Select((token, index) => index == 0
            ? token with { Concept = SemanticConcept.DATA_ENTITY } : token).ToArray() };

        var answer = executor.Execute(tokens, "general");

        var observation = Assert.Single(observations);
        Assert.Equal(("alpha beta omega", learnedAnswer), Assert.Single(observation.Candidates));
        Assert.Equal(0.0, observation.Floor);
        Assert.Equal(learnedAnswer, answer.Text);
        Assert.Equal("executor-en_ru-knn", answer.AdapterId);
        _output.WriteLine(JsonSerializer.Serialize(new
        {
            stage = "known_semantic_candidate_before_floor", candidate_count = observation.Candidates.Count,
            source = observation.Candidates[0].Source, target = observation.Candidates[0].Target,
            floor = observation.Floor, answer = answer.Text
        }));
    }

    [Fact]
    public async Task CorruptingRestoredAnswerMap_BreaksLearnedAnswer()
    {
        var source = new TranslationMachine();
        var train = await source.TrainStructuredBatchAsync(BuildPairModeBatch());
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var exported = source.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var saved = exported.Value!;
        var target = new TranslationMachine();
        var imported = target.ImportCheckpoint(saved);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);

        var before = await AskThroughOrdinaryPath(target, TrainedSourceText);
        Assert.Equal(TrainedTargetText, before.FinalHumanResult);
        Assert.Equal("TRANSLATED", before.OverallVerdict);
        Assert.Equal(ExpectedAnsweringAdapterId, before.FlowResult.Metrics["adapter_id"]);

        // Меняется ровно Target использованной ячейки, не gold, схема, topic или source.
        // Повторный Import публикует эту карту в ТОЙ ЖЕ целевой машине без Train.
        const string corruptAnswer = "She checked the file";
        var executor = saved.Executors.Single(entry => entry.Direction == TranslationDirection.RU_EN);
        var cell = Assert.Single(executor.TopicSourceMap!);
        Assert.Equal(TrainedTargetText, cell.Target);
        var corruptExecutor = executor with { TopicSourceMap = [cell with { Target = corruptAnswer }] };
        var corrupt = saved with { Executors = saved.Executors.Select(entry =>
            entry.Direction == TranslationDirection.RU_EN ? corruptExecutor : entry).ToArray() };
        Assert.Equal(CheckpointFingerprint(saved), CheckpointFingerprint(corrupt with
            { Executors = corrupt.Executors.Select(entry => entry.Direction == TranslationDirection.RU_EN
                ? executor : entry).ToArray() }));
        var corruptedImport = target.ImportCheckpoint(corrupt);
        Assert.True(corruptedImport.IsSuccess, corruptedImport.ErrorMessage);
        var versionBeforeRead = target.CurrentStateVersion;

        var after = await AskThroughOrdinaryPath(target, TrainedSourceText);
        Assert.Equal("TRANSLATED", after.OverallVerdict);
        Assert.Equal(ExpectedAnsweringAdapterId, after.FlowResult.Metrics["adapter_id"]);
        Assert.Equal(corruptAnswer, after.FinalHumanResult);
        Assert.NotEqual(TrainedTargetText, after.FinalHumanResult);
        Assert.Equal(versionBeforeRead, target.CurrentStateVersion);
        Assert.Equal(CheckpointFingerprint(corrupt), CheckpointFingerprint(target.ExportCheckpoint().Value!));
        Assert.Equal(CheckpointFingerprint(saved), CheckpointFingerprint(source.ExportCheckpoint().Value!));
        _output.WriteLine(JsonSerializer.Serialize(new
        {
            stage = "restored_topic_source_map_consumer", before = before.FinalHumanResult,
            after = after.FinalHumanResult, gold = TrainedTargetText,
            before_adapter = before.FlowResult.Metrics["adapter_id"],
            after_adapter = after.FlowResult.Metrics["adapter_id"], changed_field = "RU_EN.TopicSourceMap[0].Target"
        }));
    }
}
