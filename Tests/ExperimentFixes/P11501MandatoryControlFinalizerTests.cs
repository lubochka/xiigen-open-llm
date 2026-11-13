// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using P11501Experiment;

// Что здесь проверяется, по-человечески.
//
// Это раздел про проверяющих и итоговую пригодность: план сто тридцать девять, раздел
// R117-01; аудит сто тридцать восемь, находки F03 и F05.
//
// Что было неправильно, по коду. Обязательные контроли записывались в свидетельство, а
// итог задавался строковой константой. В файле опыта
// experiments-v2/RP01-MICRO/P11501/Program.cs до этой правки стояло:
//   строка 658: execution_status = "COMPLETED"
//   строка 659: learning_validity = "VALID_FOR_NAMED_UNIT"
//   строки 747-748: те же две константы во втором файле состояния
// Изменение отпечатка замороженных весов вычислялось на строке 639, при расхождении
// добавляло предупреждение на строке 641, попадало в поле на строке 671 — и в итоговую
// пригодность не входило вовсе. Итог гипотезы на строке 725 зависел ровно от трёх
// величин (паритет оценок, паритет порядка, падение числа приведений текста к корням) и
// не смотрел ни на честную расписку, ни на отклонение испорченной, ни на превышение
// порога вырождения, ни на заморозку весов.
//
// Второе: сравнение плеч шло по общему префиксу. Цикл на строке 860 был
// for (i < Math.Min(left.Count, right.Count)), а число сравнённых строк на строке 910
// записывалось тем же Math.Min. Поэтому укороченная панель не ломала паритет и даже не
// была видна в числе строк. Опознания запросов не сравнивались вовсе — строки
// сопоставлялись по положению, поэтому перестановка и дубликат проходили. Полный текст
// ответа (SelectedOutput) не сравнивался ни разу, поэтому тот же выбранный документ с
// другим ответом проходил. Уверенность сравнивалась как Math.Abs(a - b) > 1e-12 без
// проверки конечности, а для «не числа» это сравнение ложно, то есть «не число» молча
// считалось совпадением.
//
// Что стало. Итог вычисляет сама сверка: финализатор получает доверенный список
// обязательных контролей из паспорта опыта, а не из проверяемого ответа, и любое из
// пяти обстоятельств — отсутствие, неизвестность, провал, дубликат, лишнее опознание
// вне назначения — по отдельности отзывает пригодность. Полная сверка панелей требует
// равного числа строк, точной последовательности опознаний запросов и полного текста
// ответа, а общий префикс совпадением не считает.
//
// Важная тонкость, названная прямо. Контроль пройден, когда наблюдение совпало с
// ЗАРАНЕЕ ЗАДАННЫМ ожиданием, а не когда «все исходные логические поля истинны».
// Отрицательный контроль «испорченная расписка принята» имеет ожидание ЛОЖЬ: он пройден
// ровно тогда, когда заранее испорченный пример правильно отклонён. Требование «все
// истина» сломало бы именно этот контроль.
//
// Чего эта правка НЕ лечит, и я этого не заявляю. Первое: ложная терминальность
// запускателей (находка F07) — исправление назначено разделу R117-02, здесь оно не
// закрыто, оболочки запуска я не менял. Второе: независимая ожидаемая версия снимка из
// находки F03 — здесь закрыты только её части про конечность и точные множества внутри
// полной сверки панелей; доверенная ожидаемая версия происхождения остаётся за
// проверяющим происхождения. Третье: это не научный пересчёт опыта — сохранённые сырые
// свидетельства я не переписывал.
//
// Числа опорного случая взяты из уже сохранённой попытки
// experiments-v2/RP01-MICRO/P11501/results/attempt-0001/metrics.json: заморозка весов
// истина, честная расписка принята, испорченная НЕ принята, паритет оценок и порядка
// истина, слепая точность 0,5 против доли постоянного ответчика 0,25.
//
// Проверки зовут ДЕЙСТВУЮЩИЕ функции, скомпилированные и в хост опыта, и сюда: ни одной
// копии правила в тесте нет. Обращений к модели и обучению ноль.
//
// О красном прогоне на прежней логике. Красные по сути, то есть на прежнем коде они
// обязаны были падать: «отсутствующий обязательный контроль отзывает итог», «неизвестное
// состояние обязательного контроля отзывает итог» и «укороченная панель отвергнута» —
// прежний итог был строковой константой и не менялся ни в одном из этих случаев, а
// укороченная панель проходила по общему префиксу. Положительный контроль «честный
// сохранённый прогон проходит» зелёный и до, и после.
//
// Прогона не было: сборка и тесты в этой среде запрещены политикой, я их не запускал и
// зелёными не называю.
public sealed class P11501MandatoryControlFinalizerTests
{
    /// <summary>Опознание опыта из паспорта, а не из проверяемого ответа.</summary>
    private const string ExperimentId = "P115-PRIMARY-01";

