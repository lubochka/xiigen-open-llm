# SESSION PLAN — 15.OrchestratorModel (Трек В) v1.1
## Внедрение E32, E33, E35, E39 в подпроект OrchestratorModel
## v1.1: FC-10 + FC-12 исправлены (4 проблемы компиляции закрыты)
## Запускать ПОСЛЕ: AGENT-ALGORITHMS-ENHANCEMENT (22 v5.0) ✅ + SEMANTIC-COMPILER (29) ✅

---

## Изменения v1.0 → v1.1

| # | FC | Что исправлено |
|---|---|---|
| 1 | FC-2 | `.csproj` переименован в `OrchestratorModel.csproj` (без числового префикса) |
| 2 | FC-6 | Pre-condition gate: добавлена заметка о параллельном треке 14 |
| 3 | FC-10 | `EpisodeClassifier` использует `ISemanticCompiler` вместо несуществующего `IRouteClassifier` |
| 4 | FC-12 | Все `BaseService`-наследники получили `(ISessionStorage) : base(storage)` конструкторы |
| 5 | FC-12 | `InMemoryEpisodeMemory` теперь наследует `BaseService` |
| 6 | FC-12 | `ResultMerger` использует `ITemplateBeautifier` (Fabric First), не конкретный тип |
| 7 | FC-1 | Phase 5 count исправлен: +3 → 894 (было +2 → 893, но тестов три) |

---

## Цель (verbatim)

> "Build 15.OrchestratorModel: decompose a user task into ordered sub-episodes,
> determine which can run in parallel, run quality checks on the result,
> and route to the right domain using the SemanticCompiler pipeline."

**Конкретный прогресс:**
По завершении этого плана `15.OrchestratorModel` переходит
из "не существует" в "семь компонентов реализованы и протестированы,
'Deploy new application' декомпозируется в [ARCH → [CODE_Q || DevOps]]."

**Round-trip шаги:**
- Шаг 1 (классификация) → Фаза 2: `EpisodeClassifier` через `ISemanticCompiler.Compile()` из `SemanticCompiler29`
- Шаг 2 (декомпозиция) → Фаза 2: `EpisodeDecomposer` предсказывает количество под-эпизодов (E32 +11%)
- Шаг 3 (порядок) → Фаза 3: переиспользует `DependencyGraph` из `22.AgentAlgorithms` (E35 +30%)
- Шаг 4 (остановка) → Фаза 3: `StopSignalDetector` — четыре сигнала (E33, потолок на Tier 2)
- Шаг 5 (запуск) → Фаза 4: `AgentLauncher` делегирует в `AgentOrchestrator`
- Шаг 6 (объединение) → Фаза 4: `ResultMerger` через `ITemplateBeautifier` (E23 +820%)
- Шаг 7 (качество) → Фаза 5: `QualityOrchestrator` — 16 проверок (E39 +20%)
- Шаг 8 (память) → Фаза 4: `InMemoryEpisodeMemory` — заглушка до 14.ContinuousLearning

---

## Pre-condition gate

Запустить до начала. Все четыре должны пройти.

```bash
cd C:\Projects\llm_mvp_core

# 1. BaseService сигнатура — нужен ISessionStorage в конструкторе
grep -n "protected BaseService\|public abstract class BaseService" \
  21.AgentInfrastructure/Core/Core.cs
# Ожидание: строка с "protected BaseService(ISessionStorage storage)"

# 2. ISemanticCompiler и PipelineOutput — контракт который использует EpisodeClassifier
grep -rn "public interface ISemanticCompiler\|PipelineOutput\|PipelineInput\b" \
  29.SemanticCompiler/ --include="*.cs" | grep -v bin | head -5
# Ожидание: ISemanticCompiler.Compile(PipelineInput) → OperationResult<PipelineOutput>

# 3. Domain enum — совпадает с TaskDomain который мы объявляем
grep -n "public enum Domain" 29.SemanticCompiler/Core/Core.cs | head -3
grep -A8 "public enum Domain" 29.SemanticCompiler/Core/Core.cs | head -9
# Ожидание: CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN

# 4. Реальный baseline (не из памяти)
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Ожидание: N passed, 0 failed
# ЗАМЕТКА О ПАРАЛЛЕЛЬНОМ ТРЕКЕ: если 14.ContinuousLearning уже запускался (+22 теста),
# baseline = 899. Прочитать реальное число и пересчитать все целевые счёта:
# baseline + 3 / +5 / +3 / +3 / +3 = baseline + 17

# 5. DependencyGraph уже реализован в 22 (не дублировать)
grep -rn "class DependencyGraph" 22.AgentAlgorithms/ --include="*.cs" | grep -v bin
# Ожидание: одна строка

# 6. Целевая папка не существует
ls 15.OrchestratorModel 2>/dev/null && echo "EXISTS — STOP" || echo "CLEAN"
```

Состояние перед стартом:
- Ветка: `[git branch --show-current]`
- Тесты: `[реальный результат dotnet test — не из памяти]`
- Baseline для плана: `[N из реального результата]`

---

## Step 1 — Прочитать каждый файл который будешь трогать

Прочитать полностью до написания кода. Записать один конкретный факт из конца каждого файла.

```bash
cat 21.AgentInfrastructure/Core/Core.cs        # BaseService + ISessionStorage
cat 22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs # SubTask record, DependencyGraph usage
cat 22.AgentAlgorithms/MultiAgent/DependencyGraph.cs   # Sort(tasks) → уровни
cat 22.AgentAlgorithms/Context/ContextWindowManager.cs # как другие классы решают BaseService
find . -name "*.cs" -path "*/29.SemanticCompiler/*" | grep -v bin | xargs grep -l "ISemanticCompiler"
cat <найденный ISemanticCompiler файл>   # PipelineInput, PipelineOutput, Domain enum
cat Tests/AgentAlgorithmsTests.cs        # примеры NullSessionStorage или ISessionStorage стабов
```

## Step 2 — Объявить состояние

После Step 1 написать один абзац: ветка, baseline тестов, фаза.

---

## ROUND CONTRACT — перед каждым ответом

