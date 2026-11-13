// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие владения ОТВЕТОМ обучаемого блока ближайших соседей
// (работа R2 плана ремонта 102, поздний аудит 29 октября).
//
// Что здесь проверяется по-человечески. Блок ближайших соседей учится на
// размеченных уроках и потом отвечает сохранённым ответом ближайшего урока.
// Описания урока (условия, предметные метки, ссылки на доказательства) он уже
// копировал себе раньше. Вопрос этой работы другой: кому принадлежит сам ОТВЕТ.
//
// Что было сломано. Ответ хранился и выдавался той же ссылкой, которую дала
// вызывающая сторона. Если ответ изменяемый — например ответ-маршрут со
// списком шагов, — то и обучающая сторона, и получатель ответа могли изменить
// этот объект и тем самым молча переписать обученную память. Второй раз тот же
// вопрос возвращал уже испорченный ответ, хотя никакого обучения между двумя
// чтениями не было. Для обученной памяти это тихая порча, а не удобство.
//
// Как это доказывается. Два хорошо различимых урока, обучение один раз, затем
// нападения с обеих сторон. После КАЖДОГО изменения память читается сразу: без
// повторного обучения, без восстановления значений и без перестройки указателя
// соседей — иначе проверка просто скрыла бы порчу. Ожидаемые значения выписаны
// буквально, а не прочитаны из самого же объекта, и сверяются три вещи:
// прежний ответ, то же найденное имя урока и отпечаток собственного значения.
//
// Бюджет этих проверок. По одной подготовке токенизатора на каждое успешное
// обучение очень малого набора (два урока), несколько чтений ответа и две
// отвергнутые попытки обучения, которые останавливаются до токенизатора.
// Ни одного обучения с учителем, ни одной пары предпочтений, ни одной записи
// на диск.
//
// Чего это покрытие НЕ доказывает. Оно не делает безопасным любой изменяемый
// тип ответа: память владеет ответом только тогда, когда для этого типа есть
// заданная типизированная политика либо тип заведомо неизменяем. Для прочих
// типов вызывающая сторона обязана сама назвать режим общего доступа, и это
// состояние видно в описи модели — проверка ниже требует именно такой честной
// отметки, а не молчаливого обещания владения. Запрет на МОЛЧАЛИВЫЙ выбор
// общего доступа прежней перегрузкой проверяет соседний файл-владелец
// KnnDefaultAnswerOwnershipTests, а не этот. Результат этого класса не
// распространяется на соседние блоки другого устройства и на путь сохранения
// модели на диск.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Изменяемый ответ-маршрут: изменяемое верхнее поле плюс вложенный изменяемый
// список шагов. Ровно такой формой ответа и пользуется нападение в проверках.
public sealed class RouteAnswer
{
    // Маршрут собирается из названия точки назначения и списка шагов.
    public RouteAnswer(string destination, List<string> steps)
    {
        // Точка назначения хранится как обычное изменяемое поле.
        Destination = destination;
        // Список шагов хранится ссылкой: именно он и есть вложенная коллекция.
        Steps = steps;
    }

    // Изменяемое верхнее поле ответа: его меняет нападение на верхний объект.
    public string Destination { get; set; }

    // Вложенный изменяемый список шагов: его меняет отдельное нападение.
    public List<string> Steps { get; }

    // Отпечаток собственного значения: одна строка, по которой видно сразу и
    // точку назначения, и весь список шагов в их порядке.
    public string Fingerprint => $"{Destination}|{string.Join(">", Steps)}";
}

public sealed class KnnAnswerOwnershipTests
{
    // Имя урока, который обязан находиться на учебном вопросе ниже.
    private const string ExpectedLessonId = "pair-security";

    // Буквально ожидаемая точка назначения сохранённого ответа.
    private const string ExpectedDestination = "SECURITY";

    // Буквально ожидаемый отпечаток сохранённого ответа целиком.
    private const string ExpectedFingerprint = "SECURITY|validate-token>check-scope>return-verdict";

    // Учебный вопрос повторяет текст урока-победителя.
    private const string Question = "jwt oauth authorization token";

