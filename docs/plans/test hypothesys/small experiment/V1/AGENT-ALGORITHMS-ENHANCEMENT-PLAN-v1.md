# 21/22/23 AgentAlgorithms — План: оригинал vs улучшения из экспериментов
## Пять мест + сравнительный тест с оценкой LLM

**Проекты:** llm-mvp-core — подпроекты 21.AgentInfrastructure, 22.AgentAlgorithms, 23.OssEquivalents  
**Этот документ не касается XIIGen.**

---

## Главная идея

Строим два варианта агента рядом:

```
AgentOriginal   — как есть: embedding API, text/4 токены, conjunction split
AgentEnhanced   — наши алгоритмы: AST ранжирование, AST упаковка, Roslyn валидация

ComparisonHarness — запускает одну и ту же задачу через оба варианта,
                    записывает метрики, просит LLM оценить качество контекста
```

Оба варианта реализуют одни и те же интерфейсы — подмена без изменений в AgentLoop.

---

## Пять мест улучшения

### Место 0 — ContextWindowManager: AST упаковка (новое, самое важное)

**Файл:** `22.AgentAlgorithms/Context/AstPackedContextBuilder.cs`

**Проблема:**  
`CountTokens(text) = text.Length / 4` — подсчёт по символам.  
В контекст идёт сырой C# текст. Бюджет 8 000 токенов = ~32 000 символов кода.

**Что показали эксперименты:**

```
E06 Scale 2 результат:
  BPE:  4.056 бит/символ
  AST:  0.510 бит/символ
  Соотношение: 8.0x

Смысл: AST-узлы несут в 8 раз больше структурной информации на единицу.
При том же бюджете токенов в контекст влезет в 8 раз больше кода.
```

**Enhanced:** вместо сырого кода отправлять в контекст AST-представление:

```
Оригинал (в контекст идёт):
  "public bool ValidateToken(string token) {\n    if (token == null) return false;\n    return token.Length > 0;\n}"
  = 81 символ = ~20 BPE токенов

AST-упакованный (в контекст идёт):
  "MethodDecl[bool ValidateToken(string)] IfStmt NullCheck ReturnFalse ReturnBoolExpr"
  = 83 символа = ~21 BPE токен, но описывает СТРУКТУРУ не текст

Эффект: в тот же бюджет 8000 токенов влезает описание в 8x большего количества методов.
LLM получает карту всего проекта вместо первых 50 файлов.
```

**Реализация:**

```csharp
// AstPackedContextBuilder.cs
// Строит компактное AST-представление файла для включения в контекст LLM.
// Каждый метод = одна строка вида: "MethodDecl[сигнатура] -> [список типов узлов тела]"
public sealed class AstPackedContextBuilder
{
    // Построить компактное представление файла.
    // Результат: строка длиной в 5-10x меньше исходного кода, сохраняет структуру.
    public string Pack(string csharpSource, string filePath)
    {
        var tree = CSharpSyntaxTree.ParseText(csharpSource);
        var root = tree.GetRoot();
        var sb   = new StringBuilder();

        sb.AppendLine($"// {Path.GetFileName(filePath)}");

        // Каждый класс и метод — отдельная строка
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            // Сигнатура: "ReturnType MethodName(params)"
            string sig = $"{method.ReturnType} {method.Identifier}({string.Join(",", method.ParameterList.Parameters.Select(p => p.Type))})";

            // Список уникальных AST типов в теле метода (не тексте, а структуре)
            var bodyKinds = method.Body?
                .DescendantNodes()
                .Select(n => AbbreviateKind(n.Kind().ToString()))
                .Distinct()
                .ToList() ?? [];

            sb.AppendLine($"  fn {sig} [{string.Join(" ", bodyKinds)}]");
        }

        return sb.ToString();
    }

    // Сократить длинные SyntaxKind имена для компактности
    // "MethodDeclaration" → "MDecl", "IfStatement" → "If", "ReturnStatement" → "Ret"
    private static string AbbreviateKind(string kind) => kind switch
    {
        "MethodDeclaration"   => "MDecl",
        "IfStatement"         => "If",
        "ReturnStatement"     => "Ret",
        "ForStatement"        => "For",
        "InvocationExpression" => "Call",
        "AssignmentExpression" => "Assign",
        _ => kind.Length > 6 ? kind[..6] : kind
    };
}
```

**Измерение:** `tokens_in_original_context` vs `tokens_in_ast_context` vs `files_covered`.

---

### Место 1 — SemanticRanker: AST Jaccard вместо embedding API

