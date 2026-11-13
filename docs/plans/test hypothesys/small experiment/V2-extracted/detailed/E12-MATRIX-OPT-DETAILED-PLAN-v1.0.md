# E12 — Оптимизация умножения матриц
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`  
**Датасет:** существующий `llm_mvp_core`, Tier 2 для E12a, синтетические данные для E12b/c  
**Этот документ не касается XIIGen.**

---

## Три под-эксперимента

```
E12a: разреженная N-граммная таблица vs плотный массив
E12b: SIMD матрично-векторное умножение 64×64 vs обычное
E12c: SIMD матрично-векторное умножение 512×64 vs обычное
```

Все три — только скорость, точность результата не меняется.

---

## Шаг 1 — Создать SparseNgramTable.cs

**Путь:** `35.ExperimentsV2/Core/SparseNgramTable.cs`

```csharp
namespace ExperimentsV2;

// SparseNgramTable хранит N-граммную таблицу как словарь словарей
// вместо плотного двумерного массива.
// При заполненности 2-5% (типичной для BPE кода) экономит 20-50× памяти
// и ускоряет поиск ArgmaxRow в 10-50×.
public sealed class SparseNgramTable
{
    // Для каждого предыдущего токена: словарь {следующий токен → счётчик}
    private readonly Dictionary<int, Dictionary<int, int>> _counts = new();

    // Добавить наблюдение: после токена prev пришёл токен next
    public void Add(int prev, int next)
    {
        if (!_counts.TryGetValue(prev, out var row))
            _counts[prev] = row = new Dictionary<int, int>();
        row[next] = row.GetValueOrDefault(next, 0) + 1;
    }

    // Найти самый частый следующий токен после prev.
    // Возвращает 0 если prev ни разу не встречался.
    public int ArgmaxRow(int prev)
    {
        if (!_counts.TryGetValue(prev, out var row) || row.Count == 0)
            return 0;
        int best = 0; int bestCount = -1;
        foreach (var (token, count) in row)
            if (count > bestCount) { bestCount = count; best = token; }
        return best;
    }

    // Количество уникальных (prev, next) пар — для измерения заполненности
    public int UniquePairs => _counts.Values.Sum(r => r.Count);

    // Количество уникальных предыдущих токенов
    public int UniqueContexts => _counts.Count;
}
```

---

## Шаг 2 — Создать SimdMatMul.cs

**Путь:** `35.ExperimentsV2/Core/SimdMatMul.cs`

```csharp
using System.Numerics;

namespace ExperimentsV2;

// SimdMatMul реализует матрично-векторное умножение с использованием SIMD.
// Обрабатывает Vector<float>.Count (8 на AVX, 16 на AVX-512) элементов за раз.
// Для матриц размером >= 64×64 даёт 4-8× ускорение на современных процессорах.
public static class SimdMatMul
{
    // Умножить матрицу (rows × cols) на вектор (cols) → результат (rows).
    // matrix хранится row-major: matrix[row * cols + col].
    public static float[] MatVec(float[] matrix, float[] vector, int rows, int cols)
    {
        var result   = new float[rows];
        int vSize    = Vector<float>.Count;

        for (int row = 0; row < rows; row++)
        {
            int    rowOffset = row * cols;
            float  sum       = 0f;
            int    col       = 0;

            // Обрабатываем vSize элементов за раз (8 на AVX2)
            for (; col <= cols - vSize; col += vSize)
            {
                var mVec = new Vector<float>(matrix, rowOffset + col);
                var vVec = new Vector<float>(vector, col);
                sum += Vector.Dot(mVec, vVec);
            }

            // Остаток (когда cols не кратен vSize)
            for (; col < cols; col++)
                sum += matrix[rowOffset + col] * vector[col];

            result[row] = sum;
        }
        return result;
    }

    // Обычное умножение без SIMD — для сравнения
    public static float[] MatVecPlain(float[] matrix, float[] vector, int rows, int cols)
    {
        var result = new float[rows];
        for (int row = 0; row < rows; row++)
        {
            float sum = 0f;
            for (int col = 0; col < cols; col++)
                sum += matrix[row * cols + col] * vector[col];
            result[row] = sum;
        }
        return result;
    }
}
```

---

## Шаг 3 — Создать E12MatrixOpt.cs

**Путь:** `35.ExperimentsV2/Experiments/E12MatrixOpt.cs`

```csharp
namespace ExperimentsV2;

