// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Сохранение проигравшего обученного кандидата.
//
// ЧТО ЭТО ЧИНИТ, ПО КОДУ.
// Назначение: план сто тридцать девять, раздел R117-03; аудит сто тридцать восемь,
// находка F01 (потеря уже оплаченной обученной работы при конфликте двух писателей).
//
// Дефект. В TranslationMachine.cs отказ публикации по несовпадению основы говорил
// дословно «обученный кандидат сохранён» (строка 147), а соседний комментарий на
// строках 375-377 честно признавал обратное: «обученный кандидат остаётся в закрытой
// копии и просто выходит из области видимости». Закрытая копия обученного состояния
// (stagedRuntime, строки 281-294) передавалась только в условную публикацию на строках
// 363-373 и при отказе исчезала вместе с кадром вызова. Отказ на строках 378-380
// возвращал код «версия состояния изменилась» без указателя, дескриптора и пути,
// поэтому вызывающий не мог ни прочитать кандидата, ни проверить обещание. Оплаченное
// обучение терялось, а сообщение называло его сохранённым.
//
// ПОЧЕМУ ОБЫЧНЫЙ ПУТЬ СОХРАНЕНИЯ ЗДЕСЬ НЕПРИГОДЕН. Обычный SaveCheckpointAsync зовёт
// ExportCheckpoint, а тот читает ТЕКУЩУЮ опубликованную версию. После проигрыша
// сравнения-с-заменой текущей версией является ПОБЕДИТЕЛЬ, поэтому обычный путь записал
// бы победителя под именем кандидата — ложь о данных. Поэтому запись кандидата строится
// из его собственного держателя версии и его собственной закрытой копии исполнения.
//
// ЧТО ЗДЕСЬ СДЕЛАНО.
// Первое: запись кандидата содержит ФАКТИЧЕСКОЕ состояние — собранную контрольную точку,
// а не ожидаемый журнал уроков.
// Второе: «сохранено» говорится только после подтверждения записи. Подтверждение — это
// повторное чтение записанных байтов с диска и сверка их свёртки, а не успешный возврат
// вызова записи.
// Третье: при ошибке хранения возвращается отдельный названный отказ
// CANDIDATE_PERSISTENCE_FAILED, и слово «сохранено» в этом случае не произносится.
// Четвёртое: указатель на запись проверяем — это путь, переживающий процесс, а не
// локальная переменная.
// Пятое: свёртка записанных байтов подтверждает сохранённые байты, а опознание состояния
// сравнивает канонический состав состояния БЕЗ времени выгрузки, поэтому новое время
// выгрузки не создаёт ложного расхождения при сверке восстановленного состояния.
// Шестое: повторное чтение записи обучения не запускает: здесь нет ни одного обращения к
// обучателям, к модели и к сети.

namespace TranslationDomain;

using System.Security.Cryptography; // Свёртка записанных байтов для подтверждения записи.
using System.Text;                  // Кодировка UTF-8 без изменения байтов текста.
using System.Text.Json;             // Единое правило записи состояния в текст.
using AgentInfrastructure;          // Конверт результата операции этой сборки.

/// <summary>
/// Расход, уже оплаченный проигравшим кандидатом. Нужен, чтобы по записи было видно, что
/// именно потеряли бы при молчаливом отбрасывании.
/// </summary>
public sealed record TranslationLostCandidateCost(
    int RowsTrained,   // Сколько учебных строк кандидат уже применил к своей копии.
    long CpuMs,        // Процессорное время, уже потраченное на эти строки.
    long ElapsedMs,    // Прошедшее время работы кандидата.
    long PeakRamMb);   // Наибольшая занятая память во время работы кандидата.

