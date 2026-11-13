// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Журнал состоявшихся выдач ответа и путь от обычной выдачи к адресному исправлению.
//
// Зачем это нужно, по-человечески. Координатор адресного исправления уже умеет довести урок до
// той единицы, которая отвечала, и не задеть соседей. Но до этой правки к нему не вело ни одного
// продуктового пути: обычная выдача ответа пользователю ничего о себе не запоминала, и когда
// приходило подтверждённое исправление, адресата было неоткуда взять. В опыте это дало ровно
// половину правильных адресов (1 из 2) и одно изменение ЧУЖОЙ памяти, потому что получателя
// угадывали по порядку регистрации.
//
// Что делает этот журнал. Ровно две вещи, названные карточкой переноса:
//   1) в точке обычной выдачи запоминает, КАКАЯ единица была выбрана и КАКОЙ ответ фактически
//      показан пользователю;
//   2) когда по ЭТОЙ выдаче приходит подтверждённое исправление, передаёт его вместе с выбранным
//      получателем в УЖЕ СУЩЕСТВУЮЩИЙ координатор адресного исправления.
//
// Чем это ЯВЛЯЕТСЯ по уровню устройства. Это ДЕТЕРМИНИРОВАННАЯ СВЯЗКА (deterministic plumbing)
// внутри уже разрешённого пути единиц: она запоминает факт выдачи и передаёт материал дальше.
//
// Чем это НЕ является, названо прямо:
//   * это НЕ обучаемый блок: журнал не выучивает ни одного веса и ни одного счётчика
//     предпочтения, обучением занимается родной обучающий блок за границей писателя исправления;
//   * это НЕ выбор получателя: получателя ВЫБРАЛ обычный путь выдачи, а журнал только запомнил
//     его опознание и сверил, что ответивший объект — это и есть зарегистрированный владелец;
//   * это НЕ доказательство побайтовой сохранности обученного состояния: опознание состояния
//     здесь СОСТАВЛЯЕТСЯ из опознания владельца в реестре и дальше только СВЕРЯЕТСЯ;
//   * это НЕ контрольная точка обучаемого блока: запись о выдаче не является сохранённой моделью,
//     и отсутствие родного сохранения и загрузки у блока ближайших соседей ею не прикрывается.

// Запомненная выдача: кто ответил и что именно было показано пользователю.
//
// Ссылка на выбранную единицу хранится ОБЪЕКТОМ, а не только именем: одинаковое имя роли у двух
// владельцев — обычное дело, и адресом оно быть не может. Объект же отличает владельцев всегда.
public sealed record AnsweredDelivery<TOutput>(
    string RunId,
    string DeliveryId,
    AddressedCorrectionOwnerIdentity Owner,
    ILogicalUnit<string, TOutput> SelectedUnit,
    TOutput ShownAnswer);

// Граница запоминания выдачи. Правило 2 проекта: точка выдачи зависит от интерфейса, а не от
// конкретного журнала, поэтому её можно проверить и с журналом, и без него.
public interface IAnsweredDeliveryLedger<TOutput>
{
    // Запомнить одну состоявшуюся выдачу ответа пользователю.
    OperationResult<AnsweredDelivery<TOutput>> RecordDelivery(
        string runId,
        string deliveryId,
        ILogicalUnit<string, TOutput> selectedUnit,
        string answeredUnitId,
        string answeredUnitRole,
        string answeredDomainTag,
        TOutput shownAnswer);
}

