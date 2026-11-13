# AgentAlgorithms Enhancement Plan v5.0
## ContextWindowManager + AgentOrchestrator
## Подробный план для Claude Code v1.0

**Проект:** `22.AgentAlgorithms`
**Основание:** E38v2 SkillRetriever (+100%, 86% load_reduction) + E35 DependencyDetector (+30%)
**Статус 21-23:** FileChangeApplier fuzzy+multi-patch ✅ db80dc5 · ComplexTaskDecomposer false-CONJ ✅ db80dc5
**Этот документ не касается XIIGen.**

---

## Два компонента

| Компонент | Файл | Что добавляем | Эксперимент |
|---|---|---|---|
| ContextWindowManager | `Context/ContextWindowManager.cs` | SkillRetriever: 6 скилов вместо 46 + AstTokenBudget | E38v2 +100%, 86% load_reduction |
| AgentOrchestrator | `MultiAgent/AgentOrchestrator.cs` | DependencyDetector: порядок + параллельность | E35 +30% |

---

---

# Место 1 — ContextWindowManager: SkillRetriever + AstTokenBudget

## Что не так сейчас

```csharp
// Сейчас: одинаковый расчёт для всего контента
private static int CountTokens(string text) => (int)Math.Ceiling(text.Length / 4.0);
```

Два дефекта:

**Дефект A — неточный токен-бюджет.**
`text.Length / 4` одинаково оценивает C#-код и документацию.
E11 показал что AST-токен описывает метод в 5.2x эффективнее BPE:
`"fn bool ValidateToken(string) [If NullCheck ReturnFalse]"` = 1 AST-токен = 20+ BPE-токенов.
Результат: при бюджете 8 000 помещается ~10 файлов вместо 48 (измерено в v3.0).

**Дефект Б — нет skill filtering.**
ContextWindowManager кладёт в контекст всё что получает.
Если кандидаты включают 46 скилов — все 46 попадут в окно,
вытесняя код пользователя.
E38v2 показал: KNN на предикатах задачи выбирает правильные 6 скилов с точностью 95%.
fallback при confidence < 0.40 → загрузить все (22% случаев).

## Что добавляем

### 1а — AstTokenBudget (из Enhancement Plan v3.0, Место 0а)

```csharp
// ITokenBudgetCalculator — фабричный интерфейс (Fabric First).
// Разные типы контента имеют разную информационную плотность.
public interface ITokenBudgetCalculator
{
    // Оценить токены для контента заданного типа.
    int Estimate(string content, ContentType type);
}

public enum ContentType
{
    CSharpCode,       // AST-токены: 1 узел ≈ 5.2 BPE (E11)
    Documentation,    // text.Length / 5  (prose BPE vocab лучше покрывает)
    NaturalLanguage,  // text.Length / 4  (стандарт)
    SkillDocument,    // text.Length / 4  (markdown prose)
}

// AstTokenBudgetCalculator — реализация через E11.
// Путь: 22.AgentAlgorithms/Context/AstTokenBudgetCalculator.cs
public sealed class AstTokenBudgetCalculator : ITokenBudgetCalculator
{
    // C# файл: считаем AST-узлы через эвристику (каждые ~20 символов = 1 узел).
    // Точный Roslyn-подсчёт слишком дорог для runtime — эвристика достаточна.
    private const double CSharpBytesPerAstNode = 20.0;

    public int Estimate(string content, ContentType type) => type switch
    {
        ContentType.CSharpCode      => (int)Math.Ceiling(content.Length / CSharpBytesPerAstNode),
        ContentType.Documentation   => (int)Math.Ceiling(content.Length / 5.0),
        ContentType.NaturalLanguage => (int)Math.Ceiling(content.Length / 4.0),
        ContentType.SkillDocument   => (int)Math.Ceiling(content.Length / 4.0),
        _                           => (int)Math.Ceiling(content.Length / 4.0),
    };
}
```