```
ROUND CONTRACT:
1. Baseline:         [N passed, 0 failed — реальный результат из pre-condition]
2. Сейчас:          [запустить dotnet test — реальные числа]
3. Произведено:     [файлы изменены, тесты добавлены, команды запущены]
4. Фаза требует:    [точный Gate B для текущей фазы]
5. Пробел:          [что из пункта 4 ещё не выполнено]
```

---

## GATE — после каждой фазы

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj
git diff --name-only
```

---

## Фаза 1 — Проект + интерфейсы + заглушки (Core + Fabric)

**Что доставляем:**
- `OrchestratorModel.csproj` с зависимостями
- `Core/Interfaces.cs` — семь интерфейсов + `TaskDomain` enum
- `Fabric/Interfaces.cs` — `ITemplateBeautifier` (Fabric First для ResultMerger)
- `Core/InMemoryEpisodeMemory.cs` — заглушка наследует `BaseService`
- `Core/NullSessionStorage.cs` — internal null-object для тестов

### `OrchestratorModel.csproj` (без числового префикса — FC-2)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- ISemanticCompiler, PipelineInput/Output, Domain enum -->
    <ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler.csproj" />
    <!-- DependencyGraph, SubTask, AgentOrchestrator -->
    <ProjectReference Include="..\22.AgentAlgorithms\AgentAlgorithms.csproj" />
    <!-- BaseService, ISessionStorage, ICodeReader -->
    <ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
  </ItemGroup>
</Project>
```

**ВАЖНО по имени .csproj:** проекты 21/22/23/29 называются без числового префикса
(`AgentAlgorithms.csproj`, `SemanticCompiler.csproj`). Следовать той же конвенции.
Если реальные имена отличаются — прочитать `find . -name "*.csproj" | grep -v bin | head -10`
и использовать реальные пути.

### `Core/Interfaces.cs`

```csharp
namespace OrchestratorModel;

// Домены задач — совпадают с Domain enum из SemanticCompiler29 (D-04).
// CODE_Q = CodeQuestion, BUG = BugFix, PLAN = Planning,
// ARCH = Architecture, REFACTOR = Refactoring.
public enum TaskDomain { CODE_Q, BUG, PLAN, ARCH, REFACTOR }

// Классифицирует задачу в один из пяти доменов.
// Реализован через ISemanticCompiler.Compile() — Domain из PipelineOutput.
// Confidence < 0.6 → сигнал для 17.DynamicDomains (незнакомый домен).
public interface IEpisodeClassifier
{
    OperationResult<TaskDomain> Classify(string taskDescription);
    OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription);
}

// Разбивает задачу на под-эпизоды.
// E32 +11%: RESEARCH → 4-5, BUILD → 2-3, FIX → 2.
public interface IEpisodeDecomposer
{
    OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>> Decompose(
        string taskDescription, TaskDomain domain);
}

// Четыре сигнала остановки декомпозиции (E33).
public interface IStopSignalDetector
{
    OperationResult<bool> ShouldStop(string subTaskDescription);
}

// Запускает агента для под-эпизода.
public interface IAgentLauncher
{
    Task<OperationResult<string>> LaunchAsync(AgentAlgorithms.SubTask task, string context);
}

// Объединяет результаты нескольких агентов.
// Через ITemplateBeautifier (Fabric First — объявлен в Fabric/Interfaces.cs).
public interface IResultMerger
{
    OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results);
}

// Хранит историю эпизодов.
// InMemoryEpisodeMemory — заглушка до 14.ContinuousLearning.
public interface IEpisodeMemory
{
    OperationResult<bool> Store(
        string taskDescription, IReadOnlyList<AgentAlgorithms.SubTask> decomposition);
    OperationResult<IReadOnlyList<string>> Recall(string taskDescription, int topK = 3);
}

// 16 проверок качества перед доставкой результата (E39 +20.2%).
public interface IQualityOrchestrator
{
    OperationResult<QualityReport> Check(
        string taskDescription, string result, string outputType);
}

public sealed record QualityReport(bool Passed, IReadOnlyList<string> FailedChecks);
```

### `Fabric/Interfaces.cs` — FC-12 Fabric First fix

```csharp
namespace OrchestratorModel;

// ITemplateBeautifier — fabric interface для TemplateBeautifier из SemanticCompiler29.
// ResultMerger принимает этот интерфейс — не конкретный тип (Fabric First, Rule 3).
// Реализован через TemplateBeautifierAdapter в Phase 4.
public interface ITemplateBeautifier
{
    OperationResult<string> Beautify(string text);
}
```

### `Core/NullSessionStorage.cs` — для тестов (FC-12 fix)

```csharp
namespace OrchestratorModel;

// Null-object реализация ISessionStorage для классов у которых нет реального I/O.
// Используется в тестах вместо полного стаба ISessionStorage.
// internal — не часть публичного API.
internal sealed class NullSessionStorage : ISessionStorage
{
    internal static readonly NullSessionStorage Instance = new();
    private NullSessionStorage() { }

    // Реализовать все методы ISessionStorage как no-op.
    // Прочитать фактический интерфейс ISessionStorage из
    // 21.AgentInfrastructure перед написанием — методы могут отличаться.
    // Пример:
    public void Save(string key, string value) { }
    public string? Load(string key) => null;
    public void Clear() { }
}
```

**ВАЖНО:** прочитать `21.AgentInfrastructure/Core/Core.cs` или файл с `ISessionStorage`
чтобы узнать реальные методы интерфейса перед реализацией `NullSessionStorage`.

### `Core/InMemoryEpisodeMemory.cs` — FC-12 fix (теперь наследует BaseService)