/// <summary>
/// Запись проигравшего кандидата: кто его основа, кто победил, что именно он выучил и
/// почему публикация отказана. Содержит фактическое состояние, а не журнал уроков.
/// </summary>
public sealed record TranslationLostCandidateArtifact(
    int SchemaVersion,                                     // Версия формата самой записи.
    string MachineId,                                      // Опознание машины, которой принадлежит кандидат.
    string DomainTag,                                      // Предметная область машины.
    // Тип ДЛИННОЕ целое: номер приходит из счётчика публикаций, объявленного длинным. Обычное
    // целое здесь не собиралось бы, а приведение молча усекало бы номер после предела обычного
    // целого — и две разные основы получили бы один номер. Для уже записанных файлов это
    // безопасно: число в тексте записывается и читается как обычное число, и прежние небольшие
    // значения разбираются в длинное без потери.
    long BaseStateVersionNumber,
    string BaseSemanticVersion,                            // Смысловой номер версии той же основы.
    // Номер версии, которая победила в сравнении-с-заменой, ИЛИ пустое значение, когда
    // победителя не было вовсе.
    //
    // Что было неправильно. Это поле заполнялось номером текущей опубликованной версии на
    // любом пути отказа. На пути ОТМЕНЫ конкурента нет, текущей остаётся сама основа, и
    // запись читалась так, будто основа «победила» кандидата. Это ложь о том, что произошло.
    //
    // Что стало. Пустое значение означает прямо: победителя не было, публикация не
    // состоялась по отмене, а не проиграла чужой работе. Читатель записи различает два
    // совершенно разных события, не додумывая.
    // Тип ДЛИННОЕ целое с возможным пустым значением — по той же причине, что и у основы.
    long? WinningStateVersionNumber,
    string CandidateBatchId,                               // Опознание партии обучения кандидата.
    string CandidateSemanticVersion,                       // Смысловой номер версии самого кандидата.
    string CandidateStateIdentity,                         // Опознание состояния кандидата без времени выгрузки.
    TranslationMachineCheckpoint CandidateCheckpoint,       // ФАКТИЧЕСКОЕ состояние кандидата целиком.
    IReadOnlyList<string> LineageExampleIdentities,         // Происхождение: опознания учебных строк кандидата.
    TranslationLostCandidateCost Cost,                      // Уже оплаченный расход кандидата.
    string RefusalErrorCode,                                // Код отказа публикации.
    string RefusalReason,                                   // Человеческая причина отказа публикации.
    DateTimeOffset BuiltAtUtc);                             // Время сборки записи.

/// <summary>
/// Проверяемый указатель на сохранённую запись кандидата. Возвращается только после
/// подтверждённой записи, поэтому его наличие и есть доказательство сохранности.
/// </summary>
public sealed record TranslationLostCandidateArtifactRef(
    string Locator,                   // Путь к записи: он переживает процесс, в отличие от переменной.
    string WrittenBytesHash,          // Свёртка фактически записанных и перечитанных байтов.
    string CandidateStateIdentity,    // Опознание состояния кандидата без времени выгрузки.
    long BaseStateVersionNumber,      // Основа кандидата; длинное целое, как и сам счётчик публикаций.
    long? WinningStateVersionNumber); // Победившая версия, либо пустое значение, когда победителя не было.

/// <summary>
/// Хранилище проигравших кандидатов. Отдельный договор нужен, чтобы машина не знала о
/// файловой системе и чтобы отказ хранения можно было проверить без настоящего диска.
/// </summary>
public interface ITranslationLostCandidateStore
{
    /// <summary>
    /// Сохраняет запись кандидата и возвращает проверяемый указатель. Успех возвращается
    /// ТОЛЬКО после подтверждения записи перечитыванием.
    /// </summary>
    OperationResult<TranslationLostCandidateArtifactRef> Save(TranslationLostCandidateArtifact artifact);
}

/// <summary>
/// Единое правило записи состояния в текст и на диск. Одно место отвечает и за
/// преобразование в текст, и за неделимую замену файла, поэтому у правила нет слабого
/// двойника в другом методе.
/// </summary>
public static class TranslationAtomicJsonWriter
{
    /// <summary>Преобразует любую запись состояния в текст одним общим правилом.</summary>
    public static string Serialize(object payload)
        => JsonSerializer.Serialize(payload, TranslationCheckpointJsonOptions.Standard); // Одно правило для всех записей.

    /// <summary>
    /// Опознание состояния контрольной точки БЕЗ времени выгрузки: сверка
    /// восстановленного состояния не должна падать из-за новых стенных часов.
    /// </summary>
    public static string CanonicalStateIdentity(TranslationMachineCheckpoint checkpoint)
    {
        var withoutExportTime = checkpoint with { ExportedAtUtc = DateTimeOffset.UnixEpoch }; // Время выгрузки обнуляется.
        return HashOfText(Serialize(withoutExportTime));                                      // Свёртка канонического состава.
    }

