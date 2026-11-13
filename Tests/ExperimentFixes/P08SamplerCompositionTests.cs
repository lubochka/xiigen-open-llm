// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Проверки переноса последовательных отсечений в продукт (R28-S1, вторая часть).
//
// Что проверяется по-человечески. Пользователь может задать сразу два ограничения на
// выбор следующего знака: сколько кандидатов оставить и какую суммарную вероятность
// они должны набрать. Раньше при заданном числе кандидатов второе ограничение молча
// пропадало. Теперь сначала работает маска по числу кандидатов, а отсечение по сумме
// вероятностей считает сумму уже по этому короткому списку.
//
// Как доказывается состав кандидатов. Продуктовая функция выбора возвращает только
// номер знака, поэтому состав восстанавливается её же открытым входом: случайная
// величина пробегает по промежутку от нуля до единицы, и по тому, какой знак выпал,
// видно и набор кандидатов, и границы их промежутков. Ожидание считает отдельный
// независимый эталон, а для настоящих чисел восьмого опыта — ещё и сохранённый файл
// плеча B, из которого числа ЧИТАЮТСЯ, а не вписываются в код.
//
// Граница этих проверок. Ресурсы здесь не измеряются вовсе: это отдельный наряд,
// статус ресурсной оси остаётся «не измерено». Исторические файлы восьмого опыта,
// его квитанции и экспериментальная копия плеча B не изменяются и только читаются.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Diagnostics;
using System.Text.Json;
using Inference;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

/// <summary>Проверки последовательных отсечений «сначала число кандидатов, затем сумма вероятностей».</summary>
public sealed class P08SamplerCompositionTests
{
    // Настройки сравнения плеч восьмого опыта: ровно те, что записаны в его свидетельствах.
    private const float ArmTemperature = 1.0f; // Температура выдачи обоих плеч опыта.
    private const int ArmTopK = 2; // Сколько кандидатов оставляет первое правило.
    private const float ArmTopP = 0.6f; // Какую суммарную вероятность набирает второе правило.
    private const int ArmSeed = 20261027; // Зерно случайности каждой выдачи опыта.

    // Насколько близко к границе промежутка берутся пробные случайные величины.
    private const double BoundaryProbe = 1e-6;
    // Сколько пробных точек берётся, чтобы увидеть ВЕСЬ набор достижимых знаков.
    private const int SupportProbePoints = 512;
    // Допуск сравнения вероятностей: продукт считает в одинарной точности, эталон — в двойной.
    private const double ProbabilityTolerance = 1e-5;

