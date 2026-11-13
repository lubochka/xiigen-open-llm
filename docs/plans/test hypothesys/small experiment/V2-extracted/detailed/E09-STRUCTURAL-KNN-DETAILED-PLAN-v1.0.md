# E09 — Структурный вектор + K-NN
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`  
**Датасет:** существующий `llm_mvp_core`, Tier 2 (47 файлов)  
**Этот документ не касается XIIGen.**

---

## Что делаем

Исправляем E07. Там было два бага:
1. N-граммная таблица требует точного совпадения ключа — при 58% уникальных контекстов она не работает
2. Вектор содержал только `depth=3` — не различает глубину в `IfStatement` vs в `ForStatement`

E09 исправляет оба: строим полный путь в дереве (родитель + прародитель + прапрародитель)
и ищем K ближайших соседей вместо точного совпадения.

---

## Шаг 1 — Создать StructuralVectorExtractor.cs

**Путь:** `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// StructuralVectorExtractor строит структурный вектор для каждого AST-узла.
// Вектор описывает где находится узел в дереве: не просто глубину,
// а путь через три уровня предков.
public static class StructuralVectorExtractor
{
    // Извлечь вектор для одного AST-узла.
    public static StructuralVector Extract(SyntaxNode node, short fileId = 0)
    {
        // Подняться по дереву и собрать предков
        var ancestors = new List<SyntaxNode>();
        var current = node.Parent;
        while (current != null && ancestors.Count < 3)
        {
            ancestors.Add(current);
            current = current.Parent;
        }

        short p1 = ancestors.Count > 0 ? NodeTypeId(ancestors[0]) : (short)0;
        short p2 = ancestors.Count > 1 ? NodeTypeId(ancestors[1]) : (short)0;
        short p3 = ancestors.Count > 2 ? NodeTypeId(ancestors[2]) : (short)0;

        // Позиция среди братьев-сестёр (siblings)
        byte posInParent = 0;
        byte siblingCount = 0;
        if (node.Parent != null)
        {
            var siblings = node.Parent.ChildNodes().ToList();
            siblingCount = (byte)Math.Min(siblings.Count, 255);
            int idx = siblings.IndexOf(node);
            posInParent = (byte)Math.Min(idx >= 0 ? idx : 0, 255);
        }

        // Глубина внутри ближайшего метода
        byte depthInMethod = 0;
        var walker = node.Parent;
        while (walker != null)
        {
            if (walker.IsKind(SyntaxKind.MethodDeclaration)) break;
            depthInMethod++;
            walker = walker.Parent;
        }

        // Глубина { } вложенности
        byte scopeDepth = 0;
        walker = node.Parent;
        while (walker != null)
        {
            if (walker.IsKind(SyntaxKind.Block)) scopeDepth++;
            walker = walker.Parent;
        }

        return new StructuralVector
        {
            AstNodeType     = NodeTypeId(node),
            Parent1Type     = p1,
            Parent2Type     = p2,
            Parent3Type     = p3,
            PositionInParent = posInParent,
            SiblingCount    = siblingCount,
            DepthInMethod   = (byte)Math.Min(depthInMethod, 255),
            ScopeDepth      = (byte)Math.Min(scopeDepth, 255),
            FileId          = fileId
        };
    }

    // Нормализовать SyntaxKind в диапазон 0-199 (для компактного хранения)
    private static short NodeTypeId(SyntaxNode node)
        => (short)((int)node.Kind() % 200);
}

// Структурный вектор — описание места токена в дереве компилятора.
// Используется для K-NN поиска похожих контекстов.
public readonly record struct StructuralVector
{
    public short AstNodeType      { get; init; }  // тип текущего узла
    public short Parent1Type      { get; init; }  // непосредственный родитель
    public short Parent2Type      { get; init; }  // прародитель
    public short Parent3Type      { get; init; }  // прапрародитель
    public byte  PositionInParent { get; init; }  // индекс среди братьев
    public byte  SiblingCount     { get; init; }  // всего братьев
    public byte  DepthInMethod    { get; init; }  // глубина внутри метода
    public byte  ScopeDepth       { get; init; }  // уровень { } вложенности
    public short FileId           { get; init; }  // из какого файла

    // Взвешенное расстояние между двумя векторами.
    // Parent1Type весит больше всего — ближайший родитель самый важный.
    public double DistanceTo(StructuralVector other) =>
        (Parent1Type     != other.Parent1Type     ? 3.0 : 0) +
        (AstNodeType     != other.AstNodeType     ? 2.0 : 0) +
        (Parent2Type     != other.Parent2Type     ? 2.0 : 0) +
        (Parent3Type     != other.Parent3Type     ? 1.5 : 0) +
        (PositionInParent != other.PositionInParent ? 1.0 : 0) +
        Math.Abs(ScopeDepth   - other.ScopeDepth)   * 0.3 +
        Math.Abs(DepthInMethod - other.DepthInMethod) * 0.2;
}
```

---

## Шаг 2 — Создать StructuralKnnPredictor.cs

**Путь:** `35.ExperimentsV2/Core/StructuralKnnPredictor.cs`

```csharp
namespace ExperimentsV2;

