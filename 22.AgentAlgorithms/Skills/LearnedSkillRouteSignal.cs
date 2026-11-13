// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Типизированный сигнал обученного маршрутизатора для обычного выбора навыка.
//
// Кто здесь обучаемая власть. Решение о маршруте принимает обученная единица
// разрешённого уровня `ILogicalUnit<TInput,TOutput>` —
// `AgentInfrastructure.Logical.KnnLogicalUnit<string, RouteDecision>`: у неё есть
// вход обучения (`Train`), собственное обученное состояние (словарь BPE, указатель
// ближайших соседей, принятые уроки) и чтение этого состояния (`ExecuteAsync`).
// Этот файл НЕ является новым обучаемым механизмом и не становится властью: он
// только переносит уже полученный ответ обученной единицы к выбору навыка и
// проверяет, что перенос законен.
//
// Кто здесь детерминированное основание. Мера Жаккара словесных признаков и
// порог 0,40 в `SkillRetriever` остаются детерминированным базовым уровнем
// (eval/baseline, внутренняя деталь). Они не объявляются обученной способностью
// и в этом срезе не меняются.
//
// Почему проверка обязательна. Сигнал приходит извне выбора, поэтому он может
// назвать несуществующий навык, прийти из чужой темы обучения или чужой модели,
// либо принести бессмысленную оценку. Любой такой сигнал обязан быть отвергнут
// ДО выдачи списка навыков, а не превращён в максимальный приоритет.
namespace AgentAlgorithms.Skills;

// Ответ обученного маршрутизатора в том виде, в каком его читает выбор навыка:
// какой навык названа обученная память, с какой оценкой, какой моделью и по какой
// предметной теме. Оценка — это балл ранжирования, а НЕ вероятность правильного
// ответа.
public sealed record LearnedSkillRouteSignal(string SkillId, double Score, string ModelId, string DomainId);

// Выбранная конфигурация потребителя: та модель и та предметная тема, для
// которых потребитель уже выбрал маршрут. Сигнал законен только внутри неё.
public sealed record SkillRouteConfiguration(string ModelId, string DomainId);

// Привязка сигнала обученного маршрутизатора к выбранной конфигурации.
// Один и тот же разбор обслуживает два пути: обычный потребитель получает
// типизированный отказ без исключения, а прямой вызов выбора навыка с
// некорректным сигналом получает исключение о неверном аргументе.
public static class LearnedSkillRouteSignalBinder
{
    // Нижняя граница допустимой оценки сигнала: оценка ниже нуля смысла не имеет.
    public const double MinimumScore = 0.0;

    // Верхняя граница допустимой оценки сигнала: оценка выше единицы сделала бы
    // один сигнал сильнее любого мыслимого детерминированного основания.
    public const double MaximumScore = 1.0;

    // Код отказа: оценка не является конечным числом внутри [0,1].
    public const string ScoreOutOfRangeCode = "LEARNED_SKILL_ROUTE_SCORE_OUT_OF_RANGE";

    // Код отказа: названный навык отсутствует в каталоге навыков.
    public const string UnknownSkillCode = "LEARNED_SKILL_ROUTE_SKILL_UNKNOWN";

    // Код отказа: модель или предметная тема сигнала не совпадают с выбранной конфигурацией.
    public const string ConfigurationMismatchCode = "LEARNED_SKILL_ROUTE_CONFIGURATION_MISMATCH";

    // Код отказа: сигнал подан без выбранной конфигурации, поэтому привязать его не к чему.
    public const string ConfigurationMissingCode = "LEARNED_SKILL_ROUTE_CONFIGURATION_MISSING";

    // Ключ, по которому обычный потребитель передаёт сигнал обученного маршрутизатора.
    public const string SignalMetadataKey = "learned_skill_route.signal";

    // Ключ, по которому обычный потребитель передаёт выбранную конфигурацию.
    public const string ConfigurationMetadataKey = "learned_skill_route.configuration";

    // Ключ, в который потребитель записывает, был ли обученный сигнал применён.
    public const string AppliedMetadataKey = "learned_skill_route.applied";

    // Ключ, в который потребитель записывает оформленный отказ вместо молчаливого отката.
    public const string RefusalMetadataKey = "learned_skill_route.refusal_code";

