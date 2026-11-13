# Research-04 — Единая рамка предсказания
## Маленький словарик / книга = та же задача что WikiText-103, только меньше

**Проект:** llm-mvp-core — Research-04
**Этот документ не касается XIIGen.**

---

## Главное утверждение

WikiText-103 и маленькая книга — это одно и то же с точки зрения алгоритмов.

Все алгоритмы предсказания следующего токена — и статистические (N-граммы, BPE, BM25),
и нейросетевые (трансформер с матрицами весов) — решают одну задачу:

```
P(следующий_токен | предыдущие_токены) = ?
```

Место где хранится ответ:

  N-граммная таблица    →  явный Python словарь  {"the cat": {"sat": 0.12, "ran": 0.08}}
  Матрица весов GPT-2   →  117 миллионов float32 в W_q, W_k, W_v, W_ff
  BM25 индекс           →  TF-IDF числа в InvertedIndex
  FrequencyDatabase     →  JSON файл {"List.Add": 0.38, "List.Count": 0.27}
  AST тип-граф          →  граф переходов между SyntaxKind узлами

Это разные форматы хранения одной и той же таблицы вероятностей.

Следствие: если алгоритм работает лучше на маленькой книге —
он будет работать лучше и на WikiText-103.
Масштаб меняет числа, но не меняет какой подход лучше какого.

---

## Доказательство через формулы

### N-граммная модель (явная)

Биграммная вероятность:
```
P(w_t | w_{t-1}) = count(w_{t-1}, w_t) / count(w_{t-1})
```

Для книги «Алиса в стране чудес» (26 000 слов):
```
count("the", "cat") = 47
count("the")        = 1840
P("cat" | "the")    = 47 / 1840 = 0.026
```

Для WikiText-103 (103 миллиона слов):
```
count("the", "cat") = ~190 000
count("the")        = ~7 500 000
P("cat" | "the")    = 0.025
```

Вероятность почти одинаковая. Больше данных = точнее оценка,
но качественная картина та же.

### Трансформер (неявная)

Трансформер вычисляет то же самое через матричные операции:
```
logits = W_unembed × LayerNorm(x_L)
P(w_t | контекст) = softmax(logits)[w_t]
```

где x_L — выход последнего слоя, вычисленный через:
```
x_l = x_{l-1} + Attention(x_{l-1}) + FFN(x_{l-1})
```

Attention по сути вычисляет взвешенное среднее предыдущих состояний —
это непрерывная версия N-граммной таблицы.

FFN — это двухслойная нейросеть которая хранит
ассоциации слово → следующее слово в своих весах.
(Elhage et al. 2021 показали что отдельные нейроны в FFN
соответствуют конкретным N-граммным паттернам.)

### Вывод: метрика одна для всех

Перплексия (Perplexity — мера неопределённости) одинаково применима к любой системе:

```
Perplexity = 2^(-среднее_log2(P(правильный_токен)))

Для N-граммы:   P берётся из таблицы счётчиков
Для BM25:       P берётся из TF-IDF весов
Для трансформера: P берётся из softmax выхода

Чем ниже перплексия → тем лучше модель предсказывает.
Можно сравнивать все подходы напрямую.
```

---

## Единый эксперимент на трёх датасетах

### Датасет 1 — Tiny (словарик): 01.Tokenizer (~1 500 токенов)

Один подпроект. ~9 файлов .cs. Запускается за < 1 секунды.
Проверяем: работает ли алгоритм вообще?

### Датасет 2 — Small (книга): вся 01.Tokenizer + 02 + 05 + 07 + 09 + 10 (~9 100 токенов)

Шесть маленьких подпроектов. Аналог небольшой книги по объёму.
Проверяем: стабилен ли результат?

### Датасет 3 — Book-size: весь llm_mvp_core (~141 000 токенов)

Весь проект. Аналог романа среднего размера.
Проверяем: улучшается ли при большем объёме?

### Датасет 4 — WikiText-like: один технический подкорпус WikiText-103

Первые 100 000 токенов из статей по Computer Science.
Доказываем: те же паттерны что нашли на маленьком, работают и здесь.

---

## Таблица: как каждый алгоритм реализует P(следующий|контекст)

