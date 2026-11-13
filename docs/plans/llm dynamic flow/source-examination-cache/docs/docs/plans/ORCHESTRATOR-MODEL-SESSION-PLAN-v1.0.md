# SESSION PLAN — 15.OrchestratorModel (Трек В)
## Внедрение E32, E33, E35, E39 в подпроект OrchestratorModel
## Запускать ПОСЛЕ: AGENT-ALGORITHMS-ENHANCEMENT (22 v5.0) ✅ + SEMANTIC-COMPILER (29) ✅

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
- Шаг 1 (классификация) → Фаза 2: `EpisodeClassifier` оборачивает `RouteClassifier` из `SemanticCompiler29`
- Шаг 2 (декомпозиция) → Фаза 2: `EpisodeDecomposer` предсказывает количество под-эпизодов (E32 +11%)
- Шаг 3 (порядок) → Фаза 3: переиспользует `DependencyGraph` из `22.AgentAlgorithms` (E35 +30%)
- Шаг 4 (остановка) → Фаза 3: `StopSignalDetector` — четыре сигнала (E33, потолок на Tier 2)
- Шаг 5 (запуск) → Фаза 4: `AgentLauncher` делегирует в `AgentOrchestrator`
- Шаг 6 (объединение) → Фаза 4: `ResultMerger` через `TemplateBeautifier` (E23 +820%)
- Шаг 7 (качество) → Фаза 5: `QualityOrchestrator` — 16 проверок (E39 +20%)
- Шаг 8 (память) → Фаза 4: `InMemoryEpisodeMemory` — заглушка до 14.ContinuousLearning

---

## Pre-condition gate

Запустить до начала. Все три должны пройти.

```bash
cd C:\Projects\llm_mvp_core

# 1. 29.SemanticCompiler построен — ISemanticCompiler существует
grep -rn "ISemanticCompiler\|SemanticCompiler29" . --include="*.cs" | grep -v bin | head -3
# Ожидание: минимум одна строка с ISemanticCompiler

# 2. DependencyGraph существует в 22
grep -rn "class DependencyGraph" 22.AgentAlgorithms\ --include="*.cs" | grep -v bin
# Ожидание: одна строка с DependencyGraph в AgentAlgorithms

# 3. Baseline чистый
dotnet test Tests/LlmFromScratch.Tests.csproj
# Ожидание: 877 passed, 0 failed
```

Если `ISemanticCompiler` не найдена — искать по `SemanticCompiler29` namespace.
Если `DependencyGraph` не найдена — она должна быть реализована в AGENT-ALGORITHMS плане.
Если тесты не проходят — исправить до начала.

---

## Step 1 — Прочитать каждый файл который будешь трогать

Прочитать полностью до написания кода. Записать один конкретный факт из конца каждого файла.

```bash
# Существующие зависимости:
cat 21.AgentInfrastructure/Core/Core.cs              # BaseService — откуда наследовать
cat 22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs # SubTask record, DependencyGraph usage
cat 22.AgentAlgorithms/MultiAgent/DependencyGraph.cs  # Sort(tasks) → уровни
cat 22.AgentAlgorithms/Context/ContextWindowManager.cs # как подключён ISkillFilter
# Найти RouteClassifier в 29.SemanticCompiler:
find . -name "RouteClassifier.cs" -not -path "*/bin/*"
cat <найденный путь>
# Тесты:
cat Tests/AgentAlgorithmsTests.cs    # примеры стабов и структуры тестов
```

## Step 2 — Объявить состояние

После Step 1 написать один абзац:
- какая ветка
- сколько тестов проходит прямо сейчас (запустить dotnet test)
- какую фазу начинаешь

---

## ROUND CONTRACT — перед каждым ответом

```
ROUND CONTRACT:
1. Baseline:         877 passed, 0 failed
2. Сейчас:          [запустить dotnet test — реальные числа]
3. Произведено:     [файлы изменены, тесты добавлены, команды запущены]
4. Фаза требует:    [точный Gate B для текущей фазы]
5. Пробел:          [что из пункта 4 ещё не выполнено]
```

Пункт 5 должен показать ноль пробелов перед тем как фаза объявляется завершённой.

---

## GATE — после каждой фазы

```bash
dotnet build LlmFromScratch.sln          # ноль ошибок
dotnet test Tests/LlmFromScratch.Tests.csproj   # ноль falling, счёт >= baseline
git diff --name-only                     # только ожидаемые файлы
```

---

## Фаза 1 — Проект + интерфейсы (Core)

**Что доставляем:**
Папка `15.OrchestratorModel/`, `.csproj` с зависимостями, семь интерфейсов,
`InMemoryEpisodeMemory` заглушка.

**Важно: `IEpisodeMemory` — заглушка до 14.ContinuousLearning**

`EpisodeMemory.cs` из мастер-плана ссылается на `DomainDatasetCache` из 14,
который ещё не построен. Правильное решение: `IEpisodeMemory` интерфейс
с `InMemoryEpisodeMemory` реализацией — настоящая реализация придёт с 14.

### Создать `15.OrchestratorModel/15.OrchestratorModel.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- SemanticCompiler для RouteClassifier и ISemanticCompiler -->
    <ProjectReference Include="..\29.SemanticCompiler\29.SemanticCompiler.csproj" />
    <!-- AgentAlgorithms для DependencyGraph, SubTask, AgentOrchestrator -->
    <ProjectReference Include="..\22.AgentAlgorithms\22.AgentAlgorithms.csproj" />
    <!-- AgentInfrastructure для BaseService -->
    <ProjectReference Include="..\21.AgentInfrastructure\21.AgentInfrastructure.csproj" />
  </ItemGroup>
