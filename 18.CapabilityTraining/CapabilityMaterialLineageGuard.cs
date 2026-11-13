// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

// Детерминированный сторож происхождения материала обучения способности.
//
// Что он делает, по-человечески. Берёт ФАКТИЧЕСКИЕ строки, подготовленные к обучению, и
// замороженный доверенный источник. Пересчитывает отпечаток каждой фактической строки из её
// собственных байтов, находит по этому отпечатку строку источника, берёт оттуда семью и роль и
// только после этого считает запрещённые пересечения обучения с проверкой и экзаменом.
//
// Два дефекта опытного проверяющего, которые здесь НЕ повторены.
//
// Первый. Опытный проверяющий искал совпадение по ПРЕДЪЯВЛЕННЫМ полям отпечатка записи
// (`MaterialLedgerVerifier.cs`, строки 61 и 64) и ни разу не пересчитывал их из самого
// содержания. Это то же самое, что доверять входному признаку вместо его вывода из данных —
// правила проекта такое запрещают прямо. Согласованно подделанная расписка прошла бы у него как
// честная, и изменённый текст со старым отпечатком тоже. Здесь отпечаток ВСЕГДА считается из
// фактических байтов, а предъявленное значение сверяется с посчитанным; расхождение — отказ.
//
// Второй. Опытный проверяющий строил указатель от приведённого текста к строке набора через
// `GroupBy(...).First()` (строки 37 и 42): при двух семьях с одним приведённым отпечатком молча
// побеждала первая. Здесь такая неоднозначность — названный отказ, а не тихий выбор.
//
// Чего этот сторож НЕ делает. Он не обучает, не опрашивает обученную память, не создаёт модель,
// не разбивает наборы и не определяет смысловую семью по тексту: семья берётся из замороженного
// источника. Он не доказывает независимость ПОСЛЕ превращения текста в числа для модели —
// источник-непересекающийся не означает непересекающийся после кодирования, и в расписке это
// поле честно стоит как «не измерено».
public static class CapabilityMaterialLineageGuard
{
    // Коды отказов. Каждый называет ровно одну причину, чтобы вызывающий различал их по коду.
    public const string MaterialRequiredErrorCode = "CAPABILITY_MATERIAL_REQUIRED";
    public const string SourceRowsRequiredErrorCode = "CAPABILITY_MATERIAL_SOURCE_ROWS_REQUIRED";
    public const string ActualRowsRequiredErrorCode = "CAPABILITY_MATERIAL_ACTUAL_ROWS_REQUIRED";
    public const string PresentedDigestMismatchErrorCode = "CAPABILITY_MATERIAL_PRESENTED_DIGEST_MISMATCH";
    public const string AmbiguousContentLineageErrorCode = "CAPABILITY_MATERIAL_AMBIGUOUS_CONTENT_LINEAGE";
    public const string ContentNotInSourceErrorCode = "CAPABILITY_MATERIAL_CONTENT_NOT_IN_SOURCE";
    public const string LineageContradictsContentErrorCode = "CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT";
    public const string DuplicateSourceRowErrorCode = "CAPABILITY_MATERIAL_DUPLICATE_SOURCE_ROW";
    public const string ForbiddenContentOverlapErrorCode = "CAPABILITY_MATERIAL_FORBIDDEN_CONTENT_OVERLAP";
    public const string ForbiddenFamilyOverlapErrorCode = "CAPABILITY_MATERIAL_FORBIDDEN_FAMILY_OVERLAP";
    // Обучать нечем: фактически потребляемого материала не подано вовсе.
    public const string ConsumedMaterialRequiredErrorCode = "CAPABILITY_MATERIAL_CONSUMED_REQUIRED";
    // В том, что обучают, найдено содержание отложенной строки замороженного источника.
    public const string ConsumedHeldOutContentErrorCode = "CAPABILITY_MATERIAL_CONSUMED_HELD_OUT_CONTENT";
    // Не всё разрешённое обучение дошло до обучателя: часть допущенных учебных строк потеряна.
    public const string ConsumedTrainIncompleteErrorCode = "CAPABILITY_MATERIAL_CONSUMED_TRAIN_INCOMPLETE";
    // После допуска содержание носителя изменилось: читать его дальше нельзя.
    public const string ConsumedMaterialChangedErrorCode = "CAPABILITY_MATERIAL_CONSUMED_CHANGED_AFTER_ADMISSION";

    // ValidateBeforeLearning проверяет фактический материал ДО любого обучения и возвращает
    // расписку допуска. Отказ — обычный результат операции с человеческой причиной и кодом;
    // исключений эта проверка не бросает.
    public static OperationResult<CapabilityMaterialAdmissionReceipt> ValidateBeforeLearning(
        CapabilityEvaluationMaterial material,
        IReadOnlyList<CapabilityActualMaterialRow> actualOwnedRows)
    {
        // Без доверенного договора проверять нечего: это не отказ материала, а отсутствие входа.
        if (material is null)
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                "Доверенный договор оценки материала обязателен для проверки происхождения.",
                MaterialRequiredErrorCode);