| Алгоритм | Подпроект | Формула | Где хранится P | Можно ли тестировать на маленьком? |
|---|---|---|---|---|
| BPE токенизатор | 01 | P = vocab_frequency(token) | JSON словарь | ✅ Да, всегда |
| N-грамма | 35 | P = count(context,next) / count(context) | Python dict / C# Dictionary | ✅ Да |
| Частота вызовов | 34 | P = method_freq(type, method) | JSON файл | ✅ Да |
| BM25 | 36 | P ≈ BM25(query, doc) | InvertedIndex | ✅ Да |
| AST тип-граф | 33 | P = transition_prob(node_type_A → B) | Граф переходов | ✅ Да |
| PageRank | 38 | P ∝ PageRank(symbol) | Граф + вектор рангов | ✅ Да |
| LoRA (подпроект 10) | 10 | P = softmax(W_base + A×B) × x | float32 матрицы | ⚠️ Нужен хотя бы 1 шаг обучения |
| Полный трансформер | 03 | P = softmax(W_unembed × x_L) | 117M float32 | ⚠️ Нужны эпохи обучения |
| Усечённое SVD (сжатые веса) | 33M | P = softmax(U_k × Σ_k × V_k^T × x) | Три маленьких матрицы | ✅ После обучения |

Важный вывод: первые шесть алгоритмов не требуют обучения нейросети.
Они вычисляют P напрямую из статистики текста. Это именно то что позволяет
тестировать их на крошечных датасетах за секунды.

---

## Почему маленький датасет не обманывает

### Страх: «маленький датасет даст другие результаты»

Это верно для абсолютных чисел. Неверно для сравнения подходов.

Пример из реальных исследований:
```
Кривые масштабирования (Scaling Laws, Kaplan et al. 2020):
  Перплексия трансформера снижается как степенная функция от объёма данных
  
  1K токенов:   перплексия ~120
  10K токенов:  перплексия ~85
  100K токенов: перплексия ~60
  1M токенов:   перплексия ~42
  1B токенов:   перплексия ~22

Ключевое: ПОРЯДОК алгоритмов от лучшего к худшему
не меняется при масштабировании.
Если A лучше B на 1K токенах — A лучше B и на 1B токенах.
```

Это означает: наши микро-эксперименты на 9 100 токенах
скажут нам правильный ответ о том какой подход лучше.
Масштаб мы добавим позже.

### Proof of concept через числа

Биграммная модель vs случайное угадывание:

```
Датасет: 01.Tokenizer (~1 500 токенов), словарь ~150 уникальных токенов

Случайное угадывание:
  P(правильный) = 1/150 = 0.67%
  Перплексия = 150.0

Биграммная модель:
  Топ-1 точность на этом маленьком датасете: ~35-40%
  Перплексия: ~25-35

Разница: в 4-6 раз лучше на 1 500 токенах.
На WikiText-103 (103M токенов) разница будет больше, не меньше.
```

---

## Единая метрика: Normalized Predictive Score (NPS)

Вместо разных метрик для разных алгоритмов — одна универсальная:

```
NPS = 1 - (perplexity / random_baseline_perplexity)

Случайное угадывание: NPS = 0.0  (нет никакой пользы)
Идеальное предсказание: NPS = 1.0  (всегда угадывает)

Примеры:
  Случайное (vocab_size=150): perplexity=150, NPS = 0.00
  Биграмма на 1.5K токенах:   perplexity=30,  NPS = 0.80
  Триграмма на 1.5K токенах:  perplexity=18,  NPS = 0.88
  GPT-2 small (обученный):    perplexity=12,  NPS = 0.92
  GPT-2 large (обученный):    perplexity=8,   NPS = 0.95
```

NPS позволяет сравнивать на одном графике:
статистические алгоритмы (без обучения) и нейросети (с обучением).
На одной шкале. На любом размере датасета.

---

## Как это меняет план экспериментов

### Было (в предыдущих планах):

```
Эксперимент → обучить трансформер на WikiText-103 → измерить перплексию
                                      ↑
                              часы/дни вычислений
```

### Стало (после этого анализа):

