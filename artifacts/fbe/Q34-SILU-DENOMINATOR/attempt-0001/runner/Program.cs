// Оболочка минимальной проверки переноса «сохранить знаменатель плавного вентиля
// до обратного прохода» (раздел переноса Q34, якорь q34-min, карточка
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/47-silu-sigmoid-reuse.md).
//
// Она делает ровно то, что названо карточкой и нарядом: показывает наблюдаемое
// различие, доказывающее, что обратный проход читает сохранённое значение;
// показывает, что продуктовый слой передаёт признак обучения и числа при этом не
// меняются; восстанавливает общий обученный старт h64-silu/v1, берёт один урок и
// выполняет ОДИН обычный шаг обучения с учителем. Процент ускорения здесь заново
// не измеряется и не обещается.
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using TransformerModel.Layers;

// Два входа задаются снаружи, чтобы в оболочке не было спрятанных путей.
var sharedRoot = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);

// Удостоверения фактически загруженных сборок: по ним видно, что прогон прошёл
// именно через свежесобранные изменённые проекты, а не через старые библиотеки.
var tensorAssembly = typeof(TensorOps).Assembly;
var modelAssembly = typeof(LlmModel).Assembly;
var trainerAssembly = typeof(Trainer).Assembly;
foreach (var assembly in new[] { tensorAssembly, modelAssembly, trainerAssembly })
{
    Console.WriteLine("ASSEMBLY " + assembly.GetName().Name
        + " " + assembly.Location
        + " MVID " + assembly.ManifestModule.ModuleVersionId);
}

// Наличие новой перегрузки проверяется по фактически загруженной сборке, а не по
// исходнику: так видно, что собран именно изменённый плавный вентиль.
var cachedSiluOverload = typeof(TensorOps).GetMethod(
    "Silu",
    new[] { typeof(Tensor), typeof(bool), typeof(CancellationToken) });
var cachedSwiGluOverload = typeof(SwiGLUFeedForward).GetMethod(
    "Forward",
    new[] { typeof(Tensor), typeof(bool), typeof(CancellationToken) });
Console.WriteLine("SILU_CACHED_OVERLOAD_PRESENT " + (cachedSiluOverload is not null));
Console.WriteLine("SWIGLU_TRAINING_OVERLOAD_PRESENT " + (cachedSwiGluOverload is not null));

// ── Контроль 1: тот же вход, те же числа ────────────────────────────────────────
//
// Учебный путь и прежний путь на неизменённом входе обязаны дать производную,
// совпадающую бит в бит: сохранённый знаменатель — ровно тот же float, который
// прежний обратный проход вычислял заново.
var probeValues = new[] { -8f, -2f, -0.5f, 0f, 0.5f, 2f, 8f };
var upstreamValues = new[] { 1f, -2f, 0.25f, 3f, -0.75f, 2f, -1f };

var unchangedCached = SiluGradient(probeValues, upstreamValues, useCache: true, mutate: false);
var unchangedLegacy = SiluGradient(probeValues, upstreamValues, useCache: false, mutate: false);
var unchangedBitwiseEqual = BitsEqual(unchangedCached, unchangedLegacy);
Console.WriteLine("UNCHANGED_INPUT_BITWISE_EQUAL " + unchangedBitwiseEqual);

// ── Контроль 2: вход изменён между прямым и обратным проходом ───────────────────
//
// Это и есть наблюдаемое доказательство того, что обратный проход учебного вызова
// читает СОХРАНЁННОЕ значение: сигмоида у него остаётся от старого входа, тогда
// как прежний путь пересчитывает экспоненту от нового. Градиенты обязаны
// РАЗЛИЧАТЬСЯ. Если бы правка не работала, они совпали бы.
var mutatedCached = SiluGradient(probeValues, upstreamValues, useCache: true, mutate: true);
var mutatedLegacy = SiluGradient(probeValues, upstreamValues, useCache: false, mutate: true);
var mutatedDiffers = !BitsEqual(mutatedCached, mutatedLegacy);
Console.WriteLine("MUTATED_INPUT_GRADIENTS_DIFFER " + mutatedDiffers);

// Замороженный вход при включённом признаке остаётся без градиента и без графа:
// массив знаменателей в этом режиме не создаётся и создавать его некому.
var frozenInput = new Tensor(probeValues.ToArray(), new TensorShape(probeValues.Length));
var frozenResult = TensorOps.Silu(frozenInput, cacheBackwardDenominator: true, CancellationToken.None);
var frozenLegacy = TensorOps.Silu(
    new Tensor(probeValues.ToArray(), new TensorShape(probeValues.Length)),
    CancellationToken.None);
