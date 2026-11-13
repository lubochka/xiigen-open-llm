// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

// Читатель исправления утверждения, связанный со своим источником.
//
// Зачем это нужно, по-человечески. Аудит иногда поправляет ТОЛКОВАНИЕ старого сохранённого
// результата: сырое число остаётся прежним, а вывод по нему оказывается слабее, чем звучал.
// Такую поправку нельзя применить «куда-нибудь»: применённая к другой попытке, она превращается
// в ложь о том опыте, которого никто не проверял.
//
// Что уже было сделано до этого файла. Сами поправки существуют как сохранённый пакет аудита и
// уже несут опознание своего источника: хранилище, срез, путь, отпечаток точных байтов и
// указатель внутрь документа. Поэтому связь здесь не выдумывается, а требуется.
//
// Чего здесь нет намеренно. Здесь нет изменения исторических сырых данных, нет обучения, нет
// оценки качества и нет собственного мнения о том, какое толкование правильное: толкование
// приходит из поправки, а этот код отвечает только за то, чтобы оно попало ровно к своему
// источнику и чтобы сырое значение осталось видимым рядом.

// ClaimCorrectionSource — опознание источника поправки. Без него поправки не существует.
public sealed record ClaimCorrectionSource(
    // Repository — хранилище, которому принадлежит источник.
    string Repository,
    // SourceCommit — срез, на котором источник был прочитан.
    string SourceCommit,
    // SourcePath — путь к файлу источника внутри хранилища.
    string SourcePath,
    // SourceSha256 — отпечаток точных байтов источника. Читатель считает его заново сам.
    string SourceSha256,
    // JsonPointer — указатель на то самое место внутри документа, о котором говорит поправка.
    string JsonPointer);

// ClaimCorrection — одна поправка толкования вместе со своим источником.
//
// Почему это один носитель, а не два довода. Два довода — поправка и опознание источника — можно
// рассогласовать: передать опознание одного источника и толкование другого. Здесь опознание
// лежит ВНУТРИ поправки, создать поправку без него нельзя, и у читателя нет входа, который
// принимал бы толкование отдельно. Это отнятие возможности, а не проверка вдогонку.
public sealed class ClaimCorrection
{
    // Отказ: у поправки нет собственного опознания или оно неполное.
    public const string SourceIdentityRequiredErrorCode = "CLAIM_CORRECTION_SOURCE_IDENTITY_REQUIRED";

    // Отказ: у поправки нет опознания самой поправки или нет толкования.
    public const string CorrectionFieldsRequiredErrorCode = "CLAIM_CORRECTION_FIELDS_REQUIRED";

    private ClaimCorrection(
        string correctionId,
        ClaimCorrectionSource source,
        string originalClaim,
        string interpretationStatus,
        string authoritativeInterpretation,
        string reason)
    {
        // Поля заполняются один раз и дальше не меняются.
        CorrectionId = correctionId;
        Source = source;
        OriginalClaim = originalClaim;
        InterpretationStatus = interpretationStatus;
        AuthoritativeInterpretation = authoritativeInterpretation;
        Reason = reason;
    }

    // Опознание самой поправки в пакете аудита.
    public string CorrectionId { get; }

    // Опознание источника — неотделимая часть поправки.
    public ClaimCorrectionSource Source { get; }

    // Как утверждение звучало в исходном отчёте.
    public string OriginalClaim { get; }

    // Короткое состояние толкования, например «не доказано».
    public string InterpretationStatus { get; }

    // Полное авторитетное толкование, которым аудит заменяет прежнее звучание.
    public string AuthoritativeInterpretation { get; }

    // Человеческая причина поправки.
    public string Reason { get; }

