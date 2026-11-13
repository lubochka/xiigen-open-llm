// Оболочка минимальной проверки переноса «убрать предварительную копию входа в
// выходную память при нормализации внимания для непрерывной последней оси»
// (раздел переноса Q32, якорь q32-min, карточка
// docs/plans/replanning-possible-diffs/fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md).
//
// Она делает ровно то, что названо карточкой и нарядом: показывает, что правленая
// ветвь даёт те же числа, что независимый эталон, и что прежние ветви, обратный
// проход и обработка отмены сохранены; восстанавливает общий обученный старт
// h64-rms-row/v1 после 400 обновлений, берёт урок T1 и выполняет ОДИН обычный шаг
// обучения внимания со чтением знака до и после.
//
// Время здесь заново не измеряется и никакое ускорение не обещается: в исходном
// опыте разброс контрольного времени был больше наблюдённой разницы.
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

// Два входа задаются снаружи, чтобы в оболочке не было спрятанных путей.
var sharedRoot = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);

// Удостоверения фактически загруженных сборок: по ним видно, что прогон прошёл
// именно через свежесобранное изменённое тензорное ядро, а не через старую
// библиотеку.
var tensorAssembly = typeof(TensorOps).Assembly;
var modelAssembly = typeof(LlmModel).Assembly;
var trainerAssembly = typeof(Trainer).Assembly;
foreach (var assembly in new[] { tensorAssembly, modelAssembly, trainerAssembly })
{
    Console.WriteLine("ASSEMBLY " + assembly.GetName().Name
        + " " + assembly.Location
        + " MVID " + assembly.ManifestModule.ModuleVersionId);
}

// ── Контроль 1: правленая ветвь на форме, которой пользуется внимание ──────────
//
// Внимание вызывает нормализацию по последней оси на форме
// [партия, головы, позиции, позиции] и перед этим ставит минус бесконечность в
// невидимые причинной маской места. Поэтому проверочная форма и расстановка
// минус бесконечности здесь такие же: именно этот вход проходит через правленую
// ветвь на каждом прямом проходе внимания.
const int batch = 1;
const int heads = 4;
const int positions = 21;
var attentionScores = new float[batch * heads * positions * positions];
for (var index = 0; index < attentionScores.Length; index++)
{
    // Номер столбца и номер строки нужны, чтобы повторить причинную маску.
    var column = index % positions;
    var row = (index / positions) % positions;
    // Видимые места получают обычные числа, невидимые — минус бесконечность,
    // ровно как после причинной маски продукта.
    attentionScores[index] = column <= row ? -0.2f + 0.007f * index : float.NegativeInfinity;
}

var attentionShape = new TensorShape(batch, heads, positions, positions);
var attentionInput = new Tensor(attentionScores.ToArray(), attentionShape);
// Копия входных чисел нужна, чтобы потом убедиться: вход не изменился.
var attentionInputCopy = attentionInput.Data.ToArray();
// Выделенные байты одного вызова: датчик следит, что новых массивов не появилось.
var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
var attentionResult = TensorOps.Softmax(attentionInput, dim: -1, CancellationToken.None);
var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
var attentionAllocatedBytes = allocatedAfter - allocatedBefore;