    /// <summary>
    /// (а) Настоящие числа восьмого опыта через настоящий порождатель текста.
    /// Продуктовый порождатель действительно вызывается, сам создаёт зерно случайности,
    /// сам расшифровывает выданный знак; ожидаемый знак читается из сохранённого файла плеча B.
    /// </summary>
    [Theory]
    [InlineData("T1")]
    [InlineData("T2")]
    [InlineData("E1")]
    [InlineData("E2")]
    public void RealGeneratorWithBothLimitsReproducesTheSavedArmBAnswerOfTheEighthExperiment(string rowIdentifier)
    {
        var savedRow = LoadSavedRow(rowIdentifier); // Сохранённое показание строки опыта.
        var expected = LoadExpectedArmBSample(rowIdentifier); // Ожидание плеча B из сохранённого файла.
        var tokenizer = LoadExperimentTokenizer(); // Тот же побайтовый разбиватель текста, что в опыте.

        // Разбиватель текста обязан дать ровно те номера знаков подсказки, что сохранены в опыте:
        // это доказывает, что проверка работает с той же строкой, а не с похожей.
        Assert.Equal(savedRow.PromptTokenIds, tokenizer.Encode(savedRow.Prompt));

        // Перенос R30-S1: несущая модель заменена настоящим восстановленным состоянием S1
        // восьмого опыта. В продуктовый выбор теперь идут числа, полученные ОТ МОДЕЛИ;
        // сохранённые логиты остаются только эталоном сравнения.
        var model = RestoreEighthExperimentTrainedState();
        var trainableBefore = ParameterValuesHash(model, trainable: true); // Отпечаток обучаемых массивов до выдачи.
        var frozenBefore = ParameterValuesHash(model, trainable: false); // Отпечаток замороженной основы до выдачи.

        int samplerCalls = 0; // Сколько раз порождатель позвал функцию выбора знака.
        bool calledByProductGenerator = false; // Был ли продуктовый порождатель в цепочке вызовов.
        float observedTemperature = float.NaN; // Температура, которую передал сам порождатель.
        int observedTopK = -1; // Число кандидатов, которое передал сам порождатель.
        float observedTopP = float.NaN; // Сумма вероятностей, которую передал сам порождатель.
        float[] modelLogitValues = []; // Числа, которые выдала сама модель.
        int samplerDraws = -1; // Сколько раз выбор обратился к датчику случайных чисел.

        var generator = new TextGenerator(
            model,
            tokenizer,
            layers => new KvCache(layers),
            (modelLogits, random, temperature, topK, topP) =>
            {
                samplerCalls++; // Считаем фактические вызовы: созданный объект вызовом не считается.
                calledByProductGenerator = CallChainContainsProductGenerator(); // Снимок цепочки вызывающих кадров.
                observedTemperature = temperature; // Запоминаем, что именно донёс порождатель.
                observedTopK = topK;
                observedTopP = topP;
                // Снимок чисел модели делается до выбора: сравнение с эталоном идёт по ним.
                modelLogitValues = [.. modelLogits.Data];
                // Расход случайности считается отдельно, но датчик остаётся тем же, что дал порождатель.
                var countingRandom = new CountingDrawRandom(random);
                var selected = Sampler.Sample(modelLogits, countingRandom, temperature, topK, topP);
                samplerDraws = countingRandom.Draws;
                return selected;
            });

        var produced = generator.GenerateWithFirstDecision(
            savedRow.Prompt, maxNewTokens: 1, ArmTemperature, ArmTopK, ArmTopP, ArmSeed,
            verifiedEosToken: null, CancellationToken.None);

        // Порождатель позвал функцию выбора ровно один раз: один новый знак — один выбор.
        Assert.Equal(1, samplerCalls);
        // Вызов пришёл именно из продуктового порождателя, а не из тела проверки.
        Assert.True(calledByProductGenerator, "Выбор знака обязан быть вызван продуктовым порождателем текста.");
        // Порождатель донёс до выбора ровно те настройки опыта, которые ему передали.
        Assert.Equal(ArmTemperature, observedTemperature);
        Assert.Equal(ArmTopK, observedTopK);
        Assert.Equal(ArmTopP, observedTopP);
        // Предзаполнение выполнено одним прямым проходом, продолжений нет.
        Assert.Equal(1, produced.PrefillForwardCalls);
        Assert.Equal(0, produced.DecodeForwardCalls);

        // Числа самой модели сверяются с сохранённым эталоном по всей длине словаря.
        // Допуск зафиксирован заранее и при расхождении не увеличивается.
        Assert.Equal(savedRow.Logits.Length, modelLogitValues.Length);
        Assert.Equal(256, modelLogitValues.Length);
        for (int index = 0; index < modelLogitValues.Length; index++)
        {
            Assert.True(float.IsFinite(modelLogitValues[index]),
                $"Число модели {index} не конечно: {modelLogitValues[index]}.");
            var allowed = LogitAbsoluteTolerance + (LogitRelativeTolerance * Math.Abs(savedRow.Logits[index]));
            Assert.True(
                Math.Abs(modelLogitValues[index] - savedRow.Logits[index]) <= allowed,
                $"Число модели {index}: ожидалось {savedRow.Logits[index]}, получено {modelLogitValues[index]}, допуск {allowed}.");
        }

        // Выбранный знак совпал с сохранённым ответом плеча B этой строки.
        Assert.Equal(expected.SelectedTokenId, produced.Decision.SelectedTokenId);
        // И расшифрованный ответ порождателя совпал с сохранённым ответом плеча B.
        Assert.Equal(expected.DecodedAnswer, produced.Receipt.Text);
        // Стохастический выбор потратил ровно одну случайную величину.
        Assert.Equal(1, samplerDraws);
        // Выдача ничего не обучала: ни обучаемые массивы, ни замороженная основа не сдвинулись.
        Assert.Equal(trainableBefore, ParameterValuesHash(model, trainable: true));
        Assert.Equal(frozenBefore, ParameterValuesHash(model, trainable: false));
        // Зерно опыта даёт ровно ту случайную величину, при которой получен сохранённый ответ.
        Assert.Equal(LoadSharedDraw(), new Random(ArmSeed).NextDouble(), 15);
    }