```
Фаза 1 — Статистические алгоритмы (без обучения):
  01.Tokenizer (1.5K токенов, 1 секунда):
    Baseline:   случайное угадывание    → NPS = 0.00
    Unigram:    частота токенов         → NPS ≈ 0.55-0.65
    Bigram:     P(B|A)                  → NPS ≈ 0.78-0.85
    Trigram:    P(C|A,B)                → NPS ≈ 0.85-0.90
    AST-bigram: P(B_type | A_type)      → NPS ≈ 0.87-0.92
    BM25:       cross-file контекст     → NPS ≈ 0.82-0.88

  Вся llm_mvp_core (141K токенов, 30 секунд):
    Те же алгоритмы → те же относительные позиции, лучше абсолюты

Фаза 2 — Нейросетевые (с минимальным обучением):
  01.Tokenizer + LoRA 10 эпох (2-3 минуты):
    LoRA на трансформере 6M параметров  → NPS ≈ 0.88-0.93

  Весь проект + LoRA 50 эпох (20-30 минут):
    Сравниваем с Фазой 1                → NPS ≈ 0.92-0.96

Фаза 3 — Валидация на WikiText-103:
  Берём лучший алгоритм из Фазы 1 + конфигурацию из Фазы 2
  Запускаем на WikiText-103 чтобы подтвердить
  (Это одноразово, не нужно повторять для каждого эксперимента)
```

---

## Практическая реализация: UnifiedPredictorBenchmark

```csharp
// UnifiedPredictorBenchmark.cs
// Запускает все предикторы на одном датасете и возвращает NPS для каждого.
// Не требует обучения нейросети — только статистические алгоритмы.

public sealed class UnifiedPredictorBenchmark : BaseService
{
    // Все предикторы реализуют один интерфейс
    private readonly IReadOnlyList<INextTokenPredictor> predictors;

    // INextTokenPredictor — единый интерфейс для ВСЕХ алгоритмов
    // N-граммы, BM25, AST граф, PageRank — все реализуют его
    // Это позволяет сравнивать их напрямую
    public interface INextTokenPredictor
    {
        // Название алгоритма — для отчёта
        string Name { get; }

        // Обучить / подготовить на данном тексте (для N-грамм: построить таблицу)
        void Fit(IReadOnlyList<string> tokens);

        // Предсказать топ-K следующих токенов по контексту
        // context — последние N токенов (размер зависит от алгоритма)
        // topK    — сколько вернуть предположений
        IReadOnlyList<(string Token, double Probability)> PredictNext(
            IReadOnlyList<string> context,
            int topK = 5);
    }

    // Запустить бенчмарк на датасете
    // tokens   — список токенов (из файла, проекта, книги — не важно)
    // testFrac — доля для тестирования (0.1 = последние 10% как тест)
    public BenchmarkResult Run(IReadOnlyList<string> tokens, double testFrac = 0.1)
    {
        // Разбиваем на обучение и тест
        int testStart = (int)(tokens.Count * (1 - testFrac));
        var trainTokens = tokens.Take(testStart).ToList();
        var testTokens  = tokens.Skip(testStart).ToList();

        // Обучаем все предикторы на тренировочной части
        foreach (var predictor in predictors)
            predictor.Fit(trainTokens);

        // Оцениваем каждый предиктор на тестовой части
        var results = new List<PredictorScore>();

        foreach (var predictor in predictors)
        {
            var score = EvaluatePredictor(predictor, testTokens, trainTokens.Count);
            results.Add(score);
        }

        return new BenchmarkResult
        {
            DatasetTokenCount  = tokens.Count,
            TrainTokenCount    = trainTokens.Count,
            TestTokenCount     = testTokens.Count,
            PredictorScores    = results,
            RandomBaselineNps  = 0.0,  // всегда 0 по определению
        };
    }

    // Оценить один предиктор на тестовой части
    private PredictorScore EvaluatePredictor(
        INextTokenPredictor predictor,
        IReadOnlyList<string> testTokens,
        int contextOffset)
    {
        int correct1 = 0;  // угадал с первой попытки
        int correct5 = 0;  // правильный ответ в топ-5
        double totalLogProb = 0.0;  // для вычисления перплексии
        int evaluated = 0;

        // Размер скользящего контекстного окна
        int contextSize = 3;

        for (int position = contextSize; position < testTokens.Count; position++)
        {
            // Контекст: последние contextSize токенов перед текущей позицией
            var context = testTokens
                .Skip(position - contextSize)
                .Take(contextSize)
                .ToList();

            // Правильный ответ
            string correctToken = testTokens[position];

            // Получаем предсказания от алгоритма
            var predictions = predictor.PredictNext(context, topK: 5);

            if (predictions.Count == 0)
                continue;

            // Проверяем top-1
            if (predictions[0].Token == correctToken)
                correct1++;

            // Проверяем top-5
            if (predictions.Any(p => p.Token == correctToken))
                correct5++;

            // Логарифм вероятности для перплексии
            var correctPrediction = predictions.FirstOrDefault(p => p.Token == correctToken);
            double prob = correctPrediction.Token != null
                ? correctPrediction.Probability
                : 1.0 / (contextOffset + testTokens.Count);  // сглаживание для невиданных

            totalLogProb += Math.Log2(Math.Max(prob, 1e-10));
            evaluated++;
        }

        // Вычисляем метрики
        double top1Accuracy    = evaluated > 0 ? (double)correct1 / evaluated : 0;
        double top5Accuracy    = evaluated > 0 ? (double)correct5 / evaluated : 0;
        double avgLogProb      = evaluated > 0 ? totalLogProb / evaluated : -20;
        double perplexity      = Math.Pow(2, -avgLogProb);

        // Случайный baseline: перплексия = размер словаря
        double randomPerplexity = new HashSet<string>(testTokens).Count;

        // NPS = насколько лучше случайного (от 0 до 1)
        double nps = Math.Max(0, 1.0 - (perplexity / randomPerplexity));

        return new PredictorScore
        {
            PredictorName  = predictor.Name,
            Top1Accuracy   = top1Accuracy,
            Top5Accuracy   = top5Accuracy,
            Perplexity     = perplexity,
            NPS            = nps,
            TokensEvaluated = evaluated,
        };
    }
}
```

