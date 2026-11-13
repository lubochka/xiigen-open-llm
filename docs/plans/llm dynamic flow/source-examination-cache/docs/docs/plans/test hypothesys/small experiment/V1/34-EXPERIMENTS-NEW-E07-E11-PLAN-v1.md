# 34.Experiments — Пять новых экспериментов E07–E11
## Добавляем в тот же подпроект который уже работает

**Проект:** llm-mvp-core — подпроект 34.Experiments  
**Этот документ не касается XIIGen.**

---

## Что уже есть в 34.Experiments

E01–E06 прошли шкалы Scale 0–3, результаты записаны в `experiments/results/`.  
Инфраструктура работает: `DatasetLoader`, `ExperimentRunner`, `ResultWriter`.

Новые эксперименты добавляются в те же папки:
```
34.Experiments/
├── Experiments/
│   ├── E01–E06 (уже есть)
│   ├── E07NaturalLanguageNgram.cs     ← новый
│   ├── E08NaturalLanguageGrammar.cs   ← новый
│   ├── E09ContextVectorKnn.cs         ← новый
│   ├── E10SvdPmiEmbedding.cs          ← новый
│   └── E11RegexDataPrep.cs            ← новый
├── Infrastructure/
│   ├── AliceTextLoader.cs             ← новый (загрузка датасета)
│   └── (остальное без изменений)
└── ExperimentsDispatcher.cs           ← добавить RunE07..RunE11
```

`ExperimentRunner` нужно расширить: добавить новые ID в switch и логику загрузки текста для E07/E08.

---

## E07 — N-граммы на естественном языке

**Проверяем:** биграмма на словах работает так же как на коде — или теория специфична только для кода?

### Датасет

**«Алиса в стране чудес»** — Project Gutenberg, public domain.  
URL: `https://www.gutenberg.org/files/11/11-0.txt`  
Размер: ~28 000 слов.

Загрузка — один раз, кеш в `data/alice.txt`:
```csharp
// AliceTextLoader.cs
public string Load(string dataDir)
{
    string path = Path.Combine(dataDir, "alice.txt");
    if (!File.Exists(path))
    {
        using var client = new HttpClient();
        string text = client.GetStringAsync("https://www.gutenberg.org/files/11/11-0.txt")
                           .GetAwaiter().GetResult();
        File.WriteAllText(path, text);
    }
    return File.ReadAllText(path);
}
```

**Тиры для E07 и E08:**
```
Scale 0 — первые 2 000 слов   (Алиса: ~главы 1–2)
Scale 1 — первые 8 000 слов   (~треть книги)
Scale 2 — первые 15 000 слов  (~половина книги)
Scale 3 — все 28 000 слов     (полная книга)
```

**Токенизация:** слова (пробел + нижний регистр + убрать знаки препинания).  
Это `WordLevelTokenize` — аналог `CharLevelTokenize` но слова а не символы.

### Baseline

Унаграммная модель: всегда предсказывать самое частое слово в корпусе.  
На «Алисе» это слово "the" (~1600 вхождений из 28K = 5.7% от всех).  
`baseline top1_accuracy ≈ 5.7%`

### Enhanced

Биграммная модель на словах: `P(next_word | prev_word)`.  
Тот же `BigramMarkov` из `Core/MarkovChain.cs` — новый код писать не нужно.

### Метрика

`top1_accuracy` — доля позиций где predicted_word == actual_next_word.

### Критерий прохождения

Scale 0: `improvement_pct >= 5%`.  
Ожидаем: +150–250% (биграмма на словах намного лучше унаграммы в языке — каждое предыдущее слово сильно ограничивает следующее).

### Где в коде

```csharp
// E07NaturalLanguageNgram.cs
// Принимает IReadOnlyList<int> wordTokens (слова закодированы в int)
// Логика: та же что E03 (BigramMarkov), только токены — слова а не символы

// ExperimentRunner: добавить в switch:
"E07" => _dispatcher.RunE07(WordLevelTokenize(dataset.NaturalText)),

// DatasetContents расширить полем NaturalText (загружается из alice.txt)
// или передавать отдельно через перегрузку Run()
```

---

## E08 — Грамматический фильтр на естественном языке

**Проверяем:** простые грамматические правила улучшают предсказание слов так же как Roslyn фильтр улучшает предсказание токенов кода (E06: +30–60%).

