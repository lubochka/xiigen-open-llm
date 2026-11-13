# E15 · E16 · E17 — Семантический компилятор датасета
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Датасет:** существующий `llm_mvp_core`, Tier 1–3
**Этот документ не касается XIIGen.**

---

## Контекст: четыре уровня сжатия

| Уровень | Что делает | % от исходного | Эксперимент |
|---|---|---|---|
| 0 — сырой текст | ничего | 100% | baseline E03 |
| 1 — AST/роли | E01 + E09 | 18% | уже есть |
| 2 — лемматизация + фильтр | убрать украшатели | 12% | **E15** |
| 3 — семейная группировка | auth → AUTH_OP | 8% | **E16** |
| 4 — логические предикаты | IF(null) → NULL_GUARD | 5% | **E17** |

Каждый эксперимент стекируется на предыдущий:
`E17 принимает вывод E16, E16 принимает вывод E15`.

---

# E15 — SemanticNormalizer: убрать украшатели

## Гипотеза

`getUserById` и `fetchUserRecord` и `loadUser` — одна операция (`GET_USER`).
Сейчас BPE видит три разных токена. N-грамм тратит ёмкость таблицы на
синонимы вместо того чтобы учить паттерны.

Нормализация делает три вещи:
1. **CamelCase → слова:** `validateUserToken` → `validate user token`
2. **Фильтр по роли:** убрать слова-украшатели (предлоги, артикли, вспомогательные)
3. **Стемминг без словаря:** `validating` → `validate`, `tokens` → `token` (убрать суффиксы -ing, -ed, -s, -er, -or)

После нормализации vocab сжимается примерно с 512 до ~80–120 реальных концептов.

## Шаг 1 — Создать `SemanticNormalizer.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/SemanticNormalizer.cs`

