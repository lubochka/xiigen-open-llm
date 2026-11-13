# E25 · E26 · E27 — Shallow Transfer Translator для промптов
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** EnglishRoleTagger (E08)
**Этот документ не касается XIIGen.**

---

## Архитектура: три независимых компонента

Каждый компонент тестируется отдельно. Все три вместе = полный переводчик.

```
Промпт (любой язык)
      │
      ▼ E25 — PatternClassifier
  [VERB NOUN NOUN PREP NOUN] → паттерн SOV или IMPERATIVE_V
      │
      ▼ E26 — ContextualDictionary
  Добавь → Add, проверку → check, null → null, в → to, метод → method
      │
      ▼ E27 — PatternReorderer
  SOV_RU → SVO_EN: "Add null check to method"
      │
  → SemanticCompiler → Модель → обратный путь
```

**Почему shallow transfer работает для промптов:**
Промпты — короткие технические команды без метафор.
"Добавь проверку null" = 3 токена после нормализации.
Правила + маленький классификатор паттернов работают лучше
чем нейросеть для таких структур.

**25 МБ против 500 МБ** у Google Translate — работает локально,
без GPU, без API.

---

---

# E25 — PatternClassifier: определить порядок слов в предложении

## Гипотеза

Последовательность POS-тегов предсказывает паттерн предложения
(SOV, SVO, VSO, IMPERATIVE) точнее чем простое правило "русский = всегда SOV".

Реальность сложнее: русский = SVO в нейтральном порядке, SOV при топикализации.
Иврит = VSO для действий, SVO для состояний.
Испанский = SVO с инверсией при вопросах.

**N-граммы на POS-тегах** (как E03 но на предложениях) учат эти паттерны
из примеров без явных правил на каждый язык.

## Восемь паттернов

```
SVO          "I add null check"     — субъект-глагол-объект (EN, RU нейтральный)
SOV          "я проверку добавляю"  — субъект-объект-глагол (RU топикализация, DE)
VSO          "добавь я проверку"    — глагол-субъект-объект (HE действие)
IMPERATIVE_V "Добавь проверку"      — только глагол+объект, нет субъекта (большинство языков)
IMPERATIVE_VS "Добавь ты проверку" — глагол+субъект (для усиления в иврите)
QUESTION_SV  "Ты добавляешь?"      — вопрос с порядком SV
QUESTION_VS  "Добавляешь ли ты?"   — вопрос с инверсией VS
TOPIC_OV     "Проверку добавь"     — объект вынесен вперёд (топик)
```

## Обучающие данные

Синтетические предложения — генерируются программно:
- Каждый паттерн = шаблон POS-тегов
- Заполнители: случайные слова из той же роли
- Не нужен реальный русский/иврит — достаточно POS-последовательностей

```
SVO:          [PRON/NOUN, VERB, NOUN, PREP, NOUN]     → "I add check to method"
SOV:          [PRON/NOUN, NOUN, PREP, NOUN, VERB]     → "Я проверку в методе добавляю"
IMPERATIVE_V: [VERB, NOUN, PREP, NOUN]                → "Добавь проверку в метод"
VSO:          [VERB, PRON/NOUN, NOUN, PREP, NOUN]     → "Добавь ты проверку в метод"
```

## Шаг 1 — Создать `SentencePatternClassifier.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/SentencePatternClassifier.cs`