### Датасет

Та же «Алиса», те же тиры что E07.

### Baseline

Биграммная модель на словах — результат E07 Enhanced (одни и те же данные).

### Enhanced

Биграммная модель + POS фильтр на основе простых regex-правил (без ML, без внешних библиотек):

```csharp
// PosRulesFilter.cs
// 6 правил, компилируются один раз
static readonly HashSet<string> Articles     = new() { "the", "a", "an" };
static readonly HashSet<string> Prepositions = new() { "in", "on", "at", "of", "to", "from", "with", "by", "for" };
static readonly Regex           Adjective    = new(@"(?:ful|less|ous|al|ive|able|ible|ing|ed)$", RegexOptions.Compiled);

// Правило 1: после "the/a/an" → только слова на заглавную (имена) или Adjective
// Правило 2: после предлогов → не глаголы, не артикли
// Правило 3: после "said/asked/cried" → имена или "the/a/an"
// ...
```

**Маска допустимых:** для каждой текущей позиции — список слов которые грамматически допустимы следующими. Биграмма предсказывает только из этого списка.

### Метрика

`top1_accuracy` — та же что E07.  
Вторичная: `grammar_violations_pct` — сколько процентов предсказаний нарушают правила.

### Критерий прохождения

Scale 0: `improvement_pct >= 5%` относительно E07 enhanced (биграмма без фильтра).

---

## E09 — 5-мерный вектор + K-NN

**Проверяем:** добавление семантического положения в дереве Roslyn + K-NN поиск похожих контекстов улучшает предсказание следующего токена кода.

### Датасет

**Код C#** — тот же что E01–E06, `DatasetLoader.Load(scale, projectRoot)`.  
**Тир старта: Scale 1** (~9 100 токенов). На Scale 0 (1 файл) слишком мало векторов для K-NN.

### Baseline

Биграммная модель на BPE токенах — тот же что в E01 baseline и E03 baseline.  
Значение уже известно: Scale 1 ≈ 13–14% top1_accuracy.

### Enhanced

Вектор из пяти компонент + HNSW K-NN из подпроекта 06:

```
Для каждого токена в позиции i:
  context_vec = [
    bpe_id        / vocab_size,       // какой токен (0..1)
    ast_type_id   / 200.0f,           // тип AST узла (0..1, ~200 уникальных)
    ast_depth     / 20.0f,            // глубина в дереве (0..1)
    parent_type   / 200.0f,           // тип родительского узла (0..1)
    syntactic_role / 5.0f             // роль: 0=условие, 1=возврат, 2=аргумент, 3=присваивание, 4=другое
  ]

Построить HnswIndex (подпроект 06) из всех векторов обучающего среза.
Предсказание: найти K=5 ближайших векторов → голосование → предсказать самый частый следующий токен.
Fallback: если K-NN пустой → биграмма.
```

### Метрика

`top1_accuracy` на валидационном срезе 20%.

### Критерий прохождения

Scale 1: `improvement_pct >= 5%`.

### Где в коде

```csharp
// E09ContextVectorKnn.cs
// Использует: Roslyn (уже в E06) + HnswIndex из подпроекта 06

// ExperimentRunner: добавить
"E09" => _dispatcher.RunE09(
    BpeTokenize(dataset.CombinedText, 500),
    dataset.CombinedText),  // нужен исходный код для Roslyn
```

---

## E10 — SVD вместо прямой PMI матрицы

**Проверяем:** усечённое SVD сглаживает разреженную PMI матрицу и даёт лучшее предсказание чем прямой argmax.

### Датасет

**Код C#**, тот же `DatasetLoader`.  
**Тир старта: Scale 3** (~55K токенов). PMI матрица 500×500 нуждается в плотных данных.

### Baseline

Биграммная модель на BPE токенах.  
Scale 3 ≈ 35–45% top1_accuracy (из результатов E03 Scale 3).

### Enhanced

```
1. Построить матрицу совместной встречаемости: cooccur[a][b] = count(a → b)
2. TruncatedSvd(cooccur, k=30) → U[vocab×30], S[30], Vt[30×vocab]
   Использует TruncatedSvdService из 33.MatrixAlgorithms (worktree → перенести в main)
3. embedding[token] = U[token] × sqrt(diag(S))  // 30-мерный вектор
4. Предсказание: argmax_b( embedding[current] · embedding[b] ) для всех b в vocab
```

