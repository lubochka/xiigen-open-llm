// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие АДРЕСНОЙ ДОСТАВКИ исправляющего урока выбранному владельцу памяти
// (узкий срез двенадцатой строки переноса реестра правок по итогам опытов, опознаватель FIX-KDP;
// дочерний владелец требования — карточка «исправление учит выбранного владельца, чужая память
// остаётся прежней»).
//
// Что здесь проверяется по-человечески. Пользователь исправил ответ. Исправляющий урок обязан дойти
// ровно до той обученной единицы, которая этот ответ дала, и ни до какой другой. Проверяются четыре
// вещи, каждая своей решающей проверкой:
//   1) связка домена, модели и её версии: урок несёт, к какому домену, какой модели и какой её
//      версии он относится, и доставка это СВЕРЯЕТ, а не принимает на слово;
//   2) выбранный писатель: урок применяется именно к тому объекту единицы, который отвечал, — это
//      доказывается ФАКТИЧЕСКИ вызванным членом с фактическим объектом, а не записью в полях;
//   3) восстановление: после поднятия отметки с диска адресная доставка продолжает работать;
//   4) устойчивая дедупликация ПОСЛЕ успешного применения: повторный тот же урок не применяется
//      второй раз, признак этого живёт на диске, и при НЕУДАЧНОМ применении отметки не возникает,
//      то есть урок остаётся применимым.
//
// Что было сломано в опытном образце. Набор доставок (HashSet) заполнялся ещё на решении, ДО
// обучения. Поэтому неудачная доставка всё равно считалась состоявшейся: урок терялся молча. Этот
// образец сюда НАМЕРЕННО не перенесён, и проверка «при неудаче отметки нет» охраняет именно это.
//
// Чем эти проверки НЕ являются, названо прямо. Это НЕ общий допуск доменных функций ранжирования: ни
// один каталог принципов здесь не создаётся и не включается. Это НЕ измерение пользы обучения: ни
// одна точность здесь не улучшается и не заявляется. Это НЕ доказательство побайтовой сохранности
// состояния: опознание состояния только переносится и сверяется. Писатель за границей обучения —
// подставной состав ЭТИХ проверок; продуктовый допущенный писатель принадлежит своему владельцу и
// в этом срезе отсутствует.
//
// Расход родного обучения. Общая неизменяемая подготовка обучает двух владельцев (2 родных
// обучения). Каждая из двух родных проверок добавляет ровно одно обучение исправленного кандидата,
// итого 4 родных обучения — ровно назначенный карточкой местный предел. Остальные шесть проверок —
// чистые: родных обучений 0, обращений к модели 0.
//
// Числовые сравнения. Все утверждения здесь ТОЧНЫЕ: строковое равенство ответов, равенство ссылок
// на объект, равенство счётчиков вызовов, равенство опознаний. Сравнений с допуском нет ни одного,
// поэтому вопрос «различима ли величина на фоне допуска» к этому файлу не относится.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Xunit;

// Подставная единица без модели. Нужна чистым проверкам: они судят о связке, а не об обучении,
// поэтому родное обучение им не требуется вовсе.
internal sealed class StubAddressedCorrectionUnit : ILogicalUnit<string, string>
{
    // Ответ, который эта подставная единица выдаёт на любой вход.
    private readonly string _answer;

    // Подставная единица создаётся из одного ответа: своего обученного состояния у неё нет.
    internal StubAddressedCorrectionUnit(string answer) => _answer = answer;

    // Выдача ответа общим конвертом проекта, как и у настоящей единицы.
    public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(
            new LogicalUnitResult<string>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                _answer,
                1.0,
                [],
                [request.DomainTag],
                [],
                [])));
}

// Подставной справочник владельцев. Он позволяет проверкам задать ровно то живое опознание, о
// котором судит связка, и подменить его между двумя обращениями.
internal sealed class StubAddressedCorrectionDirectory : IAddressedCorrectionOwnerDirectory<string>
{
    // Записи справочника по опознанию модели: описание владельца и его исполняемый объект.
    private readonly Dictionary<string, ResolvedLogicalUnitRegistryEntry<string, string>> _entries = new(StringComparer.Ordinal);

    // Счётчик обращений: по нему видно, что связка читает живое опознание дважды — до обучения и
    // перед публикацией.
    internal int ResolveCalls { get; private set; }

    // Подмена записи между обращениями: так проверяется случай «владельца сменил другой писатель».
    internal Action<int>? BeforeResolve { get; set; }

    // Положить владельца в справочник или заменить его запись.
    internal void Put(LogicalUnitDescriptor descriptor, ILogicalUnit<string, string> unit)
        => _entries[descriptor.UnitId] = new ResolvedLogicalUnitRegistryEntry<string, string>(descriptor, unit);

    // Чтение последней версии владельца по опознанию модели.
    public OperationResult<ResolvedLogicalUnitRegistryEntry<string, string>> ResolveLatestOwnerById(string unitId)
    {
        // Номер обращения увеличивается до подмены, чтобы проверка знала, какое обращение идёт.
        ResolveCalls++;
        // Проверка может подменить запись именно перед нужным обращением.
        BeforeResolve?.Invoke(ResolveCalls);
        // Отсутствующий владелец возвращается отказом тем же конвертом, что и у продуктового реестра.
        return _entries.TryGetValue(unitId, out var entry)
            ? OperationResult<ResolvedLogicalUnitRegistryEntry<string, string>>.Ok(entry)
            : OperationResult<ResolvedLogicalUnitRegistryEntry<string, string>>.Fail(
                $"No logical unit registered for exact id '{unitId}'.",
                "LOGICAL_UNIT_ID_NOT_FOUND");
    }
}