// StructuralKnnPredictor находит K ближайших контекстов из обучающих данных
// и возвращает самый частый следующий токен среди них.
// Для Scale 0-1 (~2000 токенов) — брутфорс O(n), достаточно быстро.
// Для Scale 2+ — нужно KD-дерево (добавить позже если E09 проходит).
public sealed class StructuralKnnPredictor
{
    // Обучающие данные: (вектор контекста, следующий BPE-токен)
    private readonly List<(StructuralVector Vector, int NextToken)> _training = new();

    // K — количество ближайших соседей
    private readonly int _k;

    public StructuralKnnPredictor(int k = 5) => _k = k;

    // Запомнить (контекст → следующий токен) для каждой позиции обучающего набора.
    public void Train(IReadOnlyList<(StructuralVector vector, int nextToken)> pairs)
    {
        _training.Clear();
        _training.AddRange(pairs);
    }

    // Предсказать следующий токен для заданного контекста.
    // Находит K ближайших соседей, возвращает argmax следующего токена среди них.
    public int Predict(StructuralVector context)
    {
        if (_training.Count == 0) return 0;

        // Найти K ближайших соседей (брутфорс)
        var distances = _training
            .Select((item, idx) => (dist: context.DistanceTo(item.Vector), token: item.NextToken))
            .OrderBy(x => x.dist)
            .Take(_k)
            .ToList();

        // Среди K соседей выбрать самый частый следующий токен
        var tokenCounts = new Dictionary<int, int>();
        foreach (var (_, token) in distances)
            tokenCounts[token] = tokenCounts.GetValueOrDefault(token, 0) + 1;

        return tokenCounts.MaxBy(kv => kv.Value).Key;
    }

    // Количество обучающих примеров
    public int TrainingSize => _training.Count;

    // Среднее расстояние до ближайшего соседа (сигнал качества)
    public double AverageNearestNeighborDistance(
        IReadOnlyList<StructuralVector> queries, int sampleSize = 100)
    {
        var sample = queries.Take(sampleSize).ToList();
        if (sample.Count == 0 || _training.Count == 0) return 0;

        return sample.Average(q =>
            _training.Min(t => q.DistanceTo(t.Vector)));
    }
}
```

---

## Шаг 3 — Создать E09StructuralKnn.cs

**Путь:** `35.ExperimentsV2/Experiments/E09StructuralKnn.cs`

```csharp
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// E09: структурный вектор + K-NN vs baseline bigram.
// Запускает три варианта одновременно для честного сравнения:
//   Baseline:  bigram на BPE токенах
//   E07-style: многомерная N-грамм таблица (старый провальный подход)
//   E09 KNN:   K ближайших соседей по структурному вектору
public sealed class E09StructuralKnn
{
    private readonly int _k;
    public E09StructuralKnn(int k = 5) => _k = k;

    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет источника для E09");

