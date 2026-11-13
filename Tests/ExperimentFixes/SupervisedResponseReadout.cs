// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using ContinuousLearning;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;

/// <summary>
/// Читатель ответа из сохранённого состояния. Он получает формат учебной
/// последовательности от единственного владельца формата в продукте
/// (SupervisedTextSequence) и поэтому больше не может разойтись с обучением в
/// разделителе. Отдельным именованным профилем сохраняется старое
/// диагностическое чтение «встык, без разделителя»: оно нужно как контроль
/// представления и не управляет остановкой обучения.
/// </summary>
internal static class SupervisedResponseReadout
{
    /// <summary>Профиль обучения с учителем: тот же, что у писателя обучения.</summary>
    internal const string SupervisedJointLfProfile =
        SupervisedTextSequence.ProfileVersion;

    /// <summary>
    /// Профиль диагностического контроля: запрос и ответ кодируются отдельно и
    /// склеиваются встык, как в материале прямого сравнения предпочтений.
    /// </summary>
    internal const string DiagnosticNoSeparatorProfile =
        "dpo-segmented-no-separator";

    /// <summary>Усреднение оценки ответа; записывается рядом с числами.</summary>
    internal const string ResponseScoreAggregation =
        "mean_response_log_probability";

    /// <summary>Отказ, когда назван неизвестный профиль чтения.</summary>
    internal const string UnknownProfileErrorCode =
        "SUPERVISED_READOUT_PROFILE_UNKNOWN";

    /// <summary>Отказ, когда фактические учебные знаки не совпали с форматом.</summary>
    internal const string ActualIdsMismatchErrorCode =
        "SUPERVISED_READOUT_ACTUAL_IDS_MISMATCH";

    /// <summary>Отказ, когда выход модели не той формы, что ожидает чтение.</summary>
    internal const string LogitsShapeErrorCode =
        "SUPERVISED_READOUT_LOGITS_SHAPE_INVALID";

    /// <summary>
    /// Готовит материал чтения: сами знаки учебной строки, границу ответа,
    /// сдвинутые входы и цели родного тренера и номера ответных целей. Ни одного
    /// вызова модели здесь не делается.
    /// </summary>
    /// <param name="tokenizer">Тот же словарь, которым строка обучалась.</param>
    /// <param name="prompt">Текст запроса.</param>
    /// <param name="response">Текст правильного ответа.</param>
    /// <param name="profileVersion">Имя профиля склейки.</param>
    /// <param name="actualTrainingIds">
    /// Фактически переданные обучению знаки, если они известны: чтение обязано
    /// совпасть с ними знак в знак, иначе читается не то, чему учили.
    /// </param>
    internal static OperationResult<SupervisedResponseReadoutMaterial> Prepare(
        BpeTokenizer tokenizer,
        string prompt,
        string response,
        string profileVersion,
        IReadOnlyList<int>? actualTrainingIds = null)
    {
        // Пустая ссылка на словарь — ошибка вызова, а не учебный случай.
        ArgumentNullException.ThrowIfNull(tokenizer);
        // Профиль обучения с учителем берёт границу у владельца формата.
        if (StringComparer.Ordinal.Equals(profileVersion, SupervisedJointLfProfile))
        {
            var span = SupervisedTextSequence.ResolveResponseSpan(
                tokenizer,
                prompt,
                response);
            // Неподтверждённая граница — отказ чтения, а не отказ обучения.
            if (!span.IsSuccess || span.Value is null)
                return span.MapError<SupervisedResponseReadoutMaterial>();
            var material = new SupervisedResponseReadoutMaterial(
                span.Value.ProfileVersion,
                tokenizer.ComputeStateSha256(),
                span.Value.FullSequenceIds,
                span.Value.PromptWithSeparatorTokenCount,
                span.Value.ShiftedInputIds,
                span.Value.ShiftedTargetIds,
                span.Value.ResponseTargetIndices,
                span.Value.SequenceDigestSha256);
            // Фактические учебные знаки сверяются до любого вызова модели.
            return MatchesActualTrainingIds(material, actualTrainingIds)
                ? OperationResult<SupervisedResponseReadoutMaterial>.Ok(material)
                : ActualIdsMismatch();
        }

        // Диагностический контроль: запрос и ответ кодируются по отдельности и
        // склеиваются встык. Это именно то чтение, которое теряло первый знак
        // ответа; оно сохраняется как контроль, а не как мерило усвоения.
        if (StringComparer.Ordinal.Equals(profileVersion, DiagnosticNoSeparatorProfile))
        {
            var promptIds = tokenizer.Encode(prompt);
            var responseIds = tokenizer.Encode(response);
            // Без знаков запроса или ответа ответные позиции назвать нельзя.
            if (promptIds.Count <= 0 || responseIds.Count <= 0)
            {
                return OperationResult<SupervisedResponseReadoutMaterial>.Fail(
                    "Диагностическое чтение без разделителя требует непустых знаков запроса и ответа.",
                    SupervisedTextSequence.BoundaryUnresolvedErrorCode);
            }

            var joined = new List<int>(promptIds.Count + responseIds.Count);
            // Знаки запроса идут первыми и задают границу ответа.
            joined.AddRange(promptIds);
            // Знаки ответа приклеиваются встык, без разделителя.
            joined.AddRange(responseIds);
            var material = BuildMaterial(
                DiagnosticNoSeparatorProfile,
                tokenizer.ComputeStateSha256(),
                joined,
                promptIds.Count);
            // И для контроля фактические знаки сверяются так же строго.
            return MatchesActualTrainingIds(material, actualTrainingIds)
                ? OperationResult<SupervisedResponseReadoutMaterial>.Ok(material)
                : ActualIdsMismatch();
        }

        // Любое другое имя профиля — отказ: молча подставлять формат нельзя.
        return OperationResult<SupervisedResponseReadoutMaterial>.Fail(
            "Имя профиля чтения ответа неизвестно.",
            UnknownProfileErrorCode);
    }