    // Create — единственный способ получить поправку. Неполное опознание источника отвергается
    // здесь, поэтому поправка без источника до читателя не доходит вовсе.
    public static OperationResult<ClaimCorrection> Create(
        string? correctionId,
        ClaimCorrectionSource? source,
        string? originalClaim,
        string? interpretationStatus,
        string? authoritativeInterpretation,
        string? reason)
    {
        // Без опознания источника поправка не создаётся: применять её было бы некуда.
        if (source is null ||
            string.IsNullOrWhiteSpace(source.Repository) ||
            string.IsNullOrWhiteSpace(source.SourceCommit) ||
            string.IsNullOrWhiteSpace(source.SourcePath) ||
            string.IsNullOrWhiteSpace(source.SourceSha256) ||
            string.IsNullOrWhiteSpace(source.JsonPointer))
        {
            return OperationResult<ClaimCorrection>.Fail(
                "Поправка толкования обязана нести полное опознание своего источника: хранилище, " +
                "срез, путь, отпечаток байтов и указатель внутрь документа.",
                SourceIdentityRequiredErrorCode);
        }

        // Без опознания поправки и без толкования читать нечего.
        if (string.IsNullOrWhiteSpace(correctionId) ||
            string.IsNullOrWhiteSpace(interpretationStatus) ||
            string.IsNullOrWhiteSpace(authoritativeInterpretation))
        {
            return OperationResult<ClaimCorrection>.Fail(
                "Поправка толкования обязана нести своё опознание, состояние толкования и само " +
                "авторитетное толкование.",
                CorrectionFieldsRequiredErrorCode);
        }

        // Готовая поправка: опознание источника уже внутри неё.
        return OperationResult<ClaimCorrection>.Ok(new ClaimCorrection(
            correctionId,
            source,
            originalClaim ?? string.Empty,
            interpretationStatus,
            authoritativeInterpretation,
            reason ?? string.Empty));
    }
}

// ActualClaimSource — ФАКТИЧЕСКИЙ источник: то, что лежит на диске сейчас, вместе со своим
// хранилищем, срезом и путём. Отпечаток здесь не принимается: его считает читатель сам.
public sealed record ActualClaimSource(
    // Repository — хранилище фактического источника.
    string Repository,
    // SourceCommit — срез, на котором фактический источник прочитан.
    string SourceCommit,
    // SourcePath — путь, по которому он прочитан.
    string SourcePath,
    // Content — точное содержимое документа источника.
    string Content);

// ClaimCorrectionReading — прочитанная поправка: сырое значение и авторитетное толкование лежат
// РЯДОМ и раздельно, потому что подменять одно другим нельзя.
public sealed record ClaimCorrectionReading(
    // CorrectionId — какая именно поправка прочитана.
    string CorrectionId,
    // Source — к какому источнику она относится.
    ClaimCorrectionSource Source,
    // RawValue — сырое значение из самого источника, по указателю поправки. Остаётся видимым.
    string RawValue,
    // InterpretationStatus — состояние толкования.
    string InterpretationStatus,
    // AuthoritativeInterpretation — авторитетное толкование аудита.
    string AuthoritativeInterpretation);

// ClaimCorrectionReader — читатель поправки, который применяет её ТОЛЬКО к своему источнику.
public static class ClaimCorrectionReader
{
    // Отказ: фактический источник отсутствует или пуст.
    public const string ActualSourceRequiredErrorCode = "CLAIM_CORRECTION_ACTUAL_SOURCE_REQUIRED";

    // Отказ: поправка относится к другому хранилищу.
    public const string RepositoryMismatchErrorCode = "CLAIM_CORRECTION_REPOSITORY_MISMATCH";

    // Отказ: поправка относится к другому пути.
    public const string PathMismatchErrorCode = "CLAIM_CORRECTION_PATH_MISMATCH";

    // Отказ: поправка относится к другому срезу. Это явный конфликт версии, а не повод тихо
    // применить поправку к другой попытке.
    public const string CommitConflictErrorCode = "CLAIM_CORRECTION_SOURCE_VERSION_CONFLICT";