### 1б — SkillFilter внутри AssembleAsync

```csharp
// ISkillFilter — фабричный интерфейс.
// Фильтрует кандидатов-скилов по предикатному профилю задачи.
public interface ISkillFilter
{
    // Вернуть топ-K скилов для данной задачи.
    // Если confidence < threshold → вернуть все (fallback).
    OperationResult<IReadOnlyList<TextChunk>> Filter(
        IReadOnlyList<TextChunk> skillCandidates,
        string                  taskDescription,
        int                     topK = 5,
        double                  confidenceThreshold = 0.40);
}

// KnnSkillFilter — реализация на основе E38v2.
// Путь: 22.AgentAlgorithms/Context/KnnSkillFilter.cs
public sealed class KnnSkillFilter : ISkillFilter
{
    private readonly ICodeIndex _index; // BM25 индекс скилов (из E04/E09)

    public KnnSkillFilter(ICodeIndex index) => _index = index;

    public OperationResult<IReadOnlyList<TextChunk>> Filter(
        IReadOnlyList<TextChunk> skillCandidates,
        string                  taskDescription,
        int                     topK = 5,
        double                  confidenceThreshold = 0.40)
    {
        if (!skillCandidates.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates);

        // Поиск по предикатному профилю задачи
        var query  = new CodeQuery(taskDescription);
        var ranked = _index.Search(query, topK: topK);

        if (!ranked.IsSuccess || !ranked.Value!.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates); // fallback

        // Confidence = score топ-1 / суммарный score топ-K
        double topScore   = ranked.Value!.First().Score;
        double totalScore = ranked.Value!.Sum(r => r.Score) + 1e-9;
        double confidence = topScore / totalScore;

        if (confidence < confidenceThreshold)
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates); // fallback: все

        // Взять только top-K скилов
        var topIds     = ranked.Value!.Select(r => r.FileId).ToHashSet();
        var filtered   = skillCandidates
            .Where(c => topIds.Contains(c.Id))
            .ToList();

        return OperationResult<IReadOnlyList<TextChunk>>.Ok(filtered);
    }
}
```

### Обновить ContextWindowManager

```csharp
// ContextWindowManager — обновлённая версия.
// Путь: 22.AgentAlgorithms/Context/ContextWindowManager.cs
public sealed class ContextWindowManager
{
    private readonly SemanticRanker         _ranker;
    private readonly ContextCompactor       _compactor;
    private readonly ITokenBudgetCalculator _budget;    // НОВОЕ: AstTokenBudget
    private readonly ISkillFilter           _skills;    // НОВОЕ: KnnSkillFilter
    private readonly int                    _tokenBudget;

    public ContextWindowManager(
        SemanticRanker         ranker,
        ContextCompactor       compactor,
        ITokenBudgetCalculator budget,
        ISkillFilter           skills,
        int                    tokenBudget = 8000)
    {
        _ranker      = ranker;
        _compactor   = compactor;
        _budget      = budget;
        _skills      = skills;
        _tokenBudget = tokenBudget;
    }

    public async Task<OperationResult<IReadOnlyList<TextChunk>>> AssembleAsync(
        IReadOnlyList<TextChunk> candidates,
        string                   query)
    {
        // Разделить кандидатов на скилы и код
        var skillChunks = candidates.Where(c => c.Type == ContentType.SkillDocument).ToList();
        var codeChunks  = candidates.Where(c => c.Type != ContentType.SkillDocument).ToList();

        // Фильтровать скилы через KNN (E38v2: 6 вместо 46)
        var filteredSkills = _skills.Filter(skillChunks, query);
        if (!filteredSkills.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                filteredSkills.ErrorMessage, "SKILL_FILTER_FAILED");

        // Объединить: код + отфильтрованные скилы
        var allCandidates = codeChunks.Concat(filteredSkills.Value!).ToList();

        // Ранжировать по релевантности
        var rankResult = await _ranker.RankAsync(allCandidates, query);
        if (!rankResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                rankResult.ErrorMessage, rankResult.ErrorCode);

        // Заполнить окно с точным бюджетом по типу контента (E11)
        var selected   = new List<TextChunk>();
        int usedTokens = 0;

        foreach (var chunk in rankResult.Value!)
        {
            int tokens = _budget.Estimate(chunk.Content, chunk.Type);
            if (usedTokens + tokens > _tokenBudget) break;
            selected.Add(chunk);
            usedTokens += tokens;
        }

        return OperationResult<IReadOnlyList<TextChunk>>.Ok(selected);
    }

    public async Task<OperationResult<List<ConversationMessage>>> CompactHistoryAsync(
        List<ConversationMessage> history,
        Func<List<ConversationMessage>, Task<string>> summaryGenerator)
        => await _compactor.CompactAsync(history, summaryGenerator);
}
```