    // Нападение со стороны ВХОДА: обучающая сторона меняет свой объект ответа
    // уже после обучения. Память обязана остаться прежней.
    [Fact]
    public async Task SourceAnswerMutatedAfterTraining_DoesNotChangeMemory()
    {
        // Объект ответа, который вызывающая сторона отдаёт в обучение и
        // продолжает держать у себя: именно его потом и будут менять.
        var sourceAnswer = new RouteAnswer("SECURITY", ["validate-token", "check-scope", "return-verdict"]);

        // Обучение с явной типизированной политикой владения ответом.
        var unit = TrainRouteUnit(sourceAnswer);

        // Чтение первое: обычный ответ до любого нападения.
        var first = await Ask(unit);
        AssertExpectedAnswer(first);

        // Нападение: обучающая сторона меняет и верхнее поле, и вложенный
        // список прямо в том объекте, который она передала в обучение.
        sourceAnswer.Destination = "СЛОМАНО-ВХОД";
        sourceAnswer.Steps[0] = "сломано-вход";
        sourceAnswer.Steps.Add("лишний-шаг-входа");

        // Чтение второе: между нападением и этим чтением не было ни обучения,
        // ни восстановления значений, ни перестройки указателя соседей.
        var second = await Ask(unit);
        AssertExpectedAnswer(second);

        // Память и объект обучающей стороны — это два разных объекта.
        Assert.NotSame(sourceAnswer, second.Output);
        Assert.NotSame(sourceAnswer.Steps, second.Output.Steps);
    }

    // Нападение со стороны ВЫХОДА, верхний объект: получатель ответа меняет
    // поле в том объекте, который ему выдал блок. Память обязана остаться прежней.
    [Fact]
    public async Task ReturnedAnswerTopLevelMutation_DoesNotChangeMemory()
    {
        // Обучение одним и тем же набором, что и в остальных проверках.
        var unit = TrainRouteUnit(new RouteAnswer("SECURITY", ["validate-token", "check-scope", "return-verdict"]));

        // Чтение первое: обычный ответ до нападения.
        var first = await Ask(unit);
        AssertExpectedAnswer(first);

        // Нападение: получатель меняет верхнее поле полученного ответа.
        first.Output.Destination = "СЛОМАНО-ВЫХОД";

        // Чтение второе сразу после изменения, без восстановления и обучения.
        var second = await Ask(unit);
        AssertExpectedAnswer(second);

        // Два разных ответа не делят один и тот же верхний объект.
        Assert.NotSame(first.Output, second.Output);
    }

    // Нападение со стороны ВЫХОДА, вложенный список шагов: это отдельная
    // проверка, потому что копия только верхнего объекта её бы не выдержала.
    [Fact]
    public async Task ReturnedAnswerNestedStepListMutation_DoesNotChangeMemory()
    {
        // Обучение одним и тем же набором, что и в остальных проверках.
        var unit = TrainRouteUnit(new RouteAnswer("SECURITY", ["validate-token", "check-scope", "return-verdict"]));

        // Чтение первое: обычный ответ до нападения.
        var first = await Ask(unit);
        AssertExpectedAnswer(first);

        // Нападение ровно во вложенный список: подмена шага, лишний шаг и
        // полная очистка — три разных способа испортить одну и ту же память.
        first.Output.Steps[0] = "сломано-шаг";
        first.Output.Steps.Add("лишний-шаг-выхода");
        first.Output.Steps.Clear();

        // Чтение второе сразу после изменения, без восстановления и обучения.
        var second = await Ask(unit);
        AssertExpectedAnswer(second);

        // Вложенный список тоже не общий: каждый ответ получает свой список.
        Assert.NotSame(first.Output.Steps, second.Output.Steps);
    }