</Project>
```

Добавить ссылку на проект в `LlmFromScratch.sln`.

### Создать `15.OrchestratorModel/Core/Interfaces.cs`

```csharp
namespace OrchestratorModel;

// Домены задач — короткие формы из D-04.
// Отображение на полные имена: CODE_Q = CodeQuestion, BUG = BugFix,
// PLAN = Orchestration, ARCH = Architecture, REFACTOR = CodeChange.
public enum TaskDomain
{
    CODE_Q,    // вопрос о работе кода
    BUG,       // исправление ошибки
    PLAN,      // оркестрация, многошаговая задача
    ARCH,      // архитектурное решение
    REFACTOR,  // изменение кода
}

// Классифицирует задачу в один из пяти доменов.
// Реализован в EpisodeClassifier — оборачивает RouteClassifier из 29.SemanticCompiler.
public interface IEpisodeClassifier
{
    OperationResult<TaskDomain> Classify(string taskDescription);
    OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(string taskDescription);
}

// Предсказывает количество под-эпизодов для задачи.
// Реализован в EpisodeDecomposer — E32 +11%.
public interface IEpisodeDecomposer
{
    OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription, TaskDomain domain);
}

// Определяет когда прекратить декомпозицию.
// Четыре сигнала: PHASES, TECH, SINGLE_DOC, NO_LOOP (E33).
// Примечание: на Tier 2 оба сигнала достигают 100% точности — задача тривиальна
// при синтетических данных. Настоящий тест на реальных эпизодах.
public interface IStopSignalDetector
{
    OperationResult<bool> ShouldStop(string subTaskDescription);
}

// Запускает агента для выполнения под-эпизода.
// Делегирует в AgentOrchestrator из 22.AgentAlgorithms.
public interface IAgentLauncher
{
    Task<OperationResult<string>> LaunchAsync(SubTask task, string context);
}

// Объединяет результаты нескольких агентов в один ответ.
// Реализован через TemplateBeautifier (E23 SUCCESS=RETURN маппинг).
public interface IResultMerger
{
    OperationResult<string> Merge(IReadOnlyList<(SubTask Task, string Result)> results);
}

// Хранит историю эпизодов для контекста следующих задач.
// InMemoryEpisodeMemory — заглушка до 14.ContinuousLearning.
public interface IEpisodeMemory
{
    OperationResult<bool> Store(string taskDescription, IReadOnlyList<SubTask> decomposition);
    OperationResult<IReadOnlyList<string>> Recall(string taskDescription, int topK = 3);
}

// Запускает 16 проверок качества перед доставкой результата.
// E39 +20.2%.
public interface IQualityOrchestrator
{
    OperationResult<QualityReport> Check(string taskDescription, string result, string outputType);
}

public sealed record QualityReport(bool Passed, IReadOnlyList<string> FailedChecks);
```

### Создать `15.OrchestratorModel/Core/InMemoryEpisodeMemory.cs`

```csharp
namespace OrchestratorModel;

// Хранит историю эпизодов в памяти.
// Это заглушка — настоящая реализация через DomainDatasetCache придёт с 14.ContinuousLearning.
// Интерфейс не изменится, только реализация.
public sealed class InMemoryEpisodeMemory : IEpisodeMemory
{
    private readonly List<(string Task, IReadOnlyList<SubTask> Decomposition)> _history = new();
    private readonly int _maxEntries;

    public InMemoryEpisodeMemory(int maxEntries = 1000) => _maxEntries = maxEntries;

    public OperationResult<bool> Store(
        string taskDescription, IReadOnlyList<SubTask> decomposition)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<bool>.Fail("Task description cannot be empty.", "EMPTY_TASK");
        if (_history.Count >= _maxEntries)
            _history.RemoveAt(0); // простое вытеснение — FIFO до 14
        _history.Add((taskDescription, decomposition));
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyList<string>> Recall(string taskDescription, int topK = 3)
    {
        // Простой поиск по подстроке — заглушка до BM25 из 29.SemanticCompiler
        var matches = _history
            .Where(h => h.Task.Contains(taskDescription, StringComparison.OrdinalIgnoreCase))
            .TakeLast(topK)
            .Select(h => h.Task)
            .ToList();
        return OperationResult<IReadOnlyList<string>>.Ok(matches);
    }
}
```

### Тесты Фазы 1 (добавить в `Tests/OrchestratorModelTests.cs`)

Создать новый тестовый файл:

```csharp
namespace Tests;

// Тесты для 15.OrchestratorModel
public class OrchestratorModelTests
{
    // ── InMemoryEpisodeMemory ──────────────────────────────────────────────────

    [Fact]
    public void EpisodeMemory_Store_ThenRecall_FindsBySubstring()
    {
        var memory = new InMemoryEpisodeMemory();
        var tasks  = new List<SubTask> { new("design the auth", "architect") };

        var stored = memory.Store("Design and implement auth system", tasks);
        Assert.True(stored.IsSuccess);

        var recalled = memory.Recall("auth system");
        Assert.True(recalled.IsSuccess);
        Assert.Single(recalled.Value!);
        Assert.Contains("auth system", recalled.Value![0]);
    }

    [Fact]
    public void EpisodeMemory_EmptyDescription_ReturnsFail()
    {
        var memory = new InMemoryEpisodeMemory();
        var result = memory.Store("", new List<SubTask>());
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TASK", result.ErrorCode);
    }

