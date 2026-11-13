# Plan: E35, E38V2, E40V2, E34V2 — Integration
# Version: 1
# Date: 2026-05-07
# Floor before: 1265
# Floor after:  1285 (+20 tests)

---

## Четыре задачи

**E35 Dependency Detector** — алгоритм `IsSequential` доказан (+30%), но в
`DependencyGraph` (22.AgentAlgorithms) не подключён. `DependencyGraph` сейчас
требует ручного `AddDependency` — статической конфигурации. E35 даёт
динамическое обнаружение: определяет по предикатам задач (CREATE_OP/GET_OP,
пересечение тегов) какие пары SEQUENTIAL, а какие PARALLEL. Флаг
`UseE35DependencyDetection` в `LogicalExecutionFlags` — он уже там живёт.

**E38V2 Skill Retriever** — KNN по тегам из `FamilyCollapser`, fallback когда
max_sim < 0.40. Результат бинарный для KNN матрицы: если fallback → 0.0,
иначе score из Jaccard. `SkillTriggerLibrary` живёт только в экспериментах.
Флаг `UseSkillRetriever` в `LogicalExecutionFlags`.

**E40V2 TrustScorer** — циклическая зависимость: `16` ссылается на `21`,
поэтому `21` использует reflection. Решение: контракт (`ITrustScorer`,
`SourceFeatures`, `TrustScorerOutput`, `TrustLevel`) переезжает в `00.Core`.
`21` ссылается на `00.Core` напрямую — reflection убирается.

**E34V2 BM25+KNN для выбора replay** — сейчас `FindReplayManifests` берёт
последние 3 манифеста по домену независимо от релевантности текущему промпту.
Если ранжировать их через BM25+KNN fusion по скомпилированному тексту промпта
— DPO получает более близкие negative примеры. Флаг
`UseReplayFusionRanking` в `LogicalExecutionFlags`.

---

## Part A — E40V2: перенос контракта TrustScorer в 00.Core

### Phase A1 — Контракт в 00.Core

**Modified:** `00.Core/LogicalContracts.cs`

Добавить в конец файла (без изменения существующих типов):

```csharp
// Trust level classification used by TrustScorer (E40V2).
public enum TrustLevel { Low, Medium, High }

// Input features for TrustScorer.Score — 7 signals calibrated in E40V2.
public sealed record SourceFeatures(
    bool IsInternal,
    bool HasCIPassing,
    bool HasTests,
    int  UpdateDaysAgo,
    int  PopularityScore,
    bool IsOfficialDocs,
    bool DomainMatch);

// Output of TrustScorer.Score.
public sealed record TrustScorerOutput(double Score, TrustLevel Level);

// Interface for trust scoring — implemented by 16.AutonomousLearning,
// consumed by 21.AgentInfrastructure without circular dependency.
public interface ITrustScorer
{
    OperationResult<TrustScorerOutput> Score(SourceFeatures features);
}
```

`TrustLevel`, `SourceFeatures`, `TrustScorerOutput` уже определены в
`16.AutonomousLearning/Core/Interfaces.cs` — там они остаются как type aliases
(или удаляются и заменяются using из `00.Core`) чтобы не сломать существующие тесты.

**Modified:** `16.AutonomousLearning/TrustScorer/TrustScorer.cs`

`TrustScorer` объявляет `: ITrustScorer` — реализует интерфейс из `00.Core`.

**Modified:** `21.AgentInfrastructure/Logical/LogicalExperimentOptimizations.cs`

`AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust` принимает
`ITrustScorer? trustScorer = null` вместо reflection:

```csharp
public static LogicalUnitTrustState ScoreCapabilityTrust(
    LogicalCapabilityTrainingRequest request,
    IReadOnlyList<TrainingBranchPlanView> branchPlans,
    IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
    ITrustScorer? trustScorer = null)      // ← прямая зависимость, не reflection
{
    if (trustScorer is null)
        return LogicalUnitTrustState.Probationary;

    var hasTests = branchPlans.Any(p => p.PredicateIds.Any(id =>
        string.Equals(id, "TEST", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(id, "QA",   StringComparison.OrdinalIgnoreCase)));
    var domainMatch = branchPlans.Any(p => p.DomainIds.Any(d =>
        string.Equals(d, request.Domain.ToString(), StringComparison.OrdinalIgnoreCase)));

    var features = new SourceFeatures(
        IsInternal:     true,
        HasCIPassing:   true,
        HasTests:       hasTests,
        UpdateDaysAgo:  0,
        PopularityScore: replayManifests.Count > 0 ? 3 : 2,
        IsOfficialDocs: false,
        DomainMatch:    domainMatch);

    var result = trustScorer.Score(features);
    if (!result.IsSuccess) return LogicalUnitTrustState.Probationary;

    return result.Value!.Level switch
    {
        TrustLevel.High   => LogicalUnitTrustState.Trusted,
        TrustLevel.Medium => LogicalUnitTrustState.Probationary,
        _                 => LogicalUnitTrustState.Untrusted
    };
}
```

`DefaultLogicalCapabilityTrainer` получает `ITrustScorer?` через конструктор
(optional, default null → Probationary как сейчас).

### Gate A1

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.
Reflection код удалён. Существующие тесты `TrustScorer` проходят.

---

## Part B — E35: динамическое обнаружение зависимостей

### Phase B1 — Перенос IsSequential в 22.AgentAlgorithms

**Modified:** `22.AgentAlgorithms/MultiAgent/DependencyGraph.cs`

Добавить статический метод `IsSequentialPair` рядом с `AddDependency`:

```csharp
// E35: detect whether task A must precede task B based on predicate overlap.
// Returns true (SEQUENTIAL) when A produces output that B consumes,
// or when A and B share ≥ 2 predicate tags (tight coupling).
// Returns false (PARALLEL) otherwise.
// Proven: 0% → 30% parallel opportunity rate on synthetic pairs (E35 tier 2).
public static bool IsSequentialPair(string taskA, string taskB)
{
    var aTags   = FamilyCollapser.CollapseString(taskA).ToHashSet();
    var bTags   = FamilyCollapser.CollapseString(taskB).ToHashSet();
    bool aCreates = aTags.Contains("CREATE_OP") || aTags.Contains("UPDATE_OP");
    bool bGets    = bTags.Contains("GET_OP");
    bool overlap  = aTags.Count(t => bTags.Contains(t)) >= 2;
    return (aCreates && bGets) || overlap;
}
```

`FamilyCollapser` уже доступен через project reference `12.SemanticCompiler` в
`22.AgentAlgorithms` — проверить csproj.

### Phase B2 — Подключить в AdaptiveLogicalOrchestrator

**Modified:** `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

В методе `ExecuteBranchesAsync` (или где строится порядок выполнения веток)
добавить условный шаг — когда `flags.UseE35DependencyDetection == true` и
веток больше одной, прогнать пары через `DependencyGraph.IsSequentialPair`
перед тем как расставить их по уровням параллельности.

Флаг `UseE35DependencyDetection` уже существует в `LogicalExecutionFlags` — он
там объявлен среди `OptimizationFlagNames`. Нужно только добавить его в
`OptimizationFlagNames` если там нет (проверить), и wire логику.

```csharp
// E35: когда флаг включён — обнаруживать параллельные ветки динамически.
if (flags.UseE35DependencyDetection && decisions.Length > 1)
{
    var dynamicGraph = new DependencyGraph();
    for (int i = 0; i < decisions.Length; i++)
    for (int j = i + 1; j < decisions.Length; j++)
    {
        if (DependencyGraph.IsSequentialPair(
                decisions[i].SubPhaseDescription,
                decisions[j].SubPhaseDescription))
            dynamicGraph.AddDependency(decisions[i].SubPhaseId, decisions[j].SubPhaseId);
    }
    // Заменить текущий граф динамически построенным
    decisions = ReorderByGraph(decisions, dynamicGraph);
}
```

### Gate B

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Part C — E38V2: SkillRetriever в production

### Phase C1 — Перенос SkillTriggerLibrary

**New file:** `22.AgentAlgorithms/Skills/SkillRetriever.cs`

Переносит из `35.ExperimentsV2/Infrastructure/SkillTriggerLibrary.cs`:
- `SkillTriggerLibrary` (static class со всеми триггерами)
- `SkillRetriever` (новый класс, логика из `E38SkillRetriever`)

```csharp
namespace AgentAlgorithms.Skills;