    /// <summary>
    /// (а) Состав кандидатов на настоящих числах восьмого опыта. Это и есть различающая
    /// проверка: при прежнем «иначе если» на строке T1 остаются два кандидата вместо одного.
    /// </summary>
    [Theory]
    [InlineData("T1")]
    [InlineData("T2")]
    [InlineData("E1")]
    [InlineData("E2")]
    public void BothLimitsOnSavedExperimentLogitsNarrowTheCandidateSetToTheSavedArmBComposition(string rowIdentifier)
    {
        var savedRow = LoadSavedRow(rowIdentifier); // Сохранённые логиты строки опыта.
        var expected = LoadExpectedArmBSample(rowIdentifier); // Сохранённый состав плеча B.

        // Независимый эталон считает тот же состав по описанию правил, без продуктового кода.
        var reference = P08SamplerCompositionReference.Compose(savedRow.Logits, ArmTemperature, ArmTopK, ArmTopP);

        // Сначала эталон сверяется с сохранённым файлом опыта: если они разойдутся, виноват
        // эталон или прочтение файла, а не продукт, и это надо увидеть отдельно.
        Assert.Equal(expected.SupportTokenIds, reference.TokenIds);
        AssertProbabilitiesClose(expected.SupportProbabilities, reference.Probabilities);

        // Затем продукт сверяется с эталоном через свой открытый вход.
        AssertProductCompositionMatchesReference(savedRow.Logits, ArmTemperature, ArmTopK, ArmTopP, reference);

        // И выбор продукта при сохранённой случайной величине совпадает с сохранённым знаком.
        Assert.Equal(
            expected.SelectedTokenId,
            SampleWithProduct(savedRow.Logits, LoadSharedDraw(), ArmTemperature, ArmTopK, ArmTopP));
    }