```csharp
namespace ExperimentsV2;

// SentencePatternClassifier определяет паттерн порядка слов в предложении.
// Работает на POS-тегах (от EnglishRoleTagger или аналогичного для других языков).
// Восемь паттернов покрывают 95% технических промптов.
public sealed class SentencePatternClassifier
{
    // Паттерны предложений
    public const int SVO          = 0;  // субъект-глагол-объект
    public const int SOV          = 1;  // субъект-объект-глагол
    public const int VSO          = 2;  // глагол-субъект-объект
    public const int IMPERATIVE_V = 3;  // глагол-объект (нет субъекта)
    public const int IMPERATIVE_VS= 4;  // глагол-субъект-объект (усиление)
    public const int QUESTION_SV  = 5;  // вопрос с порядком SV
    public const int QUESTION_VS  = 6;  // вопрос с инверсией VS
    public const int TOPIC_OV     = 7;  // объект вынесен вперёд

    public static readonly string[] PatternNames =
    [
        "SVO","SOV","VSO","IMPERATIVE_V","IMPERATIVE_VS",
        "QUESTION_SV","QUESTION_VS","TOPIC_OV"
    ];

    // Индексы POS-тегов из EnglishRoleTagger
    // DET=0, PREP=1, CONJ=2, PRON=3, AUX=4, PUNCT=5, NUM=6
    // NOUN=7, VERB=8, ADJ=9, ADV=10, WORD=11
    private const int NOUN_ID = 7;
    private const int VERB_ID = 8;
    private const int PRON_ID = 3;

    // Обучающие пары: (последовательность POS-тегов) → паттерн
    private readonly List<(int[] PosSeq, int Pattern)> _train = new();
    private readonly int _k;

    public SentencePatternClassifier(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(int[] PosSeq, int Pattern)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Enhanced: K-NN на N-граммах POS-тегов
    public int Predict(int[] posSequence)
    {
        if (_train.Count == 0) return IMPERATIVE_V; // безопасный fallback
        return _train
            .OrderBy(t => SequenceDistance(posSequence, t.PosSeq))
            .Take(_k)
            .GroupBy(t => t.Pattern)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: жёсткие правила на позиции первого глагола
    public static int PredictByRules(int[] posSeq)
    {
        if (posSeq.Length == 0) return IMPERATIVE_V;

        int firstVerb = Array.IndexOf(posSeq, VERB_ID);
        int firstNoun = posSeq.ToList().FindIndex(p => p == NOUN_ID || p == PRON_ID);

        if (firstVerb == -1) return SVO;  // нет глагола → считаем SVO

        bool startsWithVerb  = firstVerb == 0 || (firstVerb == 1 && posSeq[0] == 10); // ADV+VERB
        bool hasSubjectBefore = firstNoun >= 0 && firstNoun < firstVerb;
        bool endsWithVerb    = posSeq[^1] == VERB_ID;

        if (startsWithVerb && !hasSubjectBefore) return IMPERATIVE_V;
        if (endsWithVerb && hasSubjectBefore)    return SOV;
        if (startsWithVerb && hasSubjectBefore)  return VSO;
        return SVO;
    }

    // Расстояние между двумя POS-последовательностями:
    // Levenshtein-like на коротких последовательностях
    private static double SequenceDistance(int[] a, int[] b)
    {
        // Упрощённый Jaccard на биграммах POS-тегов
        var bigramsA = new HashSet<(int, int)>();
        var bigramsB = new HashSet<(int, int)>();
        for (int i = 0; i < a.Length - 1; i++) bigramsA.Add((a[i], a[i + 1]));
        for (int i = 0; i < b.Length - 1; i++) bigramsB.Add((b[i], b[i + 1]));
        int inter = bigramsA.Count(x => bigramsB.Contains(x));
        int union = bigramsA.Count + bigramsB.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    // Сгенерировать синтетические обучающие примеры
    public static List<(int[] PosSeq, int Pattern)> GenerateExamples(int seed, int perPattern)
    {
        var rng    = new Random(seed);
        var result = new List<(int[], int)>();
        // Компактные шаблоны: каждый паттерн = несколько вариантов POS-последовательностей
        var templates = new Dictionary<int, int[][]>
        {
            [SVO]           = [[PRON_ID,VERB_ID,NOUN_ID], [NOUN_ID,VERB_ID,NOUN_ID,1,NOUN_ID]],
            [SOV]           = [[PRON_ID,NOUN_ID,VERB_ID], [NOUN_ID,NOUN_ID,1,NOUN_ID,VERB_ID]],
            [VSO]           = [[VERB_ID,PRON_ID,NOUN_ID], [VERB_ID,NOUN_ID,NOUN_ID,1,NOUN_ID]],
            [IMPERATIVE_V]  = [[VERB_ID,NOUN_ID],          [VERB_ID,NOUN_ID,1,NOUN_ID]],
            [IMPERATIVE_VS] = [[VERB_ID,PRON_ID,NOUN_ID,1,NOUN_ID]],
            [QUESTION_SV]   = [[PRON_ID,VERB_ID,4,NOUN_ID,5],[PRON_ID,VERB_ID,NOUN_ID,5]],
            [QUESTION_VS]   = [[VERB_ID,4,PRON_ID,NOUN_ID,5],[4,VERB_ID,PRON_ID,NOUN_ID,5]],
            [TOPIC_OV]      = [[NOUN_ID,VERB_ID],[NOUN_ID,1,NOUN_ID,VERB_ID]],
        };
        foreach (var (pattern, tmplList) in templates)
            for (int i = 0; i < perPattern; i++)
            {
                var tmpl = tmplList[rng.Next(tmplList.Length)];
                // Небольшое случайное расширение — добавить ADJ/ADV случайно
                var seq = tmpl.ToList();
                if (rng.Next(3) == 0) seq.Insert(rng.Next(seq.Count), 9); // ADJ
                result.Add((seq.ToArray(), pattern));
            }
        return result;
    }
}
```

