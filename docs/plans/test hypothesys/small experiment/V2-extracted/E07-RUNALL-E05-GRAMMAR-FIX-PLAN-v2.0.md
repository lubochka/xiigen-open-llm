# E07-RunAll + E05 Grammar Mask — Fix Plan v1.0
## Два исправления, одна сессия Claude Code

**Проект:** llm-mvp-core — 35.ExperimentsV2  
**Файлов меняется:** 2 (ExperimentRunner.cs + E05SyntaxFilter.cs)  
**Этот документ не касается XIIGen.**

---

## Исправление 1 — E07 не в RunAll (1 строка)

**Файл:** `35.ExperimentsV2/ExperimentRunner.cs`, строка 76

```csharp
// БЫЛО:
string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06"];

// СТАЛО:
string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07"];
```

**Проверка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаемый вывод: `Build succeeded. 0 Error(s)`

Затем запустить E07 отдельно и убедиться что появился файл результата:
```
runner.Run("E07", tier: 1)
```
Ожидаем в консоли строку:
```
[E07] Tier 1 загрузка... 3 файлов, ...
[E07] Запуск... OK top1_accuracy (up) baseline=... enhanced=... | ...
  -> .../experiments-v2/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-YYYY-MM-DD-001.json
```

---

## Исправление 2 — E05 тавтологическая маска (30 строк)

### Корень проблемы

`TrigramModelV2.ValidContinuations(prev2, prev1)` возвращает токены
которые встречались после `(prev2, prev1)` **в обучающих данных**.
Это в точности то же множество что триграмм уже использует для предсказания.
Ограничить prediction этим множеством = ничего не изменить → delta=0%.

### Принцип исправления

Маска должна строиться из **всех файлов датасета** (train + val вместе),
не из обучающего среза.

Тогда:
- Маска знает переходы которые встречаются в C# коде в целом
- Триграмм знает только переходы из training split
- Когда триграмм хочет предсказать токен X, но маска говорит
  "X никогда не следует за этим контекстом ни в одном файле" —
  enhanced выбирает следующий по вероятности токен из маски
- Это реальное ограничение, а не тавтология

### Что изменить в `E05SyntaxFilter.cs`

#### Шаг 1: изменить сигнатуру Run()

```csharp
// БЫЛО:
public OperationResult<ExperimentV2Result> Run(string csharpSource, int tier = 1)

// СТАЛО:
public OperationResult<ExperimentV2Result> Run(
    string csharpSource,
    IReadOnlyList<string>? allSources = null,   // все файлы датасета (train+val)
    int tier = 1)
```

`allSources` — необязательный параметр. Если не передан, fallback на `csharpSource`
(поведение не хуже чем сейчас, тест не сломается).

#### Шаг 2: добавить метод BuildGrammarTable

Добавить в конец класса `E05SyntaxFilter` перед закрывающей `}`:

```csharp
// BuildGrammarTable строит таблицу (prev2, prev1) -> {valid next tokens}
// из ВСЕХ файлов датасета — не только из обучающего среза.
// Это даёт реальное ограничение: если переход (A,B)->C никогда не встречался
// ни в одном файле проекта — он грамматически маловероятен для данного кода.
private static Dictionary<(int, int), HashSet<int>> BuildGrammarTable(
    IReadOnlyList<string> sources)
{
    var table = new Dictionary<(int, int), HashSet<int>>();

    foreach (var source in sources)
    {
        var parsed = E01AstTokenizer.ParseAstTokens(source);
        if (!parsed.IsSuccess) continue;

        var toks = parsed.Value!.tokens;
        for (int i = 2; i < toks.Count; i++)
        {
            var key = (toks[i - 2], toks[i - 1]);
            if (!table.TryGetValue(key, out var set))
                table[key] = set = new HashSet<int>();
            set.Add(toks[i]);
        }
    }

    return table;
}
```

#### Шаг 3: использовать BuildGrammarTable в Run()

В теле `Run()`, сразу после разбора AST токенов (после строки `if (astTokens.Count < 6)`):

```csharp
// Построить грамматическую таблицу из ВСЕХ источников (не только train).
// Если allSources не передан — используем только csharpSource (слабее, но работает).
var grammarSources = allSources ?? new[] { csharpSource };
var grammarTable   = BuildGrammarTable(grammarSources);
```