    /// <summary>
    /// Читает ответ одним вызовом модели без обучения: по каждой ответной
    /// позиции сообщает нужный знак, знак первого места, место правильного
    /// знака, а также среднюю логарифмическую вероятность ответа и среднюю
    /// отрицательную логарифмическую правдоподобность по той же маске.
    /// Ни одного обновления весов и ни одного второго вызова модели.
    /// </summary>
    internal static OperationResult<SupervisedResponseReadoutResult> Evaluate(
        LlmModel model,
        SupervisedResponseReadoutMaterial material)
    {
        // Пустые ссылки — ошибка вызова, а не учебный случай.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(material);
        // Пустой знаменатель не превращается в ноль правильных ответов.
        if (material.ResponseTargetCount <= 0)
        {
            return OperationResult<SupervisedResponseReadoutResult>.Fail(
                "У чтения ответа нет ни одной ответной позиции.",
                SupervisedTextSequence.BoundaryUnresolvedErrorCode);
        }

        var length = material.ShiftedInputIds.Count;
        var inputs = new float[length];
        for (var index = 0; index < length; index++)
        {
            // В модель уходят только сдвинутые входы, как в родном тренере.
            inputs[index] = material.ShiftedInputIds[index];
        }

        // Единственный вызов модели: обучение выключено, веса не меняются.
        var logits = model.Forward(
            new Tensor(inputs, new TensorShape(1, length)),
            training: false);
        var dimensions = logits.Shape.Dimensions;
        // Выход обязан быть «одна строка, столько же позиций, словарь».
        if (dimensions.Length != 3
            || dimensions[0] != 1
            || dimensions[1] != length
            || dimensions[2] <= 0)
        {
            return OperationResult<SupervisedResponseReadoutResult>.Fail(
                "Выход модели не совпал с ожидаемой формой чтения ответа.",
                LogitsShapeErrorCode);
        }

        var vocabularySize = dimensions[2];
        var positions = new SupervisedResponsePosition[material.ResponseTargetCount];
        var targetTokenIds = new int[material.ResponseTargetCount];
        var correctTokens = 0;
        var logProbabilitySum = 0d;
        for (var slot = 0; slot < material.ResponseTargetCount; slot++)
        {
            var shiftedIndex = material.ResponseTargetIndices[slot];
            // Номер ответной позиции обязан попадать внутрь выхода модели.
            if (shiftedIndex < 0 || shiftedIndex >= length)
            {
                return OperationResult<SupervisedResponseReadoutResult>.Fail(
                    "Номер ответной позиции вышел за пределы выхода модели.",
                    LogitsShapeErrorCode);
            }

            var targetTokenId = material.ShiftedTargetIds[shiftedIndex];
            // Нужный знак обязан существовать в словаре модели.
            if (targetTokenId < 0 || targetTokenId >= vocabularySize)
            {
                return OperationResult<SupervisedResponseReadoutResult>.Fail(
                    "Нужный знак ответа отсутствует в словаре модели.",
                    LogitsShapeErrorCode);
            }

            var rowOffset = shiftedIndex * vocabularySize;
            var maximumValue = float.NegativeInfinity;
            var argmaxTokenId = 0;
            for (var candidate = 0; candidate < vocabularySize; candidate++)
            {
                var value = logits.Data[rowOffset + candidate];
                // Первым местом считается наибольшая оценка этой позиции.
                if (value > maximumValue)
                {
                    maximumValue = value;
                    argmaxTokenId = candidate;
                }
            }

            var goldValue = logits.Data[rowOffset + targetTokenId];
            var above = 0;
            var exponentSum = 0d;
            for (var candidate = 0; candidate < vocabularySize; candidate++)
            {
                var value = logits.Data[rowOffset + candidate];
                // Место правильного знака — сколько знаков стоят выше него.
                if (value > goldValue)
                    above++;
                // Сумма считается с вычитанием наибольшего значения, иначе
                // показательная функция переполнится на больших оценках.
                exponentSum += Math.Exp(value - maximumValue);
            }

            // Логарифм вероятности правильного знака по устойчивой формуле.
            var logSumExp = maximumValue + Math.Log(exponentSum);
            logProbabilitySum += goldValue - logSumExp;
            var correct = argmaxTokenId == targetTokenId;
            // Правильной считается только позиция, где нужный знак на первом месте.
            if (correct)
                correctTokens++;
            targetTokenIds[slot] = targetTokenId;
            positions[slot] = new SupervisedResponsePosition(
                shiftedIndex,
                targetTokenId,
                argmaxTokenId,
                correct,
                above + 1);
        }

        // Среднее берётся только по ответным позициям: запрос в него не входит.
        var meanLogProbability = logProbabilitySum / material.ResponseTargetCount;
        return OperationResult<SupervisedResponseReadoutResult>.Ok(
            new SupervisedResponseReadoutResult(
                material.ProfileVersion,
                material.SequenceDigestSha256,
                correctTokens,
                material.ResponseTargetCount,
                correctTokens == material.ResponseTargetCount,
                meanLogProbability,
                -meanLogProbability,
                ResponseScoreAggregation,
                Array.AsReadOnly(positions),
                Array.AsReadOnly(targetTokenIds)));
    }