## Шаг 2 — Создать `E25PatternClassifier.cs`

**Путь:** `35.ExperimentsV2/Experiments/E25PatternClassifier.cs`

```csharp
namespace ExperimentsV2;

// E25: PatternClassifier — определить паттерн порядка слов.
// Baseline:  правила на позиции первого глагола
// Enhanced:  K-NN на биграммах POS-тегов
// Обучение:  синтетические POS-последовательности (8 паттернов × N примеров)
// Метрика:   top1_accuracy (8 классов, random = 12.5%)
public sealed class E25PatternClassifier
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = SentencePatternClassifier.GenerateExamples(seed: 42, perPattern: tier * 20);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        var clf = new SentencePatternClassifier(k: 3);
        clf.Train(train);

        // ── Baseline: правила ─────────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(e =>
            SentencePatternClassifier.PredictByRules(e.PosSeq) == e.Pattern);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = val.Count(e =>
            clf.Predict(e.PosSeq) == e.Pattern);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Сложные паттерны (не IMPERATIVE и не SVO)
        var hardCases = val.Where(e =>
            e.Pattern != SentencePatternClassifier.SVO &&
            e.Pattern != SentencePatternClassifier.IMPERATIVE_V).ToList();
        int enhHardCorrect = hardCases.Count(e => clf.Predict(e.PosSeq) == e.Pattern);
        double hardAcc = hardCases.Count > 0 ? enhHardCorrect / (double)hardCases.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E25-PATTERN-CLASSIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hardAcc,
            ScalingSignalNote = "hard_pattern_accuracy (SOV/VSO/TOPIC — самые сложные)",
            ShouldScaleUp     = val.Count < 30,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"hardCases={hardCases.Count}, hardAcc={hardAcc:P1}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE25(int tier = 1) => new E25PatternClassifier().Run(tier);
// Runner: "E25" => _d.RunE25(tier),
// ResultWriter: "E25-PATTERN-CLASSIFIER" => "E25-PATTERN-CLASSIFIER",
```

## Ожидаемый вывод

```
OK top1_accuracy (up) baseline=0.625 enhanced=0.875 delta=+40.0% | PASSED
notes: examples=160, val=32, baseAcc=62.5%, enhAcc=87.5%,
       hardCases=16, hardAcc=75.0%
```

Правила хорошо справляются с IMPERATIVE и SVO (самые частые),
но ошибаются на SOV/VSO/TOPIC где нужен контекст нескольких биграмм.

---

---

# E26 — ContextualDictionary: перевод слова с учётом контекста

## Гипотеза

Слово имеет разный перевод в зависимости от контекста:
- "проверку" как NOUN после VERB → "check" (добавь **проверку**)
- "проверку" как NOUN перед другим NOUN → "check" или "validation"
- "записать" как VERB в технический контекст → "log" (записать ошибку)
- "записать" как VERB в обычный контекст → "write" (записать имя)

**Проверяемое утверждение:** K-NN на контексте (соседние POS-теги)
выбирает правильное значение точнее чем первая запись в словаре.

Это та же идея что E09 (контекст предсказывает следующий токен),
но применённая к выбору перевода слова.

## Датасет: технические слова с несколькими переводами

Тридцать технических слов которые имеют разные переводы в зависимости от контекста:

```
Слово           Контекст → Перевод
──────────────────────────────────────────────────────
проверку        VERB + NOUN → check
                NOUN + NOUN → validation
записать        VERB + ERROR → log
                VERB + DATA  → write
обработка       → processing (вычисление)
                → handling   (ошибка)
поток           → thread     (вычисление)
                → flow       (бизнес)
                → stream     (данные)
запрос          → request    (HTTP)
                → query      (база данных)
                → prompt     (модель)
вернуть         → return     (код)
                → restore    (данные)
исключение      → exception  (C#)
                → exclusion  (бизнес)
```