        // Пустой замороженный источник не даёт ни одной семьи, значит и проверять не по чему.
        if (material.SourceRows is null || material.SourceRows.Count == 0)
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                "Замороженный источник оценки не содержит ни одной строки.",
                SourceRowsRequiredErrorCode);

        // Фактические строки обязательны: расписка без лежащих под ней строк не принимается.
        if (actualOwnedRows is null || actualOwnedRows.Count == 0)
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                "Фактические строки материала обязательны: расписку без них принять нельзя.",
                ActualRowsRequiredErrorCode);

        // Имя строки источника должно быть уникальным: иначе одна и та же ссылка ведёт в два места.
        var duplicateSourceRow = material.SourceRows
            .GroupBy(row => row.SourceRowId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateSourceRow is not null)
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                $"Строка источника '{duplicateSourceRow.Key}' описана {duplicateSourceRow.Count()} раза: " +
                "ссылка на неё была бы неоднозначной.",
                DuplicateSourceRowErrorCode);

        // Указатель от ПЕРЕСЧИТАННОГО отпечатка точных байтов источника к его строкам. Значение —
        // список, а не одна строка: несколько строк с одним отпечатком не победят молча.
        var sourceByExactDigest = GroupSourceRowsBy(material.SourceRows, row => ComputeSha256(row.Content));

        // Тот же указатель по приведённому тексту — только если приведение вообще выбрано.
        var sourceByNormalizedDigest = material.NormalizationPolicy ==
                CapabilityMaterialNormalizationPolicy.ExactBytesOnly
            ? new Dictionary<string, List<CapabilitySourceRow>>(StringComparer.Ordinal)
            : GroupSourceRowsBy(
                material.SourceRows,
                row => ComputeSha256(Normalize(row.Content, material.NormalizationPolicy)));

        // Здесь копятся разрешённые к обучению строки и отпечатки по ролям.
        var trainFamilies = new SortedSet<string>(StringComparer.Ordinal);
        var heldOutFamilies = new SortedSet<string>(StringComparer.Ordinal);
        var trainExactDigests = new HashSet<string>(StringComparer.Ordinal);
        var heldOutExactDigests = new HashSet<string>(StringComparer.Ordinal);
        var trainNormalizedDigests = new HashSet<string>(StringComparer.Ordinal);
        var heldOutNormalizedDigests = new HashSet<string>(StringComparer.Ordinal);
        var trainRowCount = 0;
        var devRowCount = 0;
        var testRowCount = 0;

        // Каждая фактическая строка проверяется отдельно и по собственным байтам.
        foreach (var actual in actualOwnedRows)
        {
            // Отпечаток точных байтов считается ЗАНОВО из фактического текста строки.
            var recomputedExact = ComputeSha256(actual.Content);
            // Отпечаток приведённого текста тоже считается из того же фактического текста.
            var recomputedNormalized = ComputeSha256(Normalize(actual.Content, material.NormalizationPolicy));

            // Предъявленный отпечаток точных байтов обязан совпасть с посчитанным. Это и есть
            // первый невоспроизведённый дефект: доверять заявке вместо пересчёта нельзя.
            if (!DigestMatches(actual.PresentedContentSha256, recomputedExact))
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Строка '{actual.ExampleId}': предъявленный отпечаток содержания " +
                    $"'{Describe(actual.PresentedContentSha256)}' не совпал с посчитанным из фактического " +
                    $"текста '{recomputedExact}'. Материал отвергнут до обучения.",
                    PresentedDigestMismatchErrorCode);

            // То же требование к отпечатку приведённого текста, когда приведение выбрано.
            if (material.NormalizationPolicy != CapabilityMaterialNormalizationPolicy.ExactBytesOnly &&
                !DigestMatches(actual.PresentedNormalizedSha256, recomputedNormalized))
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Строка '{actual.ExampleId}': предъявленный отпечаток приведённого текста " +
                    $"'{Describe(actual.PresentedNormalizedSha256)}' не совпал с посчитанным " +
                    $"'{recomputedNormalized}'. Материал отвергнут до обучения.",
                    PresentedDigestMismatchErrorCode);

            // Строка источника ищется по ПЕРЕСЧИТАННОМУ отпечатку точных байтов.
            var resolved = ResolveSingleSourceRow(sourceByExactDigest, recomputedExact);
            // Неоднозначность по точным байтам — названный отказ, а не выбор первой строки.
            if (resolved.Ambiguous)
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Строка '{actual.ExampleId}': одному и тому же содержанию соответствует несколько " +
                    $"семей источника [{string.Join(", ", resolved.Families)}]. Однозначного происхождения " +
                    "нет, молчаливый выбор первой семьи запрещён.",
                    AmbiguousContentLineageErrorCode);

            // Если по точным байтам не нашлось, пробуем приведённый текст — там же правило.
            if (resolved.Row is null && sourceByNormalizedDigest.Count > 0)
            {
                var byNormalized = ResolveSingleSourceRow(sourceByNormalizedDigest, recomputedNormalized);
                // Та же неоднозначность, тот же названный отказ.
                if (byNormalized.Ambiguous)
                    return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                        $"Строка '{actual.ExampleId}': приведённому тексту соответствует несколько семей " +
                        $"источника [{string.Join(", ", byNormalized.Families)}]. Однозначного происхождения " +
                        "нет, молчаливый выбор первой семьи запрещён.",
                        AmbiguousContentLineageErrorCode);

                // Иначе берём найденную по приведённому тексту строку: переименование и правка
                // знаков препинания новой семьи не создают.
                resolved = byNormalized;
            }

            // Содержание, которого в замороженном источнике нет, принять нельзя.
            if (resolved.Row is null)
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Строка '{actual.ExampleId}': её содержание отсутствует в замороженном источнике " +
                    $"оценки '{material.ProfileId}'. Материал отвергнут до обучения.",
                    ContentNotInSourceErrorCode);

            // Заявленная связь со строкой источника сверяется с найденными по содержанию.
            //
            // Что было неправильно. Сверка требовала совпадения именно с ПЕРВОЙ строкой списка
            // этого отпечатка. Одно и то же содержание законно встречается в источнике дважды —
            // именно так и выглядит запрещённое пересечение, когда экзаменационная строка
            // присутствует ещё и как учебная. Тогда вторая фактическая строка не могла подтвердить
            // свою собственную заявку никогда: ей отвечали первой строкой списка, и материал
            // отвергался ранним отказом о противоречии происхождения. Проверка пересечения
            // содержания, которая и должна была сработать, до своего отказа не доходила вовсе.
            //
            // Что стало. Заявка признаётся верной, если названная строка есть СРЕДИ строк этого
            // отпечатка. Семья у них одна — иначе выше уже был бы отказ о неоднозначности, — а роли
            // могут быть разные, и именно разность ролей и даёт запрещённое пересечение.
            var claimedRow = resolved.Rows.FirstOrDefault(row =>
                string.Equals(row.SourceRowId, actual.ClaimedSourceRowId, StringComparison.Ordinal));
            if (claimedRow is null)
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Строка '{actual.ExampleId}': заявлена связь со строкой источника " +
                    $"'{Describe(actual.ClaimedSourceRowId)}', но по содержанию это строка " +
                    $"'{resolved.Row.SourceRowId}' семьи '{resolved.Row.SourceFamilyId}'. " +
                    "Переименование семьи происхождения не меняет.",
                    LineageContradictsContentErrorCode);

            // Роль берётся из источника, а не из заявки фактической строки: иначе роль можно было
            // бы переобъявить и провести экзаменационную строку в обучение под видом учебной.
            var role = claimedRow.Role;

            // Отпечатки и семьи раскладываются по двум сторонам границы: обучение и отложенное.
            if (role == CapabilityMaterialRole.Train)
            {
                trainRowCount++;
                trainFamilies.Add(claimedRow.SourceFamilyId);
                trainExactDigests.Add(recomputedExact);
                trainNormalizedDigests.Add(recomputedNormalized);
            }
            else
            {
                // Промежуточная проверка и экзамен считаются по отдельности, но обе отложены.
                if (role == CapabilityMaterialRole.Dev) devRowCount++; else testRowCount++;
                heldOutFamilies.Add(claimedRow.SourceFamilyId);
                heldOutExactDigests.Add(recomputedExact);
                heldOutNormalizedDigests.Add(recomputedNormalized);
            }
        }

        // Запрещённое совпадение точных байтов между обучением и отложенным материалом.
        var sharedExact = trainExactDigests.Intersect(heldOutExactDigests, StringComparer.Ordinal).ToArray();
        if (sharedExact.Length > 0 &&
            (material.IntersectionPolicy.ForbidTrainTestContentOverlap ||
             material.IntersectionPolicy.ForbidTrainDevContentOverlap))
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                $"Обучение и отложенный материал содержат {sharedExact.Length} совпадающих по точным " +
                "байтам строк. Политика оценки это запрещает, материал отвергнут до обучения.",
                ForbiddenContentOverlapErrorCode);

        // То же по приведённому тексту — только если политика велит так сравнивать.
        if (material.IntersectionPolicy.CompareByNormalizedContent &&
            material.NormalizationPolicy != CapabilityMaterialNormalizationPolicy.ExactBytesOnly)
        {
            var sharedNormalized = trainNormalizedDigests
                .Intersect(heldOutNormalizedDigests, StringComparer.Ordinal)
                .ToArray();
            // Экзаменационная строка под новым именем, в нижнем регистре и без запятой — это та же
            // строка. Политика независимой оценки такое запрещает.
            if (sharedNormalized.Length > 0)
                return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                    $"Обучение и отложенный материал содержат {sharedNormalized.Length} строк, совпадающих " +
                    "после приведения текста. Это тот же материал под другим именем, материал отвергнут " +
                    "до обучения.",
                    ForbiddenContentOverlapErrorCode);
        }

        // Запрещённое пересечение СЕМЕЙ: строки разные, но происхождение одно.
        var sharedFamilies = trainFamilies.Intersect(heldOutFamilies, StringComparer.Ordinal).ToArray();
        if (sharedFamilies.Length > 0 &&
            (material.IntersectionPolicy.ForbidTrainTestFamilyOverlap ||
             material.IntersectionPolicy.ForbidTrainDevFamilyOverlap))
            return OperationResult<CapabilityMaterialAdmissionReceipt>.Fail(
                $"Семьи [{string.Join(", ", sharedFamilies)}] присутствуют и в обучении, и в отложенном " +
                "материале. Независимая оценка на таком разбиении невозможна, материал отвергнут до " +
                "обучения.",
                ForbiddenFamilyOverlapErrorCode);

        // Материал допущен. В расписке лежат только ПЕРЕСЧИТАННЫЕ величины и применённые правила.
        return OperationResult<CapabilityMaterialAdmissionReceipt>.Ok(
            new CapabilityMaterialAdmissionReceipt(
                material.ProfileId,
                CapabilityEvaluationMaterial.SchemaVersion,
                actualOwnedRows.Count,
                trainRowCount,
                devRowCount,
                testRowCount,
                trainFamilies.ToArray(),
                heldOutFamilies.ToArray(),
                material.NormalizationPolicy,
                material.IntersectionPolicy,
                // Совпадения после превращения текста в числа для модели здесь не измерялись.
                CapabilityMaterialAdmissionReceipt.NotMeasured,
                // Эта проверка видит только строки подготовителя. Сколько пар получит обучатель,
                // ей неизвестно, поэтому связи нет и число равно нулю.
                ConsumedPreferenceRowCount: 0,
                // Отпечатка потребляемого материала здесь нет: отсутствие связи, а не пустой текст.
                ConsumedMaterialSha256: null,
                // И расписка говорит об этом прямо, чтобы не выглядеть сильнее, чем есть.
                CapabilityMaterialAdmissionReceipt.ConsumedMaterialNotBound));
    }

    // SelectTrainingRows возвращает только те фактические строки, роль которых по ИСТОЧНИКУ —
    // обучение. Верные ответы отложенных строк остаются у оценщика и в обучение не попадают.
    public static OperationResult<IReadOnlyList<CapabilityActualMaterialRow>> SelectTrainingRows(
        CapabilityEvaluationMaterial material,
        IReadOnlyList<CapabilityActualMaterialRow> actualOwnedRows)
    {
        // Проверка происхождения обязательна до отбора: отбирать из непроверенного нечего.
        var admission = ValidateBeforeLearning(material, actualOwnedRows);
        // Отказ проверки передаётся вызывающему как есть, вместе с его кодом и причиной.
        if (!admission.IsSuccess)
            return admission.MapError<IReadOnlyList<CapabilityActualMaterialRow>>();

        // Роль берётся из строки источника, найденной по имени: заявке фактической строки о своей
        // роли здесь не верят.
        var roleBySourceRowId = material.SourceRows.ToDictionary(
            row => row.SourceRowId, row => row.Role, StringComparer.Ordinal);

        // Остаются только строки обучения, в прежнем порядке подачи.
        var trainingRows = actualOwnedRows
            .Where(actual =>
                roleBySourceRowId.TryGetValue(actual.ClaimedSourceRowId ?? string.Empty, out var role) &&
                role == CapabilityMaterialRole.Train)
            .ToArray();

        // Отобранные строки возвращаются как обычный результат операции.
        return OperationResult<IReadOnlyList<CapabilityActualMaterialRow>>.Ok(trainingRows);
    }

    // ResolvedSourceRow — исход поиска строки источника: найдена одна, найдено несколько семей,
    // либо не найдено ничего.
    private readonly record struct ResolvedSourceRow(
        CapabilitySourceRow? Row,
        bool Ambiguous,
        IReadOnlyList<string> Families,
        // Rows — ВСЕ строки источника с этим отпечатком. Их больше одной тогда, когда одно и то же
        // содержание законно встречается в источнике несколько раз: именно так выглядит
        // запрещённое пересечение обучения с отложенным материалом. Семья у них одна, иначе
        // исход был бы помечен неоднозначным; роли могут различаться.
        IReadOnlyList<CapabilitySourceRow> Rows);

    // ResolveSingleSourceRow находит ровно одну строку источника по отпечатку. Если отпечатку
    // соответствуют строки РАЗНЫХ семей, исход помечается неоднозначным: именно здесь опытный
    // проверяющий брал первую запись и терял различие.
    private static ResolvedSourceRow ResolveSingleSourceRow(
        Dictionary<string, List<CapabilitySourceRow>> index,
        string digest)
    {
        // Отпечатка нет в указателе: ничего не найдено, и это не неоднозначность.
        if (!index.TryGetValue(digest, out var rows) || rows.Count == 0)
            return new ResolvedSourceRow(null, false, [], []);

        // Разные семьи под одним отпечатком — это и есть неоднозначное происхождение.
        var families = rows
            .Select(row => row.SourceFamilyId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(family => family, StringComparer.Ordinal)
            .ToArray();
        // Больше одной семьи — отказ; одна семья — найденные строки этого отпечатка целиком.
        return families.Length > 1
            ? new ResolvedSourceRow(null, true, families, [])
            : new ResolvedSourceRow(rows[0], false, families, [.. rows]);
    }

    // GroupSourceRowsBy строит указатель от отпечатка к СПИСКУ строк источника. Список, а не одна
    // строка: это и позволяет отличить однозначное происхождение от неоднозначного.
    private static Dictionary<string, List<CapabilitySourceRow>> GroupSourceRowsBy(
        IReadOnlyList<CapabilitySourceRow> sourceRows,
        Func<CapabilitySourceRow, string> digestOf)
    {
        // Указатель сравнивает отпечатки точно по байтам их шестнадцатеричной записи.
        var index = new Dictionary<string, List<CapabilitySourceRow>>(StringComparer.Ordinal);
        // Каждая строка источника попадает в список своего отпечатка.
        foreach (var row in sourceRows)
        {
            // Отпечаток считается переданным способом: точные байты или приведённый текст.
            var digest = digestOf(row);
            // Список под этот отпечаток создаётся при первом появлении.
            if (!index.TryGetValue(digest, out var bucket))
            {
                bucket = [];
                index[digest] = bucket;
            }

            // Строка добавляется в список, а не заменяет предыдущую.
            bucket.Add(row);
        }

        // Готовый указатель возвращается вызывающему.
        return index;
    }

    // ComputeSha256 считает отпечаток точных байтов текста в кодировке UTF-8.
    private static string ComputeSha256(string value)
        // Пустой текст тоже имеет свой отпечаток: особого случая здесь нет.
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    // Normalize приводит текст по ВЫБРАННОЙ политике. Граница приведения названа в договоре:
    // нижний регистр и удаление знаков препинания — приведение короткого английского тестового
    // корпуса, а не доказательство смысловой эквивалентности кода, запросов или перевода.
    private static string Normalize(string value, CapabilityMaterialNormalizationPolicy policy)
    {
        // Без приведения текст сравнивается как есть.
        if (policy == CapabilityMaterialNormalizationPolicy.ExactBytesOnly)
            return value ?? string.Empty;

        // Выбранное приведение: нижний регистр, знаки препинания убраны, пробелы сжаты.
        var builder = new StringBuilder((value ?? string.Empty).Length);
        // Каждый символ рассматривается отдельно, чтобы правило было видно целиком.
        foreach (var character in value ?? string.Empty)
        {
            // Знаки препинания и служебные символы не попадают в приведённый текст.
            if (char.IsPunctuation(character) || char.IsSymbol(character))
                continue;

            // Любой пробельный символ становится одним обычным пробелом.
            builder.Append(char.IsWhiteSpace(character) ? ' ' : char.ToLowerInvariant(character));
        }

        // Повторные пробелы сжимаются, края обрезаются: это часть того же приведения.
        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    // DigestMatches сверяет предъявленный отпечаток с посчитанным. Сравнение без учёта регистра
    // шестнадцатеричной записи, потому что регистр записи отпечатка смысла не несёт.
    private static bool DigestMatches(string? presented, string recomputed)
        // Пустая заявка отпечатка совпадением не считается: это отсутствие доказательства.
        => !string.IsNullOrWhiteSpace(presented) &&
           string.Equals(presented.Trim(), recomputed, StringComparison.OrdinalIgnoreCase);

    // Describe делает пустое значение видимым в человеческой причине отказа.
    private static string Describe(string? value)
        // Пустое поле называется словами, а не показывается пустым местом.
        => string.IsNullOrWhiteSpace(value) ? "не заявлено" : value;

    // AdmittedTrainingMaterial — ЕДИНСТВЕННЫЙ носитель материала, который принимают входы обучения.
    //
    // Зачем он нужен, по-человечески. Проверка происхождения выше смотрит на строки подготовителя
    // оценки. Обучение же потребляет свои собственные наборы. Пока эти две вещи не связаны, можно
    // проверить одну копию, а обучать по другой, и проверка остаётся формальностью: подменить
    // содержание между проверкой и обучением было бы нечем помешать.
    //
    // Почему отнятие возможности, а не проверка вдогонку. Конструктор закрыт, фабрика одна, и
    // спросить подготовителя она решает САМА. Поэтому в сборке нет способа собрать этот носитель
    // мимо проверки, а входы обучения не принимают материал ни в каком другом виде: у них просто
    // нет для него довода. Это то же решение, что отнятие текста корпуса у словесной оценки.
    public sealed class AdmittedTrainingMaterial
    {
        // Заметка для прежнего разрешённого пути: материала оценки нет, независимая оценка не
        // выдаётся, обучение продолжается как раньше. Это НЕ молчаливое «прошло».
        public const string NoLineageEvaluationNote = "NOT_EVALUATED_NO_LINEAGE";

        // Заметка, когда происхождение проверено И потребляемый материал связан с проверенным.
        public const string AdmittedWithLineageNote = "ADMITTED_WITH_LINEAGE";

        // Пары предпочтений хранятся в закрытом поле: наружу уходит только копия, поэтому
        // переставить или подменить набор после допуска нельзя.
        private readonly LogicalDpoPreferenceExample[] _preferences;

        // Упорядоченный корпус ближайших соседей — тот же закрытый способ хранения. Корпус попал
        // в носитель затем, чтобы обучатель поиска читал РОВНО допущенные строки: прежде он
        // собирал свой корпус сам, из входов, которых допуск вообще не видел.
        private readonly GeneratedCapabilityKnnExample[] _retrievalCorpus;

        // Отпечаток тождества всего, чем владеет носитель: общий текст, вопрос, обе стороны и
        // УЧЕБНЫЙ ВЕКТОР каждой пары, плюс упорядоченный корпус. Сверяется при каждой выдаче.
        private readonly string _consumedIdentitySha256;

        private AdmittedTrainingMaterial(
            CapabilityMaterialAdmissionReceipt? receipt,
            string evaluationNote,
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText,
            GeneratedCapabilityKnnExample[] retrievalCorpus,
            string consumedIdentitySha256)
        {
            // Расписка допуска либо есть, либо её нет честно: null означает прежний путь без оценки.
            Receipt = receipt;
            // Заметка об оценке независимости называется словами всегда, в обоих случаях.
            EvaluationNote = evaluationNote;
            // Набор пар запоминается как собственная копия вызывающего набора.
            _preferences = preferences;
            // Общий учебный текст — обычная строка, то есть величина неизменяемая по природе.
            CombinedTrainingText = combinedTrainingText;
            // Корпус поиска запоминается собственной копией по той же причине, что и пары.
            _retrievalCorpus = retrievalCorpus;
            // Отпечаток тождества запоминается один раз, в момент допуска, и дальше только читается.
            _consumedIdentitySha256 = consumedIdentitySha256;
        }

        // Расписка допуска материала или её отсутствие на прежнем разрешённом пути.
        public CapabilityMaterialAdmissionReceipt? Receipt { get; }

        // Что сказано об оценке независимости этого обучения, словами.
        public string EvaluationNote { get; }

        // Общий учебный текст, допущенный к обучению.
        //
        // ГРАНИЦА ЭТОГО СВОЙСТВА, названная прямо. Это диагностическое чтение без сверки
        // тождества. Подменить строку нельзя по природе самого типа — строка неизменяема, —
        // поэтому отдельная сверка здесь ничего бы не доказала. Продуктовое обучение всё равно
        // читает текст сверяющим чтением ниже: тогда одна и та же выдача ловит и порчу вектора,
        // и порчу корпуса, даже если за текстом пришли первым.
        public string CombinedTrainingText { get; }

        // Сколько пар предпочтений несёт этот носитель. Нужно для человеческой диагностики.
        public int PreferenceRowCount => _preferences.Length;

        // Сколько строк упорядоченного корпуса поиска несёт носитель.
        public int RetrievalCorpusRowCount => _retrievalCorpus.Length;

        // ReadPreferencesForLearner выдаёт набор пар под один прогон оптимизатора и ПЕРЕД выдачей
        // сверяет тождество того, чем владеет, с отпечатком, записанным при допуске.
        //
        // Что было неправильно до этой правки. Выдача была простой копией массива с пояснением
        // «сами пары — записи, их значения не меняются». Для текста это верно, а для УЧЕБНОГО
        // ВЕКТОРА — нет: вектор лежит в записи обычным изменяемым массивом, и копия массива записей
        // отдавала тот же самый объект вектора. Вызывающий, оставивший пару себе на руках, менял
        // число в векторе ПОСЛЕ допуска, и обучение получало уже другие числа. Отпечаток
        // потребляемого материала этого не замечал, потому что считался только по тексту.
        //
        // Что стало. Вектор принадлежит носителю собственной копией, выдаётся тоже копией, а
        // отпечаток тождества покрывает и его. Поэтому подмена после допуска либо невозможна, либо
        // названа отказом до того, как обучение возьмёт материал.
        public OperationResult<LogicalDpoPreferenceExample[]> ReadPreferencesForLearner()
        {
            // Сначала тождество: читать изменённое после допуска содержание нельзя.
            var identity = VerifyConsumedIdentityUnchanged();
            // Отказ тождества передаётся вызывающему как есть, вместе с кодом и причиной.
            if (!identity.IsSuccess)
                return identity.MapError<LogicalDpoPreferenceExample[]>();

            // Выдаётся свежий массив, и у каждой пары — свежая копия учебного вектора.
            return OperationResult<LogicalDpoPreferenceExample[]>.Ok(
                [.. _preferences.Select(CopyWithOwnedVector)]);
        }

        // ReadCombinedTrainingTextForLearner выдаёт общий учебный текст той же сверяющей выдачей:
        // порча вектора или корпуса ловится и здесь, на самом раннем потребителе материала.
        public OperationResult<string> ReadCombinedTrainingTextForLearner()
        {
            // Та же сверка тождества, что и у набора пар: правило одно на всех потребителей.
            var identity = VerifyConsumedIdentityUnchanged();
            // Отказ тождества передаётся как есть.
            if (!identity.IsSuccess)
                return identity.MapError<string>();

            // Строка неизменяема, поэтому выдаётся как есть: копировать её незачем.
            return OperationResult<string>.Ok(CombinedTrainingText);
        }

        // ReadRetrievalCorpusForLearner выдаёт УПОРЯДОЧЕННЫЙ корпус ближайших соседей, тот самый,
        // который прошёл допуск. Порядок строк значим, поэтому выдаётся он, а не множество.
        public OperationResult<IReadOnlyList<GeneratedCapabilityKnnExample>> ReadRetrievalCorpusForLearner()
        {
            // Та же сверка тождества перед выдачей.
            var identity = VerifyConsumedIdentityUnchanged();
            // Отказ тождества передаётся как есть.
            if (!identity.IsSuccess)
                return identity.MapError<IReadOnlyList<GeneratedCapabilityKnnExample>>();

            // Строки корпуса неизменяемы по построению своего типа, и их наборы уже только для
            // чтения; выдаётся свежий массив, чтобы получатель не менял порядок у носителя.
            return OperationResult<IReadOnlyList<GeneratedCapabilityKnnExample>>.Ok(
                [.. _retrievalCorpus]);
        }

        // VerifyConsumedIdentityUnchanged пересчитывает отпечаток тождества из того, чем носитель
        // владеет СЕЙЧАС, и сверяет с отпечатком, записанным при допуске.
        private OperationResult<bool> VerifyConsumedIdentityUnchanged()
        {
            // Отпечаток считается заново по тому же однозначному правилу, что и при допуске.
            var recomputed = ComputeConsumedIdentityDigest(
                _preferences,
                CombinedTrainingText,
                _retrievalCorpus);
            // Расхождение означает, что содержание носителя после допуска изменилось.
            if (!string.Equals(recomputed, _consumedIdentitySha256, StringComparison.Ordinal))
                return OperationResult<bool>.Fail(
                    $"Содержание допущенного материала изменилось после допуска: отпечаток " +
                    $"тождества при допуске был '{_consumedIdentitySha256}', сейчас " +
                    $"'{recomputed}'. Обучение по изменённому материалу запрещено.",
                    ConsumedMaterialChangedErrorCode);

            // Тождество сохранено: материал можно выдавать обучению.
            return OperationResult<bool>.Ok(true);
        }

        // CopyWithOwnedVector возвращает ту же пару, но с СОБСТВЕННОЙ копией учебного вектора.
        private static LogicalDpoPreferenceExample CopyWithOwnedVector(
            LogicalDpoPreferenceExample preference)
            // Отсутствующий вектор остаётся отсутствующим: это другой факт, чем пустой вектор.
            => preference with
            {
                TeachingVector = preference.TeachingVector is null
                    ? null
                    : [.. preference.TeachingVector]
            };

        // Admit — единственный вход. Он сам спрашивает подготовителя, сам проверяет происхождение
        // и сам связывает проверенное с фактически потребляемым. Вызывающий не выбирает режим:
        // решение «есть материал оценки или нет» принимается здесь, а не снаружи.
        public static OperationResult<AdmittedTrainingMaterial> Admit(
            ICapabilityEvaluationMaterialProvider? provider,
            string runId,
            string domainTag,
            IReadOnlyList<LogicalDpoPreferenceExample> preferences,
            string combinedTrainingText,
            // retrievalCorpusExamples — упорядоченные строки будущего корпуса ближайших соседей.
            // Довод стоит последним и необязателен, поэтому прежние вызывающие работают без правок;
            // продуктовое обучение подаёт его всегда, и тогда корпус поиска читается из носителя.
            IReadOnlyList<GeneratedCapabilityKnnExample>? retrievalCorpusExamples = null)
        {
            // Пустой набор пар — это не материал без оценки, а отсутствие материала вовсе.
            if (preferences is null || preferences.Count == 0)
                return OperationResult<AdmittedTrainingMaterial>.Fail(
                    "Фактически потребляемые обучением пары предпочтений обязательны: обучать нечем.",
                    ConsumedMaterialRequiredErrorCode);

            // Пустой общий учебный текст так же означает отсутствие потребляемого материала.
            if (string.IsNullOrWhiteSpace(combinedTrainingText))
                return OperationResult<AdmittedTrainingMaterial>.Fail(
                    "Фактически потребляемый обучением общий текст обязателен: обучать нечем.",
                    ConsumedMaterialRequiredErrorCode);

            // Своя копия набора делается СРАЗУ, до любой проверки: дальше проверяется и уходит в
            // обучение ровно она, а не ссылка на набор вызывающего, который он может ещё изменить.
            // Учебный вектор каждой пары тоже копируется: он лежит в записи изменяемым массивом, и
            // копия одного массива записей оставляла бы вызывающему власть менять числа после
            // допуска.
            var ownedPreferences = preferences.Select(CopyWithOwnedVector).ToArray();

            // Корпус поиска берётся собственной копией строк. Строки этого типа неизменяемы по
            // построению, но копия защищает ещё и ПОРЯДОК, а он у корпуса значим.
            var ownedCorpus = (retrievalCorpusExamples ?? [])
                .Select(example => example.Snapshot())
                .ToArray();

            // Отпечаток тождества считается один раз здесь и дальше служит образцом для сверки при
            // каждой выдаче материала обучению.
            var identityDigest = ComputeConsumedIdentityDigest(
                ownedPreferences,
                combinedTrainingText,
                ownedCorpus);

            // Подготовителя нет — прежний разрешённый путь целиком, с честной заметкой.
            if (provider is null)
                return OperationResult<AdmittedTrainingMaterial>.Ok(new AdmittedTrainingMaterial(
                    receipt: null,
                    NoLineageEvaluationNote,
                    ownedPreferences,
                    combinedTrainingText,
                    ownedCorpus,
                    identityDigest));

            // Подготовитель отвечает своим обычным результатом операции.
            var submission = provider.TryProvide(runId, domainTag);
            // Его отказ передаётся наружу как есть: причина и код принадлежат ему.
            if (!submission.IsSuccess)
                return submission.MapError<AdmittedTrainingMaterial>();

            // Подготовитель есть, но материала для этого запуска у него нет: снова прежний путь.
            if (submission.Value is null)
                return OperationResult<AdmittedTrainingMaterial>.Ok(new AdmittedTrainingMaterial(
                    receipt: null,
                    NoLineageEvaluationNote,
                    ownedPreferences,
                    combinedTrainingText,
                    ownedCorpus,
                    identityDigest));

            // Материал есть: его фактические строки проверяются заново по собственным байтам.
            var material = submission.Value.Material;
            var admission = ValidateBeforeLearning(material, submission.Value.ActualOwnedRows);
            // Отказ происхождения прекращает обучение здесь же, до любого разрешения на изменение.
            if (!admission.IsSuccess)
                return admission.MapError<AdmittedTrainingMaterial>();

            // Связь проверенного материала с фактически потребляемым. Отрицательная половина:
            // содержания отложенных строк в том, что обучают, быть не должно.
            var binding = RefuseHeldOutContentInConsumedMaterial(
                material,
                ownedPreferences,
                combinedTrainingText,
                ownedCorpus);
            // Найденная утечка отложенного содержания — названный отказ до обучения.
            if (!binding.IsSuccess)
                return binding.MapError<AdmittedTrainingMaterial>();

            // Положительная половина той же связи: ВСЁ разрешённое обучение обязано дойти до
            // обучателя, а не часть его.
            var completeness = RequireEveryAdmittedTrainRowInConsumedMaterial(
                material,
                submission.Value.ActualOwnedRows,
                ownedPreferences,
                combinedTrainingText,
                ownedCorpus);
            // Потерянная учебная строка — названный отказ до обучения.
            if (!completeness.IsSuccess)
                return completeness.MapError<AdmittedTrainingMaterial>();

            // Расписка дополняется тем, что было ФАКТИЧЕСКИ связано: число пар, отпечаток
            // потребляемых байтов, отпечаток тождества, размер корпуса и название связи.
            var boundReceipt = admission.Value! with
            {
                ConsumedPreferenceRowCount = ownedPreferences.Length,
                ConsumedMaterialSha256 = ComputeConsumedDigest(ownedPreferences, combinedTrainingText),
                ConsumedMaterialBinding =
                    CapabilityMaterialAdmissionReceipt.ConsumedTrainCompleteAndHeldOutAbsent,
                ConsumedIdentitySha256 = identityDigest,
                ConsumedRetrievalCorpusRowCount = ownedCorpus.Length
            };

            // Носитель собран: дальше обучение получает ровно его.
            return OperationResult<AdmittedTrainingMaterial>.Ok(new AdmittedTrainingMaterial(
                boundReceipt,
                AdmittedWithLineageNote,
                ownedPreferences,
                combinedTrainingText,
                ownedCorpus,
                identityDigest));
        }

        // RequireEveryAdmittedTrainRowInConsumedMaterial требует, чтобы содержание КАЖДОЙ
        // допущенной учебной строки действительно присутствовало в том, что обучают.
        //
        // Зачем это нужно отдельно от отрицательной проверки. Отрицательная проверка доказывает,
        // что в обучении нет отложенного материала. Она ничего не говорит о потере: подготовитель
        // мог честно предъявить шесть разрешённых учебных строк, а до обучателя дошли три. Тогда
        // обучение идёт на другом, более бедном материале, чем тот, который допущен и назван в
        // расписке, и расписка начинает описывать не то обучение, которое состоялось.
        //
        // Почему роль берётся из ИСТОЧНИКА, а не из заявки фактической строки. По той же причине,
        // что и в самой проверке происхождения: иначе подготовитель мог бы переобъявить роль
        // потерянной строки и тем самым вывести её из-под этого требования.
        private static OperationResult<bool> RequireEveryAdmittedTrainRowInConsumedMaterial(
            CapabilityEvaluationMaterial material,
            IReadOnlyList<CapabilityActualMaterialRow> actualOwnedRows,
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText,
            GeneratedCapabilityKnnExample[] retrievalCorpus)
        {
            // Роль каждой строки источника берётся по её имени: заявке фактической строки не верим.
            var roleBySourceRowId = material.SourceRows.ToDictionary(
                row => row.SourceRowId, row => row.Role, StringComparer.Ordinal);

            // Всё, что обучают, собирается одним списком названных кусков — тем же, что и у
            // отрицательной проверки, чтобы «присутствует» и «отсутствует» считались одинаково.
            var consumed = CollectConsumedPieces(preferences, combinedTrainingText, retrievalCorpus);

            foreach (var actual in actualOwnedRows)
            {
                // Строка не учебная — требовать её присутствия в обучении нельзя и не нужно.
                if (!roleBySourceRowId.TryGetValue(actual.ClaimedSourceRowId ?? string.Empty, out var role) ||
                    role != CapabilityMaterialRole.Train)
                    continue;

                // Пустым содержанием присутствие не доказывается и не опровергается: пустая строка
                // входит в любой текст, поэтому такая проверка была бы всегда успешной впустую.
                if (string.IsNullOrWhiteSpace(actual.Content))
                    continue;

                // Содержание учебной строки ищется среди кусков по точным байтам.
                if (consumed.Any(piece => piece.Text.Contains(actual.Content, StringComparison.Ordinal)))
                    continue;

                // Учебная строка допущена, но до обучателя не дошла: это и есть потеря.
                return OperationResult<bool>.Fail(
                    $"Допущенная учебная строка '{actual.ExampleId}' (строка источника " +
                    $"'{actual.ClaimedSourceRowId}') в том, что обучают, отсутствует. Обучение " +
                    "получило бы не весь разрешённый материал, поэтому материал отвергнут до " +
                    "обучения.",
                    ConsumedTrainIncompleteErrorCode);
            }

            // Все допущенные учебные строки дошли до обучателя.
            return OperationResult<bool>.Ok(true);
        }

        // CollectConsumedPieces собирает ВСЁ, что обучают, названными человеческими кусками: общий
        // учебный текст, вопрос и обе стороны каждой пары, учебный текст и ответ каждой строки
        // упорядоченного корпуса поиска. Один владелец этого списка на обе половины связи.
        private static List<(string Where, string Text)> CollectConsumedPieces(
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText,
            GeneratedCapabilityKnnExample[] retrievalCorpus)
        {
            // Общий учебный текст идёт первым куском: он уходит в сборку учебного стека.
            var consumed = new List<(string Where, string Text)>
            {
                ("общий учебный текст", combinedTrainingText)
            };
            for (var index = 0; index < preferences.Length; index++)
            {
                // Номер пары называется от единицы: так его читает человек.
                var pair = preferences[index];
                consumed.Add(($"вопрос пары {index + 1}", pair.Prompt ?? string.Empty));
                consumed.Add(($"правильный ответ пары {index + 1}", pair.Chosen ?? string.Empty));
                consumed.Add(($"неправильный ответ пары {index + 1}", pair.Rejected ?? string.Empty));
            }

            for (var index = 0; index < retrievalCorpus.Length; index++)
            {
                // Корпус поиска — такой же фактически потребляемый материал: по нему обучаемая
                // единица отвечает, поэтому экзаменационная строка в нём — такая же утечка.
                var example = retrievalCorpus[index];
                consumed.Add(($"учебный текст строки {index + 1} корпуса поиска", example.TrainingText ?? string.Empty));
                consumed.Add(($"ответ строки {index + 1} корпуса поиска", example.OutputText ?? string.Empty));
            }

            // Готовый список кусков возвращается вызывающему.
            return consumed;
        }

        // RefuseHeldOutContentInConsumedMaterial ищет содержание отложенных строк в том, что
        // обучают.
        //
        // Почему иглы берутся из ЗАМОРОЖЕННОГО источника, а не из предъявленных подготовителем
        // строк. Замороженный источник — authority о том, что отложено. Если бы отложенные иглы
        // брались из предъявленного списка, подготовитель мог бы просто не предъявлять
        // экзаменационную строку и тем самым разрешить её утечку в обучение.
        private static OperationResult<bool> RefuseHeldOutContentInConsumedMaterial(
            CapabilityEvaluationMaterial material,
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText,
            GeneratedCapabilityKnnExample[] retrievalCorpus)
        {
            // Сравнивать ли ещё и по приведённому тексту: решает политика материала.
            var compareNormalized = material.IntersectionPolicy.CompareByNormalizedContent &&
                material.NormalizationPolicy != CapabilityMaterialNormalizationPolicy.ExactBytesOnly;

            // Что обучают: общий учебный текст, обе стороны каждой пары вместе с её вопросом и
            // строки упорядоченного корпуса поиска. Каждый кусок назван человеческим именем, чтобы
            // отказ сказал, ГДЕ нашлась утечка.
            var consumed = CollectConsumedPieces(preferences, combinedTrainingText, retrievalCorpus);

            // Приведённые куски считаются один раз на весь поиск, а не для каждой иглы.
            // Список объявлен отдельной строкой, а не условным выражением: тип элемента назван
            // явно, и пустой случай не зависит от вывода общего типа ветвей.
            var normalizedConsumed = new List<(string Where, string Text)>();
            if (compareNormalized)
            {
                foreach (var piece in consumed)
                {
                    // Каждый кусок приводится тем же правилом, что и сами иглы ниже.
                    normalizedConsumed.Add((piece.Where, Normalize(piece.Text, material.NormalizationPolicy)));
                }
            }

            foreach (var sourceRow in material.SourceRows)
            {
                // Обучающие строки источника утечкой не являются: они и должны быть в обучении.
                if (sourceRow.Role == CapabilityMaterialRole.Train)
                    continue;

                // Пустая игла совпадает с любым текстом, поэтому поиском по ней ничего доказать
                // нельзя: это дефект материала, а не утечка, и обучение им запрещать неверно.
                if (string.IsNullOrWhiteSpace(sourceRow.Content))
                    continue;

                foreach (var piece in consumed)
                {
                    // Точные байты отложенной строки внутри того, что обучают, — утечка.
                    if (piece.Text.Contains(sourceRow.Content, StringComparison.Ordinal))
                        return OperationResult<bool>.Fail(
                            $"В том, что обучают, найдено содержание отложенной строки " +
                            $"'{sourceRow.SourceRowId}' семьи '{sourceRow.SourceFamilyId}' " +
                            $"(место: {piece.Where}, совпадение по точным байтам). " +
                            "Материал отвергнут до обучения.",
                            ConsumedHeldOutContentErrorCode);
                }

                // Та же игла по приведённому тексту — только если политика велит так сравнивать.
                if (!compareNormalized)
                    continue;

                // Игла приводится тем же правилом, что и куски: иначе сравнение было бы неравным.
                var normalizedNeedle = Normalize(sourceRow.Content, material.NormalizationPolicy);
                // Приведение могло съесть иглу целиком; пустой иглой искать нельзя по той же причине.
                if (string.IsNullOrWhiteSpace(normalizedNeedle))
                    continue;

                foreach (var piece in normalizedConsumed)
                {
                    // Экзаменационная строка в нижнем регистре и без запятой — та же строка.
                    if (piece.Text.Contains(normalizedNeedle, StringComparison.Ordinal))
                        return OperationResult<bool>.Fail(
                            $"В том, что обучают, найдено содержание отложенной строки " +
                            $"'{sourceRow.SourceRowId}' семьи '{sourceRow.SourceFamilyId}' " +
                            $"(место: {piece.Where}, совпадение после приведения текста). " +
                            "Материал отвергнут до обучения.",
                            ConsumedHeldOutContentErrorCode);
                }
            }

            // Отложенного содержания в том, что обучают, не найдено.
            return OperationResult<bool>.Ok(true);
        }

        // ComputeConsumedDigest считает отпечаток ФАКТИЧЕСКИ потребляемых обучением байтов, чтобы
        // задним числом было видно, что обучали именно то, что допущено.
        //
        // Что было неправильно. Материал собирался в один текст через перевод строки, и
        // разделитель НЕ экранировался. Перевод строки внутри вопроса или внутри предпочитаемой
        // стороны — обычное содержание, а не подделка, поэтому два РАЗНЫХ набора пар давали один
        // и тот же текст и один отпечаток: вопрос с переносом строки превращался в два поля, а
        // границы следующих полей сдвигались. Отсутствующее значение при этом приводилось к
        // пустой строке, то есть «стороны нет» и «сторона пустая» тоже становились одним
        // материалом. Отпечаток, который не различает разный материал, перестаёт быть
        // доказательством: по нему нельзя сказать, что обучали именно допущенное.
        //
        // Что стало. Поля пишутся общим однозначным правилом: длина каждого поля объявляется до
        // его байтов, отсутствующее значение несёт свою метку, число пар входит в поток. Правило
        // одно на этого сторожа и на словесную статистику обученного корпуса.
        //
        // Граница этого отпечатка, названная прямо. Он ТЕКСТОВЫЙ: в него входят общий учебный
        // текст, вопрос и обе стороны каждой пары. Ни учебный вектор, ни происхождение строки,
        // ни иные поля, влияющие на обновитель весов, в него не входят, поэтому полным
        // тождеством ВСЕХ входов обучения он не является и выдаваться за него не должен. Имя
        // поля расписки оставлено прежним намеренно: это опубликованный член договора расписки,
        // и переименование сломало бы его читателей; граница названа здесь и в справочнике.
        private static string ComputeConsumedDigest(
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText)
        {
            // Поток отпечатка: версия правила кодирования и назначение потока записываются сразу.
            var writer = new UnambiguousFieldEncoding.Writer("capability-consumed-training-text");
            // Общий учебный текст идёт первым полем, как и в самом обучении.
            writer.WriteField(combinedTrainingText);
            // Число пар входит в поток: набор из двух пар не даст поток набора из одной.
            writer.WriteCount(preferences.Length);
            foreach (var pair in preferences)
            {
                // Вопрос и обе стороны пары записываются своими полями с объявленной длиной;
                // отсутствующая сторона несёт свою метку и не сливается с пустой.
                writer.WriteField(pair.Prompt)    // Вопрос пары.
                      .WriteField(pair.Chosen)    // Предпочитаемая сторона.
                      .WriteField(pair.Rejected); // Отвергаемая сторона.
            }

            // Отпечаток считается один раз по всему накопленному потоку.
            return writer.Digest();
        }

        // ComputeConsumedIdentityDigest считает отпечаток ТОЖДЕСТВА всего, чем владеет носитель.
        //
        // Чем он отличается от текстового отпечатка выше и зачем нужен отдельно. Текстовый
        // отпечаток — опубликованный член расписки, и его байты намеренно оставлены прежними:
        // по нему читатели сверяют тот самый материал, который расписка называет. Но он покрывает
        // только текст, а обучение потребляет ещё две вещи, которые текст не описывает: учебный
        // ВЕКТОР каждой пары и упорядоченный КОРПУС поиска. Пока отпечаток их не покрывал,
        // изменение числа в векторе после допуска было для расписки невидимым, а корпус в неё не
        // входил вовсе. Поэтому тождество считается своим отпечатком, покрывающим всё три части.
        //
        // Правило кодирования то же самое и у того же единственного владельца: длина поля перед
        // его байтами, отдельная метка отсутствующего значения, число записей в самом потоке.
        // Назначение потока названо своим именем, поэтому отпечаток тождества и текстовый
        // отпечаток не совпадают даже на одном и том же материале.
        private static string ComputeConsumedIdentityDigest(
            LogicalDpoPreferenceExample[] preferences,
            string combinedTrainingText,
            GeneratedCapabilityKnnExample[] retrievalCorpus)
        {
            // Поток тождества опознаётся своим назначением.
            var writer = new UnambiguousFieldEncoding.Writer("capability-consumed-training-identity-v1");
            // Общий учебный текст идёт первым полем, как и в самом обучении.
            writer.WriteField(combinedTrainingText);
            // Число пар входит в поток: набор из двух пар не даст поток набора из одной.
            writer.WriteCount(preferences.Length);
            foreach (var pair in preferences)
            {
                // Текстовые поля пары записываются своими полями с объявленной длиной.
                writer.WriteField(pair.Prompt)     // Вопрос пары.
                      .WriteField(pair.Chosen)     // Предпочитаемая сторона.
                      .WriteField(pair.Rejected);  // Отвергаемая сторона.
                // Отсутствующий вектор несёт метку отсутствия и не сливается с пустым вектором.
                if (pair.TeachingVector is null)
                {
                    writer.WriteField(null);
                    continue;
                }

                // Длина вектора входит в поток до его чисел: короткий и длинный вектор различимы.
                writer.WriteCount(pair.TeachingVector.Length);
                foreach (var value in pair.TeachingVector)
                {
                    // Каждое число записывается своими ТОЧНЫМИ битами: округление до текста
                    // сделало бы два разных числа одним полем и вернуло бы ту же неоднозначность.
                    writer.WriteCount(BitConverter.SingleToInt32Bits(value));
                }
            }

            // Число строк корпуса входит в поток: потеря строки меняет отпечаток.
            writer.WriteCount(retrievalCorpus.Length);
            foreach (var example in retrievalCorpus)
            {
                // Имя строки, её учебный текст и её ответ — три отдельных поля.
                writer.WriteField(example.ExampleId)
                      .WriteField(example.TrainingText)
                      .WriteField(example.OutputText);
                // Признаки запроса: их число и порядок значимы, поэтому входят оба.
                writer.WriteCount(example.PredicateIds.Count);
                foreach (var predicateId in example.PredicateIds)
                    writer.WriteField(predicateId);
                // Доменные метки — тем же правилом, тоже с числом и в порядке.
                writer.WriteCount(example.DomainTags.Count);
                foreach (var domainTag in example.DomainTags)
                    writer.WriteField(domainTag);
            }

            // Отпечаток считается один раз по всему накопленному потоку.
            return writer.Digest();
        }
    }
}