    /// <summary>Попытка, закреплённая константой пути в хозяине опыта.</summary>
    private const string AttemptId = "attempt-0001";

    /// <summary>Доверенный список обязательных контролей ровно в том составе, что в хозяине опыта.</summary>
    private static MandatoryControlAssignment Assignment()
        => new(
            ExperimentId,
            AttemptId,
            new[]
            {
                "encoder_stayed_frozen",
                "honest_receipt_accepted",
                "damaged_receipt_accepted",
                "panel_identity_full",
                "scores_identical",
                "ranking_identical",
                "accuracy_above_degeneracy"
            });

    /// <summary>
    /// Наблюдения честного сохранённого прогона. Значения взяты из сохранённой попытки;
    /// у отрицательного контроля ожидание — ложь, и наблюдение ложь его проходит.
    /// </summary>
    private static List<MandatoryControlObservation> SavedHonestObservations()
        => new()
        {
            new MandatoryControlObservation("encoder_stayed_frozen", true, true, "отпечатки весов до и после"),
            new MandatoryControlObservation("honest_receipt_accepted", true, true, "честная расписка"),
            new MandatoryControlObservation("damaged_receipt_accepted", false, false, "заранее испорченная расписка отклонена"),
            new MandatoryControlObservation("panel_identity_full", true, true, "полная сверка панелей"),
            new MandatoryControlObservation("scores_identical", true, true, "оценки кандидатов"),
            new MandatoryControlObservation("ranking_identical", true, true, "порядок кандидатов"),
            new MandatoryControlObservation("accuracy_above_degeneracy", true, true, "0,5 против 0,25")
        };

    /// <summary>Одна строка панели: один кандидат, названный документ и полный текст ответа.</summary>
    private static QueryReadout Row(string queryId, string selectedId, string output, double confidence)
        => new(
            queryId,
            "вопрос " + queryId,
            "v1",
            new[] { new ScoredCandidate(selectedId, 0.5, 0.5, 0.5, 0.0, 0.5, 0) },
            selectedId,
            output,
            0.5,
            confidence,
            1,
            1,
            1,
            1.0,
            1);

    /// <summary>Честная панель из двух строк, одинаковая в обоих плечах.</summary>
    private static List<QueryReadout> HonestPanel()
        => new()
        {
            Row("r1", "doc-1", "первый полный ответ", 0.75),
            Row("r2", "doc-2", "второй полный ответ", 0.5)
        };

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ, зелёный и до, и после. Честный сохранённый прогон проходит
    // все семь обязательных контролей, и пригодность остаётся прежней.
    [Fact]
    public void SavedHonestRunPassesEveryMandatoryControlAndKeepsTheValidVerdict()
    {
        var verdict = MandatoryControlFinalizer.Finalize(Assignment(), SavedHonestObservations());

        Assert.True(verdict.MeasurementValid);
        Assert.Equal(MandatoryControlFinalizer.ValidLearningValidity, verdict.LearningValidity);
        Assert.Empty(verdict.MissingCheckIds);
        Assert.Empty(verdict.UnknownCheckIds);
        Assert.Empty(verdict.FailedCheckIds);
        Assert.Empty(verdict.DuplicateCheckIds);
        Assert.Empty(verdict.UnassignedCheckIds);
    }