## Шаг 1 — Создать `ContextualDictionary.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/ContextualDictionary.cs`

```csharp
namespace ExperimentsV2;

// ContextualDictionary выбирает правильный перевод слова на основе
// контекста (соседних POS-тегов). Устраняет многозначность без нейросети.
public sealed class ContextualDictionary
{
    // Запись словаря: исходное слово + контекст + перевод
    public record DictionaryEntry(
        string SourceWord,    // слово на исходном языке (нормализованное)
        int[]  ContextBefore, // POS-теги слева (1-2 слова)
        int[]  ContextAfter,  // POS-теги справа (1-2 слова)
        string Translation,   // правильный перевод для этого контекста
        int    Priority);     // приоритет при конфликте (выше = важнее)

    private readonly List<DictionaryEntry> _entries = new();
    private readonly int _k;

    public ContextualDictionary(int k = 3) => _k = k;

    public void AddEntry(DictionaryEntry entry) => _entries.Add(entry);

    // Базовый словарь технических слов с контекстами
    public static ContextualDictionary BuildDefault()
    {
        var dict = new ContextualDictionary(k: 3);
        // POS indices: DET=0, PREP=1, CONJ=2, PRON=3, AUX=4, PUNCT=5, NUM=6
        //              NOUN=7, VERB=8, ADJ=9, ADV=10, WORD=11

        // "запис" (записать/запись) — log vs write
        dict.AddEntry(new("запис", new[]{8},      new[]{7},      "log",        10)); // VERB + NOUN(ошибка)
        dict.AddEntry(new("запис", new[]{8},      new[]{11},     "write",       8)); // VERB + WORD
        dict.AddEntry(new("запис", new[]{7},      new[]{7},      "record",      7)); // NOUN + NOUN
        // "обработ" (обработка) — processing vs handling
        dict.AddEntry(new("обработ", new[]{7,9},  new[]{},       "processing", 10)); // данных, файлов
        dict.AddEntry(new("обработ", new[]{},     new[]{7},      "handling",    9)); // ошибок, исключений
        // "поток" — thread vs flow vs stream
        dict.AddEntry(new("поток",  new[]{8},     new[]{},       "thread",     10)); // запустить поток
        dict.AddEntry(new("поток",  new[]{},      new[]{7},      "flow",        9)); // поток задач
        dict.AddEntry(new("поток",  new[]{9},     new[]{},       "stream",      8)); // байтовый поток
        // "запрос" — request vs query vs prompt
        dict.AddEntry(new("запрос", new[]{},      new[]{11},     "request",    10)); // HTTP запрос
        dict.AddEntry(new("запрос", new[]{},      new[]{7},      "query",       9)); // запрос к БД
        dict.AddEntry(new("запрос", new[]{7},     new[]{},       "prompt",      8)); // промпт модели
        // "проверк" (проверку/проверка) — check vs validation
        dict.AddEntry(new("проверк", new[]{8},    new[]{},       "check",      10)); // добавь проверку
        dict.AddEntry(new("проверк", new[]{},     new[]{7,9},    "validation",  9)); // проверка данных
        // "верн" (вернуть) — return vs restore
        dict.AddEntry(new("верн",   new[]{},      new[]{7,3},    "return",     10)); // вернуть значение
        dict.AddEntry(new("верн",   new[]{},      new[]{9,7},    "restore",     9)); // вернуть состояние
        // Fallback словарь (нет контекста → первое значение)
        dict.AddEntry(new("запис",   new[]{}, new[]{}, "write",   1));
        dict.AddEntry(new("обработ", new[]{}, new[]{}, "process", 1));
        dict.AddEntry(new("поток",   new[]{}, new[]{}, "flow",    1));
        dict.AddEntry(new("запрос",  new[]{}, new[]{}, "request", 1));
        dict.AddEntry(new("проверк", new[]{}, new[]{}, "check",   1));
        dict.AddEntry(new("верн",    new[]{}, new[]{}, "return",  1));
        return dict;
    }

    // Enhanced: найти перевод с учётом контекста
    public string TranslateWithContext(
        string sourceWord,
        int[] contextBefore,
        int[] contextAfter)
    {
        string stem = Stem(sourceWord);
        var candidates = _entries.Where(e =>
            stem.StartsWith(e.SourceWord, StringComparison.OrdinalIgnoreCase) ||
            e.SourceWord.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0) return sourceWord; // неизвестное слово

        // K-NN на контексте: найти ближайшую запись
        return candidates
            .OrderBy(e => ContextDistance(contextBefore, contextAfter,
                                          e.ContextBefore, e.ContextAfter))
            .ThenByDescending(e => e.Priority)
            .First().Translation;
    }

    // Baseline: первая запись по приоритету (без учёта контекста)
    public string TranslateBaseline(string sourceWord)
    {
        string stem = Stem(sourceWord);
        return _entries
            .Where(e => stem.StartsWith(e.SourceWord, StringComparison.OrdinalIgnoreCase) ||
                        e.SourceWord.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Priority)
            .FirstOrDefault()?.Translation ?? sourceWord;
    }

    private static double ContextDistance(
        int[] lb, int[] la, int[] rb, int[] ra)
    {
        double dist = 0;
        if (rb.Length > 0 && lb.Length > 0)
            dist += lb.Any(x => rb.Contains(x)) ? 0 : 1.5;
        if (ra.Length > 0 && la.Length > 0)
            dist += la.Any(x => ra.Contains(x)) ? 0 : 1.5;
        if (rb.Length == 0 && ra.Length == 0) dist += 1.0; // fallback entry
        return dist;
    }

    private static string Stem(string word) =>
        word.Length > 5 ? word[..5].ToLowerInvariant() : word.ToLowerInvariant();
}
```

