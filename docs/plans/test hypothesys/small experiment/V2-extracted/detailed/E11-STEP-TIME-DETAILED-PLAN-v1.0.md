# E11 — Измерение времени шага обучения
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`  
**Датасет:** существующий `llm_mvp_core`, Tier 2 (47 файлов, ~17K токенов)  
**Внешние зависимости:** нет (градиенты считаем вручную, без TorchSharp)  
**Этот документ не касается XIIGen.**

---

## Что делаем

Запускаем одинаковый код обучения с тремя представлениями:
- BPE токены (vocab 512, ~218 токенов на KB кода)
- AST токены (vocab 145, ~34 токена на KB кода)
- Char-level токены (vocab 128, аналог E03)

Измеряем: время одного шага, количество шагов до сходимости,
итоговое время обучения.

**Нейросеть:** 2 линейных слоя, размерность 64. Настолько маленькая
что все градиенты считаем вручную в C# — без внешних библиотек.

---

## Шаг 1 — Создать ProbeNeuralNetwork.cs

**Путь:** `35.ExperimentsV2/Core/ProbeNeuralNetwork.cs`

```csharp
namespace ExperimentsV2;

// ProbeNeuralNetwork — минимальная нейросеть для измерения скорости обучения.
// Архитектура: Embedding(vocabSize, 64) → Linear(64, 64) → ReLU → Linear(64, vocabSize)
// Loss: CrossEntropy на следующий токен.
// Все операции реализованы вручную (без внешних библиотек).
public sealed class ProbeNeuralNetwork
{
    private readonly int _vocabSize;
    private readonly int _hiddenSize;
    private readonly float _lr;
    private readonly Random _rng;

    // Embedding матрица: vocabSize × hiddenSize
    private float[,] _embed;
    // Слой 1: hiddenSize × hiddenSize + bias hiddenSize
    private float[,] _w1;
    private float[] _b1;
    // Слой 2: vocabSize × hiddenSize + bias vocabSize
    private float[,] _w2;
    private float[] _b2;

    public ProbeNeuralNetwork(int vocabSize, int hiddenSize = 64, float lr = 0.01f, int seed = 42)
    {
        _vocabSize  = vocabSize;
        _hiddenSize = hiddenSize;
        _lr         = lr;
        _rng        = new Random(seed);

        // Xavier инициализация: scale = sqrt(2 / (fan_in + fan_out))
        _embed = InitMatrix(vocabSize, hiddenSize, scale: MathF.Sqrt(2f / (vocabSize + hiddenSize)));
        _w1    = InitMatrix(hiddenSize, hiddenSize, scale: MathF.Sqrt(2f / (hiddenSize + hiddenSize)));
        _b1    = new float[hiddenSize];
        _w2    = InitMatrix(vocabSize, hiddenSize, scale: MathF.Sqrt(2f / (hiddenSize + vocabSize)));
        _b2    = new float[vocabSize];
    }

    // Один шаг обучения на паре (currentToken → nextToken).
    // Возвращает loss для этого примера.
    public float TrainStep(int currentToken, int nextToken)
    {
        // ── Forward ──────────────────────────────────────────────────────────
        // 1. Embedding lookup: просто взять строку из _embed
        var emb = new float[_hiddenSize];
        for (int j = 0; j < _hiddenSize; j++)
            emb[j] = _embed[currentToken, j];

        // 2. Слой 1: h_pre = W1 @ emb + b1
        var hPre = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            float sum = _b1[i];
            for (int j = 0; j < _hiddenSize; j++)
                sum += _w1[i, j] * emb[j];
            hPre[i] = sum;
        }

        // 3. ReLU: h = max(0, h_pre)
        var h = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
            h[i] = hPre[i] > 0 ? hPre[i] : 0f;

        // 4. Слой 2: logits = W2 @ h + b2
        var logits = new float[_vocabSize];
        for (int i = 0; i < _vocabSize; i++)
        {
            float sum = _b2[i];
            for (int j = 0; j < _hiddenSize; j++)
                sum += _w2[i, j] * h[j];
            logits[i] = sum;
        }