    // Utf8BytesOf — ЕДИНСТВЕННОЕ определение байтов записи для этого типа.
    //
    // Что было сломано ровно. Свёртка считалась от `Encoding.UTF8.GetBytes(текст)`, то есть БЕЗ
    // метки порядка байтов, а запись шла через `File.WriteAllText` с тем же `Encoding.UTF8`,
    // который метку в начало файла ВЫДАЁТ. Поэтому перечитанные с диска байты всегда отличались
    // от посчитанных ровно на три первых байта, подтверждение записи не могло совпасть НИКОГДА,
    // и правдивое «сохранено» не возвращалось даже при полностью успешной записи.
    //
    // Почему так. Кодирование объявлено явно и без метки: один и тот же массив байтов идёт и в
    // свёртку, и в запись, и в сравнение после перечитывания. Текстовые читатели от этого не
    // страдают: они метку и так снимали сами, а разбор JSON с меткой в начале вообще невозможен.
    private static readonly UTF8Encoding Utf8WithoutByteOrderMark = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Байты записи: одно определение для свёртки, записи и перечитывания.</summary>
    public static byte[] Utf8BytesOf(string? text)
        => Utf8WithoutByteOrderMark.GetBytes(text ?? string.Empty); // Пустой текст даёт пустой массив.

    /// <summary>Свёртка текста: шестнадцатеричная запись без разделителей.</summary>
    public static string HashOfText(string text)
    {
        var bytes = Utf8BytesOf(text);                             // Те же байты, что лягут на диск.
        return Convert.ToHexString(SHA256.HashData(bytes));        // Шестнадцатеричная запись свёртки.
    }

    /// <summary>Свёртка уже прочитанных с диска байтов: подтверждение именно записанного.</summary>
    public static string HashOfBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes ?? Array.Empty<byte>())); // Пустые байты не роняют свёртку.

    /// <summary>
    /// Неделимая запись текста по пути: сначала во временный файл, затем перемещение
    /// одной операцией, поэтому частично записанного файла не остаётся даже при обрыве.
    /// </summary>
    public static OperationResult<string> WriteAtomically(string? path, string? json)
    {
        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Fail("Путь записи обязателен.", "WRITE_PATH_REQUIRED"); // Без пути писать некуда.

        try
        {
            var directory = Path.GetDirectoryName(path!);                    // Каталог записи.
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory!);                       // Отсутствующий каталог создаётся.

            var temporaryPath = path! + ".tmp";                              // Временное имя до неделимой замены.
            // Пишутся РОВНО те байты, от которых считается свёртка: без метки порядка байтов,
            // поэтому перечитанное с диска совпадает с посчитанным, а не расходится на три байта.
            File.WriteAllBytes(temporaryPath, Utf8BytesOf(json));            // Запись во временный файл.
            File.Move(temporaryPath, path!, overwrite: true);                // Неделимая замена целевого файла.

            return OperationResult<string>.Ok(Path.GetFullPath(path!));      // Полный путь как проверяемый указатель.
        }
        catch (Exception error)
        {
            return OperationResult<string>.Fail(error.Message, "WRITE_FAILED"); // Ошибка хранения названа, а не скрыта.
        }
    }

    /// <summary>
    /// Та же неделимая запись для вызывающих, которые работают ожиданием. Правило имени
    /// временного файла и порядок операций здесь ровно те же.
    /// </summary>
    public static async Task<OperationResult<string>> WriteAtomicallyAsync(
        string? path,
        string? json,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<string>.Fail("Запись отменена.", "CANCELLED"); // Отмена названа отдельно.

        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Fail("Путь записи обязателен.", "WRITE_PATH_REQUIRED"); // Без пути писать некуда.

        try
        {
            var directory = Path.GetDirectoryName(path!);                     // Каталог записи.
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory!);                        // Отсутствующий каталог создаётся.

            var temporaryPath = path! + ".tmp";                               // Временное имя до неделимой замены.
            // Второй писатель обязан писать те же байты, что и первый: иначе подтверждение
            // записи зависело бы от того, каким из двух путей файл оказался на диске.
            await File.WriteAllBytesAsync(temporaryPath, Utf8BytesOf(json), ct); // Запись во временный файл.
            File.Move(temporaryPath, path!, overwrite: true);                 // Неделимая замена целевого файла.

            return OperationResult<string>.Ok(Path.GetFullPath(path!));       // Полный путь как проверяемый указатель.
        }
        catch (Exception error)
        {
            return OperationResult<string>.Fail(error.Message, "WRITE_FAILED"); // Ошибка хранения названа, а не скрыта.
        }
    }
}

