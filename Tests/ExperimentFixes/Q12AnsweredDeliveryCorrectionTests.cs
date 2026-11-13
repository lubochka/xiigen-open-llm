// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Покрытие ПУТИ от обычной выдачи ответа до адресного исправления
// (узкий перенос Q12 карточки 74, раздел «Как доставить исправление тому, чей ответ увидел
// пользователь»; основание — опыт P11101, случай case-U2).
//
// Что здесь проверяется по-человечески. Пользователь задал вопрос и увидел ответ. Ответ дал один
// из двух зарегистрированных получателей. Потом пользователь подтвердил исправление ЭТОГО ответа.
// Проверка смотрит, дошёл ли урок ровно до того получателя, чей ответ был показан, и остался ли
// сосед нетронутым.
//
// Чем эта проверка НЕ является, названо прямо:
//   * это НЕ общий допуск доменных функций ранжирования: ни один каталог принципов здесь не
//     создаётся и не включается;
//   * это НЕ измерение пользы обучения: ни одна точность не улучшается и не заявляется;
//   * это НЕ доказательство побайтовой сохранности состояния: опознание состояния только
//     переносится и сверяется;
//   * писатель обучения — состав ЭТОЙ проверки. Он обучает РОДНЫМ обучением блока ближайших
//     соседей, но набор уроков владельца держит у себя, потому что у блока ближайших соседей НЕТ
//     родного сохранения и загрузки. Переобучение из набора проверки — это НЕ контрольная точка
//     обучаемого блока, и отсутствие родного сохранения им не прикрывается.
//
// Расход родного обучения: два обучения подготовки (по одному на получателя) и ровно одно
// обучение исправленного кандидата. Итого три — минимальный набор для одного случая.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Xunit;

// Писатель обучения и публикации для ЭТОЙ проверки.
//
// Он выполняет родное обучение блока ближайших соседей на исправленном уроке и публикует ТОЛЬКО
// выбранного владельца новой версией в тот же продуктовый реестр единиц.
internal sealed class Q12NativeTrainingWriter : IAddressedCorrectionWriter<string>
{
    // Продуктовый реестр единиц: из него читают владельца и в него же его публикуют.
    private readonly InMemoryLogicalUnitRegistry _registry;

    // Набор уроков каждого владельца. Он живёт здесь, потому что у блока ближайших соседей нет
    // родного сохранения и загрузки, из которых его можно было бы прочитать обратно.
    private readonly Dictionary<string, List<LogicalKnnTrainingExample<string, string>>> _lessons;

    // Обученный, но ещё не опубликованный набор уроков. Он ждёт успешной публикации.
    private readonly Dictionary<string, List<LogicalKnnTrainingExample<string, string>>> _pending =
        new(StringComparer.Ordinal);

    // Запрос пользователя, на который отвечают оба получателя.
    private readonly string _query;

    // Сколько раз связка попросила обучить исправленного кандидата.
    public int TrainCallCount { get; private set; }

    // Объект единицы, который связка передала на обучение. Это доказательство адресности.
    public ILogicalUnit<string, string>? SelectedUnitPassedToTraining { get; private set; }

    // Писатель собирается из реестра, набора уроков и текста запроса.
    public Q12NativeTrainingWriter(
        InMemoryLogicalUnitRegistry registry,
        Dictionary<string, List<LogicalKnnTrainingExample<string, string>>> lessons,
        string query)
    {
        // Реестр обязателен: без него публиковать некуда.
        _registry = registry;
        // Набор уроков обязателен: без него переобучать нечего.
        _lessons = lessons;
        // Текст запроса обязателен: исправленный урок отвечает на тот же вопрос.
        _query = query;
    }

