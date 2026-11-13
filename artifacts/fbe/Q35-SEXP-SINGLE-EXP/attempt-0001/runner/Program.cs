// Оболочка минимальной проверки переноса «использовать ту же экспоненту при
// вычислении градиента» (раздел переноса «FIX-SEXP — Q35», якорь q35-min).
// Она делает ровно то, что названо карточкой: восстанавливает принятый
// родительский шаг 1000 тем же загрузчиком, что в учебном примере, берёт один
// урок и выполняет ОДИН шаг обучения с учителем, который проходит по
// изменённому обратному проходу. Ничего больше она не доказывает.
using System.Globalization;
using System.Text.Json;
using ContinuousLearning;
using TensorCore.Core;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;

// Три входа задаются снаружи, чтобы в оболочке не было спрятанных путей.
var parentRepository = Path.GetFullPath(args[0]);
var parentHandoff = Path.GetFullPath(args[1]);
var outputPath = Path.GetFullPath(args[2]);

// Удостоверение фактически загруженной сборки обучающего цикла: по нему видно,
// что шаг прошёл именно через свежесобранный изменённый обратный проход.
var trainerAssembly = typeof(Trainer).Assembly;
Console.WriteLine("TRAINER_ASSEMBLY " + trainerAssembly.Location);
Console.WriteLine("TRAINER_MODULE_VERSION " + trainerAssembly.ManifestModule.ModuleVersionId);

// Старт: принятый обученный родитель ищется и перепроверяется самим загрузчиком.
var startResult = SftAcceptedParent.PrepareStartLatest(parentRepository, parentHandoff, default);
if (!startResult.IsSuccess || startResult.Value is null)
{
    // Неудачу старта печатаем дословно и выходим ненулевым кодом, без догадок.
    Console.WriteLine("START_FAILED " + startResult.ErrorCode + " " + startResult.ErrorMessage);
    return 2;
}
var start = startResult.Value;

