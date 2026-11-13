// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inference;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class CacheAwareForwardTests
{
    [Fact]
    public void ForwardWithCache_EmptyPrefillMatchesOrdinaryForward()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);
        var tokenIds = TokenIds(1, 2, 3, 4);
        using var cache = new KvCache(config.NumLayers);

        var expected = model.Forward(tokenIds, training: false);
        var actual = model.ForwardWithCache(tokenIds, cache);

        AssertTensorClose(expected, actual);
        Assert.Equal(4, cache.GetUniformSequenceLength());
    }

    [Fact]
    public void ForwardWithCache_ChunkedGqaLogitsMatchTheSameAbsoluteFullContextPositions()
    {
        var config = TinyConfig();
        Assert.True(config.NumHeads > config.NumKvHeads);
        var model = new LlmModel(config);
        var allTokens = TokenIds(1, 2, 3, 4, 5);
        using var cache = new KvCache(config.NumLayers);

        var full = model.Forward(allTokens, training: false);
        var first = model.ForwardWithCache(TokenIds(1, 2), cache);
        var second = model.ForwardWithCache(TokenIds(3, 4), cache);
        var third = model.ForwardWithCache(TokenIds(5), cache);

        AssertLogitWindowClose(full, first, firstFullPosition: 0);
        AssertLogitWindowClose(full, second, firstFullPosition: 2);
        AssertLogitWindowClose(full, third, firstFullPosition: 4);
        Assert.Equal(5, cache.GetUniformSequenceLength());
    }

    [Fact]
    public void ForwardWithCache_VisitsLayersInOrderAndStoresUnrepeatedKvHeads()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);
        var cache = new RecordingCache(config.NumLayers);

        _ = model.ForwardWithCache(TokenIds(1, 2), cache);

        Assert.Equal(Enumerable.Range(0, config.NumLayers), cache.UpdatedLayers);
        Assert.All(cache.UpdatedKeyHeadCounts, heads => Assert.Equal(config.NumKvHeads, heads));
        Assert.All(cache.UpdatedKeyShapes, shape =>
            Assert.Equal(new[] { 1, config.NumKvHeads, 2, config.HeadDim }, shape));
    }

    [Fact]
    public void ForwardWithCache_PreflightRejectionsPreserveTheExistingCache()
    {
        var config = TinyConfig(contextLength: 4);
        var model = new LlmModel(config);

        var rankCache = SeededCache(config, sequenceLength: 1);
        var rankException = Assert.Throws<ArgumentException>(() =>
            model.ForwardWithCache(new Tensor(new float[1], new TensorShape(1, 1, 1)), rankCache));
        Assert.Equal("LLM_KV_TOKEN_IDS_RANK_INVALID", rankException.Message);
        AssertCacheUnchanged(rankCache, 1);

        var layerCountCache = new RecordingCache(layerCount: config.NumLayers - 1);
        var layerCountException = Assert.Throws<InvalidOperationException>(() =>
            model.ForwardWithCache(TokenIds(1), layerCountCache));
        Assert.Equal("LLM_KV_CACHE_LAYER_COUNT_MISMATCH", layerCountException.Message);
        AssertCacheUnchanged(layerCountCache, 0);

        var skewedCache = new RecordingCache(config.NumLayers);
        skewedCache.SeedLayer(0, sequenceLength: 1, config);
        var skewedException = Assert.Throws<InvalidOperationException>(() =>
            model.ForwardWithCache(TokenIds(1), skewedCache));
        Assert.Equal("TEST_CACHE_SKEWED", skewedException.Message);
        Assert.Equal(new[] { 1, 0 }, skewedCache.SequenceLengths);
        Assert.Equal(0, skewedCache.UpdateCount);
        Assert.Equal(0, skewedCache.ClearCount);

        var overflowCache = SeededCache(config, sequenceLength: config.ContextLen);
        var overflowException = Assert.Throws<InvalidOperationException>(() =>
            model.ForwardWithCache(TokenIds(1), overflowCache));
        Assert.Equal("LLM_KV_CONTEXT_LENGTH_EXCEEDED", overflowException.Message);
        AssertCacheUnchanged(overflowCache, config.ContextLen);

        var preCancelledCache = SeededCache(config, sequenceLength: 1);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            model.ForwardWithCache(TokenIds(1), preCancelledCache, cancellation.Token));
        AssertCacheUnchanged(preCancelledCache, 1);

        var nullTokenCache = SeededCache(config, sequenceLength: 1);
        Assert.Throws<ArgumentNullException>(() => model.ForwardWithCache(null!, nullTokenCache));
        AssertCacheUnchanged(nullTokenCache, 1);
    }

    [Fact]
    public void ForwardWithCache_CancellationAfterFirstOrLastUpdateClearsEveryLayer()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);

        using (var cancellation = new CancellationTokenSource())
        {
            var cache = new RecordingCache(config.NumLayers, cancellation, cancelAfterUpdate: 1);
            Assert.Throws<OperationCanceledException>(() =>
                model.ForwardWithCache(TokenIds(1), cache, cancellation.Token));
            AssertCacheWasCleared(cache);
        }

        using (var cancellation = new CancellationTokenSource())
        {
            var cache = new RecordingCache(
                config.NumLayers,
                cancellation,
                cancelAfterUpdate: config.NumLayers);
            Assert.Throws<OperationCanceledException>(() =>
                model.ForwardWithCache(TokenIds(1), cache, cancellation.Token));
            AssertCacheWasCleared(cache);
        }
    }

    [Fact]
    public void ForwardWithCache_UpdateFailureClearsAndPreservesTheOperationException()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);
        var cache = new RecordingCache(config.NumLayers, throwOnUpdate: 2);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            model.ForwardWithCache(TokenIds(1), cache));

        Assert.Equal("TEST_UPDATE_FAILURE", exception.Message);
        AssertCacheWasCleared(cache);
    }

    [Fact]
    public void ForwardWithCache_ClearFailureAggregatesOperationAndResetFailures()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);
        var cache = new RecordingCache(config.NumLayers, throwOnUpdate: 1, throwOnClear: true);

        var exception = Assert.Throws<AggregateException>(() =>
            model.ForwardWithCache(TokenIds(1), cache));

        Assert.Contains("LLM_KV_CACHE_RESET_FAILED", exception.Message, StringComparison.Ordinal);
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Equal("TEST_UPDATE_FAILURE", exception.InnerExceptions[0].Message);
        Assert.Equal("TEST_CLEAR_FAILURE", exception.InnerExceptions[1].Message);
        Assert.Equal(1, cache.ClearCount);
    }

    [Fact]
    public void ForwardWithCache_FinalLengthMismatchClearsTheCache()
    {
        var config = TinyConfig();
        var model = new LlmModel(config);
        var cache = new RecordingCache(config.NumLayers) { ReportedUniformLength = 0 };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            model.ForwardWithCache(TokenIds(1), cache));

        Assert.Equal("LLM_KV_CACHE_FINAL_LENGTH_MISMATCH", exception.Message);
        AssertCacheWasCleared(cache);
    }

    [Fact]
    public void Forward_DefaultLiteralRemainsTheInferenceBooleanOverload()
    {
        var model = new LlmModel(TinyConfig());
        var tokenIds = TokenIds(1, 2);

        var defaultLiteral = model.Forward(tokenIds, default);
        var inference = model.Forward(tokenIds, training: false);

        AssertTensorClose(inference, defaultLiteral);
    }

    // ── Усиление уже существующего покрытия настоящим обученным состоянием ─────────
    //
    // Две проверки выше работают на СЛУЧАЙНО созданной крошечной модели: они
    // доказывают сам механизм кэша (пустое предзаполнение повторяет обычный проход,
    // куски повторяют те же абсолютные позиции полного контекста), но ничего не
    // говорят о сохранении уже выученного ответа. Две проверки ниже закрывают
    // именно этот разрыв и держат ДВА РАЗНЫХ уровня доказательства раздельно:
    //
    //   уровень КВИТАНЦИИ — читается файл чисел, который опыт о кэшированном чтении
    //     обученного состояния записал на диск; модель при этом не вызывается ни
    //     разу. Такая проверка подтверждает только сохранённую запись;
    //
    //   уровень НАСТОЯЩЕЙ МОДЕЛИ — штатное обученное состояние восстанавливается с
    //     диска, и оба продуктовых входа чтения вызываются заново. Такая проверка
    //     подтверждает сегодняшнего потребителя, а не прошлую запись.
    //
    // Смешивать их нельзя: совпадение чисел в файле не доказывает, что продукт
    // сегодня даёт то же самое, а совпадение двух живых путей не доказывает, что
    // сохранённая квитанция честна.

    // Уровень КВИТАНЦИИ. Сверяются числа обоих плеч, уже записанные опытом на диск.
    // Настоящая модель здесь не вызывается: ноль прямых проходов, ноль обновлений.
    [Fact]
    public void SavedTrainedStateReadingReceipt_FullForwardAndCachePrefillNumbersAreIdentical()
    {
        // Сохранённая квитанция опыта: полные числа всех позиций обоих плеч.
        using var receipt = JsonDocument.Parse(
            File.ReadAllBytes(TrainedStateReadingArtifact("arms-logits.json")));
        // Ширина словаря квитанции обязана совпасть с паспортной: 256 кандидатов на позицию.
        Assert.Equal(TrainedStateReadingVocabularySize, receipt.RootElement.GetProperty("vocabulary_size").GetInt32());

        // Плечо полного прямого прохода, записанное опытом.
        var fullForwardRows = receipt.RootElement.GetProperty("arm_a").EnumerateArray().ToArray();
        // Плечо предзаполнения пустого кэша, записанное тем же опытом.
        var cachePrefillRows = receipt.RootElement.GetProperty("arm_b").EnumerateArray().ToArray();
        // Панель опыта состоит ровно из четырёх строк, и в каждом плече их столько же.
        Assert.Equal(TrainedStateReadingRowIds.Length, fullForwardRows.Length);
        Assert.Equal(TrainedStateReadingRowIds.Length, cachePrefillRows.Length);

        var comparedValueCount = 0; // Сколько чисел фактически сверено между плечами.
        var differingValueCount = 0; // Сколько чисел разошлись хотя бы одним битом.
        foreach (var rowIdentifier in TrainedStateReadingRowIds)
        {
            // Строка каждого плеча ищется по своему обозначению, а не по месту в файле.
            var fullForwardRow = fullForwardRows.Single(row => row.GetProperty("row_id").GetString() == rowIdentifier);
            var cachePrefillRow = cachePrefillRows.Single(row => row.GetProperty("row_id").GetString() == rowIdentifier);

            // У полного прохода длины кэша нет вовсе: хранилище в этом плече не участвовало.
            Assert.Equal(JsonValueKind.Null, fullForwardRow.GetProperty("cache_length_before").ValueKind);
            Assert.Equal(JsonValueKind.Null, fullForwardRow.GetProperty("cache_length_after").ValueKind);
            // У пути кэша длина обязана быть нулевой в начале и равной длине входа в конце.
            Assert.Equal(0, cachePrefillRow.GetProperty("cache_length_before").GetInt32());
            Assert.Equal(TrainedStateReadingPromptLength, cachePrefillRow.GetProperty("cache_length_after").GetInt32());
            // Главный кандидат последней позиции — это и есть односимвольный ответ строки.
            Assert.Equal(
                fullForwardRow.GetProperty("last_position_argmax").GetInt32(),
                cachePrefillRow.GetProperty("last_position_argmax").GetInt32());

            var fullForwardPositions = fullForwardRow.GetProperty("logits_by_position").EnumerateArray().ToArray();
            var cachePrefillPositions = cachePrefillRow.GetProperty("logits_by_position").EnumerateArray().ToArray();
            // Позиций в квитанции ровно столько, сколько знаков во входе строки.
            Assert.Equal(TrainedStateReadingPromptLength, fullForwardPositions.Length);
            Assert.Equal(TrainedStateReadingPromptLength, cachePrefillPositions.Length);

            for (var position = 0; position < fullForwardPositions.Length; position++)
            {
                // Числа позиции переносятся из файла без округления и без пересчёта.
                var fullForwardValues = fullForwardPositions[position].EnumerateArray().Select(value => value.GetSingle()).ToArray();
                var cachePrefillValues = cachePrefillPositions[position].EnumerateArray().Select(value => value.GetSingle()).ToArray();
                // На каждой позиции обязаны стоять все 256 кандидатов словаря.
                Assert.Equal(TrainedStateReadingVocabularySize, fullForwardValues.Length);
                Assert.Equal(TrainedStateReadingVocabularySize, cachePrefillValues.Length);
                for (var candidate = 0; candidate < fullForwardValues.Length; candidate++)
                {
                    comparedValueCount++; // Ещё одно сверенное число квитанции.
                    // Сравнение побитовое: ни допуска, ни округления здесь нет вовсе.
                    if (BitConverter.SingleToInt32Bits(fullForwardValues[candidate])
                        != BitConverter.SingleToInt32Bits(cachePrefillValues[candidate]))
                        differingValueCount++;
                }
            }
        }

        // Квитанция обязана содержать ровно 21 504 числа: 4 строки × 21 позиция × 256 кандидатов.
        Assert.Equal(TrainedStateReadingReceiptValueCount, comparedValueCount);
        // Ни одно сверенное число не разошлось — это и есть содержание сохранённой квитанции.
        Assert.Equal(0, differingValueCount);
    }

    // Уровень НАСТОЯЩЕЙ МОДЕЛИ. Восстанавливается штатное обученное состояние, и оба
    // продуктовых входа чтения вызываются заново на тех же четырёх исходных запросах.
    //
    // Происхождение восстанавливаемого состояния (оно же проверяется ниже по фактам,
    // а не принимается на слово): четыре урока обычного учительского обучения в
    // порядке «первая учебная строка, вторая, первая, вторая»; счётчик шага
    // оптимизатора 400 → 404; изменились ровно четыре обучаемых массива адаптеров
    // запроса и значения одного блока; ответ первой учебной строки удержан — он был
    // верным до вмешательства и остался верным после.
    [Fact]
    public void RestoredTrainedStateOfTheReadingExperiment_EmptyCachePrefillMatchesOrdinaryForward()
    {
        // ── Происхождение состояния: что именно записал опыт о своём обучении ──────
        using var savedMetrics = JsonDocument.Parse(File.ReadAllBytes(TrainedStateReadingArtifact("metrics.json")));
        var savedLearning = savedMetrics.RootElement.GetProperty("learning");
        // Четыре урока обычного учительского обучения, порядок закреплён рецептом.
        Assert.Equal(4, savedLearning.GetProperty("updates_applied").GetInt32());
        Assert.Equal(
            new[] { "T1", "T2", "T1", "T2" },
            savedLearning.GetProperty("order").EnumerateArray().Select(value => value.GetString()).ToArray());
        // Счётчик шага оптимизатора прошёл путь 400 → 404: по одному шагу на урок.
        Assert.Equal(400, savedLearning.GetProperty("optimizer_step_before").GetInt32());
        Assert.Equal(404, savedLearning.GetProperty("optimizer_step_after").GetInt32());
        // Изменились ровно четыре обучаемых массива адаптеров запроса и значения.
        var changedArrays = savedLearning.GetProperty("weight_deltas").EnumerateArray()
            .Where(entry => entry.GetProperty("changed").GetBoolean())
            .Select(entry => entry.GetProperty("name").GetString())
            .ToArray();
        Assert.Equal(TrainedStateReadingAdapterArrayNames, changedArrays);
        // Ответ первой учебной строки был верным до уроков и остался верным после.
        Assert.True(savedLearning.GetProperty("acquired_t1_before").GetBoolean());
        Assert.True(savedLearning.GetProperty("acquired_t1_after").GetBoolean());

        // Счётчик шага читается ещё и из самих сохранённых байтов моментов обоих состояний:
        // начальное состояние лежит в общем сохранённом старте, конечное — в этой попытке.
        var startOptimizerState = AdamWPortableState.FromBytes(
            File.ReadAllBytes(Path.Combine(SharedLearnedStartDirectory(), "S0.optimizer.bin")));
        var trainedOptimizerState = AdamWPortableState.FromBytes(
            File.ReadAllBytes(TrainedStateReadingArtifact("S1.optimizer.bin")));
        Assert.NotNull(startOptimizerState);
        Assert.NotNull(trainedOptimizerState);
        Assert.Equal(400, startOptimizerState!.Step);
        Assert.Equal(404, trainedOptimizerState!.Step);

        // Сами байты обученных весов обязаны быть теми же, что записал опыт.
        var trainedWeightsPath = TrainedStateReadingArtifact("S1.weights.bin");
        Assert.Equal(
            savedLearning.GetProperty("saved_state").GetProperty("weights_sha256").GetString(),
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trainedWeightsPath))).ToLowerInvariant());

        // ── Восстановление штатного обученного состояния ──────────────────────────
        // Численная конфигурация берётся из описания порождения общего старта.
        using var generationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(SharedLearnedStartDirectory(), "generation.json")));
        var modelBlock = generationDocument.RootElement.GetProperty("model");
        var config = new ModelConfig
        {
            VocabSize = modelBlock.GetProperty("VocabSize").GetInt32(), // Размер словаря.
            ContextLen = modelBlock.GetProperty("ContextLen").GetInt32(), // Длина окна внимания.
            HiddenDim = modelBlock.GetProperty("HiddenDim").GetInt32(), // Ширина скрытого слоя.
            NumLayers = modelBlock.GetProperty("NumLayers").GetInt32(), // Число слоёв.
            NumHeads = modelBlock.GetProperty("NumHeads").GetInt32(), // Число голов запроса.
            NumKvHeads = modelBlock.GetProperty("NumKvHeads").GetInt32(), // Число голов ключей и значений.
            FfnHiddenDim = modelBlock.GetProperty("FfnHiddenDim").GetInt32(), // Ширина полносвязного слоя.
            RopeTheta = modelBlock.GetProperty("RopeTheta").GetSingle(), // Основание поворотных позиций.
            RmsNormEps = modelBlock.GetProperty("RmsNormEps").GetSingle() // Сглаживание нормировки.
        };

        LlmModel model; // Модель объявлена снаружи блока зерна, чтобы жить дальше.
        // Замороженная основа строится тем же зерном, которым её строил производитель набора.
        using (Initializers.UseDeterministicSeed(TrainedStateReadingBaseSeed))
            model = new LlmModel(config);
        // Адаптеры запроса и значения прикладываются тем же зерном и тем же потоком.
        LoraApplicator.ApplySeeded(
            model,
            new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] },
            TrainedStateReadingAdapterSeed,
            TrainedStateReadingAdapterStream);
        // Поверх начальных значений кладутся сохранённые обученные веса этой попытки.
        model.LoadCheckpoint(trainedWeightsPath);

        // Обучаемыми обязаны остаться ровно четыре названных массива адаптеров.
        Assert.Equal(
            TrainedStateReadingAdapterArrayNames,
            model.GetCheckpointParameterEntries()
                .Where(entry => entry.Parameter.Value.RequiresGrad)
                .Select(entry => entry.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray());

        // Отпечаток восстановленных обучаемых чисел обязан совпасть с записанным опытом:
        // это доказательство, что читается именно обученное состояние, а не чужое.
        var trainableParameters = model.Parameters().Where(parameter => parameter.Value.RequiresGrad).ToArray();
        var trainableHashBeforeReading = ParametersHash(trainableParameters);
        Assert.Equal(savedLearning.GetProperty("trainable_after_sha256").GetString(), trainableHashBeforeReading);

        // ── Те же четыре исходных запроса, что читал опыт ──────────────────────────
        using var savedGold = JsonDocument.Parse(File.ReadAllBytes(TrainedStateReadingArtifact("gold-oracle.json")));
        var promptByRowId = savedGold.RootElement.GetProperty("rows").EnumerateArray()
            .ToDictionary(row => row.GetProperty("id").GetString()!, row => row.GetProperty("prompt").GetString()!);
        using var savedReceipt = JsonDocument.Parse(File.ReadAllBytes(TrainedStateReadingArtifact("arms-logits.json")));
        var receiptFullForwardRows = savedReceipt.RootElement.GetProperty("arm_a").EnumerateArray().ToArray();

        var comparedValueCount = 0; // Сколько живых чисел сверено между двумя путями.
        var differingValueCount = 0; // Сколько живых чисел разошлись хотя бы одним битом.
        foreach (var rowIdentifier in TrainedStateReadingRowIds)
        {
            // Номера знаков считаются из самого текста запроса: словарь опыта побайтовый.
            var tokenIdentifiers = Encoding.UTF8.GetBytes(promptByRowId[rowIdentifier]).Select(value => (int)value).ToArray();
            // Длина запроса опыта — 21 знак; иначе читался бы другой вход.
            Assert.Equal(TrainedStateReadingPromptLength, tokenIdentifiers.Length);
            // Этот же вход записан в квитанции опыта: живой запрос и записанный совпадают.
            var receiptRow = receiptFullForwardRows.Single(row => row.GetProperty("row_id").GetString() == rowIdentifier);
            Assert.Equal(
                receiptRow.GetProperty("prompt_token_ids").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
                tokenIdentifiers);

            // Путь без кэша: весь запрос одним обычным прямым проходом.
            var ordinaryForward = model.Forward(TokenIds(tokenIdentifiers), training: false);
            // Путь кэша: тот же запрос одним вызовом, у строки свой только что созданный кэш.
            using var cache = new KvCache(config.NumLayers);
            // Сторож начала: до вызова хранилище обязано быть пустым.
            Assert.Equal(0, cache.GetUniformSequenceLength());
            var cachePrefillForward = model.ForwardWithCache(TokenIds(tokenIdentifiers), cache);
            // Сторож конца: после вызова длина обязана равняться длине входа, то есть 21.
            Assert.Equal(TrainedStateReadingPromptLength, cache.GetUniformSequenceLength());

            // Формы выходов обязаны совпасть до единого размера оси.
            Assert.Equal(ordinaryForward.Shape.Dimensions, cachePrefillForward.Shape.Dimensions);
            for (var index = 0; index < ordinaryForward.Data.Length; index++)
            {
                comparedValueCount++; // Ещё одно сверенное живое число.
                // Сравнение побитовое: сохранение уже выученного ответа проверяется без допуска.
                if (BitConverter.SingleToInt32Bits(ordinaryForward.Data[index])
                    != BitConverter.SingleToInt32Bits(cachePrefillForward.Data[index]))
                    differingValueCount++;
            }

            // Главный кандидат последней позиции — односимвольный ответ строки.
            var expectedAnswerTokenId = receiptRow.GetProperty("last_position_argmax").GetInt32();
            Assert.Equal(expectedAnswerTokenId, LastPositionArgmax(ordinaryForward, config.VocabSize));
            Assert.Equal(expectedAnswerTokenId, LastPositionArgmax(cachePrefillForward, config.VocabSize));
            // Удержание первой учебной строки: её верный ответ — знак «плюс», номер 43.
            if (rowIdentifier == "T1")
                Assert.Equal(TrainedStateReadingPlusTokenId, expectedAnswerTokenId);

            // ── Перенос R30-C1: тот же запрос тремя последовательными окнами ──────
            // Обученное состояние читается не одним куском, а по частям, как это
            // делает настоящее продолжение разговора: у строки свой отдельный пустой
            // кэш, тот же самый вход делится на 7/7/7 знаков, абсолютные начала
            // окон 0/7/14, а длина хранилища растёт 0 → 7 → 14 → 21. Каждое окно
            // сверяется с тем же куском обычного прохода, который уже посчитан выше;
            // второй обычный проход ради этого не делается.
            using var chunkedCache = new KvCache(config.NumLayers);
            Assert.Equal(0, chunkedCache.GetUniformSequenceLength()); // Сторож начала: хранилище пусто.

            Tensor? lastWindow = null; // Последнее окно: его хвост даёт ответ строки.
            var consumedTokens = 0; // Сколько знаков уже прочитано предыдущими окнами.
            foreach (var windowLength in TrainedStateReadingWindowLengths)
            {
                var windowTokenIdentifiers = tokenIdentifiers.Skip(consumedTokens).Take(windowLength).ToArray();
                Assert.Equal(windowLength, windowTokenIdentifiers.Length);

                var windowForward = model.ForwardWithCache(TokenIds(windowTokenIdentifiers), chunkedCache);
                // Числа окна обязаны совпасть с тем же куском обычного прохода.
                // Побитового тождества другого порядка вычислений здесь не требуется,
                // допуск задан заранее и при расхождении не ослабляется.
                AssertLogitWindowClose(ordinaryForward, windowForward, firstFullPosition: consumedTokens);

                consumedTokens += windowLength;
                // Длина хранилища после окна равна числу уже прочитанных знаков.
                Assert.Equal(consumedTokens, chunkedCache.GetUniformSequenceLength());
                lastWindow = windowForward;
            }

            // Три окна прочитали ровно весь запрос, не больше и не меньше.
            Assert.Equal(TrainedStateReadingPromptLength, consumedTokens);
            // Ответ по частям совпал с ответом одним куском и с записанным в квитанции.
            Assert.Equal(expectedAnswerTokenId, LastPositionArgmax(lastWindow!, config.VocabSize));
        }

        // Живых чисел сверено ровно столько же, сколько содержит квитанция опыта.
        Assert.Equal(TrainedStateReadingReceiptValueCount, comparedValueCount);
        // Ни одно живое число двух путей не разошлось на восстановленном обученном состоянии.
        Assert.Equal(0, differingValueCount);
        // Чтение не меняет обученное состояние: дополнительных обновлений здесь ноль.
        Assert.Equal(trainableHashBeforeReading, ParametersHash(trainableParameters));
    }

    // Обозначения четырёх строк панели опыта о кэшированном чтении обученного состояния.
    private static readonly string[] TrainedStateReadingRowIds = ["T1", "T2", "E1", "E2"];

    // Имена четырёх обучаемых массивов адаптеров запроса и значения одного блока.
    private static readonly string[] TrainedStateReadingAdapterArrayNames =
    [
        "lora.block_0000.q_proj.lora_a",
        "lora.block_0000.q_proj.lora_b",
        "lora.block_0000.v_proj.lora_a",
        "lora.block_0000.v_proj.lora_b"
    ];

    // Три последовательных окна чтения одного запроса: 7 + 7 + 7 знаков в сумме дают 21.
    private static readonly int[] TrainedStateReadingWindowLengths = [7, 7, 7];

    private const int TrainedStateReadingPromptLength = 21; // Длина каждого запроса опыта в знаках.
    private const int TrainedStateReadingVocabularySize = 256; // Ширина словаря опыта.
    private const int TrainedStateReadingReceiptValueCount = 21_504; // 4 строки × 21 позиция × 256 кандидатов.
    private const int TrainedStateReadingPlusTokenId = 43; // Номер знака «плюс» в побайтовом словаре.
    private const ulong TrainedStateReadingBaseSeed = 62001UL; // Зерно замороженной основы набора.
    private const ulong TrainedStateReadingAdapterSeed = 62002UL; // Зерно адаптеров набора.
    private const string TrainedStateReadingAdapterStream = "rms-row-reduce-probe-v1"; // Поток случайных чисел адаптеров.

    // Путь к сохранённому свидетельству попытки опыта о кэшированном чтении обученного состояния.
    private static string TrainedStateReadingArtifact(string fileName) => Path.Combine(
        FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "P0901", "results", "attempt-0001", fileName);

    // Путь к общему сохранённому обученному старту, из которого опыт начинал свои четыре урока.
    private static string SharedLearnedStartDirectory() => Path.Combine(
        FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "_shared-learned-starts", "h64-rms-row", "v1");

    // Поднимается от папки сборки до корня хранилища, чтобы пути не зависели от места запуска.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("CACHE_AWARE_FORWARD_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }

    // Номер самого вероятного кандидата на последней позиции выхода: это и есть ответ строки.
    private static int LastPositionArgmax(Tensor output, int vocabularySize)
    {
        var offset = output.Data.Length - vocabularySize; // Смещение последней позиции в плоских числах.
        var best = 0; // Номер самого вероятного кандидата.
        for (var candidate = 1; candidate < vocabularySize; candidate++)
            if (output.Data[offset + candidate] > output.Data[offset + best]) best = candidate;
        return best;
    }

    // Отпечаток набора обучаемых массивов: формы и все числа в объявленном порядке.
    // Повторяет ровно тот способ, которым опыт записал свой отпечаток обученных чисел.
    private static string ParametersHash(IEnumerable<Parameter> parameters)
    {
        using var stream = new MemoryStream(); // Буфер для форм и чисел.
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true); // Двоичная запись без преобразований.
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank); // Число осей массива.
            foreach (var dimension in parameter.Value.Shape.Dimensions) writer.Write(dimension); // Длина каждой оси.
            foreach (var value in parameter.Value.Data) writer.Write(value); // Все числа массива.
        }

        writer.Flush(); // Буфер дописывается до конца перед отпечатком.
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    private static ModelConfig TinyConfig(int contextLength = 8) => new()
    {
        VocabSize = 32,
        ContextLen = contextLength,
        HiddenDim = 16,
        NumLayers = 2,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 32
    };

    private static Tensor TokenIds(params int[] values) =>
        new(values.Select(value => (float)value).ToArray(), new TensorShape(1, values.Length));

    private static RecordingCache SeededCache(ModelConfig config, int sequenceLength)
    {
        var cache = new RecordingCache(config.NumLayers);
        for (var layer = 0; layer < config.NumLayers; layer++)
            cache.SeedLayer(layer, sequenceLength, config);
        return cache;
    }

    private static void AssertCacheUnchanged(RecordingCache cache, int expectedLength)
    {
        Assert.All(cache.SequenceLengths, length => Assert.Equal(expectedLength, length));
        Assert.Equal(0, cache.UpdateCount);
        Assert.Equal(0, cache.ClearCount);
    }

    private static void AssertCacheWasCleared(RecordingCache cache)
    {
        Assert.Equal(1, cache.ClearCount);
        Assert.All(cache.SequenceLengths, length => Assert.Equal(0, length));
    }

    private static void AssertLogitWindowClose(Tensor full, Tensor window, int firstFullPosition)
    {
        Assert.Equal(full.Shape.Dimensions[0], window.Shape.Dimensions[0]);
        Assert.Equal(full.Shape.Dimensions[2], window.Shape.Dimensions[2]);
        var vocabulary = full.Shape.Dimensions[2];
        for (var position = 0; position < window.Shape.Dimensions[1]; position++)
        for (var token = 0; token < vocabulary; token++)
        {
            AssertClose(
                full.Data[((firstFullPosition + position) * vocabulary) + token],
                window.Data[(position * vocabulary) + token]);
        }
    }

    private static void AssertTensorClose(Tensor expected, Tensor actual)
    {
        Assert.Equal(expected.Shape.Dimensions, actual.Shape.Dimensions);
        for (var index = 0; index < expected.Data.Length; index++)
            AssertClose(expected.Data[index], actual.Data[index]);
    }

    private static void AssertClose(float expected, float actual)
    {
        const float tolerance = 2e-5f;
        Assert.True(
            MathF.Abs(expected - actual) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
    }

    private sealed class RecordingCache : ITransformerKvCache
    {
        private readonly Tensor?[] _keys;
        private readonly Tensor?[] _values;
        private readonly CancellationTokenSource? _cancellation;
        private readonly int? _cancelAfterUpdate;
        private readonly int? _throwOnUpdate;
        private readonly bool _throwOnClear;

        public RecordingCache(
            int layerCount,
            CancellationTokenSource? cancellation = null,
            int? cancelAfterUpdate = null,
            int? throwOnUpdate = null,
            bool throwOnClear = false)
        {
            _keys = new Tensor?[layerCount];
            _values = new Tensor?[layerCount];
            _cancellation = cancellation;
            _cancelAfterUpdate = cancelAfterUpdate;
            _throwOnUpdate = throwOnUpdate;
            _throwOnClear = throwOnClear;
        }

        public int LayerCount => _keys.Length;
        public int UpdateCount { get; private set; }
        public int ClearCount { get; private set; }
        public int? ReportedUniformLength { get; set; }
        public List<int> UpdatedLayers { get; } = [];
        public List<int> UpdatedKeyHeadCounts { get; } = [];
        public List<int[]> UpdatedKeyShapes { get; } = [];
        public int[] SequenceLengths => Enumerable.Range(0, LayerCount)
            .Select(GetSequenceLength)
            .ToArray();

        public int GetSequenceLength(int layer) => _keys[layer]?.Shape.Dimensions[2] ?? 0;

        public int GetUniformSequenceLength()
        {
            if (ReportedUniformLength is not null)
                return ReportedUniformLength.Value;

            var length = GetSequenceLength(0);
            for (var layer = 1; layer < LayerCount; layer++)
            {
                if (GetSequenceLength(layer) != length)
                    throw new InvalidOperationException("TEST_CACHE_SKEWED");
            }

            return length;
        }

        public (Tensor keys, Tensor values) Update(int layer, Tensor newK, Tensor newV)
        {
            UpdatedLayers.Add(layer);
            UpdatedKeyHeadCounts.Add(newK.Shape.Dimensions[1]);
            UpdatedKeyShapes.Add(newK.Shape.Dimensions.ToArray());
            UpdateCount++;

            if (_cancelAfterUpdate == UpdateCount)
                _cancellation!.Cancel();
            if (_throwOnUpdate == UpdateCount)
                throw new InvalidOperationException("TEST_UPDATE_FAILURE");

            _keys[layer] = Append(_keys[layer], newK);
            _values[layer] = Append(_values[layer], newV);
            return (Clone(_keys[layer]!), Clone(_values[layer]!));
        }

        public void Clear()
        {
            ClearCount++;
            if (_throwOnClear)
                throw new InvalidOperationException("TEST_CLEAR_FAILURE");

            Array.Clear(_keys);
            Array.Clear(_values);
        }

        public void SeedLayer(int layer, int sequenceLength, ModelConfig config)
        {
            var shape = new TensorShape(1, config.NumKvHeads, sequenceLength, config.HeadDim);
            _keys[layer] = new Tensor(new float[shape.TotalElements], shape);
            _values[layer] = new Tensor(new float[shape.TotalElements], shape);
        }

        private static Tensor Append(Tensor? existing, Tensor append)
        {
            if (existing is null)
                return Clone(append);

            var dimensions = append.Shape.Dimensions;
            Assert.Equal(existing.Shape.Dimensions[0], dimensions[0]);
            Assert.Equal(existing.Shape.Dimensions[1], dimensions[1]);
            Assert.Equal(existing.Shape.Dimensions[3], dimensions[3]);
            var existingLength = existing.Shape.Dimensions[2];
            var appendLength = dimensions[2];
            var mergedLength = checked(existingLength + appendLength);
            var shape = new TensorShape(dimensions[0], dimensions[1], mergedLength, dimensions[3]);
            var data = new float[shape.TotalElements];
            var existingBlockLength = checked(existingLength * dimensions[3]);
            var appendBlockLength = checked(appendLength * dimensions[3]);
            var mergedBlockLength = checked(mergedLength * dimensions[3]);
            for (var block = 0; block < dimensions[0] * dimensions[1]; block++)
            {
                Array.Copy(existing.Data, block * existingBlockLength, data, block * mergedBlockLength, existingBlockLength);
                Array.Copy(append.Data, block * appendBlockLength, data, (block * mergedBlockLength) + existingBlockLength, appendBlockLength);
            }

            return new Tensor(data, shape);
        }

        private static Tensor Clone(Tensor source) =>
            new(source.Data.ToArray(), new TensorShape(source.Shape.Dimensions.ToArray()));
    }
}
