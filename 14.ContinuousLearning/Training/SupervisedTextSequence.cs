// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using Tokenizer;

/// <summary>
/// Единственный владелец формата учебной последовательности обучения с учителем
/// (SFT — supervised fine-tuning, дообучение по готовым парам «запрос → ответ»).
/// Раньше запрос и ответ склеивались переводом строки прямо внутри тренера, а
/// читатель ответа из сохранённого состояния брал материал прямого сравнения
/// предпочтений (DPO — Direct Preference Optimization) без разделителя. Из-за
/// этого на первой ответной позиции модель предсказывала пропущенный
/// разделитель, и прибор объявлял правильно выученный ответ неверным. Теперь и
/// писатель обучения, и читатель ответа, и диагностика остановки берут формат
/// отсюда, поэтому расходиться в разделителе им больше нечем.
/// </summary>
internal static class SupervisedTextSequence
{
    /// <summary>Имя версии формата; им помечаются чтения и приборы.</summary>
    internal const string ProfileVersion = "sft-joint-lf-v1";

    /// <summary>Разделитель запроса и ответа: один перевод строки (LF).</summary>
    internal const string Separator = "\n";

    /// <summary>
    /// Отказ диагностики, когда словарь склеил знак через границу запроса и
    /// ответа. Это отказ только чтения ответа и права на остановку; обычное
    /// обучение на этой строке остаётся разрешённым.
    /// </summary>
    internal const string BoundaryUnresolvedErrorCode = "response_boundary_unresolved";

    /// <summary>Отказ, когда на входе нет текста запроса или ответа.</summary>
    internal const string InvalidTextErrorCode = "SUPERVISED_SEQUENCE_TEXT_INVALID";

    // Склейка одной учебной строки в точности так, как её учили: запрос, один
    // перевод строки, ответ. Отдельный метод нужен, чтобы ни один потребитель
    // не переписывал эту склейку по памяти.
    internal static string ComposeJointText(string prompt, string response)
        => string.Concat(prompt, Separator, response);

    /// <summary>
    /// Кодирует учебную строку одним вызовом словаря — ровно как это делал
    /// тренер до выделения формата. Возвращается простой список знаков без
    /// обёртки результата: это горячий путь обучения, в нём не должно появиться
    /// ни второго кодирования, ни подсчёта метрик, ни хеширования состояний, а
    /// проверки длины и отказы остаются на стороне вызывающего тренера.
    /// </summary>
    internal static IReadOnlyList<int> EncodeJointLf(
        BpeTokenizer tokenizer,
        string prompt,
        string response)
    {
        // Пустая ссылка на словарь — ошибка вызова, а не учебный случай.
        ArgumentNullException.ThrowIfNull(tokenizer);
        // Один и только один вызов кодирования на одну учебную строку.
        return tokenizer.Encode(ComposeJointText(prompt, response));
    }

    /// <summary>
    /// Находит границу ответа в той же учебной последовательности и выдаёт
    /// материал для чтения ответа из сохранённого состояния: сдвинутые входы и
    /// цели, как их строит родной тренер, и номера именно ответных целей.
    /// Этим же материалом пользуется диагностика остановки, поэтому обучение и
    /// чтение считают ответ по одним и тем же позициям.
    /// </summary>
    internal static OperationResult<SupervisedResponseSpan> ResolveResponseSpan(
        BpeTokenizer tokenizer,
        string prompt,
        string response)
    {
        // Пустая ссылка на словарь — ошибка вызова, а не учебный случай.
        ArgumentNullException.ThrowIfNull(tokenizer);
        // Без текста запроса или ответа границы ответа не существует.
        if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(response))
        {
            return OperationResult<SupervisedResponseSpan>.Fail(
                "Для разбора границы ответа нужны непустой запрос и непустой ответ.",
                InvalidTextErrorCode);
        }

        // Знаки запроса вместе с разделителем: именно их длина задаёт границу.
        var promptWithSeparator = tokenizer.Encode(
            string.Concat(prompt, Separator));
        // Полная учебная последовательность — та же, что уходит в обучение.
        var fullSequence = EncodeJointLf(tokenizer, prompt, response);
        var promptLength = promptWithSeparator.Count;
        var fullLength = fullSequence.Count;

        // Граница подтверждается только тогда, когда знаки запроса с
        // разделителем дословно остались началом полной последовательности и
        // после них есть хотя бы один ответный знак. Иначе словарь слил знак
        // через границу, и чтение ответа отказывается называть позиции.
        if (promptLength <= 0
            || fullLength <= promptLength
            || !StartsWithSameTokens(fullSequence, promptWithSeparator))
        {
            return OperationResult<SupervisedResponseSpan>.Fail(
                "Знаки запроса с разделителем не являются началом учебной последовательности, поэтому ответные позиции назвать нельзя.",
                BoundaryUnresolvedErrorCode);
        }

