// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// Одна допущенная пара предпочтений вместе со своим номером из долговечной
/// транзакции. Номер нужен потому, что сама пара (DPO — Direct Preference
/// Optimization, прямое сравнение предпочтений) своего номера не хранит, а
/// происхождение исправленного урока обязано называть именно ту пару, которая
/// исправление разрешила.
/// </summary>
internal sealed record DpoSupervisedCorrectionCandidatePair(
    string PairId,
    DpoPair Pair);

/// <summary>
/// Запись одной замены: где стоял урок, какой у него был источник, какой
/// источник получился, какая пара дала исправление и каковы отпечатки обоих
/// текстов ответа. Обе личности источника сохраняются, поэтому по записи видно
/// и что учили раньше, и чем это заменили.
/// </summary>
internal sealed record DpoSupervisedCorrectionBinding(
    int ExampleOrdinal,
    string OriginalSourceId,
    string CorrectedSourceId,
    string TransactionPairId,
    string PairBatchSha256,
    string OriginalResponseSha256,
    string CorrectedResponseSha256);

/// <summary>
/// Неизменяемая проекция учебного набора: сами учебные строки после замены,
/// отпечаток набора до замены, упорядоченные записи замен и родословная
/// источников, в которую входят и прежние, и производные личности.
/// </summary>
internal sealed record ProjectedSupervisedBatch(
    IReadOnlyList<TrainingExample> Examples,
    string OriginalBatchSha256,
    IReadOnlyList<DpoSupervisedCorrectionBinding> Corrections,
    TrainingSourceManifest TrainingSources);

/// <summary>
/// Заменяет в учебном наборе обучения с учителем (SFT — supervised
/// fine-tuning, дообучение по готовым парам «запрос → ответ») ровно тот ответ,
/// который допущенная пара предпочтений назвала отвергнутым, на выбранный
/// текст той же пары.
///
/// Зачем это нужно продукту. Порядок обучения такой: сначала один шаг прямого
/// сравнения предпочтений уводит модель от отвергнутого текста, затем уроки
/// обучения с учителем закрепляют правильные ответы. Если в том же наборе
/// уроков ответом всё ещё стоит тот самый отвергнутый текст, последний шаг
/// учит модель именно ему и гасит только что полученное исправление. Опыты
/// это измерили: правильных знаков становилось меньше, а знак предпочтения
/// переворачивался на отрицательный.
///
/// Чем это НЕ является. Это не обучаемый выбиратель и не смысловой поиск
/// похожих текстов: сравнение строго дословное и побайтовое, без обрезки
/// пробелов, без приведения регистра и без разбора синтаксиса. Это адресный
/// ремонт источника урока внутри уже существующего пути обучения, поэтому
/// новым уровнем архитектуры он не объявляется. Решение о допуске пары
/// по-прежнему принимает прежняя власть допуска: пока пара не допущена,
/// заменить урок нельзя.
/// </summary>
internal static class DpoSupervisedCorrectionProjection
{
    /// <summary>Имя версии правила замены; им помечается происхождение.</summary>
    internal const string Version = "dpo-sft-correction-v1";

    /// <summary>Имя версии отпечатка набора пар, давших исправления.</summary>
    internal const string PairBatchDigestVersion =
        "xiigen.dpo-supervised-correction-pair-batch.v1";

    /// <summary>
    /// Отказ, когда один и тот же устаревший урок исправляют две и более пары.
    /// Выбор «по времени» или «по порядку случайной коллекции» запрещён, потому
    /// что он молча выберет чужой правильный ответ.
    /// </summary>
    internal const string AmbiguousErrorCode =
        "DPO_SUPERVISED_CORRECTION_AMBIGUOUS";

    /// <summary>
    /// Отказ, когда у допущенной пары нет непустого номера из транзакции:
    /// без него происхождение исправленного урока назвать нечем.
    /// </summary>
    internal const string PairIdentityRequiredErrorCode =
        "DPO_SUPERVISED_CORRECTION_PAIR_IDENTITY_REQUIRED";

