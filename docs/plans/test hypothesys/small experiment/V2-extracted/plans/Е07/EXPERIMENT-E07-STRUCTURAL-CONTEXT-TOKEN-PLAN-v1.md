# E07 — Структурный контекст-токен: токен + его место в мире кода
## Отдельный план эксперимента · LlmFromScratch.sln

**ID:** E07-STRUCTURAL-CONTEXT-TOKEN  
**Тип:** новый подход, синтез E01–E06 + Graph RAG + X-03 (астроцитарный слой)  
**Связанные подпроекты:** 07.GraphEngine, 08.GraphRag, 13.CodeOptimizer, 33.CodeCompletion

---

## Основная идея

Токен — это не просто число 4821. Токен — это узел в нескольких пересекающихся структурах одновременно:

```
В последовательности байтов:   позиция 1847 в файле
В синтаксическом дереве:       лист на глубине 3, сын ClassDeclaration
В графе файлов:                принадлежит файлу 7 из 195
В графе вызовов:               получает данные от токенов [3421, 8912]
В пространстве смыслов:        кластер 12 (K-Means), фрейм 142 (FrameNet)
```

Если обучать матрицу зная все эти измерения сразу — модель не тратит итерации на "открытие" структуры, которая уже известна.

---

## Структура обогащённого токена

```csharp
// StructuralToken — токен с полным структурным контекстом.
// Каждое поле отвечает за одно измерение пространства в котором живёт токен.
public readonly record struct StructuralToken
{
    // Оригинальный BPE идентификатор — как и раньше
    public int BpeId { get; init; }

    // Тип AST-узла (из E01): MethodDeclaration, IfStatement, ReturnStatement...
    // Это "что есть этот токен" в синтаксическом смысле
    public short AstNodeType { get; init; }

    // Тип РОДИТЕЛЬСКОГО AST-узла: в каком узле мы находимся прямо сейчас
    // "IfStatement.Condition" отличается от "MethodBody" — разный контекст
    public short AstParentType { get; init; }

    // Глубина в синтаксическом дереве: 0 = корень, 8 = глубоко вложенный
    // Поверхностные узлы (классы, методы) vs вложенные (выражения, условия)
    public byte AstDepth { get; init; }

    // Идентификатор файла: из какого файла этот токен
    // Позволяет модели знать "это из того же файла что предыдущий токен"
    public short FileId { get; init; }

    // Глубина вложенности областей видимости (scope): количество { } вокруг
    public byte ScopeDepth { get; init; }

    // PageRank оценка этого токена/метода в графе вызовов (из E06)
    // Важность в архитектуре: высокое = много мест зависит от этого
    public float PageRankScore { get; init; }

    // Входящий поток данных: какие токены передают данные В ЭТОТ токен
    // Максимум 4 входящих, упакованы в один long для компактности
    public long DataFlowInPacked { get; init; }

    // Семантический кластер (из K-Means, E05 idea): к какой смысловой семье относится
    public short SemanticClusterId { get; init; }
}
```

**Как это хранится:** не как объект в памяти во время инференса, а как набор параллельных массивов (SoA — Structure of Arrays). Один массив для bpe_id, один для ast_type, один для depth, и т.д. При загрузке батча — concatenate нужные измерения.

---

## Как обучать матрицу с этими связями

### Вариант A — Аддитивное встраивание (samые просто, начать с этого)

Каждое измерение имеет свою отдельную таблицу встраивания (embedding table):

```csharp
// CompositeEmbedding вычисляет встраивание токена как сумму компонент.
// Каждое измерение имеет свою таблицу — можно обучать совместно или раздельно.
public class CompositeEmbedding
{
    // Основная таблица для BPE токенов: vocab_size × hidden_dim
    private readonly Embedding BpeEmbedding;

    // Таблица для типов AST-узлов: ~200 типов × hidden_dim
    // Намного меньше чем BPE — но кодирует структуру
    private readonly Embedding AstTypeEmbedding;

    // Таблица для родительских узлов: ~200 типов × hidden_dim
    private readonly Embedding ParentTypeEmbedding;

    // Таблица для глубины дерева: 0–20 × hidden_dim
    // "Чем глубже — тем конкретнее" — это структурная семантика
    private readonly Embedding DepthEmbedding;

    // Таблица для файлов: num_files × hidden_dim
    // Токены из одного файла должны тяготеть друг к другу
    private readonly Embedding FileEmbedding;

    // Скалярный проекционный слой для непрерывных значений (pagerank, data_flow)
    private readonly Linear ContinuousProjection;

    // Вычислить встраивание токена как сумму всех компонент
    // Сумма потому что каждое измерение добавляет информацию независимо
    // (аналогично тому как BERT суммирует token + position + segment embeddings)
    public Tensor Compute(StructuralToken token)
    {
        var embedding =
            BpeEmbedding.Forward(token.BpeId)
            + AstTypeEmbedding.Forward(token.AstNodeType)
            + ParentTypeEmbedding.Forward(token.AstParentType)
            + DepthEmbedding.Forward(token.AstDepth)
            + FileEmbedding.Forward(token.FileId);

        // Добавить непрерывные сигналы (PageRank, data_flow) через проекцию
        var continuous = new float[] { token.PageRankScore, token.ScopeDepth };
        embedding = embedding + ContinuousProjection.Forward(continuous);

        return embedding;
    }
}
```

