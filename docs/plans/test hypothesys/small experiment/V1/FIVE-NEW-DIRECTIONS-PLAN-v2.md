# Пять новых экспериментов — конкретные спецификации v2
## Что именно делать, на каком датасете, что измерять

**Проект:** llm-mvp-core — 35.ExperimentsV2  
**Этот документ не касается XIIGen.**

---

## Как читать этот документ

Каждый эксперимент имеет одну и ту же структуру:
- **Датасет** — откуда берём данные, как загружаем
- **Baseline** — что запускаем как оригинальный подход
- **Enhanced** — что меняем (одно изменение!)
- **Метрика** — одно число которое сравниваем
- **Критерий** — при каком значении считаем прошедшим
- **Тир старта** — с какого размера данных начинаем

---

## E08 — Время на шаг (патч всех существующих экспериментов)

*Это не новый эксперимент, а исправление метрики в E01–E07.*

**Проблема:** `baseline_time_ms = 261ms` — каша из трёх фаз: загрузка данных + обучение модели + предсказания. Непонятно стало ли предсказание быстрее.

**Что добавить в каждый существующий эксперимент:**

```
Разбивка времени (три отдельных поля):
  data_prep_time_ms   — время загрузки файлов и токенизации (одинаково для baseline и enhanced)
  fit_time_ms         — время обучения модели (K-Means fit, N-gram build, BM25 index)
  inference_time_ms   — время собственно предсказаний на валидационном срезе
  
  time_per_prediction_us — inference_time_ms / количество_предсказаний × 1000
```

**Датасет:** те же что и в E01–E07, никаких новых данных.

**Baseline vs Enhanced:** `time_per_prediction_us` для baseline vs enhanced.  
Если enhanced медленнее — мы должны видеть это число, а не скрытое в общем итоге.

**Критерий:** не pass/fail, это диагностическая метрика.  
Обязательно присутствует в каждом результате начиная с этого момента.

**Файлы:** `Core/ResultTypes.cs` (+3 поля), патч в каждом E01–E07 (~5 строк в каждом).

---

## E08-NL — E03 на естественном языке (N-граммы)

*Проверяем: держится ли N-граммная теория на словах так же как на коде.*

### Датасет

**«Алиса в стране чудес»** — Lewis Carroll, Project Gutenberg.  
Единственный необходимый файл: `https://www.gutenberg.org/files/11/11-0.txt`  
Размер: 165 КБ, ~28 000 слов.

Загрузка при первом запуске (один раз):
```csharp
// NaturalLanguageLoader.cs
string url  = "https://www.gutenberg.org/files/11/11-0.txt";
string path = Path.Combine(dataDir, "alice.txt");
if (!File.Exists(path))
    using var client = new HttpClient();
    File.WriteAllText(path, await client.GetStringAsync(url));
```

Кеш: `data/alice.txt` — не скачивать повторно.

**Тиры:**
```
Tier 1: первые 3 000 слов     (~главы 1–2)
Tier 2: первые 15 000 слов    (~половина книги)
Tier 3: все 28 000 слов       (полная книга)
```

### Что меняется

**Токенизация:** вместо символов — слова.

```csharp
// WordLevelTokenize: разбить по пробелам, убрать знаки препинания, lowercase
// "Hello, World!" → ["hello", "world"]
// Затем слово → int через словарь (слово → уникальный индекс)
```

### Baseline

Униграммная модель: предсказывать самое частое слово из всего корпуса.  
Это нижняя граница — ничего не знаем о контексте.

```
На Alice (28K слов): самое частое слово "the" (~1600 раз = 5.7%)
baseline top-1 accuracy ≈ 5.7%
```

### Enhanced

Биграммная модель на словах: P(next_word | prev_word).

```
"the" → чаще всего следует "little" (Alice), "Queen", "Mock"
baseline: угадывает 5.7% (всегда "the")
enhanced: ожидаем 15–25% (биграмма знает что после "the" идут существительные)
```

### Метрика

`top1_accuracy` — доля предсказаний где первое слово в списке совпало с реальным.

### Критерий прохождения

Tier 1: `improvement_pct >= 5%`  
Ожидаем: +100–200% (биграмма лучше униграммы в разы на естественном языке).

### Сравнение с E03 на коде