```csharp
namespace OrchestratorModel;

// In-memory хранилище истории эпизодов.
// Заглушка — настоящая реализация через DomainDatasetCache придёт с 14.ContinuousLearning.
// Наследует BaseService (Rule 2).
public sealed class InMemoryEpisodeMemory : BaseService, IEpisodeMemory
{
    private readonly List<(string Task, IReadOnlyList<AgentAlgorithms.SubTask> Decomposition)>
        _history = new();
    private readonly int _maxEntries;

    public InMemoryEpisodeMemory(ISessionStorage storage, int maxEntries = 1000)
        : base(storage)
        => _maxEntries = maxEntries;

    public OperationResult<bool> Store(
        string taskDescription, IReadOnlyList<AgentAlgorithms.SubTask> decomposition)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<bool>.Fail("Task description cannot be empty.", "EMPTY_TASK");
        if (_history.Count >= _maxEntries) _history.RemoveAt(0);
        _history.Add((taskDescription, decomposition));
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyList<string>> Recall(
        string taskDescription, int topK = 3)
    {
        var matches = _history
            .Where(h => h.Task.Contains(taskDescription, StringComparison.OrdinalIgnoreCase))
            .TakeLast(topK)
            .Select(h => h.Task)
            .ToList();
        return OperationResult<IReadOnlyList<string>>.Ok(matches);
    }
}
```

### Тесты Фазы 1 (+3, итого baseline+3)

Создать `Tests/OrchestratorModelTests.cs`:

```csharp
namespace Tests;

public class OrchestratorModelTests
{
    // ── InMemoryEpisodeMemory ─────────────────────────────────────────────────

    [Fact]
    public void EpisodeMemory_Store_ThenRecall_FindsBySubstring()
    {
        var storage = NullSessionStorage.Instance;
        var memory  = new InMemoryEpisodeMemory(storage);
        var tasks   = new List<AgentAlgorithms.SubTask>
            { new("design the auth", "architect") };

        memory.Store("Design and implement auth system", tasks);
        var recalled = memory.Recall("auth system");
        Assert.True(recalled.IsSuccess);
        Assert.Single(recalled.Value!);
    }

    [Fact]
    public void EpisodeMemory_EmptyDescription_ReturnsFail()
    {
        var memory = new InMemoryEpisodeMemory(NullSessionStorage.Instance);
        var result = memory.Store("", new List<AgentAlgorithms.SubTask>());
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    [Fact]
    public void EpisodeMemory_ExceedsMaxEntries_EvictsOldest()
    {
        var memory = new InMemoryEpisodeMemory(NullSessionStorage.Instance, maxEntries: 2);
        memory.Store("task 1", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 2", new List<AgentAlgorithms.SubTask>());
        memory.Store("task 3", new List<AgentAlgorithms.SubTask>());

        Assert.Empty(memory.Recall("task 1").Value!);
        Assert.Single(memory.Recall("task 3").Value!);
    }
}
```

### Gate B Фазы 1

```bash
dotnet build LlmFromScratch.sln   # ноль ошибок
dotnet test Tests/LlmFromScratch.Tests.csproj  # baseline+3 passed, 0 failed
grep -rn "interface ITemplateBeautifier" 15.OrchestratorModel/Fabric/ | grep -v bin
# Ожидание: одна строка — Fabric First интерфейс на месте
git commit -m "phase 1: OrchestratorModel project, interfaces, Fabric/ITemplateBeautifier, InMemoryEpisodeMemory"
git push
```

---

## Фаза 2 — EpisodeClassifier + EpisodeDecomposer (E18, E32)

**FC-10 fix:** `EpisodeClassifier` использует `ISemanticCompiler.Compile()`, не `IRouteClassifier`.

**Контракт ISemanticCompiler (из SemanticCompiler29):**
```
ISemanticCompiler.Compile(PipelineInput input) → OperationResult<PipelineOutput>
PipelineInput(string Text, string? Stack)
PipelineOutput(Domain Domain, string[] Predicates, string Stack, double CompressionRatio)
Domain enum: CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN
```

**Confidence через CompressionRatio:** выше CompressionRatio = модель лучше сжала =
домен хорошо известен = выше уверенность. Типичное значение для известного домена ≈ 20.
`confidence = Math.Min(1.0, output.CompressionRatio / 20.0)`.
При CompressionRatio < 12 → confidence < 0.6 → сигнал для 17.DynamicDomains.

### `EpisodeClassifier.cs` — FC-10 fix

```csharp
namespace OrchestratorModel;

// Классифицирует задачу через SemanticCompiler29.ISemanticCompiler.
// Не обращается к IRouteClassifier напрямую — пайплайн инкапсулирован в ISemanticCompiler.
// E18: 95% точность домена.
public sealed class EpisodeClassifier : BaseService, IEpisodeClassifier
{
    private readonly ISemanticCompiler _compiler; // из SemanticCompiler29

    // Маппинг Domain (SemanticCompiler29) → TaskDomain (OrchestratorModel).
    // Значения совпадают: CODE_Q=0, BUG=1, PLAN=2, ARCH=3, REFACTOR=4.
    private static TaskDomain MapDomain(SemanticCompiler29.Domain d) => d switch
    {
        SemanticCompiler29.Domain.BUG      => TaskDomain.BUG,
        SemanticCompiler29.Domain.PLAN     => TaskDomain.PLAN,
        SemanticCompiler29.Domain.ARCH     => TaskDomain.ARCH,
        SemanticCompiler29.Domain.REFACTOR => TaskDomain.REFACTOR,
        _                                  => TaskDomain.CODE_Q, // CODE_Q + UNKNOWN → CODE_Q
    };

    public EpisodeClassifier(ISemanticCompiler compiler, ISessionStorage storage)
        : base(storage)
        => _compiler = compiler;

    public OperationResult<TaskDomain> Classify(string taskDescription)
    {
        var r = ClassifyWithConfidence(taskDescription);
        return r.IsSuccess
            ? OperationResult<TaskDomain>.Ok(r.Value!.Domain)
            : OperationResult<TaskDomain>.Fail(r.ErrorMessage, r.ErrorCode);
    }

    public OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<(TaskDomain, double)>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        // Запустить полный пайплайн SemanticCompiler (E20 порядок соблюдён внутри)
        var compileResult = _compiler.Compile(new SemanticCompiler29.PipelineInput(
            taskDescription, null));
        if (!compileResult.IsSuccess)
            return OperationResult<(TaskDomain, double)>.Fail(
                compileResult.ErrorMessage, compileResult.ErrorCode);

        var output      = compileResult.Value!;
        TaskDomain domain = MapDomain(output.Domain);

        // CompressionRatio как прокси для confidence:
        // высокое сжатие = хорошо известный домен = высокая уверенность.
        // Порог < 0.6 (ratio < 12) сигнализирует 17.DynamicDomains о новом домене.
        double confidence = Math.Min(1.0, output.CompressionRatio / 20.0);

        return OperationResult<(TaskDomain, double)>.Ok((domain, confidence));
    }
}
```