    // Неизменяемая строка как ответ: всё работает как раньше и лишних копий
    // не создаётся — память отдаёт ту же самую строку.
    [Fact]
    public async Task ImmutableStringAnswer_KeepsWorkingAndNeedsNoExtraCopy()
    {
        // Буквально ожидаемый ответ урока.
        const string expectedAnswer = "SECURITY";

        // Та же строка, но собранная во время выполнения: она не попадает в
        // общий пул литералов, поэтому совпадение ССЫЛОК ниже действительно
        // доказывает, что копии не было, а не совпадение двух литералов.
        var answerText = new string(expectedAnswer.ToCharArray());

        // Проверка самой заготовки: это другой объект с тем же содержимым.
        Assert.NotSame(expectedAnswer, answerText);

        // Обучение прежней перегрузкой, без передачи политики: строка заведомо
        // неизменяема, поэтому политика выбирается сама и копий не делает.
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-knn-answer-ownership-string",
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

        // Опись модели называет состояние владения ответом своим именем.
        Assert.Contains("knn-answer-snapshot:known-immutable", trained.Value!.Manifest.IngredientIds);

        // Обычное чтение обученного блока на учебном вопросе.
        var result = await trained.Value.ExecuteAsync(BuildRequest("unit-knn-answer-ownership-string"));

        // Чтение обязано пройти через общий конверт результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        // Ответ тот же, что и был: поведение не изменилось.
        Assert.Equal(expectedAnswer, result.Value!.Output);
        // Найдено то же имя урока.
        Assert.Contains(ExpectedLessonId, result.Value.EvidenceIds);
        // Лишней копии нет: наружу ушла та же самая строка, что была в обучении.
        Assert.Same(answerText, result.Value.Output);
    }

    // Нет типизированной политики для неподдерживаемого изменяемого типа —
    // внятный отказ вместо молчаливого принятия чужой ссылки.
    [Fact]
    public void UnsupportedMutableAnswerWithoutTypedPolicy_IsRefusedAndNeverClaimedAsOwned()
    {
        // Счётчик обращений к выбору текста: его зовёт только дорогая подготовка
        // токенизатора и вектора урока. Ноль докажет, что отказ пришёл раньше.
        var textSelectorCalls = 0;
        // Тот же выбор текста, но с подсчётом обращений.
        string CountingSelector(string input)
        {
            textSelectorCalls++;
            return input;
        }

        // Встроенная политика для заведомо неизменяемых ответов — это ровно та
        // политика, которой память заявляет владение. На изменяемом ответе-
        // маршруте она обязана отказать, а не принять чужую ссылку молча.
        var refused = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-answer-ownership-refusal",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer("SECURITY", ["validate-token"]))],
            CountingSelector,
            KnnAnswerSnapshot.KnownImmutable<RouteAnswer>(),
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом с именованным кодом.
        Assert.False(refused.IsSuccess);
        Assert.Equal("KNN_ANSWER_SNAPSHOT_POLICY_REQUIRED", refused.ErrorCode);
        // Сообщение называет сам тип ответа, чтобы было понятно, что чинить.
        Assert.Contains(nameof(RouteAnswer), refused.ErrorMessage);
        // Отказ случился до подготовки токенизатора и указателя соседей.
        Assert.Equal(0, textSelectorCalls);

        // Пустая политика — тоже отказ, а не тихое согласие.
        var noPolicy = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-answer-ownership-null-policy",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer("SECURITY", ["validate-token"]))],
            CountingSelector,
            null!,
            vectorDimensions: 32,
            k: 1);

        // Тот же общий конверт, свой именованный код.
        Assert.False(noPolicy.IsSuccess);
        Assert.Equal("ANSWER_SNAPSHOT_REQUIRED", noPolicy.ErrorCode);
        // И этот отказ тоже пришёл до любой дорогой подготовки.
        Assert.Equal(0, textSelectorCalls);