    // Общая сборка материала по готовым знакам и границе ответа.
    private static SupervisedResponseReadoutMaterial BuildMaterial(
        string profileVersion,
        string tokenizerSha256,
        IReadOnlyList<int> fullSequence,
        int promptBoundary)
    {
        var shiftedLength = fullSequence.Count - 1;
        var shiftedInputs = new int[shiftedLength];
        var shiftedTargets = new int[shiftedLength];
        for (var index = 0; index < shiftedLength; index++)
        {
            // Вход очередной позиции — знак последовательности.
            shiftedInputs[index] = fullSequence[index];
            // Цель очередной позиции — следующий знак последовательности.
            shiftedTargets[index] = fullSequence[index + 1];
        }

        var responseTargets = new int[fullSequence.Count - promptBoundary];
        for (var offset = 0; offset < responseTargets.Length; offset++)
        {
            // Первая ответная цель стоит на номере promptBoundary - 1.
            responseTargets[offset] = promptBoundary - 1 + offset;
        }

        return new SupervisedResponseReadoutMaterial(
            profileVersion,
            tokenizerSha256,
            fullSequence,
            promptBoundary,
            Array.AsReadOnly(shiftedInputs),
            Array.AsReadOnly(shiftedTargets),
            Array.AsReadOnly(responseTargets),
            // Отпечаток считается тем же владельцем, что и у формата обучения,
            // поэтому разницу профилей видно по самому отпечатку.
            SupervisedTextSequence.ComputeSequenceDigest(
                profileVersion,
                fullSequence,
                promptBoundary));
    }

    // Сверка с фактически переданными обучению знаками: знак в знак.
    private static bool MatchesActualTrainingIds(
        SupervisedResponseReadoutMaterial material,
        IReadOnlyList<int>? actualTrainingIds)
    {
        // Если фактические знаки не переданы, сверять нечего.
        if (actualTrainingIds is null)
            return true;
        // Разная длина сразу означает, что читается не та строка.
        if (actualTrainingIds.Count != material.FullSequenceIds.Count)
            return false;
        for (var index = 0; index < actualTrainingIds.Count; index++)
        {
            // Любое расхождение знака — отказ чтения.
            if (actualTrainingIds[index] != material.FullSequenceIds[index])
                return false;
        }

        return true;
    }

