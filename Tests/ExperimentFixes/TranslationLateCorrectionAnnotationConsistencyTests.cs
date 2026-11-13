// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие остатка переноса «правильная пометка берётся из
// проверенной стороны» (назначение FIX-P52-TAGS раздела 4 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 105-late-learning-transfer-plan-2026-10-30.md, требование D3 дополнения от
// 1 ноября; основание — опыт о времени глагола P52-PRIMARY-01, подслучай P52.1).
//
// Что здесь проверяется по-человечески. У переводчика есть поздний урок вида
// «вот этот исходный текст разбирается вот так». Когда такой урок срабатывает,
// у записи памяти менялись образец предложения и подпись, а учебные пометки
// оставались от прежнего, ошибочного вида. То есть исправленный ответ носил
// пометки своего прошлого. Дальше по этим пометкам собираются ключи поиска и
// пишется слепок состояния, поэтому расхождение расползалось:
//
//   * нормализованные пометки — оставались пометками сырого разбора: для
//     предложения в настоящем времени там стояло прошедшее;
//   * полный ключ — нёс сразу и ложное прошедшее из сырого разбора, и
//     проверенное настоящее из учебной строки, то есть противоречил сам себе;
//   * префиксный и структурный ключи — собирались ДРУГОЙ формулой, вообще без
//     пометок, хотя сырой разбор кладёт пометки в оба этих ключа. Поэтому
//     адрес, который предъявляет чтение, не совпадал с адресом, под который
//     обучение положило перевод;
//   * слепок состояния — пометок и версии записи не хранил вовсе, поэтому
//     свежая загрузка не могла восстановить ни правильные пометки, ни согласие
//     ключей.
//
// Что стало. У поздней поправки есть собственный согласованный набор пометок:
// проверенное время заменяет догадку сырого разбора, зависимые от прошедшего
// времени пометки пола и согласования при настоящем времени убираются. Все
// ключи собираются одной общей формулой из образца, лексем и ровно этого
// набора. Слепок хранит набор и номер версии записи, поэтому восстановленная
// единица отвечает тем же.
//
// Чего этот остаток НЕ доказывает (границы названы прямо):
//   * базовая догадка сырого разборщика о времени и частях речи не исправлена;
//     она чинится отдельной строкой работ. Для необученного текста прежняя
//     неточность остаётся видимой — это закреплено отдельной проверкой
//     соседнего набора `TranslationVerifiedRuleTagTests`;
//   * текст перевода этим остатком не улучшается: на обученных строках первым
//     отвечает точная память перевода, и 2 из 2 назначенных текстов верны и до,
//     и после правки. Здесь доказывается логическая целость сохранения и
//     восстановления, а не прирост качества перевода;
//   * обобщения на невиданные предложения здесь нет: читается уже известный
//     урок.
//
// Чувствительность к мутации. Каждая проверка ниже падает на неизменённом
// продуктовом коде по существу: пометки исправленной записи не соответствуют
// исправленному ответу, либо ключ собран смесью нового образца и старых
// пометок, либо слепок старое и хранит. Отдельная контрольная проверка
// переставляет проверенные пометки в учебной строке и требует, чтобы набор
// следовал ЗАПИСАННОМУ уроку, а не тексту: так видно, что проверка формы не
// берёт эталон из самого предложения.

namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using TranslationDomain;
using Xunit;

// Набор проверок согласованности учебных пометок после позднего исправления.
public sealed class TranslationLateCorrectionAnnotationConsistencyTests
{
    // Назначение и попытка — ими же назван каталог улик этой работы.
    private const string FixId = "FIX-P52-TAGS";
    private const string AttemptId = "attempt-0001";

    // Направление переноса: с русского на английский, как в опыте о времени глагола.
    private const string SourceLanguage = "ru";
    private const string TargetLanguage = "en";

    // Исходные строки двух назначенных планом учебных пар.
    private const string PastSource = "Он проверил файл";
    private const string PresentSource = "Он проверяет файл";

    // Пометка прошедшего времени в написании сырого разбора.
    private const string RawPastTense = "RU:TENSE:PAST";

    // Пометка настоящего времени в том же написании.
    private const string RawPresentTense = "RU:TENSE:PRESENT";

    // Начало семейства времени в написании сырого разбора.
    private const string RawTenseFamily = "RU:TENSE:";