    // Отказ: содержимое источника не то, по которому поправка составлена.
    public const string ContentMismatchErrorCode = "CLAIM_CORRECTION_SOURCE_CONTENT_MISMATCH";

    // Отказ: указателя поправки в этом документе нет.
    public const string PointerNotFoundErrorCode = "CLAIM_CORRECTION_POINTER_NOT_FOUND";

    // Отказ: документ источника не разбирается как документ данных.
    public const string SourceNotParsableErrorCode = "CLAIM_CORRECTION_SOURCE_NOT_PARSABLE";

    // Read связывает поправку с фактическим источником и возвращает сырое значение рядом с
    // толкованием. Опознание источника берётся ИЗ САМОЙ поправки: отдельного довода для него нет,
    // поэтому рассогласовать поправку и источник нечем.
    public static OperationResult<ClaimCorrectionReading> Read(
        ClaimCorrection correction,
        ActualClaimSource actualSource)
    {
        // Без поправки читать нечего: это ошибка вызывающего, а не конфликт версии.
        if (correction is null)
            return OperationResult<ClaimCorrectionReading>.Fail(
                "Поправка толкования обязательна.",
                ClaimCorrection.CorrectionFieldsRequiredErrorCode);

        // Без фактического источника сверять нечего.
        if (actualSource is null || string.IsNullOrWhiteSpace(actualSource.Content))
            return OperationResult<ClaimCorrectionReading>.Fail(
                "Фактический источник обязателен: сверять отпечаток не с чем.",
                ActualSourceRequiredErrorCode);

        // Хранилище сверяется первым: поправка чужого хранилища к этому документу не относится.
        if (!string.Equals(correction.Source.Repository, actualSource.Repository, StringComparison.Ordinal))
            return OperationResult<ClaimCorrectionReading>.Fail(
                $"Поправка '{correction.CorrectionId}' составлена для хранилища " +
                $"'{correction.Source.Repository}', а предъявлен источник хранилища " +
                $"'{actualSource.Repository}'. Поправка не применяется.",
                RepositoryMismatchErrorCode);

        // Путь сверяется вторым: та же поправка к другому файлу не относится.
        if (!string.Equals(correction.Source.SourcePath, actualSource.SourcePath, StringComparison.Ordinal))
            return OperationResult<ClaimCorrectionReading>.Fail(
                $"Поправка '{correction.CorrectionId}' составлена для пути " +
                $"'{correction.Source.SourcePath}', а предъявлен источник по пути " +
                $"'{actualSource.SourcePath}'. Поправка не применяется.",
                PathMismatchErrorCode);

        // Срез сверяется третьим и называется КОНФЛИКТОМ ВЕРСИИ: поправка составлена по другой
        // версии того же файла, и молча применять её к этой нельзя.
        if (!string.Equals(correction.Source.SourceCommit, actualSource.SourceCommit, StringComparison.Ordinal))
            return OperationResult<ClaimCorrectionReading>.Fail(
                $"Поправка '{correction.CorrectionId}' составлена по срезу " +
                $"'{correction.Source.SourceCommit}', а предъявлен источник среза " +
                $"'{actualSource.SourceCommit}'. Это конфликт версии: поправка не применяется.",
                CommitConflictErrorCode);

        // Отпечаток считается ЗАНОВО из фактических байтов: заявленному отпечатку здесь не верят.
        var recomputed = ComputeSha256(actualSource.Content);
        // Несовпадение содержимого — отказ: поправка составлена по другому тексту.
        if (!string.Equals(correction.Source.SourceSha256, recomputed, StringComparison.OrdinalIgnoreCase))
            return OperationResult<ClaimCorrectionReading>.Fail(
                $"Поправка '{correction.CorrectionId}' составлена по источнику с отпечатком " +
                $"'{correction.Source.SourceSha256}', а у предъявленного содержимого отпечаток " +
                $"'{recomputed}'. Поправка не применяется.",
                ContentMismatchErrorCode);

        // Сырое значение берётся из самого документа по указателю поправки.
        var rawValue = ReadRawValueAtPointer(actualSource.Content, correction.Source.JsonPointer);
        // Отказ разбора или отсутствия указателя передаётся как есть, со своим кодом.
        if (!rawValue.IsSuccess)
            return rawValue.MapError<ClaimCorrectionReading>();

        // Прочитанная поправка: сырое значение и толкование лежат рядом и раздельно.
        return OperationResult<ClaimCorrectionReading>.Ok(new ClaimCorrectionReading(
            correction.CorrectionId,
            correction.Source,
            rawValue.Value!,
            correction.InterpretationStatus,
            correction.AuthoritativeInterpretation));
    }