// Журнал выдач, который доводит подтверждённое исправление до выбранного получателя.
public sealed class KnnAnsweredDeliveryLedger<TOutput> : IAnsweredDeliveryLedger<TOutput>
{
    // Коды отказов журнала. Каждый называет ровно одну причину, по которой выдача не запомнена
    // или исправление не доставлено.
    public const string RunRequiredErrorCode = "ANSWERED_DELIVERY_RUN_REQUIRED";
    public const string DeliveryRequiredErrorCode = "ANSWERED_DELIVERY_ID_REQUIRED";
    public const string SelectedUnitRequiredErrorCode = "ANSWERED_DELIVERY_SELECTED_UNIT_REQUIRED";
    public const string AnsweredIdentityRequiredErrorCode = "ANSWERED_DELIVERY_IDENTITY_REQUIRED";
    public const string ShownAnswerRequiredErrorCode = "ANSWERED_DELIVERY_SHOWN_ANSWER_REQUIRED";
    public const string UnknownAnsweringOwnerErrorCode = "ANSWERED_DELIVERY_UNKNOWN_ANSWERING_OWNER";
    public const string ForeignAnsweringObjectErrorCode = "ANSWERED_DELIVERY_FOREIGN_ANSWERING_OBJECT";
    public const string AnsweredDomainMismatchErrorCode = "ANSWERED_DELIVERY_DOMAIN_MISMATCH";
    public const string AnsweredRoleMismatchErrorCode = "ANSWERED_DELIVERY_ROLE_MISMATCH";
    public const string FeedbackRequiredErrorCode = "ANSWERED_DELIVERY_FEEDBACK_REQUIRED";
    public const string CorrectedMaterialRequiredErrorCode = "ANSWERED_DELIVERY_CORRECTED_MATERIAL_REQUIRED";
    public const string UnknownDeliveryErrorCode = "ANSWERED_DELIVERY_UNKNOWN_DELIVERY";
    public const string CancelledErrorCode = "ANSWERED_DELIVERY_CANCELLED";

    // Справочник владельцев: откуда журнал берёт ЖИВОЕ опознание ответившей единицы.
    private readonly IAddressedCorrectionOwnerDirectory<TOutput> _directory;

    // Существующий координатор адресного исправления. Журнал его НЕ подменяет и НЕ переписывает.
    private readonly KnnAddressedCorrectionCoordinator<TOutput> _coordinator;

    // Запомненные выдачи по составному ключу «запуск и опознание выдачи».
    private readonly Dictionary<string, AnsweredDelivery<TOutput>> _deliveries = new(StringComparer.Ordinal);

    // Замок на запись и чтение журнала: выдача и исправление приходят разными ходами.
    private readonly object _gate = new();

    // Журнал собирается из справочника владельцев и существующего координатора: своего
    // обученного состояния у него нет и быть не должно.
    public KnnAnsweredDeliveryLedger(
        IAddressedCorrectionOwnerDirectory<TOutput> directory,
        KnnAddressedCorrectionCoordinator<TOutput> coordinator)
    {
        // Справочник обязателен: без него нельзя узнать живую версию ответившего владельца.
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        // Координатор обязателен: именно он, а не журнал, доводит урок до памяти владельца.
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
    }