// Подставная граница обучения и публикации для чистых проверок. Родного обучения не делает ни
// одного: она только ЗАПИСЫВАЕТ, что именно ей передали, и по приказу проверки отказывает.
internal sealed class StubAddressedCorrectionWriter : IAddressedCorrectionWriter<string>
{
    // Сколько раз связка попросила обучить кандидата. Ноль означает отказ ДО обучения.
    internal int TrainCalls { get; private set; }
    // Сколько раз связка попросила опубликовать кандидата.
    internal int PublishCalls { get; private set; }
    // Объект единицы, который связка передала на обучение. Это доказательство адресности.
    internal ILogicalUnit<string, string>? HandedOverUnit { get; private set; }
    // Приказ проверки «обучение провалилось».
    internal bool FailTraining { get; set; }
    // Приказ проверки «публикация провалилась ПОСЛЕ оплаченного обучения».
    internal bool FailPublication { get; set; }
    // Опознание владельца, которого подставной писатель объявит опубликованным.
    internal AddressedCorrectionOwnerIdentity? PublishAs { get; set; }

    // Обучение кандидата: объект владельца и материал запоминаются, модель не вызывается.
    public OperationResult<AddressedCorrectionCandidate<string>> TrainCorrectedOwner(
        ILogicalUnit<string, string> selectedUnit,
        AddressedCorrectionOwnerIdentity owner,
        string correctedExampleId,
        string correctedAnswer,
        CancellationToken cancellationToken)
    {
        // Счётчик обучений растёт только здесь: по нему проверки судят о «ДО обучения».
        TrainCalls++;
        // Переданный объект запоминается ровно той ссылкой, которой его дали.
        HandedOverUnit = selectedUnit;
        // Приказанный отказ обучения возвращается общим конвертом.
        if (FailTraining)
            return OperationResult<AddressedCorrectionCandidate<string>>.Fail(
                "Подставное обучение отказано по приказу проверки.",
                "STUB_TRAINING_REFUSED");

        // Кандидат возвращается подставной единицей с исправленным ответом.
        return OperationResult<AddressedCorrectionCandidate<string>>.Ok(
            new AddressedCorrectionCandidate<string>(
                owner,
                new StubAddressedCorrectionUnit(correctedAnswer),
                "v2"));
    }

    // Публикация кандидата: по приказу проверки она отказывает ПОСЛЕ оплаченного обучения.
    public OperationResult<AddressedCorrectionOwnerIdentity> PublishCorrectedOwner(
        AddressedCorrectionCandidate<string> candidate,
        CancellationToken cancellationToken)
    {
        // Счётчик публикаций растёт только здесь.
        PublishCalls++;
        // Приказанный отказ публикации возвращается общим конвертом.
        if (FailPublication)
            return OperationResult<AddressedCorrectionOwnerIdentity>.Fail(
                "Подставная публикация отказана по приказу проверки.",
                "STUB_PUBLICATION_REFUSED");

        // Опубликованное опознание либо задано проверкой, либо это тот же владелец новой версией.
        return OperationResult<AddressedCorrectionOwnerIdentity>.Ok(
            PublishAs ?? candidate.Owner with
            {
                Version = candidate.NextVersion,
                RegistrationOrder = candidate.Owner.RegistrationOrder + 1,
                TrainedStateIdentity = $"{candidate.Owner.UnitId}@{candidate.NextVersion}"
            });
    }
}

// Граница обучения и публикации на НАСТОЯЩЕМ родном обучающем блоке ближайших соседей
// (KNN — K-Nearest Neighbours, К ближайших соседей).
//
// Она владеет полным учебным набором каждого владельца, заменяет в НЁМ исправляемую строку и
// обучает нового кандидата настоящим родным обучением. Публикация идёт через настоящий продуктовый
// реестр единиц. Это подставной СОСТАВ проверок: продуктовый допущенный писатель принадлежит
// другому владельцу переноса и в этом срезе не создаётся.
internal sealed class NativeKnnAddressedCorrectionWriter : IAddressedCorrectionWriter<string>
{
    // Настоящий продуктовый реестр: в него публикуется исправленный владелец.
    private readonly InMemoryLogicalUnitRegistry _registry;
    // Полные учебные наборы владельцев по опознанию модели. Writer ими владеет.
    private readonly Dictionary<string, LogicalKnnTrainingExample<string, string>[]> _corpora;

    // Сколько раз выполнено РОДНОЕ обучение исправленного кандидата. Входит в учёт расхода.
    internal int NativeTrainCalls { get; private set; }
    // Объект единицы, который связка передала на обучение. Доказательство адресности.
    internal ILogicalUnit<string, string>? HandedOverUnit { get; private set; }

    // Писатель собирается из реестра и учебных наборов владельцев.
    internal NativeKnnAddressedCorrectionWriter(
        InMemoryLogicalUnitRegistry registry,
        Dictionary<string, LogicalKnnTrainingExample<string, string>[]> corpora)
    {
        _registry = registry;
        _corpora = corpora;
    }