**Файл:** `22.AgentAlgorithms/Context/AstRelevanceRanker.cs`  
**Реализует:** `ISemanticSearchProvider` — тот же интерфейс, подмена без изменений в ContextWindowManager

**Оригинал:** `ISemanticSearchProvider.SearchAsync(query)` → вызов embedding API → cosine similarity.

**Enhanced:**

```csharp
// AstRelevanceRanker.cs — ISemanticSearchProvider без вызова LLM
public sealed class AstRelevanceRanker : ISemanticSearchProvider
{
    // chunkId → набор SyntaxKind типов из этого чанка кода
    private readonly Dictionary<string, HashSet<string>> _index = new();

    // Добавить файл в индекс (вызывается при индексации)
    public void Index(string chunkId, string csharpCode)
    {
        _index[chunkId] = CSharpSyntaxTree.ParseText(csharpCode)
            .GetRoot().DescendantNodes()
            .Select(n => n.Kind().ToString())
            .ToHashSet();
    }

    // Ранжировать чанки по AST-сходству с запросом — без LLM
    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(
        string query, int topK)
    {
        var queryKinds = ExtractKindsFromQuery(query);

        return Task.FromResult(OperationResult<List<(string, double)>>.Ok(
            _index
                .Select(kv => (kv.Key, Jaccard(queryKinds, kv.Value)))
                .OrderByDescending(x => x.Item2)
                .Take(topK)
                .ToList()));
    }

    // Jaccard: |A ∩ B| / |A ∪ B|
    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    // Извлечь AST-типы из текста запроса пользователя
    // "добавь метод GetUser" → {MethodDeclaration, IdentifierName}
    private static HashSet<string> ExtractKindsFromQuery(string query)
    {
        try
        {
            var kinds = CSharpSyntaxTree.ParseText(query)
                .GetRoot().DescendantNodes()
                .Select(n => n.Kind().ToString())
                .ToHashSet();
            if (kinds.Count > 2) return kinds;
        }
        catch { }

        // Текстовый запрос: слова → SyntaxKind по словарю
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["method"]    = "MethodDeclaration",
            ["class"]     = "ClassDeclaration",
            ["return"]    = "ReturnStatement",
            ["interface"] = "InterfaceDeclaration",
            ["if"]        = "IfStatement",
        };
        return query.Split(' ')
                    .Where(w => map.ContainsKey(w))
                    .Select(w => map[w])
                    .ToHashSet();
    }
}
```

---

### Место 2 — CodebaseIndexer: PageRank для отбора файлов в контекст

**Файл:** `21.AgentInfrastructure/Indexing/PageRankFileSelector.cs`

**Оригинал:** `SymbolGraph.ComputePageRank()` уже считается — но только для сортировки символов в RepoMap.  
Файлы в контекст отбираются без учёта PageRank.

**Enhanced:** добавить метод `SelectFilesForContext()` который использует уже посчитанный PageRank:

```csharp
// PageRankFileSelector.cs — использует SymbolGraph который уже построен
public sealed class PageRankFileSelector
{
    private readonly SymbolGraph _graph;  // уже построен CodebaseIndexer.IndexAsync()

    // Топ-N файлов по максимальному PageRank их символов
    // Это "архитектурное ядро" — всегда включать в контекст первыми
    public IReadOnlyList<string> SelectCoreFiles(
        IReadOnlyList<string> allFiles, int topN = 5)
    {
        return allFiles
            .Select(file => (
                File: file,
                Score: _graph.AllNodes
                    .Where(n => n.FilePath == file)
                    .Select(n => _graph.GetScore(n.Name))
                    .DefaultIfEmpty(0).Max()))
            .OrderByDescending(x => x.Score)
            .Take(topN)
            .Select(x => x.File)
            .ToList();
    }
}
```

---

### Место 3 — StructuredLlmClient: Roslyn валидация без второго вызова LLM

**Файл:** `23.OssEquivalents/Validation/RoslynSyntaxValidator.cs`

**Оригинал:** `StructuredLlmClient` проверяет JSON-схему (поля, типы) — но когда в JSON  
есть поле `"code": "public bool Foo() { return }"` — синтаксис C# не проверяется.  
Если код невалидный → агент применяет его → компиляция падает → ещё один вызов LLM.

**Enhanced:** добавить Roslyn проверку кодовых полей перед retry:

```csharp
// RoslynSyntaxValidator.cs
// Проверяет C# синтаксис в строковых полях JSON-ответа LLM.
// E05 показал: 99.5% синтаксически корректных предсказаний с Roslyn маской.
public sealed class RoslynSyntaxValidator
{
    // Проверить все строковые поля в ответе которые похожи на C# код
    // Возвращает список полей с синтаксическими ошибками
    public IReadOnlyList<string> FindInvalidCodeFields(
        Dictionary<string, object> llmResponse)
    {
        var invalid = new List<string>();

        foreach (var (key, value) in llmResponse)
        {
            if (value is not string str) continue;
            if (!LooksLikeCSharpCode(str)) continue;

            var tree   = CSharpSyntaxTree.ParseText(str);
            var errors = tree.GetDiagnostics()
                             .Where(d => d.Severity == DiagnosticSeverity.Error)
                             .ToList();

            if (errors.Count > 0)
                invalid.Add($"{key}: {errors[0].GetMessage()}");
        }

        return invalid;
    }

    // Эвристика: строка содержит C# код если есть { } и ключевые слова
    private static bool LooksLikeCSharpCode(string s)
        => s.Contains('{') && s.Contains('}') &&
           (s.Contains("public ") || s.Contains("private ") ||
            s.Contains("class ") || s.Contains("return "));
}
```

**Интеграция:** в `StructuredLlmClient.SendAsync()` перед отправкой retry добавить:

```csharp
var syntaxErrors = _syntaxValidator.FindInvalidCodeFields(parsed);
if (syntaxErrors.Count > 0)
{
    // Добавить конкретные ошибки в retry message — не тратить токены на общий "попробуй снова"
    errorMessage = $"Syntax errors in generated code: {string.Join("; ", syntaxErrors)}";
    // Retry с конкретным сообщением об ошибке
}
```

---

### Место 4 — ComplexTaskDecomposer: NgramTaskDecomposer

**Файл:** `22.AgentAlgorithms/Tasks/NgramTaskDecomposer.cs`

**Оригинал:** разбивка на подзадачи по conjunctions `"and"`, `"then"`, `"also"`.  
Проблема: `"Find methods that return null and log them"` → неверно разбивается.

**Enhanced:** модель биграмм из E07 (обученная на Alice + docs) определяет  
границы подзадач по падению вероятности биграммы.

(Детали реализации в плане E07 из 34.Experiments)

---

## Часть 2 — Сравнительный тест

### ComparisonHarness

**Файл:** `22.AgentAlgorithms/Comparison/AgentComparisonHarness.cs`

Запускает одну задачу через оба варианта и собирает метрики:

```csharp
public sealed record ComparisonResult
{
    // Оригинальный вариант
    public int    OriginalTokensUsed       { get; init; }  // токенов в контексте
    public int    OriginalFilesInContext    { get; init; }  // файлов включено
    public int    OriginalLlmCallCount     { get; init; }  // вызовов LLM сделано
    public double OriginalRankingTimeMs    { get; init; }  // время ранжирования

    // Enhanced вариант
    public int    EnhancedTokensUsed       { get; init; }
    public int    EnhancedFilesInContext   { get; init; }
    public int    EnhancedLlmCallCount     { get; init; }  // должно быть меньше
    public double EnhancedRankingTimeMs    { get; init; }

    // AST-упаковка
    public int    EnhancedFilesViaPacking  { get; init; }  // сколько файлов влезло через AST
    public double AstPackingRatio          { get; init; }  // в сколько раз больше кода влезло

    // Roslyn валидация
    public int    SyntaxErrorsCaughtBeforeRetry { get; init; }  // сколько ошибок поймали без LLM

    // Контексты (для оценки LLM-судьёй)
    public string OriginalContextText      { get; init; } = "";
    public string EnhancedContextText      { get; init; } = "";
    public string TaskDescription          { get; init; } = "";
}
```

### LlmJudge — просим LLM оценить оба контекста

**Файл:** `22.AgentAlgorithms/Comparison/LlmJudge.cs`

```csharp
// LlmJudge отправляет оба контекста в LLM и просит сравнить их качество.
public sealed class LlmJudge
{
    private readonly ILlmGateway _llm;

    // Оценить два варианта контекста для одной задачи.
    // Возвращает оценку и объяснение от LLM.
    public async Task<OperationResult<JudgmentResult>> JudgeAsync(
        string taskDescription,
        string originalContext,
        string enhancedContext)
    {
        // Промпт для судьи
        string prompt = $"""
            You are evaluating two different contexts prepared for the same coding task.

            TASK: {taskDescription}

            CONTEXT A (original):
            {originalContext}

            CONTEXT B (enhanced, uses AST-packed representation):
            {enhancedContext}

            Compare these two contexts on three criteria:
            1. Relevance — does the context contain the files/methods needed for the task?
            2. Coverage — how much of the relevant codebase is included?
            3. Clarity — is the structural information clear and actionable?

            Answer in JSON:
            {{
              "winner": "A" | "B" | "tie",
              "relevance_winner": "A" | "B" | "tie",
              "coverage_winner": "A" | "B" | "tie",
              "clarity_winner": "A" | "B" | "tie",
              "reasoning": "one paragraph explanation"
            }}
            """;

        var response = await _llm.SendAsync([new("user", prompt)]);
        if (!response.IsSuccess)
            return OperationResult<JudgmentResult>.Fail(response.ErrorMessage);

        // Разобрать JSON-ответ
        return ParseJudgment(response.Value!);
    }
}

public sealed record JudgmentResult
{
    public string Winner           { get; init; } = "";  // "A", "B", или "tie"
    public string RelevanceWinner  { get; init; } = "";
    public string CoverageWinner   { get; init; } = "";
    public string ClarityWinner    { get; init; } = "";
    public string Reasoning        { get; init; } = "";
}
```