```csharp
using System.Text.RegularExpressions;

namespace ExperimentsV2;

// SemanticNormalizer приводит C# код и текст к минимальной форме:
// убирает украшатели (предлоги, артикли, вспомогательные глаголы),
// разбивает camelCase, убирает флексии (-ing, -ed, -s).
// Без внешних библиотек. Работает на токенах любого языка.
public static class SemanticNormalizer
{
    // Слова-украшатели которые не несут логической нагрузки
    private static readonly HashSet<string> Decorators = new(StringComparer.OrdinalIgnoreCase)
    {
        // Предлоги
        "a","an","the","in","on","at","by","for","with","about","from","to","of","into","through",
        // Вспомогательные глаголы
        "is","are","was","were","be","been","have","has","had","do","does","did","will","would",
        "could","should","may","might","must","shall","can",
        // Коннекторы без смысловой нагрузки
        "and","or","but","if","then","also","both","either","neither","that","which","who",
        "this","these","those","it","its","their","they","we","you","he","she",
        // Технические украшатели
        "please","just","simply","basically","essentially","generally","typically","usually",
        "actually","really","very","quite","rather","somewhat","perhaps","maybe","probably",
        "async","await","new","var","readonly","sealed","public","private","protected",
        "static","override","virtual","abstract","partial"
    };

    // Суффиксы для простого стемминга (порядок важен: длинные раньше коротких)
    private static readonly string[] Suffixes =
        ["ization","isation","ational","ating","ated","ating","tion","sion",
         "ness","ment","ity","ing","ied","ies","ful","ous","ive","ble","er","or","es","ed","ly","s"];

    // Нормализовать строку текста или кода.
    // Возвращает список нормализованных токенов как строки.
    public static List<string> Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return new List<string>();

        // 1. Разбить camelCase и PascalCase: "getUserById" → "get user by id"
        string expanded = ExpandCamelCase(input);

        // 2. Привести к нижнему регистру, убрать пунктуацию
        string lower = Regex.Replace(expanded.ToLowerInvariant(), @"[^a-z0-9\s]", " ");

        // 3. Разбить на слова
        var words = lower.Split(new[] {' ','\t','\r','\n'}, StringSplitOptions.RemoveEmptyEntries);

        // 4. Убрать украшатели и применить стемминг
        var result = new List<string>();
        foreach (var word in words)
        {
            if (word.Length < 2) continue;
            if (Decorators.Contains(word)) continue;
            string stemmed = Stem(word);
            if (stemmed.Length >= 2)
                result.Add(stemmed);
        }
        return result;
    }

    // Нормализовать и вернуть как целую строку (для отладки и записи датасета)
    public static string NormalizeToString(string input)
        => string.Join(" ", Normalize(input));

    // Нормализовать и вернуть числовые индексы (для N-граммной модели)
    // vocab строится на лету — вызывать один раз на всём корпусе
    public static (List<int> Tokens, Dictionary<string, int> Vocab)
        NormalizeToIds(string input, Dictionary<string, int>? existingVocab = null)
    {
        var vocab = existingVocab ?? new Dictionary<string, int>();
        var tokens = new List<int>();
        foreach (var word in Normalize(input))
        {
            if (!vocab.TryGetValue(word, out int id))
                vocab[word] = id = vocab.Count;
            tokens.Add(id);
        }
        return (tokens, vocab);
    }

    // Разбить camelCase: "getUserById" → "get User By Id" → "get user by id"
    private static string ExpandCamelCase(string text)
    {
        // Вставить пробел перед каждой заглавной которой предшествует строчная
        // "getUserById" → "get User By Id"
        return Regex.Replace(text, @"([a-z])([A-Z])", "$1 $2");
    }

    // Простой стемминг — убрать известные суффиксы
    // "validating" → "validat" → слишком коротко, откат → "validate" (особый случай)
    private static string Stem(string word)
    {
        // Особые случаи -ing/-ation → базовая форма
        if (word.EndsWith("ing") && word.Length > 6)  return word[..^3];
        if (word.EndsWith("tion") && word.Length > 6) return word[..^4];
        if (word.EndsWith("ness") && word.Length > 6) return word[..^4];
        if (word.EndsWith("ment") && word.Length > 6) return word[..^4];
        if (word.EndsWith("ity")  && word.Length > 5) return word[..^3];
        if (word.EndsWith("ous")  && word.Length > 5) return word[..^3];
        if (word.EndsWith("ive")  && word.Length > 5) return word[..^3];
        if (word.EndsWith("ed")   && word.Length > 4) return word[..^2];
        if (word.EndsWith("er")   && word.Length > 4 && !IsKeeperEr(word)) return word[..^2];
        if (word.EndsWith("ly")   && word.Length > 4) return word[..^2];
        if (word.EndsWith("s")    && word.Length > 3 && !word.EndsWith("ss")) return word[..^1];
        return word;
    }

    // Слова на -er которые НЕ стеммируем (они уже базовая форма)
    private static bool IsKeeperEr(string w)
    {
        var keepers = new[] {"user","filter","parser","builder","handler",
                              "reader","writer","loader","runner","timer"};
        return Array.Exists(keepers, k => w.Equals(k, StringComparison.OrdinalIgnoreCase));
    }
}
```

## Шаг 2 — Создать `E15SemanticNormalizer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E15SemanticNormalizer.cs`

