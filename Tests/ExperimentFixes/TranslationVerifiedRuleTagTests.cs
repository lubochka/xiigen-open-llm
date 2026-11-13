// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Что было неправильно. Пометку правила, с которой сохраняется ПРАВИЛЬНЫЙ урок пары, собирали из
// стороны «как неправильно» этой же пары (адрес `27.TranslationDomain/TranslationStructuredBatchBuilder.cs:98`,
// строка `signature = AppendRuleTags(signature, BuildRuleTags(pair.Rejected));`). Для пары про время
// это ровно противоположность правды: предложение в прошедшем времени сохранялось с пометкой
// настоящего времени, а предложение в настоящем — с пометкой прошедшего. Правильных пометок было
// ноль из двух.
//
// Что стало. Пометка берётся только из проверенной обучающей стороны (новое поле пары
// `verified_rule_tags`). Отсутствие поля — честное отсутствие проверенной пометки, а не разрешение
// снова читать отвергнутый вариант. Противоречивая, пустая или неизвестная пометка времени
// отклоняет всю партию до того, как записан хоть один урок.
//
// Что этим НЕ улучшается. Ответ пользователю не меняется: на обученных строках первым отвечает
// точная память перевода, и текст перевода там и до, и после правки верный. Своя догадка сырого
// разборщика тоже не исправлена: он по-прежнему добавляет `RU:TENSE:PAST` каждому глаголу
// (адрес `27.TranslationDomain/TranslationSemanticTokenizer.cs:169`), то есть для предложения в
// настоящем времени сообщает прошедшее. Согласование проверенной пометки с этой догадкой — другая,
// отдельно назначаемая работа.
//
// Чувствительность к мутации. Каждая проверка ниже с положительным утверждением про `RULE:TENSE=…`
// падает на старом коде: старый источник давал `RULE:TENSE_PRESENT` там, где правда — прошедшее
// время, и `RULE:TENSE_PAST` там, где правда — настоящее.
public sealed class TranslationVerifiedRuleTagTests
{
    // Строка t1 плана 105, адрес P52, раздел 4.3: источник в прошедшем времени, а отвергнутый
    // вариант намеренно заявляет настоящее время.
    private static TranslationTrainReadyPair PastRow(
        IReadOnlyList<string>? verifiedRuleTags = null,
        string? rejected = null)
    {
        return new TranslationTrainReadyPair(
            Id: "P52-TAGS-t1",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: "Он проверил файл",
            Chosen: "He checked the file",
            Rejected: rejected ?? "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: verifiedRuleTags ?? new[] { "TENSE_PAST" });
    }

    // Строка t2 того же раздела: источник в настоящем времени, отвергнутый вариант заявляет
    // прошедшее.
    private static TranslationTrainReadyPair PresentRow()
    {
        return new TranslationTrainReadyPair(
            Id: "P52-TAGS-t2",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: "Он проверяет файл",
            Chosen: "He checks the file",
            Rejected: "He checked the file [WRONG: tense] [RULE: TENSE_PAST]",
            ValidationOutcome: "train_ready",
            ValidationErrors: [],
            VerifiedRuleTags: ["TENSE_PRESENT"]);
    }

    private static TranslationLearningBatch BuildBatch(params TranslationTrainReadyPair[] pairs)
    {
        return TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "P52-TAGS-main-v1",
            pairs,
            signatureMode: TranslationSignatureMode.Structural);
    }

    [Fact]
    public void VerifiedPastTag_IsStoredWithTheLesson_NotThePresentTagOfTheRejectedVariant()
    {
        var batch = BuildBatch(PastRow());

        Assert.All(batch.Examples, example =>
        {
            Assert.Contains("RULE:TENSE=PAST", example.SemanticSignature);
            Assert.DoesNotContain("RULE:TENSE=PRESENT", example.SemanticSignature);
            Assert.DoesNotContain("RULE:TENSE_PRESENT", example.SemanticSignature);
        });
    }

    [Fact]
    public void VerifiedPresentTag_IsStoredWithTheLesson_NotThePastTagOfTheRejectedVariant()
    {
        var batch = BuildBatch(PresentRow());

        Assert.All(batch.Examples, example =>
        {
            Assert.Contains("RULE:TENSE=PRESENT", example.SemanticSignature);
            Assert.DoesNotContain("RULE:TENSE=PAST", example.SemanticSignature);
            Assert.DoesNotContain("RULE:TENSE_PAST", example.SemanticSignature);
        });
    }