// Независимый эталон тем же порядком действий: максимум строки, экспонента от
// входного числа, сумма по строке, деление. Совпадение бит в бит подтверждает,
// что порядок действий в ядре сохранён.
var attentionReferenceSameOrder = ReferenceSoftmaxLastAxis(attentionScores, positions);
var attentionSameOrderBitwiseEqual = BitsEqual(attentionResult.Data, attentionReferenceSameOrder);
// Независимый эталон в двойной точности: он не зависит от порядка округления в
// одиночной точности и проверяет саму правильность числа, а не только порядок.
var attentionReferenceDouble = ReferenceSoftmaxLastAxisDouble(attentionScores, positions);
var attentionMaxDoubleDeviation = MaxDeviation(attentionResult.Data, attentionReferenceDouble);
// Сумма каждой строки обязана равняться единице в пределах одиночной точности.
var attentionMaxRowSumDeviation = MaxRowSumDeviation(attentionResult.Data, positions);
// Вход не изменился, и массив результата — не тот же самый массив.
var attentionInputUnchanged = BitsEqual(attentionInput.Data, attentionInputCopy);
var attentionOutputArrayIsSeparate = !ReferenceEquals(attentionResult.Data, attentionInput.Data);
Console.WriteLine("ATTENTION_SHAPE " + batch + "x" + heads + "x" + positions + "x" + positions);
Console.WriteLine("ATTENTION_SAME_ORDER_BITWISE_EQUAL " + attentionSameOrderBitwiseEqual);
Console.WriteLine("ATTENTION_MAX_DOUBLE_DEVIATION "
    + attentionMaxDoubleDeviation.ToString("R", CultureInfo.InvariantCulture));
Console.WriteLine("ATTENTION_MAX_ROW_SUM_DEVIATION "
    + attentionMaxRowSumDeviation.ToString("R", CultureInfo.InvariantCulture));
Console.WriteLine("ATTENTION_INPUT_UNCHANGED " + attentionInputUnchanged);
Console.WriteLine("ATTENTION_OUTPUT_ARRAY_SEPARATE " + attentionOutputArrayIsSeparate);
Console.WriteLine("ATTENTION_ALLOCATED_BYTES "
    + attentionAllocatedBytes.ToString(CultureInfo.InvariantCulture));

// ── Контроль 2: обычная ветвь остальных осей сохранена ────────────────────────
//
// Для оси, после которой есть ещё оси длиной больше единицы, шаг между соседями
// больше единицы, строка в памяти не непрерывна, и работает прежняя ветвь с
// предварительной копией. Её числа тоже сверяются с независимым эталоном.
var middleValues = new float[2 * 3 * 4];
for (var index = 0; index < middleValues.Length; index++)
    middleValues[index] = -0.6f + 0.11f * index;
var middleInput = new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4));
var middleInputCopy = middleInput.Data.ToArray();
var middleResult = TensorOps.Softmax(middleInput, dim: 1, CancellationToken.None);
var middleReference = ReferenceSoftmaxStrided(middleValues, outerSize: 2, dimSize: 3, innerSize: 4);
var middleBitwiseEqual = BitsEqual(middleResult.Data, middleReference);
var middleInputUnchanged = BitsEqual(middleInput.Data, middleInputCopy);
Console.WriteLine("MIDDLE_AXIS_BITWISE_EQUAL " + middleBitwiseEqual);
Console.WriteLine("MIDDLE_AXIS_INPUT_UNCHANGED " + middleInputUnchanged);

// Отрицательный номер оси по-прежнему означает последнюю ось: правка условие
// нормализации номера оси не меняет.
var negativeAxisInput = new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4));
var negativeAxisResult = TensorOps.Softmax(negativeAxisInput, dim: -1, CancellationToken.None);
var explicitLastAxisInput = new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4));
var explicitLastAxisResult = TensorOps.Softmax(explicitLastAxisInput, dim: 2, CancellationToken.None);
var negativeAxisMatchesLast = BitsEqual(negativeAxisResult.Data, explicitLastAxisResult.Data);
Console.WriteLine("NEGATIVE_AXIS_MATCHES_LAST_AXIS " + negativeAxisMatchesLast);

// ── Контроль 3: обработка отмены сохранена в обеих ветвях ─────────────────────
//
// Заранее отменённый признак обязан привести к отказу до счёта, а не к расчёту.
var preCancelledLastAxis = Refuses(() =>
{
    using var source = new CancellationTokenSource();
    source.Cancel();
    TensorOps.Softmax(new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4)), dim: -1, source.Token);
});
var preCancelledMiddleAxis = Refuses(() =>
{
    using var source = new CancellationTokenSource();
    source.Cancel();
    TensorOps.Softmax(new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4)), dim: 1, source.Token);
});
Console.WriteLine("PRE_CANCELLED_LAST_AXIS_REFUSED " + preCancelledLastAxis);
Console.WriteLine("PRE_CANCELLED_MIDDLE_AXIS_REFUSED " + preCancelledMiddleAxis);