    // ReadRawValueAtPointer достаёт сырое значение по указателю внутрь документа данных.
    //
    // Указатель записан принятым способом: части разделены косой чертой, а сама коса и знак
    // тильды внутри имени записаны как «~1» и «~0». Имена частей могут быть на любом языке:
    // в сохранённых пакетах аудита они русские.
    private static OperationResult<string> ReadRawValueAtPointer(string content, string pointer)
    {
        // Документ разбирается целиком; негодный документ — названный отказ, а не исключение.
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(content);
        }
        catch (JsonException exception)
        {
            // Причина разбора сохраняется в человеческом сообщении отказа.
            return OperationResult<string>.Fail(
                $"Источник не разбирается как документ данных: {exception.Message}",
                SourceNotParsableErrorCode);
        }

        // Пустой документ указателя не содержит.
        if (node is null)
            return OperationResult<string>.Fail(
                "Источник пуст и указателя не содержит.",
                SourceNotParsableErrorCode);

        // Указатель на сам корень допустим и означает весь документ.
        if (pointer == "/" || pointer.Length == 0)
            return OperationResult<string>.Ok(node.ToJsonString());

        // Части указателя идут после первой косой черты.
        foreach (var rawSegment in pointer.TrimStart('/').Split('/'))
        {
            // Обратная запись служебных знаков: сначала коса, затем тильда.
            var segment = rawSegment.Replace("~1", "/", StringComparison.Ordinal)
                                    .Replace("~0", "~", StringComparison.Ordinal);
            // Шаг внутрь записи документа по имени части.
            if (node is JsonObject jsonObject && jsonObject.TryGetPropertyValue(segment, out var child))
            {
                // Следующий шаг пойдёт уже от найденного места.
                node = child;
                continue;
            }

            // Шаг внутрь списка по номеру части.
            if (node is JsonArray jsonArray &&
                int.TryParse(segment, out var index) &&
                index >= 0 &&
                index < jsonArray.Count)
            {
                // Следующий шаг пойдёт от элемента списка.
                node = jsonArray[index];
                continue;
            }

            // Такой части в документе нет: поправке нечего поправлять, и это названный отказ.
            return OperationResult<string>.Fail(
                $"Указатель '{pointer}' в этом источнике не найден: нет части '{segment}'.",
                PointerNotFoundErrorCode);
        }

        // Пустое место по существующему указателю — это тоже отсутствие значения.
        if (node is null)
            return OperationResult<string>.Fail(
                $"Указатель '{pointer}' в этом источнике не ведёт к значению.",
                PointerNotFoundErrorCode);

        // Сырое значение возвращается как есть: строка без кавычек, прочее — своей записью.
        return OperationResult<string>.Ok(
            node is JsonValue value && value.TryGetValue<string>(out var text)
                ? text
                : node.ToJsonString());
    }

    // ComputeSha256 считает отпечаток точных байтов текста в кодировке UTF-8.
    private static string ComputeSha256(string value)
        // Пустой текст сюда не доходит: он отвергнут выше как отсутствие источника.
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