    // Обучить исправленного кандидата ИМЕННО на той единице, которая отвечала.
    public OperationResult<AddressedCorrectionCandidate<string>> TrainCorrectedOwner(
        ILogicalUnit<string, string> selectedUnit,
        AddressedCorrectionOwnerIdentity owner,
        string correctedExampleId,
        string correctedAnswer,
        CancellationToken cancellationToken)
    {
        // Число обращений и переданный объект запоминаются до любой работы.
        TrainCallCount++;
        // Переданный объект владельца — то самое доказательство адресности, которое проверяет тест.
        SelectedUnitPassedToTraining = selectedUnit;

        // Уроки этого владельца берутся как есть; урок с тем же опознанием примера заменяется.
        var lessons = _lessons[owner.UnitId]
            .Where(lesson => !string.Equals(lesson.ExampleId, correctedExampleId, StringComparison.Ordinal))
            .ToList();

        // Исправленный урок отвечает на тот же запрос новым ответом пользователя.
        lessons.Add(new LogicalKnnTrainingExample<string, string>(
            correctedExampleId,
            _query,
            correctedAnswer,
            [],
            [owner.DomainTag],
            [$"evidence:{correctedExampleId}"]));

        // Родное обучение блока ближайших соседей: внешних библиотек машинного обучения нет.
        var trained = KnnLogicalUnit<string, string>.Train(
            owner.UnitId,
            owner.UnitRole,
            owner.DomainTag,
            lessons,
            text => text);

        // Отказ обучения возвращается тем же конвертом: отметки применения при этом не возникает.
        if (!trained.IsSuccess)
            return trained.MapError<AddressedCorrectionCandidate<string>>();

        // Обученный набор ждёт публикации и становится действующим только после неё.
        _pending[owner.UnitId] = lessons;

        // Кандидат остаётся на руках у связки вместе со своей будущей версией.
        return OperationResult<AddressedCorrectionCandidate<string>>.Ok(
            new AddressedCorrectionCandidate<string>(owner, trained.Value!, NextVersion(owner.Version)));
    }

    // Опубликовать ТОЛЬКО выбранного владельца новой версией.
    public OperationResult<AddressedCorrectionOwnerIdentity> PublishCorrectedOwner(
        AddressedCorrectionCandidate<string> candidate,
        CancellationToken cancellationToken)
    {
        // Публикация идёт обычным путём регистрации в продуктовом реестре единиц.
        var registered = _registry.Register<string, string>(
            candidate.Owner.UnitId,
            candidate.Owner.UnitRole,
            candidate.Owner.DomainTag,
            candidate.NextVersion,
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            candidate.CorrectedUnit);

        // Отказ регистрации возвращается как есть: опубликованного владельца нет.
        if (!registered.IsSuccess)
            return registered.MapError<AddressedCorrectionOwnerIdentity>();

        // Описание опубликованной версии приходит из самого реестра.
        var descriptor = registered.Value!;

        // Действующим набором уроков владельца становится опубликованный.
        _lessons[descriptor.UnitId] = _pending[descriptor.UnitId];

        // Опознание опубликованного владельца собирается из его описания в реестре.
        return OperationResult<AddressedCorrectionOwnerIdentity>.Ok(
            new AddressedCorrectionOwnerIdentity(
                descriptor.DomainTag,
                descriptor.DomainTag,
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.Version,
                descriptor.RegistrationOrder,
                $"published-{descriptor.Version}-{descriptor.RegistrationOrder}"));
    }

    // Следующая версия владельца. Правило простое и предсказуемое: третий разряд растёт на один.
    private static string NextVersion(string version)
    {
        // Версия разбирается по точкам; непригодный вид получает явное окончание.
        var parts = version.Split('.');
        // Три разряда — обычный случай, и растёт только последний.
        if (parts.Length == 3 && int.TryParse(parts[2], out var patch))
            return $"{parts[0]}.{parts[1]}.{patch + 1}";
        // Любой другой вид версии дополняется пометкой исправления.
        return version + "-corrected";
    }
}

// Проверка пути «обычная выдача -> подтверждённое исправление -> выбранный получатель».
public sealed class Q12AnsweredDeliveryCorrectionTests
{
    // Запрос, который оба получателя понимают одинаково. Одинаковый текст запроса у двух
    // владельцев — обычное дело, и адресом он быть не может.
    private const string Query = "approve report";