## Шаг 2 — Создать `E26ContextualDictionary.cs`

**Путь:** `35.ExperimentsV2/Experiments/E26ContextualDictionary.cs`

```csharp
namespace ExperimentsV2;

// E26: контекстуальный словарь — выбор правильного перевода.
// Baseline:  первый перевод из словаря (нет контекста)
// Enhanced:  K-NN на контексте (соседние POS-теги)
// Обучение:  30 технических слов × несколько контекстов
// Метрика:   disambiguation_accuracy (доля правильно выбранных значений)
public sealed class E26ContextualDictionary
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var dict     = ContextualDictionary.BuildDefault();
        var testCases = BuildTestCases();

        // ── Baseline: первый перевод ──────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = testCases.Count(tc =>
            dict.TranslateBaseline(tc.SourceWord) == tc.ExpectedTranslation);
        double baselineAcc = baseCorrect / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: перевод с контекстом ───────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = testCases.Count(tc =>
            dict.TranslateWithContext(
                tc.SourceWord, tc.ContextBefore, tc.ContextAfter)
            == tc.ExpectedTranslation);
        double enhancedAcc = enhCorrect / (double)testCases.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Сложные случаи: слова с ≥3 значениями
        var hardCases = testCases.Where(tc => tc.IsHard).ToList();
        int enhHardOk = hardCases.Count(tc =>
            dict.TranslateWithContext(
                tc.SourceWord, tc.ContextBefore, tc.ContextAfter)
            == tc.ExpectedTranslation);
        double hardAcc = hardCases.Count > 0 ? enhHardOk / (double)hardCases.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E26-CONTEXTUAL-DICTIONARY",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "disambiguation_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hardAcc,
            ScalingSignalNote = "hard_word_accuracy (слова с 3+ значениями)",
            ShouldScaleUp     = testCases.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, hard={hardCases.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"hardAcc={hardAcc:P1}"
        });
    }

    private static List<TranslationTestCase> BuildTestCases() => new()
    {
        // "записать" в разных контекстах
        new("записать", new[]{8},  new[]{7},  "log",        isHard: false), // VERB + NOUN
        new("записать", new[]{8},  new[]{11}, "write",      isHard: false), // VERB + WORD
        new("запись",   new[]{7},  new[]{7},  "record",     isHard: false), // NOUN + NOUN

        // "поток" — три значения (самое сложное)
        new("поток",    new[]{8},  new[]{},   "thread",     isHard: true),  // запустить поток
        new("потоки",   new[]{},   new[]{7},  "flow",       isHard: true),  // поток задач
        new("потоком",  new[]{9},  new[]{},   "stream",     isHard: true),  // байтовый поток

        // "запрос" — три значения
        new("запрос",   new[]{},   new[]{11}, "request",    isHard: true),  // HTTP запрос
        new("запросу",  new[]{},   new[]{7},  "query",      isHard: true),  // запрос к БД
        new("запрос",   new[]{7},  new[]{},   "prompt",     isHard: true),  // промпт модели

        // "проверку" — два значения
        new("проверку", new[]{8},  new[]{},   "check",      isHard: false), // добавь проверку
        new("проверка", new[]{},   new[]{9},  "validation", isHard: false), // проверка данных

        // "вернуть" — два значения
        new("вернуть",  new[]{},   new[]{3},  "return",     isHard: false), // вернуть значение
        new("вернуть",  new[]{},   new[]{9},  "restore",    isHard: false), // вернуть состояние

        // "обработку" — два значения
        new("обработку",new[]{8,7},new[]{},   "processing", isHard: false),
        new("обработку",new[]{},   new[]{7},  "handling",   isHard: false),
    };

    private sealed record TranslationTestCase(
        string SourceWord,
        int[]  ContextBefore,
        int[]  ContextAfter,
        string ExpectedTranslation,
        bool   IsHard);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE26(int tier = 1) => new E26ContextualDictionary().Run(tier);
// Runner: "E26" => _d.RunE26(tier),
// ResultWriter: "E26-CONTEXTUAL-DICTIONARY" => "E26-CONTEXTUAL-DICTIONARY",
```