    // Обучить исправленного кандидата на полном наборе ВЫБРАННОГО владельца.
    public OperationResult<AddressedCorrectionCandidate<string>> TrainCorrectedOwner(
        ILogicalUnit<string, string> selectedUnit,
        AddressedCorrectionOwnerIdentity owner,
        string correctedExampleId,
        string correctedAnswer,
        CancellationToken cancellationToken)
    {
        // Переданный объект запоминается ровно той ссылкой, которой его дали.
        HandedOverUnit = selectedUnit;
        // Набор берётся набором ИМЕННО этого владельца: чужой набор обучать нельзя.
        if (!_corpora.TryGetValue(owner.UnitId, out var corpus))
            return OperationResult<AddressedCorrectionCandidate<string>>.Fail(
                $"Учебного набора владельца '{owner.UnitId}' у писателя нет.",
                "OWNED_CORPUS_MISSING");

        // В копии набора заменяется ровно одна назначенная строка урока.
        var corrected = corpus
            .Select(row => string.Equals(row.ExampleId, correctedExampleId, StringComparison.Ordinal)
                ? row with { Output = correctedAnswer }
                : row)
            .ToArray();

        // Исправляемая строка обязана существовать: иначе урок исправляет пустоту.
        if (!corrected.Any(row => string.Equals(row.ExampleId, correctedExampleId, StringComparison.Ordinal)))
            return OperationResult<AddressedCorrectionCandidate<string>>.Fail(
                $"Строки урока '{correctedExampleId}' в наборе владельца '{owner.UnitId}' нет.",
                "CORRECTED_ROW_MISSING");

        // Родное обучение: тот же обучающий блок, те же размеры вектора и то же число соседей.
        NativeTrainCalls++;
        var trained = KnnLogicalUnit<string, string>.Train(
            owner.UnitId,
            owner.UnitRole,
            owner.DomainTag,
            corrected,
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Отказ родного обучения возвращается как есть, вместе с его кодом.
        if (!trained.IsSuccess)
            return OperationResult<AddressedCorrectionCandidate<string>>.Fail(trained.ErrorMessage, trained.ErrorCode);

        // Исправленный набор становится набором владельца только вместе с обученным кандидатом.
        _corpora[owner.UnitId] = corrected;

        // Кандидат возвращается ещё НЕ опубликованным: публикация — отдельный шаг.
        return OperationResult<AddressedCorrectionCandidate<string>>.Ok(
            new AddressedCorrectionCandidate<string>(owner, trained.Value!, "v2"));
    }

    // Опубликовать ТОЛЬКО выбранного владельца новой версией в настоящем реестре.
    public OperationResult<AddressedCorrectionOwnerIdentity> PublishCorrectedOwner(
        AddressedCorrectionCandidate<string> candidate,
        CancellationToken cancellationToken)
    {
        // Регистрация новой версии идёт продуктовым путём реестра, а не подменой записи.
        var registered = _registry.Register<string, string>(
            candidate.Owner.UnitId,
            candidate.Owner.UnitRole,
            candidate.Owner.DomainTag,
            candidate.NextVersion,
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            candidate.CorrectedUnit);

        // Отказ реестра возвращается как есть.
        if (!registered.IsSuccess)
            return OperationResult<AddressedCorrectionOwnerIdentity>.Fail(registered.ErrorMessage, registered.ErrorCode);

        // Опознание состояния ОБЪЯВЛЯЕТСЯ писателем. Это не отпечаток байтов состояния: полный
        // носитель состояния принадлежит своему владельцу и здесь не создаётся.
        return OperationResult<AddressedCorrectionOwnerIdentity>.Ok(candidate.Owner with
        {
            Version = registered.Value!.Version,
            RegistrationOrder = registered.Value.RegistrationOrder,
            TrainedStateIdentity = $"{candidate.Owner.UnitId}@{registered.Value.Version}"
        });
    }
}

// Общая неизменяемая подготовка РОДНОГО обучения: два владельца, одинаковый текст запроса, разные
// ответы. Обучается ОДИН раз на весь класс проверок: 2 родных обучения.
public sealed class AddressedCorrectionNativePanel
{
    // Домен обоих владельцев. Он один: адресом служит опознание модели, а не домен.
    internal const string Domain = "credit-domain";
    // Роль обоих владельцев. Она одна: роль — это место, которое владелец занимает, а не его адрес.
    internal const string Role = "credit-writer";
    // Опознание первой модели — того владельца, который отвечает «review».
    internal const string OwnerOneId = "unit-credit-owner-one";
    // Опознание второй модели — того владельца, который отвечает «archive».
    internal const string OwnerTwoId = "unit-credit-owner-two";
    // Одинаковый для обоих владельцев текст запроса: именно он и не может быть адресом.
    internal const string SharedPrompt = "approve report";
    // Опознание исправляемой строки урока. Оно одно у обоих: строки принадлежат разным наборам.
    internal const string CorrectedRowId = "row-approve-report";

    // Учебный набор первого владельца: исправляемая строка плюс явно другая строка.
    internal LogicalKnnTrainingExample<string, string>[] OwnerOneCorpus { get; }
    // Учебный набор второго владельца той же формы.
    internal LogicalKnnTrainingExample<string, string>[] OwnerTwoCorpus { get; }
    // Обученный первый владелец. Объект неизменяемый и переиспользуется всеми проверками.
    internal KnnLogicalUnit<string, string> OwnerOneUnit { get; }
    // Обученный второй владелец.
    internal KnnLogicalUnit<string, string> OwnerTwoUnit { get; }

    // Подготовка выполняет ровно два родных обучения и больше ничего.
    public AddressedCorrectionNativePanel()
    {
        // Набор первого владельца: на общий запрос он обучен отвечать «review».
        OwnerOneCorpus =
        [
            new LogicalKnnTrainingExample<string, string>(
                CorrectedRowId, SharedPrompt, "review", ["APPROVAL"], [Domain], [CorrectedRowId]),
            new LogicalKnnTrainingExample<string, string>(
                "row-archive-ticket", "archive closed ticket", "close", ["ARCHIVE"], [Domain], ["row-archive-ticket"])
        ];

        // Набор второго владельца: на ТОТ ЖЕ общий запрос он обучен отвечать «archive».
        OwnerTwoCorpus =
        [
            new LogicalKnnTrainingExample<string, string>(
                CorrectedRowId, SharedPrompt, "archive", ["APPROVAL"], [Domain], [CorrectedRowId]),
            new LogicalKnnTrainingExample<string, string>(
                "row-archive-ticket", "archive closed ticket", "close", ["ARCHIVE"], [Domain], ["row-archive-ticket"])
        ];

        // Первое родное обучение подготовки.
        var one = KnnLogicalUnit<string, string>.Train(
            OwnerOneId, Role, Domain, OwnerOneCorpus, input => input, vectorDimensions: 32, k: 1);
        // Неудача подготовки обязана быть видна сразу, а не превратиться в непонятный отказ позже.
        Assert.True(one.IsSuccess, one.ErrorMessage);
        OwnerOneUnit = one.Value!;

        // Второе родное обучение подготовки.
        var two = KnnLogicalUnit<string, string>.Train(
            OwnerTwoId, Role, Domain, OwnerTwoCorpus, input => input, vectorDimensions: 32, k: 1);
        Assert.True(two.IsSuccess, two.ErrorMessage);
        OwnerTwoUnit = two.Value!;
    }
}

// Решающие проверки адресной доставки исправляющего урока.
public sealed class KnnAddressedCorrectionTests : IClassFixture<AddressedCorrectionNativePanel>
{
    // Общая неизменяемая подготовка родного обучения.
    private readonly AddressedCorrectionNativePanel _panel;

    // Проверки получают подготовку один раз на класс: повторных обучений подготовки нет.
    public KnnAddressedCorrectionTests(AddressedCorrectionNativePanel panel) => _panel = panel;

