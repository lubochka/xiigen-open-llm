// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LlmFromScratch.Tests;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

// Приёмка переноса «причинное внимание не вычисляет запрещённую половину
// матрицы» (строка реестра по адресу R07, строка файла 203 реестра исправлений
// по опытам; блок измерений строки 629–643; карточка переноса измеренных
// нейронных изменений, раздел про только причинно видимые оценки).
//
// Что здесь доказывается человеческими словами. В причинном внимании слово
// может смотреть только назад. Поэтому половина таблицы оценок — та, что выше
// главной диагонали, — сразу заменяется маской на минус бесконечность, и её
// значения никогда не читают. Узкое ядро считает только видимые клетки.
//
// Уровень отдельного ядра уже закрыт соседней проверкой: там доказано побитовое
// равенство видимых клеток полному произведению, одинаковые вероятности и
// контекст после маски, одинаковые градиенты и отказ на прямоугольном блоке.
// Здесь закрывается ДРУГОЕ, и это ровно то, что называет строка очереди:
// восстановление настоящего ОБУЧЕННОГО начального состояния и полного состояния,
// и проверка, что на этом обученном состоянии узкий путь даёт те же ответы.
//
// Откуда независимый эталон. Донор семейства причинных оценок записал показания
// своей панели до обучения и после четырёхсот настоящих обновлений. Решающее
// обстоятельство: он снимал их при ВЫКЛЮЧЕННОМ проверяемом факторе, то есть
// полным произведением. Текущий продукт всегда идёт узким треугольником. Значит
// совпадение чисел — это сравнение двух разных путей на одном обученном
// состоянии, а эталон взят из ранее сохранённого источника, а не из этого
// прогона.
//
// Расход: четыре диагностических прямых прохода, ноль обратных, ноль обновлений.
// Это восстановление и чтение, а не обучение.
public sealed class CausalScoreTrainedStateRestoreTests
{
    // Журнал прогона: измеренные расхождения попадают в вывод проверки, а не
    // остаются только в голове исполнителя.
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public CausalScoreTrainedStateRestoreTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    // Строки, от которых донор считал отпечатки топологии и настройки адаптеров.
    private const string TopologyLiteral = "CS01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj";
    private const string LoraLiteral = "CS01|alpha8|dropout0|seed62002|frozen-transpose-probe-v1";

    // Зёрна построения и адаптеров и область случайного потока донора.
    private const ulong ConstructionSeed = 62001UL;
    private const ulong LoraSeed = 62002UL;
    private const string StreamDomain = "frozen-transpose-probe-v1";

    // Донор выполнил ровно четыреста настоящих обновлений.
    private const int DonorStep = 400;

    [Fact]
    public void RestoredTrainedStartReproducesTheRecordedPanelThroughTheTriangle()
    {
        // ── Шаг 1. Паспорт приёмки и байты донора ────────────────────────────

        var manifestPath = Path.Combine(
            RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1",
            "CausalScoreTrainedStart01", "manifest.json");
        using var manifestDocument = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        var manifest = manifestDocument.RootElement;

        // Донор лежит на своём месте и не дублируется в проверки: обученные
        // веса весят триста двадцать килобайт, копировать их незачем.
        var donor = Path.Combine(
            RepositoryRoot(),
            manifest.GetProperty("donor_relative_path").GetString()!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(Directory.Exists(donor), "DONOR_START_DIRECTORY_NOT_FOUND:" + donor);

        // Каждый прочитанный файл донора сверяется с закреплённым отпечатком:
        // молчаливая подмена обученного старта обязана быть видна.
        foreach (var pinned in manifest.GetProperty("whole_file_sha256").EnumerateObject())
        {
            var path = Path.Combine(donor, pinned.Name);
            Assert.True(File.Exists(path), "DONOR_FILE_NOT_FOUND:" + path);
            Assert.Equal(pinned.Value.GetString(), Sha256Hex(File.ReadAllBytes(path)));
        }

        // Донор объявил себя завершённым и квалифицированным по обучению.
        using var bundleDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(donor, "bundle.json")));
        Assert.Equal("COMPLETE", bundleDocument.RootElement.GetProperty("status").GetString());
        Assert.True(bundleDocument.RootElement.GetProperty("learning_qualified").GetBoolean());
        Assert.Equal(DonorStep, bundleDocument.RootElement.GetProperty("optimizerStep").GetInt32());