    /// <summary>
    /// Отказ на ошибку вызова: проекцию позвали без учебных строк или без
    /// допущенных пар. Это не учебный случай, а неверная сборка вызова.
    /// </summary>
    internal const string InvalidRequestErrorCode =
        "DPO_SUPERVISED_CORRECTION_REQUEST_INVALID";

    /// <summary>
    /// Быстрый ответ на вопрос «есть ли вообще что исправлять». Нужен для того,
    /// чтобы на обычном наборе без столкновения не выполнялось ни одного
    /// лишнего чтения транзакции и ни одного лишнего вычисления отпечатка:
    /// поведение такого набора остаётся прежним до байта.
    /// </summary>
    internal static bool HasCorrectionCandidate(
        string domain,
        IReadOnlyList<TrainingExample>? examples,
        IReadOnlyList<DpoPair>? admittedPairs)
    {
        // Без учебных строк или без допущенных пар исправлять нечего.
        if (examples is null || admittedPairs is null)
            return false;
        // Проверяется каждая учебная строка по порядку.
        foreach (var example in examples)
        {
            // Пустая строка набора — не кандидат; её отклонит обычная проверка.
            if (example is null)
                continue;
            // Достаточно одной пары, чей отвергнутый текст дословно равен ответу.
            foreach (var pair in admittedPairs)
            {
                // Пара обязана быть исправляющей и относиться к этому домену.
                if (IsCorrectingPair(pair, domain)
                    && MatchesStaleLesson(example, pair, domain))
                {
                    // Один найденный кандидат уже включает полный разбор.
                    return true;
                }
            }
        }

        // Ни одна учебная строка не повторяет отвергнутый текст.
        return false;
    }

