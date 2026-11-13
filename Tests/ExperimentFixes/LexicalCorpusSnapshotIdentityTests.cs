// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Опознание снимка словесной памяти обученной единицы: отсутствующий ответ урока и
// явно заданный пустой ответ обязаны быть РАЗНЫМ содержимым корпуса.
//
// Покрытие здесь намеренно МИНИМАЛЬНОЕ: одна проверка инварианта на двух корпусах по
// одному уроку. Это прямое указание владельца — логика переносится по уже доказанным
// опытам, заново их не доказываем и в побитовые сверки не уходим.
//
// Что было неправильно. Описание ответа урока для отпечатка содержимого
// (18.CapabilityTraining/CorpusStatisticsSnapshot.cs, метод DescribeOutput) сводило
// отсутствующий ответ и явно заданный пустой ответ к одной и той же пустой строке.
// Поток отпечатка объявляет длину каждого поля до его байтов, поэтому разделитель
// внутри значения границы полей не сдвигает, но различить нуль-длину от нуль-длины
// длина не может. Значит два разных обученных корпуса давали один отпечаток, и
// проверка происхождения снимка (CorpusStatisticsSnapshot.RequireBelongsTo)
// признавала чужой снимок своим.
//
// Чем это опасно продукту. Отпечаток содержимого — единственная живая проверка
// происхождения снимка, и её продуктовый вызов стоит на подключении каталога правил
// ранжирования к уже обученной единице
// (18.CapabilityTraining/LlmIngredientLogicalUnits.cs, вызов
// RequireBelongsTo(DescribeCorpus()) внутри WithCandidatePrinciples). Признав чужой
// снимок своим, единица продолжала бы считать словесную оценку по средней длине
// документа и частоте слов ДРУГОГО корпуса.
//
// Почему текст этой проверки одинаков до и после починки. Материал собирается тем же
// единственным описателем ответа, которым его собирает продукт
// (LexicalCorpusStatistics.DescribeOutput), а не руками. Поэтому исходный текст
// компилируется и в прежнем состоянии продукта, и в исправленном: меняется только
// продукт, а не формулировка ожидания. Это условие честного красно-зелёного перехода.
//
// Чего эта проверка НЕ доказывает. Она не измеряет пользу опыта сто пятнадцать
// (приведений текста к корням, байт, правильных ответов) — это измерение опыта, и
// здесь оно не перепроверяется. Она не доказывает, что словесная оценка, частоты
// слов, средняя длина или проба правила приведения к корням изменились: они и не
// должны меняться. Случай «ответ есть, но его текстовая форма вернула ничего» этой
// проверкой не покрыт: он разведён в самом коде отдельным полем присутствия ответа,
// и отдельной проверки на него по указанию о минимальном покрытии здесь нет.
public sealed class LexicalCorpusSnapshotIdentityTests
{
    // Один факт проекта Cedar: опознание строки памяти и её текст у обеих версий
    // корпуса одинаковы, чтобы единственным различием остался ответ урока.
    private const string LessonId = "DOC-01";

    // Текст урока, на котором обычный читатель и будет отвечать.
    private const string LessonText = "Cedar owner";

    // Один обученный словарь кусочков слов на весь класс: обучение памяти единицы его
    // не меняет, поэтому повторная сборка словаря была бы чистой тратой времени.
    private static readonly Lazy<BpeTokenizer> SharedTokenizer = new(() =>
    {
        // Словарь собирается на словах, которые встречаются в уроке этого класса.
        var tokenizer = new BpeTokenizer();
        // Небольшой словарь: он нужен только чтобы посчитать вектор текста.
        tokenizer.Train("Cedar owner status release language approved cancelled project fact question", 320);
        // Готовый словарь кусочков слов.
        return tokenizer;
    });

    // Одна крошечная модель на весь класс: обучение единицы её не меняет, по ней
    // только считаются векторы текста.
    private static readonly Lazy<LlmModel> SharedModel = new(() => new LlmModel(new ModelConfig
    {
        VocabSize = 512,   // Предел словаря модели.
        ContextLen = 64,   // Предел длины входа модели.
        HiddenDim = 16,    // Размер вектора текста.
        NumLayers = 1,     // Один слой: модель здесь только считает векторы.
        NumHeads = 4,      // Число голов внимания.
        NumKvHeads = 2,    // Число голов ключей и значений.
        FfnHiddenDim = 32  // Размер внутреннего слоя.
    }));