        // 5. Softmax + CrossEntropy loss
        float maxLogit = logits.Max();
        var expLogits = logits.Select(x => MathF.Exp(x - maxLogit)).ToArray();
        float sumExp = expLogits.Sum();
        var probs = expLogits.Select(x => x / sumExp).ToArray();
        float loss = -MathF.Log(MathF.Max(probs[nextToken], 1e-7f));

        // ── Backward ─────────────────────────────────────────────────────────
        // dL/dLogits: softmax gradient = probs - one_hot(nextToken)
        var dLogits = probs.ToArray();
        dLogits[nextToken] -= 1f;

        // dL/dW2 += dLogits ⊗ h (outer product), dL/db2 += dLogits
        // dL/dh = W2^T @ dLogits
        var dH = new float[_hiddenSize];
        for (int i = 0; i < _vocabSize; i++)
        {
            _b2[i] -= _lr * dLogits[i];
            for (int j = 0; j < _hiddenSize; j++)
            {
                _w2[i, j] -= _lr * dLogits[i] * h[j];
                dH[j] += _w2[i, j] * dLogits[i];
            }
        }

        // dL/dHPre = dH * ReLU'(hPre)  (ReLU' = 1 если x>0, иначе 0)
        var dHPre = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
            dHPre[i] = hPre[i] > 0 ? dH[i] : 0f;

        // dL/dW1 += dHPre ⊗ emb, dL/db1 += dHPre
        // dL/dEmb = W1^T @ dHPre
        var dEmb = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            _b1[i] -= _lr * dHPre[i];
            for (int j = 0; j < _hiddenSize; j++)
            {
                _w1[i, j] -= _lr * dHPre[i] * emb[j];
                dEmb[j] += _w1[i, j] * dHPre[i];
            }
        }

        // dL/dEmbed[currentToken] += dEmb
        for (int j = 0; j < _hiddenSize; j++)
            _embed[currentToken, j] -= _lr * dEmb[j];

        return loss;
    }

    // Оценить точность на валидационном наборе (без обновления весов).
    public double Evaluate(IReadOnlyList<int> tokens)
    {
        int correct = 0;
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            int predicted = Predict(tokens[i]);
            if (predicted == tokens[i + 1]) correct++;
        }
        return tokens.Count > 1 ? correct / (double)(tokens.Count - 1) : 0;
    }

    private int Predict(int token)
    {
        // Forward без backward — только argmax
        var emb = new float[_hiddenSize];
        for (int j = 0; j < _hiddenSize; j++) emb[j] = _embed[token, j];

        var h = new float[_hiddenSize];
        for (int i = 0; i < _hiddenSize; i++)
        {
            float sum = _b1[i];
            for (int j = 0; j < _hiddenSize; j++) sum += _w1[i, j] * emb[j];
            h[i] = sum > 0 ? sum : 0f;
        }

        int best = 0; float bestVal = float.MinValue;
        for (int i = 0; i < _vocabSize; i++)
        {
            float sum = _b2[i];
            for (int j = 0; j < _hiddenSize; j++) sum += _w2[i, j] * h[j];
            if (sum > bestVal) { bestVal = sum; best = i; }
        }
        return best;
    }

    private float[,] InitMatrix(int rows, int cols, float scale)
    {
        var m = new float[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                m[i, j] = (float)(_rng.NextDouble() * 2 - 1) * scale;
        return m;
    }
}
```

**Проверить сборку:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 2 — Создать E11StepTimer.cs

**Путь:** `35.ExperimentsV2/Experiments/E11StepTimer.cs`

```csharp
using Tokenizer;

namespace ExperimentsV2;

// E11: измеряет время шага обучения зонд-нейросети для трёх представлений.
// Одни и те же файлы токенизируются тремя способами.
// Для каждого: замер времени на шаг + шагов до сходимости.
public sealed class E11StepTimer
{
    private const int WarmupSteps   = 20;   // не замеряем (JIT прогрев)
    private const int MeasureSteps  = 200;  // замеряем среднее время шага
    private const int MaxTrainSteps = 1000; // максимум для поиска сходимости
    private const float ConvergenceThreshold = 0.005f; // delta loss < 0.5% за 50 шагов

