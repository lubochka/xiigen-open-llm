// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Globalization;
using System.Text;
using AgentInfrastructure;

// Долговечное хранилище расписки допуска материала к обучению.
//
// Зачем оно нужно, по-человечески. Расписка допуска жила только в памяти одного запуска: её читал
// один диагностический след и больше никто. После завершения процесса не оставалось ничего, по
// чему можно было бы сказать, какой именно материал был допущен к этому обучению и что о нём было
// доказано. Связка «материал — обучение» существовала ровно до выхода из метода, то есть
// доказательством не была: доказательство обязано переживать перезапуск и читаться обратно.
//
// Что это хранилище делает. Записывает расписку в один файл однозначным построчным видом и читает
// её обратно той же парой правил. Вместе с полями пишется отпечаток самих полей, поэтому
// изменённый на диске файл не читается молча: чтение называет расхождение отказом.
//
// Чего оно НЕ делает, названо прямо. Это не подпись и не защита от подделки: правило записи
// открыто, и тот, кто изменит файл, может пересчитать отпечаток. Отпечаток ловит порчу и
// неполную запись, а не злоумышленника с правами на этот каталог. Это и не публикация расписки в
// наружный результат обучения и не запись её в слепок состояния: наружный договор результата и
// версия слепка принадлежат своим владельцам и этой правкой не меняются.
internal static class CapabilityMaterialAdmissionReceiptStore
{
    // Имя вида записи. Входит в файл первой строкой, чтобы будущий читатель отличил этот вид от
    // другого и не прочитал чужой файл как свой.
    internal const string FormatVersion = "capability-material-admission-receipt-v1";

    // Коды отказов хранилища. Каждый называет ровно одну причину.
    internal const string WriteFailedErrorCode = "CAPABILITY_MATERIAL_RECEIPT_WRITE_FAILED";
    internal const string ReadFailedErrorCode = "CAPABILITY_MATERIAL_RECEIPT_READ_FAILED";
    internal const string NotFoundErrorCode = "CAPABILITY_MATERIAL_RECEIPT_NOT_FOUND";
    internal const string MalformedErrorCode = "CAPABILITY_MATERIAL_RECEIPT_MALFORMED";
    internal const string DigestMismatchErrorCode = "CAPABILITY_MATERIAL_RECEIPT_DIGEST_MISMATCH";

    // Метка отсутствующего значения в файле. Отдельная затем, чтобы «поля нет» никогда не
    // совпадало с «поле пустое»: это разные факты и о расписке они говорят разное.
    private const string AbsentValueToken = "~absent";

    // ResolvePath называет точный файл расписки этого запуска. Имя складывается из запуска и
    // профиля оценки, поэтому два запуска не пишут в один файл.
    internal static string ResolvePath(string receiptRoot, string runId, string profileId)
        // Недопустимые для имени файла знаки заменяются, иначе имя профиля создало бы подкаталог.
        => Path.Combine(
            receiptRoot,
            $"{SanitizeNamePart(runId)}--{SanitizeNamePart(profileId)}.receipt");