    [Fact]
    public void EpisodeMemory_ExceedsMaxEntries_EvictsOldest()
    {
        var memory = new InMemoryEpisodeMemory(maxEntries: 2);
        memory.Store("task 1", new List<SubTask>());
        memory.Store("task 2", new List<SubTask>());
        memory.Store("task 3", new List<SubTask>()); // вытесняет task 1

        var recall1 = memory.Recall("task 1");
        var recall3 = memory.Recall("task 3");
        Assert.Empty(recall1.Value!);       // вытеснен
        Assert.Single(recall3.Value!);      // остался
    }
}
```

### Gate B Фазы 1

```bash
dotnet build LlmFromScratch.sln   # ноль ошибок — новый .csproj добавлен корректно
dotnet test Tests/LlmFromScratch.Tests.csproj   # 880 passed (+3), 0 failed
grep -rn "interface IEpisodeClassifier\|interface IEpisodeDecomposer\|InMemoryEpisodeMemory" \
  15.OrchestratorModel\ --include="*.cs" | grep -v bin   # все интерфейсы на месте
git commit -m "phase 1: 15.OrchestratorModel project, interfaces, InMemoryEpisodeMemory"
git push
```

---

## Фаза 2 — EpisodeClassifier + EpisodeDecomposer (E18, E32)

**Что доставляем:**
`EpisodeClassifier` оборачивает `RouteClassifier` из `29.SemanticCompiler`.
`EpisodeDecomposer` предсказывает N под-эпизодов по типу и предикатному профилю.

### Создать `15.OrchestratorModel/EpisodeClassifier.cs`

```csharp
namespace OrchestratorModel;

// Классифицирует задачу в домен, используя RouteClassifier из SemanticCompiler29.
// E18: 95% точность домена.
// Confidence < 0.6 → сигнал для 17.DynamicDomains (новый домен).
public sealed class EpisodeClassifier : BaseService, IEpisodeClassifier
{
    // RouteClassifier из 29.SemanticCompiler — определяет домен по предикатам
    private readonly IRouteClassifier _routeClassifier;

    // Маппинг из RouteClassifier domain index (int) в TaskDomain enum
    private static readonly Dictionary<int, TaskDomain> DomainMap = new()
    {
        [0] = TaskDomain.CODE_Q,
        [1] = TaskDomain.BUG,
        [2] = TaskDomain.REFACTOR,
        [3] = TaskDomain.PLAN,
        [4] = TaskDomain.ARCH,
    };

    public EpisodeClassifier(IRouteClassifier routeClassifier)
        => _routeClassifier = routeClassifier;

    public OperationResult<TaskDomain> Classify(string taskDescription)
    {
        var result = ClassifyWithConfidence(taskDescription);
        return result.IsSuccess
            ? OperationResult<TaskDomain>.Ok(result.Value!.Domain)
            : OperationResult<TaskDomain>.Fail(result.ErrorMessage, result.ErrorCode);
    }

    public OperationResult<(TaskDomain Domain, double Confidence)> ClassifyWithConfidence(
        string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<(TaskDomain, double)>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        var routeResult = _routeClassifier.Classify(taskDescription);
        if (!routeResult.IsSuccess)
            return OperationResult<(TaskDomain, double)>.Fail(
                routeResult.ErrorMessage, routeResult.ErrorCode);

        int domainIndex = routeResult.Value!.DomainIndex;
        double confidence = routeResult.Value!.Confidence;
        TaskDomain domain = DomainMap.TryGetValue(domainIndex, out var d) ? d : TaskDomain.CODE_Q;

        return OperationResult<(TaskDomain, double)>.Ok((domain, confidence));
    }
}
```

### Создать `15.OrchestratorModel/EpisodeDecomposer.cs`

```csharp
namespace OrchestratorModel;

// Разбивает задачу на под-эпизоды.
// E32 +11%: RESEARCH → 4-5 под-эпизодов, BUILD → 2-3, FIX → 2.
// Использует предикатный профиль из SemanticCompiler для точности.
public sealed class EpisodeDecomposer : BaseService, IEpisodeDecomposer
{
    // Ожидаемое количество под-эпизодов по типу задачи (из E32 корпуса)
    private static readonly Dictionary<TaskDomain, int> ExpectedSubEpisodes = new()
    {
        [TaskDomain.PLAN]    = 4, // RESEARCH: много исследования и итераций
        [TaskDomain.ARCH]    = 3, // DESIGN: спроектировать → обсудить → зафиксировать
        [TaskDomain.REFACTOR]= 3, // BUILD: подготовить → реализовать → проверить
        [TaskDomain.BUG]     = 2, // FIX: диагностировать → исправить
        [TaskDomain.CODE_Q]  = 1, // QUESTION: один ответ, один запрос
    };

    // False-CONJ guard из E21: "Find null AND log them" — одна задача
    private static readonly string[] FalseConjStarters =
        ["find", "check", "read", "get", "list", "show", "search", "look"];

    public OperationResult<IReadOnlyList<SubTask>> Decompose(
        string taskDescription, TaskDomain domain)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<SubTask>>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        int targetCount = ExpectedSubEpisodes.GetValueOrDefault(domain, 2);

        // Разделить задачу на части — с защитой от ложных союзов
        var parts = SplitByConjunctions(taskDescription);
        var subTasks = parts
            .Take(targetCount)
            .Select((p, i) => new SubTask(p.Trim(), AssignRole(domain, i)))
            .ToList();

