// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace DPO.Models;

// Вид автора стороны хранится типом, чтобы мосты не передавали в тренер
// свободный текст и не подменяли юридически важный источник синонимом.
public enum DpoPairSideAuthorKind
{
    // Сторону написал пользователь; это отдельный законный источник от модели.
    User = 1,

    // Локальная модель отделена от внешней платной, потому что ее вывод можно
    // использовать в других правилах допуска без лицензионного риска платного API.
    LocalModel = 2,

    // Открытый учитель означает источник, разрешенный как обучающий сигнал.
    OpenTeacher = 3,

    // Детерминированный генератор фиксирует программно созданную сторону пары,
    // где воспроизводимость важнее личности автора-человека.
    DeterministicGenerator = 4,

    // Платная внешняя модель помечается отдельным значением: дверь тренера
    // отказывает по нему на любой стороне пары до изменения весов.
    PaidExternalModel = 5
}

// Допуск пары неизменяем после выдачи: fingerprint, авторы и issuer должны
// оставаться тем же паспортом, который пришел вместе с конкретной парой.
public sealed record DpoTrainerPairAdmission
{
    // Код нужен двери тренера для стабильного отказа, если batch пришел
    // из legacy-пути без паспорта допуска.
    public const string MissingErrorCode = "DPO_PAIR_ADMISSION_MISSING";

    // Код нужен двери тренера для стабильного отказа по платной стороне;
    // проверка смотрит на обе стороны, а не только на chosen.
    public const string PaidExternalModelSideErrorCode =
        "DPO_PAIR_ADMISSION_PAID_MODEL_SIDE";

    private const string FingerprintDomainSeparator =
        "DPO_TRAINER_PAIR_ADMISSION_CONTENT_SHA256_V1";

    internal DpoTrainerPairAdmission(
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor,
        string question,
        string chosen,
        string rejected,
        string issuerName)
    {
        // Внутренний дружественный путь обязан падать так же строго, как
        // публичная таможня: числовое приведение не должно стать новым автором.
        EnsureKnownAuthor(chosenAuthor, nameof(chosenAuthor));
        EnsureKnownAuthor(rejectedAuthor, nameof(rejectedAuthor));

        // Тексты нужны самому конструктору для вычисления отпечатка; дружественная
        // сборка не может обойти хеширование, передав готовую строку без пары.
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(chosen);
        ArgumentNullException.ThrowIfNull(rejected);

        // Issuer хранится как часть происхождения; пустое имя сделало бы паспорт
        // непригодным для аудита, даже если вызов пришел не через фабрику.
        ArgumentNullException.ThrowIfNull(issuerName);
        if (string.IsNullOrWhiteSpace(issuerName))
        {
            throw new ArgumentException(
                "DPO pair admission issuer name is required.",
                nameof(issuerName));
        }

        ChosenAuthor = chosenAuthor;
        RejectedAuthor = rejectedAuthor;
        HasPaidExternalModelSide = ContainsPaidExternalModelSide(
            chosenAuthor,
            rejectedAuthor);
        ContentFingerprintSha256 = ComputeContentFingerprintSha256(
            question,
            chosen,
            rejected);
        IssuerName = issuerName;
    }

    // Автор предпочтительной стороны нужен отдельно от пары, потому что сама
    // DpoPair хранит только три строки и не знает происхождение сторон.
    public DpoPairSideAuthorKind ChosenAuthor { get; }

    // Автор отклоненной стороны хранится отдельно: запрет платной модели должен
    // в будущем смотреть на обе стороны, а не только на chosen.
    public DpoPairSideAuthorKind RejectedAuthor { get; }

    // Флаг выводится конструктором из двух авторов; вызывающий код не может передать
    // отдельный bool и случайно рассинхронизировать паспорт.
    public bool HasPaidExternalModelSide { get; }

    // Отпечаток привязывает admission к точным UTF-8 байтам Prompt/Chosen/Rejected
    // без обрезки, нормализации или неоднозначного склеивания строк.
    public string ContentFingerprintSha256 { get; }

    // Issuer отвечает на вопрос "кто выдал допуск", но не заявляет, что тренер
    // уже обязан принять пару к обучению.
    public string IssuerName { get; }