    // Save записывает расписку в указанный файл и сразу читает её обратно, сверяя все поля.
    // Запись без обратного чтения доказывала бы только то, что вызов не упал.
    internal static OperationResult<string> Save(
        string receiptRoot,
        string runId,
        CapabilityMaterialAdmissionReceipt receipt)
    {
        // Без расписки писать нечего: это отсутствие входа, а не отказ материала.
        if (receipt is null)
            return OperationResult<string>.Fail(
                "Расписка допуска обязательна для записи.",
                WriteFailedErrorCode);

        // Путь считается до любой записи, чтобы отказ называл точный файл.
        var path = ResolvePath(receiptRoot, runId, receipt.ProfileId);
        try
        {
            // Каталог расписок создаётся при первой записи и далее переиспользуется.
            Directory.CreateDirectory(receiptRoot);
            // Содержимое собирается целиком в памяти: частично записанного файла не остаётся.
            File.WriteAllText(path, Render(receipt), new UTF8Encoding(false));
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            // Причина отказа называет точный путь и слова самой ошибки файловой системы.
            return OperationResult<string>.Fail(
                $"Расписку допуска не удалось записать в '{path}': {exception.Message}",
                WriteFailedErrorCode);
        }

        // Обратное чтение сразу после записи: записанное обязано читаться.
        var readBack = Load(path);
        // Отказ обратного чтения передаётся как есть, вместе с его кодом.
        if (!readBack.IsSuccess)
            return readBack.MapError<string>();

        // И прочитанное обязано совпасть с тем, что записывали, по ВСЕМ полям.
        if (!ReceiptsMatch(receipt, readBack.Value!))
            return OperationResult<string>.Fail(
                $"Расписка допуска в '{path}' прочиталась не теми значениями, которые " +
                "записывались. Запись не доказана.",
                WriteFailedErrorCode);

        // Путь сохранённого файла возвращается вызывающему как свидетельство записи.
        return OperationResult<string>.Ok(path);
    }