    // КРАСНЫЙ на прежней логике. Отсутствующий обязательный контроль обязан менять итог:
    // прежняя константа не менялась ни при каком отсутствии.
    [Fact]
    public void MissingMandatoryControlRevokesTheVerdict()
    {
        var observations = SavedHonestObservations();
        observations.RemoveAll(observation => observation.CheckId == "encoder_stayed_frozen");

        var verdict = MandatoryControlFinalizer.Finalize(Assignment(), observations);

        Assert.False(verdict.MeasurementValid);
        Assert.NotEqual(MandatoryControlFinalizer.ValidLearningValidity, verdict.LearningValidity);
        Assert.Equal(MandatoryControlFinalizer.InvalidLearningValidity, verdict.LearningValidity);
        Assert.Contains("encoder_stayed_frozen", verdict.MissingCheckIds);
        Assert.Empty(verdict.UnknownCheckIds);
        Assert.Empty(verdict.FailedCheckIds);
    }

    // КРАСНЫЙ на прежней логике. Неизвестное состояние обязательного контроля обязано
    // менять итог и обязано отличаться от отказа: отсутствие записи — не успех.
    [Fact]
    public void UnknownMandatoryControlStateRevokesTheVerdictAndIsNotCalledAFailure()
    {
        var observations = SavedHonestObservations();
        var index = observations.FindIndex(observation => observation.CheckId == "panel_identity_full");
        observations[index] = observations[index] with { ObservedOutcome = null };

        var verdict = MandatoryControlFinalizer.Finalize(Assignment(), observations);

        Assert.False(verdict.MeasurementValid);
        Assert.Equal(MandatoryControlFinalizer.InvalidLearningValidity, verdict.LearningValidity);
        Assert.Contains("panel_identity_full", verdict.UnknownCheckIds);
        Assert.DoesNotContain("panel_identity_full", verdict.FailedCheckIds);
        Assert.Empty(verdict.MissingCheckIds);
    }

    // КРАСНЫЙ на прежней логике. Укороченная панель отвергнута, и общий префикс
    // совпадением не считается: прежний цикл по Math.Min пропускал ровно этот случай.
    [Fact]
    public void TruncatedPanelIsRejectedAndNoCommonPrefixCountsAsAMatch()
    {
        var left = HonestPanel();
        var right = HonestPanel();
        right.RemoveAt(1);

        var panel = PanelIdentity.Compare(left, right);

        Assert.False(panel.Identical);
        Assert.Equal(2, panel.LeftRows);
        Assert.Equal(1, panel.RightRows);
        Assert.Contains(panel.Reasons, reason => reason.Contains("число строк панели различается"));

        var observations = SavedHonestObservations();
        var index = observations.FindIndex(observation => observation.CheckId == "panel_identity_full");
        observations[index] = observations[index] with { ObservedOutcome = panel.Identical };
        var verdict = MandatoryControlFinalizer.Finalize(Assignment(), observations);

        Assert.False(verdict.MeasurementValid);
        Assert.Contains("panel_identity_full", verdict.FailedCheckIds);
    }

    // Тонкость ожидания: правильный отказ отрицательного контроля — это успех, а
    // требование «все исходные логические поля истинны» сломало бы именно его.
    [Fact]
    public void CorrectlyRejectedDamagedReceiptIsAPassedControlWhileAcceptingItFails()
    {
        var honest = SavedHonestObservations();
        var negative = honest.Single(observation => observation.CheckId == "damaged_receipt_accepted");
        Assert.False(negative.ExpectedOutcome);
        Assert.False(negative.ObservedOutcome);
        Assert.True(negative.Accepted);

        var wronglyAccepted = SavedHonestObservations();
        var index = wronglyAccepted.FindIndex(observation => observation.CheckId == "damaged_receipt_accepted");
        wronglyAccepted[index] = wronglyAccepted[index] with { ObservedOutcome = true };

        var verdict = MandatoryControlFinalizer.Finalize(Assignment(), wronglyAccepted);

        Assert.False(verdict.MeasurementValid);
        Assert.Contains("damaged_receipt_accepted", verdict.FailedCheckIds);
    }