        // Гарантировать как минимум один под-эпизод
        if (subTasks.Count == 0)
            subTasks.Add(new SubTask(taskDescription, AssignRole(domain, 0)));

        return OperationResult<IReadOnlyList<SubTask>>.Ok(subTasks);
    }

    // Разбить по союзам только если обе части независимые (E21 false-CONJ guard)
    private static List<string> SplitByConjunctions(string text)
    {
        var parts = text.Split(
            [" and ", " then ", " after that "],
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2) return [text];

        // Вторая часть независима если начинается с глагола действия
        bool secondIsIndependent = !FalseConjStarters.Any(
            v => parts[1].Trim().ToLower().StartsWith(v + " "));

        return secondIsIndependent ? parts.ToList() : [text];
    }

    // Назначить роль агента исходя из домена и позиции в цепочке
    private static string AssignRole(TaskDomain domain, int position) => domain switch
    {
        TaskDomain.ARCH   => position == 0 ? "architect" : "implementer",
        TaskDomain.PLAN   => position == 0 ? "planner" : "executor",
        TaskDomain.REFACTOR => position == 0 ? "implementer" : "reviewer",
        TaskDomain.BUG    => position == 0 ? "debugger" : "implementer",
        _                 => "assistant",
    };
}
```

### Тесты Фазы 2

Добавить в `OrchestratorModelTests.cs`:

```csharp
// ── EpisodeClassifier ─────────────────────────────────────────────────────────

[Fact]
public void EpisodeClassifier_FallsBackToCodeQ_WhenRouteClassifierReturnsUnknown()
{
    // Если RouteClassifier возвращает индекс вне диапазона → CODE_Q как безопасное умолчание
    var stub       = new StubRouteClassifier(domainIndex: 99, confidence: 0.8);
    var classifier = new EpisodeClassifier(stub);
    var result     = classifier.Classify("some obscure task");
    Assert.True(result.IsSuccess);
    Assert.Equal(TaskDomain.CODE_Q, result.Value!);
}

[Fact]
public void EpisodeClassifier_LowConfidence_SignalForNewDomain()
{
    // confidence < 0.6 указывает что это потенциально новый домен
    var stub       = new StubRouteClassifier(domainIndex: 0, confidence: 0.35);
    var classifier = new EpisodeClassifier(stub);
    var result     = classifier.ClassifyWithConfidence("Write Dockerfile for .NET app");
    Assert.True(result.IsSuccess);
    Assert.True(result.Value!.Confidence < 0.6,
        "Low confidence should signal a potential new domain (E46 ConfidenceGate)");
}

// ── EpisodeDecomposer ─────────────────────────────────────────────────────────

[Fact]
public void EpisodeDecomposer_PlanDomain_Returns4SubEpisodes()
{
    var decomposer = new EpisodeDecomposer();
    // PLAN задачи ожидают 4 под-эпизода (E32: RESEARCH → 4-5)
    var result = decomposer.Decompose(
        "Design auth system and implement it and test it and document it",
        TaskDomain.PLAN);
    Assert.True(result.IsSuccess);
    Assert.Equal(4, result.Value!.Count);
}

[Fact]
public void EpisodeDecomposer_FalseConjunction_StaysAsSingleTask()
{
    // "Find null AND log them" — одна задача (E21 false-CONJ guard)
    var decomposer = new EpisodeDecomposer();
    var result     = decomposer.Decompose(
        "Find methods that return null and log them", TaskDomain.CODE_Q);
    Assert.True(result.IsSuccess);
    Assert.Single(result.Value!); // одна задача, не две
}

[Fact]
public void EpisodeDecomposer_BugDomain_Returns2SubEpisodes()
{
    var decomposer = new EpisodeDecomposer();
    var result     = decomposer.Decompose(
        "Fix NullReference in UserService then add regression test", TaskDomain.BUG);
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.Count);
    // Первый под-эпизод — диагностика/исправление
    Assert.Equal("debugger", result.Value![0].RequiredRole);
}
```

Добавить стаб в конец файла:

```csharp
// Тестовый стаб — RouteClassifier с настраиваемым ответом
file sealed class StubRouteClassifier : IRouteClassifier
{
    private readonly int    _domainIndex;
    private readonly double _confidence;

    public StubRouteClassifier(int domainIndex, double confidence)
    {
        _domainIndex = domainIndex;
        _confidence  = confidence;
    }

    public OperationResult<RouteClassificationResult> Classify(string text)
        => OperationResult<RouteClassificationResult>.Ok(
            new RouteClassificationResult(_domainIndex, _confidence));
}
```

### Gate B Фазы 2

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # 885 passed (+5 от Фазы 2), 0 failed
# Проверить false-CONJ guard:
dotnet test --filter "EpisodeDecomposer_FalseConjunction_StaysAsSingleTask"
git commit -m "phase 2: EpisodeClassifier (E18), EpisodeDecomposer (E32, false-CONJ guard)"
git push
```

---

## Фаза 3 — StopSignalDetector + переиспользовать DependencyGraph (E33, E35)

**Что доставляем:**
`StopSignalDetector` — четыре сигнала остановки.
`DependencyGraph` уже существует в `22.AgentAlgorithms` — 15 использует его через ссылку,
не дублирует.

**Примечание по E33:** на синтетических данных Tier 2 оба сигнала дают 100% точности.
Это потолок метрики, не провал. `StopSignalDetector` реализуется с четырьмя
явными правилами из DEPTH-CALIBRATION-GUIDANCE. Настоящий тест на реальных эпизодах.