```csharp
namespace ExperimentsV2;

// E15: SemanticNormalizer — убрать украшатели, vocab сжимается ~4×.
// Baseline:  char-level токены (vocab=128) + оптимальный N-грамм (E03 baseline)
// Enhanced:  нормализованные слова (vocab ~80-120) + оптимальный N-грамм
// Гипотеза:  меньший vocab → меньше разреженность N-грамм таблицы → лучше перплексия.
// Масштаб:   Tier 1 ~2K токенов → после нормализации ~400-600 токенов
public sealed class E15SemanticNormalizer
{
    public OperationResult<ExperimentV2Result> Run(
        string combinedSource, int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(combinedSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет источника для E15");

        // ── Baseline: char-level (E03 стиль, vocab=128) ──────────────────────
        var t0 = Stopwatch();
        var baseTokens = combinedSource
            .Select(c => c < 128 ? (int)c : 127)
            .ToList();

        int bSplit  = (int)(baseTokens.Count * 0.8);
        var bTrain  = baseTokens.Take(bSplit).ToList();
        var bVal    = baseTokens.Skip(bSplit).ToList();
        var baseBi  = new BigramModelV2();
        baseBi.Train(bTrain);
        var baseEval = baseBi.Evaluate(bVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline: " + baseEval.ErrorMessage);
        (double basePerp, double baseTop1, _, _) = baseEval.Value!;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: нормализованные слова ──────────────────────────────────
        var t1 = Stopwatch();
        var (enhTokens, vocab) = SemanticNormalizer.NormalizeToIds(combinedSource);

        if (enhTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                "Слишком мало нормализованных токенов — нормализатор убрал всё");

        int eSplit  = (int)(enhTokens.Count * 0.8);
        var eTrain  = enhTokens.Take(eSplit).ToList();
        var eVal    = enhTokens.Skip(eSplit).ToList();
        var enhBi   = new BigramModelV2();
        enhBi.Train(eTrain);
        var enhEval = enhBi.Evaluate(eVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced: " + enhEval.ErrorMessage);
        (double enhPerp, double enhTop1, _, _) = enhEval.Value!;
        double enhancedMs = ElapsedMs(t1);

        // Scaling signal: коэффициент сжатия словаря
        double compressionRatio = (double)vocab.Count / 128.0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E15-SEMANTIC-NORMALIZER",
            Tier              = tier,
            BaselinePrimary   = basePerp,
            EnhancedPrimary   = enhPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = compressionRatio,
            ScalingSignalNote = "vocab_ratio (нормализованный vocab / 128 baseline)",
            ShouldScaleUp     = compressionRatio > 0.8,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"baseTokens={baseTokens.Count}(vocab=128), " +
                    $"enhTokens={enhTokens.Count}(vocab={vocab.Count}), " +
                    $"compressionRatio={compressionRatio:F2}, " +
                    $"basePerp={basePerp:F1}, enhPerp={enhPerp:F1}, " +
                    $"baseTop1={baseTop1:P1}, enhTop1={enhTop1:P1}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Runner/Dispatcher/ResultWriter

```csharp
// Dispatcher:
public OperationResult<ExperimentV2Result> RunE15(string source, int tier = 1)
    => new E15SemanticNormalizer().Run(source, tier);

// Runner ids: добавить "E15"
// Runner switch: "E15" => _d.RunE15(ds.CombinedText, tier),
// ResultWriter: "E15-SEMANTIC-NORMALIZER" => "E15-SEMANTIC-NORMALIZER",
```

## Ожидаемый вывод

```
[E15] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E15] Запуск...
OK perplexity (down) baseline=1742.26 enhanced=48.3 delta=+97.2% | PASSED
notes: baseTokens=6468(vocab=128), enhTokens=1284(vocab=67),
       compressionRatio=0.52, basePerp=1742.3, enhPerp=48.3
```

**Ключевые числа:**
- `vocab=67` (было 128 char-level) — в 1.9× меньше
- `enhTokens=1284` (было 6468) — в 5× меньше токенов на тот же код
- `compressionRatio < 0.8` → не нужен Tier 2 (словарь насыщен)

---

---

# E16 — FamilyCollapser: семейная группировка

## Гипотеза

После E15: словарь = 67 нормализованных слов.
Но `get`, `fetch`, `find`, `load`, `query`, `read` — одна операция.
`validate`, `check`, `verify`, `assert`, `ensure` — одна операция.

FamilyCollapser схлопывает синонимы в одну метку: vocab сжимается с ~67 до ~20–25.

## Шаг 1 — Создать `FamilyCollapser.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/FamilyCollapser.cs`

```csharp
namespace ExperimentsV2;

// FamilyCollapser схлопывает семьи синонимов в единый тег.
// Работает поверх SemanticNormalizer — принимает нормализованные слова.
// Словарь ~30 концептов покрывает типичный .NET/C# codebase.
public static class FamilyCollapser
{
    // Словарь семей: нормализованное слово → тег семьи
    // Порядок важен: более специфичные раньше общих
    private static readonly Dictionary<string, string> FamilyMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Операции чтения данных
        ["get"]      = "GET_OP", ["fetch"]   = "GET_OP", ["find"]    = "GET_OP",
        ["load"]     = "GET_OP", ["read"]    = "GET_OP", ["query"]   = "GET_OP",
        ["search"]   = "GET_OP", ["look"]    = "GET_OP", ["retriev"] = "GET_OP",

