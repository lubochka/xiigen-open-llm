// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Перенос договора владения ОТВЕТОМ во вторую единицу ближайших соседей — ту,
// что ищет соседа по вложениям преобразователя
// (TransformerEmbeddingKnnLogicalUnit в проекте 18.CapabilityTraining).
//
// Зачем это нужно по-человечески. Опыт двадцатой строки опытов измерил на живом
// коде: когда единица ближайших соседей отдаёт наружу тот же объект ответа,
// который лежит у неё в памяти, внешнее изменение этого объекта портит
// обученный урок. Рука без защиты дала девять правильных ответов из двенадцати,
// рука с копией — двенадцать из двенадцати. В первой единице (обычный блок
// ближайших соседей в проекте 21.AgentInfrastructure) защита уже стояла: там
// есть договор владения ответом с вариантами «известный неизменяемый тип»,
// «явно объявленный общий доступ» и «копия по функции», и отказ по умолчанию
// для изменяемых типов. Во второй единице защиты не было вообще: ответ
// хранился и выдавался той же ссылкой, которую дала вызывающая сторона, хотя
// рядом, в том же возврате, два массива описаний уже копировались.
//
// Что проверяется здесь. Ровно один договор — кому принадлежит значение ответа:
// (1) память не сдвигается, когда вызывающая сторона меняет свой объект ответа
//     ПОСЛЕ обучения (дыра на приёме);
// (2) память не сдвигается, когда получатель меняет уже выданный ему ответ
//     (дыра на выдаче);
// (3) прежняя удобная перегрузка без договора на изменяемом типе ответа
//     ОТКАЗЫВАЕТ понятным именованным кодом, а не молча отдаёт ссылку;
// (4) отрицательный контроль: при явно объявленном общем доступе та же атака
//     ПРОХОДИТ — это доказывает, что атака настоящая, а проверки выше не
//     «зеленеют сами собой»;
// (5) положительный контроль: прежние вызывающие со строковым ответом
//     продолжают работать без правок, в том числе на порядке с каталогом
//     правил ранжирования.
//
// Бюджет этих проверок. Один обученный словарь кусочков слов и одна крошечная
// модель на весь класс, один-два урока на обучение. Обучений с учителем — ноль,
// пар предпочтений — ноль, записей на диск — ноль.
//
// Чего это покрытие НЕ доказывает. Оно не улучшает точность ближайших соседей:
// это договор владения, а не качество ответов. Оно не делает безопасным любой
// изменяемый тип ответа — оно лишь запрещает молчаливое согласие на него. Оно
// ничего не говорит о сохранении обученной единицы на диск: путь снимка
// обученной памяти (TeachingRegistryPersistence) в этот перенос не входил.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class TransformerKnnAnswerOwnershipTests
{
    // Именованный код отказа РАЗРЕШИТЕЛЯ ПО УМОЛЧАНИЮ: прежняя перегрузка без
    // договора на изменяемом типе ответа обязана отказать именно им.
    private const string DefaultPolicyRefusalCode = "KNN_ANSWER_SNAPSHOT_DEFAULT_POLICY_REQUIRED";

    // Именованный код отказа на пустом договоре. Это другой, отдельный случай.
    private const string NullPolicyRefusalCode = "ANSWER_SNAPSHOT_REQUIRED";

    // Область, под которую обучается единица, и профиль каталога правил.
    private const string Profile = "RU";

    // Опознание единственного урока памяти.
    private const string LessonId = "S01";

    // Текст урока: по нему считается вложение и строится вопрос.
    private const string LessonText = "Задание: продолжи действие. Префикс: каждый день я";

    // Вопрос повторяет текст урока, поэтому ближайшим соседом будет он же.
    private const string Question = "Задание: продолжи действие. Префикс: каждый день он";

    // Исходные части изменяемого ответа записаны отдельными строками. Ожидание
    // берётся из них, а не из того объекта, который проверка будет портить:
    // иначе атака изменила бы и «ожидаемое» значение вместе с проверяемым.
    private const string OriginalDestination = "SECURITY";
    private const string OriginalFirstStep = "validate-token";
    private const string OriginalSecondStep = "check-scope";

    // Буквально ожидаемый отпечаток сохранённого ответа целиком.
    private const string OriginalFingerprint = "SECURITY|validate-token>check-scope";

    // Ожидаемый строковый ответ положительного контроля.
    private const string OriginalStringOutput = "хожу";

    // Один обученный на словах этой панели словарь кусочков слов на весь класс:
    // обучение единицы его не меняет, оно только считает по нему вложения.
    private static readonly Lazy<BpeTokenizer> SharedTokenizer = new(() =>
    {
        // Тот же корпус и тот же размер словаря 320, что и у соседних проверок
        // этой единицы, чтобы два покрытия не спорили о словаре.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "Задание продолжи действие Префикс каждый день вчера завтра прошлой ночью я он хожу ходил пойду спал текст запрос",
            320);
        return tokenizer;
    });

    // Одна и та же крошечная модель на весь класс: обучение единицы её не
    // меняет, оно только считает по ней векторы.
    private static readonly Lazy<LlmModel> SharedModel = new(() => new LlmModel(new ModelConfig
    {
        VocabSize = 512,
        ContextLen = 64,
        HiddenDim = 16,
        NumLayers = 1,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 32
    }));

    // ПРИЁМКА 1 (дыра на приёме). Вызывающая сторона держит свой объект ответа и
    // меняет его ПОСЛЕ обучения. Память обязана отвечать прежним уроком, потому
    // что на приёме она взяла СВОЮ копию значения. Без правки память хранила бы
    // чужую ссылку, и ответ поехал бы вместе с ней, ничему не обучаясь.
    [Fact]
    public async Task StoredAnswer_DoesNotFollowCallerMutationAfterTraining()
    {
        // Объект ответа, который остаётся на руках у вызывающей стороны.
        var callerAnswer = new RouteAnswer(OriginalDestination, [OriginalFirstStep, OriginalSecondStep]);

        // Обучение одного урока с типизированным договором «копия по функции».
        var unit = TrainRouteUnit(
            "knn-transformer-answer-ownership-storage",
            callerAnswer,
            KnnAnswerSnapshot.FromCopy<RouteAnswer>(static answer => answer.Copy()));

        // Обращение 1 из 2: ответ до любой атаки.
        Assert.Equal(OriginalFingerprint, (await AnswerRouteAsync(unit)).Fingerprint);

        // Атака: вызывающая сторона меняет и верхний объект, и вложенный список шагов.
        callerAnswer.Destination = "ATTACKED-BY-CALLER";
        callerAnswer.Steps.Add("drop-all-checks");

        // Обращение 2 из 2: память не должна была сдвинуться вместе с чужим объектом.
        // Переобучения между обращениями нет.
        Assert.Equal(OriginalFingerprint, (await AnswerRouteAsync(unit)).Fingerprint);

        // Испорченным оказался именно объект вызывающей стороны: это доказывает,
        // что атака действительно меняла объект, а не писала мимо.
        Assert.Equal("ATTACKED-BY-CALLER|validate-token>check-scope>drop-all-checks", callerAnswer.Fingerprint);
    }

    // ПРИЁМКА 2 (дыра на выдаче). Получатель меняет уже выданный ему ответ.
    // Память обязана отвечать прежним уроком, потому что наружу уходит копия.
    // Эта проверка ловит случай, когда копия сделана только на приёме, а наружу
    // по-прежнему отдают саму память.
    [Fact]
    public async Task IssuedAnswer_MutationDoesNotRewriteTrainedMemory()
    {
        // Обучение одного урока тем же типизированным договором.
        var unit = TrainRouteUnit(
            "knn-transformer-answer-ownership-read",
            new RouteAnswer(OriginalDestination, [OriginalFirstStep, OriginalSecondStep]),
            KnnAnswerSnapshot.FromCopy<RouteAnswer>(static answer => answer.Copy()));

        // Обращение 1 из 3: первый выданный наружу ответ.
        var first = await AnswerRouteAsync(unit);
        Assert.Equal(OriginalFingerprint, first.Fingerprint);

        // Атака: получатель меняет верхний объект и вложенный список выданного ответа.
        first.Destination = "ATTACKED-BY-RESULT";
        first.Steps.Add("skip-authorization");

        // Обращение 2 из 3: следующий ответ обязан быть прежним уроком.
        var second = await AnswerRouteAsync(unit);
        Assert.Equal(OriginalFingerprint, second.Fingerprint);

        // Каждый ответ несёт свой собственный объект: общий объект на два ответа
        // снова открыл бы ту же дверь, которую закрывает этот перенос.
        Assert.NotSame(first, second);
        Assert.NotSame(first.Steps, second.Steps);

        // Атака 2: теперь портится второй выданный ответ.
        second.Steps.Clear();

        // Обращение 3 из 3: память по-прежнему отвечает исходным уроком.
        Assert.Equal(OriginalFingerprint, (await AnswerRouteAsync(unit)).Fingerprint);

        // Испорченными оказались именно выданные копии, а не память.
        Assert.Equal("ATTACKED-BY-RESULT|validate-token>check-scope>skip-authorization", first.Fingerprint);
        Assert.Equal("SECURITY|", second.Fingerprint);
    }

    // ПРИЁМКА 3. Прежняя удобная перегрузка без договора на изменяемом типе
    // ответа обязана отказать именованным кодом разрешителя по умолчанию, а не
    // молча согласиться на общий доступ к памяти.
    [Fact]
    public void DefaultOverloadWithMutableAnswer_IsRefusedWithANamedCode()
    {
        // Обучение через прежнюю перегрузку: договор не передан.
        var trained = TransformerEmbeddingKnnLogicalUnit<RouteAnswer>.Train(
            "knn-transformer-answer-ownership-default",
            "generated-capability",
            Profile,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<RouteAnswer>(
                LessonId,
                LessonText,
                new RouteAnswer(OriginalDestination, [OriginalFirstStep, OriginalSecondStep]),
                [Profile],
                [Profile])]);

        // Отказ приходит обычным конвертом, а не исключением.
        Assert.False(trained.IsSuccess);
        Assert.Equal(DefaultPolicyRefusalCode, trained.ErrorCode);
        // Частично собранной единицы наружу не выдано.
        Assert.Null(trained.Value);
        // Сообщение называет тип ответа, чтобы вызывающая сторона не осталась в тупике.
        Assert.Contains(typeof(RouteAnswer).FullName!, trained.ErrorMessage);
    }

    // ПРИЁМКА 4. Пустой договор — отдельный случай со своим кодом: это ошибка
    // сборки вызова, а не свойство типа ответа.
    [Fact]
    public void ExplicitOverloadWithNullPolicy_IsRefusedWithItsOwnCode()
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<RouteAnswer>.Train(
            "knn-transformer-answer-ownership-null-policy",
            "generated-capability",
            Profile,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<RouteAnswer>(
                LessonId,
                LessonText,
                new RouteAnswer(OriginalDestination, [OriginalFirstStep, OriginalSecondStep]),
                [Profile],
                [Profile])],
            (IKnnAnswerSnapshot<RouteAnswer>)null!);

        Assert.False(trained.IsSuccess);
        Assert.Equal(NullPolicyRefusalCode, trained.ErrorCode);
        Assert.Null(trained.Value);
    }

    // ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ. При явно объявленном общем доступе владение НЕ
    // заявлено, и та же атака обязана ПРОЙТИ. Эта проверка доказывает, что атаки
    // в приёмках 1 и 2 настоящие: если бы единица копировала ответ всегда и
    // независимо от договора, здесь отпечаток не изменился бы и проверка упала.
    [Fact]
    public async Task DeclaredSharedAccess_IsNotOwnership_SoTheSameAttackGetsThrough()
    {
        // Объект ответа, который остаётся на руках у вызывающей стороны.
        var callerAnswer = new RouteAnswer(OriginalDestination, [OriginalFirstStep, OriginalSecondStep]);

        // Общий доступ назван вызывающей стороной явно: это законная, но не
        // владеющая дверь, и опись обученной модели называет её своим именем.
        var unit = TrainRouteUnit(
            "knn-transformer-answer-ownership-declared-shared",
            callerAnswer,
            KnnAnswerSnapshot.DeclaredShared<RouteAnswer>());

        // Состояние владения видно в описи, а не только в коде.
        Assert.Contains("knn-answer-snapshot:declared-shared", unit.Manifest.IngredientIds);
        Assert.DoesNotContain("knn-answer-snapshot:known-immutable", unit.Manifest.IngredientIds);

        // До атаки ответ прежний.
        Assert.Equal(OriginalFingerprint, (await AnswerRouteAsync(unit)).Fingerprint);

        // Та же самая атака, что и в приёмке 1.
        callerAnswer.Steps.Add("drop-all-checks");

        // И она проходит: общий доступ владением не является, и это честно видно.
        Assert.Equal(
            "SECURITY|validate-token>check-scope>drop-all-checks",
            (await AnswerRouteAsync(unit)).Fingerprint);
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ. Прежние вызывающие со строковым ответом работают
    // без единой правки: строка входит в закрытый список заведомо неизменяемых
    // типов, поэтому разрешитель по умолчанию отвечает политикой тождества, а не
    // отказом. Проверяются оба порядка разбора кандидатов — родной и с
    // каталогом правил ранжирования.
    [Fact]
    public async Task StringAnswers_KeepWorkingThroughTheOldOverload_OnNativeAndRankedOrders()
    {
        // Обучение через прежнюю перегрузку, ровно как у существующих вызывающих.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "knn-transformer-answer-ownership-string",
            "generated-capability",
            Profile,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<string>(
                LessonId,
                LessonText,
                OriginalStringOutput,
                [Profile],
                [Profile])]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        var unit = trained.Value!;

        // Память владеет строковым ответом без копии, и это названо в описи.
        Assert.Contains("knn-answer-snapshot:known-immutable", unit.Manifest.IngredientIds);
        // Прежние составные части описи на месте: ничего не потеряно.
        Assert.Contains("05.Embeddings:EmbeddingModel", unit.Manifest.IngredientIds);
        Assert.Contains("E34V2:BM25_KNN_FUSION", unit.Manifest.IngredientIds);

        // Родной порядок отвечает прежним значением и прежними описаниями.
        var native = await AnswerStringAsync(unit);
        Assert.Equal(OriginalStringOutput, native.Output);
        Assert.Equal(Profile, Assert.Single(native.PredicateIds));
        Assert.Contains(LessonId, native.EvidenceIds);

        // Тот же обученный модуль с подключённым каталогом правил: память одна и
        // та же, переобучения нет, меняется только порядок разбора кандидатов.
        var catalog = KnnCandidatePrincipleCatalog.Create(
            Profile,
            [KnnDomainCandidatePrincipleProfiles.RussianTimeFrame()]);
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        var ranked = unit.WithCandidatePrinciples(catalog.Value!);
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);

        // Порядок с каталогом правил отвечает из той же неизменной памяти.
        Assert.Equal(OriginalStringOutput, (await AnswerStringAsync(ranked.Value!)).Output);
        // Обещание владения ответом не изменилось вместе с порядком кандидатов.
        Assert.Contains("knn-answer-snapshot:known-immutable", ranked.Value!.Manifest.IngredientIds);
    }

    // Обучение одного урока с изменяемым ответом по заданному договору владения.
    private static TransformerEmbeddingKnnLogicalUnit<RouteAnswer> TrainRouteUnit(
        string unitId,
        RouteAnswer answer,
        IKnnAnswerSnapshot<RouteAnswer> answerSnapshot)
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<RouteAnswer>.Train(
            unitId,
            "generated-capability",
            Profile,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<RouteAnswer>(
                LessonId,
                LessonText,
                answer,
                [Profile],
                [Profile])],
            answerSnapshot);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    // Один обычный вопрос к единице с изменяемым ответом через обычный конверт запроса.
    private static async Task<RouteAnswer> AnswerRouteAsync(TransformerEmbeddingKnnLogicalUnit<RouteAnswer> unit)
    {
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-transformer-answer-ownership",
            "knn-transformer-answer-ownership",
            "generated-capability",
            Profile,
            Question,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        // Тот же самый урок памяти, иначе сравнение отпечатков ничего не значит.
        Assert.Contains(LessonId, answer.Value!.EvidenceIds);
        return answer.Value!.Output;
    }

    // Один обычный вопрос к единице со строковым ответом.
    private static async Task<LogicalUnitResult<string>> AnswerStringAsync(TransformerEmbeddingKnnLogicalUnit<string> unit)
    {
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-transformer-answer-ownership-string",
            "knn-transformer-answer-ownership-string",
            "generated-capability",
            Profile,
            Question,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        return answer.Value!;
    }

    // Изменяемый ответ-маршрут: верхний объект можно переписать, и вложенный
    // список шагов тоже. Ровно такой ответ и ломался в опыте двадцатой строки.
    private sealed class RouteAnswer
    {
        // Маршрут собирается из точки назначения и своих собственных шагов.
        internal RouteAnswer(string destination, IEnumerable<string> steps)
        {
            // Точка назначения хранится как изменяемое свойство: его подменяют в атаке.
            Destination = destination;
            // Шаги копируются при сборке, чтобы сам маршрут владел своим списком.
            Steps = [.. steps];
        }

        // Точка назначения маршрута; изменяема намеренно.
        internal string Destination { get; set; }

        // Шаги маршрута; список изменяем намеренно — это вложенная коллекция.
        internal List<string> Steps { get; }

        // Отпечаток ответа целиком: по нему проверка видит и подмену точки
        // назначения, и подмену вложенного списка шагов одним сравнением.
        internal string Fingerprint => $"{Destination}|{string.Join(">", Steps)}";

        // Независимая копия: новый верхний объект И новый вложенный список.
        // Именно такую функцию копирования требует договор владения ответом.
        internal RouteAnswer Copy() => new(Destination, Steps);

        // Текстовая форма нужна отпечатку содержимого корпуса: без неё отпечаток
        // увидел бы только имя типа, и подмена ответа осталась бы незамеченной.
        public override string ToString() => Fingerprint;
    }
}