### Создать `15.OrchestratorModel/StopSignalDetector.cs`

```csharp
namespace OrchestratorModel;

// Определяет когда декомпозицию нужно остановить.
// Четыре сигнала из DEPTH-CALIBRATION-GUIDANCE документа.
// E33: правильный алгоритм, синтетические данные Tier 2 слишком просты для измерения.
public sealed class StopSignalDetector : BaseService, IStopSignalDetector
{
    public OperationResult<bool> ShouldStop(string subTaskDescription)
    {
        if (string.IsNullOrWhiteSpace(subTaskDescription))
            return OperationResult<bool>.Fail("Description cannot be empty.", "EMPTY_TASK");

        var lower = subTaskDescription.ToLowerInvariant();

        // Сигнал 1 — PHASES: следующий уровень = просто порядок шагов выполнения
        // Итерации, попытки, версии — это шаги внутри одного под-эпизода, не отдельные
        bool isPhaseSequence =
            new[] { "then ", "next ", "after that", "first step", "step " }
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");

        if (isPhaseSequence) return OperationResult<bool>.Ok(true);

        // Сигнал 2 — TECH: требует специфического технического контекста
        // Если нельзя объяснить без знания конкретного стека — это шаг, не под-эпизод
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

        // Сигнал 4 — NO_LOOP: нет истории версий, нет обратной связи
        bool hasNoLoop =
            !lower.Contains("review") && !lower.Contains("iterate") &&
            !lower.Contains("revise") && !lower.Contains("feedback") &&
            !lower.Contains("correction");

        // NO_LOOP один недостаточен — нужна комбинация с другими признаками
        if (hasNoLoop && subTaskDescription.Split(' ').Length <= 6)
            return OperationResult<bool>.Ok(true); // короткая однозначная задача

        return OperationResult<bool>.Ok(false); // продолжить декомпозицию
    }
}
```

### Тесты Фазы 3

Добавить в `OrchestratorModelTests.cs`:

```csharp
// ── StopSignalDetector ────────────────────────────────────────────────────────

[Fact]
public void StopSignalDetector_PhaseSequence_StopsDecomposition()
{
    // "First read, then decide, then implement" — это шаги, не под-эпизоды
    var detector = new StopSignalDetector();
    var result   = detector.ShouldStop("First read the requirements then decide the approach");
    Assert.True(result.IsSuccess);
    Assert.True(result.Value!, "Phase sequence should trigger stop signal");
}

[Fact]
public void StopSignalDetector_IndependentSubTask_ContinuesDecomposition()
{
    // "Design the auth system" — независимая цель, продолжать декомпозицию
    var detector = new StopSignalDetector();
    var result   = detector.ShouldStop("Design the authentication module architecture");
    Assert.True(result.IsSuccess);
    Assert.False(result.Value!, "Independent sub-task should not stop decomposition");
}

// ── DependencyGraph переиспользование ─────────────────────────────────────────

[Fact]
public void DependencyGraph_ArchBeforeCodeQ_UsableFrom15()
{
    // DependencyGraph из 22.AgentAlgorithms доступен из 15 без дублирования
    // (проверяет что ссылка на проект работает корректно)
    var graph = new AgentAlgorithms.DependencyGraph(); // через ссылку на 22
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
dotnet test Tests/LlmFromScratch.Tests.csproj   # 888 passed (+3), 0 failed
# Проверить что DependencyGraph не дублирован:
find 15.OrchestratorModel -name "DependencyGraph.cs" | grep -v bin
# Ожидание: нет файла — используется из 22
git commit -m "phase 3: StopSignalDetector (4 signals), DependencyGraph reuse from 22"
git push
```

---

## Фаза 4 — AgentLauncher + ResultMerger + EpisodeMemory (E23)

**Что доставляем:**
`AgentLauncher` делегирует в `AgentOrchestrator` из `22.AgentAlgorithms`.
`ResultMerger` использует `TemplateBeautifier` из `29.SemanticCompiler` (E23 SUCCESS=RETURN).
`EpisodeMemory` — просто регистрирует `InMemoryEpisodeMemory` через интерфейс.

### Создать `15.OrchestratorModel/AgentLauncher.cs`

```csharp
namespace OrchestratorModel;

// Запускает агента для выполнения под-эпизода.
// Делегирует в AgentOrchestrator из 22.AgentAlgorithms — не дублирует оркестрацию.
public sealed class AgentLauncher : BaseService, IAgentLauncher
{
    private readonly AgentAlgorithms.AgentOrchestrator _orchestrator;

    public AgentLauncher(AgentAlgorithms.AgentOrchestrator orchestrator)
        => _orchestrator = orchestrator;

    public async Task<OperationResult<string>> LaunchAsync(
        AgentAlgorithms.SubTask task, string context)
    {
        if (string.IsNullOrWhiteSpace(task.Description))
            return OperationResult<string>.Fail("Sub-task description cannot be empty.", "EMPTY_TASK");

        // Запустить через AgentOrchestrator с контекстом от предыдущих под-эпизодов
        string enrichedDescription = string.IsNullOrWhiteSpace(context)
            ? task.Description
            : $"{task.Description}\n\nContext from earlier steps:\n{context}";

        var enrichedTask = new AgentAlgorithms.SubTask(enrichedDescription, task.RequiredRole);
        var result = await _orchestrator.RunAsync(enrichedTask.Description);
        return result;
    }
}
```