    // Единственная проверка инварианта, через обычное обучение настоящими классами:
    // словарь кусочков слов, модель, вектор текста, указатель близких векторов и
    // обычный конверт запроса — всё настоящее, заглушек на исправленном пути нет.
    //
    // Названная граница: подменить единице чужой снимок на продуктовом вызове нельзя,
    // потому что и память, и её снимок закрыты внутри единицы, а описание памяти
    // собирается ею самой. Поэтому приём своего снимка доказан на продуктовом вызове
    // переиспользования, а отказ чужому — на самой проверке происхождения, с
    // материалом, собранным тем же единственным описателем ответа, которым его
    // собирает продукт.
    [Fact]
    public async Task AbsentOutputAndExplicitlyEmptyOutput_AreDifferentCorpusVersions()
    {
        // Единица, обученная уроку БЕЗ ответа, и единица, обученная тому же тексту с
        // явно заданным ПУСТЫМ ответом. Обычный читатель отвечает у них по-разному:
        // это и доказывает, что две обученные памяти действительно разные.
        Assert.Null(await AnswerAsync(TrainedUnit("snapshot-identity-absent-output", null), LessonText));
        var emptyOutputUnit = TrainedUnit("snapshot-identity-empty-output", string.Empty);
        Assert.Equal(string.Empty, await AnswerAsync(emptyOutputUnit, LessonText));

        // Два снимка тех же двух версий корпуса, собранные тем же единственным
        // описателем ответа, которым их собирает обучение.
        var absentOutput = BuiltStatistics(CorpusWithAbsentOutput());
        var emptyOutput = BuiltStatistics(CorpusWithExplicitlyEmptyOutput());

        // Опознания строк памяти и тексты одинаковы, поэтому ни число документов, ни
        // средняя длина различить версии не могут: различие обязано прийти от ответа.
        Assert.Equal(absentOutput.DocumentCount, emptyOutput.DocumentCount);
        Assert.Equal(absentOutput.AverageLength, emptyOutput.AverageLength);

        // Сам инвариант: отпечаток содержимого и опознание версии корпуса различаются.
        Assert.NotEqual(absentOutput.CorpusContentHash, emptyOutput.CorpusContentHash);
        Assert.NotEqual(absentOutput.CorpusVersionKey, emptyOutput.CorpusVersionKey);

        // Чужой снимок отвергается своим названным кодом, и негодного снимка наружу
        // не выдано.
        var refused = absentOutput.RequireBelongsTo(CorpusWithExplicitlyEmptyOutput());
        Assert.False(refused.IsSuccess);
        Assert.Equal(LexicalCorpusStatistics.ContentMismatchErrorCode, refused.ErrorCode);
        Assert.Null(refused.Value);

        // Положительный контроль, зелёный и до починки, и после: свой снимок
        // принимается. Без него нельзя отличить «проверка поймала дефект» от
        // «проверка отвергает всё подряд».
        var accepted = absentOutput.RequireBelongsTo(CorpusWithAbsentOutput());
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Same(absentOutput, accepted.Value);

        // Тот же положительный контроль на продуктовом вызове переиспользования
        // снимка: подключение каталога правил ранжирования заново доказывает
        // происхождение снимка по отпечатку содержимого.
        var catalog = KnnCandidatePrincipleCatalog.CreateEmpty("documentation");
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        var reused = emptyOutputUnit.WithCandidatePrinciples(catalog.Value!);
        Assert.True(reused.IsSuccess, reused.ErrorMessage);
    }

    // Корпус из одного урока, у которого ответа НЕТ вовсе.
    private static LexicalCorpusDocument[] CorpusWithAbsentOutput()
        =>
        [
            new LexicalCorpusDocument(
                LessonId,
                LessonText,
                // Отсутствующий ответ описывается тем же единственным описателем.
                LexicalCorpusStatistics.DescribeOutput<string?>(null))
        ];

    // Корпус из одного урока, у которого ответ ЕСТЬ и он — явно заданная пустая строка.
    private static LexicalCorpusDocument[] CorpusWithExplicitlyEmptyOutput()
        =>
        [
            new LexicalCorpusDocument(
                LessonId,
                LessonText,
                // Явно заданный пустой ответ описывается тем же единственным описателем.
                LexicalCorpusStatistics.DescribeOutput<string?>(string.Empty))
        ];

    // Собранный снимок или падение с человеческим сообщением отказа.
    private static LexicalCorpusStatistics BuiltStatistics(LexicalCorpusDocument[] documents)
    {
        // Сборка снимка возвращается общим конвертом результата.
        var built = LexicalCorpusStatistics.Build(documents);
        // Отказ сборки сообщается своим сообщением, а не падением без объяснения.
        Assert.True(built.IsSuccess, built.ErrorMessage);
        // Готовый снимок одной версии корпуса.
        return built.Value!;
    }

    // Единица, обученная ОДНОМУ уроку с заданным ответом: обычное обучение, без
    // переобучения, восстановления и заглушек.
    private static TransformerEmbeddingKnnLogicalUnit<string> TrainedUnit(string unitId, string? output)
    {
        // Обучение идёт настоящим путём обучения этой единицы.
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            unitId,
            "generated-capability",
            "documentation",
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<string>(
                LessonId,
                LessonText,
                output!,
                ["PRED-DOC-FACT"],
                ["documentation"])]);
        // Отказ обучения сообщается своим сообщением.
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // Обученная единица.
        return trained.Value!;
    }

    // Один обычный вопрос к единице через обычный конверт запроса.
    private static async Task<string> AnswerAsync(TransformerEmbeddingKnnLogicalUnit<string> unit, string question)
    {
        // Запрос собирается ровно так же, как его собирает продукт.
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-lexical-corpus-snapshot-identity",
            "lexical-corpus-snapshot-identity",
            "generated-capability",
            "documentation",
            question,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        // Отказ выдачи ответа сообщается своим сообщением.
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        // Сам ответ урока в том виде, в каком его отдаёт единица наружу.
        return answer.Value!.Output;
    }
}