    // Привязывает сигнал к выбранной конфигурации и возвращает либо разрешённый
    // сигнал, либо оформленный отказ. Отсутствие сигнала — это законный случай:
    // выбор навыка тогда сохраняет исходное поведение без обученного слагаемого.
    public static OperationResult<LearnedSkillRouteSignal?> Bind(
        LearnedSkillRouteSignal? signal,
        SkillRouteConfiguration? configuration)
    {
        // Отсутствующий сигнал разрешён и означает «обученного слагаемого нет».
        if (signal is null)
            return OperationResult<LearnedSkillRouteSignal?>.Ok(null);

        // Единый разбор возвращает код первой найденной причины отказа или ничего.
        var refusalCode = Inspect(signal, configuration);

        // Найденная причина превращается в типизированный отказ: потребитель обязан
        // его оформить и не имеет права назвать молчаливый откат результатом обучения.
        if (refusalCode is not null)
            return OperationResult<LearnedSkillRouteSignal?>.Fail(Describe(refusalCode, signal, configuration), refusalCode);

        // Сигнал прошёл проверку и может участвовать в ранжировании.
        return OperationResult<LearnedSkillRouteSignal?>.Ok(signal);
    }

    // Единый разбор сигнала. Порядок проверок закреплён: сначала оценка, потому что
    // бессмысленная оценка — это отдельный класс дефекта, который нельзя спрятать
    // за несовпадением конфигурации.
    public static string? Inspect(LearnedSkillRouteSignal signal, SkillRouteConfiguration? configuration)
    {
        // Пустая ссылка на сигнал до этого разбора не доходит: её обрабатывает вызывающий.
        ArgumentNullException.ThrowIfNull(signal);

        // Оценка обязана быть конечным числом: «не число» и бесконечности отвергаются.
        if (double.IsNaN(signal.Score) || double.IsInfinity(signal.Score))
            return ScoreOutOfRangeCode;

        // Оценка обязана лежать в отрезке [0,1], как уверенность исходного опыта.
        if (signal.Score < MinimumScore || signal.Score > MaximumScore)
            return ScoreOutOfRangeCode;

        // Пустой опознаватель навыка не является существующим навыком.
        if (string.IsNullOrWhiteSpace(signal.SkillId))
            return UnknownSkillCode;

        // Сигнал разрешён только для навыка, который действительно есть в каталоге.
        if (!CatalogueContainsSkill(signal.SkillId))
            return UnknownSkillCode;

        // Без выбранной конфигурации привязать сигнал к модели и теме невозможно.
        if (configuration is null)
            return ConfigurationMissingCode;

        // Пустые поля конфигурации тоже не позволяют доказать совпадение.
        if (string.IsNullOrWhiteSpace(configuration.ModelId) || string.IsNullOrWhiteSpace(configuration.DomainId))
            return ConfigurationMissingCode;

        // Модель сигнала обязана быть той же, что у выбранной конфигурации.
        if (!string.Equals(signal.ModelId, configuration.ModelId, StringComparison.Ordinal))
            return ConfigurationMismatchCode;

        // Предметная тема сигнала обязана быть той же, что у выбранной конфигурации.
        if (!string.Equals(signal.DomainId, configuration.DomainId, StringComparison.Ordinal))
            return ConfigurationMismatchCode;

        // Все условия выполнены: причины отказа нет.
        return null;
    }

    // Проверяет наличие названного навыка в каталоге навыков по его имени.
    public static bool CatalogueContainsSkill(string skillId)
    {
        // Каталог перечисляется целиком: обязательные навыки тоже считаются существующими.
        foreach (var trigger in SkillTriggerLibrary.All)
        {
            // Сравнение имени точное: похожее имя навыка другим навыком не является.
            if (string.Equals(trigger.SkillName, skillId, StringComparison.Ordinal))
                return true;
        }

        // Ни один навык каталога не носит это имя.
        return false;
    }

    // Человеческое объяснение отказа для отчёта и для текста исключения.
    public static string Describe(string refusalCode, LearnedSkillRouteSignal signal, SkillRouteConfiguration? configuration)
    {
        // Для каждой причины отказа текст называет и наблюдённое значение, и ожидание.
        return refusalCode switch
        {
            // Оценка вне допустимого отрезка либо вообще не число.
            ScoreOutOfRangeCode =>
                $"Оценка обученного сигнала должна быть конечным числом в отрезке [{MinimumScore}, {MaximumScore}]; получено {signal.Score}.",

            // Названный навык в каталоге отсутствует.
            UnknownSkillCode =>
                $"Обученный сигнал назвал навык «{signal.SkillId}», которого нет в каталоге навыков.",

            // Конфигурация не передана или пуста.
            ConfigurationMissingCode =>
                "Обученный сигнал подан без выбранной модели и предметной темы, поэтому привязать его не к чему.",

            // Модель или тема сигнала чужие для выбранной конфигурации.
            ConfigurationMismatchCode =>
                $"Обученный сигнал относится к модели «{signal.ModelId}» и теме «{signal.DomainId}», "
                + $"а выбраны модель «{configuration?.ModelId}» и тема «{configuration?.DomainId}».",

            // Непредусмотренный код отказа не скрывается за общей фразой.
            _ => $"Обученный сигнал отвергнут по причине «{refusalCode}»."
        };
    }
}