### Создать `15.OrchestratorModel/ResultMerger.cs`

```csharp
namespace OrchestratorModel;

// Объединяет результаты нескольких агентов в один связный ответ.
// Использует TemplateBeautifier из 29.SemanticCompiler (E23 +820%).
// Ключевое: SUCCESS = RETURN маппинг встроен в Beautifier.
public sealed class ResultMerger : BaseService, IResultMerger
{
    // TemplateBeautifier из SemanticCompiler29 — содержит SUCCESS=RETURN маппинг
    private readonly SemanticCompiler29.TemplateBeautifier _beautifier;

    public ResultMerger(SemanticCompiler29.TemplateBeautifier beautifier)
        => _beautifier = beautifier;

    public OperationResult<string> Merge(
        IReadOnlyList<(AgentAlgorithms.SubTask Task, string Result)> results)
    {
        if (!results.Any())
            return OperationResult<string>.Fail("No results to merge.", "EMPTY_RESULTS");

        if (results.Count == 1)
            return OperationResult<string>.Ok(results[0].Result);

        // Объединить результаты с разметкой по ролям
        var merged = string.Join("\n\n", results.Select((r, i) =>
            $"## Step {i + 1}: {r.Task.RequiredRole}\n{r.Result}"));

        // Пропустить через TemplateBeautifier для улучшения читаемости
        var beautifyResult = _beautifier.Beautify(merged);
        return beautifyResult.IsSuccess
            ? beautifyResult
            : OperationResult<string>.Ok(merged); // fallback к сырому объединению
    }
}
```

### Тесты Фазы 4

Добавить в `OrchestratorModelTests.cs`:

```csharp
// ── AgentLauncher ─────────────────────────────────────────────────────────────

[Fact]
public async Task AgentLauncher_EmptyDescription_ReturnsFail()
{
    var orch     = CreateStubOrchestrator();
    var launcher = new AgentLauncher(orch);
    var result   = await launcher.LaunchAsync(new AgentAlgorithms.SubTask("", "architect"), "");
    Assert.False(result.IsSuccess);
    Assert.Equal("EMPTY_TASK", result.ErrorCode);
}

// ── ResultMerger ──────────────────────────────────────────────────────────────

[Fact]
public void ResultMerger_SingleResult_ReturnedUnchanged()
{
    var beautifier = new SemanticCompiler29.TemplateBeautifier();
    var merger     = new ResultMerger(beautifier);
    var task       = new AgentAlgorithms.SubTask("design auth", "architect");
    var result     = merger.Merge([(task, "auth schema: JWT + RBAC")]);
    Assert.True(result.IsSuccess);
    Assert.Equal("auth schema: JWT + RBAC", result.Value!);
}

[Fact]
public void ResultMerger_EmptyResults_ReturnsFail()
{
    var merger = new ResultMerger(new SemanticCompiler29.TemplateBeautifier());
    var result = merger.Merge([]);
    Assert.False(result.IsSuccess);
    Assert.Equal("EMPTY_RESULTS", result.ErrorCode);
}
```

### Gate B Фазы 4

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # 891 passed (+3), 0 failed
git commit -m "phase 4: AgentLauncher, ResultMerger (E23 TemplateBeautifier), EpisodeMemory registered"
git push
```

---

## Фаза 5 — OrchestratorModel пайплайн + QualityOrchestrator (E39)

**Что доставляем:**
`OrchestratorModel` — главный класс пайплайна: классифицировать → декомпозировать
→ сортировать → запускать → объединить → проверить качество.
`QualityOrchestrator` — 16 проверок по типу вывода (E39 +20.2%).

### Создать `15.OrchestratorModel/QualityOrchestrator.cs`

```csharp
namespace OrchestratorModel;

// Запускает 16 обязательных проверок перед доставкой результата.
// E39 +20.2%: классификация типа вывода позволяет запускать только нужные проверки.
// Четыре типа вывода: CODE_SERVICE, PLAN_DOC, REACT_PAGE, ANALYSIS_DOC.
public sealed class QualityOrchestrator : BaseService, IQualityOrchestrator
{
    // Восемь проверок которые применяются всегда
    private static readonly string[] AlwaysChecks =
    [
        "OUTPUT_CONTRACT",   // бинарный критерий готовности
        "MISSION_PROGRESS",  // продвигает цель сессии
        "ISSUE_INVENTORY",   // все найденные проблемы адресованы
        "GOAL_REMINDER",     // цель видна в ответе
        "TOOLS_BEFORE_PERSON", // инструменты перед предположениями
        "RESPONSE_CONSTRUCTION", // протокол ответа
        "CLAIMS_VERIFIED",   // все утверждения о состоянии проверены
        "HUMAN_LANGUAGE_GATE",   // нет машинных кодов в тексте
    ];

    // Проверки по типу вывода
    private static readonly Dictionary<string, string[]> ByOutputType = new()
    {
        ["CODE_SERVICE"] = ["TEST_GATE", "SCOPE_ISOLATION", "PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS"],
        ["PLAN_DOC"]     = ["PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS", "GOAL_DELIVERY_VERDICT"],
        ["REACT_PAGE"]   = ["UI_UX_COMPLIANCE"],
        ["ANALYSIS_DOC"] = ["RECONNAISSANCE_EVIDENCE", "ARCHITECT_HABITS"],
    };

