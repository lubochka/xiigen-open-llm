// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие приёма имён уроков обучаемым блоком ближайших соседей
// (работа R1 плана ремонта 102 из папки docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments, находка A29L-01 аудита 101 от 29 октября).
//
// Что здесь проверяется по-человечески. Блок ближайших соседей запоминает уроки
// под именами. Имя урока — это ключ памяти: по нему словарь примеров хранит
// правильный ответ, и то же имя получает вектор урока в указателе соседей. Если
// два разных урока получают одно имя, память перестаёт быть однозначной.
//
// Что было сломано. Имя обрезалось от пробелов только в момент записи, а
// уникальность не проверялась вовсе. Урок «alpha → A» с именем " same-id " и
// урок «beta → B» с именем "same-id" после обрезки получали одно имя. Словарь
// примеров заменял первую запись второй, а указатель соседей сохранял ОБА
// вектора. Обучение завершалось успешно: принято две строки, а счётчик в
// паспорте равнялся одному. Запрос «alpha» находил свой СОБСТВЕННЫЙ точный
// вектор и читал по общему имени ЧУЖОЙ ответ «B» с уверенностью 1,0. Порог
// уверенности такую ошибку не ловит: вектор найден точный, уверенность
// наибольшая, неверен только ответ.
//
// Что стало. Партия с повторяющимся нормализованным именем отвергается целиком
// ДО обучения токенизатора и ДО построения указателя соседей, одним устойчивым
// кодом отказа. Политика «побеждает последний» сознательно НЕ выбрана: она
// потребовала бы одновременно заменять вектор, ответ и счётчик паспорта.
// После успешного обучения держится инвариант: принятые строки = уникальные
// нормализованные имена = записи указателя соседей = счётчик паспорта.
//
// Что считается нормализацией имени. Ровно две вещи, и ничего больше:
// обрезка пробельных знаков по краям (String.Trim) и побайтное сравнение
// остатка (StringComparer.Ordinal). Проверено на действующей площадке .NET:
// Trim снимает обычный пробел U+0020, табуляцию U+0009 и неразрывный пробел
// U+00A0, потому что для них char.IsWhiteSpace истинно; Trim НЕ снимает
// невидимые U+200B и U+FEFF, потому что для них char.IsWhiteSpace ложно.
// Ordinal различает регистр. Значит: имена, отличающиеся только краевыми
// пробельными знаками, — это ОДИН урок и отказ; имена, отличающиеся регистром
// или невидимым знаком, — это РАЗНЫЕ уроки и обычный приём. Регистронезависимое
// сравнение здесь намеренно не вводится: это была бы смена договора.
//
// Бюджет этого покрытия. Четыре успешных обучения очень малых наборов (от двух
// до трёх строк), несколько чтений ответа и пять отвергнутых попыток обучения,
// которые останавливаются до токенизатора. Ни одного обучения с учителем, ни
// одной пары предпочтений, ни одной записи на диск. Прежние обученные состояния
// не читаются и не перезаписываются.
//
// Чего это покрытие НЕ доказывает. Оно не меняет смысл успешного обучения:
// обучение по-прежнему создаёт новый блок на полном наборе, а не дописывает
// уроки к прежнему. Оно не доказывает откат общего хранилища при отказе —
// здесь прежний блок является отдельным объектом, и проверка показывает лишь,
// что отвергнутая партия его не трогает; сохранение памяти в общем хранилище —
// предмет отдельной работы R4. Оно не касается владения изменяемым ответом —
// это отдельная работа R2. Оно не проверяет качество поиска приближённых
// соседей и не распространяется на соседние блоки другого устройства.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class KnnExampleIdentityAdmissionTests
{
    // Воспроизведение дефекта из аудита: два разных урока под одним именем после
    // обрезки. Партия отвергается до обучения, а прежде обученный блок продолжает
    // отвечать ровно как раньше.
    [Fact]
    public async Task DuplicateIdAfterTrim_IsRefusedBeforeTokenizer_AndEarlierMemoryKeepsAnswering()
    {
        // Прежняя память: обычный исправный блок на двух уникальных именах.
        var earlier = TrainTwoLessons("lesson-alpha", "lesson-beta");
        // Обучение прежней памяти обязано пройти обычным конвертом результата.
        Assert.True(earlier.IsSuccess, earlier.ErrorMessage);
        // Этот же объект будет опрошен ещё раз после отвергнутой партии.
        var earlierUnit = earlier.Value!;

        // Прежняя память отвечает правильно до попытки обучить неоднозначную партию.
        Assert.Equal("A", await Ask(earlierUnit, "alpha"));
        Assert.Equal("B", await Ask(earlierUnit, "beta"));

        // Счётчик обращений к выбору текста: его спрашивает только дорогая подготовка.
        var textSelectorCalls = 0;
        // Тот же выбор текста, но с подсчётом обращений.
        string CountingSelector(string input)
        {
            textSelectorCalls++;
            return input;
        }

        // Ровно тот набор, который аудит сохранил как воспроизведение дефекта:
        // имя первого урока записано с пробелами по краям, второго — без них.
        var ambiguous = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-duplicate-after-trim",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    " same-id ", "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    "same-id", "beta", "B", ["BETA"], ["test-domain"], ["lesson-beta"])
            ],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Партия отвергнута обычным конвертом результата, а не исключением.
        Assert.False(ambiguous.IsSuccess);
        // Код отказа — тот устойчивый код, который назначен этой работой.
        Assert.Equal("TRAINING_EXAMPLE_ID_DUPLICATE", ambiguous.ErrorCode);
        // Обученного блока в отвергнутом результате нет.
        Assert.Null(ambiguous.Value);
        // Доказательство раннего отказа: выбор текста нужен только корпусу
        // токенизатора и вектору урока, и его не спросили ни разу.
        Assert.Equal(0, textSelectorCalls);

        // Прежняя память жива и отвечает теми же буквальными значениями.
        Assert.Equal("A", await Ask(earlierUnit, "alpha"));
        Assert.Equal("B", await Ask(earlierUnit, "beta"));
        // Счётчик паспорта прежней памяти тоже не изменился.
        Assert.Equal(2, earlierUnit.Manifest.TrainingExampleCount);
    }

    // Точный повтор имени без всяких пробелов отвергается так же рано.
    [Fact]
    public void ExactDuplicateId_WithDifferentLessons_IsRefusedBeforeTokenizer()
    {
        // Счётчик обращений к выбору текста доказывает место отказа.
        var textSelectorCalls = 0;
        // Выбор текста с подсчётом обращений.
        string CountingSelector(string input)
        {
            textSelectorCalls++;
            return input;
        }

        // Два разных урока записаны под буквально одинаковым именем.
        var exactDuplicate = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-exact-duplicate",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "same-id", "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    "same-id", "beta", "B", ["BETA"], ["test-domain"], ["lesson-beta"])
            ],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом с тем же устойчивым кодом.
        Assert.False(exactDuplicate.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_ID_DUPLICATE", exactDuplicate.ErrorCode);
        // Отказ случился до подготовки токенизатора и указателя соседей.
        Assert.Equal(0, textSelectorCalls);
    }

    // Повтор имени с тем же самым ответом тоже отвергается: политика имён общая,
    // а не «отказ только когда ответы различаются». Молчаливое слияние двух строк
    // в одну скрыло бы от вызывающей стороны, что один урок потерян.
    [Fact]
    public void RepeatedIdWithTheSameOutput_IsRefusedByTheSameIdentityPolicy()
    {
        // Два урока с одинаковым именем И одинаковым ответом.
        var repeated = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-repeated-same-output",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "same-id", "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    "same-id", "alpha vtoroy raz", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Общая политика имён: отказ, а не случайное молчаливое слияние.
        Assert.False(repeated.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_ID_DUPLICATE", repeated.ErrorCode);
    }

    // Положительный контроль из аудита: при уникальных именах каждый запрос
    // получает СВОЙ ответ, а не ответ соседнего урока.
    [Fact]
    public async Task UniqueIds_AreAccepted_AndEachQueryGetsItsOwnAnswer()
    {
        // Те же два урока, но под разными именами.
        var trained = TrainTwoLessons("lesson-alpha", "lesson-beta");
        // Обучение обязано пройти обычным конвертом результата.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        var unit = trained.Value!;

        // Каждый запрос получает собственный ответ; ожидания выписаны буквально.
        Assert.Equal("A", await Ask(unit, "alpha"));
        Assert.Equal("B", await Ask(unit, "beta"));
        // Счётчик паспорта равен числу принятых строк, а не единице.
        Assert.Equal(2, unit.Manifest.TrainingExampleCount);
    }

    // Инвариант после успешного обучения: принятые строки, уникальные имена,
    // записи указателя соседей и счётчик паспорта описывают одно и то же число.
    [Fact]
    public void AcceptedBatch_KeepsAcceptedRowsUniqueIdsIndexEntriesAndManifestCountEqual()
    {
        // Три строки с уникальными именами: число заведомо отличается от единицы.
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-counts-agree",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "lesson-alpha", "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    "lesson-beta", "beta", "B", ["BETA"], ["test-domain"], ["lesson-beta"]),
                new LogicalKnnTrainingExample<string, string>(
                    "lesson-gamma", "gamma", "G", ["GAMMA"], ["test-domain"], ["lesson-gamma"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Успех здесь сам по себе является доказательством согласованности:
        // обучение возвращает блок только после сверки всех четырёх носителей.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // Счётчик паспорта виден снаружи и равен числу принятых строк.
        Assert.Equal(3, trained.Value!.Manifest.TrainingExampleCount);

        // Та же сверка вызывается напрямую на согласованной четвёрке: она обязана
        // принять её и вернуть согласованное число уроков.
        var agreed = KnnLogicalUnit<string, string>.VerifyTrainedMemoryConsistency(3, 3, 3, 3);
        Assert.True(agreed.IsSuccess, agreed.ErrorMessage);
        Assert.Equal(3, agreed.Value);
    }

    // Отрицательный контроль самой сверки: если любой из четырёх носителей
    // рассогласован, сверка обязана это поймать, а не промолчать.
    [Fact]
    public void DesynchronizedCarrier_IsCaughtByTheConsistencyCheck()
    {
        // Рассогласован счётчик паспорта: ровно та форма, которую зафиксировал
        // аудит на сломанном коде — принято две строки, в паспорте одна.
        var manifestOutOfStep = KnnLogicalUnit<string, string>.VerifyTrainedMemoryConsistency(2, 2, 2, 1);
        Assert.False(manifestOutOfStep.IsSuccess);
        Assert.Equal("TRAINED_MEMORY_COUNT_MISMATCH", manifestOutOfStep.ErrorCode);

        // Рассогласован словарь примеров: две строки, но одно уникальное имя —
        // исходная причина дефекта, когда вторая запись заменяла первую.
        var storedExamplesOutOfStep = KnnLogicalUnit<string, string>.VerifyTrainedMemoryConsistency(2, 1, 2, 1);
        Assert.False(storedExamplesOutOfStep.IsSuccess);
        Assert.Equal("TRAINED_MEMORY_COUNT_MISMATCH", storedExamplesOutOfStep.ErrorCode);

        // Рассогласован указатель соседей: лишний вектор без своего урока.
        var indexOutOfStep = KnnLogicalUnit<string, string>.VerifyTrainedMemoryConsistency(2, 2, 3, 2);
        Assert.False(indexOutOfStep.IsSuccess);
        Assert.Equal("TRAINED_MEMORY_COUNT_MISMATCH", indexOutOfStep.ErrorCode);

        // Сообщение об отказе называет все четыре числа, чтобы расхождение
        // читалось без повторного воспроизведения партии.
        Assert.Contains("accepted rows 2", manifestOutOfStep.ErrorMessage);
        Assert.Contains("unique normalized ids 2", manifestOutOfStep.ErrorMessage);
        Assert.Contains("vector index entries 2", manifestOutOfStep.ErrorMessage);
        Assert.Contains("manifest training example count 1", manifestOutOfStep.ErrorMessage);
    }

    // Граница нормализации, часть первая: имена, отличающиеся ТОЛЬКО краевыми
    // пробельными знаками, — это один урок. Проверяются разные виды пробельного
    // знака, а не только обычный пробел.
    [Fact]
    public void IdsDifferingOnlyByEdgeWhitespaceKinds_AreTheSameLesson_AndRefused()
    {
        // Табуляция по краям против неразрывного пробела по краям: для обоих
        // знаков char.IsWhiteSpace истинно, поэтому Trim снимает оба и остаётся
        // одно и то же имя.
        var trimmedToTheSameName = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-whitespace-kinds",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "\tsame-id\t", "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    " same-id ", "beta", "B", ["BETA"], ["test-domain"], ["lesson-beta"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Одно имя после обрезки — значит отказ, как и для обычных пробелов.
        Assert.False(trimmedToTheSameName.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_ID_DUPLICATE", trimmedToTheSameName.ErrorCode);
    }

    // Граница нормализации, часть вторая: имена, отличающиеся регистром или
    // невидимым знаком, который Trim НЕ снимает, — это разные уроки. Здесь
    // обучение обязано пройти, а не отказать: иначе проверка молча расширила бы
    // договор до регистронезависимого или до чистки невидимых знаков.
    [Fact]
    public void IdsDifferingByCaseOrByNonTrimmedInvisibleCharacter_AreDifferentLessons()
    {
        // Регистр: Ordinal различает «L» и «l», значит это два разных имени.
        var caseDiffers = TrainTwoLessons("Lesson-Alpha", "lesson-alpha");
        // Партия принята: регистронезависимое сравнение здесь не действует.
        Assert.True(caseDiffers.IsSuccess, caseDiffers.ErrorMessage);
        Assert.Equal(2, caseDiffers.Value!.Manifest.TrainingExampleCount);

        // Невидимый знак нулевой ширины U+200B: char.IsWhiteSpace для него ложно,
        // поэтому Trim его НЕ снимает и имена остаются разными.
        var zeroWidthDiffers = TrainTwoLessons("lesson-alpha​", "lesson-alpha");
        // Партия принята: чистку невидимых знаков этот договор не обещает.
        Assert.True(zeroWidthDiffers.IsSuccess, zeroWidthDiffers.ErrorMessage);
        Assert.Equal(2, zeroWidthDiffers.Value!.Manifest.TrainingExampleCount);
    }

    // Один и тот же малый набор из двух уроков под задаваемыми именами:
    // урок «alpha → A» и урок «beta → B».
    private static OperationResult<KnnLogicalUnit<string, string>> TrainTwoLessons(
        string firstId,
        string secondId)
    {
        // Обучение идёт обычным путём, без каких-либо особых настроек.
        return KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-admission",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    firstId, "alpha", "A", ["ALPHA"], ["test-domain"], ["lesson-alpha"]),
                new LogicalKnnTrainingExample<string, string>(
                    secondId, "beta", "B", ["BETA"], ["test-domain"], ["lesson-beta"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);
    }

    // Одно обычное чтение обученного блока на заданном входе.
    private static async Task<string> Ask(KnnLogicalUnit<string, string> unit, string input)
    {
        // Запрос повторяет учебный вход; условия запроса намеренно не передаются,
        // чтобы ответ опирался на слой слов, а не на подсказку в признаках.
        var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-identity-admission",
            "unit-knn-identity-admission",
            "test-classifier",
            "test-domain",
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // Чтение обязано пройти обычным конвертом результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        // Наружу отдаётся только сам ответ: его и сверяет вызывающая проверка.
        return result.Value!.Output;
    }
}