        using var preparationDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(donor, "preparation.json")));
        var preparation = preparationDocument.RootElement;

        // Показания донора сняты при выключенном проверяемом факторе: без этого
        // сравнение было бы сравнением пути с самим собой и ничего не доказывало.
        using var sourceManifestDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(donor, "source-manifest.json")));
        Assert.True(sourceManifestDocument.RootElement
            .GetProperty("generatedProducer")
            .GetProperty("factorDisabledDuringPreparation")
            .GetBoolean());
        Assert.True(manifest.GetProperty("independent_gold_ru")
            .GetProperty("factor_disabled_during_preparation").GetBoolean());

        // ── Шаг 2. Построение ровно той же модели, что у донора ──────────────

        // Детерминированное зерно охватывает только конструктор, как у хозяина
        // опыта: после загрузки весов никаких зёрен уже не применяется.
        LlmModel model;
        using (Initializers.UseDeterministicSeed(ConstructionSeed))
            model = new LlmModel(DonorConfig());
        var adapters = LoraApplicator.ApplySeeded(model, DonorLora(), LoraSeed, StreamDomain);

        // Четыре обучаемых массива адаптеров и их настоящие ключи контрольной точки.
        var checkpointEntries = model.GetCheckpointParameterEntries();
        var bindings = new List<AdamWParameterBinding>();
        foreach (var parameter in adapters.SelectMany(adapter => adapter.AdapterParameters.Select(pair => pair.Parameter)))
        {
            var entry = checkpointEntries.FirstOrDefault(candidate => ReferenceEquals(candidate.Parameter, parameter));
            Assert.NotNull(entry);
            bindings.Add(new AdamWParameterBinding(entry!.Key, parameter));
        }

        Assert.Equal(
            manifest.GetProperty("restored_state_ru").GetProperty("trainable_arrays").GetInt32(),
            bindings.Count);

        // ── Шаг 3. Восстановление обученных весов и сверка всех массивов ─────

        using (var weights = new FileStream(
                   Path.Combine(donor, "S0.weights.bin"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            weights.Position = 0;
            model.LoadCheckpoint(weights);
        }

        // Сверяются ВСЕ именованные веса: ключ, форма и сами биты. Совпадения
        // должны быть взаимно однозначными, иначе часть состояния осталась бы
        // непроверенной и молча отличалась.
        var expectedWeights = preparation.GetProperty("weight_state_snapshot")
            .GetProperty("entries").EnumerateArray()
            .ToDictionary(entry => entry.GetProperty("key").GetString()!, entry => entry);
        var loadedWeights = model.GetCheckpointParameterEntries()
            .Select(entry => (
                entry.Key,
                Shape: entry.Parameter.Value.Shape.Dimensions,
                Bits: FloatBits(entry.Parameter.Value.Data)))
            .ToList();

        Assert.Equal(
            preparation.GetProperty("weight_state_snapshot").GetProperty("entry_count").GetInt32(),
            loadedWeights.Count);
        Assert.Equal(expectedWeights.Count, loadedWeights.Count);
        foreach (var weight in loadedWeights)
        {
            Assert.True(expectedWeights.TryGetValue(weight.Key, out var expected), "WEIGHT_KEY_NOT_IN_DONOR:" + weight.Key);
            Assert.Equal(
                expected.GetProperty("shape").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
                weight.Shape);
            Assert.Equal(
                Convert.FromBase64String(expected.GetProperty("float_bits_base64").GetString()!),
                weight.Bits);
        }

        // Общий отпечаток всего именованного состояния считается тем же правилом,
        // что у донора, и обязан совпасть с записанным.
        Assert.Equal(
            preparation.GetProperty("weight_state_snapshot").GetProperty("combined_sha256").GetString(),
            Sha256Hex(Encoding.UTF8.GetBytes(string.Join(
                "|",
                loadedWeights.Select(weight => weight.Key + ":" + Sha256Hex(weight.Bits))))));

        // ── Шаг 4. Восстановление полного состояния: оптимизатор и генератор ──

        var optimizer = new AdamW(beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);
        var savedOptimizer = AdamWPortableState.FromBytes(File.ReadAllBytes(Path.Combine(donor, "S0.optimizer.bin")));
        Assert.NotNull(savedOptimizer);
        // Номер шага равен четырёмстам: это обученное состояние, а не чистое.
        Assert.Equal(DonorStep, savedOptimizer!.Step);
        Assert.True(optimizer.TryImportPortableState(bindings, savedOptimizer));

        // Обратная выгрузка обязана совпасть по полям: шаг, набор связок, формы и
        // оба момента. Без этого «восстановлено» значило бы только «не упало».
        var exportedOptimizer = optimizer.ExportPortableState(bindings);
        Assert.Equal(savedOptimizer.Step, exportedOptimizer.Step);
        Assert.Equal(
            savedOptimizer.Entries.Select(entry => entry.Key),
            exportedOptimizer.Entries.Select(entry => entry.Key));
        foreach (var expected in savedOptimizer.Entries)
        {
            var actual = exportedOptimizer.Entries.Single(entry => entry.Key == expected.Key);
            Assert.Equal(expected.Dimensions, actual.Dimensions);
            Assert.Equal(expected.FirstMoment, actual.FirstMoment);
            Assert.Equal(expected.SecondMoment, actual.SecondMoment);
        }

        // Отпечатки топологии и настройки адаптеров: сверяются и с паспортом
        // донора, и с отпечатками объявленных строк. Это доказывает, что модель
        // собрана именно того устройства, под которое снято обученное состояние.
        var identity = manifest.GetProperty("identity_literals_ru");
        var topologySha = Sha256Hex(Encoding.UTF8.GetBytes(TopologyLiteral));
        var loraSha = Sha256Hex(Encoding.UTF8.GetBytes(LoraLiteral));
        Assert.Equal(identity.GetProperty("expected_topology_sha256").GetString(), topologySha);
        Assert.Equal(identity.GetProperty("expected_lora_configuration_sha256").GetString(), loraSha);

        var savedRandom = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
            File.ReadAllBytes(Path.Combine(donor, "S0.rng.json")));
        Assert.NotNull(savedRandom);
        Assert.Equal(DonorStep, savedRandom!.RandomStateStep);
        Assert.Equal(topologySha, savedRandom.TopologySha256);
        Assert.Equal(loraSha, savedRandom.LoraConfigurationSha256);

        model.RestoreRandomState(savedRandom, new LlmModelRandomStateIdentity(topologySha, loraSha));

        // Состояние генератора сверяется по полям записей общим договором проекта,
        // а не через текстовую запись: строка скрыла бы расхождение курсора.
        RandomStateFieldAssertions.AssertEqual(
            savedRandom,
            model.CaptureRandomState(topologySha, loraSha, DonorStep));

        // ── Шаг 5. Чтение панели через узкий треугольник ─────────────────────

        // Донор записал свою панель по УЧЕБНЫМ строкам; экзаменационных показаний
        // в ней нет, и выдумывать для них эталон нельзя. Поэтому сравниваемые
        // строки берутся из самой панели донора, а её состав закрепляется: если
        // донор когда-нибудь запишет другой набор, проверка это заметит, а не
        // молча сверит меньше строк.
        var goldAfter = Panel(preparation.GetProperty("s0_panel"));
        var goldBefore = Panel(preparation.GetProperty("w0_panel"));
        var comparedIds = goldAfter.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "T1", "T2" }, comparedIds);
        Assert.Equal(comparedIds, goldBefore.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());

        // По одному диагностическому проходу на строку панели. Правильный ответ
        // предсказателю не передаётся: он берётся экзаменатором уже после ответа.
        var rows = manifest.GetProperty("panel_rows_ru");
        var observed = new Dictionary<string, PanelReadout>(StringComparer.Ordinal);
        foreach (var id in comparedIds)
        {
            var row = rows.GetProperty(id);
            observed[id] = Read(
                model,
                row.GetProperty("prompt").GetString()!,
                row.GetProperty("answer").GetString()![0],
                DonorConfig().VocabSize);
        }

        // Расход чтения равен числу записанных донором строк и не больше.
        Assert.Equal(
            manifest.GetProperty("budget_ru").GetProperty("forward").GetInt32(),
            observed.Count);

        // Главное утверждение: на восстановленном обученном состоянии узкий
        // треугольник даёт те же числа, что записал донор полным произведением.
        foreach (var id in comparedIds)
        {
            var expected = goldAfter[id];
            var actual = observed[id];

            Assert.Equal(expected.GoldId, actual.GoldId);
            Assert.Equal(expected.PredictedId, actual.PredictedId);
            Assert.Equal(expected.Exact, actual.Exact);
            Assert.Equal(expected.StrictRank, actual.StrictRank);
            Assert.Equal(expected.TieOrderedRank, actual.TieOrderedRank);

            // Логиты сравниваются ПЕРВЫМИ и побитово, а не приблизительно. Порядок
            // здесь часть доказательства: если когда-нибудь разойдётся мера ошибки
            // ниже, логиты к этому моменту уже доказаны одинаковыми, и причина
            // будет в арифметике экзаменатора, а не в продукте.
            Assert.Equal(expected.LastLogitsBase64, actual.LastLogitsBase64);
            // Отпечаток логитов всех позиций: ошибка маски на ранней строке не
            // обязана быть видна в последней позиции, поэтому читаются все.
            Assert.Equal(expected.AllLogitsSha256, actual.AllLogitsSha256);

            // Мера ошибки сверяется ПОБИТОВО по двойной точности, а не допуском.
            // Так согласование точности с донором нельзя подменить ослаблением:
            // при любом допуске отказ можно было бы спрятать, при побитовом
            // сравнении — нельзя. Фактическое расхождение печатается в журнал.
            var expectedBits = BitConverter.DoubleToInt64Bits(expected.Nll);
            var actualBits = BitConverter.DoubleToInt64Bits(actual.Nll);
            _output.WriteLine(
                $"строка {id}: мера ошибки донора {expected.Nll:R}, наблюдённая {actual.Nll:R}, "
                + $"расхождение {actual.Nll - expected.Nll:R}, разница представлений {actualBits - expectedBits}");
            Assert.Equal(expectedBits, actualBits);
        }

        // ── Шаг 6. Защита от пустой проверки ────────────────────────────────

        // Восстановлено именно ОБУЧЕННОЕ состояние, а не чистая модель: до
        // обучения первая учебная строка была неверна и правильный ответ стоял
        // на двести двадцать девятом месте, после обучения она верна и первый.
        Assert.False(goldBefore["T1"].Exact);
        Assert.True(goldAfter["T1"].Exact);
        Assert.True(goldBefore["T1"].StrictRank > goldAfter["T1"].StrictRank);
        Assert.Equal(1, observed["T1"].StrictRank);
        Assert.True(observed["T1"].Exact);
        // И наблюдённые числа отличаются от необученных: если бы веса не
        // загрузились, панель совпала бы с состоянием до обучения.
        Assert.NotEqual(goldBefore["T1"].LastLogitsBase64, observed["T1"].LastLogitsBase64);

        // Экзаменационные строки донора остаются как есть и успехом не
        // объявляются: усвоена одна учебная строка, это не общее правило знака.
        Assert.Equal(
            manifest.GetProperty("independent_gold_ru").GetProperty("acquired_ids")
                .EnumerateArray().Select(value => value.GetString()).ToArray(),
            preparation.GetProperty("acquired_ids").EnumerateArray().Select(value => value.GetString()).ToArray());

        // ── Шаг 7. Прямоугольный путь кеша прошлого не присвоен треугольником ──

        // Строка очереди прямо запрещает менять прямоугольный путь. Узкое ядро
        // обязано отказаться от блока, где ключей прошлого больше, чем запросов:
        // там главная диагональ уже не граница видимости, и такой блок остаётся
        // на прежнем полном произведении.
        var square = Spread(1, 2, 4, 4, seed: 0);
        var pastKeys = TensorOps.Transpose(Spread(1, 2, 6, 4, seed: 5), -2, -1);
        Assert.Throws<ArgumentException>(() => TensorOps.CausalScores(square, pastKeys));
    }

    // Четыре настоящих имени обучаемых массивов адаптеров, выведённые руками из
    // устройства донора: один слой, низкоранговые адаптеры на проекциях запроса и
    // значения, у каждой — входная и выходная матрица. Эти имена записаны здесь
    // как ожидаемые постоянные, а не прочитаны из проверяемого объекта.
    private static readonly string[] ExpectedAdapterKeys =
    [
        "lora.block_0000.q_proj.lora_a",
        "lora.block_0000.q_proj.lora_b",
        "lora.block_0000.v_proj.lora_a",
        "lora.block_0000.v_proj.lora_b",
    ];

    // Ожидаемые формы тех же четырёх массивов, выведённые руками из устройства
    // донора: скрытая размерность 64, ранг 4, а у проекции значения выход равен
    // двум головам ключей по шестнадцать, то есть 32. Числа элементов 256, 256,
    // 256 и 128 следуют из этих форм и проверяются отдельно.
    private static readonly Dictionary<string, int[]> ExpectedAdapterShapes = new(StringComparer.Ordinal)
    {
        ["lora.block_0000.q_proj.lora_a"] = [4, 64],
        ["lora.block_0000.q_proj.lora_b"] = [64, 4],
        ["lora.block_0000.v_proj.lora_a"] = [4, 64],
        ["lora.block_0000.v_proj.lora_b"] = [32, 4],
    };

    // Эта проверка закрывает вторую половину названной очередью работы:
    // восстановление ПОЛНОГО состояния, а не только весов.
    //
    // Что здесь доказывается человеческими словами. Соседняя проверка выше читает
    // панель донора и доказывает, что на восстановленных весах узкий треугольник
    // даёт те же числа. Но «полное состояние» — это ещё и моменты оптимизатора и
    // номер его шага. Их прежняя проверка сверяла только сами с собой: что
    // выгрузили, то и загрузили из одного и того же двоичного файла. Такая сверка
    // не заметила бы, что состояние разложено не по настоящим именам параметров, а
    // по порядку обхода. Здесь ожидаемое берётся из ДРУГОГО, ранее записанного
    // источника — из показаний подготовки донора, — и сверяется ПО ИМЕНИ.
    //
    // Вторая закрываемая часть — назначенная договором типизированная проверка
    // полной контрольной точки по явно объявленному устройству адаптеров. Прежде
    // веса загружались без неё, то есть объявленное устройство никто не сверял.
    //
    // Расход: ноль прямых проходов, ноль обратных, ноль обновлений оптимизатора.
    // Это чтение и восстановление, а не обучение.
    [Fact]
    public void RestoredFullStateMatchesTheDonorIndependentRecordByParameterName()
    {
        // ── Шаг 1. Паспорт и байты донора под теми же закреплёнными отпечатками ──

        var manifestPath = Path.Combine(
            RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1",
            "CausalScoreTrainedStart01", "manifest.json");
        using var manifestDocument = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        var manifest = manifestDocument.RootElement;

        // Донор читается с его места: обученные веса в проверку не копируются.
        var donor = Path.Combine(
            RepositoryRoot(),
            manifest.GetProperty("donor_relative_path").GetString()!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(Directory.Exists(donor), "DONOR_START_DIRECTORY_NOT_FOUND:" + donor);

        // Молчаливая подмена донора обязана быть видна и этой проверке тоже.
        foreach (var pinned in manifest.GetProperty("whole_file_sha256").EnumerateObject())
        {
            var path = Path.Combine(donor, pinned.Name);
            Assert.True(File.Exists(path), "DONOR_FILE_NOT_FOUND:" + path);
            Assert.Equal(pinned.Value.GetString(), Sha256Hex(File.ReadAllBytes(path)));
        }

        // ── Шаг 2. Назначенная типизированная проверка полной контрольной точки ──

        // Объявленное устройство адаптеров берётся из руками выведенных постоянных:
        // ранг четыре и ровно две цели — проекции запроса и значения.
        var declaredTopology = new FullCheckpointLoraTopology(4, ["q_proj", "v_proj"]);
        var weightBytes = File.ReadAllBytes(Path.Combine(donor, "S0.weights.bin"));

        using (var admissionStream = new MemoryStream(weightBytes, writable: false))
        {
            // Настоящий продуктовый путь приёма артефакта: он сверяет число записей,
            // их имена, формы, конечность значений и точный конец файла.
            var admitted = LlmModel.ValidateFullCheckpointArtifact(
                admissionStream, DonorConfig(), declaredTopology, admissionStream.Length);
            Assert.True(admitted.IsSuccess, admitted.ErrorCode + ": " + admitted.ErrorMessage);
            Assert.True(admitted.Value);
            // Поток обязан быть прочитан ровно до конца: лишний хвост означал бы,
            // что часть артефакта никто не сверил.
            Assert.Equal(admissionStream.Length, admissionStream.Position);
        }

        // Два отрицательных контроля: без них «зелено» ничего не значило бы, потому
        // что проверка, принимающая любое устройство, не проверяет устройство.
        using (var wrongRankStream = new MemoryStream(weightBytes, writable: false))
        {
            // Другой ранг обязан быть отклонён именно по несовпадению формы.
            var wrongRank = LlmModel.ValidateFullCheckpointArtifact(
                wrongRankStream, DonorConfig(), new FullCheckpointLoraTopology(3, ["q_proj", "v_proj"]),
                wrongRankStream.Length);
            Assert.False(wrongRank.IsSuccess);
            Assert.Equal("LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH", wrongRank.ErrorCode);
        }

        using (var wrongTargetsStream = new MemoryStream(weightBytes, writable: false))
        {
            // Другие цели адаптеров обязаны быть отклонены по несовпадению имён.
            var wrongTargets = LlmModel.ValidateFullCheckpointArtifact(
                wrongTargetsStream, DonorConfig(), new FullCheckpointLoraTopology(4, ["k_proj", "o_proj"]),
                wrongTargetsStream.Length);
            Assert.False(wrongTargets.IsSuccess);
            Assert.Equal("LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH", wrongTargets.ErrorCode);
        }

        // ── Шаг 3. Та же модель донора и связки по настоящим именам ──────────

        // Детерминированное зерно охватывает только конструктор, как у хозяина опыта.
        LlmModel model;
        using (Initializers.UseDeterministicSeed(ConstructionSeed))
            model = new LlmModel(DonorConfig());
        var adapters = LoraApplicator.ApplySeeded(model, DonorLora(), LoraSeed, StreamDomain);

        // Связка «настоящий ключ контрольной точки — обучаемый массив». Ключ берётся
        // у модели, но ожидаемый НАБОР ключей задан руками выше.
        var checkpointEntries = model.GetCheckpointParameterEntries();
        var bindings = new List<AdamWParameterBinding>();
        foreach (var parameter in adapters.SelectMany(adapter => adapter.AdapterParameters.Select(pair => pair.Parameter)))
        {
            var entry = checkpointEntries.FirstOrDefault(candidate => ReferenceEquals(candidate.Parameter, parameter));
            Assert.NotNull(entry);
            bindings.Add(new AdamWParameterBinding(entry!.Key, parameter));
        }

        // Набор связанных имён обязан совпасть с руками выведенным ожидаемым набором.
        Assert.Equal(
            ExpectedAdapterKeys.OrderBy(key => key, StringComparer.Ordinal).ToArray(),
            bindings.Select(binding => binding.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.Equal(ExpectedAdapterKeys.Length, bindings.Count);

        // Обученные веса загружаются тем же продуктовым путём, что и в соседней проверке.
        using (var weights = new MemoryStream(weightBytes, writable: false))
            model.LoadCheckpoint(weights);

        // ── Шаг 4. Независимый эталон полного состояния оптимизатора ─────────

        // Показания подготовки донора — ДРУГОЙ источник, записанный до этой приёмки.
        // Именно поэтому сверка с ним является сверкой, а не тавтологией.
        using var preparationDocument = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(donor, "preparation.json")));
        var preparation = preparationDocument.RootElement;
        var goldOptimizer = preparation.GetProperty("optimizer_state");

        // Номер шага равен четырёмстам в руками выведенной постоянной и обязан
        // совпасть с независимой записью: донор выполнил четыреста обновлений.
        Assert.Equal(DonorStep, goldOptimizer.GetProperty("step").GetInt32());
        Assert.Equal(DonorStep, preparation.GetProperty("completed_updates").GetInt32());

        // Независимый эталон раскладывается по ИМЕНИ параметра, а не по позиции.
        var goldByName = goldOptimizer.GetProperty("entries").EnumerateArray()
            .ToDictionary(entry => entry.GetProperty("key").GetString()!, entry => entry, StringComparer.Ordinal);
        Assert.Equal(
            ExpectedAdapterKeys.OrderBy(key => key, StringComparer.Ordinal).ToArray(),
            goldByName.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());

        // Восстановление идёт настоящим продуктовым путём: разбор переносимого
        // состояния и ввод его в оптимизатор по связкам.
        var optimizer = new AdamW(beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);
        var savedOptimizer = AdamWPortableState.FromBytes(File.ReadAllBytes(Path.Combine(donor, "S0.optimizer.bin")));
        Assert.NotNull(savedOptimizer);
        Assert.True(optimizer.TryImportPortableState(bindings, savedOptimizer!));

        // Выгрузка сверяется с независимым эталоном ПО ИМЕНИ: шаг, форма и оба
        // момента побитово. Допуска здесь нет намеренно: при допуске отказ можно
        // было бы спрятать, при побитовом сравнении — нельзя.
        var exported = optimizer.ExportPortableState(bindings);
        Assert.Equal(DonorStep, exported.Step);
        Assert.Equal(ExpectedAdapterKeys.Length, exported.Entries.Count);

        foreach (var entry in exported.Entries)
        {
            Assert.True(goldByName.TryGetValue(entry.Key, out var gold), "OPTIMIZER_KEY_NOT_IN_DONOR_RECORD:" + entry.Key);

            // Форма сверяется и с руками выведенной постоянной, и с независимой записью.
            Assert.Equal(ExpectedAdapterShapes[entry.Key], entry.Dimensions);
            Assert.Equal(
                gold.GetProperty("dimensions").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
                entry.Dimensions);

            // Число элементов обязано следовать из формы: иначе часть состояния
            // осталась бы несверенной и молча отличалась.
            var expectedCount = ExpectedAdapterShapes[entry.Key][0] * ExpectedAdapterShapes[entry.Key][1];
            Assert.Equal(expectedCount, entry.FirstMoment.Count);
            Assert.Equal(expectedCount, entry.SecondMoment.Count);

            // Оба момента — побитово против записанных донором байтов.
            Assert.Equal(
                Convert.FromBase64String(gold.GetProperty("first_moment_bits_base64").GetString()!),
                FloatBits(entry.FirstMoment));
            Assert.Equal(
                Convert.FromBase64String(gold.GetProperty("second_moment_bits_base64").GetString()!),
                FloatBits(entry.SecondMoment));
        }

        // ── Шаг 5. Контроль «по имени, а не по порядку обхода» ───────────────

        // Если восстановление раскладывает состояние по порядку, а не по имени, то
        // при перевёрнутом порядке связок моменты сядут на чужие параметры. Тогда
        // выгрузка в исходном порядке разойдётся с независимым эталоном, причём у
        // проекции значения разойдётся даже длина: 128 элементов против 256.
        var permutedBindings = bindings.AsEnumerable().Reverse().ToList();
        Assert.NotEqual(
            bindings.Select(binding => binding.Key).ToArray(),
            permutedBindings.Select(binding => binding.Key).ToArray());

        var permutedOptimizer = new AdamW(beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);
        var permutedSaved = AdamWPortableState.FromBytes(File.ReadAllBytes(Path.Combine(donor, "S0.optimizer.bin")));
        Assert.NotNull(permutedSaved);
        Assert.True(permutedOptimizer.TryImportPortableState(permutedBindings, permutedSaved!));

        // Выгрузка запрашивается в ИСХОДНОМ порядке связок после перевёрнутого ввода.
        var afterPermutedImport = permutedOptimizer.ExportPortableState(bindings);
        Assert.Equal(DonorStep, afterPermutedImport.Step);
        foreach (var entry in afterPermutedImport.Entries)
        {
            var gold = goldByName[entry.Key];
            // Те же числа на тех же именах: порядок связок на результат не влияет.
            Assert.Equal(
                Convert.FromBase64String(gold.GetProperty("first_moment_bits_base64").GetString()!),
                FloatBits(entry.FirstMoment));
            Assert.Equal(
                Convert.FromBase64String(gold.GetProperty("second_moment_bits_base64").GetString()!),
                FloatBits(entry.SecondMoment));
        }

        // ── Шаг 6. Номер шага генератора против независимой записи ───────────

        // Прежде номер шага генератора сверялся только с его собственным файлом.
        // Здесь он сверяется с отдельной записью подготовки донора.
        Assert.Equal(DonorStep, preparation.GetProperty("randomStateStep").GetInt32());

        var savedRandom = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
            File.ReadAllBytes(Path.Combine(donor, "S0.rng.json")));
        Assert.NotNull(savedRandom);
        Assert.Equal(preparation.GetProperty("randomStateStep").GetInt32(), savedRandom!.RandomStateStep);

        // Восстановление генератора идёт тем же продуктовым путём и под теми же
        // отпечатками устройства и настройки адаптеров.
        var topologySha = Sha256Hex(Encoding.UTF8.GetBytes(TopologyLiteral));
        var loraSha = Sha256Hex(Encoding.UTF8.GetBytes(LoraLiteral));
        model.RestoreRandomState(savedRandom, new LlmModelRandomStateIdentity(topologySha, loraSha));
        RandomStateFieldAssertions.AssertEqual(
            savedRandom,
            model.CaptureRandomState(topologySha, loraSha, DonorStep));

        // Журнал прогона: наблюдённые числа попадают в вывод проверки, а не
        // остаются только в голове исполнителя.
        _output.WriteLine(
            $"полное состояние: шаг {exported.Step}, обучаемых массивов {exported.Entries.Count}, "
            + $"имена сверены с независимой записью донора, прямых проходов 0");
    }

    // ── Чтение одной строки панели ──────────────────────────────────────────

    // Один диагностический проход по подсказке, без обновления. Соглашение то
    // же, что у хозяина опыта: вход — байты подсказки, читается последняя
    // позиция, ничьи разрешаются меньшим номером.
    private static PanelReadout Read(LlmModel model, string prompt, char answer, int vocab)
    {
        var promptTokens = Encoding.UTF8.GetBytes(prompt).Select(value => (int)value).ToArray();
        var input = new Tensor(
            promptTokens.Select(token => (float)token).ToArray(),
            new TensorShape(1, promptTokens.Length));

        var logits = model.Forward(input, training: false);

        var positions = promptTokens.Length;
        var all = new float[positions * vocab];
        Array.Copy(logits.Data, 0, all, 0, all.Length);
        var last = new float[vocab];
        Array.Copy(logits.Data, (positions - 1) * vocab, last, 0, vocab);

        var gold = (int)(byte)answer;
        var best = 0;
        for (var id = 1; id < vocab; id++)
            if (last[id] > last[best]) best = id;

        // Точность здесь существенна и повторяет соглашение донора дословно:
        // максимум берётся в ДВОЙНОЙ точности. Если оставить его одинарной, то
        // вычитание внутри суммы округляется иначе и мера ошибки расходится в
        // девятом знаке при тех же самых логитах.
        double max = last.Max();
        double sumExp = last.Sum(value => Math.Exp(value - max));
        double logSumExp = max + Math.Log(sumExp);
        var nll = logSumExp - last[gold];

        var strict = 1 + last.Count(value => value > last[gold]);
        var tieOrdered = strict;
        for (var id = 0; id < gold; id++)
            if (last[id] == last[gold]) tieOrdered++;

        return new PanelReadout(
            gold, best, best == gold, nll, strict, tieOrdered,
            Convert.ToBase64String(FloatBits(last)),
            Sha256Hex(FloatBits(all)));
    }

    // Записанная донором панель, по опознавательному имени строки.
    private static Dictionary<string, PanelReadout> Panel(JsonElement panel)
        => panel.EnumerateArray().ToDictionary(
            row => row.GetProperty("id").GetString()!,
            row => new PanelReadout(
                row.GetProperty("gold_id").GetInt32(),
                row.GetProperty("predicted_id").GetInt32(),
                row.GetProperty("exact").GetBoolean(),
                row.GetProperty("nll").GetDouble(),
                row.GetProperty("strict_gold_rank").GetInt32(),
                row.GetProperty("tie_ordered_gold_rank").GetInt32(),
                row.GetProperty("last_position_logits_base64").GetString()!,
                row.GetProperty("all_positions_logits_sha256").GetString()!),
            StringComparer.Ordinal);

    // ── Устройство донора ───────────────────────────────────────────────────

    private static ModelConfig DonorConfig() => new()
    {
        VocabSize = 256, ContextLen = 64, HiddenDim = 64, NumLayers = 1,
        NumHeads = 4, NumKvHeads = 2, FfnHiddenDim = 176,
        RopeTheta = 10_000f, RmsNormEps = 1e-5f,
    };

    private static LoraConfig DonorLora() => new()
    {
        Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"],
    };

    // ── Мелкие помощники ────────────────────────────────────────────────────

    // Детерминированный вход для проверки отказа на прямоугольном блоке.
    private static Tensor Spread(int batch, int heads, int time, int depth, int seed)
    {
        var data = new float[batch * heads * time * depth];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.2f + (0.007f * ((i + seed) % 211));
        return new Tensor(data, new TensorShape([batch, heads, time, depth]));
    }

    // Биты чисел в том же порядке, в каком их записывал донор.
    private static byte[] FloatBits(IReadOnlyList<float> values)
    {
        var buffer = new byte[values.Count * 4];
        for (var i = 0; i < values.Count; i++)
            BitConverter.GetBytes(values[i]).CopyTo(buffer, i * 4);
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
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("CAUSAL_SCORE_TRAINED_START_REPOSITORY_ROOT_NOT_FOUND");
    }

    // Одно прочитанное показание панели.
    private sealed record PanelReadout(
        int GoldId,
        int PredictedId,
        bool Exact,
        double Nll,
        int StrictRank,
        int TieOrderedRank,
        string LastLogitsBase64,
        string AllLogitsSha256);
}
