# Experiments via Markov proxy — tiny datasets, milliseconds per run
## EXPERIMENT-HARNESS-v2.md · LlmFromScratch.sln

**Ключевой сдвиг от v1:** вместо зонд-нейросети как первого шага — сначала Марковская цепь.
Датасет — одна книга или один .cs файл. Прогон — миллисекунды.

---

## Почему Марков = быстрый прокси для нейросети

Марковская цепь и обученная матрица весов отвечают на один вопрос:

```
P(следующий токен | предыдущие токены) = ?
```

Марков считает это напрямую: `M[prev][next] = count(prev→next) / count(prev)`  
Нейросеть выучивает это градиентным спуском: `softmax(W × embed(context))`

Результат один и тот же — матрица вероятностей перехода. Если подход X улучшает точность Маркова — он улучшит и точность нейросети. Это не приближение; это та же математика на разной глубине.

---

## Три уровня проверки одной гипотезы

```
Уровень 1: Марковская цепь     ← этот документ
  Датасет: 1 книга или 1 .cs файл (~5,000–50,000 токенов)
  Время:   миллисекунды
  Цель:    показать что подход меняет распределение переходов

Уровень 2: Зонд-нейросеть      ← EXPERIMENT-HARNESS-v1.md
  Датасет: 1 подпроект (~2,000 токенов)
  Время:   секунды–минуты
  Цель:    подтвердить улучшение в нелинейной модели

Уровень 3: Полный трансформер
  Датасет: 10–24 подпроекта или WikiText-103
  Время:   часы
  Цель:    финальное подтверждение
```

Правило перехода: эксперимент движется на следующий уровень только если предыдущий показал ≥5% улучшения.

---

## Датасеты для уровня 1

Любой из трёх работает. Выбирать по удобству:

| Датасет | Размер | Токенов (символьный уровень) | Откуда |
|---|---|---|---|
| "Alice in Wonderland" | 165 KB | ~600,000 символов | Project Gutenberg, public domain |
| 01.Tokenizer (весь подпроект) | ~10 KB | ~40,000 символов | проект, всегда под рукой |
| Один метод из BpeTokenizer.cs | ~3 KB | ~10,000 символов | проект, минимально возможный |

Для первых прогонов брать `01.Tokenizer` — он уже есть, не нужно ничего скачивать.

---

## Что такое "точность Маркова"

Берём текст, разбиваем на токены (символьный уровень или BPE).  
Строим матрицу переходов на 80% текста (тренировочный срез).  
На оставшихся 20% (валидационный срез) для каждого токена предсказываем следующий как `argmax(M[current])`.  
Сравниваем с реальным следующим.

```csharp
// MarkovAccuracy считает процент правильных предсказаний следующего токена.
// Это то же измерение что top-1 accuracy у нейросети — просто гораздо быстрее.
public static double ComputeMarkovAccuracy(
    IReadOnlyList<int> trainTokens,   // 80% текста для построения матрицы
    IReadOnlyList<int> valTokens,     // 20% текста для проверки
    int vocabularySize)
{
    // Построить матрицу переходов: transitions[prev][next] = сколько раз next следовало за prev
    var transitions = new long[vocabularySize, vocabularySize];
    for (int i = 0; i < trainTokens.Count - 1; i++)
        transitions[trainTokens[i], trainTokens[i + 1]]++;

    // Оценить точность: для каждого токена в валидации предсказать следующий
    int correct = 0;
    for (int i = 0; i < valTokens.Count - 1; i++)
    {
        // Найти наиболее вероятный следующий токен согласно матрице
        int bestNext = FindMostLikelyNext(transitions, valTokens[i], vocabularySize);

        // Проверить совпадает ли с реальным следующим токеном
        if (bestNext == valTokens[i + 1]) correct++;
    }

    return (double)correct / (valTokens.Count - 1);
}
```

---

## Шесть экспериментов — Марковская версия

### E01-M — Семантические семьи vs плоские токены

**Гипотеза:** если заменить токен его семантической семьёй (K-Means кластер), матрица переходов становится меньше и точнее — меньше состояний, выше вероятность каждого.

**Baseline Марков:** токены — отдельные BPE id. Словарь ~2000 токенов. Матрица 2000×2000.