| | E03 (C# код) | E08-NL (Алиса) |
|---|---|---|
| Единица токена | ASCII символ | Слово |
| Baseline | Унаграмма | Унаграмма |
| Enhanced | Биграмма | Биграмма |
| Результат Scale 2 | +44.1% | ожидаем +150%+ |

Если оба проходят — теория универсальна для любых последовательностей.

---

## E09-NL — E05 на естественном языке (грамматический фильтр)

*Проверяем: ограничивает ли грамматика языка так же как Roslyn ограничивает код.*

### Датасет

Та же «Алиса», те же тиры что в E08-NL.

### Baseline

Биграммная модель на словах без фильтра (результат E08-NL enhanced = E09-NL baseline).

### Enhanced

Биграммная модель + простой POS фильтр на основе regex-правил:

```csharp
// PosRulesFilter.cs — без внешних библиотек, только regex
// Правило 1: после "the", "a", "an" → допустимы только существительные и прилагательные
// Правило 2: после глагола ("is", "was", "are") → допустимы существительные и прилагательные
// Правило 3: после "very", "so", "quite" → допустимы только прилагательные и наречия

// Regex-правила для определения части речи (грубые, без ML):
// Существительные: заглавная буква (Alice, Queen) или слова из noun_list (100 самых частых)
// Прилагательные: оканчиваются на -ful, -less, -ous, -al, -ive, -ing, -ed
// Глаголы: is, was, had, said, went, came, went + слова на -ed, -ing
```

**Важно:** фильтр не точный — это намеренно. Проверяем что даже грубая грамматика помогает.

### Метрика

`syntactically_correct_pct` — доля предсказаний где предсказанное слово грамматически допустимо после текущего слова.

`top1_accuracy` — вторичная метрика.

### Критерий прохождения

`syntactically_correct_pct >= 85%` (baseline ≈ 30–40% случайного нарушения грамматики).

---

## E10 — E07v2: 5-мерный вектор + K-NN

*Исправление E07: правильный вектор семантического положения + K-NN вместо прямого поиска.*

### Датасет

**Код C#** — тот же что 35.ExperimentsV2, тиры 1–3.  
(Сначала проверяем на коде, потом при желании на Алисе.)

**Тир старта: Tier 2** (~17K токенов, 9 подпроектов).  
Tier 1 слишком мал для K-NN: нужно минимум 1000 векторов в индексе чтобы найти 5 соседей.

### Baseline

Биграммная модель на BPE токенах (то же что E03 baseline).  
`top1_accuracy` на Tier 2 = ~36–40%.

### Enhanced

5-мерный вектор + HNSW K-NN из подпроекта 06 (VectorSearch).

**Вектор** — для каждого токена в позиции i строим:
```
[bpe_id, ast_type_id, ast_depth, parent_ast_type_id, syntactic_role_id]

bpe_id            — токен (0..vocab_size)
ast_type_id       — SyntaxKind как int (нормализованный 0..N)
ast_depth         — глубина в AST дереве (0 = корень, обычно 3–12)
parent_ast_type_id — SyntaxKind родительского узла
syntactic_role_id — 0=condition, 1=return_value, 2=argument, 3=assignment_rhs, 4=other
```

**Предсказание через K-NN:**
```
1. Для текущей позиции i строим вектор context_vec = [bpe_id_i, ast_type_i, ...]
2. Ищем K=5 ближайших векторов в HnswIndex (подпроект 06)
3. Смотрим на следующий токен после каждого из 5 соседей
4. Предсказываем токен который встречается чаще всего среди 5 ответов
5. Если K-NN не нашёл соседей (пустой индекс) → fallback на биграмму
```

**Нормализация векторов для HNSW:** все компоненты должны быть в [0,1].
```csharp
float[] Normalize(int bpeId, int astType, int depth, int parentType, int role)
{
    return new float[]
    {
        bpeId      / (float)vocabSize,      // 0..1
        astType    / (float)maxAstTypes,    // 0..1 (maxAstTypes ≈ 200)
        depth      / 20.0f,                 // 0..1 (глубина редко > 20)
        parentType / (float)maxAstTypes,    // 0..1
        role       / 5.0f,                  // 0..1 (5 ролей)
    };
}
```

### Метрика

`top1_accuracy` — доля предсказаний где K-NN угадал следующий токен.

### Критерий прохождения

Tier 2: `improvement_pct >= 5%` относительно биграммного baseline.  
Ожидаем: +5–15% (K-NN использует структурный контекст который биграмма игнорирует).

---

## E11 — SVD вместо прямой PMI (исправление E04)

*E04 провалился из-за разреженной матрицы. SVD сглаживает разреженность.*

### Датасет

**Код C#**, Tier 3 (~55K токенов, все подпроекты 00–14).  
Меньше Tier 3 не имеет смысла: SVD на матрице 128×128 тривиален.

### Baseline

Прямой PMI предсказатель (текущий E04 Enhanced):  
`argmax(PMI[current_token, :])` — предсказать токен с максимальным PMI.  
Результат E04 Scale 3: baseline_accuracy ≈ 15.3%, enhanced (прямой PMI) = 11.8% → **хуже**.

### Enhanced

SVD-based предсказание:

```
1. Построить матрицу совместной встречаемости co_occurrence[a][b] = count(a затем b)
2. Применить TruncatedSvdService(co_occurrence, k=30) → U[vocab×30], S[30], Vt[30×vocab]
3. embedding[token] = U[token] × sqrt(S)    // 30-мерный вектор для каждого токена
4. Предсказание: найти токен с максимальным dot product embedding[current] · embedding[*]
5. Это эквивалентно поиску наиболее «семантически ожидаемого» следующего токена
```

**Использует:** `TruncatedSvdService` из 33.MatrixAlgorithms (worktree → перенести в main).

### Метрика

`top1_accuracy` на валидационном срезе 20%.

### Критерий прохождения

`improvement_pct >= 5%` относительно **биграммного baseline** (не PMI baseline).  
Биграмма = 15.3% → нужно > 16.1%.

---

## E12 — Regex подготовка данных vs Roslyn (бенчмарк скорости)

*Это не эксперимент предсказания — это бенчмарк времени подготовки.*

### Датасет

**Код C#**, все три тира (1, 2, 3).  
Измеряем время подготовки на каждом тире.

### Baseline

**Roslyn** — разбор каждого файла через `CSharpSyntaxTree.ParseText()`.  
Время: измеряем сколько мс занимает разбор одного файла и всего тира.

### Enhanced

**Regex** — разметка без полного разбора AST:

```csharp
// RegexDataPreparer.cs
// Набор из 8 паттернов C# структур — компилируются один раз при старте
static readonly Regex MethodStart = new(@"\b(?:public|private|protected)\s+\w[\w<>]*\s+(\w+)\s*\(", RegexOptions.Compiled);
static readonly Regex IfBlock     = new(@"\bif\s*\(",   RegexOptions.Compiled);
static readonly Regex ForLoop     = new(@"\bfor(?:each)?\s*\(", RegexOptions.Compiled);
static readonly Regex ReturnStmt  = new(@"\breturn\b",  RegexOptions.Compiled);
static readonly Regex ClassStart  = new(@"\bclass\s+(\w+)", RegexOptions.Compiled);
static readonly Regex Assignment  = new(@"\b(\w+)\s*=[^=]", RegexOptions.Compiled);
static readonly Regex CallSite    = new(@"(\w+)\.(\w+)\s*\(", RegexOptions.Compiled);
static readonly Regex StringLit   = new(@"""[^""]*""",   RegexOptions.Compiled);
```

Каждый файл получает список `(позиция, тег_структуры)` — без полного AST дерева.

### Метрика

**Не точность предсказания** — здесь сравниваем только:

```json
"data_prep_time_ms_per_file": {
  "roslyn": 312,
  "regex":  0.8
},
"coverage": {
  "roslyn_structures_found": 1847,
  "regex_structures_found":  1621,
  "regex_coverage_pct":     87.8
}
```

`coverage` — какой процент структур regex находит по сравнению с Roslyn.

### Критерий прохождения

`regex_time_per_file < roslyn_time_per_file × 0.1` (в 10 раз быстрее)  
И `coverage >= 80%` (regex находит не менее 80% структур что Roslyn).

---

## Сводная таблица всех пяти направлений

| Эксп. | Датасет | Baseline | Enhanced | Метрика | Тир старта |
|---|---|---|---|---|---|
| Патч времени | Те же E01–E07 | — | — | time_per_prediction_us | — |
| E08-NL (N-граммы на языке) | Alice 28K слов | Униграмма слов | Биграмма слов | top1_accuracy | Tier 1 |
| E09-NL (грамм. фильтр) | Alice 28K слов | Биграмма слов | Биграмма + POS фильтр | syntactically_correct_pct | Tier 1 |
| E10 (вектор + K-NN) | Код C# 00–14 | Биграмма BPE | 5D вектор + HNSW K-NN | top1_accuracy | Tier 2 |
| E11 (SVD вместо PMI) | Код C# 00–14 | Биграмма BPE | TruncatedSVD embeddings | top1_accuracy | Tier 3 |
| E12 (regex vs Roslyn) | Код C# тиры 1–3 | Roslyn время | Regex время | time_per_file_ms | Tier 1 |

---

## Порядок реализации

```
Шаг 1: Патч времени в E01–E07 (1 день)
        → После этого все будущие результаты содержат time_per_prediction_us

Шаг 2: E08-NL + E09-NL — естественный язык (2 дня)
        → Нужен только: WordLevelTokenize + Alice загрузка + PosRulesFilter
        → Gate B: E08-NL Tier 1 показывает improvement > 50%

Шаг 3: E12 — regex бенчмарк (1.5 дня)
        → Gate B: regex в 10+ раз быстрее, покрывает > 80% структур

Шаг 4: E10 — 5D вектор + K-NN (3 дня)
        → Зависит от Шага 1 (нужен time_per_prediction_us)
        → Gate B: Tier 2 improvement > 5%

Шаг 5: E11 — SVD PMI (2 дня)
        → Нужен перенос 33.MatrixAlgorithms из worktree в main
        → Gate B: Tier 3 improvement > 5% vs биграмма
```

---

⛔ STOP — жду одобрения.

*Five New Directions v2 — конкретные спецификации · не касается XIIGen*