// Production skill retriever — E38V2 proven: 47.4% → 94.7% precision@5.
// KNN by Jaccard on FamilyCollapser tags + confidence fallback.
// When max_sim < threshold → fallback returns full category (binary signal: 0.0 for KNN matrix).
// fallbackRate=89.5% at current registry size — grows as SkillTriggerLibrary expands.
public sealed class SkillRetriever
{
    private const double ConfidenceThreshold = 0.40;
    private const int    TopK               = 5;

    // Retrieve top-K skills for a task description.
    // Returns (skills, isFallback).
    // isFallback=true when confidence < threshold — KNN matrix should treat score as 0.
    public (IReadOnlyList<SkillTriggerLibrary.SkillTrigger> Skills, bool IsFallback)
        Retrieve(string taskDescription, int? expectedCategory = null)
    {
        var qTags  = FamilyCollapser.CollapseString(taskDescription).ToHashSet();
        var ranked = SkillTriggerLibrary.All
            .Where(s => !s.AlwaysOn)
            .Select(s => (
                Skill: s,
                Sim:   JaccardSim(qTags,
                           FamilyCollapser.CollapseString(s.Condition).ToHashSet())))
            .OrderByDescending(x => x.Sim)
            .ToList();

        double maxSim = ranked.Count > 0 ? ranked[0].Sim : 0;

        if (maxSim < ConfidenceThreshold)
        {
            // Fallback: return full category if known, else all always-on skills.
            var fallback = expectedCategory.HasValue
                ? SkillTriggerLibrary.All
                    .Where(s => s.AlwaysOn || s.Category == expectedCategory.Value)
                    .ToList()
                : SkillTriggerLibrary.AlwaysRequired.ToList();
            return (fallback, IsFallback: true);
        }

        var selected = new List<SkillTriggerLibrary.SkillTrigger>(
            SkillTriggerLibrary.AlwaysRequired);
        selected.AddRange(ranked.Select(x => x.Skill).Take(TopK - selected.Count));
        return (selected, IsFallback: false);
    }

    // KNN matrix score: 0.0 when fallback, Jaccard similarity otherwise.
    public double ScoreForKnn(string taskDescription, string skillCondition)
    {
        var qTags = FamilyCollapser.CollapseString(taskDescription).ToHashSet();
        var sTags = FamilyCollapser.CollapseString(skillCondition).ToHashSet();
        double sim = JaccardSim(qTags, sTags);
        return sim >= ConfidenceThreshold ? sim : 0.0;
    }

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(x => b.Contains(x));
        int union  = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }
}
```

### Phase C2 — Флаг в LogicalExecutionFlags

**Modified:** `00.Core/LogicalContracts.cs`

Добавить `UseSkillRetriever` в `OptimizationFlagNames` и в поля:

```csharp
// E38V2: use KNN skill retriever instead of always loading all skills.
// When confidence < 0.40 (fallback) → KNN matrix score = 0.0.
// Default false — fallbackRate=89.5% at current registry size, needs more triggers.
public bool UseSkillRetriever { get; init; } = false;
```

### Phase C3 — Wire в AdaptiveLogicalOrchestrator

Когда `flags.UseSkillRetriever == true` — перед выбором units для ветки
вызвать `_skillRetriever.Retrieve(request.UserRequest)` и использовать
`ScoreForKnn` как дополнительный сигнал при скоринге кандидатов.

### Gate C

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Part D — E34V2: BM25+KNN для выбора replay

### Phase D1 — ReplayRanker

**New file:** `21.AgentInfrastructure/Logical/ReplayFusionRanker.cs`

```csharp
namespace AgentInfrastructure.Logical;