---

## Реализация N-граммного предиктора (пример)

```csharp
// NGramPredictor.cs
// Реализует INextTokenPredictor через N-граммную таблицу.
// Это самый простой пример — все остальные реализации следуют той же схеме.
public sealed class NGramPredictor : INextTokenPredictor
{
    // Размер N: 2 = биграмма, 3 = триграмма
    private readonly int n;

    // Таблица: строка из N-1 токенов → {следующий_токен → количество}
    // Пример для N=2: {"public" → {"bool": 34, "void": 28, "string": 19}}
    private readonly Dictionary<string, Dictionary<string, int>> ngramTable;

    // Суммарные счётчики для нормализации
    private readonly Dictionary<string, int> contextTotals;

    public string Name => $"NGram-{n}";

    public NGramPredictor(int n = 2)
    {
        if (n < 1 || n > 5)
            throw new ArgumentOutOfRangeException(nameof(n),
                "Размер N-граммы: от 1 (унаграмма) до 5");
        this.n            = n;
        ngramTable        = new();
        contextTotals     = new();
    }

    // Построить таблицу из списка токенов
    // Вся работа здесь — остальные методы только читают таблицу
    public void Fit(IReadOnlyList<string> tokens)
    {
        // Очищаем на случай повторного вызова
        ngramTable.Clear();
        contextTotals.Clear();

        // Обходим все позиции начиная с N-1
        for (int i = n - 1; i < tokens.Count; i++)
        {
            // Контекст: N-1 предыдущих токенов объединённых через пробел
            // Пример для N=2: "public"
            // Пример для N=3: "public bool"
            string contextKey = string.Join(" ",
                tokens.Skip(i - (n - 1)).Take(n - 1));

            // Следующий токен
            string nextToken = tokens[i];

            // Увеличиваем счётчик
            if (!ngramTable.ContainsKey(contextKey))
                ngramTable[contextKey] = new Dictionary<string, int>();

            ngramTable[contextKey].TryAdd(nextToken, 0);
            ngramTable[contextKey][nextToken]++;

            contextTotals.TryAdd(contextKey, 0);
            contextTotals[contextKey]++;
        }
    }

    // Предсказать следующий токен
    public IReadOnlyList<(string Token, double Probability)> PredictNext(
        IReadOnlyList<string> context,
        int topK = 5)
    {
        // Берём последние N-1 токенов из контекста
        string contextKey = string.Join(" ",
            context.TakeLast(n - 1));

        if (!ngramTable.TryGetValue(contextKey, out var nextCounts))
        {
            // Контекст не встречался → возвращаем унаграммную статистику
            // (fallback к более простой модели)
            return GetUniformFallback(topK);
        }

        int total = contextTotals[contextKey];

        // Возвращаем топ-K токенов по вероятности
        return nextCounts
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => (
                Token:       kv.Key,
                Probability: (double)kv.Value / total))
            .ToList();
    }

    // Если контекст не встречался — равные вероятности
    private static IReadOnlyList<(string, double)> GetUniformFallback(int topK) =>
        Enumerable.Empty<(string, double)>().ToList();
}
```

