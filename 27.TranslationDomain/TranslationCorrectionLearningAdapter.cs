// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Этот файл доставляет ПОДТВЕРЖДЁННОЕ исправление существующей переводческой единице.
//
// Что было сломано, по-человечески. Когда внешний ответ проверен арбитрами и признан
// годным, продукт собирает из него учебную пару: годная сторона и отвергнутая сторона
// (22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs, строка 232). Дальше эта пара
// ВОЗВРАЩАЕТСЯ вызывающему как данные — и на этом всё. Назначенная обучаемая единица
// перевода её не получала, то есть исправляющий ответ уроком не становился: модель не
// меняла своего ответа, сколько бы исправлений ей ни принесли.
//
// Что делает этот файл. Он переводит одну подтверждённую коррекцию в один урок той формы,
// которую читает СУЩЕСТВУЮЩИЙ получатель обучения, и отдаёт его в СУЩЕСТВУЮЩИЙ приёмник
// пакетного обучения структурированных примеров машины перевода
// (TranslationMachine.TrainStructuredBatchAsync). Новой памяти переводов здесь нет.
//
// Намеренные границы этого файла.
//   * Это НЕ общий механизм обратной связи. Он доставляет пару ОДНОЙ названной единице
//     перевода и ничего не знает ни о других областях, ни о других видах обратной связи.
//   * Здесь НЕТ глобального маршрутизатора и НЕТ автоматического учителя. Связь
//     пользовательского запроса с доменным получателем принадлежит другому владельцу, и
//     этот файл её не создаёт: решение «доставить» принимает вызывающая сторона.
//   * Здесь НЕТ собственной памяти, своего словаря и своего хранилища: вся запись идёт
//     через уже существующий приёмник обучения назначенной единицы.
//   * Здесь НЕТ связывания со сборкой алгоритмов агента. Конверт результата операции в
//     решении объявлен в тринадцати сборках по одному на сборку, и они не
//     взаимозаменяемы. Поэтому вход этого переходника — простые значения и перечисление
//     направления, а не чужая запись: преобразование делает вызывающая сторона явно.
//   * Повторная доставка одного опознавателя урока остаётся ЯВНО ОТКРЫТОЙ предпосылкой:
//     договор получателя на повторную запись одного опознавателя ещё не назван, а
//     молчаливое слияние по тексту вместо опознавателя делать запрещено. Свойством
//     прежнего опыта идемпотентность не является.
//
// Основание: docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 106-feedback-and-transition-memory-plan-2026-10-30.md, раздел R34-FB.

// TranslationVerifiedCorrection — одна подтверждённая коррекция ответа переводческой
// единицы. Это вход переходника: всё, что нужно, чтобы собрать из отказа настоящий урок.
public sealed record TranslationVerifiedCorrection(
    // LessonId — устойчивый опознаватель урока. Он задаётся вызывающей стороной и
    // остаётся тем же при повторной доставке того же исправления.
    string LessonId,
    // Direction — направление перевода, то есть какая именно единица учится.
    TranslationDirection Direction,
    // TopicDomain — тема запроса. Тема входит в адрес точной памяти единицы, поэтому она
    // приходит из запроса и никогда не угадывается.
    string TopicDomain,
    // SourceText — исходный запрос пользователя на языке источника.
    string SourceText,
    // ActualRejectedAnswer — ДЕЙСТВИТЕЛЬНО выданный ответ, который проверка забраковала.
    // Здесь не должно стоять ни непереведённого источника, ни диагностического кандидата:
    // отвергнутая сторона урока обязана быть тем, что продукт на самом деле ответил.
    string ActualRejectedAnswer,
    // VerifiedCorrectAnswer — проверенный правильный ответ. Без него коррекции нет.
    string VerifiedCorrectAnswer,
    // VerifiedAnswerConfidence — уверенность, которую назвала САМА проверка. Получатель
    // обучения это поле сегодня не читает, поэтому оно не объявляет уверенности
    // исполнения; постоянную величину из учебной оснастки прежнего опыта сюда
    // подставлять нельзя.
    double VerifiedAnswerConfidence,
    // VerificationProvenance — происхождение проверки человеческими словами: кто
    // забраковал ответ и по какой названной причине.
    string VerificationProvenance);

// TranslationCorrectionLearningAdapter — ограниченный переходник подтверждённой
// коррекции к партии обучения назначенной переводческой единицы.
public sealed class TranslationCorrectionLearningAdapter
{
    // CorrectionRequiredErrorCode — коррекции нет вовсе.
    public const string CorrectionRequiredErrorCode = "CORRECTION_REQUIRED";