    [Fact]
    public void BothRows_TogetherCarryTheirOwnVerifiedTense()
    {
        var batch = BuildBatch(PastRow(), PresentRow());

        var pastLesson = batch.Examples.First(example => example.ExampleId.StartsWith("P52-TAGS-t1", StringComparison.Ordinal));
        var presentLesson = batch.Examples.First(example => example.ExampleId.StartsWith("P52-TAGS-t2", StringComparison.Ordinal));

        Assert.Contains("RULE:TENSE=PAST", pastLesson.SemanticSignature);
        Assert.Contains("RULE:TENSE=PRESENT", presentLesson.SemanticSignature);
    }

    [Fact]
    public void MissingVerifiedAnnotation_LeavesTheLessonWithoutAnyRuleMark()
    {
        // Поле отсутствует, а отвергнутая сторона по-прежнему несёт `[RULE: TENSE_PRESENT]`.
        // Урок обязан остаться вообще без пометки правила: отсутствие проверенной пометки не
        // возвращает право читать отвергнутый вариант.
        var pair = new TranslationTrainReadyPair(
            Id: "P52-TAGS-no-verified-field",
            Source: "simulation",
            Direction: "RU_EN",
            PairFamily: "tense",
            Prompt: "Переведи",
            ASide: "Он проверил файл",
            Chosen: "He checked the file",
            Rejected: "He checks the file [WRONG: tense] [RULE: TENSE_PRESENT]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);

        var batch = BuildBatch(pair);

        Assert.All(batch.Examples, example =>
            Assert.DoesNotContain("RULE:", example.SemanticSignature));
    }

    [Fact]
    public void ChangedRejectedSide_DoesNotChangeTheVerifiedAnnotation()
    {
        var plain = BuildBatch(PastRow());
        var changed = BuildBatch(PastRow(
            rejected: "He will check the file [WRONG: tense] [RULE: TENSE_PRESENT] [RULE: CASE=DATIVE]"));

        var plainSignature = plain.Examples
            .Single(example => example.UnitId == TranslationUnitIds.SemanticTokenizer)
            .SemanticSignature;
        var changedSignature = changed.Examples
            .Single(example => example.UnitId == TranslationUnitIds.SemanticTokenizer)
            .SemanticSignature;

        Assert.Equal(plainSignature, changedSignature);
        Assert.Contains("RULE:TENSE=PAST", changedSignature);
        Assert.DoesNotContain("RULE:CASE=DATIVE", changedSignature);
    }

    [Fact]
    public void BothTensesAtOnce_AreRefusedBeforeTheBatchIsBuilt()
    {
        var refusal = TranslationVerifiedRuleTags.Canonicalize(["TENSE_PAST", "TENSE_PRESENT"]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("VERIFIED_RULE_TAG_TENSE_CONFLICT", refusal.ErrorCode);
        Assert.Throws<InvalidOperationException>(() =>
            BuildBatch(PastRow(verifiedRuleTags: ["TENSE_PAST", "TENSE_PRESENT"])));
    }

    [Fact]
    public void EmptyVerifiedTag_IsRefusedBeforeTheBatchIsBuilt()
    {
        var refusal = TranslationVerifiedRuleTags.Canonicalize(["   "]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("VERIFIED_RULE_TAG_EMPTY", refusal.ErrorCode);
        Assert.Throws<InvalidOperationException>(() =>
            BuildBatch(PastRow(verifiedRuleTags: [""])));
    }

    [Fact]
    public void UnsupportedTenseValue_IsRefusedBeforeTheBatchIsBuilt()
    {
        var refusal = TranslationVerifiedRuleTags.Canonicalize(["TENSE_FUTURE"]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("VERIFIED_RULE_TAG_UNSUPPORTED_TENSE", refusal.ErrorCode);
        Assert.Throws<InvalidOperationException>(() =>
            BuildBatch(PastRow(verifiedRuleTags: ["TENSE_FUTURE"])));
    }

    [Fact]
    public void BareFamilyName_IsRefusedAsATenseWithoutAValue()
    {
        var refusal = TranslationVerifiedRuleTags.Canonicalize(["TENSE"]);

        Assert.False(refusal.IsSuccess);
        Assert.Equal("VERIFIED_RULE_TAG_UNSUPPORTED_TENSE", refusal.ErrorCode);
    }

    [Fact]
    public void ThreeSpellingsOfThePastTense_CollapseToOneMark()
    {
        var accepted = TranslationVerifiedRuleTags.Canonicalize(["TENSE_PAST", "TENSE:PAST", "TENSE=PAST"]);

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(new[] { "TENSE=PAST" }, accepted.Value!);

        var batch = BuildBatch(PastRow(verifiedRuleTags: ["TENSE_PAST", "TENSE:PAST", "TENSE=PAST"]));
        var signature = batch.Examples
            .Single(example => example.UnitId == TranslationUnitIds.SemanticTokenizer)
            .SemanticSignature;
        var occurrences = signature.Split("RULE:TENSE=PAST").Length - 1;

        Assert.Equal(1, occurrences);
    }

    [Fact]
    public void VerifiedMarkOfAnotherFamily_KeepsTheExistingNormalization()
    {
        var accepted = TranslationVerifiedRuleTags.Canonicalize(["CASE=DATIVE", "TENSE_PAST"]);

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(new[] { "CASE=DATIVE", "TENSE=PAST" }, accepted.Value!);

        var batch = BuildBatch(PastRow(verifiedRuleTags: ["CASE=DATIVE", "TENSE_PAST"]));
        var signature = batch.Examples
            .Single(example => example.UnitId == TranslationUnitIds.SemanticTokenizer)
            .SemanticSignature;

        Assert.Contains("RULE:CASE=DATIVE", signature);
        Assert.Contains("RULE:TENSE=PAST", signature);
    }

    [Fact]
    public void AWordThatOnlyStartsWithTheFamilyName_IsNotReadAsATenseStatement()
    {
        var accepted = TranslationVerifiedRuleTags.Canonicalize(["TENSED=X"]);

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(new[] { "TENSED=X" }, accepted.Value!);
    }

    [Fact]
    public void AbsentAnnotation_IsAnAllowedEmptyResult_NotARefusal()
    {
        var absent = TranslationVerifiedRuleTags.Canonicalize(null);
        var empty = TranslationVerifiedRuleTags.Canonicalize([]);

        Assert.True(absent.IsSuccess, absent.ErrorMessage);
        Assert.Empty(absent.Value!);
        Assert.True(empty.IsSuccess, empty.ErrorMessage);
        Assert.Empty(empty.Value!);
    }

    [Fact]
    public void TrainedUnit_ReturnsTheVerifiedAnnotation_ForTheExactTrainingText()
    {
        // Урок, построенный конструктором партий, скармливается той же обученной единице
        // разборщика, которую использует продуктовая машина, и затем читается обратно. Это и есть
        // доказательство сохранения правильной пометки в обученном состоянии.
        var batch = BuildBatch(PastRow(), PresentRow());
        var unit = new SemanticTokenizerUnit();
        foreach (var example in batch.Examples.Where(example => example.UnitId == TranslationUnitIds.SemanticTokenizer))
            unit.TrainOnExample(example);

        var past = unit.Tokenize("Он проверил файл", "ru");
        var present = unit.Tokenize("Он проверяет файл", "ru");

        Assert.Contains("RULE:TENSE=PAST", past.Signature);
        Assert.DoesNotContain("RULE:TENSE=PRESENT", past.Signature);
        Assert.Contains("RULE:TENSE=PRESENT", present.Signature);
        Assert.DoesNotContain("RULE:TENSE=PAST", present.Signature);
    }

    [Fact]
    public void RawParserTenseGuess_StaysUnrepairedByThisTransfer()
    {
        // Честная граница: своя догадка сырого разборщика не исправлена. Для предложения в
        // настоящем времени он по-прежнему сообщает прошедшее. Эту проверку нельзя убирать,
        // чтобы отчёт не выглядел шире, чем сделанная работа.
        var tokenizer = new TranslationSemanticTokenizer();

        var present = tokenizer.Tokenize("Он проверяет файл", "ru");

        Assert.Contains("RU:TENSE:PAST", present.RuleTags);
        Assert.DoesNotContain("RU:TENSE:PRESENT", present.RuleTags);
    }
}
