// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «проверенное человеком противопоставление проходит
// фильтр истории обучения» (адрес P65 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт допуска по ручному
// паспорту, запись P6501).
//
// ЧТО ЗДЕСЬ ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ.
// Прежде чем взять новые уроки в обучение, продукт сравнивает исходную строку
// каждого кандидата со всеми уже выученными исходными строками и выбрасывает
// слишком похожих: они якобы ничему новому не научат. Мера сравнения — доля
// общих слов.
//
// В ЧЁМ БЫЛА ОШИБКА. Решающая строка механизма —
// 27.TranslationDomain/TranslationStructuredBatchBuilder.cs:233–234:
// «return !historyFingerprints.Any(history => JaccardTokenSimilarity(...) >=
// maxOverlapThreshold);». Доля общих слов не различает смысл. Строка «Этот
// важный файл уже НЕ доступен» отличается от выученной утвердительной строки
// одним словом «не», но смысл у неё обратный; общих слов пять из шести, то есть
// фактически измеренные 0,8333 — выше порога 0,70. Разбиение на слова
// (строки 351–357 того же файла после правки: нижний регистр, только буквы и
// цифры, разбиение по пробелам) отбрасывает знаки препинания, поэтому повтор
// «…уже доступен!» даёт 1,0000. Защита одинаково считала «почти дубликатом» и
// исправление закона полярности, и повтор со знаком препинания: противоположный
// урок не попадал в обучение, и переводчик продолжал отказываться переводить
// отрицательную строку.
//
// ИСПРАВЛЕНИЕ. У фильтра появилась вторая перегрузка: она принимает сами пары
// истории обучения и паспорт проверенных противопоставлений. Кандидат, которого
// отвергло правило общих слов, допускается только тогда, когда в паспорте есть
// запись ровно на этого кандидата, она ссылается на урок, действительно
// присутствующий в переданной истории, несёт отметку проверки человеком, называет
// закон «polarity», совпадает по хешам содержимого обеих пар, несёт полный
// 64-символьный шестнадцатеричный чек ручной проверки, сохраняет то же
// направление перевода и имеет и источник, и перевод, действительно отличающиеся
// от старого урока. Старая перегрузка со списком строк сохранена без изменений —
// именно поэтому прежняя логика ниже не копируется, а вызывается тем же
// продуктовым кодом.
//
// ЧЕГО ЭТА ПРАВКА НЕ ДЕЛАЕТ, и это проверяется отдельными плечами: продукт не
// распознаёт новизну сам и не оценивает смысл перевода по одному признаку —
// решение принимает человек, а код проверяет только форму этого решения; точная
// коррекция ТОГО ЖЕ источника через это исключение не проходит; дубликат без
// паспорта остаётся отвергнут; любое отсутствующее или несовпадающее поле
// паспорта оставляет исходное решение фильтра в силе — без исключения и без
// ошибки.
//
// ЧЕГО ЭТОТ ФАЙЛ НЕ ДОКАЗЫВАЕТ: качество перевода, обучение на живых данных,
// время, память и чтение смысла строки машиной; он также не заменяет прогон
// полного набора тестов проекта.

namespace LlmFromScratch.Tests;

using TranslationDomain;

public sealed class TranslationVerifiedContrastAdmissionLearningTests
{
    // Урок истории: он уже выучен в обоих плечах опыта.
    private const string HistoryId = "P65-t01";
    private const string HistorySource = "Этот важный файл уже доступен";
    private const string HistoryTarget = "This important file is already available";

    // Противопоставление: отличается одним словом «не», смысл обратный.
    private const string ContrastId = "P65-t02";
    private const string ContrastSource = "Этот важный файл уже не доступен";
    private const string ContrastTarget = "This important file is no longer available";

    // Дубликат: та же строка, что история, плюс восклицательный знак; перевод тот
    // же. Паспорта у него нет, и он не должен проходить нигде.
    private const string DuplicateId = "P65-t03";
    private const string DuplicateSource = "Этот важный файл уже доступен!";
    private const string DuplicateTarget = "This important file is already available";

    // Совсем другая строка: её правило общих слов и так пропускает.
    private const string NovelId = "P65-t04";
    private const string NovelSource = "Открой красную папку";
    private const string NovelTarget = "Open the red folder";

    // Чек ручной проверки нужной формы: ровно 64 шестнадцатеричных знака.
    private const string ReviewReceipt =
        "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    // ── Плечо A: что обязано сохраниться ──────────────────────────────────────