var frozenForwardEqual = BitsEqual(frozenResult.Data, frozenLegacy.Data);
Console.WriteLine("FROZEN_INPUT_REQUIRES_GRAD " + frozenResult.RequiresGrad);
Console.WriteLine("FROZEN_INPUT_FORWARD_BITWISE_EQUAL " + frozenForwardEqual);

// ── Контроль 3: продуктовый слой сети прямого распространения ───────────────────
//
// Учебная перегрузка сети прямого распространения и прежняя перегрузка на
// неизменённом входе обязаны дать одинаковые значения и одинаковые градиенты:
// перенос меняет только то, откуда берётся сигмоида, а не саму математику.
var ffnConfig = new ModelConfig { HiddenDim = 4, FfnHiddenDim = 8 };
SwiGLUFeedForward feedForward;
using (Initializers.UseDeterministicSeed(62003UL)) feedForward = new SwiGLUFeedForward(ffnConfig);
var ffnInputValues = new[] { -1f, -0.5f, 0f, 0.5f, 1f, 1.5f, -2f, 2f };
var ffnUpstreamValues = new[] { 0.5f, -1f, 1.5f, 2f, -0.25f, 0.75f, -0.5f, 1f };
var ffnTraining = FeedForwardGradients(feedForward, ffnInputValues, ffnUpstreamValues, training: true);
var ffnLegacy = FeedForwardGradients(feedForward, ffnInputValues, ffnUpstreamValues, training: false);
var ffnForwardEqual = BitsEqual(ffnTraining.Forward, ffnLegacy.Forward);
var ffnInputGradEqual = BitsEqual(ffnTraining.InputGradient, ffnLegacy.InputGradient);
var ffnParameterGradEqual = BitsEqual(ffnTraining.ParameterGradient, ffnLegacy.ParameterGradient);
Console.WriteLine("FFN_FORWARD_BITWISE_EQUAL " + ffnForwardEqual);
Console.WriteLine("FFN_INPUT_GRADIENT_BITWISE_EQUAL " + ffnInputGradEqual);
Console.WriteLine("FFN_PARAMETER_GRADIENT_BITWISE_EQUAL " + ffnParameterGradEqual);

// ── Контроль 4: признак обучения доходит до вентиля через блок трансформера ─────
//
// Датчик здесь — дополнительно выделенные байты. Массив знаменателей занимает
// ровно 4 байта на элемент вентиля, то есть positions × FfnHiddenDim × 4. Если
// признак действительно доходит до вентиля, учебный проход блока выделяет на эту
// величину больше, чем проход без обучения. Это проверка пути, а не измерение
// скорости.
var blockConfig = new ModelConfig
{
    VocabSize = 256,
    ContextLen = 64,
    HiddenDim = 64,
    NumLayers = 1,
    NumHeads = 4,
    NumKvHeads = 2,
    FfnHiddenDim = 176,
    RopeTheta = 10000f,
    RmsNormEps = 1e-5f
};
const int blockPositions = 21;
var expectedDenominatorBytes = (long)blockPositions * blockConfig.FfnHiddenDim * sizeof(float);
TransformerBlock probeBlock;
using (Initializers.UseDeterministicSeed(62001UL)) probeBlock = new TransformerBlock(blockConfig);
// Прогрев: первые проходы включают разовые расходы, в датчик они не входят.
_ = MeasureBlockAllocations(probeBlock, blockConfig, blockPositions, training: false);
_ = MeasureBlockAllocations(probeBlock, blockConfig, blockPositions, training: true);
var trainingAllocations = new long[3];
var inferenceAllocations = new long[3];
for (var repeat = 0; repeat < trainingAllocations.Length; repeat++)
{
    inferenceAllocations[repeat] = MeasureBlockAllocations(probeBlock, blockConfig, blockPositions, training: false);
    trainingAllocations[repeat] = MeasureBlockAllocations(probeBlock, blockConfig, blockPositions, training: true);
}

var allocationDeltas = new long[trainingAllocations.Length];
for (var repeat = 0; repeat < allocationDeltas.Length; repeat++)
    allocationDeltas[repeat] = trainingAllocations[repeat] - inferenceAllocations[repeat];