    // Домен обоих получателей. Он же профиль каталога правил ранжирования.
    private const string DomainTag = "D08";

    // Роль, которую занимают оба получателя в обычном пути выдачи.
    private const string Role = DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole;

    // Опознание запуска. Взято из опыта-основания, чтобы след был узнаваем.
    private const string RunId = "P111-PRIMARY-01/attempt-0001";

    // Опознание выдачи: по нему исправление находит именно эту выдачу.
    private const string FlowId = "generated-capability-flow-q12";

    [Fact]
    public async Task Исправление_доходит_до_получателя_чей_ответ_увидел_пользователь()
    {
        // Продуктовый реестр единиц: один и тот же для чтения владельцев и для публикации.
        var registry = new InMemoryLogicalUnitRegistry();

        // Набор уроков каждого получателя до исправления: по одному уроку на владельца.
        var lessons = new Dictionary<string, List<LogicalKnnTrainingExample<string, string>>>(StringComparer.Ordinal)
        {
            ["U1"] = [new LogicalKnnTrainingExample<string, string>("U1-R1", Query, "review", [], [DomainTag], ["evidence:U1-R1"])],
            ["U2"] = [new LogicalKnnTrainingExample<string, string>("U2-R1", Query, "archive", [], [DomainTag], ["evidence:U2-R1"])]
        };

        // Родное обучение первого получателя: на тот же запрос он отвечает «review».
        var firstOwner = KnnLogicalUnit<string, string>.Train("U1", Role, DomainTag, lessons["U1"], text => text);
        Assert.True(firstOwner.IsSuccess, firstOwner.ErrorMessage);

        // Родное обучение второго получателя: на тот же запрос он отвечает «archive».
        var secondOwner = KnnLogicalUnit<string, string>.Train("U2", Role, DomainTag, lessons["U2"], text => text);
        Assert.True(secondOwner.IsSuccess, secondOwner.ErrorMessage);

        // Порядок регистрации важен: обычный путь выдачи берёт последнего зарегистрированного.
        Assert.True(registry.Register<string, string>("U1", Role, DomainTag, "1.0.0", LogicalUnitTrustState.Trusted, true, firstOwner.Value!).IsSuccess);
        Assert.True(registry.Register<string, string>("U2", Role, DomainTag, "1.0.0", LogicalUnitTrustState.Trusted, true, secondOwner.Value!).IsSuccess);

        // Справочник владельцев — продуктовый переходник к тому же реестру.
        var directory = new LogicalUnitRegistryAddressedCorrectionDirectory<string>(registry);

        // Писатель обучения и публикации этой проверки.
        var writer = new Q12NativeTrainingWriter(registry, lessons, Query);

        // Каталог долговечных отметок применения. Свой у этой проверки, чтобы не мешать другим.
        var journalRoot = Path.Combine(
            Path.GetTempPath(),
            "llm-q12-answered-delivery-journal",
            $"case-u2-{Guid.NewGuid():N}");

        // Существующий координатор адресного исправления. Он НЕ переписывается этой проверкой.
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Журнал выдач: он и есть перенесённый путь от обычной выдачи к координатору.
        var ledger = new KnnAnsweredDeliveryLedger<string>(directory, coordinator);

        try
        {
            // Признаки прогона включают выбор обученной единицы: это обычный продуктовый путь.
            var flags = new LogicalExecutionFlags { UseLearnedUnitWhenAvailable = true };

            // ОБЫЧНЫЙ ПУТЬ, часть первая: получателя выбирает продуктовая фабрика потока, а не тест.
            var factory = new GeneratedCapabilityFlowFactory(
                "4",
                "4.1",
                "Backend Services",
                ["software-development"],
                ["IMPLEMENT"],
                DomainTag,
                Query,
                ledger);

            // Фабрика собирает поток из того же реестра, в котором живут оба получателя.
            var created = factory.Create(new LogicalFlowFactoryRequest(
                RunId,
                flags,
                registry,
                new Dictionary<string, object>(StringComparer.Ordinal)));
            Assert.True(created.IsSuccess, created.ErrorMessage);

            // Собранный поток обязан быть потоком обученной способности, а не запасным путём:
            // иначе ответ пришёл бы не от обученного получателя, и проверка ничего не значила бы.
            var flow = Assert.IsType<GeneratedCapabilityFlow>(created.Value!);

            // ОБЫЧНЫЙ ПУТЬ, часть вторая: ответ выдаётся вызовом потока, а НЕ прямым вызовом
            // координатора адресного исправления.
            var served = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
                RunId,
                FlowId,
                "generated-capability-q12",
                Query,
                flags,
                new Dictionary<string, object>(StringComparer.Ordinal)));
            Assert.True(served.IsSuccess, served.ErrorMessage);