    private static void EnsureKnownAuthor(
        DpoPairSideAuthorKind author,
        string parameterName)
    {
        if (!Enum.IsDefined(typeof(DpoPairSideAuthorKind), author))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                author,
                "Unknown DPO pair side author kind.");
        }
    }

    private static bool ContainsPaidExternalModelSide(
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
        => chosenAuthor is DpoPairSideAuthorKind.PaidExternalModel ||
           rejectedAuthor is DpoPairSideAuthorKind.PaidExternalModel;

    private static string ComputeContentFingerprintSha256(
        string question,
        string chosen,
        string rejected)
    {
        using var stream = new MemoryStream();
        var domainBytes = Encoding.ASCII.GetBytes(FingerprintDomainSeparator);

        // Версия схемы пишется в сам поток хеша: будущий формат отпечатка не
        // столкнется байт-в-байт со старым при тех же трех текстах.
        WriteLittleEndianInt32(stream, domainBytes.Length);
        stream.Write(domainBytes);

        // Каждое поле получает имя и длину перед значением, чтобы перестановка
        // ролей или склейка вроде "ab"+"c" против "a"+"bc" меняла fingerprint.
        WriteFramedUtf8Field(stream, "prompt", question);
        WriteFramedUtf8Field(stream, "chosen", chosen);
        WriteFramedUtf8Field(stream, "rejected", rejected);

        return Convert
            .ToHexString(SHA256.HashData(stream.ToArray()))
            .ToLowerInvariant();
    }

    private static void WriteFramedUtf8Field(
        Stream stream,
        string fieldName,
        string value)
    {
        var fieldNameBytes = Encoding.UTF8.GetBytes(fieldName);
        var valueBytes = Encoding.UTF8.GetBytes(value);

        // Имя поля тоже frame-ится, чтобы будущий формат не зависел от
        // неявной договоренности о позиции байтов в потоке.
        WriteLittleEndianInt32(stream, fieldNameBytes.Length);
        stream.Write(fieldNameBytes);

        // Длина именно UTF-8 байтов, не символов .NET: это делает fingerprint
        // одинаковым между платформами и чувствительным к raw содержимому.
        WriteLittleEndianInt64(stream, valueBytes.LongLength);
        stream.Write(valueBytes);
    }

    private static void WriteLittleEndianInt32(Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteLittleEndianInt64(Stream stream, long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        stream.Write(buffer);
    }
}

// AdmittedDpoPair переносит бедную трехстрочную пару вместе с ее паспортом:
// потребитель получает обе части одним типом и не теряет соответствие на мостах.
public sealed record AdmittedDpoPair
{
    internal AdmittedDpoPair(
        DpoPair pair,
        DpoTrainerPairAdmission admission)
    {
        // Пара обязательна: admission без исходных строк нельзя проверить
        // глазами и нельзя безопасно превратить в старый DpoBatch.
        ArgumentNullException.ThrowIfNull(pair);

        // Admission обязателен: этот тип означает уже выданный пропуск, а не
        // nullable-контейнер для старого трехполевого пути.
        ArgumentNullException.ThrowIfNull(admission);

        Pair = pair;
        Admission = admission;
    }

    // Исходные Prompt/Chosen/Rejected остаются в DpoPair без изменения байтового смысла;
    // это сохраняет совместимость с существующим бедным контрактом.
    public DpoPair Pair { get; }

    // Паспорт несет авторство сторон, отпечаток и issuer рядом с парой.
    public DpoTrainerPairAdmission Admission { get; }
}

// Таможня живет в DPO-сборке, чтобы верхние слои могли выдать допуск без
// циклической ссылки на богатую пару ContinuousLearning.
public static class DpoTrainerPairCustoms
{
    public static AdmittedDpoPair CreateAdmitted(
        string question,
        string chosen,
        string rejected,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor,
        string issuerName)
    {
        var admission = new DpoTrainerPairAdmission(
            chosenAuthor,
            rejectedAuthor,
            question,
            chosen,
            rejected,
            issuerName);

        var pair = new DpoPair
        {
            Prompt = question,
            Chosen = chosen,
            Rejected = rejected
        };

        return new AdmittedDpoPair(pair, admission);
    }
}