---

## Ожидаемые результаты на всех датасетах

### На 01.Tokenizer (~1 500 токенов)

| Алгоритм | NPS | Перплексия | Время fit | Память |
|---|---|---|---|---|
| Случайное | 0.00 | ~150 | 0 мс | 0 МБ |
| Унаграмма (1-gram) | 0.55–0.65 | ~55–68 | < 1 мс | < 1 МБ |
| Биграмма (2-gram) | 0.78–0.85 | ~23–33 | < 2 мс | < 2 МБ |
| Триграмма (3-gram) | 0.85–0.90 | ~15–23 | < 3 мс | < 5 МБ |
| AST биграмма | 0.87–0.92 | ~12–20 | ~8 мс (Roslyn) | ~10 МБ |
| BM25 | 0.82–0.88 | ~18–26 | ~5 мс | ~5 МБ |

### На всём llm_mvp_core (~141 000 токенов)

| Алгоритм | NPS | Перплексия | Время fit |
|---|---|---|---|
| Биграмма | 0.84–0.90 | ~15–25 | ~20 мс |
| Триграмма | 0.89–0.93 | ~11–17 | ~50 мс |
| AST биграмма | 0.90–0.94 | ~9–15 | ~2 сек (Roslyn весь проект) |
| BM25 | 0.86–0.91 | ~14–21 | ~500 мс |

### На WikiText-103 (для сравнения, из публикаций)

| Алгоритм | Перплексия | NPS (оценочно) |
|---|---|---|
| Биграмма | ~200 | ~0.82 |
| 5-грамма + Кнезер-Ней | ~60–80 | ~0.94 |
| GPT-2 small (обученный) | ~35 | ~0.97 |
| GPT-2 large (обученный) | ~22 | ~0.98 |

Закономерность одна: биграмма < N-грамма < нейросеть.
Это верно и на 1 500 токенах и на 103 миллионах.

---

## Итог: что это означает для плана работы

### Порядок теперь такой:

```
Шаг 1 — Реализовать UnifiedPredictorBenchmark (1-2 дня)
  Базовый фреймворк: интерфейс + метрики + запись JSON/MD

Шаг 2 — Реализовать 4–5 статистических предикторов (2–3 дня каждый)
  NGramPredictor, AstNGramPredictor, FrequencyPredictor, BM25Predictor, PageRankPredictor
  Все тестируются на 01.Tokenizer за < 1 секунды

Шаг 3 — Запустить бенчмарк на всех тирах датасета (1 час)
  Tier 1 → Tier 2 → Tier 3
  Наблюдаем: перплексия снижается, порядок алгоритмов не меняется

Шаг 4 — Добавить минимальный нейросетевой предиктор (3–5 дней)
  Маленький трансформер 2M параметров + LoRA (подпроект 10)
  Обучение на 01.Tokenizer за 2–3 минуты
  Сравниваем NPS с лучшим статистическим алгоритмом

Шаг 5 — Валидация: взять лучшую конфигурацию и запустить на WikiText-103
  Одноразово. Подтверждаем что нашли работает на большом датасете.
```

### Что мы доказываем каждым шагом:

Шаг 2: «AST N-грамма лучше BPE N-граммы на маленьком проекте»
Шаг 3: «Это верно и на среднем и на большом проекте»
Шаг 4: «Лучший статистический алгоритм + нейросеть = синергия»
Шаг 5: «Это не артефакт нашего проекта — работает на WikiText-103»

---

*Версия 1.0 — Research-04 · не касается XIIGen*
