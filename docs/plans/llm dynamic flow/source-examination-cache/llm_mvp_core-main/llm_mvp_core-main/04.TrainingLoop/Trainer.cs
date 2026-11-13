using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using DatasetPipeline;
using TrainingLoop.Optimizers;
using TrainingLoop.Schedulers;

namespace TrainingLoop;

// Основной цикл обучения:
//   1. Взять батч
//   2. Forward → cross-entropy loss
//   3. Backward → градиенты
//   4. Clip градиентов
//   5. Шаг оптимизатора
//   6. Обнулить градиенты
// Gradient accumulation имитирует большой батч при ограниченной памяти.
// DNA rule 8: чекпоинт сохраняется ДО увеличения счётчика шага.
public class Trainer
{
    private readonly TrainingConfig    _cfg;
    private readonly CosineWithWarmup  _scheduler;
    private int _globalStep;

    public Trainer(TrainingConfig cfg)
    {
        _cfg       = cfg;
        _scheduler = new CosineWithWarmup(cfg.WarmupSteps, cfg.TotalSteps, cfg.MaxLr, cfg.MinLr);
    }

    public async Task TrainAsync(LlmModel model, AdamW optimizer, DataLoader loader,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(_cfg.CheckpointDir);

        while (_globalStep < _cfg.TotalSteps)
        {
            ct.ThrowIfCancellationRequested();
            float totalLoss = 0f;

            // Накопление градиентов по нескольким мини-батчам
            for (int acc = 0; acc < _cfg.GradAccumSteps; acc++)
            {
                var (inputIds2d, targetIds2d) = loader.NextBatch();

                // Превращаем long[,] в Tensor [B, T]
                int[] flatInput  = Flatten2D(inputIds2d);
                int[] flatTarget = Flatten2D(targetIds2d);
                var inputTensor  = ToTensor(flatInput, _cfg.BatchSize, _cfg.SeqLen);

                var logits = model.Forward(inputTensor);  // [B, T, V]
                float loss = CrossEntropyLoss(logits, flatTarget) / _cfg.GradAccumSteps;
                totalLoss += loss;

                // Начинаем backward от скалярного loss
                BackwardLoss(logits, flatTarget, 1f / (_cfg.GradAccumSteps * _cfg.BatchSize * _cfg.SeqLen));
            }

            // Обрезаем градиенты чтобы предотвратить взрыв
            TensorOps.ClipGradNorm(model.Parameters(), _cfg.GradClipNorm);

            float lr = _scheduler.GetLr(_globalStep);
            optimizer.Step(model.Parameters(), lr);
            model.ZeroGrad();

            if (_globalStep % _cfg.LogEvery == 0)
                Console.WriteLine($"step={_globalStep} loss={totalLoss:F4} lr={lr:E3}");

            // DNA rule 8: сохраняем ДО инкремента счётчика
            if (_globalStep > 0 && _globalStep % _cfg.CheckpointEvery == 0)
                model.SaveCheckpoint(Path.Combine(_cfg.CheckpointDir, $"ckpt_{_globalStep:D6}.bin"));

            _globalStep++;
        }

        await Task.CompletedTask;
    }

    // Вычислить cross-entropy loss без autograd (для мониторинга)
    private static float CrossEntropyLoss(Tensor logits, int[] targets)
    {
        int b = logits.Shape.Dimensions[0];
        int t = logits.Shape.Dimensions[1];
        int v = logits.Shape.Dimensions[2];

        // Flatten [B,T,V] → [B*T, V]
        var flatLogits = TensorOps.Reshape(logits, b * t, v);
        return TensorOps.CrossEntropy(flatLogits, targets);
    }

    // Ручное backward через cross-entropy gradient (dL/dlogits = softmax(logits) - one_hot)
    private static void BackwardLoss(Tensor logits, int[] targets, float scale)
    {
        int b = logits.Shape.Dimensions[0];
        int t = logits.Shape.Dimensions[1];
        int v = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        // Для каждой позиции: grad = (softmax - one_hot) * scale
        for (int pos = 0; pos < b * t; pos++)
        {
            // Вычислить softmax строки
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
            {
                float val = logits.Data[pos * v + j];
                if (val > maxVal) maxVal = val;
            }

            float expSum = 0f;
            for (int j = 0; j < v; j++)
                expSum += MathF.Exp(logits.Data[pos * v + j] - maxVal);

            for (int j = 0; j < v; j++)
            {
                float sm = MathF.Exp(logits.Data[pos * v + j] - maxVal) / expSum;
                float oneHot = j == targets[pos] ? 1f : 0f;
                logits.Grad.Data[pos * v + j] += (sm - oneHot) * scale;
            }
        }
    }

    // Преобразовать плоский int[] в Tensor [B, T]
    private static Tensor ToTensor(int[] ids, int batch, int seq)
    {
        var data = ids.Select(i => (float)i).ToArray();
        return new Tensor(data, new TensorShape(batch, seq));
    }

    // Сплющить long[B,T] → int[]
    private static int[] Flatten2D(long[,] arr)
    {
        int b = arr.GetLength(0);
        int t = arr.GetLength(1);
        var result = new int[b * t];
        for (int i = 0; i < b; i++)
        for (int j = 0; j < t; j++)
            result[i * t + j] = (int)arr[i, j];
        return result;
    }
}