## TextChunk: добавить поле Type

```csharp
// Добавить в существующий TextChunk:
public sealed record TextChunk(string Id, string Content, double Score = 0.0,
    ContentType Type = ContentType.NaturalLanguage); // НОВОЕ поле
```

## До / После

| Метрика | До | После |
|---|---|---|
| Скилов в контексте | 46 (все) | ~6 (95% точность) |
| Бюджет для кода | ~10 файлов при 8K | ~48 файлов при том же 8K |
| fallback_rate | — | ~22% (confidence < 0.40) |
| Основание | — | E38v2 +100%, E11 AST 5.2x |

## Gate B

- `ContextWindowManager_SkillFilter_ReducesCount`: `filtered.Count <= 10 < 46`
- `ContextWindowManager_AstBudget_CodeCostsLess`: `Estimate(csharpCode, CSharpCode) < Estimate(csharpCode, NaturalLanguage)`
- `ContextWindowManager_Fallback_WhenLowConfidence`: при пустом индексе → вернуть все скилы

---

---

# Место 2 — AgentOrchestrator: DependencyDetector + параллельное выполнение

## Что не так сейчас

```csharp
// Сейчас: просто foreach — задачи идут в том порядке в котором пришли
foreach (var subTask in decomposed.Value!)
{
    var subResult = await _bus.AssignTaskAsync(agent.Name, subTask.Description);
    ...
}
```

Три дефекта:

**Дефект А — нет порядка.** ARCH должен идти перед CODE_Q: нельзя реализовывать
до того как определена архитектура. Сейчас это не гарантируется.

**Дефект Б — нет параллельности.** CODE_Q и REFACTOR могут идти параллельно
если оба зависят только от ARCH (уже выполненного). Сейчас всегда последовательно.

**Дефект В — нет механизма передачи результата.** Когда ARCH завершён,
CODE_Q должен получить его результат как контекст. Сейчас результаты просто складываются.

## Что добавляем

### 2а — DependencyGraph