    // Load читает расписку обратно и сверяет отпечаток её полей.
    internal static OperationResult<CapabilityMaterialAdmissionReceipt> Load(string path)
    {
        string text;
        try
        {
            // Отсутствие файла — отдельный названный случай, а не ошибка чтения.
            if (!File.Exists(path))
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Файла расписки допуска '{path}' нет.",
                    NotFoundErrorCode);

            // Файл читается целиком: расписка небольшая и читается одним куском.
            text = File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            // Причина называет точный путь и слова ошибки файловой системы.
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                $"Расписку допуска из '{path}' не удалось прочитать: {exception.Message}",
                ReadFailedErrorCode);
        }

        // Строки разбираются по порядку записи: вид записи задаёт порядок однозначно.
        var lines = text.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        var cursor = 0;
        try
        {
            // Первая строка называет вид записи: чужой вид дальше не разбирается.
            var format = NextValue(lines, ref cursor, "format");
            if (!string.Equals(format, FormatVersion, StringComparison.Ordinal))
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Файл '{path}' записан видом '{format}', а не '{FormatVersion}'.",
                    MalformedErrorCode);

            // Поля читаются ровно в том порядке, в котором их записал Render.
            var profileId = NextValue(lines, ref cursor, "profileId") ?? string.Empty;
            var schemaVersion = NextValue(lines, ref cursor, "schemaVersion") ?? string.Empty;
            var checkedRowCount = NextInt(lines, ref cursor, "checkedRowCount");
            var trainRowCount = NextInt(lines, ref cursor, "trainRowCount");
            var devRowCount = NextInt(lines, ref cursor, "devRowCount");
            var testRowCount = NextInt(lines, ref cursor, "testRowCount");
            var trainFamilies = NextList(lines, ref cursor, "trainFamilies");
            var heldOutFamilies = NextList(lines, ref cursor, "heldOutFamilies");
            var normalization = (CapabilityMaterialNormalizationPolicy)
                NextInt(lines, ref cursor, "normalizationPolicy");
            var intersection = new CapabilityMaterialIntersectionPolicy(
                NextBool(lines, ref cursor, "forbidTrainTestContentOverlap"),
                NextBool(lines, ref cursor, "forbidTrainDevContentOverlap"),
                NextBool(lines, ref cursor, "forbidTrainTestFamilyOverlap"),
                NextBool(lines, ref cursor, "forbidTrainDevFamilyOverlap"),
                NextBool(lines, ref cursor, "compareByNormalizedContent"));
            var modelVisibleCollisionCheck =
                NextValue(lines, ref cursor, "modelVisibleCollisionCheck") ?? string.Empty;
            var consumedPreferenceRowCount = NextInt(lines, ref cursor, "consumedPreferenceRowCount");
            var consumedMaterialSha256 = NextValue(lines, ref cursor, "consumedMaterialSha256");
            var consumedMaterialBinding =
                NextValue(lines, ref cursor, "consumedMaterialBinding") ?? string.Empty;
            var consumedIdentitySha256 = NextValue(lines, ref cursor, "consumedIdentitySha256");
            var consumedRetrievalCorpusRowCount =
                NextInt(lines, ref cursor, "consumedRetrievalCorpusRowCount");
            // Последним полем лежит отпечаток самих полей.
            var storedDigest = NextValue(lines, ref cursor, "fieldsDigest") ?? string.Empty;

            // Расписка собирается из прочитанных значений.
            var restored = new CapabilityMaterialAdmissionReceipt(
                profileId,
                schemaVersion,
                checkedRowCount,
                trainRowCount,
                devRowCount,
                testRowCount,
                trainFamilies,
                heldOutFamilies,
                normalization,
                intersection,
                modelVisibleCollisionCheck,
                consumedPreferenceRowCount,
                consumedMaterialSha256,
                consumedMaterialBinding)
            {
                ConsumedIdentitySha256 = consumedIdentitySha256,
                ConsumedRetrievalCorpusRowCount = consumedRetrievalCorpusRowCount
            };

            // Отпечаток пересчитывается из ПРОЧИТАННЫХ полей и сверяется с записанным.
            var recomputed = ComputeFieldsDigest(restored);
            if (!string.Equals(recomputed, storedDigest, StringComparison.Ordinal))
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Отпечаток полей расписки в '{path}' не совпал: в файле '{storedDigest}', " +
                    $"посчитанный из прочитанных полей '{recomputed}'. Файл изменён или записан " +
                    "не полностью.",
                    DigestMismatchErrorCode);

            // Расписка прочитана и сверена.
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Ok(restored);
        }
        catch (FormatException exception)
        {
            // Непригодная строка называется человеческой причиной, а не исключением наружу.
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                $"Файл расписки допуска '{path}' разобрать не удалось: {exception.Message}",
                MalformedErrorCode);
        }
    }

    // Render собирает содержимое файла. Порядок строк задан здесь и только здесь.
    private static string Render(CapabilityMaterialAdmissionReceipt receipt)
    {
        // Накопитель строк файла.
        var builder = new StringBuilder();
        // Вид записи идёт первой строкой.
        AppendValue(builder, "format", FormatVersion);
        // Дальше поля расписки в порядке её объявления.
        AppendValue(builder, "profileId", receipt.ProfileId);
        AppendValue(builder, "schemaVersion", receipt.SchemaVersion);
        AppendInt(builder, "checkedRowCount", receipt.CheckedRowCount);
        AppendInt(builder, "trainRowCount", receipt.TrainRowCount);
        AppendInt(builder, "devRowCount", receipt.DevRowCount);
        AppendInt(builder, "testRowCount", receipt.TestRowCount);
        AppendList(builder, "trainFamilies", receipt.TrainFamilies);
        AppendList(builder, "heldOutFamilies", receipt.HeldOutFamilies);
        AppendInt(builder, "normalizationPolicy", (int)receipt.NormalizationPolicy);
        AppendBool(builder, "forbidTrainTestContentOverlap", receipt.IntersectionPolicy.ForbidTrainTestContentOverlap);
        AppendBool(builder, "forbidTrainDevContentOverlap", receipt.IntersectionPolicy.ForbidTrainDevContentOverlap);
        AppendBool(builder, "forbidTrainTestFamilyOverlap", receipt.IntersectionPolicy.ForbidTrainTestFamilyOverlap);
        AppendBool(builder, "forbidTrainDevFamilyOverlap", receipt.IntersectionPolicy.ForbidTrainDevFamilyOverlap);
        AppendBool(builder, "compareByNormalizedContent", receipt.IntersectionPolicy.CompareByNormalizedContent);
        AppendValue(builder, "modelVisibleCollisionCheck", receipt.ModelVisibleCollisionCheck);
        AppendInt(builder, "consumedPreferenceRowCount", receipt.ConsumedPreferenceRowCount);
        AppendValue(builder, "consumedMaterialSha256", receipt.ConsumedMaterialSha256);
        AppendValue(builder, "consumedMaterialBinding", receipt.ConsumedMaterialBinding);
        AppendValue(builder, "consumedIdentitySha256", receipt.ConsumedIdentitySha256);
        AppendInt(builder, "consumedRetrievalCorpusRowCount", receipt.ConsumedRetrievalCorpusRowCount);
        // Отпечаток полей идёт последней строкой: он считается по уже известным значениям.
        AppendValue(builder, "fieldsDigest", ComputeFieldsDigest(receipt));
        // Готовое содержимое возвращается одной строкой файла.
        return builder.ToString();
    }

    // ComputeFieldsDigest считает отпечаток полей расписки тем же единственным владельцем правила
    // однозначного кодирования, что и сторож происхождения: длина поля перед его байтами.
    private static string ComputeFieldsDigest(CapabilityMaterialAdmissionReceipt receipt)
    {
        // Назначение потока называет именно расписку: тот же набор значений в другом потоке даст
        // другой отпечаток.
        var writer = new UnambiguousFieldEncoding.Writer("capability-material-admission-receipt");
        // Текстовые и счётные поля идут в порядке объявления расписки.
        writer.WriteField(receipt.ProfileId)
              .WriteField(receipt.SchemaVersion)
              .WriteCount(receipt.CheckedRowCount)
              .WriteCount(receipt.TrainRowCount)
              .WriteCount(receipt.DevRowCount)
              .WriteCount(receipt.TestRowCount);
        // Число семей входит в поток до самих семей: потеря семьи меняет отпечаток.
        writer.WriteCount(receipt.TrainFamilies.Count);
        foreach (var family in receipt.TrainFamilies)
            writer.WriteField(family);
        writer.WriteCount(receipt.HeldOutFamilies.Count);
        foreach (var family in receipt.HeldOutFamilies)
            writer.WriteField(family);
        // Выбранные правила входят своими числовыми значениями.
        writer.WriteCount((int)receipt.NormalizationPolicy)
              .WriteCount(receipt.IntersectionPolicy.ForbidTrainTestContentOverlap ? 1 : 0)
              .WriteCount(receipt.IntersectionPolicy.ForbidTrainDevContentOverlap ? 1 : 0)
              .WriteCount(receipt.IntersectionPolicy.ForbidTrainTestFamilyOverlap ? 1 : 0)
              .WriteCount(receipt.IntersectionPolicy.ForbidTrainDevFamilyOverlap ? 1 : 0)
              .WriteCount(receipt.IntersectionPolicy.CompareByNormalizedContent ? 1 : 0)
              .WriteField(receipt.ModelVisibleCollisionCheck)
              .WriteCount(receipt.ConsumedPreferenceRowCount)
              .WriteField(receipt.ConsumedMaterialSha256)
              .WriteField(receipt.ConsumedMaterialBinding)
              .WriteField(receipt.ConsumedIdentitySha256)
              .WriteCount(receipt.ConsumedRetrievalCorpusRowCount);
        // Отпечаток считается один раз по всему потоку.
        return writer.Digest();
    }

    // ReceiptsMatch сверяет две расписки по всем полям. Запись сравнивается с прочитанным именно
    // так, а не по отпечатку: отпечаток считается из тех же полей и сам по себе круговой довод.
    private static bool ReceiptsMatch(
        CapabilityMaterialAdmissionReceipt written,
        CapabilityMaterialAdmissionReceipt read)
        // Сначала простые поля, затем наборы семей по порядку и составу.
        => string.Equals(written.ProfileId, read.ProfileId, StringComparison.Ordinal) &&
           string.Equals(written.SchemaVersion, read.SchemaVersion, StringComparison.Ordinal) &&
           written.CheckedRowCount == read.CheckedRowCount &&
           written.TrainRowCount == read.TrainRowCount &&
           written.DevRowCount == read.DevRowCount &&
           written.TestRowCount == read.TestRowCount &&
           written.TrainFamilies.SequenceEqual(read.TrainFamilies, StringComparer.Ordinal) &&
           written.HeldOutFamilies.SequenceEqual(read.HeldOutFamilies, StringComparer.Ordinal) &&
           written.NormalizationPolicy == read.NormalizationPolicy &&
           written.IntersectionPolicy == read.IntersectionPolicy &&
           string.Equals(written.ModelVisibleCollisionCheck, read.ModelVisibleCollisionCheck, StringComparison.Ordinal) &&
           written.ConsumedPreferenceRowCount == read.ConsumedPreferenceRowCount &&
           string.Equals(written.ConsumedMaterialSha256, read.ConsumedMaterialSha256, StringComparison.Ordinal) &&
           string.Equals(written.ConsumedMaterialBinding, read.ConsumedMaterialBinding, StringComparison.Ordinal) &&
           string.Equals(written.ConsumedIdentitySha256, read.ConsumedIdentitySha256, StringComparison.Ordinal) &&
           written.ConsumedRetrievalCorpusRowCount == read.ConsumedRetrievalCorpusRowCount;

    // AppendValue пишет одну строку «имя, знак табуляции, закодированное значение».
    private static void AppendValue(StringBuilder builder, string name, string? value)
        // Значение кодируется так, чтобы перевод строки внутри него не создал новую строку файла.
        => builder.Append(name).Append('\t').Append(Encode(value)).Append('\n');

    // AppendInt пишет целое число неизменным видом, не зависящим от языка машины.
    private static void AppendInt(StringBuilder builder, string name, int value)
        => AppendValue(builder, name, value.ToString(CultureInfo.InvariantCulture));

    // AppendBool пишет признак словами «да»/«нет» в неизменном машинном виде.
    private static void AppendBool(StringBuilder builder, string name, bool value)
        => AppendValue(builder, name, value ? "1" : "0");

    // AppendList пишет сначала число элементов, затем сами элементы по одному в строке.
    private static void AppendList(StringBuilder builder, string name, IReadOnlyList<string> values)
    {
        // Число элементов записывается первым: по нему читатель знает, сколько строк забрать.
        AppendInt(builder, name + ".count", values.Count);
        for (var index = 0; index < values.Count; index++)
        {
            // Каждый элемент получает своё имя с номером: порядок виден и в самом файле.
            AppendValue(builder, $"{name}.{index}", values[index]);
        }
    }

    // NextValue забирает очередную строку, проверяет её имя и возвращает раскодированное значение.
    private static string? NextValue(string[] lines, ref int cursor, string expectedName)
    {
        // Кончившиеся строки означают неполный файл.
        if (cursor >= lines.Length)
            throw new FormatException($"поле '{expectedName}' отсутствует: файл кончился");

        // Строка делится ровно по первому знаку табуляции: в значении он уже закодирован.
        var line = lines[cursor++];
        var separator = line.IndexOf('\t');
        if (separator < 0)
            throw new FormatException($"строка поля '{expectedName}' не содержит разделителя");

        // Имя поля обязано совпасть с ожидаемым: иначе порядок полей не тот.
        var name = line[..separator];
        if (!string.Equals(name, expectedName, StringComparison.Ordinal))
            throw new FormatException($"ожидалось поле '{expectedName}', прочитано '{name}'");

        // Значение раскодируется обратным правилом.
        return Decode(line[(separator + 1)..]);
    }

    // NextInt читает целое число той же парой правил.
    private static int NextInt(string[] lines, ref int cursor, string expectedName)
    {
        // Значение читается как текст и разбирается неизменным видом.
        var value = NextValue(lines, ref cursor, expectedName);
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            throw new FormatException($"поле '{expectedName}' не является целым числом");

        // Разобранное число возвращается вызывающему.
        return parsed;
    }

    // NextBool читает признак из того же неизменного вида.
    private static bool NextBool(string[] lines, ref int cursor, string expectedName)
        // Единица означает «да», ноль — «нет»; иное значение означает непригодный файл.
        => NextInt(lines, ref cursor, expectedName) switch
        {
            0 => false,
            1 => true,
            _ => throw new FormatException($"поле '{expectedName}' не является признаком")
        };

    // NextList читает число элементов и затем сами элементы по их именам с номерами.
    private static IReadOnlyList<string> NextList(string[] lines, ref int cursor, string expectedName)
    {
        // Число элементов читается первым и задаёт, сколько строк забрать дальше.
        var count = NextInt(lines, ref cursor, expectedName + ".count");
        if (count < 0)
            throw new FormatException($"у набора '{expectedName}' отрицательное число элементов");

        // Накопитель элементов набора известного размера.
        var values = new List<string>(count);
        for (var index = 0; index < count; index++)
        {
            // Элемент читается по своему имени с номером: порядок проверяется самим чтением.
            values.Add(NextValue(lines, ref cursor, $"{expectedName}.{index}") ?? string.Empty);
        }

        // Готовый набор возвращается только для чтения.
        return values;
    }

    // Encode кодирует значение так, чтобы ни перевод строки, ни табуляция внутри него не
    // превратились в границу строки или поля файла.
    private static string Encode(string? value)
    {
        // Отсутствующее значение несёт свою метку и не совпадает с пустой строкой.
        if (value is null)
            return AbsentValueToken;

        // Сначала экранируется сама обратная косая черта, иначе её нельзя было бы отличить.
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }

    // Decode возвращает значение обратным правилом, знак за знаком.
    private static string? Decode(string encoded)
    {
        // Метка отсутствия читается обратно именно как отсутствие значения.
        if (string.Equals(encoded, AbsentValueToken, StringComparison.Ordinal))
            return null;

        // Накопитель раскодированного значения.
        var builder = new StringBuilder(encoded.Length);
        for (var index = 0; index < encoded.Length; index++)
        {
            // Обычный знак переносится как есть.
            if (encoded[index] != '\\')
            {
                builder.Append(encoded[index]);
                continue;
            }

            // Косая черта в самом конце означает непригодную запись.
            if (index + 1 >= encoded.Length)
                throw new FormatException("незавершённое экранирование в значении поля");

            // Следующий знак называет, что именно было закодировано.
            index++;
            builder.Append(encoded[index] switch
            {
                '\\' => '\\',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                _ => throw new FormatException("неизвестное экранирование в значении поля")
            });
        }

        // Готовое значение возвращается вызывающему.
        return builder.ToString();
    }

    // SanitizeNamePart заменяет знаки, недопустимые в имени файла, чтобы имя запуска или профиля
    // не создало подкаталог и не вышло за назначенный каталог расписок.
    private static string SanitizeNamePart(string? value)
    {
        // Пустое имя заменяется словом, иначе файл остался бы без имени.
        if (string.IsNullOrWhiteSpace(value))
            return "unnamed";

        // Накопитель безопасного имени.
        var builder = new StringBuilder(value.Length);
        // Запрещённые знаки имени файла берутся у самой системы, плюс оба разделителя путей.
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var character in value)
        {
            // Недопустимый знак и любой разделитель пути заменяются подчёркиванием.
            builder.Append(
                invalid.Contains(character) || character is '/' or '\\' or ':'
                    ? '_'
                    : character);
        }

        // Готовое имя возвращается вызывающему.
        return builder.ToString();
    }
}