**Почему сумма работает:** каждый компонент добавляет независимое измерение информации. Модель учится какой вес придавать каждому компоненту через обратное распространение ошибки. BPE компонент получит большой вес для смыслового содержания, AstType компонент — для синтаксической структуры.

### Вариант B — Граф-внимание на структурных связях

Вместо суммирования: построить граф где токены связаны структурными рёбрами, применить Graph Attention Network.

Рёбра графа:
```
parent_of:     токен → его родитель в AST дереве
same_file:     токены из одного файла
data_flows_to: токен A → токен B (данные текут от A к B)
same_scope:    токены на одной глубине вложенности
```

Внимание в трансформере тогда работает не только по позиции, но и по структурным связям — как разреженная маска из E03, но выведенная из структуры кода, а не из позиции.

### Вариант C — Структурно-направленное батчирование ("поместить рядом")

Самый интересный и самый новый вариант. Не меняем архитектуру — меняем как составляем батчи.

**Стандартное батчирование:** берём последовательные токены из файла.
```
Контекстное окно: [t1, t2, t3, t4, t5, t6, t7, t8]
Линейная последовательность — модель видит только ближайших соседей.
```

**Структурное батчирование:**
```
Для каждого целевого токена t_target:
  1. Найти его структурных соседей: токены с тем же родителем, тем же data_flow, тем же файлом
  2. Составить контекстное окно из ЭТИХ токенов, даже если они далеко в линейном тексте
  3. t_target видит: [t_parent, t_sibling_3, t_sibling_7, t_data_flow_source, t_same_scope_5]

Graph RAG при обучении, не при инференсе.
```

Это означает что модель учится на "правильных" зависимостях сразу, а не постепенно открывает их через много итераций.

---

## Марковская проверка (без нейросети, из RESEARCH-04)

Перед запуском обучения — быстрая проверка через многомерную N-граммную модель:

**Baseline (E03):** `P(next_token | prev_bpe_id)` — биграмм на BPE токенах

**Enhanced E07:** `P(next_token | prev_bpe_id, prev_ast_type, prev_parent_type, prev_depth, prev_file_id)`

```csharp
// MultidimensionalNGram — Марковская цепь на нескольких измерениях сразу.
// Если это улучшает точность → улучшит и нейросеть (RESEARCH-04).
public class MultidimensionalNGram
{
    // Таблица переходов: (bpe, ast_type, depth) → {next_bpe → count}
    // Ключ = кортеж из нескольких измерений текущего состояния
    private readonly Dictionary<(int bpe, short ast, byte depth), Dictionary<int, int>> transitions;

    // Предсказать следующий токен зная текущий BPE + AST тип + глубину
    public int Predict(int currentBpe, short currentAst, byte currentDepth)
    {
        var key = (currentBpe, currentAst, currentDepth);

        if (!transitions.TryGetValue(key, out var nextTokenCounts))
        {
            // Если нет статистики для этой комбинации — откатиться к простому биграмму
            return BigramFallback(currentBpe);
        }

        // Вернуть наиболее часто встречающийся следующий токен
        return nextTokenCounts.OrderByDescending(kv => kv.Value).First().Key;
    }
}
```

**Ожидание:** добавление ast_type и depth к контексту должно снизить перплексию на 15–30% по сравнению с чистым BPE биграммом — потому что "MethodDecl на глубине 3" предсказывает следующий токен лучше чем просто "4821".

---

## Уникальная ось масштабирования: информационная ценность каждого измерения

Каждый тир добавляет ОДНО новое измерение и измеряет насколько оно улучшает перплексию:

### Tier 1 — +ast_type (< 5 секунд)

**Датасет:** `01.Tokenizer`  
**Новое измерение:** ast_node_type  
**Вопрос:** насколько знание типа узла улучшает биграмм?  
**Ожидание:** -15–25% перплексии (словарь 300 BPE → 73 AST типа, из E01 образца)

---

### Tier 2 — +ast_parent (< 20 секунд)

**Датасет:** 6 подпроектов  
**Новое измерение:** добавить ast_parent_type к контексту  
**Вопрос:** знание родительского узла помогает больше чем просто тип текущего?  
**Ожидание:** ещё -5–15% перплексии (родительский контекст несёт дополнительную информацию)

---

### Tier 3 — +depth + file_id (< 2 минуты)

**Датасет:** все подпроекты  
**Новые измерения:** ast_depth + file_id  
**Вопрос:** зная из какого файла токен и как глубоко — улучшает ли это предсказание?  
**Ожидание:** файл улучшает для кросс-файловых паттернов; глубина помогает для вложенных структур  
**Важный тест:** информационный прирост каждого измерения отдельно

---

### Tier 4 — +data_flow + pagerank (< 10 минут)