    // Точный набор опознаний: дубликат и лишнее опознание вне назначения тоже отзывают
    // пригодность, потому что сверяется набор, а не только наличие нужных имён.
    [Fact]
    public void DuplicateAndUnassignedCheckIdsBreakTheExactSet()
    {
        var duplicated = SavedHonestObservations();
        duplicated.Add(new MandatoryControlObservation("scores_identical", true, true, "повторная запись того же контроля"));
        var duplicateVerdict = MandatoryControlFinalizer.Finalize(Assignment(), duplicated);
        Assert.False(duplicateVerdict.MeasurementValid);
        Assert.Contains("scores_identical", duplicateVerdict.DuplicateCheckIds);

        var extra = SavedHonestObservations();
        extra.Add(new MandatoryControlObservation("nice_to_have_check", true, true, "контроль, которого в назначении нет"));
        var extraVerdict = MandatoryControlFinalizer.Finalize(Assignment(), extra);
        Assert.False(extraVerdict.MeasurementValid);
        Assert.Contains("nice_to_have_check", extraVerdict.UnassignedCheckIds);
    }

    // Опознания запросов сверяются: перестановка и дубликат внутри панели отвергаются,
    // хотя число строк и состав документов прежние.
    [Fact]
    public void PermutedOrDuplicatedQueryIdIsRejected()
    {
        var left = HonestPanel();
        var permuted = new List<QueryReadout> { left[1], left[0] };
        var permutedVerdict = PanelIdentity.Compare(left, permuted);
        Assert.False(permutedVerdict.Identical);
        Assert.Contains(permutedVerdict.Reasons, reason => reason.Contains("опознание запроса различается"));

        var duplicated = new List<QueryReadout> { left[0], left[0] };
        var duplicatedVerdict = PanelIdentity.Compare(left, duplicated);
        Assert.False(duplicatedVerdict.Identical);
        Assert.Contains(duplicatedVerdict.Reasons, reason => reason.Contains("встречается"));
    }

    // Тот же выбранный документ с другим полным текстом ответа отвергается: прежнее
    // сравнение смотрело только на опознание документа и не читало сам ответ.
    [Fact]
    public void SameSelectedDocumentWithADifferentFullAnswerIsRejected()
    {
        var left = HonestPanel();
        var right = new List<QueryReadout>
        {
            Row("r1", "doc-1", "первый полный ответ", 0.75),
            Row("r2", "doc-2", "второй полный ответ, подменённый", 0.5)
        };

        var panel = PanelIdentity.Compare(left, right);

        Assert.False(panel.Identical);
        Assert.Contains(panel.Reasons, reason => reason.Contains("полный текст ответа различается"));
    }

    // Уверенность «не число» и бесконечность больше не считаются совпадением: прежде
    // сравнение по модулю разности для «не числа» было ложным и проходило молча.
    [Fact]
    public void NonFiniteConfidenceIsNotAMatch()
    {
        var left = new List<QueryReadout> { Row("r1", "doc-1", "ответ", double.NaN) };
        var right = new List<QueryReadout> { Row("r1", "doc-1", "ответ", double.NaN) };

        var notANumber = PanelIdentity.Compare(left, right);
        Assert.False(notANumber.Identical);
        Assert.Contains(notANumber.Reasons, reason => reason.Contains("не является конечным числом"));

        var infinite = PanelIdentity.Compare(
            new List<QueryReadout> { Row("r1", "doc-1", "ответ", double.PositiveInfinity) },
            new List<QueryReadout> { Row("r1", "doc-1", "ответ", double.PositiveInfinity) });
        Assert.False(infinite.Identical);
    }

    // Честные панели совпадают целиком: положительный контроль самой сверки, зелёный и
    // до, и после, чтобы строгость не превратилась в отказ на верных данных.
    [Fact]
    public void HonestPanelsAreIdenticalInFull()
    {
        var panel = PanelIdentity.Compare(HonestPanel(), HonestPanel());

        Assert.True(panel.Identical);
        Assert.Empty(panel.Reasons);
        Assert.Equal(2, panel.LeftRows);
        Assert.Equal(2, panel.RightRows);
    }
}