    // Каталог долговечных отметок этой проверки. Свой у каждой, чтобы проверки не мешали друг другу.
    private static string NewJournalRoot(string name)
        => Path.Combine(
            Path.GetTempPath(),
            "llm-addressed-correction-journal",
            $"{name}-{Guid.NewGuid():N}");

    // Описание владельца для подставного справочника.
    private static LogicalUnitDescriptor Descriptor(string unitId, string version, long order)
        => new(
            unitId,
            AddressedCorrectionNativePanel.Role,
            AddressedCorrectionNativePanel.Domain,
            version,
            LogicalUnitTrustState.Trusted,
            IsLearned: true,
            typeof(string),
            typeof(string),
            order);

    // Опознание владельца, собранное из его описания.
    private static AddressedCorrectionOwnerIdentity Owner(LogicalUnitDescriptor descriptor)
        => new(
            descriptor.DomainTag,
            descriptor.DomainTag,
            descriptor.UnitId,
            descriptor.UnitRole,
            descriptor.Version,
            descriptor.RegistrationOrder,
            $"{descriptor.UnitId}@{descriptor.Version}");

    // Опознание фактического ответа ТОГО ЖЕ владельца в том же запуске.
    private static AddressedCorrectionResponseEvidence EvidenceOf(
        string runId,
        AddressedCorrectionOwnerIdentity owner)
        => new(
            runId,
            owner.UnitId,
            owner.DomainTag,
            owner.Version,
            owner.RegistrationOrder,
            owner.TrainedStateIdentity);

    // Запрос адресной доставки, собранный из опознания владельца и его фактического ответа.
    private static AddressedCorrection<string> Request(
        string runId,
        string feedbackId,
        AddressedCorrectionOwnerIdentity owner,
        string correctedAnswer,
        AddressedCorrectionResponseEvidence? evidence = null)
        => new(
            runId,
            feedbackId,
            owner,
            evidence ?? EvidenceOf(runId, owner),
            AddressedCorrectionNativePanel.CorrectedRowId,
            correctedAnswer);

    // Один обычный вопрос обученной единице общим текстом запроса.
    private static async Task<string> AskAsync(ILogicalUnit<string, string> unit, string unitId)
    {
        // Вопрос задаётся тем же общим текстом, который обучен у обоих владельцев.
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-credit-readout",
            unitId,
            AddressedCorrectionNativePanel.Role,
            AddressedCorrectionNativePanel.Domain,
            AddressedCorrectionNativePanel.SharedPrompt,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        // Неудача обычного чтения обязана быть видна сразу.
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        // Наружу отдаётся сам ответ: именно он сравнивается точной строкой.
        return answer.Value!.Output;
    }

    // Собрать настоящий реестр с обоими обученными владельцами и вернуть их живые описания.
    private (InMemoryLogicalUnitRegistry Registry, LogicalUnitDescriptor One, LogicalUnitDescriptor Two) NewNativeRegistry()
    {
        // Настоящий продуктовый реестр единиц. Для каждой проверки свой: порядок не наследуется.
        var registry = new InMemoryLogicalUnitRegistry();
        // Первый владелец регистрируется первым: это и есть тот «первый зарегистрированный»,
        // которого ошибочный образец выбирал всегда.
        var one = registry.Register<string, string>(
            AddressedCorrectionNativePanel.OwnerOneId,
            AddressedCorrectionNativePanel.Role,
            AddressedCorrectionNativePanel.Domain,
            "v1",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            _panel.OwnerOneUnit);
        Assert.True(one.IsSuccess, one.ErrorMessage);
        // Второй владелец регистрируется вторым.
        var two = registry.Register<string, string>(
            AddressedCorrectionNativePanel.OwnerTwoId,
            AddressedCorrectionNativePanel.Role,
            AddressedCorrectionNativePanel.Domain,
            "v1",
            LogicalUnitTrustState.Trusted,
            isLearned: true,
            _panel.OwnerTwoUnit);
        Assert.True(two.IsSuccess, two.ErrorMessage);
        // Реестр и оба живых описания отдаются проверке.
        return (registry, one.Value!, two.Value!);
    }

    // CREDIT-SELECTED. Требование 2: урок применяется к ВЫБРАННОМУ владельцу, чужая память прежняя.
    // Родное обучение: 1 исправленный кандидат. Обычных чтений: 4.
    [Fact]
    public async Task CorrectionChangesSelectedOwnerOnly()
    {
        // Настоящий реестр с обоими обученными владельцами.
        var (registry, one, two) = NewNativeRegistry();
        // Исходные ответы читаются ДО исправления: они и есть «до».
        Assert.Equal("review", await AskAsync(_panel.OwnerOneUnit, one.UnitId));
        Assert.Equal("archive", await AskAsync(_panel.OwnerTwoUnit, two.UnitId));

        // Писатель владеет полными наборами обоих владельцев.
        var writer = new NativeKnnAddressedCorrectionWriter(registry, new()
        {
            [one.UnitId] = _panel.OwnerOneCorpus,
            [two.UnitId] = _panel.OwnerTwoCorpus
        });
        // Связка собирается на настоящем реестре через продуктовый переходник справочника.
        var journalRoot = NewJournalRoot("credit-selected");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(
            new LogicalUnitRegistryAddressedCorrectionDirectory<string>(registry),
            writer,
            journalRoot);

        // Исправление адресовано ВТОРОМУ владельцу: он отвечал «archive».
        var applied = await coordinator.ApplyAsync(
            Request("run-credit-01", "feedback-credit-01", Owner(two), "archive then notify"));
        // Доставка обязана состояться.
        Assert.True(applied.IsSuccess, applied.ErrorMessage);

        // Родное обучение выполнено ровно один раз: один урок — одно обучение.
        Assert.Equal(1, writer.NativeTrainCalls);
        // Доказательство адресности: писателю передали ФАКТИЧЕСКИ тот объект, который отвечал.
        Assert.Same(_panel.OwnerTwoUnit, writer.HandedOverUnit);

        // Квитанция обязана раздельно называть версию, которая ОТВЕЧАЛА, и версию, которая
        // ОПУБЛИКОВАНА: если их смешать, переход версий станет невидимым.
        Assert.Equal("v1", applied.Value!.SelectedVersion);
        Assert.Equal("v2", applied.Value.PublishedVersion);
        Assert.True(applied.Value.PublishedRegistrationOrder > applied.Value.SelectedRegistrationOrder);
        // Квитанция называет именно выбранного владельца.
        Assert.Equal(two.UnitId, applied.Value.UnitId);

        // Выбранный владелец читается из реестра заново и отвечает ИСПРАВЛЕННЫМ ответом.
        var correctedTwo = registry.ResolveLatestById<string, string>(two.UnitId);
        Assert.True(correctedTwo.IsSuccess, correctedTwo.ErrorMessage);
        Assert.Equal("archive then notify", await AskAsync(correctedTwo.Value!.Unit, two.UnitId));

        // Чужая память осталась прежней: первый владелец по-прежнему отвечает «review».
        var untouchedOne = registry.ResolveLatestById<string, string>(one.UnitId);
        Assert.True(untouchedOne.IsSuccess, untouchedOne.ErrorMessage);
        Assert.Equal("v1", untouchedOne.Value!.Descriptor.Version);
        Assert.Equal(one.RegistrationOrder, untouchedOne.Value.Descriptor.RegistrationOrder);
        Assert.Equal("review", await AskAsync(untouchedOne.Value.Unit, one.UnitId));

        // Каталог отметок этой проверки удаляется: своих временных каталогов проверка не оставляет.
        Directory.Delete(journalRoot, recursive: true);
    }