**Датасет:** все подпроекты + граф вызовов  
**Новые измерения:** data_flow_in + pagerank_score  
**Вопрос:** это самые дорогие измерения (нужен граф Roslyn) — оправдывают ли они себя?  
**Ожидание:** data_flow даёт хорошее улучшение для вызовов методов; pagerank помогает меньше

---

### Tier 5 — структурное батчирование (< 30 минут)

**Датасет:** все подпроекты  
**Изменение:** применить вариант C (структурно-направленные батчи)  
**Вопрос:** помещение структурно-близких токенов рядом ускоряет ли обучение?  
**Ожидание:** да — перплексия после 100 шагов должна быть ниже чем при линейном батчировании

---

## Информационный вклад каждого измерения (таблица для заполнения)

| Измерение | Откуда берём | Размер таблицы | Ожидаемый прирост перплексии |
|---|---|---|---|
| bpe_id (baseline) | `01.Tokenizer` | vocab × hidden | — (базовая линия) |
| + ast_type | Roslyn SyntaxKind | 200 × hidden | -15–25% |
| + ast_parent | Roslyn родительский узел | 200 × hidden | -5–15% |
| + depth | Roslyn глубина | 20 × hidden | -3–8% |
| + file_id | нумерация файлов | num_files × hidden | -5–12% |
| + scope_depth | Roslyn {-скобки} | 15 × hidden | -2–5% |
| + data_flow_in | Roslyn граф вызовов | через проекцию | -5–15% |
| + pagerank | `07.GraphEngine` | скаляр | -2–8% |
| **Итого** | | | **-35–80% от baseline** |

---

## Связь с другими экспериментами и планами

**E01 (AST)** — поставляет ast_type и ast_parent  
**E06 (PageRank)** — поставляет pagerank_score  
**E04 (BM25)** — аналог: найти похожий контекст vs поместить похожий контекст рядом при обучении  
**X-03 (астроцитарный слой)** — медленный слой наблюдает какие измерения наиболее активны → обновляет топологию  
**X-01 Кирпич D (разреженное внимание)** — маска внимания может строиться из data_flow рёбер  

**Ключевое отличие от E01–E06:** те эксперименты улучшают предсказание одним методом. E07 объединяет все улучшения в единый обогащённый токен и тестирует их суммарный эффект.

---

## JSON результат (шаблон)

```json
{
  "meta": {
    "experiment_id": "E07-STRUCTURAL-CONTEXT-TOKEN-v1",
    "hypothesis": "Token embedding enriched with ast_type, parent, depth, file_id, data_flow reduces perplexity by 35-80% vs flat BPE",
    "variant": "A_additive_embedding",
    "run_timestamp_utc": null,
    "tier": 1,
    "dimension_added_this_tier": "ast_type"
  },
  "dataset": {
    "tier": 1,
    "files_count": null,
    "tokens_count": null
  },
  "dimensions_tested": {
    "baseline": { "fields": ["bpe_id"], "perplexity": null },
    "plus_ast_type": { "fields": ["bpe_id", "ast_type"], "perplexity": null, "delta_pct": null },
    "plus_parent": { "fields": ["bpe_id", "ast_type", "ast_parent"], "perplexity": null, "delta_pct": null },
    "plus_depth": { "fields": ["bpe_id", "ast_type", "ast_parent", "depth"], "perplexity": null, "delta_pct": null },
    "plus_file": { "fields": ["bpe_id", "ast_type", "ast_parent", "depth", "file_id"], "perplexity": null, "delta_pct": null }
  },
  "resources": {
    "time_ms": null,
    "memory_peak_mb": null,
    "cpu_avg_pct": null,
    "disk_write_bytes": null
  },
  "scaling_signal": {
    "most_valuable_dimension": null,
    "diminishing_returns_after": null,
    "note": "which dimension gives the most perplexity reduction for its embedding table cost?"
  },
  "verdict": {
    "passed": null,
    "next_action": null,
    "notes": ""
  }
}
```

---

## MD результат (шаблон)

```markdown
# E07 — Структурный контекст-токен · Tier [N] · вариант [A/B/C]
Запущено: [дата] · [N] файлов · измерение этого тира: [dimension]

## Информационный вклад каждого измерения

| Измерение | Перплексия | Δ от baseline | Таблица (строк) | Стоит ли? |
|---|---|---|---|---|
| Baseline BPE | — | — | vocab | — |
| + ast_type | — | —% | 200 | ✅/❌ |
| + ast_parent | — | —% | 200 | ✅/❌ |
| + depth | — | —% | 20 | ✅/❌ |
| + file_id | — | —% | N файлов | ✅/❌ |
| + data_flow | — | —% | проекция | ✅/❌ |

## Самое ценное измерение этого тира: [dimension]
Обоснование: [почему именно оно]

## Вариант C (структурное батчирование): [запущен / не запущен]
Результат: [перплексия после 100 шагов линейное / структурное батчирование]

## Следующий шаг: [добавить следующее измерение / PROMOTE / TUNE]
```

---

*E07 plan v1.0 — enriched structural context token · не касается XIIGen*