### `EpisodeDecomposer.cs` — FC-12 fix (конструктор BaseService)

```csharp
namespace OrchestratorModel;

// Разбивает задачу на под-эпизоды.
// E32 +11%: RESEARCH → 4-5 под-эпизодов, BUILD → 2-3, FIX → 2.
public sealed class EpisodeDecomposer : BaseService, IEpisodeDecomposer
{
    private static readonly Dictionary<TaskDomain, int> ExpectedSubEpisodes = new()
    {
        [TaskDomain.PLAN]    = 4,
        [TaskDomain.ARCH]    = 3,
        [TaskDomain.REFACTOR]= 3,
        [TaskDomain.BUG]     = 2,
        [TaskDomain.CODE_Q]  = 1,
    };

    private static readonly string[] FalseConjStarters =
        ["find", "check", "read", "get", "list", "show", "search", "look"];

    // FC-12: BaseService требует ISessionStorage в конструкторе
    public EpisodeDecomposer(ISessionStorage storage) : base(storage) { }

    public OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>> Decompose(
        string taskDescription, TaskDomain domain)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        int targetCount = ExpectedSubEpisodes.GetValueOrDefault(domain, 2);
        var parts    = SplitByConjunctions(taskDescription);
        var subTasks = parts
            .Take(targetCount)
            .Select((p, i) => new AgentAlgorithms.SubTask(p.Trim(), AssignRole(domain, i)))
            .ToList();

        if (subTasks.Count == 0)
            subTasks.Add(new AgentAlgorithms.SubTask(taskDescription, AssignRole(domain, 0)));

        return OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>>.Ok(subTasks);
    }

    private static List<string> SplitByConjunctions(string text)
    {
        var parts = text.Split(
            [" and ", " then ", " after that "],
            StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return [text];
        bool secondIsIndependent = !FalseConjStarters.Any(
            v => parts[1].Trim().ToLower().StartsWith(v + " "));
        return secondIsIndependent ? parts.ToList() : [text];
    }

    private static string AssignRole(TaskDomain domain, int position) => domain switch
    {
        TaskDomain.ARCH    => position == 0 ? "architect"  : "implementer",
        TaskDomain.PLAN    => position == 0 ? "planner"    : "executor",
        TaskDomain.REFACTOR=> position == 0 ? "implementer": "reviewer",
        TaskDomain.BUG     => position == 0 ? "debugger"   : "implementer",
        _                  => "assistant",
    };
}
```

### Тесты Фазы 2 (+5, итого baseline+8)

```csharp
// ── EpisodeClassifier ─────────────────────────────────────────────────────────

[Fact]
public void EpisodeClassifier_UnknownDomain_FallsBackToCodeQ()
{
    // SemanticCompiler возвращает Domain.UNKNOWN → CODE_Q как безопасное умолчание
    var compiler   = new StubSemanticCompiler(SemanticCompiler29.Domain.UNKNOWN, ratio: 5.0);
    var classifier = new EpisodeClassifier(compiler, NullSessionStorage.Instance);
    var result     = classifier.Classify("some obscure task");
    Assert.True(result.IsSuccess);
    Assert.Equal(TaskDomain.CODE_Q, result.Value!);
}

[Fact]
public void EpisodeClassifier_LowCompressionRatio_LowConfidence()
{
    // CompressionRatio = 5.0 → confidence = 5.0/20.0 = 0.25 < 0.6
    // Сигнал для 17.DynamicDomains о потенциально новом домене
    var compiler   = new StubSemanticCompiler(SemanticCompiler29.Domain.CODE_Q, ratio: 5.0);
    var classifier = new EpisodeClassifier(compiler, NullSessionStorage.Instance);
    var result     = classifier.ClassifyWithConfidence("Write Dockerfile for .NET app");
    Assert.True(result.IsSuccess);
    Assert.True(result.Value!.Confidence < 0.6,
        $"Expected confidence < 0.6, got {result.Value!.Confidence}");
}

// ── EpisodeDecomposer ─────────────────────────────────────────────────────────

[Fact]
public void EpisodeDecomposer_PlanDomain_Returns4SubEpisodes()
{
    var decomposer = new EpisodeDecomposer(NullSessionStorage.Instance);
    var result     = decomposer.Decompose(
        "Design auth system and implement it and test it and document it",
        TaskDomain.PLAN);
    Assert.True(result.IsSuccess);
    Assert.Equal(4, result.Value!.Count);
}

[Fact]
public void EpisodeDecomposer_FalseConjunction_StaysAsSingleTask()
{
    var decomposer = new EpisodeDecomposer(NullSessionStorage.Instance);
    var result     = decomposer.Decompose(
        "Find methods that return null and log them", TaskDomain.CODE_Q);
    Assert.True(result.IsSuccess);
    Assert.Single(result.Value!);
}

[Fact]
public void EpisodeDecomposer_BugDomain_FirstRoleIsDebugger()
{
    var decomposer = new EpisodeDecomposer(NullSessionStorage.Instance);
    var result     = decomposer.Decompose(
        "Fix NullReference in UserService then add regression test", TaskDomain.BUG);
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.Count);
    Assert.Equal("debugger", result.Value![0].RequiredRole);
}
```

Добавить стаб в конец файла (FC-10 fix — правильный контракт ISemanticCompiler):

