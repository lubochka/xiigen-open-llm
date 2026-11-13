// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using Xunit.Abstractions;

// Адресная приёмка переноса «перестановка осей внимания выполняется напрямую»
// (карточка 45, код правки FIX-ATS, строка работ 10 реестра переносов).
//
// Что закрывает этот файл и чего в нём намеренно нет.
//
// Механизм в продукте уже стоит: для одной формы — четвёртый ранг, средние оси —
// место записи считается прямо из четырёх размеров, и массив координат не
// строится вообще, тогда как общая ветвь строит его на каждый элемент. Числовой
// договор этой перестановки уже проверен отдельным набором рядом
// (AttentionTransposeStrideTests, девять проверок). Здесь закрываются ровно два
// незакрытых участка своей панели плюс четыре договорных случая, которых в том
// наборе нет: дублировать покрытое запрещено.
//
// Первое: учебная нога. Перенос обязан не ломать обучение, а не только
// арифметику. Поэтому восстанавливается назначенный обученный старт и на нём
// выполняются четыре настоящих обновления весов тем же продуктовым путём, каким
// ходит обычный вызывающий.
//
// Второе: ресурсное утверждение переноса. Оно измеряется, а не обещается, и
// измеряется относительно: узкий путь против общего при РАВНОМ числе элементов.
// Исторические проценты исходного опыта здесь не закрепляются как ожидаемое —
// карточка это прямо запрещает.
//
// Два приёма, которые задают устройство этого файла.
//
// Приём первый: ожидаемые значения выписаны руками ниже как постоянные. Ни одно
// из них не берётся из проверяемого объекта, из продуктового сериализатора или
// из вывода самого прогона. Сравнение «выгрузили и тут же загрузили из того же
// файла» замкнуто само на себя и не доказывает ничего.
//
// Приём второй, и он важнее первого. Обученные веса назначенного донора
// ПОБАЙТОВО совпадают с весами ещё пяти семейств того же размера, и состояние
// оптимизатора у всех шести тоже одно и то же. Проверка, опирающаяся на веса,
// прошла бы с любым чужим донором и не заметила подмены. Поэтому назначенность
// донора доказывается тремя отдельными признаками, и отдельная проверка ниже
// требует, чтобы продукт чужое семейство именно ОТВЕРГАЛ. Без неё приёмка слепа
// ровно в том месте, о котором предупреждает карточка.
public sealed class AttentionTransposeStrideTransferTests
{
    private readonly ITestOutputHelper _output;

    public AttentionTransposeStrideTransferTests(ITestOutputHelper output)
        => _output = output;

    // ── Выписанные руками постоянные ────────────────────────────────────────
    //
    // Это весь рецепт раздела 4 карточки, перенесённый сюда буквами, а не
    // прочитанный из паспорта. Паспорт закрепляет только личность донора: если
    // рецепт лежал бы в нём, ручная правка паспорта молча сдвинула бы ожидаемое.

    // Назначенный обученный старт. Ровно он, по имени семейства.
    private const string DonorRelativePath =
        "experiments-v2/RP01-MICRO/_shared-learned-starts/h64-attn-transpose/v1";

    // Опознавательные отпечатки назначенного донора, выписанные из карточки.
    // Продукт сверяет их сам при восстановлении состояния генератора и
    // отказывается работать при расхождении.
    private const string ExpectedTopologySha256 =
        "af80aff08cd28ec5c050f9fc17a02b55049bb263a096be9c7b86b8d2c78e9813";
    private const string ExpectedLoraConfigurationSha256 =
        "a963de9112c1a01dc2acc026734c0b5bfbc8b5b688b30c7caaf24dc7b2c2655a";

    // Область случайного потока адаптеров, объявленная донором. Присоединение
    // адаптеров идёт в этом потоке и ДО загрузки обученных весов.
    private const string ExpectedStreamDomain = "attention-transpose-probe-v1";

    // Зёрна: первое охватывает только построение модели, второе — адаптеры.
    private const ulong ConstructionSeed = 62001;
    private const ulong LoraSeed = 62002;

    // Номера шагов оптимизатора: донор выполнил четыреста настоящих обновлений,
    // приёмка добавляет свои четыре.
    private const int OptimizerStepBefore = 400;
    private const int OptimizerStepAfter = 404;

    // Сколько массивов обучается, а сколько заморожено при этом устройстве.
    private const int ExpectedTrainableArrays = 4;
    private const int ExpectedFrozenArrays = 12;

    // Правильные ответы панели как номера байтов: «плюс» это 43, «минус» 45.
    private const int PlusByteId = 43;
    private const int MinusByteId = 45;

    // Две учебные строки. Экзаменационных здесь нет: назначенный пробел их не
    // требует, а лишние проходы модели были бы расходом без утверждения.
    private const string FirstTrainPrompt = "C# result sign: 9-4 =";
    private const string FirstTrainAnswer = "+";
    private const string SecondTrainPrompt = "C# result sign: 4-9 =";
    private const string SecondTrainAnswer = "-";

    // Имена четырёх обучаемых массивов, объявленные донором отдельной записью.
    private static readonly string[] ExpectedTrainableNames =
    [
        "lora.block_0000.q_proj.lora_a",
        "lora.block_0000.q_proj.lora_b",
        "lora.block_0000.v_proj.lora_a",
        "lora.block_0000.v_proj.lora_b",
    ];

    // Пять чужих семейств с теми же весами и тем же состоянием оптимизатора.
    public static TheoryData<string> NeighbourFamiliesWithIdenticalWeights()
        => new("h64-causal-scores", "h64-rms-coordinate", "h64-rms-row",
               "h64-rope-table", "h64-silu");

    // ── 1. Учебная нога: четыре настоящих обновления на обученном старте ────