    public OperationResult<ExperimentV2Result> Run(
        string combinedSource,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(combinedSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет текста для E11");

        // Токенизация тремя способами
        var bpeTokens    = TokenizeBpe(combinedSource, vocabSize: 512);
        var astTokens    = TokenizeAst(combinedSource);
        var charTokens   = combinedSource.Select(c => c < 128 ? (int)c : 127).ToList();

        if (bpeTokens.Count < 20 || astTokens.Count < 20 || charTokens.Count < 20)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало токенов");

        // Замер для каждого представления
        var bpeMetrics  = MeasureTraining(bpeTokens,  vocabSize: 512, label: "BPE");
        var astMetrics  = MeasureTraining(astTokens,  vocabSize: 200, label: "AST");
        var charMetrics = MeasureTraining(charTokens, vocabSize: 128, label: "Char");

        // Baseline = BPE. Enhanced = AST (самое интересное сравнение).
        double speedupAst  = bpeMetrics.MsPerStep / astMetrics.MsPerStep;
        double speedupChar = bpeMetrics.MsPerStep / charMetrics.MsPerStep;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E11-STEP-TIMER",
            Tier              = tier,
            BaselinePrimary   = bpeMetrics.MsPerStep,
            EnhancedPrimary   = astMetrics.MsPerStep,
            PrimaryMetricName = "ms_per_step",
            LowerIsBetter     = true,
            ScalingSignal     = speedupAst,
            ScalingSignalNote = "speedup_ast_vs_bpe (во сколько раз AST быстрее BPE на шаг)",
            BaselineTimeMs    = bpeMetrics.TotalTrainMs,
            EnhancedTimeMs    = astMetrics.TotalTrainMs,
            Notes             = $"BPE: {bpeMetrics.MsPerStep:F2}ms/step, " +
                                $"stepsToConv={bpeMetrics.StepsToConverge}, " +
                                $"tokens={bpeTokens.Count} | " +
                                $"AST: {astMetrics.MsPerStep:F2}ms/step, " +
                                $"stepsToConv={astMetrics.StepsToConverge}, " +
                                $"tokens={astTokens.Count} | " +
                                $"Char: {charMetrics.MsPerStep:F2}ms/step, " +
                                $"tokens={charTokens.Count} | " +
                                $"speedupAST={speedupAst:F1}x, " +
                                $"speedupChar={speedupChar:F1}x"
        });
    }

    private TrainingMetrics MeasureTraining(
        List<int> tokens, int vocabSize, string label)
    {
        int splitAt = (int)(tokens.Count * 0.8);
        var train   = tokens.Take(splitAt).ToList();
        var val     = tokens.Skip(splitAt).ToList();

        var net = new ProbeNeuralNetwork(vocabSize, hiddenSize: 64, lr: 0.01f);

        // Прогрев JIT
        for (int i = 0; i < WarmupSteps && i < train.Count - 1; i++)
            net.TrainStep(train[i], train[i + 1]);

        // Замер времени шага
        long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int step = 0; step < MeasureSteps; step++)
        {
            int idx = step % (train.Count - 1);
            net.TrainStep(train[idx], train[idx + 1]);
        }
        double measureMs = (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
        double msPerStep = measureMs / MeasureSteps;

        // Поиск сходимости: тренируем до MaxTrainSteps, проверяем loss каждые 50 шагов
        net = new ProbeNeuralNetwork(vocabSize, hiddenSize: 64, lr: 0.01f);
        float prevLoss = float.MaxValue;
        int stepsToConverge = MaxTrainSteps;
        double totalTrainMs = 0;

        long tTrain = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int step = 0; step < MaxTrainSteps; step++)
        {
            int idx = step % (train.Count - 1);
            net.TrainStep(train[idx], train[idx + 1]);

            if (step > 0 && step % 50 == 0)
            {
                // Оценить средний loss на небольшой выборке
                float loss = 0;
                int evalN = Math.Min(100, train.Count - 1);
                for (int i = 0; i < evalN; i++)
                {
                    var netTmp = new ProbeNeuralNetwork(vocabSize, 64, 0.01f);
                    // Просто замеряем accuracy как прокси для loss
                }
                // Используем accuracy как прокси
                double acc = net.Evaluate(val.Take(200).ToList());
                float currentLoss = (float)(1.0 - acc);

                if (MathF.Abs(prevLoss - currentLoss) < ConvergenceThreshold)
                {
                    stepsToConverge = step;
                    break;
                }
                prevLoss = currentLoss;
            }
        }
        totalTrainMs = (System.Diagnostics.Stopwatch.GetTimestamp() - tTrain)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;

        return new TrainingMetrics(msPerStep, stepsToConverge, totalTrainMs);
    }

    private static List<int> TokenizeBpe(string source, int vocabSize)
    {
        var tok = new BpeTokenizer();
        tok.Train(source, vocabSize);
        return tok.Encode(source);
    }

    private static List<int> TokenizeAst(string source)
    {
        var result = E01AstTokenizer.ParseAstTokens(source);
        return result.IsSuccess ? result.Value!.tokens : new List<int>();
    }

    private readonly record struct TrainingMetrics(
        double MsPerStep, int StepsToConverge, double TotalTrainMs);
}
```

**Проверить сборку:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 3 — Добавить в Dispatcher и Runner

### В `ExperimentsV2Dispatcher.cs`:
```csharp
public OperationResult<ExperimentV2Result> RunE11(string source, int tier = 1)
    => new E11StepTimer().Run(source, tier);