    // CREDIT-PARITY. Требование 2: выбор ПЕРВОГО зарегистрированного владельца тоже законен.
    // Это положительный контроль: правильный адрес не обязан быть «не первым».
    // Родное обучение: 1 исправленный кандидат. Обычных чтений: 2.
    [Fact]
    public async Task FirstSelectedOwnerRemainsValid()
    {
        // Настоящий реестр с обоими обученными владельцами.
        var (registry, one, two) = NewNativeRegistry();
        // Писатель владеет полными наборами обоих владельцев.
        var writer = new NativeKnnAddressedCorrectionWriter(registry, new()
        {
            [one.UnitId] = _panel.OwnerOneCorpus,
            [two.UnitId] = _panel.OwnerTwoCorpus
        });
        // Связка собирается на настоящем реестре.
        var journalRoot = NewJournalRoot("credit-parity");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(
            new LogicalUnitRegistryAddressedCorrectionDirectory<string>(registry),
            writer,
            journalRoot);

        // Исправление адресовано ПЕРВОМУ владельцу: он отвечал «review».
        var applied = await coordinator.ApplyAsync(
            Request("run-credit-02", "feedback-credit-02", Owner(one), "review then notify"));
        Assert.True(applied.IsSuccess, applied.ErrorMessage);
        // Родное обучение выполнено ровно один раз.
        Assert.Equal(1, writer.NativeTrainCalls);
        // Писателю передали фактический объект первого владельца.
        Assert.Same(_panel.OwnerOneUnit, writer.HandedOverUnit);

        // Первый владелец отвечает исправленным ответом.
        var correctedOne = registry.ResolveLatestById<string, string>(one.UnitId);
        Assert.True(correctedOne.IsSuccess, correctedOne.ErrorMessage);
        Assert.Equal("review then notify", await AskAsync(correctedOne.Value!.Unit, one.UnitId));

        // Второй владелец остался прежним: чужая память не изменилась.
        var untouchedTwo = registry.ResolveLatestById<string, string>(two.UnitId);
        Assert.True(untouchedTwo.IsSuccess, untouchedTwo.ErrorMessage);
        Assert.Equal("archive", await AskAsync(untouchedTwo.Value!.Unit, two.UnitId));

        // Свой временный каталог проверка удаляет.
        Directory.Delete(journalRoot, recursive: true);
    }

    // CREDIT-MISSING. Требование 1: неизвестный владелец и чужой домен отвергаются ДО обучения.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task UnknownOrForeignBindingRefusesBeforeFit()
    {
        // Подставной справочник: в нём живёт только второй владелец.
        var directory = new StubAddressedCorrectionDirectory();
        var twoDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(twoDescriptor, new StubAddressedCorrectionUnit("archive"));
        // Подставной писатель считает обучения и публикации.
        var writer = new StubAddressedCorrectionWriter();
        var journalRoot = NewJournalRoot("credit-missing");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Случай первый: владельца с таким опознанием модели в справочнике нет вовсе.
        var missingOwner = Owner(Descriptor("unit-credit-owner-missing", "v1", 9));
        var missing = await coordinator.ApplyAsync(
            Request("run-credit-03", "feedback-credit-03", missingOwner, "review then notify"));
        // Отказ обязан быть именно «неизвестный выбранный владелец».
        Assert.False(missing.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.UnknownSelectedOwnerErrorCode,
            missing.ErrorCode);

        // Случай второй: владелец существует, но урок адресован ЧУЖОМУ домену.
        var foreignDomainOwner = Owner(twoDescriptor) with { DomainTag = "other-domain", Profile = "other-domain" };
        var foreign = await coordinator.ApplyAsync(Request(
            "run-credit-03",
            "feedback-credit-04",
            foreignDomainOwner,
            "archive then notify"));
        // Отказ обязан быть именно «чужой домен владельца».
        Assert.False(foreign.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.ForeignOwnerDomainErrorCode,
            foreign.ErrorCode);

        // Случай третий: домен верный, а профиль каталога правил ранжирования чужой.
        var foreignProfileOwner = Owner(twoDescriptor) with { Profile = "other-profile" };
        var foreignProfile = await coordinator.ApplyAsync(Request(
            "run-credit-03",
            "feedback-credit-05",
            foreignProfileOwner,
            "archive then notify"));
        Assert.False(foreignProfile.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.ForeignOwnerProfileErrorCode,
            foreignProfile.ErrorCode);

        // Ни одного обучения и ни одной публикации: все три отказа случились ДО обучения.
        Assert.Equal(0, writer.TrainCalls);
        Assert.Equal(0, writer.PublishCalls);
        // Отметок применения на диске не возникло: каталога журнала нет вовсе.
        Assert.False(Directory.Exists(journalRoot));
    }