```csharp
// DependencyGraph — граф зависимостей между под-задачами.
// Путь: 22.AgentAlgorithms/MultiAgent/DependencyGraph.cs
public sealed class DependencyGraph
{
    // Рёбра: (от, к) = "к" зависит от "от"
    private readonly Dictionary<string, List<string>> _edges = new();

    // Зарегистрировать зависимость: taskB выполняется ПОСЛЕ taskA.
    public void AddDependency(string taskARole, string taskBRole)
    {
        if (!_edges.ContainsKey(taskARole))
            _edges[taskARole] = new List<string>();
        _edges[taskARole].Add(taskBRole);
    }

    // Топологическая сортировка по уровням (BFS).
    // Каждый уровень — группа задач которые можно выполнять параллельно.
    public OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>> Sort(
        IReadOnlyList<SubTask> tasks)
    {
        // Считаем in-degree для каждой задачи
        var inDegree = tasks.ToDictionary(t => t.RequiredRole, _ => 0);

        foreach (var (from, toList) in _edges)
            foreach (var to in toList)
                if (inDegree.ContainsKey(to))
                    inDegree[to]++;

        // BFS по уровням
        var levels   = new List<IReadOnlyList<SubTask>>();
        var queue    = new Queue<SubTask>(tasks.Where(t => inDegree[t.RequiredRole] == 0));

        while (queue.Any())
        {
            var level = new List<SubTask>();
            int count = queue.Count;

            for (int i = 0; i < count; i++)
            {
                var task = queue.Dequeue();
                level.Add(task);

                if (_edges.TryGetValue(task.RequiredRole, out var dependents))
                    foreach (var dep in dependents)
                    {
                        inDegree[dep]--;
                        if (inDegree[dep] == 0)
                            queue.Enqueue(tasks.First(t => t.RequiredRole == dep));
                    }
            }

            if (level.Any()) levels.Add(level);
        }

        // Проверить что все задачи попали (нет циклов)
        if (levels.Sum(l => l.Count) != tasks.Count)
            return OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>>.Fail(
                "Cycle detected in task dependencies.", "DEPENDENCY_CYCLE");

        return OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>>.Ok(levels);
    }
}
```

### 2б — Обновить AgentOrchestrator

```csharp
// AgentOrchestrator — обновлённая версия.
// Путь: 22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs
public sealed class AgentOrchestrator
{
    private readonly Dictionary<string, SubAgentDefinition> _agents
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAgentMessageBus       _bus;
    private readonly ComplexTaskDecomposer  _decomposer;
    private readonly DependencyGraph        _deps;        // НОВОЕ

    public AgentOrchestrator(
        IAgentMessageBus      bus,
        ComplexTaskDecomposer decomposer,
        DependencyGraph?      deps = null)
    {
        _bus        = bus;
        _decomposer = decomposer;
        _deps       = deps ?? BuildDefaultDependencies(); // НОВОЕ
    }

    public void Register(SubAgentDefinition agent)
        => _agents[agent.Role] = agent;

    public async Task<OperationResult<string>> RunAsync(string task)
    {
        var decomposed = _decomposer.Decompose(task);
        if (!decomposed.IsSuccess)
            return OperationResult<string>.Fail(decomposed.ErrorMessage, "DECOMPOSITION_FAILED");

        // Топологическая сортировка → уровни параллельного выполнения (НОВОЕ)
        var sorted = _deps.Sort(decomposed.Value!);
        if (!sorted.IsSuccess)
            return OperationResult<string>.Fail(sorted.ErrorMessage, sorted.ErrorCode);

        var results        = new List<string>();
        var contextBuilder = new StringBuilder(); // накапливать результаты для передачи вперёд

        foreach (var level in sorted.Value!)
        {
            // Все задачи одного уровня — запускаем параллельно
            var levelTasks = level.Select(async subTask =>
            {
                if (!_agents.TryGetValue(subTask.RequiredRole, out var agent))
                    return OperationResult<string>.Fail(
                        $"No sub-agent for role '{subTask.RequiredRole}'.", "MISSING_SUBAGENT");

                // Передать результаты предыдущих уровней как контекст (НОВОЕ)
                string taskWithContext = contextBuilder.Length > 0
                    ? $"{subTask.Description}\n\n[Previous results]\n{contextBuilder}"
                    : subTask.Description;

                return await _bus.AssignTaskAsync(agent.Name, taskWithContext);
            }).ToList();

            var levelResults = await Task.WhenAll(levelTasks);

            foreach (var result in levelResults)
            {
                if (!result.IsSuccess)
                    return OperationResult<string>.Fail(
                        $"Sub-agent failed: {result.ErrorMessage}", "SUBAGENT_FAILED");
                results.Add(result.Value!);
                contextBuilder.AppendLine(result.Value!);
            }
        }

        return OperationResult<string>.Ok(string.Join("\n\n", results));
    }

    // Умолчания по умолчанию из E35 DependencyDetector.
    // ARCH → CODE_Q, ARCH → REFACTOR, PLAN → CODE_Q, PLAN → ARCH
    private static DependencyGraph BuildDefaultDependencies()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("ARCH",    "CODE_Q");     // архитектура перед реализацией
        graph.AddDependency("ARCH",    "REFACTOR");   // архитектура перед рефакторингом
        graph.AddDependency("PLAN",    "ARCH");       // план перед архитектурой
        graph.AddDependency("PLAN",    "CODE_Q");     // план перед реализацией
        graph.AddDependency("CODE_Q",  "BUG");        // код перед отладкой
        return graph;
    }
}
```