// ── Контроль 4: обратный проход работоспособен ───────────────────────────────
//
// Производная нормализации читает значения результата и градиент сверху; правка
// их источник не меняет. Сверка идёт с независимым выражением
// grad_x[i] = s[i] * (grad[i] − сумма(grad * s)) по строке.
var gradientInput = new Tensor(middleValues.ToArray(), new TensorShape(2, 3, 4)) { RequiresGrad = true };
var gradientResult = TensorOps.Softmax(gradientInput, dim: -1, CancellationToken.None);
var upstreamValues = new float[middleValues.Length];
for (var index = 0; index < upstreamValues.Length; index++)
    upstreamValues[index] = 0.013f * (index + 1) - 0.21f;
gradientResult.Backward(new Tensor(upstreamValues.ToArray(), new TensorShape(2, 3, 4)));
var gradientReference = ReferenceSoftmaxGradientLastAxis(gradientResult.Data, upstreamValues, dimSize: 4);
var gradientBitwiseEqual = BitsEqual(gradientInput.Grad!.Data, gradientReference);
var gradientAllFinite = gradientInput.Grad!.Data.All(float.IsFinite);
Console.WriteLine("BACKWARD_BITWISE_EQUAL_TO_REFERENCE " + gradientBitwiseEqual);
Console.WriteLine("BACKWARD_ALL_FINITE " + gradientAllFinite);