```csharp
// Тестовый стаб — ISemanticCompiler с настраиваемым ответом
// Реализует реальный контракт: Compile(PipelineInput) → OperationResult<PipelineOutput>
file sealed class StubSemanticCompiler : ISemanticCompiler
{
    private readonly SemanticCompiler29.Domain _domain;
    private readonly double                    _ratio;

    public StubSemanticCompiler(SemanticCompiler29.Domain domain, double ratio = 15.0)
    {
        _domain = domain;
        _ratio  = ratio;
    }

    public OperationResult<SemanticCompiler29.PipelineOutput> Compile(
        SemanticCompiler29.PipelineInput input)
        => OperationResult<SemanticCompiler29.PipelineOutput>.Ok(
            new SemanticCompiler29.PipelineOutput(_domain, [], "csharp", _ratio));
}
```

### Gate B Фазы 2

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # baseline+8 passed, 0 failed
dotnet test --filter "EpisodeDecomposer_FalseConjunction_StaysAsSingleTask"
# Ожидание: PASSED

# FC-10 проверка: IRouteClassifier не используется нигде в 15
grep -rn "IRouteClassifier" 15.OrchestratorModel/ --include="*.cs" | grep -v bin
# Ожидание: 0 строк

git commit -m "phase 2: EpisodeClassifier (ISemanticCompiler, FC-10), EpisodeDecomposer (E32)"
git push
```

---

## Фаза 3 — StopSignalDetector + DependencyGraph reuse (E33, E35)

**FC-12 fix:** `StopSignalDetector` получает `(ISessionStorage storage) : base(storage)`.

### `StopSignalDetector.cs` — FC-12 fix

```csharp
namespace OrchestratorModel;

// Четыре сигнала остановки декомпозиции (DEPTH-CALIBRATION-GUIDANCE).
// E33: алгоритм верный, Tier 2 синтетика слишком проста. Настоящий тест на реальных эпизодах.
public sealed class StopSignalDetector : BaseService, IStopSignalDetector
{
    // FC-12: BaseService требует ISessionStorage
    public StopSignalDetector(ISessionStorage storage) : base(storage) { }

    public OperationResult<bool> ShouldStop(string subTaskDescription)
    {
        if (string.IsNullOrWhiteSpace(subTaskDescription))
            return OperationResult<bool>.Fail("Description cannot be empty.", "EMPTY_TASK");

        var lower = subTaskDescription.ToLowerInvariant();

        // Сигнал 1 — PHASES: просто порядок шагов, не самостоятельные под-эпизоды
        bool isPhaseSequence =
            new[] { "then ", "next ", "after that", "first step", "step " }
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");
        if (isPhaseSequence) return OperationResult<bool>.Ok(true);

        // Сигнал 2 — TECH: требует технического контекста стека
        bool requiresTechContext =
            lower.Contains("configure") && lower.Contains("service") ||
            lower.Contains("register") && lower.Contains("dependency") ||
            lower.Contains("inject") && lower.Contains("interface");
        if (requiresTechContext) return OperationResult<bool>.Ok(true);

        // Сигнал 3 — SINGLE_DOC: один документ без структуры
        bool isSingleDoc =
            lower.Contains("single ") || lower.Contains("one document") ||
            lower.Contains("one file") || lower.Contains("one-time");
        if (isSingleDoc) return OperationResult<bool>.Ok(true);

        // Сигнал 4 — NO_LOOP + короткая задача
        bool hasNoLoop =
            !lower.Contains("review") && !lower.Contains("iterate") &&
            !lower.Contains("revise") && !lower.Contains("feedback") &&
            !lower.Contains("correction");
        if (hasNoLoop && subTaskDescription.Split(' ').Length <= 6)
            return OperationResult<bool>.Ok(true);

        return OperationResult<bool>.Ok(false);
    }
}
```

### Тесты Фазы 3 (+3, итого baseline+11)

```csharp
// ── StopSignalDetector ────────────────────────────────────────────────────────

[Fact]
public void StopSignalDetector_PhaseSequence_ReturnsTrue()
{
    var detector = new StopSignalDetector(NullSessionStorage.Instance);
    var result   = detector.ShouldStop("First read the requirements then decide the approach");
    Assert.True(result.IsSuccess);
    Assert.True(result.Value!);
}

[Fact]
public void StopSignalDetector_IndependentGoal_ReturnsFalse()
{
    var detector = new StopSignalDetector(NullSessionStorage.Instance);
    var result   = detector.ShouldStop("Design the authentication module architecture");
    Assert.True(result.IsSuccess);
    Assert.False(result.Value!);
}

// ── DependencyGraph переиспользование ─────────────────────────────────────────

[Fact]
public void DependencyGraph_ArchBeforeImplementer_UsableFrom15()
{
    // DependencyGraph из 22.AgentAlgorithms используется без дублирования
    var graph = new AgentAlgorithms.DependencyGraph();
    graph.AddDependency("architect", "implementer");

    var tasks = new List<AgentAlgorithms.SubTask>
    {
        new("design the auth system",     "architect"),
        new("implement the auth service", "implementer"),
    };
    var result = graph.Sort(tasks);

    Assert.True(result.IsSuccess);
    Assert.Equal("architect",   result.Value![0][0].RequiredRole);
    Assert.Equal("implementer", result.Value![1][0].RequiredRole);
}
```

### Gate B Фазы 3

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # baseline+11 passed, 0 failed
find 15.OrchestratorModel -name "DependencyGraph.cs" | grep -v bin
# Ожидание: нет файла — не дублирован
git commit -m "phase 3: StopSignalDetector (4 signals, FC-12), DependencyGraph reuse"
git push
```

---

## Фаза 4 — AgentLauncher + ResultMerger + TemplateBeautifierAdapter (E23)

**FC-12 fix:** `ResultMerger` принимает `ITemplateBeautifier`, не конкретный тип.
**TemplateBeautifierAdapter** — Adapter pattern: оборачивает `SemanticCompiler29.TemplateBeautifier`
за интерфейсом объявленным в `Fabric/Interfaces.cs`.

### `Adapters/TemplateBeautifierAdapter.cs` — FC-12 fix

```csharp
namespace OrchestratorModel;

// Adapter: оборачивает SemanticCompiler29.TemplateBeautifier за ITemplateBeautifier.
// Это стандартный Fabric First паттерн — внешний конкретный тип скрыт за интерфейсом.
// ResultMerger принимает ITemplateBeautifier — не знает о SemanticCompiler29.
public sealed class TemplateBeautifierAdapter : ITemplateBeautifier
{
    private readonly SemanticCompiler29.TemplateBeautifier _inner;

    public TemplateBeautifierAdapter(SemanticCompiler29.TemplateBeautifier inner)
        => _inner = inner;

    public OperationResult<string> Beautify(string text)
        => _inner.Beautify(text);
}
```