// Ranks replay manifests by relevance to the current compiled prompt using
// BM25+KNN fusion — same algorithm as E34V2 inference scoring.
// E34V2: hit@1 72.2% → 77.8% (+7.7%). Applied to replay selection:
// DPO receives negative examples closest to the current prompt in predicate space
// rather than simply the most recent 3 manifests.
internal static class ReplayFusionRanker
{
    // Return manifests ordered by BM25+KNN fusion score against compiledText.
    // Higher score = more relevant = better negative example for DPO.
    // When compiledText is null/empty — returns manifests in original order (no ranking).
    public static IReadOnlyList<GeneratedCapabilityCheckpointManifest> Rank(
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> manifests,
        string? compiledText,
        int topK = 3)
    {
        if (string.IsNullOrWhiteSpace(compiledText) || manifests.Count <= topK)
            return manifests.Take(topK).ToList();

        var queryTokens = Tokenize(compiledText);

        return manifests
            .Select(m =>
            {
                var lexical = ComputeBm25Like(queryTokens, m.TrainingText, manifests.Count);
                var vector  = ComputePredicateOverlap(compiledText, m.TrainingText);
                var fused   = (vector * 0.45) + (lexical * 0.35) + (vector * 0.20);
                return (Manifest: m, Score: fused);
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Manifest)
            .ToList();
    }

    // Predicate overlap as proxy for vector similarity — no embedding needed.
    private static double ComputePredicateOverlap(string a, string b)
    {
        var aToks = Tokenize(a);
        var bToks = Tokenize(b);
        if (aToks.Count == 0 || bToks.Count == 0) return 0;
        int inter = aToks.Keys.Count(k => bToks.ContainsKey(k));
        int union  = aToks.Count + bToks.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static double ComputeBm25Like(
        Dictionary<string, int> queryTokens,
        string document, int corpusSize)
    {
        // Simplified BM25 — same constants as E34V2 ScoreCandidates
        const double k1 = 1.2, b = 0.75;
        var docTokens = Tokenize(document);
        if (docTokens.Count == 0) return 0;
        int docLen = docTokens.Values.Sum();
        double avgLen = Math.Max(1, docLen); // single doc approximation
        double score = 0;
        foreach (var (token, qFreq) in queryTokens)
        {
            if (!docTokens.TryGetValue(token, out int tf)) continue;
            double idf = Math.Log(1 + (corpusSize + 0.5) / 1.5);
            double num = tf * (k1 + 1);
            double den = tf + k1 * (1 - b + b * docLen / avgLen);
            score += idf * num / Math.Max(1e-9, den);
        }
        return score;
    }

    private static Dictionary<string, int> Tokenize(string text)
    {
        var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var t in TrainingVectorBuilder.NormalizeRoots(text).Where(t => t.Length > 1))
            tokens[t] = tokens.TryGetValue(t, out int c) ? c + 1 : 1;
        return tokens;
    }
}
```

### Phase D2 — Флаг и wire в FindReplayManifests

**Modified:** `00.Core/LogicalContracts.cs`

```csharp
// E34V2: rank replay manifests by BM25+KNN fusion against compiled prompt
// instead of taking the most recent 3 by timestamp.
// Proven at inference: hit@1 +7.7%. Applied to replay selection for DPO negative examples.
public bool UseReplayFusionRanking { get; init; } = false;
```

**Modified:** `DefaultLogicalCapabilityTrainer` — `FindReplayManifests` получает
`compiledText` параметр и применяет ранкер когда флаг включён:

```csharp
private GeneratedCapabilityCheckpointManifest[] FindReplayManifests(
    string domainAdapterTag,
    string? compiledText,
    bool useReplayFusionRanking)
{
    var all = LoadGeneratedCapabilityManifests()
        .Where(m => string.Equals(m.DomainTag, domainAdapterTag, StringComparison.Ordinal))
        .OrderByDescending(m => m.CreatedAtUtc)
        .Take(10)           // расширяем кандидатный пул для ранкера
        .ToArray();

    if (!useReplayFusionRanking || string.IsNullOrWhiteSpace(compiledText))
        return all.Take(3).ToArray();

    return ReplayFusionRanker.Rank(all, compiledText, topK: 3).ToArray();
}
```

### Gate D

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase E — Tests

**New file:** `Tests/E35E38V2E40V2E34V2IntegrationTests.cs`

### E40V2 — TrustScorer без reflection

**Test 1 — ITrustScorer доступен в 00.Core без ссылки на 16**
```
Given: using AgentInfrastructure (ссылается на 00.Core)
When:  ITrustScorer it = null — компилируется без ProjectReference на 16
Then:  компиляция успешна
```

**Test 2 — TrustScorer HIGH: IsInternal + HasCIPassing + IsOfficialDocs**
```
Given: SourceFeatures(IsInternal=true, HasCIPassing=true, HasTests=true,
                      UpdateDaysAgo=0, PopularityScore=3,
                      IsOfficialDocs=true, DomainMatch=true)
When:  TrustScorer.Score(features)
Then:  result.Value.Level == TrustLevel.High
       result.Value.Score >= 0.70
```

**Test 3 — TrustScorer LOW: все false, 365 дней**
```
Given: SourceFeatures(false, false, false, 365, 0, false, false)
When:  Score(features)
Then:  result.Value.Level == TrustLevel.Low
       result.Value.Score < 0.30
```

**Test 4 — ScoreCapabilityTrust без reflection: trustScorer=null → Probationary**
```
Given: trustScorer = null
When:  AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(
           request, branchPlans, replayManifests, trustScorer: null)
Then:  result == LogicalUnitTrustState.Probationary
```

**Test 5 — ScoreCapabilityTrust с реальным scorer: TEST предикат → High**
```
Given: branchPlans с PredicateIds содержащим "TEST"
       trustScorer = new TrustScorer(storage)
When:  ScoreCapabilityTrust called
Then:  result == LogicalUnitTrustState.Trusted
```

### E35 — Dependency Detector

**Test 6 — IsSequentialPair: CREATE_OP → GET_OP = SEQUENTIAL**
```
Given: taskA = "Design the database schema"  (CREATE_OP)
       taskB = "Implement the data access layer" (GET_OP)
When:  DependencyGraph.IsSequentialPair(taskA, taskB)
Then:  true
```

**Test 7 — IsSequentialPair: независимые задачи = PARALLEL**
```
Given: taskA = "Write unit tests"
       taskB = "Write the documentation"
When:  DependencyGraph.IsSequentialPair(taskA, taskB)
Then:  false
```

**Test 8 — IsSequentialPair: overlap ≥ 2 тегов = SEQUENTIAL**
```
Given: taskA содержит теги [AUTH, IMPLEMENT, TEST]
       taskB содержит теги [AUTH, IMPLEMENT, DEPLOY]
       (пересечение = AUTH + IMPLEMENT = 2)
When:  DependencyGraph.IsSequentialPair(taskA, taskB)
Then:  true
```

**Test 9 — Флаг UseE35DependencyDetection=false: граф не меняется**
```
Given: два решения с независимыми описаниями
       flags с UseE35DependencyDetection = false
When:  ExecuteBranchesAsync called
Then:  порядок веток такой же как был до вызова
```

### E38V2 — SkillRetriever

**Test 10 — SkillRetriever.Retrieve: релевантный запрос → не fallback**
```
Given: taskDescription = "Review the auth controller for UseGuards decorators"
When:  retriever.Retrieve(taskDescription)
Then:  isFallback == false
       skills содержат как минимум один скилл из AUTH категории
```

**Test 11 — SkillRetriever.ScoreForKnn: fallback → 0.0**
```
Given: taskDescription = "xyz abc 123" (нет совпадений с тегами скиллов)
       skillCondition = "totally different topic"
When:  ScoreForKnn(taskDescription, skillCondition)
Then:  result == 0.0
```

**Test 12 — SkillRetriever.ScoreForKnn: точное совпадение тегов → > 0**
```
Given: taskDescription содержит "auth guard controller"
       skillCondition = "controller generated without UseGuards or Roles auth declaration"
       (оба дают пересекающиеся теги через FamilyCollapser)
When:  ScoreForKnn(taskDescription, skillCondition)
Then:  result > 0.0
```

**Test 13 — AlwaysRequired скиллы всегда включены**
```
When:  SkillTriggerLibrary.AlwaysRequired
Then:  count > 0
       все имеют AlwaysOn == true
```

### E34V2 — Replay Fusion Ranking

**Test 14 — ReplayFusionRanker: релевантный манифест ранжируется выше**
```
Given: manifest_a.TrainingText = "auth guard controller UseGuards TEST"
       manifest_b.TrainingText = "database migration schema"
       compiledText = "auth guard controller review"
When:  ReplayFusionRanker.Rank([manifest_a, manifest_b], compiledText, topK=1)
Then:  result[0] == manifest_a
       (auth манифест ближе к auth запросу)
```

**Test 15 — ReplayFusionRanker: пустой compiledText → оригинальный порядок**
```
Given: manifests = [m1, m2, m3], compiledText = ""
When:  Rank(manifests, compiledText, topK=3)
Then:  result == [m1, m2, m3]  (без изменений)
```

**Test 16 — FindReplayManifests с UseReplayFusionRanking=false: последние 3**
```
Given: 5 манифестов одного домена, сохранённых по времени
       UseReplayFusionRanking = false
When:  FindReplayManifests(domain, compiledText, false)
Then:  result == последние 3 по CreatedAtUtc
```

**Test 17 — FindReplayManifests с UseReplayFusionRanking=true: релевантные 3**
```
Given: 5 манифестов одного домена
       compiledText близкий к одному конкретному манифесту
       UseReplayFusionRanking = true
When:  FindReplayManifests(domain, compiledText, true)
Then:  result.Count == 3
       наиболее релевантный манифест присутствует в result
```

### Интеграция — флаги в LogicalExecutionFlags

**Test 18 — UseSkillRetriever=false по умолчанию**
```
When:  new LogicalExecutionFlags()
Then:  flags.UseSkillRetriever == false
```

**Test 19 — UseReplayFusionRanking=false по умолчанию**
```
When:  new LogicalExecutionFlags()
Then:  flags.UseReplayFusionRanking == false
```

**Test 20 — UseE35DependencyDetection уже в OptimizationFlagNames**
```
When:  LogicalExecutionFlags.OptimizationFlagNames
Then:  contains "UseE35DependencyDetection"
```

### Gate E

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1285, zero failures.
(+20 новых тестов)

---

## Итоговая таблица всех флагов

| Флаг | Проект | Default | Когда включать |
|---|---|---|---|
| `UseRegexPreprocessor` | SemanticCompilerOptions | true | Всегда для CSharp |
| `UseStructuralKnn` | SemanticCompilerOptions | false | Когда tier 2 пройден |
| `UseMethodKnn` | SemanticCompilerOptions | true | Tier 2 пройден |
| `UseFileActionKnn` | SemanticCompilerOptions | false | Когда датасет сбалансирован |
| `UseE35DependencyDetection` | LogicalExecutionFlags | false | Когда ≥2 ветки в запросе |
| `UseSkillRetriever` | LogicalExecutionFlags | false | Когда реестр вырастет |
| `UseReplayFusionRanking` | LogicalExecutionFlags | false | Следующий calibration run |

---

## Execution order

Part A → Part B → Part C → Part D → Phase E.
Каждый gate: `dotnet test`, floor 1265, zero failures.
Phase E поднимает floor до 1285.

---

## Floor summary

| After | Floor |
|---|---:|
| Start | 1265 |
| Parts A–D | 1265 |
| Phase E | 1285 |