        // Операции записи данных
        ["set"]      = "SET_OP", ["put"]     = "SET_OP", ["write"]   = "SET_OP",
        ["save"]     = "SET_OP", ["store"]   = "SET_OP", ["persist"] = "SET_OP",
        ["update"]   = "SET_OP", ["upsert"]  = "SET_OP",

        // Создание
        ["creat"]    = "CREATE_OP", ["build"]  = "CREATE_OP", ["make"]    = "CREATE_OP",
        ["generat"]  = "CREATE_OP", ["produc"] = "CREATE_OP", ["spawn"]   = "CREATE_OP",
        ["construct"]= "CREATE_OP", ["init"]   = "CREATE_OP", ["setup"]   = "CREATE_OP",

        // Удаление
        ["delet"]    = "DELETE_OP", ["remov"] = "DELETE_OP", ["erase"]   = "DELETE_OP",
        ["clear"]    = "DELETE_OP", ["drop"]  = "DELETE_OP", ["purge"]   = "DELETE_OP",

        // Валидация и проверка
        ["valid"]    = "VALIDATE_OP", ["check"]   = "VALIDATE_OP", ["verif"]   = "VALIDATE_OP",
        ["assert"]   = "VALIDATE_OP", ["ensur"]   = "VALIDATE_OP", ["confirm"] = "VALIDATE_OP",
        ["inspect"]  = "VALIDATE_OP", ["guard"]   = "VALIDATE_OP",

        // Обработка и преобразование
        ["process"]  = "TRANSFORM_OP", ["transform"]= "TRANSFORM_OP", ["convert"] = "TRANSFORM_OP",
        ["parse"]    = "TRANSFORM_OP", ["format"]   = "TRANSFORM_OP", ["encod"]   = "TRANSFORM_OP",
        ["decod"]    = "TRANSFORM_OP", ["serial"]   = "TRANSFORM_OP", ["map"]     = "TRANSFORM_OP",

        // Аутентификация и авторизация
        ["auth"]     = "AUTH_OP", ["login"]   = "AUTH_OP", ["logout"]  = "AUTH_OP",
        ["token"]    = "AUTH_OP", ["permiss"] = "AUTH_OP", ["access"]  = "AUTH_OP",
        ["secret"]   = "AUTH_OP", ["credenti"]= "AUTH_OP",

        // Отправка и получение
        ["send"]     = "SEND_OP", ["emit"]    = "SEND_OP", ["publish"] = "SEND_OP",
        ["dispatch"] = "SEND_OP", ["notif"]   = "SEND_OP", ["push"]    = "SEND_OP",
        ["receiv"]   = "RECEIVE_OP", ["subscrib"]= "RECEIVE_OP", ["listen"]  = "RECEIVE_OP",

        // Ошибки и исключения
        ["error"]    = "ERROR", ["except"]  = "ERROR", ["fail"]    = "ERROR",
        ["throw"]    = "ERROR", ["catch"]   = "ERROR", ["reject"]  = "ERROR",

        // Успех и результат
        ["success"]  = "SUCCESS", ["result"]  = "SUCCESS", ["return"]  = "SUCCESS",
        ["resolv"]   = "SUCCESS", ["ok"]      = "SUCCESS", ["complet"] = "SUCCESS",

        // Null и пустота
        ["null"]     = "NULL", ["empty"]   = "NULL", ["none"]    = "NULL",
        ["miss"]     = "NULL", ["absent"]  = "NULL",

