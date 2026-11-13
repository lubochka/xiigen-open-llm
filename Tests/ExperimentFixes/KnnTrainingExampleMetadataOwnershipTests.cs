// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Перенос раздела 3 адресного плана исправления переносов от 29 октября (файл 95):
// память модуля ближайших соседей должна владеть своими метками, а не теми
// массивами, которые остались на руках у вызывающей стороны.
//
// Проверяется ровно один договор: после обучения ни исходный вызывающий, ни
// получатель ответа не могут изменить то, что модуль отвечает дальше. Это
// маленькая договорная проверка, а не новый опыт качества: ни одна оценка
// качества ответов здесь не измеряется.
public sealed class KnnTrainingExampleMetadataOwnershipTests
{
    // Исходные значения меток записаны отдельными строками. Ожидание берётся
    // из них, а не из массивов, которые проверка будет портить: иначе атака
    // изменила бы и «ожидаемое» значение вместе с проверяемым.
    private const string OriginalPredicate = "PRED-ORIGINAL";
    private const string OriginalDomainTag = "RU";
    private const string OriginalExampleId = "S01";
    private const string OriginalOutput = "хожу";
    private const string OriginalText = "Задание: продолжи действие. Префикс: каждый день я";

    // Один обученный на словах этой панели токенизатор на весь класс:
    // доза раздела 3 разрешает ровно одно обучение словаря.
    private static readonly Lazy<BpeTokenizer> SharedTokenizer = new(() =>
    {
        // Тот же словарь и тот же размер 320, что и у существующей проверки правил ранжирования.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "Задание продолжи действие Префикс каждый день вчера завтра прошлой ночью я он хожу ходил пойду спал текст запрос",
            320);
        return tokenizer;
    });

    // Одна и та же крошечная модель на весь класс: обучение модуля её не меняет,
    // оно только считает по ней векторы.
    private static readonly Lazy<LlmModel> SharedModel = new(() => new LlmModel(new ModelConfig
    {
        VocabSize = 512,
        ContextLen = 64,
        HiddenDim = 16,
        NumLayers = 1,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 32
    }));

    // Главная проверка: после одного обучения памяти из одной строки ни правка
    // массивов у исходного вызывающего, ни правка массивов первого ответа не
    // меняют того, что модуль отвечает дальше — ни на родном порядке, ни на
    // порядке с подключённым каталогом правил. Переобучения и восстановления
    // после атаки нет.
    [Fact]
    public async Task OwnedMetadata_SurvivesCallerMutationAndResultMutation_OnNativeAndRankedAnswers()
    {
        // Массивы, которые остаются на руках у вызывающей стороны. Оба непустые.
        var callerPredicateIds = new[] { OriginalPredicate };
        var callerDomainTags = new[] { OriginalDomainTag };

        // Обучение одной строки: одно предъявление, одна запись в индекс.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "knn-metadata-ownership",
            "generated-capability",
            OriginalDomainTag,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<string>(
                OriginalExampleId,
                OriginalText,
                OriginalOutput,
                callerPredicateIds,
                callerDomainTags)]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        var unit = trained.Value!;

        // Тот же обученный модуль с подключённым каталогом правил: память одна и та же,
        // переобучения нет, меняется только порядок разбора кандидатов.
        var catalog = KnnCandidatePrincipleCatalog.Create(
            OriginalDomainTag,
            [KnnDomainCandidatePrincipleProfiles.RussianTimeFrame()]);
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        var ranked = unit.WithCandidatePrinciples(catalog.Value!);
        Assert.True(ranked.IsSuccess, ranked.ErrorMessage);
        var rankedUnit = ranked.Value!;

        // Обращение 1 из 6: родной порядок до любой атаки.
        var first = await AnswerAsync(unit);
        AssertOriginalMetadata(first, "родной ответ до атаки");

        // Атака 1: исходный вызывающий меняет оба своих непустых массива.
        callerPredicateIds[0] = "PRED-ATTACKED-BY-CALLER";
        callerDomainTags[0] = "ATTACKED-BY-CALLER";

        // Обращение 2 из 6: память не должна была сдвинуться вместе с массивами вызывающего.
        var second = await AnswerAsync(unit);
        AssertOriginalMetadata(second, "родной ответ после правки массивов вызывающего");
        // Каждый ответ несёт свои собственные массивы: общий объект на два ответа
        // снова открыл бы ту же дверь, которую закрывает этот перенос.
        Assert.NotSame(first.PredicateIds, second.PredicateIds);
        Assert.NotSame(first.DomainTags, second.DomainTags);

        // Атака 2: получатель меняет массивы уже выданного первого ответа.
        // Эта атака ловит случай, когда копия сделана только на входе, а наружу
        // по-прежнему отдают саму память.
        first.PredicateIds[0] = "PRED-ATTACKED-BY-RESULT";
        first.DomainTags[0] = "ATTACKED-BY-RESULT";

        // Обращение 3 из 6: память не должна была сдвинуться вместе с первым ответом.
        AssertOriginalMetadata(await AnswerAsync(unit), "родной ответ после правки массивов первого ответа");

        // Обращение 4 из 6: порядок с каталогом правил отвечает из той же неизменной памяти.
        var rankedAnswer = await AnswerAsync(rankedUnit);
        AssertOriginalMetadata(rankedAnswer, "ответ с каталогом правил после обеих атак");

        // Атака 3: получатель меняет массивы ответа, выданного порядком с каталогом правил.
        rankedAnswer.PredicateIds[0] = "PRED-ATTACKED-BY-RANKED-RESULT";
        rankedAnswer.DomainTags[0] = "ATTACKED-BY-RANKED-RESULT";

        // Обращение 5 из 6: порядок с каталогом правил по-прежнему отвечает исходными метками.
        AssertOriginalMetadata(await AnswerAsync(rankedUnit), "ответ с каталогом правил после правки его же ответа");

        // Обращение 6 из 6: родной порядок после всех трёх атак, без переобучения и восстановления.
        AssertOriginalMetadata(await AnswerAsync(unit), "родной ответ после всех атак");

        // Испорченными оказались именно копии, а не исходные значения: это
        // доказывает, что атаки действительно меняли массивы, а не писали мимо.
        Assert.Equal("PRED-ATTACKED-BY-CALLER", callerPredicateIds[0]);
        Assert.Equal("PRED-ATTACKED-BY-RESULT", first.PredicateIds[0]);
        Assert.Equal("PRED-ATTACKED-BY-RANKED-RESULT", rankedAnswer.PredicateIds[0]);
    }

    // Отсутствующий носитель меток отвергается через общий конверт результата,
    // и частично собранный модуль наружу не выдаётся.
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MissingMetadataCarrier_IsRefusedThroughTheEnvelope(bool predicateIdsMissing, bool domainTagsMissing)
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            "knn-metadata-ownership-null",
            "generated-capability",
            OriginalDomainTag,
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<string>(
                OriginalExampleId,
                OriginalText,
                OriginalOutput,
                predicateIdsMissing ? null! : [OriginalPredicate],
                domainTagsMissing ? null! : [OriginalDomainTag])]);

        // Отказ приходит обычным конвертом, а не исключением.
        Assert.False(trained.IsSuccess);
        Assert.Equal("INVALID_TRAINING_EXAMPLE", trained.ErrorCode);
        // Частично собранного модуля наружу не выдано.
        Assert.Null(trained.Value);
    }

    // Один обычный вопрос к модулю через обычный конверт запроса.
    private static async Task<LogicalUnitResult<string>> AnswerAsync(TransformerEmbeddingKnnLogicalUnit<string> unit)
    {
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-knn-metadata-ownership",
            "knn-metadata-ownership",
            "generated-capability",
            OriginalDomainTag,
            "Задание: продолжи действие. Префикс: каждый день он",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        return answer.Value!;
    }

    // Ответ обязан нести исходные метки и исходный номер строки памяти.
    private static void AssertOriginalMetadata(LogicalUnitResult<string> answer, string moment)
    {
        Assert.Equal(OriginalPredicate, Assert.Single(answer.PredicateIds));
        Assert.Equal(OriginalDomainTag, Assert.Single(answer.DomainTags));
        // Та же самая строка памяти и тот же самый её ответ, иначе сравнение меток ничего не значит.
        Assert.Equal(OriginalOutput, answer.Output);
        Assert.Contains(OriginalExampleId, answer.EvidenceIds);
        // Момент атаки назван в сообщении, чтобы падение сразу показывало, какая дверь открылась.
        Assert.True(answer.PredicateIds[0] == OriginalPredicate, $"Метки сдвинулись: {moment}.");
    }
}