    // Настройки файлов улик: читаемый отступ и русский текст как есть.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Строка панели — исходный текст и независимый эталон. Эталон читается
    // ТОЛЬКО утверждением и только после получения фактического ответа.
    private sealed record PanelRow(string Code, string Source, string? Gold);

    // Панель четырёх чтений раздела 4.3 плана с назначенными там же эталонами.
    // У двух последних строк пунктуация отличается от учебной: ключ точной
    // памяти перевода убирает знаки препинания, поэтому текст у них тот же.
    // Про их АННОТАЦИЮ план ничего не обещает, и здесь она не проверяется —
    // проверяется только удержание текста.
    private static readonly PanelRow[] Panel =
    [
        new("e1", PastSource, "He checked the file"),
        new("e2", PresentSource, "He checks the file"),
        new("e3", PastSource + ".", "He checked the file"),
        new("e4", PresentSource + ".", "He checks the file")
    ];

    // Строка t1 раздела 4.3: источник в прошедшем времени, отвергнутый вариант
    // намеренно заявляет настоящее время.
    private static TranslationTrainReadyPair PastRow(IReadOnlyList<string>? verifiedRuleTags = null)
    {
        return new TranslationTrainReadyPair(
            Id: "P52-TAGS-t1",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: PastSource,
            Chosen: "He checked the file",
            Rejected: "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: verifiedRuleTags ?? ["TENSE_PAST"]);
    }

    // Строка t2 того же раздела: источник в настоящем времени, отвергнутый
    // вариант заявляет прошедшее.
    private static TranslationTrainReadyPair PresentRow(IReadOnlyList<string>? verifiedRuleTags = null)
    {
        return new TranslationTrainReadyPair(
            Id: "P52-TAGS-t2",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: PresentSource,
            Chosen: "He checks the file",
            Rejected: "He checked the file [WRONG: tense] [RULE: TENSE_PAST]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: verifiedRuleTags ?? ["TENSE_PRESENT"]);
    }

    // BuildBatch строит учебную партию обычным продуктовым конструктором.
    private static TranslationLearningBatch BuildBatch(params TranslationTrainReadyPair[] pairs)
    {
        return TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "P52-TAGS-main-v1",
            pairs,
            signatureMode: TranslationSignatureMode.Structural);
    }

    // TrainSemanticUnit скармливает уроки разборщика той же обученной единице,
    // которую использует продуктовая машина. Обучения исполнителя и
    // проверяющего здесь нет: наблюдается ровно разборщик.
    private static SemanticTokenizerUnit TrainSemanticUnit(TranslationLearningBatch batch)
    {
        var unit = new SemanticTokenizerUnit();
        foreach (var example in batch.Examples
            .Where(example => example.UnitId == TranslationUnitIds.SemanticTokenizer))
            unit.TrainOnExample(example);

        return unit;
    }

    // ── Проверка 1: пометки исправленной записи говорят о проверенном времени ─
    [Fact]
    public void TagsOfTheLateCorrection_StateTheVerifiedTense_NotTheOldGuess()
    {
        var unit = TrainSemanticUnit(BuildBatch(PastRow(), PresentRow()));

        // Предложение в настоящем времени: проверенная учебная сторона сказала
        // «настоящее». Именно это обязано стоять в пометках исправленной записи.
        var present = unit.Tokenize(PresentSource, SourceLanguage);
        Assert.Contains(RawPresentTense, present.RuleTags);
        Assert.DoesNotContain(RawPastTense, present.RuleTags);

        // Пометки, которые имеют смысл только при прошедшем времени — пол глагола
        // и согласование подлежащего с прошедшей формой, — при настоящем времени
        // остаться не могут: они противоречили бы собственному времени записи.
        Assert.DoesNotContain(
            present.RuleTags,
            tag => tag.Contains("VERB_GENDER", StringComparison.Ordinal) ||
                   tag.Contains("_PAST_", StringComparison.Ordinal));

        // Предложение в прошедшем времени не испорчено: его согласованный набор
        // прошедшего времени сохраняется целиком.
        var past = unit.Tokenize(PastSource, SourceLanguage);
        Assert.Contains(RawPastTense, past.RuleTags);
        Assert.DoesNotContain(RawPresentTense, past.RuleTags);
    }

    // ── Проверка 2: все четыре вещи описывают один набор пометок ──────────────
    [Fact]
    public void AllFourThingsOfTheLateCorrection_DescribeOneTagSet()
    {
        var unit = TrainSemanticUnit(BuildBatch(PastRow(), PresentRow()));

        foreach (var source in new[] { PastSource, PresentSource })
        {
            var sequence = unit.Tokenize(source, SourceLanguage);

            // Каждая возвращённая пометка обязана стоять и в полном, и в
            // структурном ключе: иначе ключ описывает не тот ответ, что пометки.
            foreach (var tag in sequence.RuleTags)
            {
                Assert.Contains(tag, sequence.Signature);
                Assert.Contains(tag, sequence.StructureSignature);
            }

            // Префиксный ключ по формуле сырого разбора несёт первые три пометки.
            foreach (var tag in sequence.RuleTags.Take(3))
                Assert.Contains(tag, sequence.PrefixSignature);

            // Противоположное время не имеет права остаться ни в одном ключе:
            // именно такая смесь и была дефектом.
            var tense = sequence.RuleTags.First(tag =>
                tag.StartsWith(RawTenseFamily, StringComparison.Ordinal));
            var oppositeTense = tense == RawPastTense ? RawPresentTense : RawPastTense;
            Assert.DoesNotContain(oppositeTense, sequence.Signature);
            Assert.DoesNotContain(oppositeTense, sequence.PrefixSignature);
            Assert.DoesNotContain(oppositeTense, sequence.StructureSignature);
        }
    }

    // ── Проверка 3: предъявленный адрес совпадает с адресом записи ────────────
    [Fact]
    public void PrefixAndStructureKeys_OfTheLateCorrection_MatchTheAddressTrainingWrites()
    {
        // Обучение исполнителя кладёт выученный перевод под префиксный и
        // структурный адреса СЫРОГО разбора
        // (27.TranslationDomain/TranslationTrainedUnits.cs, ExecutorUnit.TrainOnExample,
        // две записи памяти ближайших соседей). Чтение предъявляет адреса
        // обученного разборщика. Для строки, чьё проверенное время совпадает с
        // догадкой сырого разбора, эти адреса обязаны совпасть буквально,
        // иначе выученный перевод по двум из трёх ярусов не находится никогда.
        var addressTrainingWrites = new TranslationSemanticTokenizer()
            .Tokenize(PastSource, SourceLanguage);

        var unit = TrainSemanticUnit(BuildBatch(PastRow(), PresentRow()));
        var addressReadingPresents = unit.Tokenize(PastSource, SourceLanguage);

        Assert.Equal(addressTrainingWrites.PrefixSignature, addressReadingPresents.PrefixSignature);
        Assert.Equal(addressTrainingWrites.StructureSignature, addressReadingPresents.StructureSignature);
    }

    // ── Проверка 4: контроль переставленных проверенных пометок ───────────────
    [Fact]
    public void ControlWithSwappedVerifiedTags_FollowsTheRecordedLesson_NotTheText()
    {
        // Переставлены ТОЛЬКО явно проверенные пометки двух учебных строк.
        // Тексты, отвергнутые варианты и всё остальное не тронуты.
        var unit = TrainSemanticUnit(BuildBatch(
            PastRow(verifiedRuleTags: ["TENSE_PRESENT"]),
            PresentRow(verifiedRuleTags: ["TENSE_PAST"])));

        // Записанный урок теперь говорит о предложении в прошедшем времени
        // «настоящее». Набор пометок обязан следовать записанному уроку.
        var past = unit.Tokenize(PastSource, SourceLanguage);
        Assert.Contains(RawPresentTense, past.RuleTags);
        Assert.DoesNotContain(RawPastTense, past.RuleTags);

        // И наоборот для второй строки.
        var present = unit.Tokenize(PresentSource, SourceLanguage);
        Assert.Contains(RawPastTense, present.RuleTags);
        Assert.DoesNotContain(RawPresentTense, present.RuleTags);
    }

    // ── Проверка 5: слепок хранит исправленные пометки и свежая загрузка их даёт ─
    //
    // Назначенный расход этой проверки: одна учебная партия обычным путём
    // (шесть уроков единиц), четыре чтения обычного потока после обучения,
    // одно сохранение, одна свежая загрузка и ещё четыре чтения потока.
    // Восстановление единицы из слепка обучения не содержит.
    [Fact]
    public async Task SnapshotOfTheLateCorrection_IsRestoredByAFreshLoad_AsTheLiveObject()
    {
        // Обычная продуктовая машина, а не конструктор с подставленным потоком.
        var machine = new TranslationMachine();

        var batch = BuildBatch(PastRow(), PresentRow());
        var train = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(train.IsSuccess, train.ErrorMessage);

        // Ни один урок партии не потерян.
        Assert.Equal(batch.Examples.Count, train.Value!.RowsTrained);

        // ── Четыре чтения обычного потока после обучения ─────────────────────
        var afterTraining = new Dictionary<string, (string Text, string AdapterId)>();
        foreach (var row in Panel)
            afterTraining[row.Code] = await ReadThroughOrdinaryFlowAsync(machine, row);

        // Слепок живого объекта и слепок, восстановленный из него же.
        var liveSnapshot = SemanticTokenizerSnapshot(machine);
        var restoredUnit = new SemanticTokenizerUnit();
        restoredUnit.ImportCheckpoint(
            machine.ExportCheckpoint().Value!.SemanticTokenizer);

        var checkpointPath = Path.Combine(
            Path.GetTempPath(), $"p52-tags-{Guid.NewGuid():N}.json");
        try
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);

            var load = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);
            var restoredMachine = load.Value!.Machine;
            Assert.NotSame(machine, restoredMachine);