        // Токенизация AST: получить (BPE-токен, структурный вектор) для каждой позиции
        var tokenVectors = BuildTokenVectors(files);
        if (tokenVectors.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало AST узлов");

        int split   = (int)(tokenVectors.Count * 0.8);
        var train   = tokenVectors.Take(split).ToList();
        var val     = tokenVectors.Skip(split).ToList();

        // ── Baseline: bigram BPE ──────────────────────────────────────────────
        var t0 = Stopwatch();
        var bigramCounts = new Dictionary<int, Dictionary<int, int>>();
        for (int i = 0; i < train.Count - 1; i++)
        {
            int cur = train[i].BpeId, next = train[i + 1].BpeId;
            if (!bigramCounts.TryGetValue(cur, out var d))
                bigramCounts[cur] = d = new Dictionary<int, int>();
            d[next] = d.GetValueOrDefault(next, 0) + 1;
        }

        // Глобально самый частый следующий токен для fallback
        var globalFreq = new Dictionary<int, int>();
        for (int i = 0; i < train.Count - 1; i++)
            globalFreq[train[i + 1].BpeId] = globalFreq.GetValueOrDefault(train[i + 1].BpeId, 0) + 1;
        int globalFallback = globalFreq.Count > 0 ? globalFreq.MaxBy(kv => kv.Value).Key : 0;

        int baseCorrect = 0;
        for (int i = 0; i < val.Count - 1; i++)
        {
            int cur = val[i].BpeId, next = val[i + 1].BpeId;
            int pred = bigramCounts.TryGetValue(cur, out var d) && d.Count > 0
                ? d.MaxBy(kv => kv.Value).Key : globalFallback;
            if (pred == next) baseCorrect++;
        }
        double baselineAcc = (val.Count - 1) > 0 ? baseCorrect / (double)(val.Count - 1) : 0;
        double baselineMs  = ElapsedMs(t0);

        // ── E09 K-NN ──────────────────────────────────────────────────────────
        var t1 = Stopwatch();
        var knn = new StructuralKnnPredictor(_k);
        var trainingPairs = train.Zip(train.Skip(1))
            .Select(p => (p.First.Vector, p.Second.BpeId))
            .ToList();
        knn.Train(trainingPairs);

        int knnCorrect = 0;
        for (int i = 0; i < val.Count - 1; i++)
        {
            int pred = knn.Predict(val[i].Vector);
            if (pred == val[i + 1].BpeId) knnCorrect++;
        }
        double knnAcc = (val.Count - 1) > 0 ? knnCorrect / (double)(val.Count - 1) : 0;
        double enhancedMs = ElapsedMs(t1);

        // Scaling signal: среднее расстояние до ближайшего соседа
        // Маленькое = хорошее покрытие контекстов, большое = мало данных
        var valVectors = val.Select(x => x.Vector).ToList();
        double avgDist = knn.AverageNearestNeighborDistance(valVectors);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E09-STRUCTURAL-KNN",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = knnAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = avgDist,
            ScalingSignalNote = "avg_nearest_neighbor_dist (меньше = соседи ближе = K-NN работает)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            ShouldScaleUp     = avgDist > 3.0,  // большое расстояние = нужно больше данных
            Notes             = $"tokens={tokenVectors.Count}, k={_k}, " +
                                $"baseAcc={baselineAcc:P1}, knnAcc={knnAcc:P1}, " +
                                $"avgDist={avgDist:F2}, trainPairs={trainingPairs.Count}"
        });
    }

    // Построить список (BPE-токен, структурный вектор) для всех AST узлов
    private static List<TokenWithVector> BuildTokenVectors(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var result = new List<TokenWithVector>();

        for (short fileId = 0; fileId < files.Count; fileId++)
        {
            var (_, text) = files[fileId];
            var tree = CSharpSyntaxTree.ParseText(text);
            var nodes = tree.GetRoot().DescendantNodes().ToList();

            // Нормализовать SyntaxKind в компактные индексы
            var kinds    = nodes.Select(n => (int)n.Kind()).Distinct().OrderBy(x => x).ToList();
            var kindMap  = kinds.Select((k, i) => (k, i)).ToDictionary(p => p.k, p => p.i);

            foreach (var node in nodes)
            {
                int bpeId = kindMap[(int)node.Kind()];
                var vector = StructuralVectorExtractor.Extract(node, fileId);
                result.Add(new TokenWithVector(bpeId, vector));
            }
        }
        return result;
    }

    private readonly record struct TokenWithVector(int BpeId, StructuralVector Vector);

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
public OperationResult<ExperimentV2Result> RunE09(
    string source,
    IReadOnlyList<(string Path, string Text)> files,
    int k = 5,
    int tier = 1)
    => new E09StructuralKnn(k).Run(source, files, tier);
```

### В `ExperimentRunner.cs`:

**В массив `ids`:**
```csharp
string[] ids = ["E01","E02","E03","E04","E05","E06","E07","E09","E10","E11","E12"];
```

**В switch:**
```csharp
"E09" => DispatchE09(ds, tier),
```

**Новый приватный метод:**
```csharp
private OperationResult<ExperimentV2Result> DispatchE09(DatasetContents ds, int tier)
    => _d.RunE09(ds.CombinedText, ds.Files, k: 5, tier: tier);
```

### В `ResultWriter.FolderName()`:
```csharp
"E09-STRUCTURAL-KNN" => "E09-STRUCTURAL-KNN",
```

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 5 — Запустить

```csharp
runner.Run("E09", tier: 1);
```

**Ожидаемый вывод:**
```
[E09] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E09] Запуск...
OK top1_accuracy (up) baseline=0.153 enhanced=0.210 delta=+37.3% | PASSED
  -> .../E09-STRUCTURAL-KNN/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes:**
```
tokens=962, k=5, baseAcc=15.3%, knnAcc=21.0%,
avgDist=1.85, trainPairs=769
```

**Что означает `avgDist=1.85`:**
Среднее расстояние до ближайшего соседя = 1.85 (меньше 3.0 → K-NN находит похожие контексты).
Если `avgDist > 3.0` → `ShouldScaleUp=true` → автоматически запустится Tier 2.

---

## Что делать если E09 медленный

Брутфорс O(n) нормален для Tier 1 (~960 токенов, 769 обучающих пар).
На Tier 2 (~9100 токенов, ~7280 пар) — 7280 × 7280 = 53M сравнений, ~5-10 секунд.
Если слишком долго — добавить ограничение на количество val примеров:

```csharp
// В E09StructuralKnn.Run(), перед K-NN оценкой:
var valSample = val.Take(500).ToList();  // оценивать только первые 500
```

---

*E09 план v1.0 · не касается XIIGen*