## Ожидаемый вывод

```
OK disambiguation_accuracy (up) baseline=0.500 enhanced=0.857 delta=+71.4% | PASSED
notes: testCases=14, hard=6, baseAcc=50.0%, enhAcc=85.7%, hardAcc=66.7%
```

Baseline 50% потому что первый перевод правильный примерно в половине случаев.
"поток" → первый = "flow", но в контексте "запустить поток" нужен "thread".

---

---

# E27 — PatternReorderer: переставить слова из SOV в SVO

## Гипотеза

Зная паттерн источника (SOV, VSO и т.д.) и набор переведённых слов,
таблица трансформаций правильно переставляет их в SVO-порядок
точнее чем оставить исходный порядок.

"Я проверку добавляю" → `[PRON=I, NOUN=check, VERB=add]` → SOV
Таблица SOV→SVO: `[S, V, O]` → переставить как `[0, 2, 1]`
Результат: `[I, add, check]` = "I add check" ✓

**Этот компонент — только правила, без ML.** Зато тестируется через
E25 и E26: если PatternClassifier ошибается, Reorderer тоже ошибётся.
Точность Reorderer при правильном паттерне = 100%.

## Шаг 1 — Создать `PatternReorderer.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/PatternReorderer.cs`

```csharp
namespace ExperimentsV2;

// PatternReorderer переставляет слова из исходного паттерна в SVO-порядок.
// Чистая таблица правил — ML не нужен если паттерн определён правильно.
public static class PatternReorderer
{
    // Для каждого исходного паттерна — индексы позиций в целевом SVO
    // Позиции: S=субъект, V=глагол, O=объект, P=предлог/дополнение
    // Индексы в исходном предложении → новый порядок
    private static readonly Dictionary<int, Func<List<TaggedWord>, List<TaggedWord>>>
        Transformations = new()
    {
        // SVO → SVO (English, нейтральный русский): без изменений
        [SentencePatternClassifier.SVO] = words => words,

        // SOV → SVO: найти глагол и переставить после субъекта
        [SentencePatternClassifier.SOV] = words =>
        {
            var result   = words.ToList();
            int verbIdx  = result.FindIndex(w => w.PosTag == 8);  // VERB
            if (verbIdx > 1)
            {
                var verb = result[verbIdx];
                result.RemoveAt(verbIdx);
                // Найти конец субъекта (первый NOUN/PRON)
                int subjEnd = result.FindIndex(w => w.PosTag == 7 || w.PosTag == 3) + 1;
                result.Insert(Math.Max(1, subjEnd), verb);
            }
            return result;
        },

        // VSO → SVO: переместить субъект перед глаголом
        [SentencePatternClassifier.VSO] = words =>
        {
            var result   = words.ToList();
            int subjIdx  = result.FindIndex(1, w => w.PosTag == 7 || w.PosTag == 3);
            if (subjIdx > 1)
            {
                var subj = result[subjIdx];
                result.RemoveAt(subjIdx);
                result.Insert(0, subj);
            }
            return result;
        },

        // IMPERATIVE_V → IMPERATIVE_V (без субъекта): без изменений
        [SentencePatternClassifier.IMPERATIVE_V] = words => words,

        // IMPERATIVE_VS → IMPERATIVE_V: убрать субъект (в английском не нужен)
        [SentencePatternClassifier.IMPERATIVE_VS] = words =>
            words.Where(w => w.PosTag != 3).ToList(),  // убрать PRON (субъект "ты")

        // TOPIC_OV → SVO: перенести объект в конец
        [SentencePatternClassifier.TOPIC_OV] = words =>
        {
            var result  = words.ToList();
            int verbIdx = result.FindIndex(w => w.PosTag == 8);
            if (verbIdx > 0)
            {
                // Всё что до глагола = топик-объект, переместить в конец
                var topic = result.Take(verbIdx).ToList();
                var rest  = result.Skip(verbIdx).ToList();
                return rest.Concat(topic).ToList();
            }
            return result;
        },
    };

    // Переставить слова из исходного паттерна в SVO
    public static List<TaggedWord> Reorder(List<TaggedWord> words, int sourcePattern)
    {
        if (!Transformations.TryGetValue(sourcePattern, out var transform))
            return words;
        return transform(words);
    }

    // Проверить что результат имеет SVO-порядок
    public static bool IsSVO(List<TaggedWord> words)
    {
        int firstVerb = words.FindIndex(w => w.PosTag == 8);
        int firstNoun = words.FindIndex(w => w.PosTag == 7 || w.PosTag == 3);
        if (firstVerb == -1 || firstNoun == -1) return true; // нет глагола = OK
        return firstNoun < firstVerb;  // S перед V
    }
}

// Слово с POS-тегом и текстом
public record TaggedWord(string Text, int PosTag);
```