// E12: три под-эксперимента по оптимизации матричных операций.
// E12a: SparseNgramTable vs PlainArray для N-граммного предсказания
// E12b: SIMD vs Plain для матрицы 64×64 (hidden layer нейросети)
// E12c: SIMD vs Plain для матрицы 512×64 (embedding lookup)
public sealed class E12MatrixOpt
{
    // Количество операций для замера — достаточно для стабильного среднего
    private const int NgramOps  = 50_000;
    private const int MatmulOps = 10_000;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<int> tokens,  // для E12a нужны реальные токены
        int tier = 1)
    {
        var rng = new Random(42);

        // ── E12a: Sparse vs Dense N-грамм ────────────────────────────────────
        var (sparseMs, denseMs, sparsity) = BenchmarkNgram(tokens);

        // ── E12b: SIMD 64×64 ─────────────────────────────────────────────────
        var (simd64Ms, plain64Ms) = BenchmarkMatMul(rows: 64, cols: 64, rng);

        // ── E12c: SIMD 512×64 ────────────────────────────────────────────────
        var (simd512Ms, plain512Ms) = BenchmarkMatMul(rows: 512, cols: 64, rng);

        double speedupNgram   = denseMs   / sparseMs;
        double speedup64      = plain64Ms  / simd64Ms;
        double speedup512     = plain512Ms / simd512Ms;
        double bestSpeedup    = Math.Max(Math.Max(speedupNgram, speedup64), speedup512);

        bool passed = speedupNgram >= 2.0 || speedup64 >= 2.0 || speedup512 >= 2.0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E12-MATRIX-OPT",
            Tier              = tier,
            // Baseline = суммарное время всех plain операций
            // Enhanced = суммарное время всех оптимизированных операций
            BaselinePrimary   = denseMs + plain64Ms + plain512Ms,
            EnhancedPrimary   = sparseMs + simd64Ms + simd512Ms,
            PrimaryMetricName = "total_benchmark_ms",
            LowerIsBetter     = true,
            ScalingSignal     = bestSpeedup,
            ScalingSignalNote = "best_speedup_factor (во сколько раз быстрее лучший метод)",
            BaselineTimeMs    = denseMs + plain64Ms + plain512Ms,
            EnhancedTimeMs    = sparseMs + simd64Ms + simd512Ms,
            Notes             = $"E12a ngram: dense={denseMs:F1}ms sparse={sparseMs:F1}ms " +
                                $"speedup={speedupNgram:F1}x sparsity={sparsity:P1} | " +
                                $"E12b 64x64: plain={plain64Ms:F1}ms simd={simd64Ms:F1}ms " +
                                $"speedup={speedup64:F1}x | " +
                                $"E12c 512x64: plain={plain512Ms:F1}ms simd={simd512Ms:F1}ms " +
                                $"speedup={speedup512:F1}x | " +
                                $"simd_width={System.Numerics.Vector<float>.Count}"
        });
    }

    private static (double sparseMs, double denseMs, double sparsity)
        BenchmarkNgram(IReadOnlyList<int> tokens)
    {
        // Построить обе таблицы из одних данных
        int vocab = tokens.Max() + 1;

        // Dense
        var dense = new int[vocab][];
        for (int i = 0; i < vocab; i++) dense[i] = new int[vocab];
        for (int i = 0; i < tokens.Count - 1; i++)
            dense[tokens[i]][tokens[i + 1]]++;

        // Sparse
        var sparse = new SparseNgramTable();
        for (int i = 0; i < tokens.Count - 1; i++)
            sparse.Add(tokens[i], tokens[i + 1]);

        double sparsity = 1.0 - (double)sparse.UniquePairs / ((double)vocab * vocab);

        // Замер Dense ArgmaxRow
        var t0 = Stopwatch();
        for (int op = 0; op < NgramOps; op++)
        {
            int prev = tokens[op % tokens.Count];
            if (prev >= vocab) continue;
            int best = 0; int bestC = -1;
            for (int j = 0; j < vocab; j++)
                if (dense[prev][j] > bestC) { bestC = dense[prev][j]; best = j; }
        }
        double denseMs = ElapsedMs(t0);

        // Замер Sparse ArgmaxRow
        var t1 = Stopwatch();
        for (int op = 0; op < NgramOps; op++)
            sparse.ArgmaxRow(tokens[op % tokens.Count]);
        double sparseMs = ElapsedMs(t1);

        return (sparseMs, denseMs, sparsity);
    }

    private static (double simdMs, double plainMs)
        BenchmarkMatMul(int rows, int cols, Random rng)
    {
        // Создать случайную матрицу и вектор
        var matrix = new float[rows * cols];
        var vector = new float[cols];
        for (int i = 0; i < matrix.Length; i++) matrix[i] = (float)rng.NextDouble();
        for (int i = 0; i < vector.Length; i++) vector[i] = (float)rng.NextDouble();

        // Прогрев
        for (int i = 0; i < 100; i++)
        {
            SimdMatMul.MatVec(matrix, vector, rows, cols);
            SimdMatMul.MatVecPlain(matrix, vector, rows, cols);
        }

        // Замер SIMD
        var t0 = Stopwatch();
        for (int op = 0; op < MatmulOps; op++)
            SimdMatMul.MatVec(matrix, vector, rows, cols);
        double simdMs = ElapsedMs(t0);

        // Замер Plain
        var t1 = Stopwatch();
        for (int op = 0; op < MatmulOps; op++)
            SimdMatMul.MatVecPlain(matrix, vector, rows, cols);
        double plainMs = ElapsedMs(t1);

        return (simdMs, plainMs);
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Шаг 4 — Добавить в Dispatcher и Runner

### В `ExperimentsV2Dispatcher.cs`:
```csharp
public OperationResult<ExperimentV2Result> RunE12(
    IReadOnlyList<int> tokens, int tier = 1)
    => new E12MatrixOpt().Run(tokens, tier);
```

### В `ExperimentRunner.cs`:

**В массив `ids`:**
```csharp
string[] ids = ["E01","E02","E03","E04","E05","E06","E07","E10","E11","E12"];
```

**В switch:**
```csharp
"E12" => DispatchE12(ds, tier),
```

**Новый приватный метод:**
```csharp
private OperationResult<ExperimentV2Result> DispatchE12(DatasetContents ds, int tier)
{
    var adapt = InputAdapters.ForE03(ds);  // используем те же char-level токены что E03
    return adapt.IsSuccess
        ? _d.RunE12(adapt.Value!, tier)
        : OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
}
```

### В `ResultWriter.FolderName()`:
```csharp
"E12-MATRIX-OPT" => "E12-MATRIX-OPT",
```

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 5 — Запустить на Tier 2

```csharp
runner.Run("E12", tier: 2);
```

**Ожидаемый вывод:**
```
[E12] Tier 2 загрузка... 47 файлов, 38 412 симв.
[E12] Запуск...
OK total_benchmark_ms (down) baseline=1240.5 enhanced=187.3 delta=-84.9% | PASSED
  -> .../E12-MATRIX-OPT/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes (примерные):**
```
E12a ngram: dense=850ms sparse=75ms speedup=11.3x sparsity=96.8% |
E12b 64x64: plain=180ms simd=48ms speedup=3.8x |
E12c 512x64: plain=210ms simd=65ms speedup=3.2x |
simd_width=8
```

**Что означает `simd_width=8`:** процессор поддерживает AVX (8 float за раз).
На AVX-512 будет 16 — ускорение вырастет до 6-8×.

---

## Что делать с результатами

**Если E12a speedup ≥ 3×:** заменить `int[][]` на `SparseNgramTable` в E03, E07, E09.
Конкретно: в `E03NgramModel.cs` найти инициализацию bigram/trigram таблиц
и заменить на `SparseNgramTable`.

**Если E12b/E12c speedup ≥ 2×:** использовать `SimdMatMul.MatVec` вместо
ручного цикла в `ProbeNeuralNetwork.cs` (E11).

**Если после замены запустить E11 снова:** время шага должно ещё уменьшиться.

---

## Тест на корректность (обязательно для E12a)

Sparse и Dense должны давать одинаковые предсказания:

```csharp
[Fact]
public void SparseNgram_SameResultsAsDense()
{
    var tokens = new[] { 1, 2, 3, 2, 1, 2, 4, 2, 1, 3 };
    int vocab = 5;

    var dense = new int[vocab][];
    for (int i = 0; i < vocab; i++) dense[i] = new int[vocab];
    var sparse = new SparseNgramTable();

    for (int i = 0; i < tokens.Length - 1; i++)
    {
        dense[tokens[i]][tokens[i + 1]]++;
        sparse.Add(tokens[i], tokens[i + 1]);
    }

    for (int prev = 0; prev < vocab; prev++)
    {
        // Dense argmax
        int denseArgmax = 0; int bestC = -1;
        for (int j = 0; j < vocab; j++)
            if (dense[prev][j] > bestC) { bestC = dense[prev][j]; denseArgmax = j; }

        // Sparse argmax
        int sparseArgmax = sparse.ArgmaxRow(prev);

        Assert.Equal(denseArgmax, sparseArgmax);
    }
}
```

```
dotnet test
```
Ожидаем: все тесты зелёные.

---

*E12 план v1.0 · не касается XIIGen*