**Enhanced Марков:** каждый токен заменяется номером его K-Means кластера (50 кластеров). Матрица 50×50. Предсказываем кластер, потом токен внутри кластера.

**Почему это работает:** "attack", "hit", "strike" — три разных токена в baseline. В enhanced — один токен "насилие" (кластер 47). Матрица переходов для "насилие → жертва" намного плотнее и точнее чем три разрозненных перехода.

```csharp
// Преобразовать последовательность токенов в последовательность кластеров
// Это единственное изменение между baseline и enhanced
static int[] TokensToClusterIds(int[] tokens, Dictionary<int, int> tokenToCluster)
    => tokens.Select(t => tokenToCluster.GetValueOrDefault(t, 0)).ToArray();
```

**Ожидаемый результат на Alice:** accuracy +15–25% (словарь уменьшается с ~2000 до 50 → каждый переход гораздо более предсказуем).

---

### E02-M — Энтропия перед сжатием vs после

**Гипотеза:** Zstandard сжимает хорошо то что предсказуемо. Предсказуемость = низкая энтропия Шеннона. Если наш токенизатор создаёт более предсказуемый поток — сжатие будет лучше.

Это единственный эксперимент где не нужна Марковская матрица — достаточно посчитать энтропию токенного потока.

**Baseline:** энтропия потока BPE токенов.  
**Enhanced:** энтропия потока семантических объектов (токены по кластерам).

```csharp
// Shannon entropy of a token sequence
// Меньше = более предсказуемая последовательность = лучше сжимается
static double ShannonEntropy(IReadOnlyList<int> tokens)
{
    var counts = new Dictionary<int, int>();
    foreach (var t in tokens) counts[t] = counts.GetValueOrDefault(t, 0) + 1;

    double entropy = 0;
    double n = tokens.Count;
    foreach (var c in counts.Values)
    {
        double p = c / n;
        entropy -= p * Math.Log2(p);
    }
    return entropy;
}
```

**Ожидаемый результат:** энтропия -30–50% (меньше = лучше сжимается).

---

### E03-M — Локальное окно vs полный контекст

**Гипотеза:** ограничить Марков N предыдущими токенами вместо 1 — улучшает ли accuracy? И важно: узкое окно должно давать ТАКОЕ ЖЕ или ЛУЧШЕЕ улучшение чем широкое при меньших ресурсах.

**Baseline Марков:** биграм (1 предыдущий токен), полная матрица.  
**Enhanced Марков:** триграм (2 предыдущих токена) + ограниченное окно.

```csharp
// Trigram Markov: P(next | prev2, prev1) — использует два предыдущих токена вместо одного
// Более точный но требует больше памяти (V² состояний вместо V)
// Для маленького словаря (50 кластеров) это 50×50×50 = 125K vs 50×50 = 2500 — всё ещё мало
var trigramTransitions = new Dictionary<(int, int), int[]>();
```

**Ожидаемый результат:** trigramm accuracy +5–15% выше bigram. При кластеризованных токенах — тригграм на 50 состояниях лучше бигграма на 2000.

---

### E04-M — PMI матрица как прямой предсказатель

**Гипотеза:** PMI (Pointwise Mutual Information — взаимная точечная информация) напрямую измеряет насколько сильно связаны два токена. PMI[a][b] = log(P(a,b) / P(a)×P(b)). Это почти то же что SVD инициализация делает с embedding матрицей — только мы можем посчитать PMI за секунды без обучения.

**Baseline:** биграм Марков (count-based).  
**Enhanced:** PMI-weighted предсказание — предсказывать `argmax(PMI[current_token])` вместо `argmax(count[current_token])`.

```csharp
// PMI[a][b] = log( P(a followed by b) / (P(a) * P(b)) )
// Положительный PMI = b часто следует за a больше чем ожидается по случаю
// Отрицательный PMI = редкое сочетание
static double[,] ComputePmi(long[,] cooccurrences, long[] marginals, int vocabSize)
{
    long total = marginals.Sum();
    var pmi = new double[vocabSize, vocabSize];
    for (int a = 0; a < vocabSize; a++)
    for (int b = 0; b < vocabSize; b++)
    {
        if (cooccurrences[a, b] == 0) continue;
        double pAB = cooccurrences[a, b] / (double)total;
        double pA  = marginals[a] / (double)total;
        double pB  = marginals[b] / (double)total;
        pmi[a, b]  = Math.Log2(pAB / (pA * pB));
    }
    return pmi;
}
```