            // ── Четыре чтения той же панели восстановленной машиной ──────────
            var afterFreshLoad = new Dictionary<string, (string Text, string AdapterId)>();
            foreach (var row in Panel)
                afterFreshLoad[row.Code] = await ReadThroughOrdinaryFlowAsync(restoredMachine, row);

            var restoredSnapshot = SemanticTokenizerSnapshot(restoredMachine);

            // Улики пишутся ДО решающих утверждений, чтобы журнал обнаружения
            // остался и у красного прогона.
            WriteReceipt("snapshot-and-fresh-load-receipt.json", new
            {
                назначение = FixId,
                попытка = AttemptId,
                обучено_уроков_единиц = train.Value!.RowsTrained,
                партий_обучения = 1,
                чтений_потока = Panel.Length * 2,
                сохранений = 1,
                свежих_загрузок = 1,
                ответы_после_обучения = afterTraining.ToDictionary(
                    entry => entry.Key,
                    entry => new { текст = entry.Value.Text, переходник = entry.Value.AdapterId }),
                ответы_после_свежей_загрузки = afterFreshLoad.ToDictionary(
                    entry => entry.Key,
                    entry => new { текст = entry.Value.Text, переходник = entry.Value.AdapterId }),
                слепок_живого_объекта = liveSnapshot,
                слепок_после_свежей_загрузки = restoredSnapshot
            });