            // Точка выдачи запомнила выдачу: признак виден в показателях самого прогона.
            Assert.Equal(bool.TrueString, served.Value!.Metrics["answered_delivery_recorded"]);

            // Записанный выбранный получатель — второй владелец: его ответ и увидел пользователь.
            Assert.Equal("U2", served.Value.Metrics["answered_delivery_owner_id"]);

            // Тот же выбранный получатель назван и обычным показателем выдачи.
            Assert.Equal("U2", served.Value.Metrics["retrieval_unit_id"]);

            // Ответы обоих получателей ДО исправления читаются обычным путём реестра.
            var secondBefore = await AnswerOf(directory, "U2", flags);
            var firstBefore = await AnswerOf(directory, "U1", flags);

            // Замороженные ожидания опыта-основания до исправления.
            Assert.Equal("archive", secondBefore);
            Assert.Equal("review", firstBefore);

            // Подтверждённое исправление приходит ПО ЭТОЙ ВЫДАЧЕ и уходит в существующий координатор.
            var receipt = await ledger.ApplyConfirmedCorrectionAsync(
                RunId,
                FlowId,
                "fb-case-U2",
                "U2-R1",
                "archive then notify");
            Assert.True(receipt.IsSuccess, receipt.ErrorMessage);

            // Квитанция называет выбранного получателя, а не соседа.
            Assert.Equal("U2", receipt.Value!.UnitId);
            Assert.Equal(DomainTag, receipt.Value.DomainTag);

            // Обучение случилось ровно один раз и получило ФАКТИЧЕСКИЙ объект выбранного владельца.
            Assert.Equal(1, writer.TrainCallCount);
            Assert.Same(secondOwner.Value, writer.SelectedUnitPassedToTraining);

            // Теперь снова обращаемся к ОБОИМ получателям обычным путём реестра.
            var secondAfter = await AnswerOf(directory, "U2", flags);
            var firstAfter = await AnswerOf(directory, "U1", flags);

            // Первое из трёх: урок ФАКТИЧЕСКИ применён — исходное действие дополнено «notify».
            Assert.Equal("archive then notify", secondAfter);
            Assert.NotEqual(secondBefore, secondAfter);

            // Второе из трёх: ЧУЖОЙ получатель не изменился ни на знак.
            Assert.Equal("review", firstAfter);
            Assert.Equal(firstBefore, firstAfter);

            // Третье из трёх: свой восстановленный ответ на месте — память владельца не потеряна,
            // он по-прежнему отвечает на тот же запрос.
            Assert.False(string.IsNullOrWhiteSpace(secondAfter));