    // Прежняя логика, вызванная тем же продуктовым кодом: старая перегрузка со
    // списком исходных строк отвергает и противопоставление, и дубликат.
    [Fact]
    public void ArmA_OldOverloadStillDropsBothTheContrastAndTheDuplicate()
    {
        var candidates = new[] { Contrast(), Duplicate(), Novel() };

        var kept = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { HistorySource });

        // Остаётся только совсем другая строка.
        Assert.Equal(new[] { NovelId }, kept.Select(pair => pair.Id).ToArray());
    }

    // Фактически измеренные доли общих слов продуктовой мерой: у
    // противопоставления 0,8333, у дубликата 1,0000 — обе выше порога 0,70.
    [Fact]
    public void ArmA_MeasuredWordOverlapIsAboveTheThresholdForBothRejectedRows()
    {
        var contrastOverlap = TranslationStructuredBatchBuilder.ComputeTokenOverlap(
            ContrastSource, HistorySource);
        var duplicateOverlap = TranslationStructuredBatchBuilder.ComputeTokenOverlap(
            DuplicateSource, HistorySource);

        Assert.Equal(0.8333, contrastOverlap, precision: 4);
        Assert.Equal(1.0000, duplicateOverlap, precision: 4);
    }

    // Новая перегрузка без паспорта повторяет прежнее решение один в один:
    // те же записи, в том же порядке.
    [Fact]
    public void ArmA_NewOverloadWithoutManifestRepeatsTheOldDecision()
    {
        var candidates = new[] { Contrast(), Duplicate(), Novel() };

        var oldDecision = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { HistorySource });
        var newDecisionWithoutManifest = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { History() },
            contrasts: null);
        var newDecisionWithEmptyManifest = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { History() },
            new Dictionary<string, VerifiedTranslationContrast>());

        Assert.Equal(
            oldDecision.Select(pair => pair.Id).ToArray(),
            newDecisionWithoutManifest.Select(pair => pair.Id).ToArray());
        Assert.Equal(
            oldDecision.Select(pair => pair.Id).ToArray(),
            newDecisionWithEmptyManifest.Select(pair => pair.Id).ToArray());
    }

    // Пустая история и пустой список кандидатов ведут себя как прежде.
    [Fact]
    public void ArmA_EmptyHistoryAndEmptyCandidatesBehaveAsBefore()
    {
        var candidates = new[] { Contrast(), Duplicate() };

        // Без истории сравнивать не с чем — проходят все кандидаты.
        var withoutHistory = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            Array.Empty<TranslationTrainReadyPair>(),
            BuildValidManifest());

        // Пустой список кандидатов остаётся пустым.
        var withoutCandidates = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            Array.Empty<TranslationTrainReadyPair>(),
            new[] { History() },
            BuildValidManifest());

        Assert.Equal(new[] { ContrastId, DuplicateId }, withoutHistory.Select(pair => pair.Id).ToArray());
        Assert.Empty(withoutCandidates);
    }

    // ── Плечо B: что исправлено ───────────────────────────────────────────────

    // Действительный паспорт допускает противопоставление и не трогает дубликат;
    // порядок кандидатов сохраняется.
    [Fact]
    public void ArmB_VerifiedContrastIsAdmittedWhileTheDuplicateStaysRejected()
    {
        var candidates = new[] { Contrast(), Duplicate(), Novel() };

        var kept = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { History() },
            BuildValidManifest());

        // Противопоставление допущено, дубликат отвергнут, порядок прежний.
        Assert.Equal(new[] { ContrastId, NovelId }, kept.Select(pair => pair.Id).ToArray());
    }

    // Допущенный урок действительно увеличивает объём обучения: продуктовый
    // сборщик партии раскладывает каждую пару на три урока единиц (разборщик,
    // исполнитель направления, проверяющий качество), поэтому было 3, стало 6.
    [Fact]
    public void ArmB_AdmittedContrastDoublesTheTrainedUnitExamples()
    {
        var history = new[] { History() };

        var armA = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "p65-arm-a",
            history,
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        var armB = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "p65-arm-b",
            new[] { History(), Contrast() },
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        Assert.Equal(3, armA.Examples.Count);
        Assert.Equal(6, armB.Examples.Count);

        // Среди уроков плеча B появился перевод отрицательной строки для
        // исполнителя направления русский->английский.
        Assert.Contains(armB.Examples, example =>
            example.UnitId == TranslationUnitIds.ExecutorRuEn &&
            example.ChosenOutputJson.Contains("no longer available", StringComparison.Ordinal));

        // В плече A такого урока нет вовсе.
        Assert.DoesNotContain(armA.Examples, example =>
            example.ChosenOutputJson.Contains("no longer available", StringComparison.Ordinal));
    }

    // ── Плечо C: чувствительность к прежней логике ────────────────────────────

    // Те же утверждения, что в плече B, на прежней логике не выполняются: без
    // паспорта противопоставления в допущенных нет, и объём обучения остаётся 3.
    // Прежняя логика здесь не копия, а тот же продуктовый код, вызванный старым
    // способом.
    [Fact]
    public void ArmC_WithoutTheManifestTheSameAssertionsFail()
    {
        var candidates = new[] { Contrast(), Duplicate(), Novel() };

        var keptByOldLogic = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            candidates,
            new[] { HistorySource });

        // Утверждение плеча B «противопоставление допущено» здесь ложно.
        Assert.DoesNotContain(keptByOldLogic, pair => pair.Id == ContrastId);

        // И объём обучения остаётся прежним: только три урока истории.
        var trainedByOldLogic = new List<TranslationTrainReadyPair> { History() };
        trainedByOldLogic.AddRange(keptByOldLogic.Where(pair => pair.Id != NovelId));
        var armAOnly = TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch(
            "p65-arm-c",
            trainedByOldLogic,
            heldOut: false,
            signatureMode: TranslationSignatureMode.Structural);

        Assert.Equal(3, armAOnly.Examples.Count);
    }

    // ── Плечо D: восемь проверок правила допуска без обучения ─────────────────

    // Пять исходных краёв опыта: не отмечено проверкой человеком; закон назван
    // другим словом; ссылка указывает на урок вне переданной истории; паспорта
    // на этого кандидата нет вовсе; перевод совпадает со старым уроком.
    [Fact]
    public void ArmD_FiveOriginalEdgesKeepTheRefusal()
    {
        var contrast = Contrast();
        var history = History();

        // 1. Отметка проверки человеком не стоит.
        AssertRefused("не отмечено проверкой человеком",
            contrast, history, ValidManifestFor(contrast, history) with { Verified = false });

        // 2. Закон назван словом не из разрешённых.
        AssertRefused("закон назван другим словом",
            contrast, history, ValidManifestFor(contrast, history) with { ChangedLaw = "unknown" });

        // 3. Ссылка указывает на урок, которого в переданной истории нет.
        AssertRefused("ссылка вне переданной истории",
            contrast, history, ValidManifestFor(contrast, history) with { ReferenceId = "P65-t99" });

        // 4. Паспорт выписан на другого кандидата, то есть на этого его нет.
        AssertRefused("паспорта на этого кандидата нет",
            contrast,
            history,
            ValidManifestFor(contrast, history) with { CandidateId = DuplicateId },
            manifestKey: DuplicateId);

        // 5. Перевод совпадает со старым уроком: изменённого закона в паре нет.
        var sameTargetCandidate = Pair(ContrastId, ContrastSource, HistoryTarget);
        AssertRefused("перевод совпадает со старым уроком",
            sameTargetCandidate, history, ValidManifestFor(sameTargetCandidate, history));
    }

    // Подмена содержимого кандидата: байты урока изменились после ручной
    // проверки, поэтому паспорт к ним больше не относится.
    [Fact]
    public void ArmD_SubstitutedCandidateContentKeepsTheRefusal()
    {
        var verifiedContrast = Contrast();
        var history = History();
        var manifest = ValidManifestFor(verifiedContrast, history);

        // Тот же идентификатор, но другой перевод: хеш содержимого уже не тот.
        var substituted = Pair(ContrastId, ContrastSource, "This important file is available again");

        AssertRefused("подменено содержимое кандидата", substituted, history, manifest);
    }

    // Подмена содержимого старого урока: паспорт ссылается на переписанную
    // строку истории, значит сравнение человеком относилось к другому тексту.
    [Fact]
    public void ArmD_SubstitutedReferenceContentKeepsTheRefusal()
    {
        var contrast = Contrast();
        var originalHistory = History();
        var manifest = ValidManifestFor(contrast, originalHistory);

        // Тот же идентификатор истории, но другой перевод.
        var substitutedHistory = Pair(HistoryId, HistorySource, "This important file is open");

        AssertRefused("подменено содержимое старого урока", contrast, substitutedHistory, manifest);
    }

    // Пустой или укороченный чек ручной проверки: происхождение решения не
    // записано, допуска нет.
    [Fact]
    public void ArmD_EmptyOrMalformedReviewReceiptKeepsTheRefusal()
    {
        var contrast = Contrast();
        var history = History();
        var valid = ValidManifestFor(contrast, history);

        AssertRefused("пустой чек проверки", contrast, history, valid with { ReviewReceiptSha256 = string.Empty });
        AssertRefused("укороченный чек проверки", contrast, history, valid with { ReviewReceiptSha256 = ReviewReceipt[..63] });
        AssertRefused("чек не шестнадцатеричный", contrast, history, valid with { ReviewReceiptSha256 = new string('z', 64) });
    }

    // Точная коррекция того же источника остаётся путём другого владельца: это
    // исключение её не пропускает.
    [Fact]
    public void ArmD_SameSourceSentenceIsNotLetThroughAsContrast()
    {
        var history = History();

        // Тот же источник, другой перевод: закон полярности здесь не менялся.
        var correction = Pair("P65-t05", HistorySource, "This important file is accessible");

        AssertRefused("тот же источник, только исправленный перевод", correction, history,
            ValidManifestFor(correction, history));
    }

    // Разные направления перевода несравнимы, даже если всё остальное совпало.
    [Fact]
    public void ArmD_DirectionMismatchKeepsTheRefusal()
    {
        var history = History();
        var otherDirection = Pair(ContrastId, ContrastSource, ContrastTarget, direction: "EN_RU");

        AssertRefused("разные направления перевода", otherDirection, history,
            ValidManifestFor(otherDirection, history));
    }

    // ── Общие построители ─────────────────────────────────────────────────────

    // AssertRefused проверяет, что при данном паспорте кандидат остаётся
    // отвергнутым: правило общих слов по-прежнему решает, а исключение не выдано.
    private static void AssertRefused(
        string caseName,
        TranslationTrainReadyPair candidate,
        TranslationTrainReadyPair history,
        VerifiedTranslationContrast manifest,
        string? manifestKey = null)
    {
        var manifestByCandidate = new Dictionary<string, VerifiedTranslationContrast>
        {
            [manifestKey ?? candidate.Id] = manifest
        };

        var kept = TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness(
            new[] { candidate },
            new[] { history },
            manifestByCandidate);

        Assert.True(kept.Count == 0, $"случай «{caseName}» должен был остаться отвергнутым");
    }

    // BuildValidManifest собирает паспорт ровно на противопоставление, как в опыте:
    // у дубликата паспорта нет.
    private static Dictionary<string, VerifiedTranslationContrast> BuildValidManifest()
        => new()
        {
            [ContrastId] = ValidManifestFor(Contrast(), History())
        };

    // ValidManifestFor считает оба хеша содержимого тем же продуктовым помощником,
    // которым их считает доверенный вызывающий процесс.
    private static VerifiedTranslationContrast ValidManifestFor(
        TranslationTrainReadyPair candidate,
        TranslationTrainReadyPair reference)
        => new(
            CandidateId: candidate.Id,
            ReferenceId: reference.Id,
            ChangedLaw: VerifiedTranslationContrast.PolarityLaw,
            CandidateContentSha256: VerifiedTranslationContrast.ComputeContentSha256(candidate),
            ReferenceContentSha256: VerifiedTranslationContrast.ComputeContentSha256(reference),
            ReviewReceiptSha256: ReviewReceipt,
            Verified: true);

    private static TranslationTrainReadyPair History()
        => Pair(HistoryId, HistorySource, HistoryTarget);

    private static TranslationTrainReadyPair Contrast()
        => Pair(ContrastId, ContrastSource, ContrastTarget);

    private static TranslationTrainReadyPair Duplicate()
        => Pair(DuplicateId, DuplicateSource, DuplicateTarget);

    private static TranslationTrainReadyPair Novel()
        => Pair(NovelId, NovelSource, NovelTarget);

    // Pair собирает обучающую пару нужной формы: направление русский->английский,
    // тема general, строка годна к обучению.
    private static TranslationTrainReadyPair Pair(
        string id,
        string aSide,
        string chosen,
        string direction = "RU_EN")
        => new(
            Id: id,
            Source: "p65-fixture",
            Direction: direction,
            PairFamily: "general",
            Prompt: "Translate:",
            ASide: aSide,
            Chosen: chosen,
            Rejected: "[WRONG: polarity_opposite.]",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);
}