            // ── Удержание текстов двух обученных строк ──────────────────────
            // Эталон читается только здесь, после получения фактических ответов.
            foreach (var row in Panel.Where(row => row.Gold is not null))
            {
                Assert.Equal(row.Gold, afterTraining[row.Code].Text);
                Assert.Equal(row.Gold, afterFreshLoad[row.Code].Text);
            }

            // ── Слепок обязан нести исправленные пометки ─────────────────────
            // Предложение в настоящем времени: в сохранённом состоянии обязано
            // стоять проверенное настоящее время. Пока пометок в слепке нет
            // вовсе, восстановить согласие ключей нечем.
            Assert.Contains(RawPresentTense, liveSnapshot);

            // ── Свежая загрузка обязана дать ровно то же состояние ───────────
            Assert.Equal(liveSnapshot, restoredSnapshot);

            // ── Восстановленная из слепка единица отвечает тем же ───────────
            // Это и есть проверка «свежая загрузка даёт то же, что живой
            // объект» для всех четырёх вещей сразу: если слепок не хранит
            // набор пометок, восстановленная единица вернёт догадку сырого
            // разбора и это утверждение упадёт.
            var restoredPresent = restoredUnit.Tokenize(PresentSource, SourceLanguage);
            Assert.Contains(RawPresentTense, restoredPresent.RuleTags);
            Assert.DoesNotContain(RawPastTense, restoredPresent.RuleTags);
            foreach (var tag in restoredPresent.RuleTags)
            {
                Assert.Contains(tag, restoredPresent.Signature);
                Assert.Contains(tag, restoredPresent.StructureSignature);
            }
        }
        finally
        {
            if (File.Exists(checkpointPath)) File.Delete(checkpointPath);
        }
    }

    // ── Чистые проверки формы: обучения ноль ──────────────────────────────────
    //
    // Эти проверки считаются ОТДЕЛЬНО от учебного расхода: партий, уроков единиц
    // и чтений потока в них нет, строятся только записи данных.

    [Fact]
    public void Reconciliation_DropsTagsThatOnlyMakeSenseInThePastTense()
    {
        // Набор сырого разбора прошедшей формы целиком: пол подлежащего, время,
        // пол глагола прошедшей формы и согласование подлежащего с ней.
        var rawPastSet = new[]
        {
            "RU:SUBJECT:MASC",
            RawPastTense,
            "RU:VERB_GENDER:MASC",
            "RU:AGREEMENT:SUBJECT_MASC_PAST_MASC"
        };

        var reconciled = TranslationVerifiedRuleTags.ReconcileTrainedTags(
            rawPastSet, ["TENSE=PRESENT"]);

        Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);

        // Проверенное настоящее время встало на место догадки прошедшего.
        Assert.Contains(RawPresentTense, reconciled.Value!);
        Assert.DoesNotContain(RawPastTense, reconciled.Value!);

        // Пол глагола прошедшей формы и согласование с ней при настоящем времени
        // не сохраняются: они описывали бы вид записи, которого уже нет.
        Assert.DoesNotContain("RU:VERB_GENDER:MASC", reconciled.Value!);
        Assert.DoesNotContain("RU:AGREEMENT:SUBJECT_MASC_PAST_MASC", reconciled.Value!);

        // Пометка, не зависящая от времени, остаётся на своём месте.
        Assert.Contains("RU:SUBJECT:MASC", reconciled.Value!);
    }

    [Fact]
    public void Reconciliation_KeepsThePastSet_WhenTheVerifiedTenseIsPast()
    {
        var rawPastSet = new[] { "RU:SUBJECT:MASC", RawPastTense, "RU:VERB_GENDER:MASC" };

        var reconciled = TranslationVerifiedRuleTags.ReconcileTrainedTags(
            rawPastSet, ["TENSE=PAST"]);

        Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);

        // Согласованный набор прошедшего времени сохраняется целиком и в том же порядке.
        Assert.Equal(rawPastSet, reconciled.Value!);
    }

    [Fact]
    public void Reconciliation_WithoutAVerifiedTense_LeavesTheRawSetAsItIs()
    {
        var rawPastSet = new[] { "RU:SUBJECT:MASC", RawPastTense };

        // Проверенная сторона о времени не сказала: придумывать за неё нечего.
        var reconciled = TranslationVerifiedRuleTags.ReconcileTrainedTags(rawPastSet, []);

        Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
        Assert.Equal(rawPastSet, reconciled.Value!);
    }

    [Fact]
    public void TrainedSetNamingTwoTenses_IsRefused()
    {
        var refusal = TranslationVerifiedRuleTags.ValidateTrainedAnnotation(
            [RawPastTense, RawPresentTense]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("TRAINED_RULE_TAG_TENSE_CONFLICT", refusal.ErrorCode);
    }

    [Fact]
    public void TrainedSetWithAnEmptyTag_IsRefused()
    {
        var refusal = TranslationVerifiedRuleTags.ValidateTrainedAnnotation([RawPastTense, "   "]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("TRAINED_RULE_TAG_EMPTY", refusal.ErrorCode);
    }

    [Theory]
    // Неизвестная форма записи: что значат её поля, читателю не известно.
    [InlineData(7, new[] { "RU:TENSE:PAST" }, "CHECKPOINT_TOKENIZER_ANNOTATION_VERSION_UNSUPPORTED")]
    // Прежняя форма записи не может нести набор пометок.
    [InlineData(0, new[] { "RU:TENSE:PAST" }, "CHECKPOINT_TOKENIZER_ANNOTATION_INVALID")]
    // Согласованная запись с двумя разными временами — это та самая смесь.
    [InlineData(1, new[] { "RU:TENSE:PAST", "RU:TENSE:PRESENT" }, "CHECKPOINT_TOKENIZER_ANNOTATION_INVALID")]
    public async Task SavedCorrectionWithABadAnnotation_IsRefusedBeforeAMachineIsBuilt(
        int annotationVersion,
        string[] ruleTags,
        string expectedErrorCode)
    {
        // Берётся сохранённое состояние ХОЛОДНОЙ машины: обучения в этой проверке нет.
        var cold = new TranslationMachine().ExportCheckpoint();
        Assert.True(cold.IsSuccess, cold.ErrorMessage);

        // В состояние подставляется одна негодная поправка разборщика.
        var tampered = cold.Value! with
        {
            SemanticTokenizer = cold.Value!.SemanticTokenizer with
            {
                SignatureCorrections =
                [
                    new SemanticTokenizerSignatureCorrectionCheckpoint(
                        PresentSource,
                        SentencePattern.SOV,
                        "SOV|RU:TENSE:PRESENT",
                        ruleTags,
                        annotationVersion)
                ]
            }
        };

        var path = Path.Combine(Path.GetTempPath(), $"p52-tags-bad-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(tampered, TranslationCheckpointJsonOptions.Standard));

            var load = await TranslationMachine.LoadCheckpointAsync(path);

            // Загрузка обязана отказать и не отдать ни одной машины.
            Assert.False(load.IsSuccess);
            Assert.Equal(expectedErrorCode, load.ErrorCode);
            Assert.Null(load.Value);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    // ReadThroughOrdinaryFlowAsync делает одно обычное чтение перевода: тот же
    // распорядитель и тот же поток, которым пользуется пользователь.
    private static async Task<(string Text, string AdapterId)> ReadThroughOrdinaryFlowAsync(
        TranslationMachine machine,
        PanelRow row)
    {
        // Распорядитель берётся у самой машины — обходных путей нет.
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        var result = await orchestrator.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: $"p52-tags-{row.Code}-{Guid.NewGuid():N}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: row.Source,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = SourceLanguage,
                ["target_language"] = TargetLanguage,
                ["topic_domain"] = "general"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Идентификатор переходника читается из показаний потока, не угадывается.
        var adapterId = result.Value!.FlowResult.Metrics.TryGetValue("adapter_id", out var id)
            ? id
            : "НЕ_СООБЩЁН";

        return (result.Value!.FinalHumanResult, adapterId);
    }

    // SemanticTokenizerSnapshot даёт сохранённое состояние разборщика как текст.
    // Сравнение текстом нужно потому, что доказывается именно СОСТАВ
    // сохранённого, а не ссылочное равенство объектов.
    private static string SemanticTokenizerSnapshot(TranslationMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);

        return JsonSerializer.Serialize(
            export.Value!.SemanticTokenizer,
            TranslationCheckpointJsonOptions.Standard);
    }

    // EvidenceDirectory — каталог улик этой попытки, разрешённый приёмником доказательств.
    private static string EvidenceDirectory() => AllowedEvidenceDirectory.Value;

    // Корень рабочего дерева: подъём от каталога сборки проверок до каталога улик.
    private static string? WorkingTreeRootOrNull()
    {
        // Корень дерева ищется подъёмом от каталога сборки проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "artifacts", "fbe")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Куда этому прогону разрешено писать улики. Решение принимает общий приёмник
    // доказательств ОДИН раз за прогон, по одному факту: отслеживает ли хранилище файлы
    // внутри назначенного каталога улик. Пока каталог не опубликован, прогон пишет в него и
    // остаётся в хранилище — это запись наблюдения. После публикации тот же прогон
    // становится проверочным и заметно уходит в игнорируемый каталог результатов испытаний.
    private static readonly Lazy<string> AllowedEvidenceDirectory = new(() =>
    {
        var root = WorkingTreeRootOrNull();

        // Корень не найден: улики пишутся рядом со сборкой, и это видно по пути.
        if (root is null)
        {
            var fallback = Path.Combine(AppContext.BaseDirectory, FixId, AttemptId);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        // Назначенный каталог улик этой попытки.
        var declared = Path.Combine(root, "artifacts", "fbe", FixId, AttemptId);

        // Разрешённый каталог выдаёт приёмник, он же сообщает род прогона и точный путь.
        return ExperimentFixes.ExperimentEvidenceSink
            .ForDeclaredDirectory(root, declared)
            .EnsureWriteRootDirectory();
    });

    // WriteReceipt пишет один файл улик в кодировке UTF-8 без метки порядка
    // байтов, только с переводом строки Unix и ровно одним в конце.
    private static void WriteReceipt(string fileName, object payload)
    {
        var path = Path.Combine(EvidenceDirectory(), fileName);

        var json = JsonSerializer.Serialize(payload, ReceiptJson)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        if (!json.EndsWith('\n')) json += "\n";
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }
}