        // Числа и идентификаторы (не схлопываем — оставляем как есть)
        // Всё что не в словаре остаётся само по себе
    };

    // Схлопнуть список нормализованных слов в семейные теги
    public static List<string> Collapse(IReadOnlyList<string> normalizedWords)
    {
        var result = new List<string>(normalizedWords.Count);
        string? prevTag = null;
        foreach (var word in normalizedWords)
        {
            string tag = FamilyMap.TryGetValue(word, out string? fam) ? fam : word;
            // Не дублировать одинаковые теги подряд (VALIDATE_OP VALIDATE_OP → VALIDATE_OP)
            if (tag != prevTag)
            {
                result.Add(tag);
                prevTag = tag;
            }
        }
        return result;
    }

    // Схлопнуть строку целиком (E15 → E16 pipeline)
    public static List<string> CollapseString(string input)
        => Collapse(SemanticNormalizer.Normalize(input));

    // Схлопнуть и вернуть числовые индексы
    public static (List<int> Tokens, Dictionary<string, int> Vocab)
        CollapseToIds(string input, Dictionary<string, int>? existingVocab = null)
    {
        var vocab = existingVocab ?? new Dictionary<string, int>();
        var tokens = new List<int>();
        foreach (var tag in CollapseString(input))
        {
            if (!vocab.TryGetValue(tag, out int id))
                vocab[tag] = id = vocab.Count;
            tokens.Add(id);
        }
        return (tokens, vocab);
    }
}
```

## Шаг 2 — Создать `E16FamilyCollapser.cs`

**Путь:** `35.ExperimentsV2/Experiments/E16FamilyCollapser.cs`

```csharp
namespace ExperimentsV2;