    /// <summary>
    /// Строит проекцию: для каждой учебной строки в исходном порядке ищет ровно
    /// одну допущенную пару, чей отвергнутый текст дословно равен ответу урока,
    /// и заменяет только ответ и производную личность источника. Запрос, домен,
    /// вес, время снятия, позиция и число уроков сохраняются.
    /// </summary>
    internal static OperationResult<ProjectedSupervisedBatch> Project(
        string domain,
        IReadOnlyList<TrainingExample>? examples,
        IReadOnlyList<DpoSupervisedCorrectionCandidatePair>? admittedPairs,
        string originalBatchSha256)
    {
        // Имя домена обязано быть непустым: по нему сверяются пара и урок.
        if (string.IsNullOrWhiteSpace(domain))
            return InvalidRequest();
        // Без учебных строк проекция бессмысленна.
        if (examples is null || examples.Count == 0)
            return InvalidRequest();
        // Без допущенных пар исправлять нечем.
        if (admittedPairs is null || admittedPairs.Count == 0)
            return InvalidRequest();
        // Отпечаток исходного набора обязан быть уже вычислен вызывающим.
        if (string.IsNullOrWhiteSpace(originalBatchSha256))
            return InvalidRequest();
        // У каждой допущенной пары обязан быть непустой номер из транзакции.
        foreach (var candidate in admittedPairs)
        {
            // Пустая ссылка на пару или пустой номер — отказ до любой замены.
            if (candidate is null
                || candidate.Pair is null
                || string.IsNullOrWhiteSpace(candidate.PairId))
            {
                return OperationResult<ProjectedSupervisedBatch>.Fail(
                    "Каждая допущенная пара предпочтений обязана иметь непустой номер из долговечной транзакции.",
                    PairIdentityRequiredErrorCode);
            }
        }

        // Уникальность прежних личностей источника проверяется штатным
        // построителем происхождения: его отказы и их коды не меняются.
        var originalManifest = TrainingSourceProvenance.BuildManifest(
            examples.Select(static example => example?.SourceId));
        if (!originalManifest.IsSuccess || originalManifest.Value is null)
            return originalManifest.MapError<ProjectedSupervisedBatch>();

        // Отпечаток набора пар входит в происхождение каждой замены.
        var pairBatchSha256 = ComputeAdmittedPairBatchSha256(admittedPairs);
        // Новый список уроков строится копией: исходный набор не меняется.
        var projectedExamples = new List<TrainingExample>(examples.Count);
        // Записи замен складываются по возрастанию позиции урока.
        var corrections = new List<DpoSupervisedCorrectionBinding>();
        // Производные личности источника собираются отдельно для родословной.
        var correctedSourceIds = new List<string>();
        // Разбор идёт по позициям, потому что позиция входит в запись замены.
        for (var ordinal = 0; ordinal < examples.Count; ordinal++)
        {
            // Текущая учебная строка в исходном порядке.
            var example = examples[ordinal];
            // Пустая строка набора проходит дальше без изменения.
            if (example is null)
            {
                // Отклонять её — дело обычной проверки запроса, не проекции.
                projectedExamples.Add(example!);
                continue;
            }

            // Подходящие пары собираются все: несколько — это отказ, не выбор.
            DpoSupervisedCorrectionCandidatePair? selected = null;
            // Счётчик подходящих пар отвечает на вопрос об однозначности.
            var matchCount = 0;
            // Перебираются все допущенные пары этого домена.
            foreach (var candidate in admittedPairs)
            {
                // Пара обязана быть исправляющей и дословно совпасть с уроком.
                if (!IsCorrectingPair(candidate.Pair, domain)
                    || !MatchesStaleLesson(example, candidate.Pair, domain))
                {
                    // Несовпадение — следующая пара.
                    continue;
                }

                // Совпадение найдено: оно учитывается и запоминается.
                matchCount++;
                // Первое совпадение становится предполагаемым исправлением.
                selected ??= candidate;
            }

            // Две и более подходящие пары — явный отказ до любой мутации модели.
            if (matchCount > 1)
            {
                return OperationResult<ProjectedSupervisedBatch>.Fail(
                    "Устаревший учебный ответ исправляют несколько допущенных пар предпочтений; выбор одной из них не разрешён.",
                    AmbiguousErrorCode);
            }

            // Нет совпадения — урок передаётся дальше без изменения.
            if (selected is null)
            {
                // Тот же объект урока: ни ответ, ни источник не трогаются.
                projectedExamples.Add(example);
                continue;
            }

            // Производная личность источника строится штатным построителем:
            // происхождение называет версию правила, прежний источник, номер
            // пары и отпечаток набора пар, а содержимым берутся точные байты
            // выбранного ответа в кодировке UTF-8.
            var correctedSourceId = TrainingSourceProvenance.CreateSourceId(
                string.Concat(
                    Version,
                    ":",
                    example.SourceId,
                    ":",
                    selected.PairId,
                    ":",
                    pairBatchSha256),
                selected.Pair.Chosen);
            // Урок пересобирается с одним изменённым ответом и новым источником.
            projectedExamples.Add(new TrainingExample(
                example.Prompt,
                selected.Pair.Chosen,
                example.Domain,
                example.Weight,
                example.CapturedAt,
                correctedSourceId));
            // Запись замены сохраняет обе личности источника и оба отпечатка.
            corrections.Add(new DpoSupervisedCorrectionBinding(
                ordinal,
                example.SourceId!,
                correctedSourceId,
                selected.PairId,
                pairBatchSha256,
                Sha256Utf8(example.Response),
                Sha256Utf8(selected.Pair.Chosen)));
            // Производная личность пойдёт в родословную источников.
            correctedSourceIds.Add(correctedSourceId);
        }

        // Без замен родословной остаётся проверенный исходный набор личностей.
        var trainingSources = originalManifest.Value;
        if (corrections.Count > 0)
        {
            // Производные личности проверяются тем же построителем отдельно.
            var correctedManifest = TrainingSourceProvenance.BuildManifest(
                correctedSourceIds);
            if (!correctedManifest.IsSuccess || correctedManifest.Value is null)
                return correctedManifest.MapError<ProjectedSupervisedBatch>();
            // Объединение уже проверенных прежних и производных личностей:
            // это родословная, а не новые учебные строки.
            var lineage = TrainingSourceProvenance.Combine(
                originalManifest.Value,
                correctedManifest.Value);
            if (!lineage.IsSuccess || lineage.Value is null)
                return lineage.MapError<ProjectedSupervisedBatch>();
            // Родословная становится происхождением этого учебного прохода.
            trainingSources = lineage.Value;
        }

        // Результат неизменяем: списки закрываются только для чтения.
        return OperationResult<ProjectedSupervisedBatch>.Ok(
            new ProjectedSupervisedBatch(
                Array.AsReadOnly(projectedExamples.ToArray()),
                originalBatchSha256,
                Array.AsReadOnly(corrections.ToArray()),
                trainingSources));
    }