        // Сдвиг на один знак — тот же, что в родном тренере: по входу с номером
        // i предсказывается знак с номером i + 1.
        var shiftedLength = fullLength - 1;
        var shiftedInputs = new int[shiftedLength];
        var shiftedTargets = new int[shiftedLength];
        for (var index = 0; index < shiftedLength; index++)
        {
            // Вход очередной позиции — знак учебной последовательности.
            shiftedInputs[index] = fullSequence[index];
            // Цель очередной позиции — следующий знак той же строки.
            shiftedTargets[index] = fullSequence[index + 1];
        }

        // Ответными считаются цели, которые стоят после разделителя. На
        // сдвинутых целях это номера с promptLength - 1 по shiftedLength - 1;
        // условие «номер не меньше promptLength» снова потеряло бы первый
        // ответный знак, ради которого и делается это исправление.
        var responseTargetIndices = new int[fullLength - promptLength];
        for (var offset = 0; offset < responseTargetIndices.Length; offset++)
        {
            // Первая ответная цель стоит ровно на номере promptLength - 1.
            responseTargetIndices[offset] = promptLength - 1 + offset;
        }

        // Отпечаток последовательности нужен отчётам, чтобы доказать, что две
        // проверки читали одни и те же знаки; в обучении он не вычисляется.
        var digest = ComputeSequenceDigest(ProfileVersion, fullSequence, promptLength);
        return OperationResult<SupervisedResponseSpan>.Ok(
            new SupervisedResponseSpan(
                ProfileVersion,
                fullSequence,
                promptLength,
                Array.AsReadOnly(shiftedInputs),
                Array.AsReadOnly(shiftedTargets),
                Array.AsReadOnly(responseTargetIndices),
                digest));
    }

    // Дословная проверка начала: сравниваются сами знаки, а не только их число.
    private static bool StartsWithSameTokens(
        IReadOnlyList<int> fullSequence,
        IReadOnlyList<int> prefix)
    {
        // Длина начала не может превышать длину полной последовательности.
        if (prefix.Count > fullSequence.Count)
            return false;
        for (var index = 0; index < prefix.Count; index++)
        {
            // Любое расхождение знака означает неподтверждённую границу.
            if (fullSequence[index] != prefix[index])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Отпечаток последовательности: имя формата, длина границы и все знаки
    /// строки. Один владелец отпечатка нужен, чтобы диагностический контроль
    /// без разделителя считал его тем же способом и расхождение формата было
    /// видно по самому отпечатку.
    /// </summary>
    internal static string ComputeSequenceDigest(
        string profileVersion,
        IReadOnlyList<int> fullSequence,
        int promptLength)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> number = stackalloc byte[sizeof(int)];
        // Имя формата входит в отпечаток: другой формат даёт другой отпечаток.
        hash.AppendData(Encoding.UTF8.GetBytes(profileVersion));
        // Граница ответа входит в отпечаток как отдельное число.
        BinaryPrimitives.WriteInt32LittleEndian(number, promptLength);
        hash.AppendData(number);
        // Длина строки отделяет короткую строку от её начала в длинной.
        BinaryPrimitives.WriteInt32LittleEndian(number, fullSequence.Count);
        hash.AppendData(number);
        foreach (var tokenId in fullSequence)
        {
            // Каждый знак учебной строки попадает в отпечаток по порядку.
            BinaryPrimitives.WriteInt32LittleEndian(number, tokenId);
            hash.AppendData(number);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}

/// <summary>
/// Разобранная учебная последовательность: сами знаки, граница ответа,
/// сдвинутые входы и цели родного тренера, номера ответных целей и отпечаток.
/// Это описание данных, а не обучаемый выбиратель и не политика остановки.
/// </summary>
internal sealed record SupervisedResponseSpan(
    string ProfileVersion,
    IReadOnlyList<int> FullSequenceIds,
    int PromptWithSeparatorTokenCount,
    IReadOnlyList<int> ShiftedInputIds,
    IReadOnlyList<int> ShiftedTargetIds,
    IReadOnlyList<int> ResponseTargetIndices,
    string SequenceDigestSha256)
{
    /// <summary>Сколько ответных знаков проверяется: знаменатель чтения.</summary>
    internal int ResponseTargetCount => ResponseTargetIndices.Count;
}