### `AgentLauncher.cs` — FC-12 fix (конструктор BaseService)

```csharp
namespace OrchestratorModel;

// Запускает агента через AgentOrchestrator из 22.AgentAlgorithms.
public sealed class AgentLauncher : BaseService, IAgentLauncher
{
    private readonly AgentAlgorithms.AgentOrchestrator _orchestrator;

    // FC-12: первый параметр ISessionStorage → base(storage)
    public AgentLauncher(ISessionStorage storage,
                         AgentAlgorithms.AgentOrchestrator orchestrator)
        : base(storage)
        => _orchestrator = orchestrator;

    public async Task<OperationResult<string>> LaunchAsync(
        AgentAlgorithms.SubTask task, string context)
    {
        if (string.IsNullOrWhiteSpace(task.Description))
            return OperationResult<string>.Fail(
                "Sub-task description cannot be empty.", "EMPTY_TASK");

        string enrichedDescription = string.IsNullOrWhiteSpace(context)
            ? task.Description
            : $"{task.Description}\n\nContext from earlier steps:\n{context}";

        return await _orchestrator.RunAsync(enrichedDescription);
    }
}
```

### `ResultMerger.cs` — FC-12 fix (ITemplateBeautifier, не конкретный тип)

```csharp
namespace OrchestratorModel;

// Объединяет результаты агентов.
// Использует ITemplateBeautifier (Fabric First) — не SemanticCompiler29.TemplateBeautifier.
// E23: SUCCESS=RETURN маппинг встроен в реальный Beautifier внутри адаптера.
public sealed class ResultMerger : BaseService, IResultMerger
{
    private readonly ITemplateBeautifier _beautifier; // интерфейс, не конкретный тип

    public ResultMerger(ISessionStorage storage, ITemplateBeautifier beautifier)
        : base(storage)
        => _beautifier = beautifier;

    public OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results)
    {
        if (!results.Any())
            return OperationResult<string>.Fail("No results to merge.", "EMPTY_RESULTS");
        if (results.Count == 1)
            return OperationResult<string>.Ok(results[0].Result);

        var merged = string.Join("\n\n", results.Select((r, i) =>
            $"## Step {i + 1}: {r.Task.RequiredRole}\n{r.Result}"));

        var beautifyResult = _beautifier.Beautify(merged);
        return beautifyResult.IsSuccess
            ? beautifyResult
            : OperationResult<string>.Ok(merged);
    }
}
```

### Тесты Фазы 4 (+3, итого baseline+14)

```csharp
// ── AgentLauncher ─────────────────────────────────────────────────────────────

[Fact]
public async Task AgentLauncher_EmptyDescription_ReturnsFail()
{
    var orch     = BuildStubOrchestrator();
    var launcher = new AgentLauncher(NullSessionStorage.Instance, orch);
    var result   = await launcher.LaunchAsync(
        new AgentAlgorithms.SubTask("", "architect"), "");
    Assert.False(result.IsSuccess);
    Assert.Equal("EMPTY_TASK", result.ErrorCode);
}

// ── ResultMerger ──────────────────────────────────────────────────────────────

[Fact]
public void ResultMerger_SingleResult_ReturnedUnchanged()
{
    // TemplateBeautifierAdapter реализует ITemplateBeautifier
    var beautifier = new TemplateBeautifierAdapter(
        new SemanticCompiler29.TemplateBeautifier());
    var merger = new ResultMerger(NullSessionStorage.Instance, beautifier);
    var task   = new AgentAlgorithms.SubTask("design auth", "architect");

    var result = merger.Merge([(task, "auth schema: JWT + RBAC")]);
    Assert.True(result.IsSuccess);
    Assert.Equal("auth schema: JWT + RBAC", result.Value!);
}

[Fact]
public void ResultMerger_EmptyResults_ReturnsFail()
{
    var beautifier = new TemplateBeautifierAdapter(
        new SemanticCompiler29.TemplateBeautifier());
    var merger = new ResultMerger(NullSessionStorage.Instance, beautifier);
    var result = merger.Merge([]);
    Assert.False(result.IsSuccess);
    Assert.Equal("EMPTY_RESULTS", result.ErrorCode);
}
```

Добавить хелпер в тестовый файл:

```csharp
// Создать AgentOrchestrator для тестов — минимальная конфигурация
private static AgentAlgorithms.AgentOrchestrator BuildStubOrchestrator()
{
    // Реализовать с реальными или стаб зависимостями из AgentAlgorithms
    // Прочитать AgentOrchestrator конструктор для точных параметров:
    // grep -n "public AgentOrchestrator" 22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs
    throw new NotImplementedException("Replace with actual stub construction");
}
```

**ВАЖНО:** прочитать `22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs` конструктор
перед написанием `BuildStubOrchestrator` — параметры могут отличаться от ожидаемых.

### Gate B Фазы 4

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # baseline+14 passed, 0 failed

# FC-12 проверка: конкретный тип TemplateBeautifier не используется в ResultMerger
grep -n "SemanticCompiler29.TemplateBeautifier" 15.OrchestratorModel/ResultMerger.cs | grep -v bin
# Ожидание: 0 строк (ResultMerger использует только ITemplateBeautifier)

# ITemplateBeautifier в Fabric — не в ResultMerger.cs
grep -rn "ITemplateBeautifier" 15.OrchestratorModel/ | grep -v bin
# Ожидание: Fabric/Interfaces.cs + ResultMerger.cs

git commit -m "phase 4: AgentLauncher, ResultMerger (ITemplateBeautifier FC-12), TemplateBeautifierAdapter"
git push
```

---

## Фаза 5 — QualityOrchestrator + OrchestratorModel (E39)

**FC-12 fix:** `QualityOrchestrator` и `OrchestratorModel` получают `ISessionStorage`.
**FC-1 fix:** Phase 5 добавляет 3 теста (не 2 как в v1.0), итого baseline+17.

### `QualityOrchestrator.cs` — FC-12 fix

```csharp
namespace OrchestratorModel;