```

### В `ExperimentRunner.cs`:

**В массив `ids`:**
```csharp
string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07", "E10", "E11"];
```

**В switch:**
```csharp
"E11" => _d.RunE11(ds.CombinedText, tier),
```

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 4 — Запустить на Tier 2

**Tier 1 слишком мал** — нейросеть не успеет показать разницу в сходимости.
Tier 2 (47 файлов) даёт достаточно данных.

```csharp
runner.Run("E11", tier: 2);
```

**Ожидаемый вывод:**
```
[E11] Tier 2 загрузка... 47 файлов, 38 412 симв.
[E11] Запуск...
OK ms_per_step (down) baseline=12.40 enhanced=1.80 delta=-85.5% | PASSED | PROMOTE_TO_NEXT_TIER
  -> .../E11-STEP-TIMER/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes:**
```
BPE: 12.40ms/step, stepsToConv=650, tokens=17718 |
AST: 1.80ms/step, stepsToConv=800, tokens=2750 |
Char: 0.95ms/step, tokens=38412 |
speedupAST=6.9x, speedupChar=13.1x
```

**Что означают числа:**
- `speedupAST` > 1.0 → AST быстрее на шаг (ожидаем 5-15×)
- `stepsToConv` больше у AST → нужно больше шагов (из-за меньше токенов)
- Итоговое время = `msPerStep × stepsToConverge` → должно быть меньше у AST

**Проверить итоговое время вручную из notes:**
```python
# BPE:  12.40ms × 650 steps = 8 060ms
# AST:   1.80ms × 800 steps = 1 440ms  ← в 5.6 раза быстрее итого
```

---

## Шаг 5 — Добавить в ResultWriter

В `FolderName()`:
```csharp
"E11-STEP-TIMER" => "E11-STEP-TIMER",
```

---

## Что делать если числа неожиданные

**Если `speedupAST` < 2:** AST не даёт ускорения. Проверить количество токенов —
если `tokens_ast` близко к `tokens_bpe`, значит AST-токенизация не работает
(смотреть что возвращает `E01AstTokenizer.ParseAstTokens`).

**Если `stepsToConverge` одинаковый:** Нейросеть не сошлась за MaxTrainSteps.
Увеличить `MaxTrainSteps` до 2000 или снизить `lr` до 0.001.

**Если `MsPerStep` одинаковый у всех:** JIT ещё не разогрет.
Увеличить `WarmupSteps` до 100.

---

*E11 план v1.0 · не касается XIIGen*