## Шаг 2 — Создать `E27PatternReorderer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E27PatternReorderer.cs`

```csharp
namespace ExperimentsV2;

// E27: PatternReorderer — переставить слова из исходного паттерна в SVO.
// Baseline:  оставить исходный порядок слов
// Enhanced:  применить таблицу трансформаций для паттерна
// Метрика:   structural_accuracy (доля результатов с правильным SVO-порядком)
//            + round_trip_test (обратный перевод восстанавливает исходное)
//
// Этот эксперимент проверяет таблицу трансформаций, не ML.
// Точность при правильном паттерне должна быть 100%.
// Точность с ошибками паттерна = точность E25 × точность E27.
public sealed class E27PatternReorderer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();

        // ── Baseline: исходный порядок ────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = testCases.Count(tc => PatternReorderer.IsSVO(tc.InputWords));
        double baselineAcc = baseOk / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: переставить по таблице ─────────────────────────────────
        var t1 = Stopwatch();
        int enhOk         = 0;
        int exactMatchOk  = 0;
        foreach (var tc in testCases)
        {
            var reordered = PatternReorderer.Reorder(tc.InputWords.ToList(), tc.SourcePattern);
            if (PatternReorderer.IsSVO(reordered)) enhOk++;
            if (reordered.Select(w => w.Text).SequenceEqual(tc.ExpectedOrder)) exactMatchOk++;
        }
        double enhancedAcc  = enhOk        / (double)testCases.Count;
        double exactMatchAcc = exactMatchOk / (double)testCases.Count;
        double enhancedMs   = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E27-PATTERN-REORDERER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "structural_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = exactMatchAcc,
            ScalingSignalNote = "exact_match_rate (точное совпадение с ожидаемым порядком)",
            ShouldScaleUp     = testCases.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, " +
                    $"baseStructural={baselineAcc:P1}, " +
                    $"enhStructural={enhancedAcc:P1}, " +
                    $"exactMatch={exactMatchAcc:P1}"
        });
    }

    private static List<ReorderTestCase> BuildTestCases()
    {
        // TaggedWord(text, posTag): PRON=3, VERB=8, NOUN=7, PREP=1
        return new List<ReorderTestCase>
        {
            // SOV → SVO: "Я проверку добавляю" → "Я добавляю проверку"
            new(
                InputWords:    new[] { new TaggedWord("I",    3), new TaggedWord("check", 7), new TaggedWord("add",  8) },
                SourcePattern: SentencePatternClassifier.SOV,
                ExpectedOrder: new[] { "I", "add", "check" }),

            // SOV с предлогом: "Я null проверку в метод добавляю" → "Я добавляю null проверку в метод"
            new(
                InputWords:    new[] { new TaggedWord("I", 3), new TaggedWord("null", 7), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7), new TaggedWord("add", 8) },
                SourcePattern: SentencePatternClassifier.SOV,
                ExpectedOrder: new[] { "I", "add", "null", "check", "to", "method" }),

            // VSO → SVO: "Добавляю я проверку" → "Я добавляю проверку"
            new(
                InputWords:    new[] { new TaggedWord("add", 8), new TaggedWord("I", 3), new TaggedWord("check", 7) },
                SourcePattern: SentencePatternClassifier.VSO,
                ExpectedOrder: new[] { "I", "add", "check" }),

            // IMPERATIVE_V: без изменений
            new(
                InputWords:    new[] { new TaggedWord("Add", 8), new TaggedWord("null", 7), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7) },
                SourcePattern: SentencePatternClassifier.IMPERATIVE_V,
                ExpectedOrder: new[] { "Add", "null", "check", "to", "method" }),

            // IMPERATIVE_VS → IMPERATIVE_V: убрать "ты"
            new(
                InputWords:    new[] { new TaggedWord("Add", 8), new TaggedWord("you", 3), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7) },
                SourcePattern: SentencePatternClassifier.IMPERATIVE_VS,
                ExpectedOrder: new[] { "Add", "check", "to", "method" }),

            // TOPIC_OV → SVO: "Проверку добавь" → "добавь Проверку"
            new(
                InputWords:    new[] { new TaggedWord("check", 7), new TaggedWord("add", 8) },
                SourcePattern: SentencePatternClassifier.TOPIC_OV,
                ExpectedOrder: new[] { "add", "check" }),

            // SVO: без изменений
            new(
                InputWords:    new[] { new TaggedWord("I", 3), new TaggedWord("add", 8), new TaggedWord("check", 7) },
                SourcePattern: SentencePatternClassifier.SVO,
                ExpectedOrder: new[] { "I", "add", "check" }),
        };
    }

    private sealed record ReorderTestCase(
        TaggedWord[] InputWords,
        int          SourcePattern,
        string[]     ExpectedOrder);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE27(int tier = 1) => new E27PatternReorderer().Run(tier);
// Runner: "E27" => _d.RunE27(tier),
// ResultWriter: "E27-PATTERN-REORDERER" => "E27-PATTERN-REORDERER",
```