// Назначенные карточкой признаки старта проверяются до любого обучения.
Console.WriteLine("START_ACCEPTED_ORDINAL " + start.AcceptedOrdinal.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("START_OPTIMIZER_STEP " + start.OptimizerStep.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("START_MODEL_STATE_SHA256 " + start.ModelStateSha256);

// Единственный урок карточки: имя файла и ожидаемый ответ.
const string lessonFile = "One.cs";
const string lessonSource = "static int One()=>1;";
// Текст урока собирается ровно так же, как в учебном примере опыта.
var lessonText = "// file: " + lessonFile + "\n" + lessonSource;
// Последовательность токенов урока — вход одного шага обучения.
var lessonIds = start.Tokenizer.Encode(lessonText).ToArray();
// Длина приглашения нужна, чтобы считать верные токены только в ответе.
var promptLength = start.Tokenizer.Encode("// file: " + lessonFile + "\n").Count;
Console.WriteLine("LESSON_TOKENS " + lessonIds.Length.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("LESSON_PROMPT_TOKENS " + promptLength.ToString(CultureInfo.InvariantCulture));

// Привязки «имя параметра — тензор» нужны для чтения шага оптимизатора.
AdamWParameterBinding[] Bindings() => start.Model.GetCheckpointParameterEntries()
    .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
    .ToArray();

// Чтение показаний до обучения: один прямой проход, маска только на ответе.
var readoutBefore = ReadResponse(start.Model, lessonIds, promptLength);
// Шаг оптимизатора до обучения берётся из его же переносимого состояния.
var optimizerStepBefore = start.Optimizer.ExportPortableState(Bindings()).Step;
Console.WriteLine("BEFORE_OPTIMIZER_STEP " + optimizerStepBefore.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("BEFORE_RESPONSE_CORRECT_TOKENS "
    + readoutBefore.CorrectTokens.ToString(CultureInfo.InvariantCulture)
    + " of " + readoutBefore.TargetCount.ToString(CultureInfo.InvariantCulture));

// ОДИН шаг обучения с учителем: он и проходит по изменённому обратному проходу.
var trainer = new Trainer(start.TrainingConfig);
var loss = trainer.TrainSupervisedNextTokenStep(
    start.Model, start.Optimizer, lessonIds, weight: 1f);
Console.WriteLine("STEP_LOSS " + loss.ToString("R", CultureInfo.InvariantCulture));
Console.WriteLine("STEP_LOSS_FINITE " + float.IsFinite(loss).ToString());

// Чтение показаний после обучения тем же способом, что и до него.
var optimizerStepAfter = start.Optimizer.ExportPortableState(Bindings()).Step;
var readoutAfter = ReadResponse(start.Model, lessonIds, promptLength);
Console.WriteLine("AFTER_OPTIMIZER_STEP " + optimizerStepAfter.ToString(CultureInfo.InvariantCulture));
Console.WriteLine("AFTER_RESPONSE_CORRECT_TOKENS "
    + readoutAfter.CorrectTokens.ToString(CultureInfo.InvariantCulture)
    + " of " + readoutAfter.TargetCount.ToString(CultureInfo.InvariantCulture));

// Сырые наблюдения сохраняются рядом как улика, без оценок и без вердиктов.
var observed = new
{
    lesson = new { file = lessonFile, source = lessonSource, tokens = lessonIds.Length, promptTokens = promptLength },
    trainerAssembly = new
    {
        location = trainerAssembly.Location,
        moduleVersionId = trainerAssembly.ManifestModule.ModuleVersionId.ToString(),
        assemblyName = trainerAssembly.GetName().FullName
    },
    start = new
    {
        acceptedOrdinal = start.AcceptedOrdinal,
        optimizerStep = start.OptimizerStep,
        parentManifestPath = start.ParentManifestPath,
        parentManifestSha256 = start.ParentManifestSha256,
        weightsSha256 = start.WeightsSha256,
        tokenizerSha256 = start.TokenizerSha256,
        modelStateSha256 = start.ModelStateSha256
    },
    step = new
    {
        loss,
        lossBits = BitConverter.SingleToInt32Bits(loss),
        lossFinite = float.IsFinite(loss),
        optimizerStepBefore,
        optimizerStepAfter
    },
    responseReadout = new
    {
        beforeCorrectTokens = readoutBefore.CorrectTokens,
        beforeTargetCount = readoutBefore.TargetCount,
        beforeExactResponse = readoutBefore.ExactResponse,
        afterCorrectTokens = readoutAfter.CorrectTokens,
        afterTargetCount = readoutAfter.TargetCount,
        afterExactResponse = readoutAfter.ExactResponse
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

// Чтение верных токенов ответа: один прямой проход без обучения, затем для
// каждой позиции ответа сравнивается самый вероятный токен с ожидаемым.
static ResponseReadout ReadResponse(LlmModel model, int[] ids, int promptTokenCount)
{
    // Предсказывается следующий токен, поэтому позиций на одну меньше длины.
    var positions = ids.Length - 1;
    // Размер словаря задаёт шаг строки в плоском массиве выходов модели.
    var vocabulary = model.Config.VocabSize;
    // Вход прямого прохода — все токены, кроме последнего.
    var tensor = new Tensor(
        ids.Take(positions).Select(id => (float)id).ToArray(),
        new TensorShape(1, positions));
    // Обучение здесь выключено: это только чтение показаний.
    var logits = model.Forward(tensor, training: false).Data;
    // Счётчики относятся только к токенам ответа, приглашение не считается.
    var correct = 0;
    var targets = 0;
    for (var position = 0; position < positions; position++)
    {
        // Позиции до конца приглашения в показания ответа не входят.
        if (position + 1 < promptTokenCount)
            continue;
        // Начало строки выходов для этой позиции.
        var row = position * vocabulary;
        // Ищем самый вероятный токен этой позиции.
        var best = 0;
        for (var candidate = 1; candidate < vocabulary; candidate++)
            if (logits[row + candidate] > logits[row + best])
                best = candidate;
        // Каждая позиция ответа — одна проверяемая цель.
        targets++;
        // Совпадение с ожидаемым следующим токеном даёт один верный токен.
        if (best == ids[position + 1])
            correct++;
    }
    // Полный ответ считается точным, только когда верны все его токены.
    return new ResponseReadout(correct, targets, targets > 0 && correct == targets);
}

// Запись показаний ответа: сколько верных токенов, сколько целей, точен ли ответ.
internal sealed record ResponseReadout(int CorrectTokens, int TargetCount, bool ExactResponse);