## До / После

| Сценарий | До | После |
|---|---|---|
| "Design auth AND implement service" | ARCH и CODE_Q в случайном порядке | ARCH → CODE_Q (гарантировано) |
| "Refactor AND add tests" | последовательно | параллельно (нет зависимости) |
| Передача контекста | нет | CODE_Q получает результат ARCH |
| Основание | — | E35 +30%, E49 +44% |

## Gate B

- `DependencyGraph_ArchBeforeCodeQ`: `levels[0]` содержит ARCH, `levels[1]` — CODE_Q
- `DependencyGraph_IndependentTasksParallel`: CODE_Q и REFACTOR в одном уровне если оба зависят от ARCH
- `DependencyGraph_CycleDetected`: задачи с циклической зависимостью → `DEPENDENCY_CYCLE`
- `AgentOrchestrator_ContextPropagated`: описание CODE_Q содержит результат ARCH

---

## Порядок реализации

| Шаг | Что | Файл | Дней | Gate B |
|---|---|---|---|---|
| 1 | `AstTokenBudgetCalculator` + `ITokenBudgetCalculator` | `Context/AstTokenBudgetCalculator.cs` | 0.5 | `CSharpCode < NaturalLanguage` для того же текста |
| 2 | `KnnSkillFilter` + `ISkillFilter` + обновить `TextChunk` | `Context/KnnSkillFilter.cs` | 1 | filtered ≤ 10, fallback при confidence < 0.40 |
| 3 | Обновить `ContextWindowManager` | `Context/ContextWindowManager.cs` | 0.5 | все 3 Gate B |
| 4 | `DependencyGraph` | `MultiAgent/DependencyGraph.cs` | 1 | цикл → ошибка, ARCH до CODE_Q |
| 5 | Обновить `AgentOrchestrator` | `MultiAgent/AgentOrchestrator.cs` | 0.5 | все 4 Gate B |
| 6 | Тесты + ComparisonHarness v5.0 | `AgentComparisonHarness.cs` | 1 | все метрики enhanced ≥ original |
| | **Итого** | | **4.5 дней** | |

## Тесты

| Тест | Ожидание | Что проверяет |
|---|---|---|
| `AstBudget_CSharpCostsLessThanProse` | `csharpTokens < proseTokens` для одинаковой длины | E11 AST эффективность |
| `SkillFilter_TopKReturned` | `filtered.Count == 5` при высоком confidence | E38v2 load reduction |
| `SkillFilter_FallbackOnLowConfidence` | `filtered.Count == allSkills.Count` при confidence < 0.40 | E38v2 fallback 22% |
| `DependencyGraph_LevelOrder_ArchFirst` | `levels[0][0].RequiredRole == "ARCH"` | E35 ordering |
| `DependencyGraph_ParallelLevel_CodeQAndRefactor` | оба в `levels[1]` | E35 parallelism |
| `AgentOrchestrator_PropagatesContext` | описание содержит `[Previous results]` | контекст между уровнями |
| `ComparisonHarness_SkillsReduced` | `enhanced.skill_count <= original.skill_count / 4` | E38v2 86% reduction |