    // Запомнить выдачу: какая единица была выбрана и какой ответ фактически показан.
    public OperationResult<AnsweredDelivery<TOutput>> RecordDelivery(
        string runId,
        string deliveryId,
        ILogicalUnit<string, TOutput> selectedUnit,
        string answeredUnitId,
        string answeredUnitRole,
        string answeredDomainTag,
        TOutput shownAnswer)
    {
        // Запуск обязателен: он первая половина адреса будущего исправления.
        if (string.IsNullOrWhiteSpace(runId))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Опознание запуска обязательно.", RunRequiredErrorCode);

        // Опознание выдачи обязательно: по нему исправление находит именно эту выдачу.
        if (string.IsNullOrWhiteSpace(deliveryId))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Опознание выдачи обязательно.", DeliveryRequiredErrorCode);

        // Объект выбранной единицы обязателен: именно он отличает двух владельцев с одним именем роли.
        if (selectedUnit is null)
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Объект выбранной единицы обязателен.", SelectedUnitRequiredErrorCode);

        // Опознание ответившей единицы, её роль и домен обязательны: это адрес урока.
        if (string.IsNullOrWhiteSpace(answeredUnitId) ||
            string.IsNullOrWhiteSpace(answeredUnitRole) ||
            string.IsNullOrWhiteSpace(answeredDomainTag))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Опознание ответившей единицы обязательно.", AnsweredIdentityRequiredErrorCode);

        // Показанный ответ обязателен: запоминать выдачу без её ответа бессмысленно.
        if (shownAnswer is null)
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Показанный ответ обязателен.", ShownAnswerRequiredErrorCode);

        // Живое опознание ответившего владельца читается ОБЫЧНЫМ путём чтения реестра единиц.
        var resolved = _directory.ResolveLatestOwnerById(answeredUnitId);

        // Неизвестный владелец — отказ сразу: запоминать адрес, которого нет, нельзя.
        if (!resolved.IsSuccess || resolved.Value is null)
            return OperationResult<AnsweredDelivery<TOutput>>.Fail(
                $"Ответившего владельца '{answeredUnitId}' в справочнике нет: {resolved.ErrorMessage}",
                UnknownAnsweringOwnerErrorCode);

        // Описание владельца и его исполняемый объект берутся из ОДНОЙ записи справочника.
        var live = resolved.Value.Descriptor;

        // Ответивший объект обязан быть ТЕМ ЖЕ объектом, который лежит в реестре под этим именем.
        // Это и есть доказательство адресности на стороне выдачи: сравнивается не запись о
        // владельце, а фактический объект, который дал ответ.
        if (!ReferenceEquals(resolved.Value.Unit, selectedUnit))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail(
                $"Ответ выдал объект, который не совпадает с зарегистрированным владельцем '{answeredUnitId}'.",
                ForeignAnsweringObjectErrorCode);

        // Домен ответа обязан совпадать с доменом живого владельца: домен — часть связки.
        if (!string.Equals(live.DomainTag, answeredDomainTag, StringComparison.Ordinal))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail(
                $"Владелец '{answeredUnitId}' живёт в домене '{live.DomainTag}', а ответ помечен доменом '{answeredDomainTag}'.",
                AnsweredDomainMismatchErrorCode);

        // Роль — это место, которое владелец занимает. Чужая роль означает другого адресата.
        if (!string.Equals(live.UnitRole, answeredUnitRole, StringComparison.Ordinal))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail(
                $"Владелец '{answeredUnitId}' занимает роль '{live.UnitRole}', а ответ помечен ролью '{answeredUnitRole}'.",
                AnsweredRoleMismatchErrorCode);

        // Опознание выбранного владельца собирается из живой записи реестра.
        //
        // Профиль каталога правил ранжирования берётся равным домену владельца: именно это
        // равенство требует координатор, и оно же является продуктовым инвариантом обученной
        // единицы, отвергающей каталог чужого профиля.
        var owner = new AddressedCorrectionOwnerIdentity(
            live.DomainTag,
            live.DomainTag,
            live.UnitId,
            live.UnitRole,
            live.Version,
            live.RegistrationOrder,
            ComposeTrainedStateIdentity(live));

        // Запись о выдаче хранит и опознание владельца, и его объект, и показанный ответ.
        var delivery = new AnsweredDelivery<TOutput>(runId, deliveryId, owner, selectedUnit, shownAnswer);

        // Запись кладётся под составной ключ однозначным кодированием, а не склейкой строк.
        lock (_gate)
        {
            // Повторная выдача с тем же ключом заменяет прежнюю запись: адресом остаётся последняя.
            _deliveries[ComposeDeliveryRecordKey(runId, deliveryId)] = delivery;
        }

        // Наружу уходит запомненная выдача: вызывающая сторона видит выбранного получателя.
        return OperationResult<AnsweredDelivery<TOutput>>.Ok(delivery);
    }

    // Найти запомненную выдачу. Отсутствие записи — именованный отказ, а не пустая ссылка.
    public OperationResult<AnsweredDelivery<TOutput>> FindDelivery(string runId, string deliveryId)
    {
        // Запуск обязателен: без него ключ записи не складывается.
        if (string.IsNullOrWhiteSpace(runId))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Опознание запуска обязательно.", RunRequiredErrorCode);

        // Опознание выдачи обязательно по той же причине.
        if (string.IsNullOrWhiteSpace(deliveryId))
            return OperationResult<AnsweredDelivery<TOutput>>.Fail("Опознание выдачи обязательно.", DeliveryRequiredErrorCode);

        // Чтение журнала идёт под тем же замком, что и запись.
        lock (_gate)
        {
            // Найденная запись возвращается как есть.
            if (_deliveries.TryGetValue(ComposeDeliveryRecordKey(runId, deliveryId), out var delivery))
                return OperationResult<AnsweredDelivery<TOutput>>.Ok(delivery);
        }

        // Ненайденная выдача называется прямо: исправлять нечего, и угадывать адресата нельзя.
        return OperationResult<AnsweredDelivery<TOutput>>.Fail(
            $"Выдача '{deliveryId}' запуска '{runId}' не запомнена, поэтому адресата исправления взять неоткуда.",
            UnknownDeliveryErrorCode);
    }