            // Наблюдения сохраняются уликой в дереве проекта.
            WriteObservedEvidence(
                selectedOwnerId: served.Value.Metrics["answered_delivery_owner_id"],
                secondBefore: secondBefore,
                secondAfter: secondAfter,
                firstBefore: firstBefore,
                firstAfter: firstAfter,
                receiptUnitId: receipt.Value.UnitId,
                receiptPublishedVersion: receipt.Value.PublishedVersion,
                servedThroughOrdinaryPath: flow.GetType().Name,
                trainCallCount: writer.TrainCallCount);
        }
        finally
        {
            // Каталог отметок этой проверки убирается за собой.
            if (Directory.Exists(journalRoot))
                Directory.Delete(journalRoot, recursive: true);
        }
    }

    // Обратиться к одному получателю обычным путём: чтение последней версии владельца из реестра
    // по его опознанию и обычный вызов его ответа.
    private static async Task<string> AnswerOf(
        LogicalUnitRegistryAddressedCorrectionDirectory<string> directory,
        string unitId,
        LogicalExecutionFlags flags)
    {
        // Владелец читается тем же продуктовым путём, которым его читает координатор.
        var resolved = directory.ResolveLatestOwnerById(unitId);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // Ответ запрашивается обычным конвертом запроса к единице.
        var answered = await resolved.Value!.Unit.ExecuteAsync(new LogicalUnitRequest<string>(
            RunId,
            unitId,
            Role,
            DomainTag,
            Query,
            flags,
            new Dictionary<string, object>(StringComparer.Ordinal)));
        Assert.True(answered.IsSuccess, answered.ErrorMessage);

        // Наружу уходит фактический ответ получателя.
        return answered.Value!.Output;
    }

    // Сохранить наблюдённые значения уликой рядом с объявленными ожиданиями.
    private static void WriteObservedEvidence(
        string selectedOwnerId,
        string secondBefore,
        string secondAfter,
        string firstBefore,
        string firstAfter,
        string receiptUnitId,
        string receiptPublishedVersion,
        string servedThroughOrdinaryPath,
        int trainCallCount)
    {
        // Корень дерева ищется вверх от каталога сборки: жёсткого пути в проверке нет.
        var root = new DirectoryInfo(AppContext.BaseDirectory);

        // Корнем считается каталог, в котором лежат и проект обучения способностей, и улики.
        while (root is not null &&
               !(Directory.Exists(Path.Combine(root.FullName, "18.CapabilityTraining")) &&
                 Directory.Exists(Path.Combine(root.FullName, "artifacts"))))
            root = root.Parent;

        // Без найденного корня улика не пишется, но и проверка от этого не падает.
        if (root is null)
            return;

        // Точный каталог улик этого наряда.
        var evidenceDirectory = Path.Combine(
            root.FullName,
            "artifacts",
            "fbe",
            "Q12-ADDRESSED-DELIVERY",
            "attempt-0001");

        // Каталог создаётся, если его ещё нет.
        Directory.CreateDirectory(evidenceDirectory);

        // Наблюдения пишутся простыми строками «имя: значение», чтобы их читал человек.
        var lines = new[]
        {
            "# Наблюдённые значения прогона одного случая (case-U2 опыта P11101)",
            string.Empty,
            $"путь выдачи (собранный продуктовой фабрикой поток): {servedThroughOrdinaryPath}",
            $"выбранный получатель, записанный точкой выдачи: {selectedOwnerId}",
            string.Empty,
            $"получатель U2, ответ ДО исправления: {secondBefore}",
            $"получатель U2, ответ ПОСЛЕ исправления: {secondAfter}",
            $"получатель U1, ответ ДО исправления: {firstBefore}",
            $"получатель U1, ответ ПОСЛЕ исправления: {firstAfter}",
            string.Empty,
            $"квитанция координатора, владелец: {receiptUnitId}",
            $"квитанция координатора, опубликованная версия: {receiptPublishedVersion}",
            $"обучений исправленного кандидата: {trainCallCount}",
            string.Empty,
            "Чужой получатель не изменился: " + (string.Equals(firstBefore, firstAfter, StringComparison.Ordinal) ? "да" : "НЕТ"),
            "Урок фактически применён: " + (!string.Equals(secondBefore, secondAfter, StringComparison.Ordinal) ? "да" : "НЕТ"),
            "Свой восстановленный ответ на месте: " + (!string.IsNullOrWhiteSpace(secondAfter) ? "да" : "НЕТ")
        };

        // Файл наблюдений кладётся рядом с объявленными заранее ожиданиями.
        File.WriteAllLines(Path.Combine(evidenceDirectory, "04-observed-run-values.md"), lines);
    }
}