    // LessonIdRequiredErrorCode — у урока нет устойчивого опознавателя.
    public const string LessonIdRequiredErrorCode = "CORRECTION_LESSON_ID_REQUIRED";

    // SourceTextRequiredErrorCode — нет исходного запроса, значит нет и адреса урока.
    public const string SourceTextRequiredErrorCode = "CORRECTION_SOURCE_TEXT_REQUIRED";

    // WithoutVerifiedAnswerErrorCode — отказ проверки БЕЗ проверенного правильного
    // текста. Это незавершённая коррекция, и урока из неё нет.
    public const string WithoutVerifiedAnswerErrorCode = "CORRECTION_WITHOUT_VERIFIED_ANSWER";

    // RejectedAnswerRequiredErrorCode — не назван фактически выданный отвергнутый ответ.
    public const string RejectedAnswerRequiredErrorCode = "CORRECTION_REJECTED_ANSWER_REQUIRED";

    // ProvenanceRequiredErrorCode — не названо происхождение проверки.
    public const string ProvenanceRequiredErrorCode = "VERIFICATION_PROVENANCE_REQUIRED";

    // RejectedEqualsVerifiedErrorCode — отвергнутая сторона равна проверенной правильной,
    // то есть исправлять нечего и урока из этого нет.
    public const string RejectedEqualsVerifiedErrorCode = "REJECTED_EQUALS_VERIFIED_ANSWER";

    // RejectedAnswerSubstitutedBySourceErrorCode — вместо выданного ответа подставлен
    // непереведённый источник. Именно такая подмена стояла в учебном построителе прежнего
    // опыта, и получатель обучения эту сторону не читает вовсе, поэтому молча принять её
    // значит записать урок с ложным происхождением.
    public const string RejectedAnswerSubstitutedBySourceErrorCode = "REJECTED_ANSWER_SUBSTITUTED_BY_SOURCE";

    // UnknownReceiverErrorCode — у этой машины нет получателя обучения с таким
    // опознавателем единицы. Приёмник партий такую строку молча пропускает и объявляет
    // партию успешной, поэтому известность получателя проверяется ДО записи.
    public const string UnknownReceiverErrorCode = "UNKNOWN_LEARNING_RECEIVER";

    // LessonNotAppliedErrorCode — партия принята, но назначенная единица в числе
    // продвинутых не названа: урок до получателя не дошёл.
    public const string LessonNotAppliedErrorCode = "CORRECTION_LESSON_NOT_APPLIED";

    // VerifiedCorrectionSourceId — устойчивое имя источника учебных строк этого
    // переходника. Оно попадает в опознания обученных строк слепка состояния, поэтому по
    // сохранённому состоянию видно, что урок пришёл из подтверждённой коррекции.
    public const string VerifiedCorrectionSourceId = "VERIFIED-CORRECTION";

    // RejectedTypeCategory — короткое название рода отказа отвергнутой стороны.
    private const string RejectedTypeCategory = "verified_external_correction";