    // Передать подтверждённое исправление по ЭТОЙ выдаче в существующий координатор.
    public async Task<OperationResult<AddressedCorrectionReceipt>> ApplyConfirmedCorrectionAsync(
        string runId,
        string deliveryId,
        string feedbackId,
        string correctedExampleId,
        TOutput correctedAnswer,
        CancellationToken cancellationToken = default)
    {
        // Отмена возвращается общим конвертом до любой работы.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<AddressedCorrectionReceipt>.Fail("Отменено.", CancelledErrorCode);

        // Опознание обращения пользователя обязательно: оно вторая половина ключа доставки.
        if (string.IsNullOrWhiteSpace(feedbackId))
            return OperationResult<AddressedCorrectionReceipt>.Fail("Опознание обращения обязательно.", FeedbackRequiredErrorCode);

        // Материал исправления обязателен: пустым уроком учить нечему.
        if (string.IsNullOrWhiteSpace(correctedExampleId) || correctedAnswer is null)
            return OperationResult<AddressedCorrectionReceipt>.Fail("Материал исправления обязателен.", CorrectedMaterialRequiredErrorCode);

        // Выдача, к которой относится исправление, читается из журнала.
        var found = FindDelivery(runId, deliveryId);

        // Незапомненная выдача останавливает доставку: обе памяти остаются прежними.
        if (!found.IsSuccess || found.Value is null)
            return found.MapError<AddressedCorrectionReceipt>();

        // Выбранный получатель берётся из записи о выдаче, а не из текста запроса пользователя.
        var owner = found.Value.Owner;

        // Опознание фактического ответа составляется из той же записи: это и есть адрес урока.
        var evidence = new AddressedCorrectionResponseEvidence(
            found.Value.RunId,
            owner.UnitId,
            owner.DomainTag,
            owner.Version,
            owner.RegistrationOrder,
            owner.TrainedStateIdentity);

        // Запрос адресной доставки собирается ровно из запомненной выдачи и пришедшего материала.
        var request = new AddressedCorrection<TOutput>(
            found.Value.RunId,
            feedbackId,
            owner,
            evidence,
            correctedExampleId,
            correctedAnswer);

        // Дальше работает УЖЕ СУЩЕСТВУЮЩИЙ координатор: обучение и публикация принадлежат ему.
        return await _coordinator.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
    }

    // Составной ключ записи о выдаче. Поля кодируются однозначно: длина поля объявляется до
    // его байтов, поэтому разделитель внутри значения не может сдвинуть границы полей.
    private static string ComposeDeliveryRecordKey(string runId, string deliveryId)
    {
        // Назначение потока называет именно запись о выдаче ответа.
        var writer = new UnambiguousFieldEncoding.Writer("knn-answered-delivery-record-key");
        // Запуск и опознание выдачи вместе образуют ключ записи.
        writer.WriteField(runId)
              .WriteField(deliveryId);
        // Отпечаток полей служит ключом словаря.
        return writer.Digest();
    }

    // Опознание обученного состояния для записи о выдаче.
    //
    // Оно СОСТАВЛЯЕТСЯ из живого опознания владельца в реестре и дальше только сверяется
    // координатором. Побайтовую сохранность обученного состояния оно НЕ доказывает и
    // контрольной точкой обучаемого блока НЕ является.
    private static string ComposeTrainedStateIdentity(LogicalUnitDescriptor live)
    {
        // Назначение потока называет именно опознание состояния ответившего владельца.
        var writer = new UnambiguousFieldEncoding.Writer("knn-answered-delivery-state-identity");
        // В опознание входят имя владельца, его роль, домен, версия и порядковый номер регистрации.
        writer.WriteField(live.UnitId)
              .WriteField(live.UnitRole)
              .WriteField(live.DomainTag)
              .WriteField(live.Version)
              .WriteField(live.RegistrationOrder.ToString(System.Globalization.CultureInfo.InvariantCulture));
        // Отпечаток полей и есть непрозрачная строка опознания состояния.
        return writer.Digest();
    }
}