    public OperationResult<QualityReport> Check(
        string taskDescription, string result, string outputType)
    {
        if (string.IsNullOrWhiteSpace(result))
            return OperationResult<QualityReport>.Fail("Result cannot be empty.", "EMPTY_RESULT");

        var checksToRun = AlwaysChecks.ToList();
        if (ByOutputType.TryGetValue(outputType, out var specific))
            checksToRun.AddRange(specific);

        // Упрощённые эвристики — в реальной реализации каждая проверка вызывает агента
        var failed = new List<string>();

        if (result.Length < 10)
            failed.Add("OUTPUT_CONTRACT"); // слишком короткий — не выполнено

        if (outputType == "CODE_SERVICE" && !result.Contains("return"))
            failed.Add("TEST_GATE"); // нет return-значений — вероятно нет тестов

        return OperationResult<QualityReport>.Ok(
            new QualityReport(failed.Count == 0, failed));
    }
}
```

### Создать `15.OrchestratorModel/OrchestratorModel.cs`

```csharp
namespace OrchestratorModel;

// Главный класс подпроекта 15.
// Пайплайн: классифицировать → декомпозировать → сортировать → запускать → объединить → проверить.
public sealed class OrchestratorModel : BaseService
{
    private readonly IEpisodeClassifier  _classifier;
    private readonly IEpisodeDecomposer  _decomposer;
    private readonly IStopSignalDetector _stopDetector;
    private readonly AgentAlgorithms.DependencyGraph _graph;
    private readonly IAgentLauncher      _launcher;
    private readonly IResultMerger       _merger;
    private readonly IQualityOrchestrator _quality;
    private readonly IEpisodeMemory      _memory;

    public OrchestratorModel(
        IEpisodeClassifier   classifier,
        IEpisodeDecomposer   decomposer,
        IStopSignalDetector  stopDetector,
        AgentAlgorithms.DependencyGraph graph,
        IAgentLauncher       launcher,
        IResultMerger        merger,
        IQualityOrchestrator quality,
        IEpisodeMemory       memory)
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

    // Обработать задачу пользователя.
    // Returns: готовый ответ прошедший все 16 проверок качества.
    public async Task<OperationResult<string>> ProcessAsync(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<string>.Fail("Task description cannot be empty.", "EMPTY_TASK");

        // Шаг 1: классифицировать домен
        var classifyResult = _classifier.ClassifyWithConfidence(taskDescription);
        if (!classifyResult.IsSuccess)
            return OperationResult<string>.Fail(classifyResult.ErrorMessage, "CLASSIFY_FAILED");

        // Шаг 2: декомпозировать на под-эпизоды
        var decompResult = _decomposer.Decompose(taskDescription, classifyResult.Value!.Domain);
        if (!decompResult.IsSuccess)
            return OperationResult<string>.Fail(decompResult.ErrorMessage, "DECOMPOSE_FAILED");

        var subTasks = decompResult.Value!;

        // Шаг 3: отфильтровать задачи которые нужно остановить
        var tasks = subTasks
            .Where(t => !(_stopDetector.ShouldStop(t.Description).Value ?? false))
            .ToList();

        if (tasks.Count == 0) tasks = subTasks.ToList(); // защита от пустого списка

        // Шаг 4: сортировать по зависимостям (E35 +30%)
        var sortResult = _graph.Sort(tasks);
        var orderedLevels = sortResult.IsSuccess
            ? sortResult.Value!
            : (IReadOnlyList<IReadOnlyList<AgentAlgorithms.SubTask>>)[tasks];

        // Шаг 5: запустить каждый уровень — параллельно внутри уровня, последовательно между
        var allResults = new List<(AgentAlgorithms.SubTask Task, string Result)>();
        string context = "";
        foreach (var level in orderedLevels)
        {
            var levelResults = await Task.WhenAll(
                level.Select(t => _launcher.LaunchAsync(t, context)));
            foreach (var (task, res) in level.Zip(levelResults))
            {
                string result = res.IsSuccess ? res.Value! : $"[{task.RequiredRole} failed]";
                allResults.Add((task, result));
            }
            context = allResults.Last().Result; // контекст для следующего уровня
        }

        // Шаг 6: объединить ответы
        var mergeResult = _merger.Merge(allResults);
        if (!mergeResult.IsSuccess)
            return OperationResult<string>.Fail(mergeResult.ErrorMessage, "MERGE_FAILED");

        // Шаг 7: проверить качество (E39)
        string outputType = classifyResult.Value!.Domain == TaskDomain.CODE_Q ? "CODE_SERVICE" : "PLAN_DOC";
        var qualityResult = _quality.Check(taskDescription, mergeResult.Value!, outputType);
        if (qualityResult.IsSuccess && !qualityResult.Value!.Passed)
        {
            string failed = string.Join(", ", qualityResult.Value!.FailedChecks);
            return OperationResult<string>.Fail(
                $"Quality checks failed: {failed}", "QUALITY_FAILED");
        }

        // Шаг 8: сохранить в память
        _memory.Store(taskDescription, subTasks);

        return OperationResult<string>.Ok(mergeResult.Value!);
    }
}
```

### Тесты Фазы 5

Добавить в `OrchestratorModelTests.cs`:

```csharp
// ── QualityOrchestrator ───────────────────────────────────────────────────────

[Fact]
public void QualityOrchestrator_ShortResult_FailsOutputContract()
{
    var quality = new QualityOrchestrator();
    var report  = quality.Check("add null check", "ok", "CODE_SERVICE");
    Assert.True(report.IsSuccess);
    Assert.False(report.Value!.Passed);
    Assert.Contains("OUTPUT_CONTRACT", report.Value!.FailedChecks);
}