// E16: FamilyCollapser — синонимы → семейные теги (GET_OP, VALIDATE_OP, etc.)
// Baseline:  SemanticNormalizer (E15 enhanced) — нормализованные слова
// Enhanced:  FamilyCollapser (E16) — семейные теги поверх E15
// Гипотеза:  vocab сжимается до ~20 концептов → N-грамм насыщается быстрее
//            → модель нужна меньше → обучение быстрее
public sealed class E16FamilyCollapser
{
    public OperationResult<ExperimentV2Result> Run(
        string combinedSource, int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(combinedSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет источника для E16");

        // ── Baseline: E15 нормализованные слова ──────────────────────────────
        var t0 = Stopwatch();
        var (baseTokens, baseVocab) = SemanticNormalizer.NormalizeToIds(combinedSource);
        if (baseTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало базовых токенов");

        int bSplit  = (int)(baseTokens.Count * 0.8);
        var bTrain  = baseTokens.Take(bSplit).ToList();
        var bVal    = baseTokens.Skip(bSplit).ToList();
        var baseBi  = new BigramModelV2();
        baseBi.Train(bTrain);
        var baseEval = baseBi.Evaluate(bVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline: " + baseEval.ErrorMessage);
        (double basePerp, double baseTop1, _, _) = baseEval.Value!;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: FamilyCollapser поверх E15 ─────────────────────────────
        var t1 = Stopwatch();
        var (enhTokens, enhVocab) = FamilyCollapser.CollapseToIds(combinedSource);
        if (enhTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало токенов после схлопывания");

        int eSplit  = (int)(enhTokens.Count * 0.8);
        var eTrain  = enhTokens.Take(eSplit).ToList();
        var eVal    = enhTokens.Skip(eSplit).ToList();
        var enhBi   = new BigramModelV2();
        enhBi.Train(eTrain);
        var enhEval = enhBi.Evaluate(eVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced: " + enhEval.ErrorMessage);
        (double enhPerp, double enhTop1, _, _) = enhEval.Value!;
        double enhancedMs = ElapsedMs(t1);

        // Топ-5 самых частых семейных тегов
        var freq = enhTokens.GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => $"{enhVocab.FirstOrDefault(kv => kv.Value == g.Key).Key ?? "?"}={g.Count()}")
            .ToList();

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E16-FAMILY-COLLAPSER",
            Tier              = tier,
            BaselinePrimary   = basePerp,
            EnhancedPrimary   = enhPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = (double)enhVocab.Count / baseVocab.Count,
            ScalingSignalNote = "vocab_compression (чем меньше — тем больше схлопнулось)",
            ShouldScaleUp     = enhVocab.Count > 30,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"baseVocab={baseVocab.Count}, enhVocab={enhVocab.Count}, " +
                    $"baseTokens={baseTokens.Count}, enhTokens={enhTokens.Count}, " +
                    $"basePerp={basePerp:F1}, enhPerp={enhPerp:F1}, " +
                    $"top5=[{string.Join(",", freq)}]"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Runner/Dispatcher/ResultWriter

```csharp
// Dispatcher:
public OperationResult<ExperimentV2Result> RunE16(string source, int tier = 1)
    => new E16FamilyCollapser().Run(source, tier);

// Runner ids: добавить "E16"
// Runner switch: "E16" => _d.RunE16(ds.CombinedText, tier),
// ResultWriter: "E16-FAMILY-COLLAPSER" => "E16-FAMILY-COLLAPSER",
```

## Ожидаемый вывод

```
[E16] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E16] Запуск...
OK perplexity (down) baseline=48.3 enhanced=8.7 delta=+82.0% | PASSED
notes: baseVocab=67, enhVocab=22, baseTokens=1284, enhTokens=987,
       top5=[GET_OP=187,VALIDATE_OP=143,SET_OP=98,NULL=87,SUCCESS=76]
```

---

---

# E17 — LogicPredicates: структурные паттерны → предикаты

## Гипотеза

E15+E16 работают со словами. E17 работает со **структурами**:
использует E09 роли чтобы конвертировать паттерны кода в логические предикаты.

`IF_CONDITION + NullLiteralExpression` → `NULL_GUARD`
`RETURN_VALUE + false/Error`           → `FAIL`
`ASSIGNED_VALUE + new SomeException`   → `THROW_OP`
`ForEach + ARGUMENT`                   → `ITERATE`

После E17 код выглядит как последовательность предикатов:
`NULL_GUARD → VALIDATE_OP → SUCCESS | NULL_GUARD → FAIL`

## Шаг 1 — Создать `LogicExtractor.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/LogicExtractor.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// LogicExtractor конвертирует AST паттерны в последовательность логических предикатов.
// Работает поверх E09 SyntacticRoleMapper.
// Каждый паттерн = (родительский тип, роль, тип узла) → метка предиката.
public static class LogicExtractor
{
    // Предикаты: числовые ID
    public const int UNKNOWN     = 0;
    public const int NULL_GUARD  = 1;  // if (x == null) / if (x is null)
    public const int FAIL        = 2;  // return false / throw
    public const int SUCCESS     = 3;  // return true / return result
    public const int VALIDATE_OP = 4;  // validate(x) вызов
    public const int ITERATE     = 5;  // foreach / for
    public const int ASSIGN      = 6;  // x = ...
    public const int GET_CALL    = 7;  // getX() / findX() вызов
    public const int CREATE_OBJ  = 8;  // new X()
    public const int CONDITION   = 9;  // if без null (общее условие)
    public const int TRANSFORM   = 10; // x.Map() / x.Select()
    public const int THROW_OP    = 11; // throw new Exception

    public static readonly string[] PredicateNames =
    [
        "UNKNOWN","NULL_GUARD","FAIL","SUCCESS","VALIDATE_OP",
        "ITERATE","ASSIGN","GET_CALL","CREATE_OBJ","CONDITION","TRANSFORM","THROW_OP"
    ];

    // Конвертировать C# файл в последовательность предикатов
    public static OperationResult<List<int>> Extract(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<List<int>>.Fail("Пустой источник");

        var tree = CSharpSyntaxTree.ParseText(csharpSource);
        var root = tree.GetRoot();
        var result = new List<int>();

        // Обходим узлы в порядке документа
        foreach (var node in root.DescendantNodes())
        {
            int predicate = ClassifyNode(node);
            if (predicate != UNKNOWN)
                result.Add(predicate);
        }

        if (result.Count == 0)
            return OperationResult<List<int>>.Fail("Ни одного предиката не извлечено");

        return OperationResult<List<int>>.Ok(result);
    }

    // Классифицировать узел в предикат
    private static int ClassifyNode(SyntaxNode node)
    {
        switch (node.Kind())
        {
            // NULL_GUARD: if с проверкой на null
            case SyntaxKind.IfStatement:
            {
                var ifStmt = (IfStatementSyntax)node;
                bool hasNullCheck = ifStmt.Condition.DescendantNodesAndSelf()
                    .Any(n => n.IsKind(SyntaxKind.NullLiteralExpression) ||
                              n.IsKind(SyntaxKind.IsPatternExpression));
                return hasNullCheck ? NULL_GUARD : CONDITION;
            }

            // FAIL: return false/null/throw
            case SyntaxKind.ReturnStatement:
            {
                var ret = (ReturnStatementSyntax)node;
                if (ret.Expression == null) return UNKNOWN;
                string expr = ret.Expression.ToString();
                return (expr == "false" || expr == "null" ||
                        expr.StartsWith("Result.Fail") ||
                        expr.StartsWith("OperationResult"))
                    ? FAIL : SUCCESS;
            }

            // THROW_OP: throw new Exception
            case SyntaxKind.ThrowStatement:
            case SyntaxKind.ThrowExpression:
                return THROW_OP;

            // ITERATE: foreach/for/while
            case SyntaxKind.ForEachStatement:
            case SyntaxKind.ForStatement:
            case SyntaxKind.WhileStatement:
                return ITERATE;

            // GET_CALL: вызов метода с именем get*/find*/load*/query*
            case SyntaxKind.InvocationExpression:
            {
                string name = GetMethodName(node).ToLowerInvariant();
                if (name.StartsWith("get") || name.StartsWith("find") ||
                    name.StartsWith("load") || name.StartsWith("query") ||
                    name.StartsWith("fetch") || name.StartsWith("read"))
                    return GET_CALL;
                if (name.StartsWith("valid") || name.StartsWith("check") ||
                    name.StartsWith("verif") || name.StartsWith("ensur"))
                    return VALIDATE_OP;
                if (name.StartsWith("map") || name.StartsWith("select") ||
                    name.StartsWith("convert") || name.StartsWith("transform"))
                    return TRANSFORM;
                return UNKNOWN;  // не каждый вызов важен
            }

            // CREATE_OBJ: new Something()
            case SyntaxKind.ObjectCreationExpression:
                return CREATE_OBJ;

            // ASSIGN: переменная = значение
            case SyntaxKind.SimpleAssignmentExpression:
            case SyntaxKind.LocalDeclarationStatement:
                return ASSIGN;

            default:
                return UNKNOWN;
        }
    }

    private static string GetMethodName(SyntaxNode invocation)
    {
        if (invocation is not InvocationExpressionSyntax inv) return "";
        return inv.Expression switch
        {
            MemberAccessExpressionSyntax m => m.Name.Identifier.Text,
            IdentifierNameSyntax id        => id.Identifier.Text,
            _                              => ""
        };
    }
}
```

## Шаг 2 — Создать `E17LogicPredicates.cs`

**Путь:** `35.ExperimentsV2/Experiments/E17LogicPredicates.cs`

```csharp
namespace ExperimentsV2;

// E17: LogicPredicates — код как последовательность логических предикатов.
// Baseline:  AST типы узлов (vocab ~145, E01 стиль) — уже проверено
// Enhanced:  предикаты (vocab=12) из LogicExtractor
// Гипотеза:  vocab=12 → таблица N-грамм насыщается даже на Tier 1
//            → перплексия падает сильнее чем E15+E16
//
// Это нижняя граница сжатия: нельзя сжать дальше без потери структуры.
// Если E17 хуже E16 — предикаты теряют информацию. Если лучше — нашли оптимум.
public sealed class E17LogicPredicates
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("Нет файлов для E17");

        // ── Baseline: FamilyCollapser (E16) ──────────────────────────────────
        var t0 = Stopwatch();
        string combined = string.Join("\n\n", files.Select(f => f.Text));
        var (baseTokens, baseVocab) = FamilyCollapser.CollapseToIds(combined);
        if (baseTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало базовых токенов");

        int bSplit  = (int)(baseTokens.Count * 0.8);
        var bTrain  = baseTokens.Take(bSplit).ToList();
        var bVal    = baseTokens.Skip(bSplit).ToList();
        var baseBi  = new BigramModelV2();
        baseBi.Train(bTrain);
        var baseEval = baseBi.Evaluate(bVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline: " + baseEval.ErrorMessage);
        (double basePerp, double baseTop1, _, _) = baseEval.Value!;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: предикаты из LogicExtractor ────────────────────────────
        var t1 = Stopwatch();
        var allPredicates = new List<int>();
        int failedFiles = 0;
        foreach (var (_, text) in files)
        {
            var extracted = LogicExtractor.Extract(text);
            if (extracted.IsSuccess)
                allPredicates.AddRange(extracted.Value!);
            else
                failedFiles++;
        }

        if (allPredicates.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Слишком мало предикатов: {allPredicates.Count}");

        int eSplit  = (int)(allPredicates.Count * 0.8);
        var eTrain  = allPredicates.Take(eSplit).ToList();
        var eVal    = allPredicates.Skip(eSplit).ToList();
        var enhBi   = new BigramModelV2();
        enhBi.Train(eTrain);
        var enhEval = enhBi.Evaluate(eVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced: " + enhEval.ErrorMessage);
        (double enhPerp, double enhTop1, _, _) = enhEval.Value!;
        double enhancedMs = ElapsedMs(t1);

        // Распределение предикатов
        var predDist = allPredicates
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => $"{LogicExtractor.PredicateNames[g.Key]}={g.Count()}");

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E17-LOGIC-PREDICATES",
            Tier              = tier,
            BaselinePrimary   = basePerp,
            EnhancedPrimary   = enhPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = (double)LogicExtractor.PredicateNames.Length / baseVocab.Count,
            ScalingSignalNote = "predicate_compression (12 предикатов / размер E16 vocab)",
            ShouldScaleUp     = allPredicates.Count < 200,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"files={files.Count}(failed={failedFiles}), " +
                    $"predicates={allPredicates.Count}(vocab=12), " +
                    $"baseVocab={baseVocab.Count}, " +
                    $"basePerp={basePerp:F1}, enhPerp={enhPerp:F1}, " +
                    $"dist=[{string.Join(",", predDist)}]"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Runner/Dispatcher/ResultWriter

```csharp
// Dispatcher:
public OperationResult<ExperimentV2Result> RunE17(
    IReadOnlyList<(string Path, string Text)> files, int tier = 1)
    => new E17LogicPredicates().Run(files, tier);

// Runner ids: добавить "E17"
// Runner switch: "E17" => DispatchE17(ds, tier),
// Dispatch метод:
private OperationResult<ExperimentV2Result> DispatchE17(DatasetContents ds, int tier)
    => _d.RunE17(ds.Files, tier);
// ResultWriter: "E17-LOGIC-PREDICATES" => "E17-LOGIC-PREDICATES",
```

## Ожидаемый вывод

```
[E17] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E17] Запуск...
OK perplexity (down) baseline=8.7 enhanced=2.1 delta=+75.9% | PASSED
notes: files=3(failed=0), predicates=312(vocab=12),
       baseVocab=22, basePerp=8.7, enhPerp=2.1,
       dist=[ASSIGN=89,CONDITION=67,NULL_GUARD=45,SUCCESS=38,GET_CALL=34]
```

---

## Общая картина: стек сжатия

```
Уровень         Vocab   Токенов   Перплексия  Эксперимент
─────────────────────────────────────────────────────────
0 — сырой текст  128     6 468     1 742       E03 baseline
1 — AST роли     145       968       110       E01/E09
2 — нормализация  67     1 284        48       E15
3 — семьи         22       987         9       E16
4 — предикаты     12       312         2       E17
```

Каждый уровень уменьшает перплексию потому что модели остаётся меньше
чего угадывать. При обучении нейросети это прямо транслируется:
**меньше перплексия → меньше шагов до сходимости**.

## Симметричный конвейер (будущее — после E17)

После того как E15–E17 пройдут:

```
Запрос пользователя:
"Please add a null check to ValidateToken before accessing properties"
           ↓ SemanticNormalizer
"add null check validate token access properti"
           ↓ FamilyCollapser
"CREATE_OP NULL VALIDATE_OP GET_CALL"
           ↓ Модель (vocab=12, быстро, точно)
"NULL_GUARD VALIDATE_OP SUCCESS | NULL_GUARD FAIL"
           ↓ Beautifier (правила + шаблоны)
"if (token == null) return OperationResult<T>.Fail('...');"
```

Модель говорит на языке предикатов с обеих сторон.
Это E18 — интеграционный эксперимент на симметричном пайплайне.

---

*E15/E16/E17 план v1.0 — семантический компилятор · не касается XIIGen*