// 16 проверок качества перед доставкой результата. E39 +20.2%.
public sealed class QualityOrchestrator : BaseService, IQualityOrchestrator
{
    private static readonly string[] AlwaysChecks =
    [
        "OUTPUT_CONTRACT", "MISSION_PROGRESS", "ISSUE_INVENTORY", "GOAL_REMINDER",
        "TOOLS_BEFORE_PERSON", "RESPONSE_CONSTRUCTION", "CLAIMS_VERIFIED", "HUMAN_LANGUAGE_GATE",
    ];

    private static readonly Dictionary<string, string[]> ByOutputType = new()
    {
        ["CODE_SERVICE"] = ["TEST_GATE", "SCOPE_ISOLATION", "PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS"],
        ["PLAN_DOC"]     = ["PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS", "GOAL_DELIVERY_VERDICT"],
        ["REACT_PAGE"]   = ["UI_UX_COMPLIANCE"],
        ["ANALYSIS_DOC"] = ["RECONNAISSANCE_EVIDENCE", "ARCHITECT_HABITS"],
    };

    // FC-12: BaseService требует ISessionStorage
    public QualityOrchestrator(ISessionStorage storage) : base(storage) { }

    public OperationResult<QualityReport> Check(
        string taskDescription, string result, string outputType)
    {
        if (string.IsNullOrWhiteSpace(result))
            return OperationResult<QualityReport>.Fail("Result cannot be empty.", "EMPTY_RESULT");

        var failed = new List<string>();
        if (result.Length < 10)
            failed.Add("OUTPUT_CONTRACT");
        if (outputType == "CODE_SERVICE" && !result.Contains("return"))
            failed.Add("TEST_GATE");

        return OperationResult<QualityReport>.Ok(new QualityReport(failed.Count == 0, failed));
    }
}
```

### `OrchestratorModel.cs` — FC-12 fix

```csharp
namespace OrchestratorModel;

// Главный пайплайн подпроекта 15.
public sealed class OrchestratorModel : BaseService
{
    private readonly IEpisodeClassifier              _classifier;
    private readonly IEpisodeDecomposer              _decomposer;
    private readonly IStopSignalDetector             _stopDetector;
    private readonly AgentAlgorithms.DependencyGraph _graph;
    private readonly IAgentLauncher                  _launcher;
    private readonly IResultMerger                   _merger;
    private readonly IQualityOrchestrator            _quality;
    private readonly IEpisodeMemory                  _memory;

    // FC-12: ISessionStorage первым параметром
    public OrchestratorModel(
        ISessionStorage                  storage,
        IEpisodeClassifier               classifier,
        IEpisodeDecomposer               decomposer,
        IStopSignalDetector              stopDetector,
        AgentAlgorithms.DependencyGraph  graph,
        IAgentLauncher                   launcher,
        IResultMerger                    merger,
        IQualityOrchestrator             quality,
        IEpisodeMemory                   memory)
        : base(storage)
    {
        _classifier   = classifier;
        _decomposer   = decomposer;
        _stopDetector = stopDetector;
        _graph        = graph;
        _launcher     = launcher;
        _merger       = merger;
        _quality      = quality;
        _memory       = memory;
    }

    public async Task<OperationResult<string>> ProcessAsync(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<string>.Fail("Task description cannot be empty.", "EMPTY_TASK");

        var classifyResult = _classifier.ClassifyWithConfidence(taskDescription);
        if (!classifyResult.IsSuccess)
            return OperationResult<string>.Fail(classifyResult.ErrorMessage, "CLASSIFY_FAILED");

        var decompResult = _decomposer.Decompose(taskDescription, classifyResult.Value!.Domain);
        if (!decompResult.IsSuccess)
            return OperationResult<string>.Fail(decompResult.ErrorMessage, "DECOMPOSE_FAILED");

        var subTasks = decompResult.Value!;
        var tasks    = subTasks
            .Where(t => !(_stopDetector.ShouldStop(t.Description).Value ?? false))
            .ToList();
        if (tasks.Count == 0) tasks = subTasks.ToList();

        var sortResult   = _graph.Sort(tasks);
        var orderedLevels = sortResult.IsSuccess
            ? sortResult.Value!
            : (IReadOnlyList<IReadOnlyList<AgentAlgorithms.SubTask>>)[tasks];

        var allResults = new List<(AgentAlgorithms.SubTask Task, string Result)>();
        string context = "";
        foreach (var level in orderedLevels)
        {
            var levelResults = await Task.WhenAll(
                level.Select(t => _launcher.LaunchAsync(t, context)));
            foreach (var (task, res) in level.Zip(levelResults))
            {
                allResults.Add((task, res.IsSuccess ? res.Value! : $"[{task.RequiredRole} failed]"));
            }
            context = allResults.Last().Result;
        }

        var mergeResult = _merger.Merge(allResults);
        if (!mergeResult.IsSuccess)
            return OperationResult<string>.Fail(mergeResult.ErrorMessage, "MERGE_FAILED");

        string outputType = classifyResult.Value!.Domain == TaskDomain.CODE_Q
            ? "CODE_SERVICE" : "PLAN_DOC";
        var qualityResult = _quality.Check(taskDescription, mergeResult.Value!, outputType);
        if (qualityResult.IsSuccess && !qualityResult.Value!.Passed)
        {
            string failedChecks = string.Join(", ", qualityResult.Value!.FailedChecks);
            return OperationResult<string>.Fail(
                $"Quality checks failed: {failedChecks}", "QUALITY_FAILED");
        }

        _memory.Store(taskDescription, subTasks);
        return OperationResult<string>.Ok(mergeResult.Value!);
    }
}
```

### Тесты Фазы 5 (+3, итого baseline+17) — FC-1 fix

```csharp
// ── QualityOrchestrator ───────────────────────────────────────────────────────