Console.WriteLine("BLOCK_EXPECTED_DENOMINATOR_BYTES "
    + expectedDenominatorBytes.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("BLOCK_ALLOCATION_DELTAS "
    + string.Join(",", allocationDeltas.Select(value => value.ToString(CultureInfo.InvariantCulture))));

// ── Назначенная карточкой проверка: один учебный шаг из общего старта ───────────
//
// Удостоверение старта читается до любого обучения: ни один байт не берётся на
// веру, семейство и его отпечатки названы явно.
// Имена полей в удостоверении записаны со строчной первой буквы, поэтому
// сравнение имён идёт без учёта регистра.
var authority = JsonSerializer.Deserialize<StartAuthority>(
        File.ReadAllBytes(Path.Combine(sharedRoot, "authority.json")),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidOperationException("Q34_START_AUTHORITY_UNREADABLE");
// Отсутствие любого из трёх отпечатков останавливает прогон до обучения.
if (string.IsNullOrEmpty(authority.TopologySha256)
    || string.IsNullOrEmpty(authority.LoraConfigurationSha256)
    || string.IsNullOrEmpty(authority.RandomStateStreamDomain))
{
    throw new InvalidOperationException("Q34_START_AUTHORITY_INCOMPLETE");
}
var weightsPath = Path.Combine(sharedRoot, "S0.weights.bin");
var optimizerPath = Path.Combine(sharedRoot, "S0.optimizer.bin");
var randomStatePath = Path.Combine(sharedRoot, "S0.rng.json");
var weightsBytes = File.ReadAllBytes(weightsPath);
var weightsSha = Convert.ToHexString(SHA256.HashData(weightsBytes)).ToLowerInvariant();
Console.WriteLine("START_WEIGHTS_BYTES " + weightsBytes.LongLength.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("START_WEIGHTS_SHA256 " + weightsSha);

// Модель создаётся под базовым зерном старта, адаптеры назначаются ДО загрузки
// весов: загрузка весов топологию не создаёт.
var modelConfig = new ModelConfig
{
    VocabSize = 256,
    ContextLen = 64,
    HiddenDim = 64,
    NumLayers = 1,
    NumHeads = 4,
    NumKvHeads = 2,
    FfnHiddenDim = 176,
    RopeTheta = 10000f,
    RmsNormEps = 1e-5f
};
LlmModel model;
using (Initializers.UseDeterministicSeed(62001UL)) model = new LlmModel(modelConfig);
var loraConfig = new LoraConfig
{
    Rank = 4,
    Alpha = 8f,
    Dropout = 0f,
    TargetModules = ["q_proj", "v_proj"]
};
LoraApplicator.ApplySeeded(model, loraConfig, 62002UL, authority.RandomStateStreamDomain);
model.LoadCheckpoint(weightsPath);

// Состав обучаемых и замороженных массивов проверяется до обучения.
var trainable = model.Parameters().Where(parameter => parameter.Value.RequiresGrad).ToArray();
var frozen = model.Parameters().Where(parameter => !parameter.Value.RequiresGrad).ToArray();
Console.WriteLine("START_TRAINABLE_ARRAYS " + trainable.Length.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("START_FROZEN_ARRAYS " + frozen.Length.ToString(CultureInfo.InvariantCulture));

// Привязки «имя параметра — тензор» берутся из записей контрольной точки, а не по
// случайному порядку массивов.
AdamWParameterBinding[] Bindings() => model.GetCheckpointParameterEntries()
    .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
    .ToArray();

// Оптимизатор восстанавливается из переносимого состояния старта.
var optimizer = new AdamW(beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);
var portable = AdamWPortableState.FromBytes(File.ReadAllBytes(optimizerPath))
    ?? throw new InvalidOperationException("Q34_START_OPTIMIZER_UNREADABLE");
if (!optimizer.TryImportPortableState(Bindings(), portable))
    throw new InvalidOperationException("Q34_START_OPTIMIZER_IMPORT_REFUSED");
Console.WriteLine("START_OPTIMIZER_STEP " + portable.Step.ToString(CultureInfo.InvariantCulture));

// Состояние датчика случайных чисел восстанавливается с точными отпечатками
// топологии и адаптеров: чужой старт эту проверку не пройдёт.
var randomState = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
        File.ReadAllBytes(randomStatePath))
    ?? throw new InvalidOperationException("Q34_START_RANDOM_STATE_UNREADABLE");
model.RestoreRandomState(
    randomState,
    new LlmModelRandomStateIdentity(authority.TopologySha256, authority.LoraConfigurationSha256));
Console.WriteLine("START_RANDOM_STATE_RESTORED true");

// Единственный урок карточки и ожидаемый ответ.
const string lessonPrompt = "C# result sign: 9-4 =";
const string lessonAnswer = "+";
var lessonIds = Utf8Bytes(lessonPrompt + lessonAnswer);
var goldTokenId = Utf8Bytes(lessonAnswer)[0];
Console.WriteLine("LESSON_PROMPT " + lessonPrompt);
Console.WriteLine("LESSON_GOLD_TOKEN " + goldTokenId.ToString(CultureInfo.InvariantCulture));

// Чтение ответа ДО обучения тем же способом, что и после.
var before = ReadAnswer(model, lessonPrompt, goldTokenId);
var trainableBefore = ValuesHash(trainable);
var frozenBefore = ValuesHash(frozen);
Console.WriteLine("BEFORE_PREDICTED_TOKEN " + before.PredictedTokenId.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("BEFORE_EXACT " + before.Exact);

// ОДИН обычный шаг обучения с учителем: он и проходит по изменённому обратному
// проходу, потому что учебный проход модели передаёт признак обучения до вентиля.
var trainer = new Trainer(new TrainingConfig
{
    BatchSize = 1,
    SeqLen = 64,
    WarmupSteps = 0,
    TotalSteps = 400,
    MaxLr = 1e-3f,
    MinLr = 1e-3f,
    GradClipNorm = 1f,
    GradAccumSteps = 1,
    WeightDecay = 0f
});
var loss = trainer.TrainSupervisedNextTokenStep(model, optimizer, lessonIds, weight: 1f);
Console.WriteLine("STEP_LOSS " + loss.ToString("R", CultureInfo.InvariantCulture));
Console.WriteLine("STEP_LOSS_FINITE " + float.IsFinite(loss));

// Показания после шага: тот же читатель, тот же урок, тот же ожидаемый ответ.
var optimizerStepAfter = optimizer.ExportPortableState(Bindings()).Step;
var trainableAfter = ValuesHash(trainable);
var frozenAfter = ValuesHash(frozen);
var after = ReadAnswer(model, lessonPrompt, goldTokenId);
Console.WriteLine("AFTER_OPTIMIZER_STEP " + optimizerStepAfter.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("AFTER_PREDICTED_TOKEN " + after.PredictedTokenId.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("AFTER_EXACT " + after.Exact);
Console.WriteLine("TRAINABLE_CHANGED " + (trainableBefore != trainableAfter));
Console.WriteLine("FROZEN_UNCHANGED " + (frozenBefore == frozenAfter));
Console.WriteLine("LEARNED_SIGN_PRESERVED " + (before.Exact && after.Exact));

// Сырые наблюдения сохраняются рядом как улика, без оценок и без вердиктов.
var observed = new
{
    card = "Q34 — один учебный шаг с сохранённым знаменателем плавного вентиля",
    assemblies = new[] { tensorAssembly, modelAssembly, trainerAssembly }.Select(assembly => new
    {
        name = assembly.GetName().Name,
        location = assembly.Location,
        moduleVersionId = assembly.ManifestModule.ModuleVersionId.ToString()
    }).ToArray(),
    overloads = new
    {
        siluCachedPresent = cachedSiluOverload is not null,
        swiGluTrainingPresent = cachedSwiGluOverload is not null
    },
    backwardReadsStoredDenominator = new
    {
        unchangedInputBitwiseEqual = unchangedBitwiseEqual,
        unchangedCachedGradient = unchangedCached,
        unchangedLegacyGradient = unchangedLegacy,
        mutatedInputGradientsDiffer = mutatedDiffers,
        mutatedCachedGradient = mutatedCached,
        mutatedLegacyGradient = mutatedLegacy
    },
    frozenAndDisabledModes = new
    {
        requiresGrad = frozenResult.RequiresGrad,
        forwardBitwiseEqual = frozenForwardEqual
    },
    feedForwardLayer = new
    {
        forwardBitwiseEqual = ffnForwardEqual,
        inputGradientBitwiseEqual = ffnInputGradEqual,
        parameterGradientBitwiseEqual = ffnParameterGradEqual
    },
    transformerBlockPath = new
    {
        positions = blockPositions,
        ffnHiddenDim = blockConfig.FfnHiddenDim,
        expectedDenominatorBytes,
        inferenceAllocations,
        trainingAllocations,
        allocationDeltas
    },
    start = new
    {
        sharedRoot,
        weightsBytes = weightsBytes.LongLength,
        weightsSha256 = weightsSha,
        topologySha256 = authority.TopologySha256,
        loraConfigurationSha256 = authority.LoraConfigurationSha256,
        randomStateStreamDomain = authority.RandomStateStreamDomain,
        trainableArrays = trainable.Length,
        frozenArrays = frozen.Length,
        optimizerStepBefore = portable.Step
    },
    lesson = new
    {
        prompt = lessonPrompt,
        answer = lessonAnswer,
        goldTokenId,
        tokens = lessonIds.Length
    },
    singleStep = new
    {
        loss,
        lossBits = BitConverter.SingleToInt32Bits(loss),
        lossFinite = float.IsFinite(loss),
        optimizerStepBefore = portable.Step,
        optimizerStepAfter,
        trainableChanged = trainableBefore != trainableAfter,
        frozenUnchanged = frozenBefore == frozenAfter,
        trainableHashBefore = trainableBefore,
        trainableHashAfter = trainableAfter,
        frozenHashBefore = frozenBefore,
        frozenHashAfter = frozenAfter
    },
    answerReadout = new
    {
        beforePredictedTokenId = before.PredictedTokenId,
        beforeExact = before.Exact,
        beforeGoldRank = before.GoldRank,
        beforeGoldLogit = before.GoldLogit,
        afterPredictedTokenId = after.PredictedTokenId,
        afterExact = after.Exact,
        afterGoldRank = after.GoldRank,
        afterGoldLogit = after.GoldLogit,
        learnedSignPreserved = before.Exact && after.Exact
    },
    utc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
};
// Файл пишется с переводом строки в конце и без метки порядка байтов.
File.WriteAllText(
    outputPath,
    JsonSerializer.Serialize(observed, new JsonSerializerOptions { WriteIndented = true }) + "\n",
    new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine("OBSERVED_WRITTEN " + outputPath);
// Нулевой код выхода означает только одно: прогон дошёл до конца без исключения.
return 0;

// Производная плавного вентиля на заданном входе. Когда mutate включён, входные
// числа меняются между прямым и обратным проходом — именно это различает путь с
// сохранённым знаменателем и путь, который пересчитывает экспоненту.
static float[] SiluGradient(float[] values, float[] upstream, bool useCache, bool mutate)
{
    // Каждый случай получает собственный вход и собственный граф, чтобы
    // накопленный градиент не смешивался между случаями.
    var input = new Tensor(values.ToArray(), new TensorShape(values.Length)) { RequiresGrad = true };
    // Прямой проход: либо учебный вызов с сохранением знаменателя, либо прежний.
    var result = useCache
        ? TensorOps.Silu(input, cacheBackwardDenominator: true, CancellationToken.None)
        : TensorOps.Silu(input, CancellationToken.None);
    if (mutate)
    {
        // Изменение входа после прямого прохода: сохранённый знаменатель остаётся
        // от старых чисел, а пересчёт экспоненты пошёл бы уже от новых.
        for (var i = 0; i < input.Data.Length; i++)
            input.Data[i] = input.Data[i] + 1.5f;
    }

    // Обратный проход запускается заданным градиентом сверху.
    result.Backward(new Tensor(upstream.ToArray(), new TensorShape(upstream.Length)));
    // Градиент входа — наблюдаемый итог этого случая.
    return input.Grad!.Data.ToArray();
}

// Значения и градиенты сети прямого распространения для одного режима признака.
static FeedForwardReadout FeedForwardGradients(
    SwiGLUFeedForward layer,
    float[] values,
    float[] upstream,
    bool training)
{
    // Градиенты параметров обнуляются, чтобы два режима не складывались вместе.
    layer.ZeroGrad();
    // Отдельный вход на каждый режим: накопленный градиент входа не переносится.
    var input = new Tensor(values.ToArray(), new TensorShape(1, 2, 4)) { RequiresGrad = true };
    // Учебная перегрузка против прежней: сравниваются ровно эти два пути.
    var output = training
        ? layer.Forward(input, training: true, CancellationToken.None)
        : layer.Forward(input, CancellationToken.None);
    // Один и тот же градиент сверху в обоих режимах.
    output.Backward(new Tensor(upstream.ToArray(), new TensorShape(1, 2, 4)));
    // Градиенты параметров собираются в одном устойчивом порядке.
    var parameterGradient = layer.Parameters()
        .SelectMany(parameter => parameter.Grad is null
            ? Array.Empty<float>()
            : parameter.Grad.Data.ToArray())
        .ToArray();
    return new FeedForwardReadout(output.Data.ToArray(), input.Grad!.Data.ToArray(), parameterGradient);
}

// Выделенные байты одного прохода блока трансформера вместе с обратным проходом.
// Датчик намеренно простой: он отвечает на вопрос «создался ли массив
// знаменателей», а не измеряет скорость.
static long MeasureBlockAllocations(
    TransformerBlock block,
    ModelConfig config,
    int positions,
    bool training)
{
    // Градиенты обнуляются до измерения, чтобы их выделение не попало в датчик.
    block.ZeroGrad();
    // Вход одинаковой формы и с одинаковыми числами в обоих режимах.
    var values = new float[positions * config.HiddenDim];
    for (var i = 0; i < values.Length; i++)
        values[i] = (float)Math.Sin(i * 0.01);
    var input = new Tensor(values, new TensorShape(1, positions, config.HiddenDim)) { RequiresGrad = true };
    var upstream = new Tensor(
        Enumerable.Repeat(1f, values.Length).ToArray(),
        new TensorShape(1, positions, config.HiddenDim));
    // Отсчёт берётся по текущему потоку: посторонние потоки в него не попадают.
    var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    var output = block.Forward(input, training, CancellationToken.None);
    output.Backward(upstream);
    var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
    return allocatedAfter - allocatedBefore;
}

// Чтение ответа: один прямой проход без обучения, затем самый вероятный токен
// последней позиции подсказки сравнивается с ожидаемым ответом.
static AnswerReadout ReadAnswer(LlmModel model, string prompt, int goldTokenId)
{
    // Вход — байты подсказки; ответ модель должна предсказать сама.
    var ids = Utf8Bytes(prompt);
    var tensor = new Tensor(
        ids.Select(id => (float)id).ToArray(),
        new TensorShape(1, ids.Length));
    // Обучение здесь выключено: это только чтение показаний.
    var logits = model.Forward(tensor, training: false).Data;
    // Начало строки выходов последней позиции подсказки.
    var offset = (ids.Length - 1) * model.Config.VocabSize;
    // Самый вероятный токен этой позиции и есть наблюдаемый ответ.
    var best = 0;
    for (var candidate = 1; candidate < model.Config.VocabSize; candidate++)
        if (logits[offset + candidate] > logits[offset + best])
            best = candidate;
    // Место ожидаемого ответа в общем порядке: 1 означает первое место.
    var goldLogit = logits[offset + goldTokenId];
    var goldRank = 1;
    for (var candidate = 0; candidate < model.Config.VocabSize; candidate++)
        if (logits[offset + candidate] > goldLogit)
            goldRank++;
    return new AnswerReadout(best, best == goldTokenId, goldRank, goldLogit);
}

// Побитовое сравнение двух наборов чисел: равная длина и равные биты каждого.
static bool BitsEqual(IReadOnlyList<float> left, IReadOnlyList<float> right)
{
    if (left.Count != right.Count)
        return false;
    for (var i = 0; i < left.Count; i++)
        if (BitConverter.SingleToInt32Bits(left[i]) != BitConverter.SingleToInt32Bits(right[i]))
            return false;
    return true;
}

// Отпечаток значений набора параметров: по нему видно, изменились они или нет.
static string ValuesHash(IEnumerable<Parameter> parameters)
{
    // Числа укладываются в один буфер в том же порядке, в каком идут параметры.
    var buffer = new List<byte>();
    foreach (var parameter in parameters)
        foreach (var value in parameter.Value.Data)
            buffer.AddRange(BitConverter.GetBytes(value));
    return Convert.ToHexString(SHA256.HashData(buffer.ToArray())).ToLowerInvariant();
}

// Байты строки в кодировке UTF-8 как номера токенов: словарь здесь байтовый.
static int[] Utf8Bytes(string text) =>
    System.Text.Encoding.UTF8.GetBytes(text).Select(value => (int)value).ToArray();

// Удостоверение общего обученного старта: отпечатки топологии, адаптеров и имя
// потока случайных чисел.
internal sealed record StartAuthority(
    string TopologySha256,
    string LoraConfigurationSha256,
    string RandomStateStreamDomain);

// Значения и градиенты одного режима сети прямого распространения.
internal sealed record FeedForwardReadout(
    float[] Forward,
    float[] InputGradient,
    float[] ParameterGradient);

// Наблюдаемый ответ на урок: предсказанный токен, точность, место и значение.
internal sealed record AnswerReadout(int PredictedTokenId, bool Exact, int GoldRank, float GoldLogit);