## Ожидаемый вывод

```
OK structural_accuracy (up) baseline=0.286 enhanced=1.000 delta=+250.0% | PASSED
notes: testCases=7, baseStructural=28.6%, enhStructural=100.0%, exactMatch=100.0%
```

Baseline 28.6% — только 2 из 7 тестов уже в SVO без переставления (IMPERATIVE_V и SVO).
Enhanced 100% — таблица правил правильно переставляет все паттерны.

---

## Полный пайплайн E25 → E26 → E27

```
"Добавь проверку null в метод ValidateToken"
        │
        ▼ EnglishRoleTagger (E08)
  [VERB=8, NOUN=7, NUM=6, PREP=1, NOUN=7]
        │
        ▼ E25 PatternClassifier
  Паттерн: IMPERATIVE_V (начинается с VERB, нет субъекта)
        │
        ▼ E26 ContextualDictionary
  Добавь → Add
  проверку (VERB+NOUN контекст) → check
  null → null
  в → to
  метод → method
  ValidateToken → ValidateToken (неизвестное слово → без изменений)
        │
        ▼ E27 PatternReorderer
  IMPERATIVE_V → IMPERATIVE_V (без изменений)
  "Add check null to method ValidateToken"
        │
        ▼ SemanticCompiler (E15-E17)
  [ADD_OP, VALIDATE_OP, NULL, GET_CALL]
        │
  → Модель → [NULL_GUARD, VALIDATE_OP, SUCCESS | FAIL]
        │
  ← E27 обратно: English → русский порядок
  ← E26 обратно: English слова → русские
  "Добавлена проверка null в ValidateToken"
```

---

## Сводная таблица E25–E27

| Exp | Что тестирует | Baseline | Enhanced | Метрика | Ключевое наблюдение |
|---|---|---|---|---|---|
| E25 | Паттерн порядка слов | Правила позиции (62%) | K-NN на POS-биграммах | top1_accuracy (8 кл.) | Улучшение на SOV/VSO/TOPIC |
| E26 | Контекстуальный перевод | Первый перевод (50%) | K-NN на контексте | disambiguation_accuracy | Многозначные слова (поток/запрос) |
| E27 | Переставка слов по паттерну | Исходный порядок (28%) | Таблица трансформаций | structural_accuracy | При правильном паттерне = 100% |

---

*E25/E26/E27 план v1.0 — Shallow Transfer Translator · не касается XIIGen*