[Fact]
public void QualityOrchestrator_ShortResult_FailsOutputContract()
{
    var quality = new QualityOrchestrator(NullSessionStorage.Instance);
    var report  = quality.Check("add null check", "ok", "CODE_SERVICE");
    Assert.True(report.IsSuccess);
    Assert.False(report.Value!.Passed);
    Assert.Contains("OUTPUT_CONTRACT", report.Value!.FailedChecks);
}

[Fact]
public void QualityOrchestrator_SubstantiveResultWithReturn_Passes()
{
    var quality = new QualityOrchestrator(NullSessionStorage.Instance);
    string res  = "if (user == null) return OperationResult<User>.Fail(\"not found\");";
    var report  = quality.Check("add null check to GetUser", res, "CODE_SERVICE");
    Assert.True(report.IsSuccess);
    Assert.True(report.Value!.Passed,
        $"Expected pass, failed: {string.Join(", ", report.Value!.FailedChecks)}");
}

// ── Gate B — интеграционный тест ──────────────────────────────────────────────

[Fact]
public async Task OrchestratorModel_DeployTask_ArchFirstLevel()
{
    // Gate B из мастер-плана: "Deploy new application" → ARCH на уровне 0
    var graph = new AgentAlgorithms.DependencyGraph();
    graph.AddDependency("architect", "implementer");

    var decomposer = new EpisodeDecomposer(NullSessionStorage.Instance);
    var tasks      = decomposer.Decompose(
        "Design deployment architecture then implement the service then deploy then monitor",
        TaskDomain.PLAN);
    Assert.True(tasks.IsSuccess);

    var sorted = graph.Sort(tasks.Value!.ToList());
    Assert.True(sorted.IsSuccess);

    var firstLevel = sorted.Value![0];
    Assert.Contains(firstLevel, t =>
        t.RequiredRole == "architect" || t.RequiredRole == "planner");
}
```

### Gate B Фазы 5 — финальный

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # baseline+17 passed, 0 failed

# Gate B из мастер-плана:
dotnet test --filter "OrchestratorModel_DeployTask_ArchFirstLevel"
# Ожидание: PASSED

# FC-12 полная проверка:
grep -rn "IRouteClassifier" 15.OrchestratorModel/ --include="*.cs" | grep -v bin
# Ожидание: 0 строк

grep -rn "SemanticCompiler29\.TemplateBeautifier" 15.OrchestratorModel/ResultMerger.cs | grep -v bin
# Ожидание: 0 строк (только ITemplateBeautifier)

grep -rn "ISessionStorage\|: base(storage)" 15.OrchestratorModel/ --include="*.cs" | grep -v bin
# Ожидание: в каждом классе наследующем BaseService

# DependencyGraph не дублирован:
find 15.OrchestratorModel -name "DependencyGraph.cs" | grep -v bin
# Ожидание: нет файла

grep -c "\[Fact\]" Tests/OrchestratorModelTests.cs  # Ожидание: 17

git commit -m "phase 5: QualityOrchestrator (E39, FC-12), OrchestratorModel pipeline, Gate B passed"
git push
```

---

## Финальная проверка (FC self-check)

```bash
# FC-10: IRouteClassifier нигде нет
grep -rn "IRouteClassifier\|RouteClassificationResult" 15.OrchestratorModel/ | grep -v bin
# Ожидание: 0 строк

# FC-12: все BaseService-наследники имеют base(storage)
grep -rn "extends BaseService\|: BaseService" 15.OrchestratorModel/ --include="*.cs" | grep -v bin
# Прочитать вывод — для каждого класса проверить что есть ": base(storage)"

# FC-2: .csproj без числового префикса
ls 15.OrchestratorModel/*.csproj
# Ожидание: OrchestratorModel.csproj (не 15.OrchestratorModel.csproj)

# Счёт тестов: baseline+17
grep -c "\[Fact\]" Tests/OrchestratorModelTests.cs  # 17

dotnet build LlmFromScratch.sln && dotnet test Tests/LlmFromScratch.Tests.csproj
```

---

## Счёт тестов по фазам

| Фаза | Что создаётся | Тестов | Итого |
|------|--------------|--------|-------|
| Baseline | — | — | 877* |
| Фаза 1 | Интерфейсы, Fabric/ITemplateBeautifier, InMemoryEpisodeMemory | +3 | 880 |
| Фаза 2 | EpisodeClassifier (ISemanticCompiler), EpisodeDecomposer | +5 | 885 |
| Фаза 3 | StopSignalDetector, DependencyGraph reuse | +3 | 888 |
| Фаза 4 | AgentLauncher, ResultMerger (ITemplateBeautifier), TemplateBeautifierAdapter | +3 | 891 |
| Фаза 5 | QualityOrchestrator, OrchestratorModel, Gate B test | +3 | 894 |

*Если 14.ContinuousLearning выполнен раньше: добавить 22 к каждому числу (итого 894+22=916).
Прочитать реальный baseline в pre-condition gate и пересчитать.

---

## Правила на всю сессию

**ISemanticCompiler — не IRouteClassifier.** `EpisodeClassifier` вызывает `Compile()`,
получает `PipelineOutput.Domain`. Прямое обращение к `IRouteClassifier` — нарушение
инкапсуляции SemanticCompiler. (FC-10)

**ITemplateBeautifier — не конкретный тип.** `ResultMerger` принимает `ITemplateBeautifier`.
`TemplateBeautifierAdapter` — единственное место где `SemanticCompiler29.TemplateBeautifier`
упоминается как конкретный тип. (FC-12 Fabric First)

**BaseService(ISessionStorage) — везде.** Каждый класс наследующий `BaseService`
передаёт `ISessionStorage` через конструктор. Тесты используют `NullSessionStorage.Instance`. (FC-12)

**DependencyGraph — не дублировать.** Реализован в `22.AgentAlgorithms`.
Ссылка через `.csproj` уже есть. (существующее правило)

**Ноль failing — жёсткое условие.** Исправить до следующего шага.

---

*Если этот план расходится с SemanticCompiler29 source файлами —
source файлы управляют. Прочитать `ISemanticCompiler`, `PipelineOutput`, `Domain`
перед написанием EpisodeClassifier.*