### Метрика

`top1_accuracy`.

### Критерий прохождения

Scale 3: `improvement_pct >= 5%` относительно биграммного baseline.

### Предварительное условие

**33.MatrixAlgorithms должен быть в main**, не только в worktree.  
Если не перенесён — E10 откладывается.

---

## E11 — Regex vs Roslyn: бенчмарк времени подготовки

**Проверяем:** regex-разметка файлов в 10+ раз быстрее Roslyn при покрытии ≥ 80% структур.

### Датасет

**Код C#**, Scale 1, 2, 3 — все три (проверяем как время масштабируется).

### Baseline

Roslyn разбор каждого файла: `CSharpSyntaxTree.ParseText(fileContent)`.  
Метрика: `ms_per_file` = суммарное время разбора / количество файлов.

### Enhanced

8 скомпилированных regex паттернов:

```csharp
static readonly Regex MethodStart = new(@"\b(public|private|protected)\s+[\w<>\[\]]+\s+\w+\s*\(", RegexOptions.Compiled);
static readonly Regex IfBlock     = new(@"\bif\s*\(",          RegexOptions.Compiled);
static readonly Regex ForLoop     = new(@"\bfor(?:each)?\s*\(", RegexOptions.Compiled);
static readonly Regex WhileLoop   = new(@"\bwhile\s*\(",       RegexOptions.Compiled);
static readonly Regex ReturnStmt  = new(@"\breturn\b",         RegexOptions.Compiled);
static readonly Regex ClassDecl   = new(@"\bclass\s+\w+",      RegexOptions.Compiled);
static readonly Regex CallSite    = new(@"\b\w+\.\w+\s*\(",    RegexOptions.Compiled);
static readonly Regex Assignment  = new(@"\b\w+\s*=[^=]",      RegexOptions.Compiled);
```

Метрика: `ms_per_file` для regex + `coverage_pct` = структуры найденные regex / структуры найденные Roslyn × 100.

### Метрика (основная)

`time_pct_saved` = (roslyn_ms - regex_ms) / roslyn_ms × 100.  
Вторичная: `coverage_pct`.

### Критерий прохождения

`time_pct_saved >= 90%` (regex в 10+ раз быстрее)  
И `coverage_pct >= 80%`.

---

## Изменения в существующих файлах

| Файл | Что добавить |
|---|---|
| `Infrastructure/ExperimentRunner.cs` | Новые ветки в switch: `"E07"..  "E11"` + `WordLevelTokenize()` метод |
| `ExperimentsDispatcher.cs` | `RunE07()..RunE11()` методы |
| `Infrastructure/DatasetLoader.cs` | Тиры для текста (Scale 0–3 по словам Алисы) — либо отдельный `AliceTextLoader` |

| Новый файл | Что делает |
|---|---|
| `Infrastructure/AliceTextLoader.cs` | Загрузка + кеш alice.txt |
| `Experiments/E07NaturalLanguageNgram.cs` | Биграмма на словах |
| `Experiments/E08NaturalLanguageGrammar.cs` | Биграмма + POS фильтр |
| `Experiments/E09ContextVectorKnn.cs` | 5D вектор + HNSW K-NN |
| `Experiments/E10SvdPmiEmbedding.cs` | TruncatedSVD embedding |
| `Experiments/E11RegexDataPrep.cs` | Regex vs Roslyn бенчмарк |

---

## Порядок

```
Шаг 1: AliceTextLoader + WordLevelTokenize + E07 (2 дня)
        Gate B: E07 Scale 0 improvement > 50%

Шаг 2: E08 POS фильтр (1 день)
        Gate B: grammar_violations_pct падает от baseline до enhanced

Шаг 3: E09 вектор + K-NN (3 дня)
        Gate B: Scale 1 improvement > 5%

Шаг 4: Перенести 33.MatrixAlgorithms из worktree в main → E10 (2 дня)

Шаг 5: E11 regex бенчмарк (1 день)
        Gate B: regex в 10× быстрее при 80%+ покрытии
```

---

⛔ STOP — жду одобрения.

*34.Experiments New Experiments E07–E11 Plan v1.0 — не касается XIIGen*