    /// <summary>
    /// Отпечаток набора допущенных пар, давших право на исправление. В него
    /// входят имя версии, число пар и по каждой паре её номер, запрос,
    /// выбранный текст, отвергнутый текст и домен — в исходном порядке.
    /// </summary>
    internal static string ComputeAdmittedPairBatchSha256(
        IReadOnlyList<DpoSupervisedCorrectionCandidatePair> admittedPairs)
    {
        // Отпечаток считается потоком: весь набор в память второй раз не копируется.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        // Имя версии отделяет этот отпечаток от любого другого набора полей.
        AppendText(hash, PairBatchDigestVersion);
        // Число пар отделяет короткий набор от начала длинного.
        AppendInt32(hash, admittedPairs.Count);
        foreach (var candidate in admittedPairs)
        {
            // Номер пары из транзакции: он связывает исправление с её записью.
            AppendText(hash, candidate.PairId);
            // Запрос пары.
            AppendText(hash, candidate.Pair.Prompt);
            // Выбранный текст: именно он становится новым ответом урока.
            AppendText(hash, candidate.Pair.Chosen);
            // Отвергнутый текст: именно его исправление убирает из обучения.
            AppendText(hash, candidate.Pair.Rejected);
            // Домен пары.
            AppendText(hash, candidate.Pair.Domain);
        }

        // Отпечаток приводится к строчным шестнадцатеричным знакам.
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    // Исправляющей считается непустая допущенная пара своего домена, у которой
    // выбранный текст непуст и дословно отличается от отвергнутого. Пара, где
    // обе стороны совпали, ничего не исправляет и кандидатом не становится.
    private static bool IsCorrectingPair(DpoPair pair, string domain)
        => pair is not null
           && StringComparer.Ordinal.Equals(pair.Domain, domain)
           && !string.IsNullOrEmpty(pair.Chosen)
           && !string.IsNullOrEmpty(pair.Rejected)
           && !StringComparer.Ordinal.Equals(pair.Chosen, pair.Rejected);

    // Устаревшим считается урок того же домена, у которого запрос дословно
    // равен запросу пары, а ответ дословно равен её отвергнутому тексту.
    // Сравнение только побайтовое: ни обрезки, ни регистра, ни похожести.
    private static bool MatchesStaleLesson(
        TrainingExample example,
        DpoPair pair,
        string domain)
        => StringComparer.Ordinal.Equals(example.Domain, domain)
           && StringComparer.Ordinal.Equals(example.Prompt, pair.Prompt)
           && StringComparer.Ordinal.Equals(example.Response, pair.Rejected);

    // Отказ на неверную сборку вызова проекции.
    private static OperationResult<ProjectedSupervisedBatch> InvalidRequest()
        => OperationResult<ProjectedSupervisedBatch>.Fail(
            "Для проекции исправления нужны домен, непустой учебный набор, непустой набор допущенных пар и отпечаток исходного набора.",
            InvalidRequestErrorCode);

    // Отпечаток одной строки текста в кодировке UTF-8.
    private static string Sha256Utf8(string? value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    // Длина строки и её байты дописываются в отпечаток: так две соседние
    // строки нельзя склеить в одно и то же значение.
    private static void AppendText(IncrementalHash hash, string? value)
    {
        // Отсутствующая строка помечается отдельной длиной минус один.
        if (value is null)
        {
            // Дальше байтов нет: отсутствие отличается от пустой строки.
            AppendInt32(hash, -1);
            return;
        }

        // Байты строки в кодировке UTF-8.
        var bytes = Encoding.UTF8.GetBytes(value);
        // Сначала длина, потом сами байты.
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    // Целое число дописывается в отпечаток в младшем-первым порядке байтов.
    private static void AppendInt32(IncrementalHash hash, int value)
    {
        // Четыре байта числа складываются на стек без выделения памяти.
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