**Ожидаемый результат:** PMI предсказатель +5–15% точнее count-based (он отфильтровывает частые но незначимые переходы типа "the → the").

---

### E05-M — Иерархическое предсказание vs плоское

**Гипотеза:** сначала предсказать кластер (1 из 50), потом токен внутри кластера (1 из ~40) — дешевле и точнее чем предсказывать токен напрямую (1 из 2000).

**Baseline:** одноуровневый Марков на полном словаре.  
**Enhanced:** двухуровневый Марков: первый уровень предсказывает кластер, второй — токен внутри кластера.

Ключевое измерение — не только accuracy но и РАЗМЕР матрицы:
- Baseline: 2000×2000 = 4,000,000 ячеек
- Enhanced: 50×50 + (50 × 40×40) = 2500 + 80,000 = 82,500 ячеек → в 48 раз меньше

```csharp
// Двухуровневое предсказание:
// Шаг 1: предсказать кластер следующего токена
int predictedCluster = FindMostLikelyNext(clusterTransitions, tokenToCluster[current], nClusters);

// Шаг 2: предсказать токен внутри предсказанного кластера
int predictedToken = FindMostLikelyNextInCluster(
    withinClusterTransitions[predictedCluster],
    current, clusterMembers[predictedCluster]);
```

**Ожидаемый результат:** memory -95%, time -70%, accuracy -2–5% (небольшая потеря за большой выигрыш ресурсов).

---

### E06-M — AST узлы vs символьный поток

**Это самый важный эксперимент.** Для .cs файлов — сравнить Марков на BPE токенах vs Марков на типах AST узлов.

**Baseline:** токенизировать BpeTokenizer.cs как текст → Марков на BPE.  
**Enhanced:** разобрать BpeTokenizer.cs через Roslyn → список типов AST узлов → Марков на типах.

BPE словарь на одном .cs файле: ~800 уникальных токенов.  
AST словарь: ~40 уникальных типов узлов (MethodDeclaration, IfStatement, ReturnStatement...).

```
Baseline токен-поток: ["public", " bool", " Validate", "(", "string", " token", ")", ...]
                        800 разных значений → матрица 800×800

Enhanced AST-поток:   [MethodDeclaration, ParameterList, Parameter, Block, ReturnStatement, ...]  
                        40 разных значений → матрица 40×40
```

Матрица 40×40 = 1600 ячеек. Матрица 800×800 = 640,000 ячеек. В 400 раз меньше.  
И точнее: за `MethodDeclaration` ПОЧТИ ВСЕГДА следует `ParameterList` — это детерминированная грамматика C#.

**Ожидаемый результат:** accuracy +40–60% (Марков на AST типах почти детерминированный — грамматика языка его задаёт). Memory -99.7%. Time -99%.

---

## C# реализация — всё в одном классе