// ── Назначенная карточкой проверка: один учебный шаг из общего старта ────────
//
// Удостоверение старта читается до любого обучения: ни один байт не берётся на
// веру, семейство и его отпечатки названы явно. Имена полей в удостоверении
// записаны со строчной первой буквы, поэтому сравнение имён идёт без учёта
// регистра.
var authority = JsonSerializer.Deserialize<StartAuthority>(
        File.ReadAllBytes(Path.Combine(sharedRoot, "authority.json")),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidOperationException("Q32_START_AUTHORITY_UNREADABLE");
// Отсутствие любого из трёх отпечатков останавливает прогон до обучения.
if (string.IsNullOrEmpty(authority.TopologySha256)
    || string.IsNullOrEmpty(authority.LoraConfigurationSha256)
    || string.IsNullOrEmpty(authority.RandomStateStreamDomain))
{
    throw new InvalidOperationException("Q32_START_AUTHORITY_INCOMPLETE");
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
    ?? throw new InvalidOperationException("Q32_START_OPTIMIZER_UNREADABLE");
if (!optimizer.TryImportPortableState(Bindings(), portable))
    throw new InvalidOperationException("Q32_START_OPTIMIZER_IMPORT_REFUSED");
Console.WriteLine("START_OPTIMIZER_STEP " + portable.Step.ToString(CultureInfo.InvariantCulture));

// Состояние датчика случайных чисел восстанавливается с точными отпечатками
// топологии и адаптеров: чужой старт эту проверку не пройдёт.
var randomState = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
        File.ReadAllBytes(randomStatePath))
    ?? throw new InvalidOperationException("Q32_START_RANDOM_STATE_UNREADABLE");
model.RestoreRandomState(
    randomState,
    new LlmModelRandomStateIdentity(authority.TopologySha256, authority.LoraConfigurationSha256));
Console.WriteLine("START_RANDOM_STATE_RESTORED true");

// Урок T1 читается из файла старта, а не вписан в оболочку: это тот самый урок,
// на котором старт учился.
var lessonLine = File.ReadAllLines(Path.Combine(sharedRoot, "data.source.jsonl"))
    .Select(line => line.Trim())
    .First(line => line.Length > 0 && line.Contains("\"id\":\"T1\"", StringComparison.Ordinal));
var lesson = JsonSerializer.Deserialize<LessonRow>(
        lessonLine,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidOperationException("Q32_LESSON_UNREADABLE");
var lessonIds = Utf8Bytes(lesson.Prompt + lesson.Answer);
var goldTokenId = Utf8Bytes(lesson.Answer)[0];
Console.WriteLine("LESSON_ID " + lesson.Id);
Console.WriteLine("LESSON_PROMPT " + lesson.Prompt);
Console.WriteLine("LESSON_ANSWER " + lesson.Answer);
Console.WriteLine("LESSON_GOLD_TOKEN " + goldTokenId.ToString(CultureInfo.InvariantCulture));

// Чтение знака ДО обучения тем же способом, что и после.
var before = ReadAnswer(model, lesson.Prompt, goldTokenId);
var trainableBefore = ValuesHash(trainable);
var frozenBefore = ValuesHash(frozen);
Console.WriteLine("BEFORE_PREDICTED_TOKEN " + before.PredictedTokenId.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("BEFORE_EXACT " + before.Exact);

// ОДИН обычный шаг обучения внимания: прямой и обратный проход этого шага идут
// через нормализацию внимания по последней оси, то есть через правленую ветвь.
var trainer = new Trainer(new TrainingConfig
{
    BatchSize = 1,
    SeqLen = 64,
    WarmupSteps = 0,
    TotalSteps = 404,
    MaxLr = 1e-3f,
    MinLr = 1e-3f,
    GradClipNorm = 1f,
    GradAccumSteps = 1,
    WeightDecay = 0f
});
var loss = trainer.TrainSupervisedNextTokenStep(model, optimizer, lessonIds, weight: 1f);
Console.WriteLine("STEP_LOSS " + loss.ToString("R", CultureInfo.InvariantCulture));
Console.WriteLine("STEP_LOSS_FINITE " + float.IsFinite(loss));

// Показания после шага: тот же читатель, тот же урок, тот же ожидаемый знак.
var optimizerStepAfter = optimizer.ExportPortableState(Bindings()).Step;
var trainableAfter = ValuesHash(trainable);
var frozenAfter = ValuesHash(frozen);
var after = ReadAnswer(model, lesson.Prompt, goldTokenId);
Console.WriteLine("AFTER_OPTIMIZER_STEP " + optimizerStepAfter.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("AFTER_PREDICTED_TOKEN " + after.PredictedTokenId.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("AFTER_EXACT " + after.Exact);
Console.WriteLine("TRAINABLE_CHANGED " + (trainableBefore != trainableAfter));
Console.WriteLine("FROZEN_UNCHANGED " + (frozenBefore == frozenAfter));
Console.WriteLine("LEARNED_SIGN_PRESERVED " + (before.Exact && after.Exact));

// Сырые наблюдения сохраняются рядом как улика, без оценок и без вердиктов.
var observed = new
{
    card = "Q32 — нормализация внимания без предварительной копии, один учебный шаг",
    timeClaim = "никакого ускорения не заявляется и время заново не измеряется: "
        + "в исходном опыте разброс контрольного времени 2,883 мс больше наблюдённой "
        + "разницы 1,8815 мс",
    assemblies = new[] { tensorAssembly, modelAssembly, trainerAssembly }.Select(assembly => new
    {
        name = assembly.GetName().Name,
        location = assembly.Location,
        moduleVersionId = assembly.ManifestModule.ModuleVersionId.ToString()
    }).ToArray(),
    contiguousLastAxisBranch = new
    {
        shape = new[] { batch, heads, positions, positions },
        causalMaskNegativeInfinityPresent = true,
        sameOrderReferenceBitwiseEqual = attentionSameOrderBitwiseEqual,
        maxDeviationFromDoublePrecisionReference = attentionMaxDoubleDeviation,
        maxRowSumDeviationFromOne = attentionMaxRowSumDeviation,
        inputUnchanged = attentionInputUnchanged,
        outputArraySeparateFromInput = attentionOutputArrayIsSeparate,
        allocatedBytesForOneCall = attentionAllocatedBytes
    },
    preservedGenericBranch = new
    {
        shape = new[] { 2, 3, 4 },
        axis = 1,
        bitwiseEqualToReference = middleBitwiseEqual,
        inputUnchanged = middleInputUnchanged,
        negativeAxisMatchesExplicitLastAxis = negativeAxisMatchesLast
    },
    cancellation = new
    {
        preCancelledLastAxisRefused = preCancelledLastAxis,
        preCancelledMiddleAxisRefused = preCancelledMiddleAxis
    },
    backward = new
    {
        bitwiseEqualToReference = gradientBitwiseEqual,
        allFinite = gradientAllFinite
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
        id = lesson.Id,
        prompt = lesson.Prompt,
        answer = lesson.Answer,
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
// Файл пишется с переводом строки в конце, без метки порядка байтов и только с
// переводами строки вида «один байт»: отступы сериализатора на Windows иначе
// пришли бы с возвратом каретки, а улики хранятся в едином виде.
var observedJson = JsonSerializer
    .Serialize(observed, new JsonSerializerOptions { WriteIndented = true })
    .Replace("\r\n", "\n", StringComparison.Ordinal);
File.WriteAllText(
    outputPath,
    observedJson + "\n",
    new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine("OBSERVED_WRITTEN " + outputPath);
// Нулевой код выхода означает только одно: прогон дошёл до конца без исключения.
return 0;

// Независимый эталон нормализации по непрерывной последней оси тем же порядком
// действий: максимум строки, экспонента от входного числа, сумма, деление.
static float[] ReferenceSoftmaxLastAxis(float[] values, int dimSize)
{
    // Результат пишется в собственный массив, вход не меняется.
    var reference = new float[values.Length];
    for (var rowStart = 0; rowStart < values.Length; rowStart += dimSize)
    {
        // Максимум строки для устойчивости счёта.
        var maxValue = float.NegativeInfinity;
        for (var d = 0; d < dimSize; d++)
            if (values[rowStart + d] > maxValue) maxValue = values[rowStart + d];
        // Экспоненты и их сумма в том же порядке по строке.
        var sum = 0f;
        for (var d = 0; d < dimSize; d++)
        {
            reference[rowStart + d] = MathF.Exp(values[rowStart + d] - maxValue);
            sum += reference[rowStart + d];
        }

        // Нормировка делением на сумму.
        for (var d = 0; d < dimSize; d++)
            reference[rowStart + d] /= sum;
    }

    return reference;
}

// Независимый эталон в двойной точности: он проверяет само число, а не порядок
// округления в одиночной точности.
static double[] ReferenceSoftmaxLastAxisDouble(float[] values, int dimSize)
{
    var reference = new double[values.Length];
    for (var rowStart = 0; rowStart < values.Length; rowStart += dimSize)
    {
        var maxValue = double.NegativeInfinity;
        for (var d = 0; d < dimSize; d++)
            if (values[rowStart + d] > maxValue) maxValue = values[rowStart + d];
        var sum = 0.0;
        for (var d = 0; d < dimSize; d++)
        {
            reference[rowStart + d] = Math.Exp(values[rowStart + d] - maxValue);
            sum += reference[rowStart + d];
        }

        for (var d = 0; d < dimSize; d++)
            reference[rowStart + d] /= sum;
    }

    return reference;
}

// Независимый эталон для оси с шагом больше единицы: номера считаются так же,
// как в обычной ветви продукта.
static float[] ReferenceSoftmaxStrided(float[] values, int outerSize, int dimSize, int innerSize)
{
    var reference = new float[values.Length];
    for (var outer = 0; outer < outerSize; outer++)
    for (var inner = 0; inner < innerSize; inner++)
    {
        var maxValue = float.NegativeInfinity;
        for (var d = 0; d < dimSize; d++)
        {
            var index = (outer * dimSize + d) * innerSize + inner;
            if (values[index] > maxValue) maxValue = values[index];
        }

        var sum = 0f;
        for (var d = 0; d < dimSize; d++)
        {
            var index = (outer * dimSize + d) * innerSize + inner;
            reference[index] = MathF.Exp(values[index] - maxValue);
            sum += reference[index];
        }

        for (var d = 0; d < dimSize; d++)
        {
            var index = (outer * dimSize + d) * innerSize + inner;
            reference[index] /= sum;
        }
    }

    return reference;
}

// Независимое выражение производной нормализации по строке:
// grad_x[i] = s[i] * (grad[i] − сумма(grad * s)).
static float[] ReferenceSoftmaxGradientLastAxis(float[] softmaxValues, float[] upstream, int dimSize)
{
    var reference = new float[softmaxValues.Length];
    for (var rowStart = 0; rowStart < softmaxValues.Length; rowStart += dimSize)
    {
        // Скалярное произведение градиента сверху и значений нормализации.
        var dot = 0f;
        for (var d = 0; d < dimSize; d++)
            dot += upstream[rowStart + d] * softmaxValues[rowStart + d];
        for (var d = 0; d < dimSize; d++)
            reference[rowStart + d] = softmaxValues[rowStart + d] * (upstream[rowStart + d] - dot);
    }

    return reference;
}

// Наибольшее отклонение чисел одиночной точности от эталона двойной точности.
static double MaxDeviation(IReadOnlyList<float> actual, IReadOnlyList<double> expected)
{
    var worst = 0.0;
    for (var index = 0; index < actual.Count; index++)
    {
        var deviation = Math.Abs(actual[index] - expected[index]);
        if (deviation > worst) worst = deviation;
    }

    return worst;
}

// Наибольшее отклонение суммы строки от единицы: нормировка обязана её давать.
static double MaxRowSumDeviation(IReadOnlyList<float> values, int dimSize)
{
    var worst = 0.0;
    for (var rowStart = 0; rowStart < values.Count; rowStart += dimSize)
    {
        var sum = 0.0;
        for (var d = 0; d < dimSize; d++)
            sum += values[rowStart + d];
        var deviation = Math.Abs(sum - 1.0);
        if (deviation > worst) worst = deviation;
    }

    return worst;
}

// Отрицательный контроль: действие обязано отказать отменой, а не досчитать.
static bool Refuses(Action action)
{
    try
    {
        action();
        // Досчитал — значит отмена не сработала, и это отказ проверки.
        return false;
    }
    catch (OperationCanceledException)
    {
        // Ровно ожидаемый отказ по отмене.
        return true;
    }
}

// Чтение знака: один прямой проход без обучения, затем самый вероятный токен
// последней позиции подсказки сравнивается с ожидаемым ответом урока.
static AnswerReadout ReadAnswer(LlmModel model, string prompt, int goldTokenId)
{
    // Вход — байты подсказки; знак модель должна предсказать сама.
    var ids = Utf8Bytes(prompt);
    var tensor = new Tensor(
        ids.Select(id => (float)id).ToArray(),
        new TensorShape(1, ids.Length));
    // Обучение здесь выключено: это только чтение показаний.
    var logits = model.Forward(tensor, training: false).Data;
    // Начало строки выходов последней позиции подсказки.
    var offset = (ids.Length - 1) * model.Config.VocabSize;
    // Самый вероятный токен этой позиции и есть наблюдаемый знак.
    var best = 0;
    for (var candidate = 1; candidate < model.Config.VocabSize; candidate++)
        if (logits[offset + candidate] > logits[offset + best])
            best = candidate;
    // Место ожидаемого знака в общем порядке: 1 означает первое место.
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

// Строка учебного набора старта: номер урока, подсказка и ожидаемый знак.
internal sealed record LessonRow(string Id, string Prompt, string Answer);

// Наблюдаемый ответ на урок: предсказанный токен, точность, место и значение.
internal sealed record AnswerReadout(int PredictedTokenId, bool Exact, int GoldRank, float GoldLogit);
