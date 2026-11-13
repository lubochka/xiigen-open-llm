// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Обычный потребитель маршрута навыка.
//
// Что он делает по-человечески. Запрос приходит со сведениями. Если в них лежит
// ответ обученного маршрутизатора и названа выбранная модель с предметной темой,
// потребитель проверяет, что этот ответ законен именно для них, и только тогда
// пускает его в выбор навыка. Если ответа нет — выбор навыка работает как
// раньше. Если ответ есть, но он незаконен, потребитель ОФОРМЛЯЕТ ОТКАЗ: он
// возвращает прежний список навыков и отдельно сообщает код причины. Прежний
// список при отказе результатом обучения не называется.
//
// Почему эта логика здесь, а не в узле-адаптере. Выбор и загрузка навыка —
// логика ядра продукта. Узел продукта только передаёт сведения запроса и
// записывает в них ответ потребителя, поэтому остаётся тонким.
namespace AgentAlgorithms.Skills;

// Итог одного выбора навыка обычным потребителем: выбранные навыки, был ли это
// откат к обязательным навыкам, применилось ли обученное слагаемое и код отказа.
// Пустой код отказа означает «отказа не было».
public sealed record LearnedSkillRouteSelection(
    IReadOnlyList<SkillTriggerLibrary.SkillTrigger> Skills,
    bool IsFallback,
    bool LearnedSignalApplied,
    string RefusalCode);

// Обычный потребитель: сведения запроса -> проверка сигнала -> выбор навыка.
public static class LearnedSkillRouteConsumer
{
    // Выполняет один обычный выбор навыка по запросу и его сведениям.
    public static LearnedSkillRouteSelection Select(
        SkillRetriever retriever,
        string taskDescription,
        IReadOnlyDictionary<string, object>? metadata,
        int? expectedCategory = null)
    {
        // Без выбора навыка потребителя не существует: это дефект вызывающей стороны.
        ArgumentNullException.ThrowIfNull(retriever);

        // Обученный сигнал читается из сведений запроса; его отсутствие законно.
        var learnedSignal = ReadLearnedSignal(metadata);

        // Выбранная модель и предметная тема читаются тем же правилом.
        var routeConfiguration = ReadConfiguration(metadata);

        // Привязка сигнала к выбранной конфигурации выполняется ДО выдачи списка навыков.
        var binding = LearnedSkillRouteSignalBinder.Bind(learnedSignal, routeConfiguration);

        // Разрешённым считается только прошедший привязку сигнал; отвергнутый в выбор не идёт.
        var acceptedSignal = binding.IsSuccess ? binding.Value : null;

        // Отказ сохраняет свой код, чтобы потребитель мог его оформить, а не промолчать.
        var refusalCode = binding.IsSuccess ? string.Empty : binding.ErrorCode;

        // Конфигурация передаётся вместе с сигналом, иначе выбор навыка не смог бы проверить привязку.
        var retrieval = retriever.Retrieve(
            taskDescription,
            expectedCategory,
            acceptedSignal,
            acceptedSignal is null ? null : routeConfiguration);

        // Итог называет и выбранные навыки, и честный признак применения обученного слагаемого.
        return new LearnedSkillRouteSelection(
            retrieval.Skills,
            retrieval.IsFallback,
            acceptedSignal is not null,
            refusalCode);
    }

    // Читает типизированный сигнал обученного маршрутизатора из сведений запроса.
    // Отсутствие ключа и чужой тип значения означают «сигнала нет», а не ошибку:
    // обычный путь без обученного слагаемого остаётся законным.
    public static LearnedSkillRouteSignal? ReadLearnedSignal(IReadOnlyDictionary<string, object>? metadata)
    {
        // Без сведений запроса читать нечего.
        if (metadata is null)
            return null;

        // Ключ может отсутствовать: это обычный дефолтный случай.
        if (!metadata.TryGetValue(LearnedSkillRouteSignalBinder.SignalMetadataKey, out var value))
            return null;

        // Значение принимается только тогда, когда это действительно типизированный сигнал.
        return value as LearnedSkillRouteSignal;
    }

    // Читает выбранную модель и предметную тему из сведений запроса тем же правилом.
    public static SkillRouteConfiguration? ReadConfiguration(IReadOnlyDictionary<string, object>? metadata)
    {
        // Без сведений запроса читать нечего.
        if (metadata is null)
            return null;

        // Ключ может отсутствовать: это обычный дефолтный случай.
        if (!metadata.TryGetValue(LearnedSkillRouteSignalBinder.ConfigurationMetadataKey, out var value))
            return null;

        // Значение принимается только тогда, когда это действительно типизированная конфигурация.
        return value as SkillRouteConfiguration;
    }
}