        // Режим общего доступа остался возможен, но теперь его называет САМ
        // вызывающий: прежняя перегрузка без политики больше не подставляет
        // его молча (это отдельно проверяет владелец проверок разрешителя по
        // умолчанию). Названный вызывающим общий доступ обучается и владения
        // по-прежнему не обещает: состояние прямо записано в опись модели, а
        // отметки собственного владения там нет.
        var declaredShared = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-answer-ownership-declared-shared",
            "test-classifier",
            "test-domain",
            [BuildSecurityLesson(new RouteAnswer("SECURITY", ["validate-token"]))],
            input => input,
            KnnAnswerSnapshot.DeclaredShared<RouteAnswer>(),
            vectorDimensions: 32,
            k: 1);

        // Обучение проходит: прежние вызывающие стороны не ломаются.
        Assert.True(declaredShared.IsSuccess, declaredShared.ErrorMessage);
        // Состояние названо честно: память этим ответом не владеет.
        Assert.Contains("knn-answer-snapshot:declared-shared", declaredShared.Value!.Manifest.IngredientIds);
        // И обратное утверждение тоже проверяется: собственного владения нет.
        Assert.DoesNotContain("knn-answer-snapshot:known-immutable", declaredShared.Value.Manifest.IngredientIds);
    }

    // Обучение двух хорошо различимых уроков с явной типизированной политикой.
    private static KnnLogicalUnit<string, RouteAnswer> TrainRouteUnit(RouteAnswer securityAnswer)
    {
        // Два урока: один про безопасность, второй про внешний вид. Победитель
        // на учебном вопросе всегда первый, поэтому выбор соседа проверяем.
        var trained = KnnLogicalUnit<string, RouteAnswer>.Train(
            "unit-knn-answer-ownership",
            "test-classifier",
            "test-domain",
            [
                BuildSecurityLesson(securityAnswer),
                new LogicalKnnTrainingExample<string, RouteAnswer>(
                    "pair-ui",
                    "layout screen accessibility design",
                    new RouteAnswer("UI", ["render-layout", "check-contrast", "return-view"]),
                    ["UI"],
                    ["ui"],
                    ["pair-ui"])
            ],
            input => input,
            new RouteAnswerSnapshot(),
            vectorDimensions: 32,
            k: 1);

        // Обучение обязано пройти через общий конверт результата.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // Опись модели называет состояние владения ответом своим именем.
        Assert.Contains("knn-answer-snapshot:custom", trained.Value!.Manifest.IngredientIds);
        return trained.Value;
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
    private static async Task<LogicalUnitResult<RouteAnswer>> Ask(KnnLogicalUnit<string, RouteAnswer> unit)
    {
        // Запрос повторяет учебный вход и его условия.
        var result = await unit.ExecuteAsync(BuildRequest("unit-knn-answer-ownership"));

        // Чтение обязано пройти через общий конверт результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    // Один и тот же запрос для всех чтений: меняется только имя блока.
    private static LogicalUnitRequest<string> BuildRequest(string unitId)
    {
        // Вопрос, условия и признаки повторяют учебную строку-победителя.
        return new LogicalUnitRequest<string>(
            "run-knn-answer-ownership",
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

    // Буквальные ожидаемые значения одного ответа обученного блока.
    private static void AssertExpectedAnswer(LogicalUnitResult<RouteAnswer> answer)
    {
        // Найдено то же имя урока, а не соседний урок про внешний вид.
        Assert.Contains(ExpectedLessonId, answer.EvidenceIds);
        // Прежний ответ: точка назначения выписана буквально.
        Assert.Equal(ExpectedDestination, answer.Output.Destination);
        // Прежний ответ: список шагов выписан буквально и в своём порядке.
        Assert.Equal(["validate-token", "check-scope", "return-verdict"], answer.Output.Steps);
        // Отпечаток собственного значения целиком, одной строкой.
        Assert.Equal(ExpectedFingerprint, answer.Output.Fingerprint);
    }

    // Типизированная политика владения ответом-маршрутом: новый верхний объект
    // И копия вложенного списка шагов. Универсального клонирования здесь нет.
    private sealed class RouteAnswerSnapshot : IKnnAnswerSnapshot<RouteAnswer>
    {
        // Приём урока в память: память получает собственный объект ответа.
        public OperationResult<RouteAnswer> ForStorage(RouteAnswer value)
        {
            // Один и тот же способ копирования на приёме и на выдаче.
            return Own(value);
        }

        // Выдача ответа наружу: наружу уходит отдельный объект, а не память.
        public OperationResult<RouteAnswer> ForRead(RouteAnswer ownedValue)
        {
            // Один и тот же способ копирования на приёме и на выдаче.
            return Own(ownedValue);
        }

        // Единственное место, где строится независимая копия ответа-маршрута.
        private static OperationResult<RouteAnswer> Own(RouteAnswer source)
        {
            // Пустой ответ — это ошибка набора, и она называется прямо.
            if (source is null)
                return OperationResult<RouteAnswer>.Fail("Route answer is required.", "ROUTE_ANSWER_REQUIRED");

            // Новый верхний объект и новый список шагов: общих ссылок не остаётся.
            return OperationResult<RouteAnswer>.Ok(new RouteAnswer(source.Destination, [.. source.Steps]));
        }
    }
}