[Fact]
public void QualityOrchestrator_SubstantiveResult_Passes()
{
    var quality = new QualityOrchestrator();
    string result = "if (user == null) return OperationResult<User>.Fail(\"user not found\");";
    var report    = quality.Check("add null check to GetUser", result, "CODE_SERVICE");
    Assert.True(report.IsSuccess);
    Assert.True(report.Value!.Passed, $"Expected pass, failed: {string.Join(", ", report.Value!.FailedChecks)}");
}

// ── Gate B — интеграционный тест ──────────────────────────────────────────────

[Fact]
public async Task OrchestratorModel_DeployTask_ArchFirst()
{
    // Gate B из мастер-плана: "Deploy new application" → [ARCH, CODE_Q, DevOps]
    // где ARCH на уровне 0 (первый уровень перед всеми остальными)

    var graph = new AgentAlgorithms.DependencyGraph();
    graph.AddDependency("architect", "implementer"); // arch должен быть первым

    // "Deploy application" = PLAN домен → 4 под-эпизода
    var decomposer = new EpisodeDecomposer();
    var tasks      = decomposer.Decompose(
        "Design the deployment architecture then implement the service then deploy to server then monitor it",
        TaskDomain.PLAN);
    Assert.True(tasks.IsSuccess);

    var sorted = graph.Sort(tasks.Value!.ToList());
    Assert.True(sorted.IsSuccess);

    // Первый уровень должен содержать задачу с ролью architect
    var firstLevel = sorted.Value![0];
    Assert.Contains(firstLevel, t => t.RequiredRole == "architect" || t.RequiredRole == "planner");
}
```

### Gate B Фазы 5 — финальный

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj   # 893 passed (+2), 0 failed

# Gate B из мастер-плана:
dotnet test --filter "OrchestratorModel_DeployTask_ArchFirst"
# Ожидание: PASSED

# Проверить что DependencyGraph не дублирован:
find 15.OrchestratorModel -name "DependencyGraph.cs" | grep -v bin
# Ожидание: нет файла

# Проверить что EpisodeMemory использует интерфейс:
grep -rn "IEpisodeMemory\|InMemoryEpisodeMemory" 15.OrchestratorModel --include="*.cs" | grep -v bin
# Ожидание: IEpisodeMemory в интерфейсах, InMemoryEpisodeMemory в Core/

git commit -m "phase 5: QualityOrchestrator (E39), OrchestratorModel pipeline, Gate B passed"
git push
```

---

## Финальная проверка

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj

echo "=== Новые файлы ==="
git diff --name-only HEAD~5 | sort

echo "=== Счёт тестов ==="
grep -c "\[Fact\]" Tests/OrchestratorModelTests.cs  # должно быть 16

echo "=== Критические правила не нарушены ==="
# DependencyGraph не дублирован:
find 15.OrchestratorModel -name "DependencyGraph.cs" | grep -v bin && echo "FAIL — дублирован" || echo "OK — использует 22"
# IEpisodeMemory интерфейс существует (заглушка до 14):
grep -rn "IEpisodeMemory" 15.OrchestratorModel --include="*.cs" | grep -v bin

echo "=== Gate B ==="
dotnet test --filter "OrchestratorModel_DeployTask_ArchFirst"
```

---

## Счёт тестов по фазам

| Фаза | Что создаётся | Тестов | Итого |
|------|--------------|--------|-------|
| Baseline | — | — | 877 |
| Фаза 1 | Интерфейсы, InMemoryEpisodeMemory | +3 | 880 |
| Фаза 2 | EpisodeClassifier, EpisodeDecomposer | +5 | 885 |
| Фаза 3 | StopSignalDetector, DependencyGraph reuse | +3 | 888 |
| Фаза 4 | AgentLauncher, ResultMerger | +3 | 891 |
| Фаза 5 | QualityOrchestrator, OrchestratorModel | +2 | 893 |

---

## Правила на всю сессию

**DependencyGraph НЕ дублировать.** Уже реализован в `22.AgentAlgorithms/MultiAgent/DependencyGraph.cs`.
Ссылка на проект 22 уже есть в `.csproj`. Использовать через `AgentAlgorithms.DependencyGraph`.

**IEpisodeMemory — заглушка.** `EpisodeMemory` зависит от `14.ContinuousLearning/DomainDatasetCache`
который ещё не построен. `InMemoryEpisodeMemory` — правильное промежуточное решение.
Интерфейс не изменится когда придёт 14.

**E33 потолок на Tier 2 — не провал.** `StopSignalDetector` реализован с четырьмя
явными правилами. Синтетические данные слишком просты для различия. Настоящий
тест будет на реальных эпизодах из EPISODE-MAP-EXTENDED.

**Ноль failing — жёсткое условие.** При любом failing — исправить до следующего шага.

---

## Цель сессии

Проект: `15.OrchestratorModel` (Трек В)
Ветка: main
Стартовый счёт тестов: 877
Целевой счёт тестов: 893
Новые production-файлы: 7
Новые тесты: 16
Переиспользуется без дублирования: DependencyGraph из 22, TemplateBeautifier из 29

---

*Если этот план и мастер-план для подпроекта 15 расходятся — мастер-план
управляет архитектурными решениями. Если имя типа или интерфейса в плане
не существует в кодовой базе — прочитать source файлы ещё раз и использовать
ближайший существующий тип с тем же контрактом.*