// ClaimCorrectionLoader — загрузчик пакета поправок аудита.
//
// Почему загрузчик живёт рядом с читателем. Единственный способ получить поправку — создать её
// через проверку опознания источника. Если бы загрузчик собирал поправки своим путём, он мог бы
// впустить строку без источника, и дыра вернулась бы с другой стороны. Поэтому он проходит через
// ту же самую проверку.
public static class ClaimCorrectionLoader
{
    // Отказ: пакет поправок не разбирается или не несёт списка поправок.
    public const string PackageNotParsableErrorCode = "CLAIM_CORRECTION_PACKAGE_NOT_PARSABLE";

    // Имя списка поправок в сохранённом пакете аудита.
    private const string CorrectionsPropertyName = "corrections";

    // Load разбирает пакет и возвращает поправки. Строка без полного опознания источника
    // поправкой не становится вовсе: отказ приходит из той же единственной проверки.
    public static OperationResult<IReadOnlyList<ClaimCorrection>> Load(string packageJson)
    {
        // Пустой пакет — названный отказ, а не пустой список.
        if (string.IsNullOrWhiteSpace(packageJson))
            return OperationResult<IReadOnlyList<ClaimCorrection>>.Fail(
                "Пакет поправок пуст.",
                PackageNotParsableErrorCode);

        // Разбор пакета: негодный пакет даёт названный отказ, а не исключение наружу.
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(packageJson);
        }
        catch (JsonException exception)
        {
            // Причина разбора сохраняется в человеческом сообщении.
            return OperationResult<IReadOnlyList<ClaimCorrection>>.Fail(
                $"Пакет поправок не разбирается: {exception.Message}",
                PackageNotParsableErrorCode);
        }

        // Список поправок обязателен: пакет без него читать нечего.
        if (root is not JsonObject rootObject ||
            !rootObject.TryGetPropertyValue(CorrectionsPropertyName, out var correctionsNode) ||
            correctionsNode is not JsonArray corrections)
        {
            return OperationResult<IReadOnlyList<ClaimCorrection>>.Fail(
                "Пакет поправок не несёт списка поправок.",
                PackageNotParsableErrorCode);
        }

        // Готовые поправки копятся по порядку пакета.
        var loaded = new List<ClaimCorrection>(corrections.Count);
        foreach (var node in corrections)
        {
            // Строка, не являющаяся записью, поправкой стать не может.
            if (node is not JsonObject row)
                return OperationResult<IReadOnlyList<ClaimCorrection>>.Fail(
                    "Строка пакета поправок не является записью.",
                    PackageNotParsableErrorCode);

            // Опознание источника собирается из полей строки как есть, без домыслов.
            var source = new ClaimCorrectionSource(
                Text(row, "repository"),
                Text(row, "source_commit"),
                Text(row, "source_path"),
                Text(row, "source_sha256"),
                Text(row, "json_pointer"));

            // Та же единственная проверка: неполное опознание источника поправкой не становится.
            var correction = ClaimCorrection.Create(
                Text(row, "id"),
                source,
                Text(row, "original_claim"),
                Text(row, "interpretation_status"),
                Text(row, "authoritative_interpretation"),
                Text(row, "reason"));
            // Отказ создания передаётся наружу как есть, со своим кодом и причиной.
            if (!correction.IsSuccess)
                return correction.MapError<IReadOnlyList<ClaimCorrection>>();

            // Принятая поправка добавляется в список.
            loaded.Add(correction.Value!);
        }

        // Готовый список поправок пакета.
        return OperationResult<IReadOnlyList<ClaimCorrection>>.Ok(loaded);
    }

    // Text читает строковое поле записи; отсутствие поля даёт пустую строку, которую выше
    // отвергает проверка опознания.
    private static string Text(JsonObject row, string propertyName)
        // Отсутствующее и нестроковое значение одинаково означают «поля нет».
        => row.TryGetPropertyValue(propertyName, out var value) &&
           value is JsonValue jsonValue &&
           jsonValue.TryGetValue<string>(out var text)
            ? text
            : string.Empty;
}