    // Требование 1 и 2: исправление, чьё опознание ответа называет ДРУГОГО владельца, отвергается.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task CorrectionWhoseEvidenceNamesAnotherOwnerIsRefused()
    {
        // В справочнике живут оба владельца.
        var directory = new StubAddressedCorrectionDirectory();
        var oneDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerOneId, "v1", 1);
        var twoDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(oneDescriptor, new StubAddressedCorrectionUnit("review"));
        directory.Put(twoDescriptor, new StubAddressedCorrectionUnit("archive"));
        var writer = new StubAddressedCorrectionWriter();
        var journalRoot = NewJournalRoot("credit-evidence");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Урок адресован второму владельцу, а опознание ответа называет ПЕРВОГО.
        var crossed = await coordinator.ApplyAsync(Request(
            "run-credit-04",
            "feedback-credit-06",
            Owner(twoDescriptor),
            "archive then notify",
            EvidenceOf("run-credit-04", Owner(oneDescriptor))));
        // Отказ обязан быть именно «чужое опознание ответа».
        Assert.False(crossed.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.ForeignResponseEvidenceErrorCode,
            crossed.ErrorCode);

        // Ответ ДРУГОГО запуска тоже не исправляется этим обращением.
        var otherRun = await coordinator.ApplyAsync(Request(
            "run-credit-04",
            "feedback-credit-07",
            Owner(twoDescriptor),
            "archive then notify",
            EvidenceOf("run-credit-99", Owner(twoDescriptor))));
        Assert.False(otherRun.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.AnswerFromAnotherRunErrorCode,
            otherRun.ErrorCode);

        // Обучения не было: оба отказа случились ДО него.
        Assert.Equal(0, writer.TrainCalls);
        Assert.False(Directory.Exists(journalRoot));
    }