```csharp
// MarkovBenchmark запускает один Марковский эксперимент и возвращает ProbeResult.
// Работает на любом тексте, занимает миллисекунды.
// Используется как первый уровень проверки перед запуском нейросети.
public static class MarkovBenchmark
{
    // Запустить полный прогон: tokenize → build matrix → evaluate → measure resources
    // textPath     — путь к файлу с текстом (любой .cs, .txt, .md)
    // tokenizer    — как превращать текст в токены (baseline: CharLevel, enhanced: ClusterLevel)
    // nGramOrder   — 1 = биграм (1 пред. токен), 2 = триграм (2 пред. токена)
    public static ProbeResult Run(
        string textPath,
        Func<string, int[]> tokenizer,
        int nGramOrder = 1)
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        double cpuStart = process.TotalProcessorTime.TotalMilliseconds;
        var sw = Stopwatch.StartNew();

        // Загрузить текст и токенизировать
        var text = File.ReadAllText(textPath);
        var tokens = tokenizer(text);

        // Определить размер словаря (максимальный индекс токена + 1)
        int vocabSize = tokens.Max() + 1;

        // Разбить на тренировочный (80%) и валидационный (20%) срезы
        int splitAt = (int)(tokens.Length * 0.8);
        var train = tokens[..splitAt];
        var val   = tokens[splitAt..];

        // Построить матрицу переходов на тренировочном срезе
        var matrix = BuildTransitionMatrix(train, vocabSize, nGramOrder);

        // Оценить точность на валидационном срезе
        double accuracy = EvaluateAccuracy(matrix, val, vocabSize, nGramOrder);

        // Собрать метрики
        sw.Stop();
        process.Refresh();

        return new ProbeResult
        {
            Accuracy  = Math.Round(accuracy * 100, 2),
            TimeMs    = sw.ElapsedMilliseconds,
            MemoryMb  = Math.Round(process.PeakWorkingSet64 / 1_048_576.0, 1),
            CpuMs     = Math.Round(process.TotalProcessorTime.TotalMilliseconds - cpuStart, 1),
            DiskKb    = 0  // Марков не пишет временные файлы
        };
    }

    // Построить матрицу переходов: transitions[prev][next] = сколько раз next следовало за prev
    private static long[,] BuildTransitionMatrix(int[] tokens, int vocabSize, int order)
    {
        var matrix = new long[vocabSize, vocabSize];
        for (int i = order; i < tokens.Length; i++)
            matrix[tokens[i - order], tokens[i]]++;
        return matrix;
    }

    // Оценить точность: предсказать следующий токен для каждой позиции валидации
    private static double EvaluateAccuracy(long[,] matrix, int[] valTokens, int vocabSize, int order)
    {
        int correct = 0;
        for (int i = order; i < valTokens.Length - 1; i++)
        {
            // Найти наиболее вероятный следующий токен согласно матрице
            int prev = valTokens[i - order + order]; // упрощение для bigram
            int predicted = FindArgmax(matrix, prev, vocabSize);
            if (predicted == valTokens[i + 1]) correct++;
        }
        return (double)correct / Math.Max(1, valTokens.Length - order - 1);
    }

    // argmax по строке матрицы: токен с максимальным числом вхождений после данного
    private static int FindArgmax(long[,] matrix, int fromToken, int vocabSize)
    {
        int best = 0;
        long bestCount = -1;
        for (int j = 0; j < vocabSize; j++)
        {
            if (matrix[fromToken, j] > bestCount)
            {
                bestCount = matrix[fromToken, j];
                best = j;
            }
        }
        return best;
    }
}
```

---

## Запуск одного эксперимента — три строки кода

```csharp
// E01-M: сравнить BPE токены vs кластерные токены на файле BpeTokenizer.cs
var dataFile = "01.Tokenizer/BpeTokenizer.cs";

var baseline = MarkovBenchmark.Run(dataFile,
    text => BpeTokenizer.EncodeToInts(text));        // обычные BPE id

var enhanced = MarkovBenchmark.Run(dataFile,
    text => SemanticClusterTokenizer.EncodeToClusterIds(text, clusters: 50));  // кластеры

var delta = DeltaResult.Compute(baseline, enhanced);
BenchmarkHarness.WriteJson(new ExperimentRun { ... }, "docs/experiments/E01-semantic-tokens/RESULT-E01-markov-level1.json");
BenchmarkHarness.WriteMarkdown(..., "docs/experiments/E01-semantic-tokens/RESULT-E01-markov-level1.md");
```

Время прогона: <100 миллисекунд. Никакого GPU. Никакого WikiText-103.

---

## Почему это честный тест, а не упрощение

Марков и нейросеть оба оптимизируют одно: минимизировать неожиданность следующего токена.  
Нейросеть делает это нелинейно через много слоёв — и поэтому обычно точнее.  
Но НАПРАВЛЕНИЕ улучшения от лучшего представления токенов сохраняется на обоих уровнях.

Исключение: иногда улучшение проявляется только в глубокой модели (когда требуется нелинейная комбинация далёких контекстов). В этом случае E01-M даст 0% — это сигнал пропустить уровень 2 напрямую к нейросети.

Правило: Марков-уровень — быстрый фильтр, не финальный ответ.

---

*Harness v2.0 — Markov chain as fast proxy, tiny datasets, milliseconds per run.*