### Тестовые задачи (три конкретных)

Для каждой задачи прогоняем оба варианта и записываем `ComparisonResult`:

```
Задача 1 — добавление метода:
  "Add a GetUserById method to the UserService class that takes an int id and returns User"
  Ожидание: Enhanced находит UserService быстрее (PageRank + AST), меньше LLM calls

Задача 2 — рефакторинг:
  "Extract the validation logic from ContextWindowManager into a separate class"
  Ожидание: Enhanced показывает больше связанного кода в том же контексте (AST-упаковка)

Задача 3 — поиск дефекта:
  "Find all methods that don't check for null before accessing properties"
  Ожидание: AST-ранжирование находит методы с NullCheck узлами, embedding API может промахнуться
```

### Формат результата (JSON + MD)

```json
{
  "task": "Add a GetUserById method...",
  "original": {
    "tokens_used": 7840,
    "files_in_context": 12,
    "llm_calls": 2,
    "ranking_ms": 340
  },
  "enhanced": {
    "tokens_used": 7920,
    "files_in_context": 47,
    "llm_calls": 1,
    "ranking_ms": 8,
    "files_via_ast_packing": 47,
    "ast_packing_ratio": 3.9,
    "syntax_errors_caught": 1
  },
  "judgment": {
    "winner": "B",
    "coverage_winner": "B",
    "relevance_winner": "tie",
    "reasoning": "Context B covered 4x more files in the same token budget..."
  }
}
```

---

## Структура новых файлов

| Файл | Проект | Что делает |
|---|---|---|
| `Context/AstPackedContextBuilder.cs` | 22 | AST-упаковка кода (Место 0) |
| `Context/AstRelevanceRanker.cs` | 22 | AST Jaccard вместо embedding (Место 1) |
| `Indexing/PageRankFileSelector.cs` | 21 | PageRank для отбора файлов (Место 2) |
| `Validation/RoslynSyntaxValidator.cs` | 23 | Roslyn проверка перед retry (Место 3) |
| `Tasks/NgramTaskDecomposer.cs` | 22 | Биграммы вместо conjunction split (Место 4) |
| `Comparison/AgentComparisonHarness.cs` | 22 | Запускает оба варианта |
| `Comparison/ComparisonResult.cs` | 22 | Что сравниваем |
| `Comparison/LlmJudge.cs` | 22 | LLM оценивает оба контекста |

---

## Порядок реализации

```
Шаг 1: AstPackedContextBuilder (Место 0) — 2 дня
        Gate B: packed_context для UserService.cs в 5x меньше токенов чем raw text

Шаг 2: AstRelevanceRanker (Место 1) — 2 дня
        Gate B: SearchAsync без HTTP вызова, < 10 мс на 50 файлов

Шаг 3: PageRankFileSelector (Место 2) — 1 день
        Gate B: SelectCoreFiles возвращает 5 файлов с высоким PageRank

Шаг 4: RoslynSyntaxValidator (Место 3) — 1 день
        Gate B: находит синтаксическую ошибку в тестовом LLM ответе

Шаг 5: ComparisonHarness + LlmJudge — 2 дня
        Gate B: три тестовых задачи прошли, три JSON с judgment записаны
```

---

## Оценка времени

| Шаг | Дней |
|---|---|
| Место 0 — AST упаковка | 2 |
| Место 1 — AST ранжирование | 2 |
| Место 2 — PageRank отбор файлов | 1 |
| Место 3 — Roslyn валидация | 1 |
| Место 4 — Ngram декомпозиция | 1 |
| Harness + LlmJudge | 2 |
| **Итого** | **9 дней** |

---

⛔ STOP — жду одобрения.

*Agent Algorithms Enhancement Plan v1.0 — не касается XIIGen*