    // CREDIT-BINDING. Требование 1: версия владельца — часть связки, устаревшая отвергается ДО fit.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task StaleSelectedVersionRefusesBeforeFit()
    {
        // В справочнике владелец живёт уже ВТОРОЙ версией.
        var directory = new StubAddressedCorrectionDirectory();
        var liveDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v2", 5);
        directory.Put(liveDescriptor, new StubAddressedCorrectionUnit("archive then notify"));
        var writer = new StubAddressedCorrectionWriter();
        var journalRoot = NewJournalRoot("credit-binding");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Урок пришёл с ПЕРВОЙ версией: он относился к уже сменившемуся состоянию.
        var staleOwner = Owner(Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2));
        var stale = await coordinator.ApplyAsync(
            Request("run-credit-05", "feedback-credit-08", staleOwner, "archive then escalate"));
        // Отказ обязан быть именно «устаревшая выбранная версия».
        Assert.False(stale.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.StaleSelectedVersionErrorCode,
            stale.ErrorCode);
        // Обучения не было: отказ случился ДО него.
        Assert.Equal(0, writer.TrainCalls);
        Assert.False(Directory.Exists(journalRoot));
    }

    // CREDIT-IDEMPOTENT. Требование 4: повторный тот же урок не применяется второй раз.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task CompletedDeliveryDoesNotTrainAgain()
    {
        // В справочнике живёт второй владелец первой версией.
        var directory = new StubAddressedCorrectionDirectory();
        var descriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(descriptor, new StubAddressedCorrectionUnit("archive"));
        var writer = new StubAddressedCorrectionWriter();
        var journalRoot = NewJournalRoot("credit-idempotent");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Первая доставка проходит и оплачивает одно обучение.
        var first = await coordinator.ApplyAsync(
            Request("run-credit-06", "feedback-credit-09", Owner(descriptor), "archive then notify"));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(1, writer.TrainCalls);
        Assert.Equal(1, writer.PublishCalls);
        // Первая квитанция создана сейчас, а не перечитана.
        Assert.False(first.Value!.ReusedCompletedDelivery);

        // Повторное ТО ЖЕ обращение не обучает второй раз и возвращает ПРЕЖНЮЮ квитанцию.
        var second = await coordinator.ApplyAsync(
            Request("run-credit-06", "feedback-credit-09", Owner(descriptor), "archive then notify"));
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, writer.TrainCalls);
        Assert.Equal(1, writer.PublishCalls);
        // Эта квитанция прочитана с диска.
        Assert.True(second.Value!.ReusedCompletedDelivery);
        // И это ровно та же квитанция по всем значимым полям.
        Assert.Equal(first.Value.DeliveryKey, second.Value.DeliveryKey);
        Assert.Equal(first.Value.PublishedVersion, second.Value.PublishedVersion);
        Assert.Equal(first.Value.PublishedRegistrationOrder, second.Value.PublishedRegistrationOrder);

        // ДРУГОЕ обращение пользователя дублем не считается: оно обучает заново.
        var otherFeedback = await coordinator.ApplyAsync(Request(
            "run-credit-06",
            "feedback-credit-10",
            Owner(descriptor),
            "archive then escalate"));
        Assert.True(otherFeedback.IsSuccess, otherFeedback.ErrorMessage);
        Assert.Equal(2, writer.TrainCalls);
        Assert.False(otherFeedback.Value!.ReusedCompletedDelivery);

        // Свой временный каталог проверка удаляет.
        Directory.Delete(journalRoot, recursive: true);
    }

    // CREDIT-AFTER-SUCCESS. Требование 4, слово «ПОСЛЕ»: при НЕУДАЧНОМ применении отметки
    // дедупликации не возникает, и урок остаётся применимым.
    //
    // Это ловушка исходного опытного образца: он заполнял набор доставок ещё на решении, ДО
    // обучения, и потому терял урок молча. Чистая проверка: родных обучений 0.
    [Fact]
    public async Task FailedApplicationLeavesLessonApplicableWithoutDedupMark()
    {
        // В справочнике живёт второй владелец первой версией.
        var directory = new StubAddressedCorrectionDirectory();
        var descriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(descriptor, new StubAddressedCorrectionUnit("archive"));
        // Писателю приказано провалить ПУБЛИКАЦИЮ после уже оплаченного обучения.
        var writer = new StubAddressedCorrectionWriter { FailPublication = true };
        var journalRoot = NewJournalRoot("credit-after-success");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);
        // Один и тот же запрос используется трижды: он и есть «тот же урок».
        var request = Request("run-credit-07", "feedback-credit-11", Owner(descriptor), "archive then notify");

        // Случай первый: публикация провалилась ПОСЛЕ обучения.
        var failedPublication = await coordinator.ApplyAsync(request);
        // Доставка не состоялась, и причина — отказ публикации.
        Assert.False(failedPublication.IsSuccess);
        Assert.Equal("STUB_PUBLICATION_REFUSED", failedPublication.ErrorCode);
        // Обучение было оплачено один раз, публикация была одна и она отказала.
        Assert.Equal(1, writer.TrainCalls);
        Assert.Equal(1, writer.PublishCalls);
        // Ключевое утверждение: ОТМЕТКИ ПРИМЕНЕНИЯ НЕТ. Каталога журнала не возникло вовсе.
        Assert.False(Directory.Exists(journalRoot));
        // И по самому ключу доставки отметка не находится.
        var keyAfterFailure = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey(
            request.RunId, request.FeedbackId, request.Owner);
        var lookupAfterFailure = AddressedCorrectionDeliveryJournal.TryLoadApplied(journalRoot, keyAfterFailure);
        Assert.True(lookupAfterFailure.IsSuccess, lookupAfterFailure.ErrorMessage);
        Assert.False(lookupAfterFailure.Value!.Found);

        // Случай второй: обучение провалилось. Отметки по-прежнему нет.
        writer.FailPublication = false;
        writer.FailTraining = true;
        var failedTraining = await coordinator.ApplyAsync(request);
        Assert.False(failedTraining.IsSuccess);
        Assert.Equal("STUB_TRAINING_REFUSED", failedTraining.ErrorCode);
        // Публикации во втором случае не было вовсе: обучение не дошло до неё.
        Assert.Equal(2, writer.TrainCalls);
        Assert.Equal(1, writer.PublishCalls);
        Assert.False(Directory.Exists(journalRoot));

        // Случай третий: урок ОСТАЛСЯ ПРИМЕНИМЫМ. Та же доставка проходит после снятия отказов.
        writer.FailTraining = false;
        var recovered = await coordinator.ApplyAsync(request);
        // Та же доставка состоялась: неудача не съела урок.
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        Assert.False(recovered.Value!.ReusedCompletedDelivery);
        // Третье обучение и вторая публикация — это состоявшееся применение.
        Assert.Equal(3, writer.TrainCalls);
        Assert.Equal(2, writer.PublishCalls);
        // И только ТЕПЕРЬ отметка применения появилась на диске.
        Assert.True(File.Exists(AddressedCorrectionDeliveryJournal.ResolvePath(
            journalRoot, recovered.Value.DeliveryKey)));

        // Свой временный каталог проверка удаляет.
        Directory.Delete(journalRoot, recursive: true);
    }

    // CREDIT-RESTORE. Требование 3: после поднятия отметки С ДИСКА адресная доставка продолжает
    // работать, и связка домена, модели и версии сохраняется.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task FreshLoadKeepsOwnerAndDeliveryIdentity()
    {
        // Каталог журнала один на всю проверку: именно он переживает «перезапуск».
        var journalRoot = NewJournalRoot("credit-restore");
        var descriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        // Один и тот же запрос применяется до и после «перезапуска».
        var request = Request("run-credit-08", "feedback-credit-12", Owner(descriptor), "archive then notify");

        // Первая жизнь процесса: свой справочник, свой писатель, своя связка.
        var firstDirectory = new StubAddressedCorrectionDirectory();
        firstDirectory.Put(descriptor, new StubAddressedCorrectionUnit("archive"));
        var firstWriter = new StubAddressedCorrectionWriter();
        var firstCoordinator = new KnnAddressedCorrectionCoordinator<string>(
            firstDirectory, firstWriter, journalRoot);
        var applied = await firstCoordinator.ApplyAsync(request);
        Assert.True(applied.IsSuccess, applied.ErrorMessage);
        Assert.Equal(1, firstWriter.TrainCalls);

        // Отметка применения лежит именно на диске, и читается обратно теми же значениями.
        var markPath = AddressedCorrectionDeliveryJournal.ResolvePath(journalRoot, applied.Value!.DeliveryKey);
        Assert.True(File.Exists(markPath));
        var readBack = AddressedCorrectionDeliveryJournal.TryLoadApplied(journalRoot, applied.Value.DeliveryKey);
        Assert.True(readBack.IsSuccess, readBack.ErrorMessage);
        Assert.True(readBack.Value!.Found);
        // Связка домена, модели и версии сохранена в самой отметке.
        Assert.Equal(AddressedCorrectionNativePanel.Domain, readBack.Value.Receipt!.DomainTag);
        Assert.Equal(AddressedCorrectionNativePanel.Domain, readBack.Value.Receipt.Profile);
        Assert.Equal(AddressedCorrectionNativePanel.OwnerTwoId, readBack.Value.Receipt.UnitId);
        Assert.Equal(AddressedCorrectionNativePanel.Role, readBack.Value.Receipt.UnitRole);
        Assert.Equal("v1", readBack.Value.Receipt.SelectedVersion);
        Assert.Equal("v2", readBack.Value.Receipt.PublishedVersion);

        // Вторая жизнь процесса: ПОЛНОСТЬЮ новые справочник, писатель и связка над тем же каталогом.
        var freshDirectory = new StubAddressedCorrectionDirectory();
        freshDirectory.Put(descriptor, new StubAddressedCorrectionUnit("archive"));
        var freshWriter = new StubAddressedCorrectionWriter();
        var freshCoordinator = new KnnAddressedCorrectionCoordinator<string>(
            freshDirectory, freshWriter, journalRoot);

        // Тот же урок после поднятия отметки с диска не применяется второй раз.
        var afterRestore = await freshCoordinator.ApplyAsync(request);
        Assert.True(afterRestore.IsSuccess, afterRestore.ErrorMessage);
        // У нового писателя обучений НОЛЬ: отметка пришла с диска, а не из памяти прошлой связки.
        Assert.Equal(0, freshWriter.TrainCalls);
        Assert.Equal(0, freshWriter.PublishCalls);
        Assert.True(afterRestore.Value!.ReusedCompletedDelivery);
        // Прежняя квитанция восстановлена целиком, вместе со связкой.
        Assert.Equal(applied.Value.DeliveryKey, afterRestore.Value.DeliveryKey);
        Assert.Equal(applied.Value.UnitId, afterRestore.Value.UnitId);
        Assert.Equal(applied.Value.SelectedVersion, afterRestore.Value.SelectedVersion);
        Assert.Equal(applied.Value.PublishedVersion, afterRestore.Value.PublishedVersion);

        // Доказательство того, что признак живёт НА ДИСКЕ, а не в памяти процесса: после удаления
        // файла отметки тот же урок снова становится применимым, и новая связка его применяет.
        File.Delete(markPath);
        var afterMarkRemoved = await freshCoordinator.ApplyAsync(request);
        Assert.True(afterMarkRemoved.IsSuccess, afterMarkRemoved.ErrorMessage);
        Assert.Equal(1, freshWriter.TrainCalls);
        Assert.False(afterMarkRemoved.Value!.ReusedCompletedDelivery);

        // Свой временный каталог проверка удаляет.
        Directory.Delete(journalRoot, recursive: true);
    }

    // Требование 2: публикация ЧУЖОГО владельца отвергается, даже если обучение прошло.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task PublicationOfAnotherOwnerIsRefused()
    {
        // В справочнике живут оба владельца.
        var directory = new StubAddressedCorrectionDirectory();
        var oneDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerOneId, "v1", 1);
        var twoDescriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(oneDescriptor, new StubAddressedCorrectionUnit("review"));
        directory.Put(twoDescriptor, new StubAddressedCorrectionUnit("archive"));
        // Писателю приказано объявить опубликованным ПЕРВОГО владельца вместо второго.
        var writer = new StubAddressedCorrectionWriter
        {
            PublishAs = Owner(oneDescriptor) with { Version = "v2", RegistrationOrder = 3 }
        };
        var journalRoot = NewJournalRoot("credit-published-other");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Урок адресован второму владельцу, а публикация называет первого.
        var applied = await coordinator.ApplyAsync(
            Request("run-credit-09", "feedback-credit-13", Owner(twoDescriptor), "archive then notify"));
        // Отказ обязан быть именно «опубликован другой владелец».
        Assert.False(applied.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.PublishedOtherOwnerErrorCode,
            applied.ErrorCode);
        // Отметки применения не возникло: доставка не состоялась.
        Assert.False(Directory.Exists(journalRoot));
    }

    // Требование 1 и 4: владельца сменил другой писатель между обучением и публикацией.
    // Публикация отменяется, повторного обучения нет, отметки применения нет.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public async Task OwnerMovedBetweenTrainingAndPublicationRefusesPublication()
    {
        // В справочнике живёт второй владелец первой версией.
        var directory = new StubAddressedCorrectionDirectory();
        var descriptor = Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2);
        directory.Put(descriptor, new StubAddressedCorrectionUnit("archive"));
        // Перед ВТОРЫМ обращением связки к справочнику владельца подменяет другой писатель.
        directory.BeforeResolve = call =>
        {
            // Второе обращение — это перепроверка перед публикацией.
            if (call == 2)
                directory.Put(
                    Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v9", 77),
                    new StubAddressedCorrectionUnit("archive by another writer"));
        };
        var writer = new StubAddressedCorrectionWriter();
        var journalRoot = NewJournalRoot("credit-owner-moved");
        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);

        // Доставка начинается с живой версии, но к публикации владелец уже сменился.
        var applied = await coordinator.ApplyAsync(
            Request("run-credit-10", "feedback-credit-14", Owner(descriptor), "archive then notify"));
        // Отказ обязан быть именно «владельца сменили перед публикацией».
        Assert.False(applied.IsSuccess);
        Assert.Equal(
            KnnAddressedCorrectionCoordinator<string>.OwnerMovedBeforePublicationErrorCode,
            applied.ErrorCode);
        // Обучение было оплачено один раз, повторного обучения нет, публикации нет вовсе.
        Assert.Equal(1, writer.TrainCalls);
        Assert.Equal(0, writer.PublishCalls);
        // Отметки применения нет: урок остаётся применимым.
        Assert.False(Directory.Exists(journalRoot));
    }

    // CREDIT-KEY. Ключ доставки кодируется ОДНОЗНАЧНО: длина поля перед его байтами, а не склейка
    // через разделитель. Иначе две РАЗНЫЕ доставки получили бы один ключ, и вторая, ещё не
    // применённая, молча считалась бы применённой.
    // Чистая проверка: родных обучений 0.
    [Fact]
    public void DeliveryKeyIsLengthPrefixedNotAmbiguousConcatenation()
    {
        // Опознание владельца одно и то же у обеих доставок.
        var owner = Owner(Descriptor(AddressedCorrectionNativePanel.OwnerTwoId, "v1", 2));

        // Две РАЗНЫЕ доставки, у которых склейка через вертикальную черту дала бы одну строку:
        // «run|part» + «feedback» против «run» + «part|feedback».
        var left = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey("run|part", "feedback", owner);
        var right = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey("run", "part|feedback", owner);
        // Ключи обязаны различаться: границу полей нельзя сдвинуть содержимым поля.
        Assert.NotEqual(left, right);

        // То же для границы между обращением и доменом владельца.
        var leftDomain = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey(
            "run", "feedback|credit", owner with { DomainTag = "domain", Profile = "domain" });
        var rightDomain = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey(
            "run", "feedback", owner with { DomainTag = "credit|domain", Profile = "credit|domain" });
        Assert.NotEqual(leftDomain, rightDomain);

        // Отсутствующее поле не совпадает с пустым: это разные доставки.
        var emptyFeedback = AddressedCorrectionDeliveryJournal.ComposeDeliveryKey("run", string.Empty, owner);
        Assert.NotEqual(emptyFeedback, AddressedCorrectionDeliveryJournal.ComposeDeliveryKey("run", "x", owner));

        // Один и тот же состав полей обязан давать один и тот же ключ: иначе дедупликация не
        // работала бы вовсе.
        Assert.Equal(
            AddressedCorrectionDeliveryJournal.ComposeDeliveryKey("run|part", "feedback", owner),
            left);
    }
}