#### Шаг 4: заменить тавтологическую маску на грамматическую

В цикле оценки — найти строку:
```csharp
var mask = trigram.ValidContinuations(prev2, prev1);
```

Заменить на:
```csharp
// Грамматическая маска из всех файлов, а не из обучающего среза.
// Триграмм мог обучиться на переходах которых нет в грамматике —
// и наоборот, пропустить грамматически валидные переходы редких файлов.
grammarTable.TryGetValue((prev2, prev1), out var grammarSet);
var mask = grammarSet ?? (IReadOnlySet<int>)new HashSet<int>();
```

#### Шаг 5: обновить InputAdapters.ForE05

В `35.ExperimentsV2/Infrastructure/InputAdapters.cs`:

```csharp
// БЫЛО:
public static string ForE05(DatasetContents d) => d.CombinedText;

// СТАЛО:
public static (string Combined, IReadOnlyList<string> AllSources) ForE05(DatasetContents d)
    => (d.CombinedText, d.Files.Select(f => f.Text).ToList().AsReadOnly());
```

#### Шаг 6: обновить вызов в ExperimentRunner.DispatchE05

```csharp
// БЫЛО:
"E05" => _d.RunE05(InputAdapters.ForE05(ds), tier: tier),

// СТАЛО — в методе Dispatch():
"E05" => DispatchE05(ds, tier),

// И добавить приватный метод:
private OperationResult<ExperimentV2Result> DispatchE05(DatasetContents ds, int tier)
{
    var (combined, allSources) = InputAdapters.ForE05(ds);
    return _d.RunE05(combined, allSources, tier: tier);
}
```

#### Шаг 7: обновить RunE05 в Dispatcher

```csharp
// БЫЛО:
public OperationResult<ExperimentV2Result> RunE05(string csharpSource, int tier = 1)
    => _e05.Run(csharpSource, tier);

// СТАЛО:
public OperationResult<ExperimentV2Result> RunE05(
    string csharpSource,
    IReadOnlyList<string>? allSources = null,
    int tier = 1)
    => _e05.Run(csharpSource, allSources, tier);
```

---

## Проверка E05 после исправления

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

**Запустить E05 и убедиться что delta != 0:**
```
runner.Run("E05", tier: 1)
```

Ожидаем в консоли:
```
[E05] Tier 1 загрузка... 3 файлов, ...
[E05] Запуск... OK top1_accuracy (up) baseline=0.51 enhanced=0.55 delta=+X.X% | ...
```

Delta должна быть ненулевой (положительной или отрицательной — неважно, главное не 0).
Нулевая delta при Tier 1 с 3 файлами допустима только если все 3 файла идентичны
(их нет) — в реальности грамматическая таблица из 3 файлов будет шире training split.

**Запустить все:**
```
runner.RunAll(tier: 1)
```

Ожидаем E07 в таблице и E05 с ненулевой delta.

---

## Порядок выполнения для Claude Code

```
1. Изменить ExperimentRunner.cs строка 76: добавить "E07"
   -> dotnet build -> Build succeeded

2. Изменить E05SyntaxFilter.cs:
   a. Новая сигнатура Run() с allSources
   b. Добавить BuildGrammarTable в конец класса
   c. Использовать grammarTable вместо trigram.ValidContinuations

3. Изменить InputAdapters.cs: ForE05 возвращает кортеж
4. Изменить ExperimentRunner.cs: DispatchE05 приватный метод
5. Изменить ExperimentsV2Dispatcher.cs: RunE05 с allSources

   -> dotnet build -> Build succeeded
   -> dotnet test -> все тесты зелёные (0 failures)

6. runner.Run("E07", tier: 1) -> файл JSON создан
7. runner.Run("E05", tier: 1) -> delta != 0.0%
8. runner.RunAll(tier: 1)     -> E07 в таблице, E05 с delta
```

---

## Что НЕ трогать

- E02 и E03 с 0.0% delta — это правильные сигналы "нужен Scale 1", не баги.
- Остальные эксперименты E01, E04, E06 — без изменений.
- Core/NgramModel.cs — без изменений.
- Тесты — без изменений (существующие тесты не должны упасть).

---

*Fix Plan v1.0 — E07 RunAll + E05 Grammar Mask · не касается XIIGen*