    // LessonJson — настройки записи учебных текстов. Перечисления пишутся словами ровно
    // так, как их читает получатель обучения, иначе разбор у него откажет.
    private static readonly JsonSerializerOptions LessonJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // BuildLesson собирает ОДИН урок из подтверждённой коррекции и НИЧЕГО не записывает.
    // Весь допуск стоит здесь, то есть до любой записи в память единицы.
    public OperationResult<TranslationLearningExample> BuildLesson(TranslationVerifiedCorrection correction)
    {
        // Без самой коррекции собирать нечего.
        if (correction is null)
            return OperationResult<TranslationLearningExample>.Fail(
                "Подтверждённая коррекция обязательна.", CorrectionRequiredErrorCode);

        // Без устойчивого опознавателя урок нельзя будет ни узнать, ни доставить повторно.
        if (string.IsNullOrWhiteSpace(correction.LessonId))
            return OperationResult<TranslationLearningExample>.Fail(
                "У подтверждённой коррекции нет устойчивого опознавателя урока.", LessonIdRequiredErrorCode);

        // Без исходного запроса у урока нет адреса: по нему строится и точная память, и
        // структурная подпись.
        if (string.IsNullOrWhiteSpace(correction.SourceText))
            return OperationResult<TranslationLearningExample>.Fail(
                "У подтверждённой коррекции нет исходного запроса.", SourceTextRequiredErrorCode);

        // Один отказ проверки БЕЗ проверенного правильного текста урока не создаёт. Это
        // незавершённая коррекция, а не отрицательный урок.
        if (string.IsNullOrWhiteSpace(correction.VerifiedCorrectAnswer))
            return OperationResult<TranslationLearningExample>.Fail(
                $"Коррекция '{correction.LessonId}' не несёт проверенного правильного ответа: " +
                "это незавершённая коррекция, и урока из неё нет.",
                WithoutVerifiedAnswerErrorCode);

        // Отвергнутая сторона обязана быть названа: без неё происхождение урока неизвестно.
        if (string.IsNullOrWhiteSpace(correction.ActualRejectedAnswer))
            return OperationResult<TranslationLearningExample>.Fail(
                $"Коррекция '{correction.LessonId}' не называет фактически выданного отвергнутого ответа.",
                RejectedAnswerRequiredErrorCode);

        // Происхождение проверки обязано быть названо: иначе урок нельзя будет отличить от
        // неподтверждённой правки.
        if (string.IsNullOrWhiteSpace(correction.VerificationProvenance))
            return OperationResult<TranslationLearningExample>.Fail(
                $"Коррекция '{correction.LessonId}' не называет происхождения проверки.",
                ProvenanceRequiredErrorCode);

        // Подмена первого рода: отвергнутая сторона совпала с проверенной правильной.
        // Исправлять нечего, и урок здесь был бы ложью о происхождении.
        if (TextsMatch(correction.ActualRejectedAnswer, correction.VerifiedCorrectAnswer))
            return OperationResult<TranslationLearningExample>.Fail(
                $"В коррекции '{correction.LessonId}' отвергнутый ответ совпадает с проверенным " +
                "правильным: исправлять нечего, и урока из этого нет.",
                RejectedEqualsVerifiedErrorCode);

        // Подмена второго рода: вместо выданного ответа подставлен непереведённый источник.
        if (TextsMatch(correction.ActualRejectedAnswer, correction.SourceText))
            return OperationResult<TranslationLearningExample>.Fail(
                $"В коррекции '{correction.LessonId}' на месте отвергнутого ответа стоит непереведённый " +
                "источник, а не действительно выданный ответ.",
                RejectedAnswerSubstitutedBySourceErrorCode);

        // Структурная подпись берётся у СУЩЕСТВУЮЩЕГО продуктового разборщика того же
        // текста: под этим адресом урок и будет искать чтение.
        var sequence = new TranslationSemanticTokenizer()
            .Tokenize(correction.SourceText, SourceLanguageOf(correction.Direction));

        // Вход урока в той форме, которую разбирает получатель обучения.
        var input = new ExecutorInput(
            correction.SourceText,
            SourceLanguageOf(correction.Direction),
            correction.TopicDomain);

        // Годная сторона урока: проверенный правильный ответ и уверенность САМОЙ проверки.
        // Словарных и фразовых соответствий коррекция не несёт, и выдумывать их нельзя.
        var chosen = new ExecutorOutput(
            correction.VerifiedCorrectAnswer,
            correction.VerifiedAnswerConfidence,
            null,
            null);

        // Отвергнутая сторона урока: ДЕЙСТВИТЕЛЬНО выданный ответ, дословно. Уверенность
        // здесь ноль, потому что названного числа у забракованного ответа нет, а получатель
        // обучения это поле не читает вовсе; выдумывать число ради красивой записи нельзя.
        var rejected = new ExecutorOutput(correction.ActualRejectedAnswer, 0.0, null, null);

        return OperationResult<TranslationLearningExample>.Ok(new TranslationLearningExample(
            // Опознаватель урока приходит от вызывающей стороны и не переписывается.
            ExampleId: correction.LessonId,
            // Получатель урока — исполнительная единица названного направления.
            UnitId: TranslationUnitIds.ExecutorFor(correction.Direction),
            // Поток тот же, в котором эта единица отвечает на запросы.
            FlowId: TranslationRoutingFlow.FlowId,
            // Вход, годная и отвергнутая сторона — текстом, как их читает получатель.
            InputJson: JsonSerializer.Serialize(input, LessonJson),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, LessonJson),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, LessonJson),
            // Род отказа несёт и короткое название, и человеческое происхождение проверки:
            // по сохранённому уроку видно, кто и почему забраковал прежний ответ.
            RejectedType: $"{RejectedTypeCategory}: {correction.VerificationProvenance}",
            // Рабочий ключ — подпись существующего разборщика, без отрезков происхождения.
            SemanticSignature: sequence.Signature,
            // Источник учебной строки: подтверждённая коррекция, а не корпус.
            SourceId: VerifiedCorrectionSourceId,
            // Урок обучающий, а не отложенный на измерение.
            HeldOut: false,
            // Номер учебной пары у коррекции отсутствует: её происхождение — проверка
            // ответа, а не строка корпуса. Поле остаётся пустым, а не заполняется догадкой.
            PairId: null,
            // Форму ключа переходник НЕ объявляет: подпись пришла прямо от продуктового
            // разборщика, и приёмник выведет форму из самих данных. Объявить версию здесь
            // значило бы утверждать о ключе то, что переходник не проверял.
            KeySchemaVersion: null));
    }

    // DeliverAsync доставляет подтверждённую коррекцию назначенной единице через её
    // СОБСТВЕННЫЙ существующий приёмник пакетного обучения структурированных примеров.
    public async Task<OperationResult<LearningBatchResult>> DeliverAsync(
        TranslationMachine machine,
        TranslationVerifiedCorrection correction,
        CancellationToken ct = default)
    {
        // Без машины перевода доставлять некуда.
        if (machine is null)
            return OperationResult<LearningBatchResult>.Fail(
                "Машина перевода обязательна для доставки коррекции.", CorrectionRequiredErrorCode);

        // Весь допуск коррекции проходит ДО любой записи в память единицы.
        var lesson = BuildLesson(correction);
        // Отказ допуска отдаётся как есть: ни одной записи при этом не делалось.
        if (!lesson.IsSuccess)
            return lesson.MapError<LearningBatchResult>();

        // Известность получателя проверяется ТОЖЕ до записи. Приёмник партий строку с
        // неизвестным получателем молча пропускает и объявляет партию успешной, поэтому
        // без этой проверки «доставка» прошла бы, не обучив ничего.
        if (!machine.HasStructuredTrainerFor(lesson.Value!.UnitId))
            return OperationResult<LearningBatchResult>.Fail(
                $"У этой машины перевода нет получателя обучения '{lesson.Value.UnitId}', поэтому " +
                $"коррекция '{correction.LessonId}' не доставлена: записи не делалось.",
                UnknownReceiverErrorCode);

        // Партия из ровно одной учебной строки: переходник доставляет одну коррекцию.
        var batch = new TranslationLearningBatch(
            BatchId: $"{VerifiedCorrectionSourceId}-{correction.LessonId}",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [lesson.Value],
            SourceIds: [VerifiedCorrectionSourceId]);

        // Запись идёт ТОЛЬКО через существующий приёмник обучения машины перевода.
        var trained = await machine.TrainStructuredBatchAsync(batch, ct);
        // Отказ приёмника отдаётся вызывающей стороне своим кодом без переписывания.
        if (!trained.IsSuccess)
            return trained;

        // Последняя сверка по факту: назначенная единица обязана быть названа в числе
        // продвинутых. Принятая партия без продвинутой единицы означает, что урок до
        // получателя не дошёл, и называть такую доставку успешной нельзя.
        if (!trained.Value!.PromotedUnitIds.Contains(lesson.Value.UnitId, StringComparer.Ordinal))
            return OperationResult<LearningBatchResult>.Fail(
                $"Партия коррекции '{correction.LessonId}' принята, но единица " +
                $"'{lesson.Value.UnitId}' в числе продвинутых не названа: урок до получателя не дошёл.",
                LessonNotAppliedErrorCode);

        return trained;
    }

    // TextsMatch сравнивает два текста как ответы человека: без краевых пробелов и без
    // различения регистра. Это сравнение ТОЛЬКО для допуска подмены; молчаливого слияния
    // уроков по тексту вместо опознавателя здесь нет.
    private static bool TextsMatch(string left, string right)
        => string.Equals(
            (left ?? string.Empty).Trim(),
            (right ?? string.Empty).Trim(),
            StringComparison.OrdinalIgnoreCase);

    // SourceLanguageOf называет язык источника названного направления двухбуквенным кодом.
    // Выбор записан перечислением поимённо, а не разбором имени перечисления: имя — это
    // текст для человека, и строить на нём поведение продукта нельзя.
    private static string SourceLanguageOf(TranslationDirection direction)
        => direction switch
        {
            // Русский источник.
            TranslationDirection.RU_EN => "ru",
            // Английский источник.
            TranslationDirection.EN_RU => "en",
            // Иврит как источник.
            TranslationDirection.HE_EN => "he",
            // Английский источник для иврита.
            TranslationDirection.EN_HE => "en",
            // Испанский источник.
            TranslationDirection.ES_EN => "es",
            // Английский источник для испанского.
            TranslationDirection.EN_ES => "en",
            // Неизвестное направление языка источника не называет: догадка здесь была бы
            // ложью о данных, и разборщик получил бы чужой язык.
            _ => string.Empty
        };
}
