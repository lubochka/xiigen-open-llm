// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие владения признаками обычного блока ближайших соседей
// (работа R9 плана ремонта 98 из папки docs/plans/replanning-possible-diffs/
// fixes-based-on-experiments, находка A29-09 аудита 97).
//
// Что здесь проверяется по-человечески. Блок ближайших соседей учится на
// размеченных примерах и потом отвечает по ближайшему из них. Вместе с ответом
// он отдаёт три списка признаков: условия, предметные метки и ссылки на
// доказательства. Вопрос простой: принадлежат ли эти списки самому блоку или
// он делится ими с вызывающей стороной.
//
// Что было сломано. При приёме примера делалась копия записи, но вложенные
// массивы оставались общими с вызывающим кодом, а на выдаче два списка
// отдавались наружу напрямую. Поэтому вызывающая сторона могла изменить свои
// массивы уже после обучения — либо изменить массивы первого полученного
// ответа — и получить другой следующий ответ, хотя блок ничему не учился.
// Для обученной памяти это тихая порча, а не удобство.
//
// Как это доказывается. Один маленький учебный набор из одной строки с
// буквальными метками. Сначала обычный ответ. Потом нападение со стороны
// входа: меняются те самые массивы, которые вызывающая сторона передала в
// обучение. Потом нападение со стороны выхода: меняются массивы первого
// полученного ответа. Между нападением и повторным чтением НЕТ ни повторного
// обучения, ни восстановления, ни перестройки указателя соседей — иначе
// проверка просто скрыла бы порчу. Ожидаемые значения выписаны буквально, а не
// взяты из самого же объекта.
//
// Бюджет этой проверки. Одна подготовка токенизатора на корпусе текущего
// набора, одно успешное обучение одной строки, три чтения ответа и две
// отвергнутые попытки обучения до токенизатора и указателя соседей. Ни одного
// обучения с учителем, ни одной пары предпочтений.
//
// Чего это покрытие НЕ доказывает. Оно не проверяет запасной путь для пустого
// списка предметных меток: основной сценарий намеренно идёт с непустыми
// метками. Оно не даёт универсального владения любым изменяемым ответом:
// значение ответа отдаётся как есть, и для изменяемого типа ответа нужен
// отдельный договор конкретного типа, а не обход через сериализацию. Результат
// этого класса не распространяется на соседние блоки другого устройства.

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;