    // Единый отказ сверки фактических учебных знаков.
    private static OperationResult<SupervisedResponseReadoutMaterial>
        ActualIdsMismatch()
        => OperationResult<SupervisedResponseReadoutMaterial>.Fail(
            "Фактически переданные обучению знаки не совпали с читаемой последовательностью.",
            ActualIdsMismatchErrorCode);
}

/// <summary>
/// Материал чтения ответа: знаки строки, граница ответа, сдвинутые входы и
/// цели, номера ответных целей, личность словаря и отпечаток последовательности.
/// </summary>
internal sealed record SupervisedResponseReadoutMaterial(
    string ProfileVersion,
    string TokenizerSha256,
    IReadOnlyList<int> FullSequenceIds,
    int PromptBoundaryTokenCount,
    IReadOnlyList<int> ShiftedInputIds,
    IReadOnlyList<int> ShiftedTargetIds,
    IReadOnlyList<int> ResponseTargetIndices,
    string SequenceDigestSha256)
{
    /// <summary>Знаменатель чтения: сколько ответных знаков проверяется.</summary>
    internal int ResponseTargetCount => ResponseTargetIndices.Count;
}

/// <summary>Одна ответная позиция: что требуется, что поставила модель.</summary>
internal sealed record SupervisedResponsePosition(
    int ShiftedIndex,
    int TargetTokenId,
    int ArgmaxTokenId,
    bool Correct,
    int GoldRank);

/// <summary>
/// Результат чтения ответа: целые числа правильных и всех ответных знаков,
/// позиции, средняя логарифмическая вероятность ответа и её минус.
/// </summary>
internal sealed record SupervisedResponseReadoutResult(
    string ProfileVersion,
    string SequenceDigestSha256,
    int CorrectTokens,
    int ResponseTargetCount,
    bool AllCorrect,
    double ResponseMeanLogProbability,
    double ResponseMeanNll,
    string ScoreAggregation,
    IReadOnlyList<SupervisedResponsePosition> Positions,
    IReadOnlyList<int> ResponseTargetTokenIds)
{
    /// <summary>
    /// Доля правильных ответных знаков. При пустом знаменателе это не ноль, а
    /// отсутствие значения: пустое чтение нельзя называть нулевым усвоением.
    /// </summary>
    internal double? ResponseCorrectFraction
        => ResponseTargetCount > 0
            ? (double)CorrectTokens / ResponseTargetCount
            : null;
}

/// <summary>
/// Диагностика остановки обучения с учителем: предикат, который разрешает
/// считать ответ усвоенным. Он истинен только на профиле обучения, при
/// ненулевом знаменателе и при полном совпадении всех ответных знаков. Это
/// проверка правильности измерения, а не включённая в продукт политика
/// остановки: в показанном цикле тренера такого решения нет.
/// </summary>
internal static class SftAcquisitionStop
{
    /// <summary>Отказ, когда остановку считают по чужому профилю чтения.</summary>
    internal const string IncompatibleProfileErrorCode =
        "SFT_STOP_PROFILE_INCOMPATIBLE";

    /// <summary>Отказ, когда у чтения нет ни одной ответной позиции.</summary>
    internal const string EmptyDenominatorErrorCode =
        "SFT_STOP_EMPTY_DENOMINATOR";

    /// <summary>
    /// Разрешено ли остановить обучение на этом чтении. Несовместимый профиль и
    /// пустой знаменатель — отказы, а не «ещё не выучено».
    /// </summary>
    internal static OperationResult<bool> IsSatisfied(
        SupervisedResponseReadoutResult readout)
    {
        // Пустая ссылка — ошибка вызова, а не учебный случай.
        ArgumentNullException.ThrowIfNull(readout);
        // Решение об остановке принимается только по профилю обучения.
        if (!StringComparer.Ordinal.Equals(
                readout.ProfileVersion,
                SupervisedResponseReadout.SupervisedJointLfProfile))
        {
            return OperationResult<bool>.Fail(
                "Остановку обучения нельзя решать по диагностическому профилю чтения.",
                IncompatibleProfileErrorCode);
        }

        // Пустой знаменатель не даёт права останавливать обучение.
        if (readout.ResponseTargetCount <= 0)
        {
            return OperationResult<bool>.Fail(
                "У чтения ответа нет ни одной ответной позиции, поэтому остановка не обоснована.",
                EmptyDenominatorErrorCode);
        }

        // Остановка разрешена только при полном совпадении всех ответных знаков.
        return OperationResult<bool>.Ok(
            readout.CorrectTokens == readout.ResponseTargetCount);
    }
}