    [Fact]
    public void RestoredTrainedStart_SurvivesFourUpdates_AndEveryAdapterArrayMoves()
    {
        // Шаг 1. Байты донора сверяются ДО первого прохода модели: подмена
        // обученного старта обязана быть видна раньше любых чисел.
        using var manifestDocument = OpenManifest();
        var manifest = manifestDocument.RootElement;
        var donor = VerifyDonorBytes(manifest);

        // Какие именно байты продукта считали этот результат. Без этой расписки
        // нельзя сказать, что проверялось, а при доказательстве чувствительности
        // это и есть главное.
        _output.WriteLine("Загруженные сборки продукта:");
        _output.WriteLine(NativeAssemblyReceipt());

        // Шаг 2. Донор объявил себя завершённым и квалифицированным по обучению.
        // Это его собственная запись, а не утверждение приёмки.
        using var bundleDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "bundle.json")));
        var bundle = bundleDocument.RootElement;
        Assert.Equal("h64-attn-transpose", bundle.GetProperty("family_id").GetString());
        Assert.Equal("COMPLETE", bundle.GetProperty("status").GetString());
        Assert.True(bundle.GetProperty("learning_qualified").GetBoolean());
        Assert.Equal(OptimizerStepBefore, bundle.GetProperty("optimizerStep").GetInt32());

        // Шаг 3. Показания донора сняты при ВЫКЛЮЧЕННОМ проверяемом факторе, а
        // текущий продукт идёт узким путём всегда, в том числе на чтениях. Без
        // этой строки сравнение ниже было бы сравнением пути с самим собой.
        using var sourceManifestDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "source-manifest.json")));
        Assert.True(sourceManifestDocument.RootElement
            .GetProperty("generatedProducer")
            .GetProperty("factorDisabledDuringPreparation")
            .GetBoolean());

        using var preparationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "preparation.json")));
        var preparation = preparationDocument.RootElement;
        Assert.Equal(
            "original A semantics; attention transpose offsets disabled",
            preparation.GetProperty("branch").GetString());

        // Шаг 4. Восстановление назначенного обученного начала продуктовым путём.
        var start = RestoreAssignedStart(donor);

        // Разделение обучаемого и замороженного состояния то же, что у опыта.
        // Пустой набор любой из сторон сделал бы утверждения ниже бессмысленными.
        Assert.Equal(ExpectedTrainableArrays, start.Trainable.Count);
        Assert.Equal(ExpectedFrozenArrays, start.Frozen.Count);
        Assert.Equal(ExpectedTrainableArrays, start.Bindings.Count);

        // Состояние оптимизатора действительно ВВЕЗЕНО, а не создано заново:
        // обратная выгрузка совпадает по номеру шага, набору имён, формам и обоим
        // накопленным моментам. Иначе «восстановлено» значило бы «не упало».
        var exported = start.Optimizer.ExportPortableState(start.Bindings);
        Assert.Equal(OptimizerStepBefore, start.DonorOptimizerState.Step);
        Assert.Equal(OptimizerStepBefore, exported.Step);
        Assert.Equal(
            start.DonorOptimizerState.Entries.Select(entry => entry.Key),
            exported.Entries.Select(entry => entry.Key));
        foreach (var expected in start.DonorOptimizerState.Entries)
        {
            var actual = exported.Entries.Single(entry => entry.Key == expected.Key);
            Assert.Equal(expected.Dimensions, actual.Dimensions);
            Assert.Equal(expected.FirstMoment, actual.FirstMoment);
            Assert.Equal(expected.SecondMoment, actual.SecondMoment);
        }

        // Номер шага случайного состояния донора — тот же четырёхсотый.
        Assert.Equal(
            OptimizerStepBefore,
            preparation.GetProperty("randomStateStep").GetInt32());
        Assert.Equal(OptimizerStepBefore, start.DonorRandomState.RandomStateStep);

        // Шаг 5. Сопоставление восстановленного состояния ПО ИМЕНАМ параметров.
        //
        // Зачем отдельно от отпечатка всего набора. Отпечаток набора считается в
        // порядке обхода модели и не говорит, КАКОЕ имя несёт какие значения: при
        // переименовании или перестановке массивов он либо краснеет не указывая
        // виновного, либо вообще не различает подмену. Независимая запись донора
        // перечисляет свои обучаемые массивы по именам; её байты закреплены
        // паспортом и уже сверены выше, и сама она не является проверяемым
        // объектом. Имена берутся из настоящих записей контрольной точки продукта
        // — тем же путём, которым продукт сохраняет и загружает модель.
        using var authorityDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "authority.json")));
        var authority = authorityDocument.RootElement;

        var declaredNames = authority.GetProperty("namedBindings").EnumerateArray()
            .Select(value => value.GetString()!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        // Запись донора совпадает с выписанным руками набором имён. Это сверка
        // независимой записи с постоянной, а не записи с самой собой.
        Assert.Equal(ExpectedTrainableNames, declaredNames);

        // Та же область случайного потока, что объявил донор своей записью.
        Assert.Equal(
            ExpectedStreamDomain,
            authority.GetProperty("randomStateStreamDomain").GetString());

        var checkpointEntries = start.Model.GetCheckpointParameterEntries();
        var trainableByName = checkpointEntries
            .Where(entry => entry.Parameter.Value.RequiresGrad)
            .ToDictionary(entry => entry.Key, entry => entry.Parameter, StringComparer.Ordinal);
        var frozenByName = checkpointEntries
            .Where(entry => !entry.Parameter.Value.RequiresGrad)
            .ToDictionary(entry => entry.Key, entry => entry.Parameter, StringComparer.Ordinal);

        // Набор обучаемых имён совпадает с объявленным целиком: ни одного лишнего
        // и ни одного пропущенного.
        Assert.Equal(
            declaredNames,
            trainableByName.Keys.OrderBy(name => name, StringComparer.Ordinal).ToArray());

        // Замороженные массивы не носят имён обучаемых. Без этой строки
        // «изменилось» и «не изменилось» ниже могли бы относиться к одному массиву.
        Assert.Empty(frozenByName.Keys
            .Where(name => declaredNames.Contains(name, StringComparer.Ordinal)));
        Assert.Equal(ExpectedFrozenArrays, frozenByName.Count);

        // Форма каждого обучаемого массива совпадает с формой, записанной под ТЕМ
        // ЖЕ именем в файле состояния оптимизатора донора. Это отдельный файл
        // донора, а не проверяемый объект и не продуктовый сериализатор.
        foreach (var donorEntry in start.DonorOptimizerState.Entries)
        {
            Assert.True(
                trainableByName.ContainsKey(donorEntry.Key),
                "ATS_DONOR_NAMED_ARRAY_NOT_RESTORED:" + donorEntry.Key);
            Assert.Equal(
                donorEntry.Dimensions,
                trainableByName[donorEntry.Key].Value.Shape.Dimensions);
        }

        // Отпечаток КАЖДОГО массива по отдельности, снятый до обучения. Без него
        // утверждение «обучаемое состояние изменилось» опиралось бы на один
        // отпечаток всего набора и прошло бы даже тогда, когда обновился только
        // один адаптерный массив из четырёх.
        var trainableHashBefore = trainableByName.ToDictionary(
            pair => pair.Key,
            pair => ParametersHash([pair.Value]),
            StringComparer.Ordinal);
        var frozenHashBefore = frozenByName.ToDictionary(
            pair => pair.Key,
            pair => ParametersHash([pair.Value]),
            StringComparer.Ordinal);

        // Шаг 6. Вход годен: усвоенная строка уже верна ДО наших обновлений.
        //
        // Это предпроверка качества входа, а не результат приёмки. Если бы веса
        // не загрузились, здесь был бы необученный ответ, и тогда всё, что ниже,
        // измеряло бы пустоту. Отсутствие усвоенной строки — конкретный дефект
        // постановки, а не повод тихо дообучить модель.
        var firstBefore = Observe(start.Model, FirstTrainPrompt, FirstTrainAnswer);
        var secondBefore = Observe(start.Model, SecondTrainPrompt, SecondTrainAnswer);
        var diagnosticReads = 2;

        Assert.Equal(PlusByteId, firstBefore.GoldId);
        Assert.Equal(MinusByteId, secondBefore.GoldId);
        Assert.True(
            firstBefore.Exact,
            "ATS_RESTORED_START_IS_NOT_THE_LEARNED_ONE: усвоенная учебная строка "
            + "неверна до обучения, значит восстановлен не обученный старт");
        Assert.Equal(1, firstBefore.GoldRank);

        // И то же самое по независимой записи донора: до его четырёхсот
        // обновлений строка была неверна и правильный ответ стоял далеко, в точке
        // старта — верна и первая. Второе независимое свидетельство того же
        // состояния, из файла, который приёмка не создавала.
        var donorFresh = RecordedPanelById(preparation, "w0_panel");
        var donorAtStart = RecordedPanelById(preparation, "s0_panel");
        Assert.False(donorFresh["T1"].Exact);
        Assert.True(donorAtStart["T1"].Exact);
        Assert.True(donorFresh["T1"].GoldRank > donorAtStart["T1"].GoldRank);
        Assert.Equal(PlusByteId, donorAtStart["T1"].GoldId);
        Assert.Equal(1, donorAtStart["T1"].GoldRank);

        // Шаг 7. Четыре настоящих обновления весов в назначенном порядке.
        var sequences = new[]
        {
            Bytes(FirstTrainPrompt + FirstTrainAnswer),
            Bytes(SecondTrainPrompt + SecondTrainAnswer),
            Bytes(FirstTrainPrompt + FirstTrainAnswer),
            Bytes(SecondTrainPrompt + SecondTrainAnswer),
        };

        var losses = new float[sequences.Length];
        for (var step = 0; step < sequences.Length; step++)
        {
            losses[step] = start.Trainer.TrainSupervisedNextTokenStep(
                start.Model, start.Optimizer, sequences[step], weight: 1f);
            _output.WriteLine($"обновление {step + 1}: потеря {losses[step]:R}");
        }

        // Обновления настоящие, а не пустые вызовы: счётчик шагов оптимизатора
        // прошёл от четырёхсот к четырёмстам четырём.
        var afterTraining = start.Optimizer.ExportPortableState(start.Bindings);
        Assert.Equal(OptimizerStepAfter, afterTraining.Step);

        // Потери конечны и положительны. Точные значения здесь НЕ закрепляются:
        // это не двухстадийный прогон, и подгонять свежий прогон под старые байты
        // запрещено.
        foreach (var loss in losses)
        {
            Assert.True(float.IsFinite(loss), "ATS_LOSS_NOT_FINITE");
            Assert.True(loss > 0f, "ATS_LOSS_NOT_POSITIVE");
        }

        // Шаг 8. Что изменилось, а что обязано было остаться.
        //
        // Оба факта проверяются отпечатками ЭТОГО прогона, а не сверкой со старым
        // двоичным отпечатком.
        foreach (var pair in trainableByName.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var trained = ParametersHash([pair.Value]);
            _output.WriteLine(
                $"обучаемый массив «{pair.Key}»: до {trainableHashBefore[pair.Key]}, "
                + $"после {trained}");
            Assert.NotEqual(trainableHashBefore[pair.Key], trained);
        }

        foreach (var pair in frozenByName.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            Assert.Equal(frozenHashBefore[pair.Key], ParametersHash([pair.Value]));
        }

        // Шаг 9. Обучение изменило не только веса, но и ответы.
        var firstAfter = Observe(start.Model, FirstTrainPrompt, FirstTrainAnswer);
        var secondAfter = Observe(start.Model, SecondTrainPrompt, SecondTrainAnswer);
        diagnosticReads += 2;
        Assert.NotEqual(firstBefore.LastLogitsBase64, firstAfter.LastLogitsBase64);

        // Шаг 10. Никакого завышения результата.
        //
        // Усвоенная строка удержана, но правило знака НЕ освоено: вторая учебная
        // строка неверна и до, и после, учебная доля остаётся одна из двух. Это
        // равно простому постоянному ответу и успехом по качеству не является.
        Assert.True(firstAfter.Exact);
        Assert.False(secondBefore.Exact);
        Assert.False(secondAfter.Exact);
        Assert.Equal(1, new[] { firstBefore, secondBefore }.Count(row => row.Exact));
        Assert.Equal(1, new[] { firstAfter, secondAfter }.Count(row => row.Exact));

        // Шаг 11. Учёт расхода: четыре обновления и четыре чтения, ни одного
        // лишнего прохода и ни одного порождения текста.
        Assert.Equal(4, losses.Length);
        Assert.Equal(4, diagnosticReads);
        _output.WriteLine(
            $"Расход прогона: учебных обновлений {losses.Length}, учебных проходов "
            + $"вперёд {sequences.Length}, обратных {sequences.Length}, "
            + $"диагностических чтений {diagnosticReads}, порождений текста 0.");
    }

    // ── 2. Обязательный отрицательный контроль: чужой донор отвергается ─────

    [Theory]
    [MemberData(nameof(NeighbourFamiliesWithIdenticalWeights))]
    public void ANeighbourDonorWithIdenticalWeightsIsRefused(string neighbourFamily)
    {
        // Почему эта проверка обязательна, а не дополнительна. Обученные веса и
        // состояние оптимизатора назначенного донора побайтово совпадают с пятью
        // чужими семействами того же размера: все шесть выращены из одних зёрен на
        // одних данных при нулевом отсеве. Значит проверка, опирающаяся на веса,
        // прошла бы с любым из них. Карточка предупреждает об этом прямо.
        //
        // Отдельно: область случайного потока тоже не уникальна — у двух чужих
        // семейств она совпадает между собой. Опознают донора ровно отпечаток
        // файла случайного состояния и отпечатки опознавательных строк.
        using var manifestDocument = OpenManifest();
        var manifest = manifestDocument.RootElement;
        var donor = VerifyDonorBytes(manifest);

        var neighbour = Path.Combine(RepositoryRoot(),
            manifest.GetProperty("donor_is_not_distinguished_by_weights_ru")
                .GetProperty("neighbour_families_that_must_be_refused")
                .GetProperty(neighbourFamily)
                .GetProperty("relative_path").GetString()!
                .Replace('/', Path.DirectorySeparatorChar));
        Assert.True(Directory.Exists(neighbour), "ATS_NEIGHBOUR_DONOR_NOT_FOUND:" + neighbour);

        // Первое: веса и состояние оптимизатора совпадают ПОБАЙТОВО. Это и есть
        // причина, по которой дальше нужен отказ.
        var ourWeights = Sha256Hex(File.ReadAllBytes(Path.Combine(donor, "S0.weights.bin")));
        var theirWeights = Sha256Hex(File.ReadAllBytes(Path.Combine(neighbour, "S0.weights.bin")));
        var ourOptimizer = Sha256Hex(File.ReadAllBytes(Path.Combine(donor, "S0.optimizer.bin")));
        var theirOptimizer = Sha256Hex(File.ReadAllBytes(Path.Combine(neighbour, "S0.optimizer.bin")));
        Assert.Equal(ourWeights, theirWeights);
        Assert.Equal(ourOptimizer, theirOptimizer);

        // Второе: отпечаток файла случайного состояния различается — это один из
        // трёх настоящих опознавательных признаков.
        var ourRandomFile = Sha256Hex(File.ReadAllBytes(Path.Combine(donor, "S0.rng.json")));
        var theirRandomFile = Sha256Hex(File.ReadAllBytes(Path.Combine(neighbour, "S0.rng.json")));
        Assert.NotEqual(ourRandomFile, theirRandomFile);

        _output.WriteLine(
            $"семейство «{neighbourFamily}»: веса {theirWeights} — те же, состояние "
            + $"оптимизатора {theirOptimizer} — то же, файл случайного состояния "
            + $"{theirRandomFile} — другой.");

        // Третье, и это утверждение проверки: продукт ОТКАЗЫВАЕТСЯ восстанавливать
        // чужой снимок под личностью назначенного донора. Отказ опознаётся по
        // расхождению опознавательных строк, а не по удаче.
        var model = BuildModelWithAdapters();
        var foreignSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes(Path.Combine(neighbour, "S0.rng.json")))
            ?? throw new InvalidOperationException("ATS_NEIGHBOUR_RANDOM_STATE_UNREADABLE");

        var refusal = Assert.Throws<InvalidOperationException>(
            () => model.RestoreRandomState(
                foreignSnapshot,
                new LlmModelRandomStateIdentity(
                    ExpectedTopologySha256, ExpectedLoraConfigurationSha256)));
        Assert.Contains("DPO_RANDOM_STATE_IDENTITY_MISMATCH", refusal.Message, StringComparison.Ordinal);

        // И тот же самый вызов с НАШИМ снимком проходит. Без этой строки отказ
        // выше мог бы означать, что отвергается вообще всё.
        var ourSnapshot = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes(Path.Combine(donor, "S0.rng.json")))
            ?? throw new InvalidOperationException("ATS_RANDOM_STATE_UNREADABLE");
        model.RestoreRandomState(
            ourSnapshot,
            new LlmModelRandomStateIdentity(
                ExpectedTopologySha256, ExpectedLoraConfigurationSha256));
    }

    // ── 3. Ресурсная панель своей строки ───────────────────────────────────

    [Fact]
    public void NarrowPathAllocatesFarLessThanTheGeneralPathForTheSameWork()
    {
        // Что именно доказывает эта проверка и чего она НЕ доказывает.
        //
        // Доказывает продуктовое свойство переноса: узкий путь не строит массив
        // координат, а общий строит его на каждый элемент. Поэтому при РАВНОМ
        // числе элементов узкий путь обязан выделять существенно меньше памяти.
        //
        // НЕ доказывает: выигрыш во времени, процент на большой модели и
        // исторические числа исходного опыта. Жёсткий порог на байты здесь не
        // закрепляется — карточка прямо запрещает закреплять −8,67% как
        // ожидаемое. Утверждение относительное: «существенно меньше при равной
        // работе», и наблюдённые числа печатаются как есть.
        //
        // Единственный различающийся признак между двумя измерениями — ранг, то
        // есть ровно выбранная ветвь. Форма «пакет 2 × головы 4 × позиции 64 ×
        // ширина 16» — это настоящая форма внимания этой модели: 8192 элемента.
        // Контрольная форма добавляет пятую ось длины один: число элементов и
        // переставляемые оси те же, перестановка математически та же, но ранг
        // пять, поэтому работает общая ветвь.
        int[] narrowDims = [2, 4, 64, 16];
        int[] generalDims = [2, 4, 64, 16, 1];

        var narrowInput = Filled(narrowDims, seed: 17);
        var generalInput = Filled(generalDims, seed: 17);
        Assert.Equal(
            narrowInput.Shape.TotalElements,
            generalInput.Shape.TotalElements);

        // Обе перестановки дают одни и те же числа в одном и том же порядке.
        // Это доказывает, что сравнивается одна и та же работа, а не две разные.
        var narrowOnce = TensorOps.Transpose(narrowInput, 1, 2);
        var generalOnce = TensorOps.Transpose(generalInput, 1, 2);
        Assert.Equal(narrowOnce.Data.Length, generalOnce.Data.Length);
        for (var i = 0; i < narrowOnce.Data.Length; i++)
        {
            Assert.Equal(
                BitConverter.SingleToInt32Bits(narrowOnce.Data[i]),
                BitConverter.SingleToInt32Bits(generalOnce.Data[i]));
        }

        // Что внутри счётчика: ровно вызовы перестановки, повторённые одинаковое
        // число раз. Что снаружи: создание входов, прогрев и само сравнение.
        const int Repeats = 20;
        var narrowBytes = MeasureAllocations(
            () => TensorOps.Transpose(narrowInput, 1, 2), Repeats);
        var generalBytes = MeasureAllocations(
            () => TensorOps.Transpose(generalInput, 1, 2), Repeats);

        // Второе наблюдение общей ветви, ещё ближе к узкой: тот же четвёртый
        // ранг и то же число элементов, но оси не средние, поэтому ветвь общая.
        // Отображение здесь другое, поэтому сравнения чисел для него нет — только
        // расход.
        var rank4GeneralBytes = MeasureAllocations(
            () => TensorOps.Transpose(narrowInput, 0, 2), Repeats);

        var elements = narrowInput.Shape.TotalElements;
        _output.WriteLine(
            $"число элементов {elements}, повторов {Repeats}, прогрев выполнен.");
        _output.WriteLine(
            $"узкий путь (ранг 4, оси 1 и 2): выделено {narrowBytes} байт, "
            + $"{narrowBytes / (double)Repeats:F0} байт на вызов.");
        _output.WriteLine(
            $"общая ветвь (ранг 5, оси 1 и 2, то же число элементов): выделено "
            + $"{generalBytes} байт, {generalBytes / (double)Repeats:F0} байт на вызов.");
        _output.WriteLine(
            $"общая ветвь (ранг 4, оси 0 и 2, то же число элементов): выделено "
            + $"{rank4GeneralBytes} байт, {rank4GeneralBytes / (double)Repeats:F0} байт на вызов.");
        _output.WriteLine(
            $"отношение общая к узкой: ранг 5 — {generalBytes / (double)narrowBytes:F2}, "
            + $"ранг 4 вне средних осей — {rank4GeneralBytes / (double)narrowBytes:F2}.");

        // Утверждение. Запас взят широким намеренно: ожидаемая разница — один
        // массив координат на каждый элемент против ни одного, то есть величина
        // много большая, чем дрожание сборщика мусора на таком размере. Если это
        // когда-нибудь станет неустойчивым, честный путь — сказать об этом, а не
        // ослабить порог.
        Assert.True(
            narrowBytes > 0,
            "ATS_NARROW_ALLOCATION_NOT_MEASURED: счётчик выделений ничего не "
            + "показал, значит измерение недействительно, а не улучшение бесконечно");
        Assert.True(
            generalBytes > narrowBytes * 3,
            $"ATS_NARROW_PATH_DID_NOT_SAVE_ALLOCATIONS: узкий путь {narrowBytes}, "
            + $"общая ветвь {generalBytes} при равном числе элементов {elements}");
        Assert.True(
            rank4GeneralBytes > narrowBytes * 3,
            $"ATS_RANK4_GENERAL_PATH_DID_NOT_COST_MORE: узкий путь {narrowBytes}, "
            + $"общая ветвь того же ранга {rank4GeneralBytes}");
    }

    // ── 4. Четыре договорных случая, которых нет в наборе рядом ────────────

    [Fact]
    public void Rank4ButNotTheMiddleAxesKeepsTheGeneralMapping()
    {
        // Граница условия узкой ветви. Ранг четвёртый, но оси нулевая и вторая,
        // поэтому работать обязана общая ветвь. Набор рядом проверяет ранг пять и
        // ранг три, а эту форму — ровно четвёртый ранг вне средних осей — нет, и
        // это самый опасный пропуск: условие ветви проверяет И ранг, И оси.
        const int A = 2, B = 3, C = 4, D = 5;
        var x = Filled([A, B, C, D], seed: 211);
        var moved = TensorOps.Transpose(x, 0, 2);

        Assert.Equal([C, B, A, D], moved.Shape.Dimensions);

        // Независимый ответ, выписанный руками: элемент с местами (a, b, c, d)
        // становится элементом с местами (c, b, a, d).
        for (var a = 0; a < A; a++)
        for (var b = 0; b < B; b++)
        for (var c = 0; c < C; c++)
        for (var d = 0; d < D; d++)
        {
            var source = (((((a * B) + b) * C) + c) * D) + d;
            var destination = (((((c * B) + b) * A) + a) * D) + d;
            Assert.Equal(x.Data[source], moved.Data[destination]);
        }
    }

    [Fact]
    public void TheMiddleAxesResultDoesNotShareMemoryWithTheInput()
    {
        // Вторая половина случая «круговой обход без наложения». Набор рядом
        // проверяет, что двойная перестановка возвращает те же биты, но не
        // проверяет, что возвращённый массив — отдельная память. Если бы узкий
        // путь когда-нибудь вернул обёртку над входом, обход всё равно сошёлся бы,
        // а запись в результат молча испортила бы вход обучения.
        var x = Filled([2, 7, 3, 8], seed: 97);
        var before = x.Data.ToArray();

        var moved = TensorOps.Transpose(x, 1, 2);
        Assert.NotSame(x.Data, moved.Data);

        // Правка результата не доходит до входа.
        moved.Data[0] = 123.5f;
        Assert.Equal(before, x.Data);

        // И обратно: правка входа не доходит до уже выданного результата.
        var movedBefore = moved.Data.ToArray();
        x.Data[0] = -77.25f;
        Assert.Equal(movedBefore, moved.Data);
    }

    [Fact]
    public void AnAlreadyCancelledRequestRefusesBeforeAnyWork()
    {
        // Случай «заранее отменённый признак». Набор рядом отменяет работу на
        // первом сообщении о продвижении, то есть уже внутри ядра. Отказ на входе
        // метода — другая ветвь того же договора: отменённый признак обязан
        // остановить вызов до того, как будет выделена выходная память и записан
        // хотя бы один элемент.
        var x = Filled([2, 4, 64, 16], seed: 131);
        var before = x.Data.ToArray();

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => TensorOps.Transpose(x, 1, 2, cancellation.Token));

        // Вход не тронут, и частично заполненный результат наружу не вышел.
        Assert.Equal(before, x.Data);
    }

    [Fact]
    public void GraphFlagsFollowTheSameRulesOnBothPaths()
    {
        // Признаки графа, которых в наборе рядом нет совсем. Узкий путь не должен
        // отличаться от общего в том, как он подключает результат к графу:
        // обучаемость переносится, вход становится ребёнком результата, а для
        // необучаемого входа граф не создаётся вовсе. Если бы узкий путь потерял
        // ребёнка, арифметика осталась бы верной, а градиент перестал бы доходить
        // до весов — и ни одна числовая проверка этого бы не увидела.
        var trainableNarrow = Filled([2, 3, 4, 5], seed: 149);
        trainableNarrow.RequiresGrad = true;
        var narrow = TensorOps.Transpose(trainableNarrow, 1, 2);
        Assert.True(narrow.RequiresGrad);
        Assert.Same(trainableNarrow, Assert.Single(narrow.Children));

        var trainableGeneral = Filled([2, 3, 4, 5], seed: 149);
        trainableGeneral.RequiresGrad = true;
        var general = TensorOps.Transpose(trainableGeneral, 0, 2);
        Assert.True(general.RequiresGrad);
        Assert.Same(trainableGeneral, Assert.Single(general.Children));

        // Необучаемый вход: графа нет ни на одном из двух путей.
        var frozenNarrow = Filled([2, 3, 4, 5], seed: 151);
        var frozenNarrowResult = TensorOps.Transpose(frozenNarrow, 1, 2);
        Assert.False(frozenNarrowResult.RequiresGrad);
        Assert.Empty(frozenNarrowResult.Children);

        var frozenGeneral = Filled([2, 3, 4, 5], seed: 151);
        var frozenGeneralResult = TensorOps.Transpose(frozenGeneral, 0, 2);
        Assert.False(frozenGeneralResult.RequiresGrad);
        Assert.Empty(frozenGeneralResult.Children);
    }

    // ── Оснастка: паспорт, байты донора, восстановление ────────────────────

    // Одно прочитанное показание панели.
    private sealed record PanelReadout(
        string Prompt, int GoldId, int PredictedId, bool Exact,
        int GoldRank, string LastLogitsBase64);

    // Восстановленное обученное начало вместе со всем, что нужно для проверки.
    private sealed record RestoredStart(
        LlmModel Model,
        IReadOnlyList<AdamWParameterBinding> Bindings,
        AdamW Optimizer,
        Trainer Trainer,
        AdamWPortableState DonorOptimizerState,
        LlmModelRandomStateSnapshot DonorRandomState,
        IReadOnlyList<Parameter> Trainable,
        IReadOnlyList<Parameter> Frozen);

    // Паспорт лежит в хранилище, а не в папке сборки: путь не зависит от того,
    // откуда запущен прогон.
    private static JsonDocument OpenManifest()
    {
        var path = Path.Combine(
            RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1",
            "AttentionTranspose01", "manifest.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("ATS_FIXTURE_MANIFEST_NOT_FOUND:" + path, path);
        return JsonDocument.Parse(File.ReadAllBytes(path));
    }

    // Донор остаётся на своём месте: приёмка читает его, но не дублирует.
    // Несовпадение отпечатка, отсутствующий файл или указатель хранилища больших
    // файлов вместо весов — конкретный дефект постановки до первого прохода
    // модели, а не повод подобрать другую модель.
    private static string VerifyDonorBytes(JsonElement manifest)
    {
        var donor = Path.Combine(
            RepositoryRoot(),
            manifest.GetProperty("donor_relative_path").GetString()!
                .Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(donor))
            throw new DirectoryNotFoundException("ATS_DONOR_DIRECTORY_NOT_FOUND:" + donor);

        foreach (var pinned in manifest.GetProperty("whole_file_sha256").EnumerateObject())
        {
            var path = Path.Combine(donor, pinned.Name);
            if (!File.Exists(path))
                throw new FileNotFoundException("ATS_DONOR_FILE_NOT_FOUND:" + path, path);

            var actual = Sha256Hex(File.ReadAllBytes(path));
            if (actual != pinned.Value.GetString())
            {
                throw new InvalidOperationException(
                    $"ATS_DONOR_FINGERPRINT_MISMATCH:{pinned.Name}:ожидался "
                    + $"{pinned.Value.GetString()}, прочитан {actual}");
            }
        }

        // Объявленные размеры тоже сверяются: указатель вместо настоящих весов
        // весит иначе и обязан быть замечен.
        foreach (var declared in manifest.GetProperty("file_bytes").EnumerateObject())
        {
            var length = new FileInfo(Path.Combine(donor, declared.Name)).Length;
            if (length != declared.Value.GetInt64())
            {
                throw new InvalidOperationException(
                    $"ATS_DONOR_SIZE_MISMATCH:{declared.Name}:объявлено "
                    + $"{declared.Value.GetInt64()}, прочитано {length}");
            }
        }

        return donor;
    }

    // Модель того устройства, что объявил донор, с присоединёнными адаптерами.
    // Детерминированное зерно охватывает только конструктор: после загрузки весов
    // зёрна на значения не влияют. Адаптеры присоединяются в назначенном
    // случайном потоке и ДО загрузки контрольной точки, как задаёт карточка.
    private static LlmModel BuildModelWithAdapters()
    {
        LlmModel built;
        using (Initializers.UseDeterministicSeed(ConstructionSeed))
        {
            built = new LlmModel(new ModelConfig
            {
                VocabSize = 256,
                ContextLen = 64,
                HiddenDim = 64,
                NumLayers = 1,
                NumHeads = 4,
                NumKvHeads = 2,
                FfnHiddenDim = 176,
                RopeTheta = 10000f,
                RmsNormEps = 1e-5f,
            });
        }

        LoraApplicator.ApplySeeded(
            built,
            new LoraConfig
            {
                Rank = 4,
                Alpha = 8f,
                Dropout = 0f,
                TargetModules = ["q_proj", "v_proj"],
            },
            LoraSeed,
            ExpectedStreamDomain);

        return built;
    }

    // Порядок шагов взят из карточки дословно: построение с детерминированным
    // зерном, присоединение адаптеров в назначенном потоке, загрузка обученных
    // весов, привязки оптимизатора только по фактически присоединённым
    // параметрам, ввоз полного состояния оптимизатора с требованием номера шага,
    // восстановление состояния генератора и создание обучателя.
    private static RestoredStart RestoreAssignedStart(string donorPath)
    {
        var model = BuildModelWithAdapters();
        var bindings = AdapterBindings(model);

        // Обученные веса читаются из общего донора потоком, без копии в приёмку.
        using (var weights = new FileStream(
                   Path.Combine(donorPath, "S0.weights.bin"),
                   FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            weights.Position = 0;
            model.LoadCheckpoint(weights);
        }

        // Обучаемые и замороженные массивы разделяются тем же признаком, что у
        // хозяина опыта: пустой набор любой из сторон недопустим.
        var trainable = model.Parameters().Where(p => p.Value.RequiresGrad).ToArray();
        var frozen = model.Parameters().Where(p => !p.Value.RequiresGrad).ToArray();
        if (trainable.Length == 0 || frozen.Length == 0)
            throw new InvalidOperationException("ATS_MISSING_TRAINABLE_OR_FROZEN_STATE");

        // Оптимизатор создаётся с настройкой донора и получает полное сохранённое
        // состояние. Отказ ввоза — ошибка, а не тихий старт с нулевыми
        // накопленными оценками градиентов.
        var optimizer = new AdamW(
            beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);

        var donorOptimizer = AdamWPortableState.FromBytes(
                File.ReadAllBytes(Path.Combine(donorPath, "S0.optimizer.bin")))
            ?? throw new InvalidOperationException("ATS_OPTIMIZER_STATE_UNREADABLE");

        if (donorOptimizer.Step != OptimizerStepBefore)
        {
            throw new InvalidOperationException(
                $"ATS_OPTIMIZER_STEP_MISMATCH:ожидался {OptimizerStepBefore}, "
                + $"прочитан {donorOptimizer.Step}");
        }

        if (!optimizer.TryImportPortableState(bindings, donorOptimizer))
            throw new InvalidOperationException("ATS_OPTIMIZER_IMPORT_REFUSED");

        // Состояние генератора восстанавливается под выписанной руками личностью
        // донора. Продукт сам сверяет опознавательные строки и откажется при
        // расхождении — на этом стоит отрицательный контроль выше.
        var donorRandom = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes(Path.Combine(donorPath, "S0.rng.json")))
            ?? throw new InvalidOperationException("ATS_RANDOM_STATE_UNREADABLE");
        model.RestoreRandomState(
            donorRandom,
            new LlmModelRandomStateIdentity(
                ExpectedTopologySha256, ExpectedLoraConfigurationSha256));

        // Обучатель собирается из выписанной руками настройки донора.
        var trainer = new Trainer(new TrainingConfig
        {
            BatchSize = 1,
            SeqLen = 64,
            WarmupSteps = 0,
            TotalSteps = OptimizerStepAfter,
            MaxLr = 0.001f,
            MinLr = 0.001f,
            GradClipNorm = 1f,
            GradAccumSteps = 1,
            WeightDecay = 0f,
        });

        return new RestoredStart(
            model, bindings, optimizer, trainer, donorOptimizer, donorRandom,
            trainable, frozen);
    }

    // Привязки состояния оптимизатора строятся только по ФАКТИЧЕСКИ
    // присоединённым адаптерам назначенных проекций и по тождеству ссылок на их
    // параметры, а ключи берутся из настоящих записей контрольной точки и
    // упорядочиваются по порядку символов. Брать все параметры модели или другой
    // порядок нельзя: ввоз сохранённых моментов привязан именно к этому набору.
    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(LlmModel model)
    {
        string[] targets = ["q_proj", "v_proj"];

        var attached = Enumerable.Range(0, model.Blocks.Count)
            .SelectMany(block => targets
                .Select(target => model.GetAttachedProjectionAdapter(block, target)))
            .OfType<LoraLinear>()
            .ToArray();

        var allowed = attached
            .SelectMany(adapter => adapter.AdapterParameters)
            .Select(entry => entry.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance);

        var bindings = model.GetCheckpointParameterEntries()
            .Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal)
            .ToArray();

        if (bindings.Length == 0)
            throw new InvalidOperationException("ATS_ADAPTER_BINDINGS_EMPTY");

        return bindings;
    }

    // ── Чтение одной строки панели ─────────────────────────────────────────

    // Один диагностический проход по подсказке без обновления. Соглашение
    // повторяет хозяина опыта: на вход идут байты подсказки, читается последняя
    // позиция, наибольший берётся строгим сравнением. Правильный ответ
    // предсказателю не передаётся: он берётся уже после ответа.
    private static PanelReadout Observe(LlmModel model, string prompt, string answer)
    {
        var ids = Bytes(prompt);
        var input = new Tensor(
            ids.Select(id => (float)id).ToArray(), new TensorShape(1, ids.Length));

        var logits = model.Forward(input, training: false);

        var vocab = model.Config.VocabSize;
        var offset = (ids.Length - 1) * vocab;
        var last = logits.Data.AsSpan(offset, vocab).ToArray();

        var max = float.NegativeInfinity;
        var best = 0;
        for (var id = 0; id < vocab; id++)
            if (last[id] > max) { max = last[id]; best = id; }

        var gold = Bytes(answer)[0];
        var goldLogit = last[gold];

        // Строгий ранг считает только строго большие значения.
        var goldRank = 1;
        for (var id = 0; id < vocab; id++)
            if (last[id] > goldLogit) goldRank++;

        return new PanelReadout(
            prompt, gold, best, best == gold, goldRank,
            Convert.ToBase64String(FloatBits(last)));
    }

    // Записанная донором панель, разложенная по опознавательному имени строки.
    // Ключ — имя строки, а не номер в массиве: порядок записи не должен молча
    // переставлять строки местами.
    private static IReadOnlyDictionary<string, PanelReadout> RecordedPanelById(
        JsonElement root, string field)
        => root.GetProperty(field).EnumerateArray().ToDictionary(
            row => row.GetProperty("id").GetString()!,
            row => new PanelReadout(
                row.GetProperty("prompt").GetString()!,
                row.GetProperty("gold_token_id").GetInt32(),
                row.GetProperty("predicted_token_id").GetInt32(),
                row.GetProperty("exact").GetBoolean(),
                row.GetProperty("target_rank").GetInt32(),
                string.Empty),
            StringComparer.Ordinal);

    // ── Измерение расхода памяти ───────────────────────────────────────────

    // Выделения вокруг одного вызова, повторённого заданное число раз. Прогрев
    // выполняется до счётчика, чтобы однократная подготовка путей исполнения не
    // попала в измерение. Счётчик потока считает все выделения, сделанные этим
    // потоком, и сборка мусора его не обнуляет, поэтому измеряется именно расход,
    // а не остаток в памяти.
    private static long MeasureAllocations(Func<Tensor> call, int repeats)
    {
        for (var i = 0; i < repeats; i++) call();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < repeats; i++) call();
        var after = GC.GetAllocatedBytesForCurrentThread();

        return after - before;
    }

    // ── Мелкие помощники ───────────────────────────────────────────────────

    // Входной тензор с воспроизводимыми, но неравномерными значениями: равные
    // значения спрятали бы отображение, которое переносит элемент не туда.
    private static Tensor Filled(int[] dims, int seed)
    {
        var shape = new TensorShape(dims);
        var data = new float[shape.TotalElements];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.5f + (0.011f * ((i + seed) % 197));
        return new Tensor(data, shape);
    }

    // Отпечаток набора параметров тем же правилом, что у хозяина опыта: для
    // каждого параметра ранг, затем все размерности, затем все значения.
    private static string ParametersHash(IEnumerable<Parameter> parameters)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var dimension in parameter.Value.Shape.Dimensions) writer.Write(dimension);
            foreach (var value in parameter.Value.Data) writer.Write(value);
        }

        writer.Flush();
        return Sha256Hex(stream.ToArray());
    }

    // Расписка о фактически загруженных сборках продукта: место, отпечаток файла
    // и опознаватель сборки. Сборки берутся от самих используемых типов, а не по
    // предполагаемому имени каталога.
    private static string NativeAssemblyReceipt()
    {
        var targets = new (string Human, System.Reflection.Assembly Assembly)[]
        {
            ("ядро тензоров, где находится проверяемая перестановка осей", typeof(TensorOps).Assembly),
            ("модель, которая эту перестановку вызывает во внимании", typeof(LlmModel).Assembly),
            ("обучатель, который делает учебные обновления", typeof(Trainer).Assembly),
        };

        var parts = new List<string>();
        foreach (var (human, assembly) in targets)
        {
            var name = assembly.GetName().Name ?? "БЕЗ_ИМЕНИ";
            if (string.IsNullOrEmpty(assembly.Location))
            {
                parts.Add($"{human} ({name}): расположение недоступно, отпечаток снять нельзя");
                continue;
            }

            var hash = Sha256Hex(File.ReadAllBytes(assembly.Location));
            parts.Add(
                $"{human} ({name}): файл {assembly.Location}, отпечаток {hash}, "
                + $"опознаватель сборки {assembly.ManifestModule.ModuleVersionId}");
        }

        return string.Join(Environment.NewLine, parts);
    }

    // Байты текста как номера лексем: кодировка UTF-8, без разбора на части,
    // без метки начала и конца.
    private static int[] Bytes(string text)
        => Encoding.UTF8.GetBytes(text).Select(value => (int)value).ToArray();

    // Биты чисел в том же порядке, в каком их записывал опыт.
    private static byte[] FloatBits(IReadOnlyList<float> values)
    {
        var buffer = new byte[values.Count * 4];
        for (var index = 0; index < values.Count; index++)
            BitConverter.GetBytes(values[index]).CopyTo(buffer, index * 4);
        return buffer;
    }

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    // Подъём от папки сборки до корня хранилища: путь не зависит от места запуска.
    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("ATS_REPOSITORY_ROOT_NOT_FOUND");
    }
}