public sealed class PlainKnnMetadataOwnershipTests
{
    // Обученный ответ один и тот же до нападения и после обоих нападений:
    // ни вход, ни выход больше не дают вызывающей стороне чужую память.
    [Fact]
    public async Task TrainedRowKeepsItsOwnMetadata_AfterIngressAndEgressMutation()
    {
        // Массивы вызывающей стороны: именно их потом и будут менять.
        var callerPredicates = new[] { "SECURITY", "AUTHORIZATION" };
        var callerDomainTags = new[] { "security" };
        var callerEvidenceIds = new[] { "pair-security" };

        // Один учебный пример: обычная строка с непустыми метками.
        var trained = KnnLogicalUnit<string, string>.Train(
            "unit-knn-metadata-ownership",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "pair-security",
                    "jwt oauth authorization token",
                    "SECURITY",
                    callerPredicates,
                    callerDomainTags,
                    callerEvidenceIds)
            ],
            input => input,
            vectorDimensions: 32,
            k: 1);

        // Обучение обязано пройти через общий конверт результата.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // Один и тот же блок отвечает на все три чтения: подменять его нельзя.
        var unit = trained.Value!;

        // Чтение первое: обычный ответ до любого нападения.
        var first = await Ask(unit);
        // Ожидания выписаны буквально, а не прочитаны из самого ответа.
        AssertExpectedAnswer(first);

        // Нападение со стороны входа: вызывающая сторона меняет свои массивы
        // уже после обучения, прямо в тех же ячейках.
        callerPredicates[0] = "СЛОМАНО-УСЛОВИЕ";
        callerDomainTags[0] = "сломано-метка";
        callerEvidenceIds[0] = "сломано-ссылка";

        // Чтение второе: между нападением и чтением не было ни обучения, ни
        // восстановления, ни перестройки указателя соседей.
        var second = await Ask(unit);
        AssertExpectedAnswer(second);

        // Нападение со стороны выхода: меняются массивы первого ответа.
        first.PredicateIds[0] = "СЛОМАНО-ВЫХОД";
        first.DomainTags[0] = "сломано-выход";
        first.EvidenceIds[0] = "сломано-выход";

        // Чтение третье: тот же блок снова отвечает прежними значениями.
        var third = await Ask(unit);
        AssertExpectedAnswer(third);

        // Два разных ответа не делят между собой один и тот же массив.
        Assert.NotSame(first.PredicateIds, third.PredicateIds);
        Assert.NotSame(first.DomainTags, third.DomainTags);
        // Ответ не делит массив и с вызывающей стороной обучения.
        Assert.NotSame(callerPredicates, third.PredicateIds);
        Assert.NotSame(callerDomainTags, third.DomainTags);
        Assert.NotSame(callerEvidenceIds, third.EvidenceIds);
    }

    // Неполная строка отвергается до подготовки токенизатора и указателя
    // соседей: счётчик обращений к выбору текста остаётся нулевым.
    [Fact]
    public void IncompleteRow_IsRejectedBeforeTokenizerAndIndexPreparation()
    {
        // Счётчик обращений к выбору текста: он вызывается только дорогой подготовкой.
        var textSelectorCalls = 0;
        // Тот же выбор текста, но с подсчётом обращений.
        string CountingSelector(string input)
        {
            textSelectorCalls++;
            return input;
        }

        // Первая отвергнутая попытка: отсутствует список условий.
        var missingMetadata = KnnLogicalUnit<string, string>.Train(
            "unit-knn-metadata-preflight",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "pair-security", "jwt oauth authorization token", "SECURITY",
                    null!, ["security"], ["pair-security"])
            ],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом с именованным кодом.
        Assert.False(missingMetadata.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_METADATA_REQUIRED", missingMetadata.ErrorCode);

        // Вторая отвергнутая попытка: отсутствует устойчивое имя строки.
        var missingIdentity = KnnLogicalUnit<string, string>.Train(
            "unit-knn-identity-preflight",
            "test-classifier",
            "test-domain",
            [
                new LogicalKnnTrainingExample<string, string>(
                    "   ", "jwt oauth authorization token", "SECURITY",
                    ["SECURITY"], ["security"], ["pair-security"])
            ],
            CountingSelector,
            vectorDimensions: 32,
            k: 1);

        // Отказ приходит обычным конвертом с прежним именованным кодом.
        Assert.False(missingIdentity.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_ID_REQUIRED", missingIdentity.ErrorCode);

        // Доказательство раннего отказа: выбор текста нужен только корпусу
        // токенизатора и вектору примера, и его ни разу не спросили.
        Assert.Equal(0, textSelectorCalls);
    }

    // Одно обычное чтение обученного блока на том же входе, что и учебная строка.
    private static async Task<LogicalUnitResult<string>> Ask(KnnLogicalUnit<string, string> unit)
    {
        // Запрос повторяет учебный вход и его условия.
        var result = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-metadata-ownership",
            "unit-knn-metadata-ownership",
            "test-classifier",
            "test-domain",
            "jwt oauth authorization token",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>
            {
                ["predicate_ids"] = new[] { "SECURITY", "AUTHORIZATION" }
            }));

        // Чтение обязано пройти через общий конверт результата.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    // Буквальные ожидаемые значения одного ответа обученного блока.
    private static void AssertExpectedAnswer(LogicalUnitResult<string> answer)
    {
        // Выбран тот же сосед и тот же ответ.
        Assert.Equal("SECURITY", answer.Output);
        // Список условий тот, что был передан в обучение, а не изменённый позже.
        Assert.Equal(["SECURITY", "AUTHORIZATION"], answer.PredicateIds);
        // Предметная метка та же: запасной путь для пустого списка здесь не участвует.
        Assert.Equal(["security"], answer.DomainTags);
        // Ссылки на доказательства: имя соседа и ссылка самой учебной строки.
        Assert.Equal(["pair-security"], answer.EvidenceIds);
        // Уверенность осталась в прежней рабочей области значений.
        Assert.True(answer.Confidence >= 0.70, $"confidence={answer.Confidence}");
    }
}