/// <summary>
/// Хранилище проигравших кандидатов в файлах. Имя файла выводится из опознания состояния,
/// поэтому один и тот же кандидат не размножается копиями под разными именами.
/// </summary>
public sealed class TranslationFileLostCandidateStore : ITranslationLostCandidateStore
{
    /// <summary>Каталог, в котором живут записи проигравших кандидатов.</summary>
    private readonly string _directory;

    /// <summary>Каталог задаётся снаружи: сама машина о файловой системе не знает.</summary>
    public TranslationFileLostCandidateStore(string directory)
        => _directory = directory ?? string.Empty; // Пустой каталог приведёт к названному отказу, а не к исключению.

    /// <summary>
    /// Пишет запись кандидата и ПОДТВЕРЖДАЕТ её: файл перечитывается с диска, свёртка
    /// перечитанных байтов сверяется со свёрткой записанного текста. Только после этого
    /// возвращается указатель, то есть только после этого слово «сохранено» правдиво.
    /// </summary>
    public OperationResult<TranslationLostCandidateArtifactRef> Save(TranslationLostCandidateArtifact artifact)
    {
        if (artifact is null)
            return OperationResult<TranslationLostCandidateArtifactRef>.Fail(
                "Запись кандидата обязательна.", "CANDIDATE_ARTIFACT_REQUIRED"); // Нечего сохранять.

        if (string.IsNullOrWhiteSpace(_directory))
            return OperationResult<TranslationLostCandidateArtifactRef>.Fail(
                "Каталог записей проигравших кандидатов не назначен.", "CANDIDATE_STORE_DIRECTORY_REQUIRED"); // Путь не назначен.

        var json = TranslationAtomicJsonWriter.Serialize(artifact);          // Текст записи одним общим правилом.
        var expectedHash = TranslationAtomicJsonWriter.HashOfText(json);     // Свёртка того, что собираемся записать.

        // Имя файла выводится из опознания состояния: одна и та же работа — один файл.
        var shortIdentity = artifact.CandidateStateIdentity.Length >= 16
            ? artifact.CandidateStateIdentity.Substring(0, 16)
            : artifact.CandidateStateIdentity;                               // Короткая часть опознания для имени.
        var path = Path.Combine(_directory, "lost-candidate-" + shortIdentity + ".json"); // Полное имя записи.

        var written = TranslationAtomicJsonWriter.WriteAtomically(path, json); // Неделимая запись на диск.
        if (!written.IsSuccess)
            return OperationResult<TranslationLostCandidateArtifactRef>.Fail(
                "Запись проигравшего кандидата не сохранена: " + written.ErrorMessage,
                TranslationMachine.CandidatePersistenceFailedErrorCode);      // Отказ хранения назван своим кодом.

        byte[] readBack;                                                      // Фактически лежащие на диске байты.
        try
        {
            readBack = File.ReadAllBytes(written.Value!);                      // Перечитывание записанного файла.
        }
        catch (Exception error)
        {
            return OperationResult<TranslationLostCandidateArtifactRef>.Fail(
                "Запись проигравшего кандидата не перечитывается: " + error.Message,
                TranslationMachine.CandidatePersistenceFailedErrorCode);       // Неподтверждённая запись не считается сохранённой.
        }

        var actualHash = TranslationAtomicJsonWriter.HashOfBytes(readBack);    // Свёртка перечитанных байтов.
        if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            return OperationResult<TranslationLostCandidateArtifactRef>.Fail(
                "Перечитанные байты записи кандидата не совпали с записанными.",
                TranslationMachine.CandidatePersistenceFailedErrorCode);       // Расхождение байтов — отказ, а не успех.

        return OperationResult<TranslationLostCandidateArtifactRef>.Ok(
            new TranslationLostCandidateArtifactRef(
                written.Value!,                        // Проверяемый указатель: путь, переживающий процесс.
                actualHash,                            // Свёртка подтверждённых байтов.
                artifact.CandidateStateIdentity,       // Опознание состояния без времени выгрузки.
                artifact.BaseStateVersionNumber,       // Основа кандидата.
                artifact.WinningStateVersionNumber));  // Победившая версия на момент отказа.
    }
}
