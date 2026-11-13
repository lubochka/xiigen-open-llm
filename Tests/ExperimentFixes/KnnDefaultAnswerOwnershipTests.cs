// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Покрытие РАЗРЕШИТЕЛЯ ВЛАДЕНИЯ ПО УМОЛЧАНИЮ у обучаемого блока ближайших
// соседей (работа C02 адресного плана ремонта 105, аудит 30 октября).
//
// Что здесь проверяется по-человечески. У обучения блока ближайших соседей есть
// две двери. Через первую вызывающая сторона сама передаёт политику владения
// ответом. Через вторую — прежнюю, удобную — она политику не передаёт, и блок
// выбирает политику сам. Соседняя работа уже закрыла первую дверь: на
// изменяемом ответе без подходящей политики обучение отказывает. Вторая дверь
// до этой работы оставалась открытой: на любой незнакомый тип ответа блок молча
// выбирал «владение объявлено общим» и обучался. Память и вызывающая сторона
// оставались на одной ссылке, и получатель ответа мог переписать обученную
// память, не обучая её.
//
// Что требуется от нового поведения. Разрешитель по умолчанию принимает только
// явно поддержанные неизменяемые типы ответа. Незнакомый изменяемый тип
// получает конкретный именованный отказ ДО того, как память вообще построена:
// до словаря кусочков слов, до указателя соседей и до выдачи обученного блока
// наружу. Режим общего доступа остаётся доступным, но его теперь называет сам
// вызывающий, и он по-прежнему не считается успешным владением.
//
// Как это доказывается. Счётчик обращений к выбору текста равен нулю: выбор
// текста зовёт только дорогая подготовка словаря и вектора урока, поэтому ноль
// означает, что отказ случился раньше памяти. Обученный блок наружу не выдан
// вовсе. Положительные случаи читают память ПОСЛЕ нападения и без повторного
// обучения. Отдельная проверка-негативный контроль показывает, что оракул
// владения действительно ловит подмену ссылки, а не проходит сам собой.
//
// Бюджет этих проверок. Малые фиксированные наборы: один-два урока на обучение.
// Обучения с учителем — ноль, пар предпочтений — ноль, записи на диск — ноль.
//
// Чего это покрытие НЕ доказывает. Оно не улучшает точность ближайших соседей:
// это договор владения, а не качество ответов. Оно не делает безопасным любой
// изменяемый тип ответа — оно лишь запрещает молчаливое согласие на него. Оно
// ничего не говорит о блоке ближайших соседей на вложениях преобразователя
// (TransformerEmbeddingKnnLogicalUnit) и о пути сохранения модели на диск,
// которого у обычного блока сегодня нет.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class KnnDefaultAnswerOwnershipTests
{
    // Именованный код отказа РАЗРЕШИТЕЛЯ ПО УМОЛЧАНИЮ: именно он доказывает,
    // что закрыта вторая дверь, а не уже закрытая первая.
    private const string DefaultPolicyRefusalCode = "KNN_ANSWER_SNAPSHOT_DEFAULT_POLICY_REQUIRED";

    // Именованный код отказа ВСТРОЕННОЙ политики для неизменяемых ответов,
    // применённой вызывающей стороной к неподходящему типу. Это другой случай.
    private const string ImmutablePolicyMismatchCode = "KNN_ANSWER_SNAPSHOT_POLICY_REQUIRED";

    // Именованный код отказа на пустой политике. Это третий, тоже отдельный случай.
    private const string NullPolicyRefusalCode = "ANSWER_SNAPSHOT_REQUIRED";

    // Имя урока, который обязан находиться на учебном вопросе ниже.
    private const string ExpectedLessonId = "pair-security";

    // Буквально ожидаемая точка назначения сохранённого ответа.
    private const string ExpectedDestination = "SECURITY";

    // Буквально ожидаемый отпечаток сохранённого ответа целиком.
    private const string ExpectedFingerprint = "SECURITY|validate-token>check-scope>return-verdict";

    // Учебный вопрос повторяет текст урока-победителя.
    private const string Question = "jwt oauth authorization token";

    // ПРИЁМКА 1. Прежняя публичная перегрузка без политики на изменяемом объекте
    // передачи данных обязана отказать ДО создания памяти.
    [Fact]
    public void DefaultOverloadWithMutableAnswer_IsRefusedBeforeAnyMemoryIsBuilt()
    {
        // Счётчик обращений к выбору текста: его зовёт только дорогая подготовка
        // словаря кусочков слов и вектора урока. Ноль докажет отказ до памяти.
        var textSelectorCalls = 0;

        // Тот же выбор текста, что и в обычном обучении, но с подсчётом обращений.
        string CountingSelector(string input)
        {
            // Каждое обращение означает, что подготовка памяти уже началась.
            textSelectorCalls++;
            // Для этого набора входом служит сам учебный текст.
            return input;
        }

        // Обучение прежней перегрузкой: политика владения не передана вовсе.
        var trained = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-mutable",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer(ExpectedDestination, ["validate-token"]))],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом результата, а не исключением.
        Assert.False(trained.IsSuccess);

        // Код отказа принадлежит именно разрешителю по умолчанию.
        Assert.Equal(DefaultPolicyRefusalCode, trained.ErrorCode);

        // Сообщение называет сам тип ответа, чтобы было видно, что чинить.
        Assert.Contains(nameof(RouteAnswer), trained.ErrorMessage);

        // Сообщение называет обе законные двери: типизированную копию и явно
        // объявленный общий доступ. Иначе вызывающая сторона останется в тупике.
        Assert.Contains(nameof(KnnAnswerSnapshot.FromCopy), trained.ErrorMessage);
        Assert.Contains(nameof(KnnAnswerSnapshot.DeclaredShared), trained.ErrorMessage);

        // Память не строилась: выбор текста не вызывался ни разу.
        Assert.Equal(0, textSelectorCalls);

        // Обученный блок наружу не выдан вовсе: публиковать было нечего.
        Assert.Null(trained.Value);
    }

    // ПРИЁМКА 1, вторая грань. Отказ распространяется на ЛЮБОЙ незнакомый
    // изменяемый тип ответа, а не только на один подобранный объект передачи
    // данных. Массив строк — самый частый молчаливо разделяемый ответ.
    [Fact]
    public void DefaultOverloadWithMutableArrayAnswer_IsRefusedBeforeAnyMemoryIsBuilt()
    {
        // Счётчик обращений к выбору текста доказывает момент отказа.
        var textSelectorCalls = 0;

        // Выбор текста с подсчётом обращений.
        string CountingSelector(string input)
        {
            // Любое обращение означало бы начавшуюся подготовку памяти.
            textSelectorCalls++;
            // Входом служит сам учебный текст.
            return input;
        }

        // Обучение прежней перегрузкой с ответом-массивом и без политики.
        var trained = KnnLogicalUnit<string, string[]>.Train(
            "unit-knn-default-ownership-array",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string[]>(
                    ExpectedLessonId,
                    Question,
                    ["validate-token", "check-scope"],
                    ["SECURITY"],
                    ["security"],
                    [ExpectedLessonId])
            ],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит тем же конвертом результата.
        Assert.False(trained.IsSuccess);

        // Код отказа принадлежит разрешителю по умолчанию, а не другой проверке.
        Assert.Equal(DefaultPolicyRefusalCode, trained.ErrorCode);

        // Память не строилась: выбор текста не вызывался ни разу.
        Assert.Equal(0, textSelectorCalls);

        // Обученный блок наружу не выдан.
        Assert.Null(trained.Value);
    }

    // ПРИЁМКА 2, неизменяемая половина. Прежняя перегрузка остаётся удобной для
    // неизменяемого ответа: строка обучается как раньше и лишней копии не несёт.
    [Fact]
    public async Task DefaultOverloadWithImmutableStringAnswer_KeepsWorkingWithoutExtraCopy()
    {
        // Буквально ожидаемый ответ урока.
        const string expectedAnswer = ExpectedDestination;

        // Та же строка, собранная во время выполнения: она не попадает в общий
        // пул литералов, поэтому совпадение ССЫЛОК ниже действительно доказывает
        // отсутствие копии, а не совпадение двух одинаковых литералов.
        var answerText = new string(expectedAnswer.ToCharArray());

        // Проверка самой заготовки: это другой объект с тем же содержимым.
        Assert.NotSame(expectedAnswer, answerText);

        // Обучение прежней перегрузкой без политики: строка заведомо неизменяема.
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-knn-default-ownership-string",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    ExpectedLessonId, Question, answerText,
                    ["SECURITY", "AUTHORIZATION"], ["security"], [ExpectedLessonId]),
                new LogicalKnnTrainingExample<string, string>(
                    "pair-ui", "layout screen accessibility design", "UI",
                    ["UI"], ["ui"], ["pair-ui"])
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Обучение обязано пройти через общий конверт результата.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Опись модели называет состояние владения своим именем: память владеет.
        Assert.Contains("knn-answer-snapshot:known-immutable", trained.Value!.Manifest.IngredientIds);

        // Обычное чтение обученного блока на учебном вопросе.
        var result = await trained.Value.ExecuteAsync(BuildRequest("unit-knn-default-ownership-string"));

        // Чтение обязано пройти через общий конверт результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Ответ тот же, что и был: поведение прежних вызывающих не изменилось.
        Assert.Equal(expectedAnswer, result.Value!.Output);

        // Найдено то же имя урока, а не соседний урок про внешний вид.
        Assert.Contains(ExpectedLessonId, result.Value.EvidenceIds);

        // Лишней копии нет: наружу ушла та же самая строка, что была в обучении.
        Assert.Same(answerText, result.Value.Output);
    }

    // ПРИЁМКА 2, изменяемая половина. Явная типизированная копия выдерживает
    // нападение сразу с трёх сторон: исходный объект, выданный объект и его
    // вложенный список шагов. Чтение идёт сразу после нападения, без обучения.
    [Fact]
    public async Task ExplicitCopyPolicyWithMutableAnswer_SurvivesSourceReturnedAndNestedMutation()
    {
        // Объект ответа, который вызывающая сторона отдаёт в обучение и держит у себя.
        var sourceAnswer = new RouteAnswer(ExpectedDestination, ["validate-token", "check-scope", "return-verdict"]);

        // Обучение с явной типизированной политикой копирования ответа.
        var trained = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-copy",
            "test-classifier",
            "test-domain",
            [
                BuildSecurityLesson(sourceAnswer),
                new LogicalKnnTrainingExample<string, RouteAnswer>(
                    "pair-ui",
                    "layout screen accessibility design",
                    new RouteAnswer("UI", ["render-layout", "check-contrast", "return-view"]),
                    ["UI"],
                    ["ui"],
                    ["pair-ui"])
            ],
            input => input,
            BuildRouteCopyPolicy(),
            vectorDimensions: 32,
            k: 1);

        // Обучение обязано пройти через общий конверт результата.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Опись модели называет состояние владения: политика передана вызывающим.
        Assert.Contains("knn-answer-snapshot:custom", trained.Value!.Manifest.IngredientIds);

        // Чтение первое: обычный ответ до любого нападения.
        var first = await Ask(trained.Value, "unit-knn-default-ownership-copy");

        // Ожидаемые значения выписаны буквально, а не прочитаны из самого объекта.
        AssertExpectedAnswer(first);

        // Нападение первое: обучающая сторона меняет свой исходный объект.
        sourceAnswer.Destination = "СЛОМАНО-ВХОД";
        sourceAnswer.Steps[0] = "сломано-вход";

        // Нападение второе: получатель меняет верхнее поле выданного ответа.
        first.Output.Destination = "СЛОМАНО-ВЫХОД";

        // Нападение третье: получатель портит вложенный список шагов тремя способами.
        first.Output.Steps[0] = "сломано-шаг";
        first.Output.Steps.Add("лишний-шаг");
        first.Output.Steps.Clear();

        // Чтение второе: между нападением и этим чтением не было ни обучения,
        // ни восстановления значений, ни перестройки указателя соседей.
        var second = await Ask(trained.Value, "unit-knn-default-ownership-copy");

        // Принадлежащая память не изменилась ни в верхнем поле, ни во вложенном списке.
        AssertExpectedAnswer(second);

        // Найден тот же ближайший урок, а не соседний.
        Assert.Contains(ExpectedLessonId, second.EvidenceIds);

        // Общих ссылок не осталось ни с исходным объектом, ни между двумя ответами.
        Assert.NotSame(sourceAnswer, second.Output);
        Assert.NotSame(first.Output, second.Output);
        Assert.NotSame(first.Output.Steps, second.Output.Steps);
    }

    // ПРИЁМКА 3, первая половина. Несовпадение ВСТРОЕННОЙ политики для
    // неизменяемых ответов — это свой отдельный отказ со своим кодом, и он НЕ
    // заменяет проверку разрешителя по умолчанию.
    [Fact]
    public void KnownImmutablePolicyMismatch_IsItsOwnRefusalAndNotTheDefaultCheck()
    {
        // Счётчик обращений к выбору текста доказывает момент отказа.
        var textSelectorCalls = 0;

        // Выбор текста с подсчётом обращений.
        string CountingSelector(string input)
        {
            // Любое обращение означало бы начавшуюся подготовку памяти.
            textSelectorCalls++;
            // Входом служит сам учебный текст.
            return input;
        }

        // Вызывающая сторона сама передала встроенную политику для неизменяемых
        // ответов и применила её к изменяемому ответу-маршруту.
        var refused = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-mismatch",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer(ExpectedDestination, ["validate-token"]))],
            CountingSelector,
            KnnAnswerSnapshot.KnownImmutable<RouteAnswer>(),
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом результата.
        Assert.False(refused.IsSuccess);

        // Это код несовпадения политики, а не код разрешителя по умолчанию.
        Assert.Equal(ImmutablePolicyMismatchCode, refused.ErrorCode);

        // Два случая обязаны различаться, иначе одна проверка подменяла бы другую.
        Assert.NotEqual(DefaultPolicyRefusalCode, refused.ErrorCode);

        // И этот отказ тоже происходит до подготовки памяти.
        Assert.Equal(0, textSelectorCalls);
    }

    // ПРИЁМКА 3, вторая половина. Пустая политика — тоже свой отдельный отказ со
    // своим кодом, и он тоже НЕ заменяет проверку разрешителя по умолчанию.
    [Fact]
    public void NullPolicy_IsItsOwnRefusalAndNotTheDefaultCheck()
    {
        // Счётчик обращений к выбору текста доказывает момент отказа.
        var textSelectorCalls = 0;

        // Выбор текста с подсчётом обращений.
        string CountingSelector(string input)
        {
            // Любое обращение означало бы начавшуюся подготовку памяти.
            textSelectorCalls++;
            // Входом служит сам учебный текст.
            return input;
        }

        // Вызывающая сторона дошла до перегрузки с политикой, но политики не дала.
        var refused = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-null-policy",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer(ExpectedDestination, ["validate-token"]))],
            CountingSelector,
            null!,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом результата.
        Assert.False(refused.IsSuccess);

        // Это код пустой политики, а не код разрешителя по умолчанию.
        Assert.Equal(NullPolicyRefusalCode, refused.ErrorCode);

        // Три случая обязаны различаться между собой.
        Assert.NotEqual(DefaultPolicyRefusalCode, refused.ErrorCode);
        Assert.NotEqual(ImmutablePolicyMismatchCode, refused.ErrorCode);

        // И этот отказ тоже происходит до подготовки памяти.
        Assert.Equal(0, textSelectorCalls);
    }

    // Явный режим общего доступа остаётся доступным, но его называет сам
    // вызывающий, он помечен как общий и НЕ считается успешным владением.
    [Fact]
    public void ExplicitlyDeclaredSharedPolicy_TrainsButIsNeverReportedAsOwnership()
    {
        // Вызывающая сторона сама назвала режим общего доступа.
        var shared = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-declared-shared",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer(ExpectedDestination, ["validate-token"]))],
            input => input,
            KnnAnswerSnapshot.DeclaredShared<RouteAnswer>(),
            vectorDimensions: 32,
            k: 1);

        // Обучение проходит: названный вызывающим общий доступ разрешён.
        Assert.True(shared.IsSuccess, shared.ErrorMessage);

        // Состояние названо честно: доступ общий, память ответом не владеет.
        Assert.Contains("knn-answer-snapshot:declared-shared", shared.Value!.Manifest.IngredientIds);

        // Обратное утверждение тоже проверяется: отметки владения в описи нет.
        Assert.DoesNotContain("knn-answer-snapshot:known-immutable", shared.Value.Manifest.IngredientIds);
        Assert.DoesNotContain("knn-answer-snapshot:custom", shared.Value.Manifest.IngredientIds);
    }

    // ПРИЁМКА 5. Поведенческий ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ. Обратный вызов
    // копирования, который возвращает исходную ссылку, — это подмена владения
    // видимостью владения. Эта проверка доказывает, что оракул владения выше
    // действительно поймал бы такую политику, а не проходит сам собой.
    [Fact]
    public async Task AliasingCopyCallback_IsCaughtByTheSameOwnershipOracle()
    {
        // Политика копирования, которая на самом деле ничего не копирует.
        var aliasingPolicy = KnnAnswerSnapshot.FromCopy<RouteAnswer>(answer => answer);

        // Обучение с этой заведомо негодной политикой.
        var trained = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-default-ownership-alias-control",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer(ExpectedDestination, ["validate-token", "check-scope", "return-verdict"]))],
            input => input,
            aliasingPolicy,
            vectorDimensions: 32,
            k: 1);

        // Обучение проходит: подмена ссылки не видна по типу политики.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // Опись называет её переданной вызывающим политикой — имя не спасает.
        Assert.Contains("knn-answer-snapshot:custom", trained.Value!.Manifest.IngredientIds);

        // Чтение первое: до нападения ответ выглядит правильным.
        var first = await Ask(trained.Value, "unit-knn-default-ownership-alias-control");
        AssertExpectedAnswer(first);

        // Нападение в тот же вложенный список шагов, что и в основной проверке.
        first.Output.Steps[0] = "подменённый-шаг";

        // Чтение второе сразу после нападения, без обучения и восстановления.
        var second = await Ask(trained.Value, "unit-knn-default-ownership-alias-control");

        // Подмена ссылки видна прямо: оба ответа — один и тот же объект.
        Assert.Same(first.Output, second.Output);

        // Память испорчена: отпечаток больше не совпадает с ожидаемым.
        Assert.NotEqual(ExpectedFingerprint, second.Output.Fingerprint);

        // Главное утверждение контроля: ТОТ ЖЕ оракул владения, которым выше
        // принимается правильная политика, на этой политике падает. Значит он
        // чувствителен к подмене ссылки и не проходит сам собой.
        var oracleFailure = Record.Exception(() => AssertExpectedAnswer(second));
        Assert.NotNull(oracleFailure);
    }

    // Типизированная политика копирования ответа-маршрута: новый верхний объект
    // И копия вложенного списка шагов. Универсального клонирования здесь нет.
    private static IKnnAnswerSnapshot<RouteAnswer> BuildRouteCopyPolicy()
    {
        // Копия строится вручную по известным полям, а не сериализацией.
        return KnnAnswerSnapshot.FromCopy<RouteAnswer>(
            answer => new RouteAnswer(answer.Destination, [.. answer.Steps]));
    }

    // Один учебный урок про безопасность с заданным объектом ответа.
    private static LogicalKnnTrainingExample<string, RouteAnswer> BuildSecurityLesson(RouteAnswer answer)
    {
        // Текст, условия, метки и ссылки повторяют уже проверенный набор.
        return new LogicalKnnTrainingExample<string, RouteAnswer>(
            ExpectedLessonId,
            Question,
            answer,
            ["SECURITY", "AUTHORIZATION"],
            ["security"],
            [ExpectedLessonId]);
    }

    // Одно обычное чтение обученного блока на том же вопросе, что и урок.
    private static async Task<LogicalUnitResult<RouteAnswer>> Ask(
        KnnLogicalUnit<string, RouteAnswer> unit,
        string unitId)
    {
        // Запрос повторяет учебный вход и его условия.
        var result = await unit.ExecuteAsync(BuildRequest(unitId));

        // Чтение обязано пройти через общий конверт результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Наружу отдаётся сам ответ блока для дальнейших утверждений.
        return result.Value!;
    }

    // Один и тот же запрос для всех чтений: меняется только имя блока.
    private static LogicalUnitRequest<string> BuildRequest(string unitId)
    {
        // Вопрос, условия и признаки повторяют учебную строку-победителя.
        return new LogicalUnitRequest<string>(
            "run-knn-default-answer-ownership",
            unitId,
            "test-classifier",
            "test-domain",
            Question,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["predicate_ids"] = new[] { "SECURITY", "AUTHORIZATION" }
            });
    }

    // Буквальные ожидаемые значения одного ответа обученного блока. Это и есть
    // общий оракул владения: он используется и положительными проверками, и
    // отрицательным контролем подмены ссылки.
    private static void AssertExpectedAnswer(LogicalUnitResult<RouteAnswer> answer)
    {
        // Прежний ответ: точка назначения выписана буквально.
        Assert.Equal(ExpectedDestination, answer.Output.Destination);

        // Прежний ответ: список шагов выписан буквально и в своём порядке.
        Assert.Equal(["validate-token", "check-scope", "return-verdict"], answer.Output.Steps);

        // Отпечаток собственного значения целиком, одной строкой.
        Assert.Equal(ExpectedFingerprint, answer.Output.Fingerprint);
    }
}