    /// <summary>
    /// (б) Оба ограничения вместе на маленьком понятном распределении: состав обязан сузиться
    /// ровно так, как говорит независимый эталон, и стать уже, чем при одном числе кандидатов.
    /// </summary>
    [Fact]
    public void BothLimitsOnASmallDistributionNarrowTheCandidateSetExactlyAsTheReferenceSays()
    {
        // Четыре понятных числа: после деления на температуру 1 они остаются собой.
        float[] logits = [4f, 3f, 2f, 1f];

        // Три кандидата по первому правилу, затем сумма вероятностей 0,7 по уже отобранным.
        var reference = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 3, topP: 0.7f);
        // Только число кандидатов — это прежнее поведение, с которым сравнивается сужение.
        var candidateCountOnly = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 3, topP: 1f);

        // Эталон обязан показать именно сужение: трёх кандидатов стало два.
        Assert.Equal(new[] { 0, 1, 2 }, candidateCountOnly.TokenIds);
        Assert.Equal(new[] { 0, 1 }, reference.TokenIds);

        // Продукт с обоими ограничениями обязан дать суженный состав эталона.
        AssertProductCompositionMatchesReference(logits, 1f, 3, 0.7f, reference);
        // Третий кандидат при обоих ограничениях недостижим ни при какой случайной величине.
        Assert.DoesNotContain(2, ProbeReachableTokens(logits, 1f, 3, 0.7f));
    }

    /// <summary>(в) Задано только число кандидатов: состав прежний, второе правило не вмешивается.</summary>
    [Fact]
    public void CandidateCountAloneKeepsThePreviousCandidateSet()
    {
        float[] logits = [4f, 3f, 2f, 1f]; // Те же понятные числа, что и в случае с двумя ограничениями.

        var reference = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 3, topP: 1f);

        // Прежнее поведение: остаются ровно три наибольших кандидата.
        Assert.Equal(new[] { 0, 1, 2 }, reference.TokenIds);
        AssertProductCompositionMatchesReference(logits, 1f, 3, 1f, reference);
    }

    /// <summary>(г) Задана только сумма вероятностей: состав прежний, первое правило выключено.</summary>
    [Fact]
    public void ProbabilitySumAloneKeepsThePreviousCandidateSet()
    {
        float[] logits = [4f, 3f, 2f, 1f]; // Те же понятные числа для прямого сравнения случаев.

        var reference = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 0, topP: 0.7f);

        // Прежнее поведение: кандидаты набираются по убыванию, пока сумма перед ними меньше порога.
        Assert.Equal(new[] { 0, 1 }, reference.TokenIds);
        AssertProductCompositionMatchesReference(logits, 1f, 0, 0.7f, reference);
    }

    /// <summary>(д) Температура ноль: жадный выбор, случайная величина не берётся вовсе.</summary>
    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void ZeroOrNegativeTemperatureKeepsTheGreedyChoiceAndDoesNotTouchTheRandomSource(float temperature)
    {
        float[] logits = [1f, 7f, 2f, 7f]; // Наибольшее значение встречается дважды: проверяется и равенство.

        // Жадная ветвь не зависит от двух ограничений и от датчика случайных чисел.
        Assert.Equal(
            P08SamplerCompositionReference.SelectGreedily(logits),
            Sampler.Sample(ToTensor(logits), new ThrowingRandom(), temperature, topK: 2, topP: 0.6f));
        Assert.Equal(1, Sampler.Sample(ToTensor(logits), new ThrowingRandom(), temperature, topK: 0, topP: 1f));
    }

    /// <summary>
    /// (е) Правило «хотя бы один кандидат остаётся» при очень малой сумме вероятностей:
    /// состав не пустеет, в нём остаётся ровно наиболее вероятный знак.
    /// </summary>
    [Theory]
    [InlineData(1e-6f)]
    [InlineData(0f)]
    public void AVanishinglySmallProbabilitySumStillLeavesExactlyTheMostLikelyCandidate(float topP)
    {
        float[] logits = [4f, 3f, 2f, 1f]; // Наиболее вероятный кандидат здесь — нулевой.

        var reference = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 3, topP: topP);

        // Состав не пустеет и состоит ровно из одного наиболее вероятного кандидата.
        Assert.Equal(new[] { 0 }, reference.TokenIds);
        Assert.Equal(1d, Assert.Single(reference.Probabilities), 12);
        AssertProductCompositionMatchesReference(logits, 1f, 3, topP, reference);
    }

    // ── Сверка продукта с эталоном через открытый вход продуктовой функции выбора ──────────

    /// <summary>
    /// Сверяет состав кандидатов продукта с эталоном, не заглядывая внутрь продукта: случайная
    /// величина пробегает промежуток, и по выпавшим знакам видно и набор, и границы промежутков.
    /// </summary>
    private static void AssertProductCompositionMatchesReference(
        float[] logits,
        float temperature,
        int topK,
        float topP,
        P08SamplerCompositionReference.Composition reference)
    {
        // Весь достижимый набор знаков обязан совпасть с набором эталона.
        Assert.Equal(reference.TokenIds.ToArray(), ProbeReachableTokens(logits, temperature, topK, topP));

        var boundaries = P08SamplerCompositionReference.CumulativeBoundaries(reference);
        double previousBoundary = 0d; // Левый край промежутка текущего кандидата.
        for (int position = 0; position < reference.TokenIds.Count; position++)
        {
            double boundary = boundaries[position]; // Правый край промежутка текущего кандидата.
            // Середина промежутка обязана выбрать именно этого кандидата.
            Assert.Equal(
                reference.TokenIds[position],
                SampleWithProduct(logits, (previousBoundary + boundary) / 2d, temperature, topK, topP));

            // Чуть левее правого края всё ещё этот кандидат: так проверяется его вероятность.
            // Проверяются только границы между кандидатами; у последнего правого соседа нет.
            if (position + 1 < reference.TokenIds.Count)
            {
                Assert.Equal(
                    reference.TokenIds[position],
                    SampleWithProduct(logits, boundary - BoundaryProbe, temperature, topK, topP));
                // А чуть правее — уже следующий кандидат состава.
                Assert.Equal(
                    reference.TokenIds[position + 1],
                    SampleWithProduct(logits, boundary + BoundaryProbe, temperature, topK, topP));
            }

            previousBoundary = boundary; // Переходим к промежутку следующего кандидата.
        }
    }

    /// <summary>Набор знаков, достижимых у продукта при равномерном переборе случайной величины.</summary>
    private static int[] ProbeReachableTokens(float[] logits, float temperature, int topK, float topP)
    {
        var reachable = new SortedSet<int>(); // Номера знаков, которые продукт вообще может выдать.
        for (int point = 0; point < SupportProbePoints; point++)
        {
            // Берутся середины равных долей промежутка: крайняя точка остаётся строго меньше единицы.
            double draw = (point + 0.5d) / SupportProbePoints;
            reachable.Add(SampleWithProduct(logits, draw, temperature, topK, topP));
        }

        return [.. reachable];
    }

    /// <summary>Один вызов продуктовой функции выбора с заранее заданной случайной величиной.</summary>
    private static int SampleWithProduct(float[] logits, double draw, float temperature, int topK, float topP)
        => Sampler.Sample(ToTensor(logits), new SingleDrawRandom(draw), temperature, topK, topP);

    /// <summary>Сверка вероятностей с допуском: продукт считает в одинарной точности, эталон — в двойной.</summary>
    private static void AssertProbabilitiesClose(IReadOnlyList<double> expected, IReadOnlyList<double> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int index = 0; index < expected.Count; index++)
            Assert.True(
                Math.Abs(expected[index] - actual[index]) <= ProbabilityTolerance,
                $"Вероятность кандидата {index}: ожидалось {expected[index]}, получено {actual[index]}.");
    }

    // ── Чтение сохранённых свидетельств восьмого опыта ─────────────────────────────────────

    /// <summary>Сохранённое показание одной строки опыта: подсказка, её знаки и логиты после обучения.</summary>
    private sealed record SavedRow(string Prompt, List<int> PromptTokenIds, float[] Logits);

    /// <summary>Сохранённое ожидание плеча B: состав кандидатов, выбранный знак и его расшифровка.</summary>
    private sealed record ExpectedArmSample(
        int[] SupportTokenIds,
        double[] SupportProbabilities,
        int SelectedTokenId,
        string DecodedAnswer);

    /// <summary>Читает логиты и подсказку строки опыта из сохранённого показания после обучения.</summary>
    private static SavedRow LoadSavedRow(string rowIdentifier)
    {
        using var saved = JsonDocument.Parse(File.ReadAllText(ExperimentFile("results/attempt-0001/after.json")));
        // Строка выбирается по её обозначению, а не по порядковому номеру в файле.
        var row = saved.RootElement.GetProperty("rows").EnumerateArray()
            .Single(candidate => candidate.GetProperty("id").GetString() == rowIdentifier);

        return new SavedRow(
            row.GetProperty("prompt").GetString()!,
            [.. row.GetProperty("prompt_token_ids").EnumerateArray().Select(value => value.GetInt32())],
            [.. row.GetProperty("logits").EnumerateArray().Select(value => value.GetSingle())]);
    }

    /// <summary>Читает ожидание плеча B из сохранённого сравнения плеч; числа не вписываются в код.</summary>
    private static ExpectedArmSample LoadExpectedArmBSample(string rowIdentifier)
    {
        using var arms = JsonDocument.Parse(File.ReadAllText(ExperimentFile("results/attempt-0001/arms.json")));
        // Нужна запись именно плеча B и именно этой строки опыта.
        var sample = arms.RootElement.GetProperty("model_samples").EnumerateArray()
            .Single(candidate => candidate.GetProperty("arm").GetString() == "B" &&
                candidate.GetProperty("row_id").GetString() == rowIdentifier);

        return new ExpectedArmSample(
            [.. sample.GetProperty("support_token_ids").EnumerateArray().Select(value => value.GetInt32())],
            [.. sample.GetProperty("support_probabilities").EnumerateArray().Select(value => value.GetDouble())],
            sample.GetProperty("selected_token_id").GetInt32(),
            sample.GetProperty("decoded_answer").GetString()!);
    }

    /// <summary>Читает общую для обоих плеч случайную величину, записанную в сравнении плеч.</summary>
    private static double LoadSharedDraw()
    {
        using var arms = JsonDocument.Parse(File.ReadAllText(ExperimentFile("results/attempt-0001/arms.json")));
        return arms.RootElement.GetProperty("shared_draw").GetDouble();
    }

    /// <summary>Загружает тот же побайтовый разбиватель текста, которым пользовался восьмой опыт.</summary>
    private static BpeTokenizer LoadExperimentTokenizer()
        => BpeTokenizer.Load(ExperimentFile("src/byte-identity-tokenizer.json"));

    /// <summary>Путь к файлу хозяина восьмого опыта; сами файлы только читаются и не меняются.</summary>
    private static string ExperimentFile(string relativePath)
        => Path.Combine(FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "P0801",
            relativePath.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>Поднимается от папки сборки до корня хранилища, чтобы пути не зависели от способа запуска.</summary>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("P08_SAMPLER_COMPOSITION_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }

    // ── Вспомогательные построения проверки ────────────────────────────────────────────────

    /// <summary>Превращает набор чисел в одномерный тензор продуктовой формы.</summary>
    private static Tensor ToTensor(float[] logits) => new((float[])logits.Clone(), new TensorShape(logits.Length));

    /// <summary>
    /// Несущая модель порождателя: её числа в выборе не участвуют, она нужна только чтобы
    /// настоящий продуктовый порождатель мог выполнить свой обычный прямой проход.
    /// Обученным состоянием восьмого опыта она не является; то состояние живёт в его квитанции.
    /// Остаётся для проверок, которым настоящее обученное состояние не нужно.
    /// </summary>
    private static LlmModel CreateCarrierModel(int vocabSize, int contextLength)
    {
        var model = new LlmModel(new ModelConfig
        {
            VocabSize = vocabSize,
            ContextLen = contextLength,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16
        });
        // Все веса обнуляются: прямой проход обязан быть полностью предсказуемым и конечным.
        foreach (var parameter in model.BaseParameters) Array.Clear(parameter.Value.Data);
        return model;
    }

    // ── Восстановление настоящего обученного состояния восьмого опыта (перенос R30-S1) ────

    private const string TopologyLiteral = "RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj"; // Текст топологии производителя набора.
    private const string LoraLiteral = "RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1"; // Текст настройки адаптеров.
    private const string AdapterStream = "rms-row-reduce-probe-v1"; // Поток случайных чисел адаптеров.
    private const ulong SeedBase = 62001UL; // Зерно начальных значений замороженной основы.
    private const ulong SeedLora = 62002UL; // Зерно начальных значений адаптеров.
    // Допуск сравнения чисел модели с сохранённым эталоном, зафиксированный планом ремонта.
    private const double LogitAbsoluteTolerance = 1e-5;
    private const double LogitRelativeTolerance = 1e-4;

    /// <summary>
    /// Восстанавливает ровно то состояние S1, из которого восьмой опыт снял свои показания:
    /// та же численная конфигурация донора, то же зерно основы, те же адаптеры и поток,
    /// веса после обучения и снимок генератора случайных чисел. Отпечатки всех трёх
    /// сохранённых файлов сверяются с показаниями опыта ДО чтения состояния.
    /// Учебный оптимизатор здесь не создаётся: проверяется выдача, а не продолжение обучения,
    /// и счётчик шага 404 из показаний не выдаётся за выполненное новое обновление.
    /// </summary>
    private static LlmModel RestoreEighthExperimentTrainedState()
    {
        using var metrics = JsonDocument.Parse(File.ReadAllBytes(ExperimentFile("results/attempt-0001/metrics.json")));
        var savedState = metrics.RootElement.GetProperty("learning").GetProperty("saved_state");

        // Сначала сверяются отпечатки сохранённых файлов, и только потом они читаются.
        AssertDeclaredFileHash("results/attempt-0001/S1.weights.bin", savedState.GetProperty("weights_sha256").GetString()!);
        AssertDeclaredFileHash("results/attempt-0001/S1.optimizer.bin", savedState.GetProperty("optimizer_sha256").GetString()!);
        AssertDeclaredFileHash("results/attempt-0001/S1.rng.json", savedState.GetProperty("random_sha256").GetString()!);

        // Численная конфигурация берётся из описания порождения донора, а не вписывается в код.
        using var generation = JsonDocument.Parse(File.ReadAllBytes(DonorFile("generation.json")));
        var modelBlock = generation.RootElement.GetProperty("model");
        var config = new ModelConfig
        {
            VocabSize = modelBlock.GetProperty("VocabSize").GetInt32(),
            ContextLen = modelBlock.GetProperty("ContextLen").GetInt32(),
            HiddenDim = modelBlock.GetProperty("HiddenDim").GetInt32(),
            NumLayers = modelBlock.GetProperty("NumLayers").GetInt32(),
            NumHeads = modelBlock.GetProperty("NumHeads").GetInt32(),
            NumKvHeads = modelBlock.GetProperty("NumKvHeads").GetInt32(),
            FfnHiddenDim = modelBlock.GetProperty("FfnHiddenDim").GetInt32(),
            RopeTheta = modelBlock.GetProperty("RopeTheta").GetSingle(),
            RmsNormEps = modelBlock.GetProperty("RmsNormEps").GetSingle()
        };

        var lora = new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] };
        var randomIdentity = new LlmModelRandomStateIdentity(Sha256OfText(TopologyLiteral), Sha256OfText(LoraLiteral));
        var randomSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes(ExperimentFile("results/attempt-0001/S1.rng.json")))
            ?? throw new InvalidOperationException("P08_S1_RANDOM_SNAPSHOT_UNREADABLE");

        LlmModel model;
        using (Initializers.UseDeterministicSeed(SeedBase)) model = new LlmModel(config); // Та же основа.
        LoraApplicator.ApplySeeded(model, lora, SeedLora, AdapterStream); // Те же адаптеры и поток.
        model.LoadCheckpoint(ExperimentFile("results/attempt-0001/S1.weights.bin")); // Веса ПОСЛЕ обучения.
        model.RestoreRandomState(randomSnapshot, randomIdentity); // Генератор с проверкой опознавателей.

        // Байты контрольной точки в памяти обязаны совпасть с сохранённым файлом весов:
        // совпадения одного выданного знака для доказательства состояния недостаточно.
        using var inMemory = new MemoryStream();
        model.SaveCheckpointToStream(inMemory);
        Assert.Equal(
            savedState.GetProperty("weights_sha256").GetString(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(inMemory.ToArray())).ToLowerInvariant());

        return model;
    }

    /// <summary>Сверяет отпечаток сохранённого файла опыта с объявленным в его показаниях.</summary>
    private static void AssertDeclaredFileHash(string relativePath, string declaredSha256)
    {
        var actual = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(ExperimentFile(relativePath))))
            .ToLowerInvariant();
        Assert.Equal(declaredSha256, actual);
    }

    /// <summary>Отпечаток всех значений обучаемых либо замороженных массивов модели.</summary>
    private static string ParameterValuesHash(LlmModel model, bool trainable)
    {
        using var buffer = new MemoryStream();
        using (var writer = new BinaryWriter(buffer, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            foreach (var parameter in model.Parameters().Where(p => p.Value.RequiresGrad == trainable))
                foreach (var value in parameter.Value.Data)
                    writer.Write(value);
        }

        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(buffer.ToArray())).ToLowerInvariant();
    }

    /// <summary>Отпечаток строки опознавателя снимка генератора случайных чисел.</summary>
    private static string Sha256OfText(string text)
        => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))
            .ToLowerInvariant();

    /// <summary>Путь к файлу общего обученного старта донора; файлы только читаются.</summary>
    private static string DonorFile(string relativePath)
        => Path.Combine(FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "_shared-learned-starts",
            "h64-rms-row", "v1", relativePath.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>Датчик, который считает обращения и передаёт их настоящему датчику порождателя.</summary>
    private sealed class CountingDrawRandom(Random inner) : Random
    {
        private readonly Random _inner = inner; // Настоящий датчик, созданный продуктовым порождателем.

        public int Draws { get; private set; } // Сколько величин взял выбор.

        public override double NextDouble()
        {
            Draws++;
            return _inner.NextDouble();
        }
    }

    /// <summary>Проверяет, что вызов пришёл из продуктового порождателя текста, а не из тела проверки.</summary>
    private static bool CallChainContainsProductGenerator()
        => new StackTrace(fNeedFileInfo: false).GetFrames()
            .Any(frame => frame.GetMethod()?.DeclaringType == typeof(TextGenerator));

    /// <summary>Датчик случайных чисел с одной заранее заданной величиной.</summary>
    private sealed class SingleDrawRandom(double value) : Random
    {
        private readonly double _value = value; // Та самая величина, которую получит выбор.
        public override double NextDouble() => _value;
    }

    /// <summary>Датчик, который падает при обращении: жадный выбор обязан его не трогать.</summary>
    private sealed class ThrowingRandom : Random
    {
        public override double NextDouble()
            => throw new InvalidOperationException("P08_GREEDY_MUST_NOT_USE_RANDOM");
    }
}
