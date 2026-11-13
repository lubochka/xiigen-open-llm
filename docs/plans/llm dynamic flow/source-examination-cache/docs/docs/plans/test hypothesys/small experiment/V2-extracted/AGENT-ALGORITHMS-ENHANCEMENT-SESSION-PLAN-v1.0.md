# SESSION PLAN — AgentAlgorithms Enhancement v5.0
## Внедрение результатов экспериментов E11, E35, E38v2, E49 в проект 22
## Запускать ПОСЛЕ завершения PROJECTS-21-23-IMPROVEMENT-SESSION-PLAN-v1.0

---

You are implementing experiment-validated improvements to `22.AgentAlgorithms`.
The implementation is specified in `AGENT-ALGORITHMS-ENHANCEMENT-PLAN-v5_0.md`.
Before doing anything else, complete the loading sequence below in order.

---

## Pre-condition gate

This session assumes `PROJECTS-21-23-IMPROVEMENT-SESSION-PLAN-v1.0` is complete.
Verify before starting:

```bash
cd /home/claude/llm_mvp_core
git log --oneline -3
# Must show the commit "phase 4: fuzzy patch, multi-patch, false-CONJ guard"
# If that commit is not present, stop — run the prerequisites plan first.
```

---

## Step 1 — Establish the baseline

```bash
cd /home/claude/llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj
# Record: total passed, total failed
# Expected baseline: >= 113 tests passing (89 original + 24 from prerequisites plan)
# Zero failures is a hard gate — do not proceed if any test fails.
```

## Step 2 — Read every file you will touch

Read each file completely before the session begins. State one specific fact from
the last section of each file as proof you finished reading it.

```bash
# Files that will be changed:
cat 22.AgentAlgorithms/Context/SemanticRanker.cs          # TextChunk is defined here
cat 22.AgentAlgorithms/Context/ContextWindowManager.cs
cat 22.AgentAlgorithms/Fabric/Interfaces.cs               # where new interfaces go
cat 22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs
cat 22.AgentAlgorithms/MultiAgent/SubAgentDefinition.cs

# Files that will receive new tests:
cat Tests/AgentAlgorithmsTests.cs
```

## Step 3 — Read the specification

Read `AGENT-ALGORITHMS-ENHANCEMENT-PLAN-v5_0.md` completely.
This document contains the code for every class you are about to implement.
Do not write code from memory — copy from the specification.

**One interface mismatch to note before starting:**
The plan uses `ICodeIndex` in `KnnSkillFilter`. This interface does not exist in the
codebase. Use `ISemanticSearchProvider` instead — it already has
`SearchAsync(query, topK)` which is exactly the method needed.
Replace every `ICodeIndex _index` with `ISemanticSearchProvider _search`
and every `_index.Search(...)` with `await _search.SearchAsync(...)`.

## Step 4 — State where you are

After Steps 1-3, write one paragraph stating:
- Which branch you are on
- What the baseline test count is
- That you have read both the source files and the specification
- Which phase you are starting

---

## ROUND CONTRACT — runs before every response

```
ROUND CONTRACT:
1. Baseline was:        [N tests passing, 0 failing]
2. Current test count:  [run dotnet test right now — report actual numbers]
3. This round produced: [files changed, tests added, commands run]
4. Phase gate requires: [exact gate criteria for the current phase]
5. Gap check:           [what from item 4 is still missing]
```

Item 5 must show zero gaps before the phase is marked done.

---

## GATE — runs after every phase

```bash
dotnet build LlmFromScratch.sln              # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj # zero failures, count >= baseline
git diff --name-only                          # confirm only expected files changed
```

---

## Phase 1 — ContentType enum + token budget calculator

**What this delivers:**
- `ContentType` enum: CSharpCode, Documentation, NaturalLanguage, SkillDocument
- `ITokenBudgetCalculator` interface in `Fabric/Interfaces.cs`
- `AstTokenBudgetCalculator` implementing the interface
- Updated `TextChunk` record with `Type` field (default: NaturalLanguage)

**Backward compatibility requirement:**
`TextChunk` is a record with positional parameters. Adding `Type` as the last
parameter with a default value keeps all existing `new TextChunk("id", "content")`
and `new TextChunk("id", "content", score)` calls compiling without changes.
Verify this before moving on.

### Files to create or change

**`Fabric/Interfaces.cs`** — append after existing interfaces:

```csharp
// Token budget calculator — different content types pack differently per E11.
// C# AST tokens are 5.2x more information-dense than prose BPE tokens.
public interface ITokenBudgetCalculator
{
    int Estimate(string content, ContentType type);
}
```

**New file: `Context/ContentType.cs`**

```csharp
namespace AgentAlgorithms;

public enum ContentType
{
    CSharpCode,        // AST-compressed: one node ≈ 20 characters (E11)
    Documentation,     // prose, slightly denser BPE coverage
    NaturalLanguage,   // standard 4-character approximation
    SkillDocument,     // markdown prose
}
```

**New file: `Context/AstTokenBudgetCalculator.cs`**

```csharp
namespace AgentAlgorithms;

// Estimates token cost by content type.
// C# code is measured in AST nodes (one node per ~20 characters) rather than BPE tokens.
// E11 showed AST representation is 5.2x more information-dense than raw BPE.
// This means a C# file that looks 5x larger than prose actually costs the same to represent.
public sealed class AstTokenBudgetCalculator : ITokenBudgetCalculator
{
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

**`Context/SemanticRanker.cs`** — update TextChunk record:

```csharp
// Add Type as a last positional parameter with default value.
// All existing call sites compile unchanged because the default is NaturalLanguage.
public sealed record TextChunk(
    string Id,
    string Content,
    double Score = 0.0,
    ContentType Type = ContentType.NaturalLanguage);  // NEW — default preserves all existing callsites
```

### Tests for Phase 1

Add to `AgentAlgorithmsTests.cs`:

```csharp
// ── AstTokenBudgetCalculator ──────────────────────────────────────────────────

[Fact]
public void AstBudget_CSharpCode_CostsLessThanNaturalLanguage()
{
    var calc    = new AstTokenBudgetCalculator();
    string text = new string('x', 400); // same length for both
    int csharp  = calc.Estimate(text, ContentType.CSharpCode);
    int prose   = calc.Estimate(text, ContentType.NaturalLanguage);
    // C# costs 20 tokens (400/20) vs 100 tokens (400/4) for the same byte count
    Assert.True(csharp < prose,
        $"C# should cost fewer tokens than prose: {csharp} vs {prose}");
}

[Fact]
public void AstBudget_CSharpCode_OneNodePerTwentyChars()
{
    var calc   = new AstTokenBudgetCalculator();
    int tokens = calc.Estimate(new string('x', 100), ContentType.CSharpCode);
    Assert.Equal(5, tokens); // 100 / 20 = 5
}

[Fact]
public void TextChunk_DefaultType_IsNaturalLanguage()
{
    // Existing call sites that omit Type should still get NaturalLanguage
    var chunk = new TextChunk("id", "content");
    Assert.Equal(ContentType.NaturalLanguage, chunk.Type);
}

[Fact]
public void TextChunk_ExplicitType_IsPreserved()
{
    var chunk = new TextChunk("id", "void M() {}", 0.9, ContentType.CSharpCode);
    Assert.Equal(ContentType.CSharpCode, chunk.Type);
}
```

### Phase 1 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj  # count >= baseline + 4
# Verify backward compatibility:
grep -n "new TextChunk" Tests/AgentAlgorithmsTests.cs
# All existing TextChunk constructors must compile without Type argument
git commit -m "phase 1: ContentType, AstTokenBudgetCalculator, TextChunk.Type"
git push
```

---

## Phase 2 — KnnSkillFilter + ISkillFilter interface

**What this delivers:**
- `ISkillFilter` interface in `Fabric/Interfaces.cs`
- `KnnSkillFilter` implementing it using `ISemanticSearchProvider`
- No changes to existing files in this phase

**Interface adaptation:**
The specification uses `ICodeIndex` — use `ISemanticSearchProvider` instead.
`ISemanticSearchProvider.SearchAsync(query, topK)` returns
`List<(string ChunkId, double Score)>` which maps directly to what is needed.

### Files to create or change

**`Fabric/Interfaces.cs`** — append:

```csharp
// Skill filter — selects the relevant skills for a task from the full skill catalogue.
// E38v2: KNN on task predicates retrieves the correct 6 skills at 95% precision.
// load_reduction = 86% vs loading all skills. fallback_rate = 22% when confidence < 0.40.
public interface ISkillFilter
{
    OperationResult<IReadOnlyList<TextChunk>> Filter(
        IReadOnlyList<TextChunk> skillCandidates,
        string                  taskDescription,
        int                     topK                = 5,
        double                  confidenceThreshold = 0.40);
}
```

**New file: `Context/KnnSkillFilter.cs`**

```csharp
namespace AgentAlgorithms;

// Skill retriever based on E38v2 experiment results.
// Selects top-K skills using semantic search, with confidence-based fallback.
// When the top result dominates the score distribution (confidence >= threshold),
// returns only top-K skills. Otherwise falls back to returning all candidates.
public sealed class KnnSkillFilter : ISkillFilter
{
    // ISemanticSearchProvider replaces the ICodeIndex from the spec — same contract.
    private readonly ISemanticSearchProvider _search;

    public KnnSkillFilter(ISemanticSearchProvider search) => _search = search;

    public OperationResult<IReadOnlyList<TextChunk>> Filter(
        IReadOnlyList<TextChunk> skillCandidates,
        string                  taskDescription,
        int                     topK                = 5,
        double                  confidenceThreshold = 0.40)
    {
        if (!skillCandidates.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates);

        var searchResult = _search.SearchAsync(taskDescription, topK: topK)
            .GetAwaiter().GetResult(); // sync wrapper — skill filter is called in sync context

        if (!searchResult.IsSuccess || !searchResult.Value!.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates); // fallback: all

        // Confidence = top score share of total top-K score mass
        double topScore   = searchResult.Value!.First().Score;
        double totalScore = searchResult.Value!.Sum(r => r.Score) + 1e-9;
        double confidence = topScore / totalScore;

        if (confidence < confidenceThreshold)
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates); // fallback: all

        var topIds    = searchResult.Value!.Select(r => r.ChunkId).ToHashSet();
        var filtered  = skillCandidates.Where(c => topIds.Contains(c.Id)).ToList();
        return OperationResult<IReadOnlyList<TextChunk>>.Ok(filtered);
    }
}
```

### Tests for Phase 2

Add to `AgentAlgorithmsTests.cs`. The tests need a stub for `ISemanticSearchProvider`
that the existing `StubSemanticSearchProvider` already provides — reuse it.

```csharp
// ── KnnSkillFilter ────────────────────────────────────────────────────────────

[Fact]
public void SkillFilter_HighConfidence_ReturnsTopK()
{
    // Score distribution: top result has 80% of total mass → high confidence
    var search  = new StubSemanticSearchProvider([("skill-A", 0.80), ("skill-B", 0.20)]);
    var filter  = new KnnSkillFilter(search);
    var skills  = Enumerable.Range(1, 20)
        .Select(i => new TextChunk($"skill-{(char)('A' + i - 1)}", $"Skill {i}",
                                   Type: ContentType.SkillDocument))
        .ToList();

    var result = filter.Filter(skills, "add null check", topK: 2, confidenceThreshold: 0.40);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value!.Count <= 2,
        $"Expected <= 2 skills, got {result.Value!.Count}");
}

[Fact]
public void SkillFilter_LowConfidence_ReturnsFallbackAll()
{
    // Uniform score distribution → low confidence → return all
    var evenScores = Enumerable.Range(0, 5).Select(i => ($"s{i}", 0.20)).ToList();
    var search     = new StubSemanticSearchProvider(evenScores);
    var filter     = new KnnSkillFilter(search);
    var skills     = evenScores.Select(x => new TextChunk(x.Item1, "skill content")).ToList();

    var result = filter.Filter(skills, "some task", confidenceThreshold: 0.60);

    Assert.True(result.IsSuccess);
    // All 5 skills returned because confidence is below threshold
    Assert.Equal(5, result.Value!.Count);
}

[Fact]
public void SkillFilter_EmptyCandidates_ReturnsEmpty()
{
    var search = new StubSemanticSearchProvider([]);
    var filter = new KnnSkillFilter(search);

    var result = filter.Filter([], "task");

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value!);
}
```

### Phase 2 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj  # count >= baseline + 7
git commit -m "phase 2: ISkillFilter, KnnSkillFilter using ISemanticSearchProvider"
git push
```

---

## Phase 3 — ContextWindowManager: add budget and skill filter

**What this delivers:**
- ContextWindowManager takes `ITokenBudgetCalculator` and `ISkillFilter` as optional
  constructor parameters (with pass-through defaults so the existing test does not break)
- `AssembleAsync` uses content type for token counting and filters skills before ranking
- Existing test `ContextWindowManager_AssembleAsync_ReturnsRankedChunks` must still pass

**Backward compatibility requirement:**
The existing test constructs `new ContextWindowManager(ranker, compactor, tokenBudget: 100_000)`.
Make `budget` and `skills` optional with defaults:
- `budget = null` → use a `DefaultTokenBudget` that falls back to `text.Length / 4` for all types
- `skills = null` → use a `PassThroughSkillFilter` that returns all candidates unchanged

Both defaults must be private inner classes — not part of the public API.

### File to change: `Context/ContextWindowManager.cs`

```csharp
namespace AgentAlgorithms;

public sealed class ContextWindowManager
{
    private readonly SemanticRanker         _ranker;
    private readonly ContextCompactor       _compactor;
    private readonly ITokenBudgetCalculator _budget;
    private readonly ISkillFilter           _skills;
    private readonly int                    _tokenBudget;

    public ContextWindowManager(
        SemanticRanker         ranker,
        ContextCompactor       compactor,
        int                    tokenBudget = 8000,
        ITokenBudgetCalculator? budget     = null,   // null → legacy length/4 estimate
        ISkillFilter?           skills     = null)   // null → pass through all candidates
    {
        _ranker      = ranker;
        _compactor   = compactor;
        _tokenBudget = tokenBudget;
        _budget      = budget ?? new LegacyTokenBudget();
        _skills      = skills ?? new PassThroughSkillFilter();
    }

    public async Task<OperationResult<IReadOnlyList<TextChunk>>> AssembleAsync(
        IReadOnlyList<TextChunk> candidates,
        string                   query)
    {
        // Separate skills from code content for independent filtering
        var skillChunks = candidates
            .Where(c => c.Type == ContentType.SkillDocument).ToList();
        var codeChunks  = candidates
            .Where(c => c.Type != ContentType.SkillDocument).ToList();

        // Filter skills through KNN (E38v2: ~6 skills instead of 46)
        var filteredSkillsResult = _skills.Filter(skillChunks, query);
        if (!filteredSkillsResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                filteredSkillsResult.ErrorMessage, "SKILL_FILTER_FAILED");

        // Combine and rank
        var allCandidates = codeChunks.Concat(filteredSkillsResult.Value!).ToList();

        var rankResult = await _ranker.RankAsync(allCandidates, query);
        if (!rankResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                rankResult.ErrorMessage, rankResult.ErrorCode);

        // Fill the token window using content-type-aware budget (E11)
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

    // Legacy fallback: length / 4 for all types (existing behaviour before E11)
    private sealed class LegacyTokenBudget : ITokenBudgetCalculator
    {
        public int Estimate(string content, ContentType _)
            => (int)Math.Ceiling(content.Length / 4.0);
    }

    // Pass-through: return all skill candidates unchanged (no filtering)
    private sealed class PassThroughSkillFilter : ISkillFilter
    {
        public OperationResult<IReadOnlyList<TextChunk>> Filter(
            IReadOnlyList<TextChunk> candidates, string _, int topK = 5, double threshold = 0.40)
            => OperationResult<IReadOnlyList<TextChunk>>.Ok(candidates);
    }
}
```

### Tests for Phase 3

Add to `AgentAlgorithmsTests.cs`:

```csharp
// ── ContextWindowManager (enhanced) ──────────────────────────────────────────

[Fact]
public async Task ContextWindowManager_SkillFilter_ReducesSkillCount()
{
    // 30 skill chunks but the filter should select only top 2
    var ranker   = new SemanticRanker(new StubSemanticSearchProvider(
        Enumerable.Range(0, 30).Select(i => ($"skill-{i}", 1.0 - i * 0.01)).ToList()));
    var compactor = new ContextCompactor();

    // Filter that keeps only first 2 skill IDs
    var twoSkillSearch = new StubSemanticSearchProvider([("skill-0", 0.80), ("skill-1", 0.15)]);
    var skillFilter    = new KnnSkillFilter(twoSkillSearch);
    var budget         = new AstTokenBudgetCalculator();

    var manager = new ContextWindowManager(ranker, compactor,
        tokenBudget: 100_000, budget: budget, skills: skillFilter);

    var chunks = Enumerable.Range(0, 30)
        .Select(i => new TextChunk($"skill-{i}", $"Skill content {i}",
                                   Type: ContentType.SkillDocument))
        .ToList<TextChunk>();

    var result = await manager.AssembleAsync(chunks, "add null check");

    Assert.True(result.IsSuccess);
    // Skill filter reduced 30 candidates to at most 2
    Assert.True(result.Value!.Count <= 2,
        $"Expected <= 2 skills after filtering, got {result.Value!.Count}");
}

[Fact]
public async Task ContextWindowManager_AstBudget_FitsMoreCSharpThanProse()
{
    // With AST budget, C# code costs 5x less than the same byte count in prose.
    // This means more C# files fit in the same token window.
    var provider  = new StubSemanticSearchProvider(
        Enumerable.Range(0, 20).Select(i => ($"c{i}", 1.0 - i * 0.01)).ToList());
    var ranker    = new SemanticRanker(provider);
    var compactor = new ContextCompactor();
    var budget    = new AstTokenBudgetCalculator();

    // 8000 character C# file costs 400 AST tokens vs 2000 prose tokens
    // With tokenBudget=2000: AST budget fits it, length/4 budget barely fits one
    var manager = new ContextWindowManager(ranker, compactor,
        tokenBudget: 2000, budget: budget);

    string csharpContent = new string('x', 8000); // 400 AST tokens, would be 2000 prose
    var chunks = Enumerable.Range(0, 5)
        .Select(i => new TextChunk($"c{i}", csharpContent, Type: ContentType.CSharpCode))
        .ToList<TextChunk>();

    var result = await manager.AssembleAsync(chunks, "refactor service");

    Assert.True(result.IsSuccess);
    // With AST budget: 5 × 400 = 2000 tokens, all 5 fit
    // With prose budget: 5 × 2000 = 10000, only 1 would fit
    Assert.True(result.Value!.Count > 1,
        "AST budget should fit multiple C# files where prose budget fits only one");
}

[Fact]
public async Task ContextWindowManager_Fallback_WhenNoSkillsProvided_ReturnsAll()
{
    // When no ISkillFilter is provided, existing behaviour: return all candidates
    var provider  = new StubSemanticSearchProvider([("s1", 0.9)]);
    var ranker    = new SemanticRanker(provider);
    var compactor = new ContextCompactor();
    // No budget, no skills → both use legacy/passthrough defaults
    var manager   = new ContextWindowManager(ranker, compactor, tokenBudget: 100_000);

    var chunks = new List<TextChunk>
    {
        new("s1", "content", Type: ContentType.SkillDocument),
    };
    var result = await manager.AssembleAsync(chunks, "any task");

    Assert.True(result.IsSuccess);
    Assert.Single(result.Value!); // passthrough returns all 1 skill
}
```

### Phase 3 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj  # count >= baseline + 10
# Verify existing test still passes — check by name:
dotnet test Tests/LlmFromScratch.Tests.csproj \
  --filter "ContextWindowManager_AssembleAsync_ReturnsRankedChunks"
git commit -m "phase 3: ContextWindowManager with AstTokenBudget and KnnSkillFilter"
git push
```

---

## Phase 4 — DependencyGraph

**What this delivers:**
- `DependencyGraph` class with topological sort (BFS by levels)
- Each level contains tasks that can run in parallel
- Cycle detection returns `DEPENDENCY_CYCLE` error code

### New file: `MultiAgent/DependencyGraph.cs`

Implement exactly as specified in `AGENT-ALGORITHMS-ENHANCEMENT-PLAN-v5_0.md`
section "2а — DependencyGraph".
No changes to other files in this phase.

### Tests for Phase 4

```csharp
// ── DependencyGraph ───────────────────────────────────────────────────────────

[Fact]
public void DependencyGraph_ArchBeforeCodeQ_ArchInFirstLevel()
{
    var graph = new DependencyGraph();
    graph.AddDependency("ARCH", "CODE_Q"); // CODE_Q depends on ARCH

    var tasks  = new List<SubTask>
    {
        new("design the service",     "ARCH"),
        new("implement the service",  "CODE_Q"),
    };
    var result = graph.Sort(tasks);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.Count); // two levels
    Assert.Equal("ARCH",   result.Value![0][0].RequiredRole);
    Assert.Equal("CODE_Q", result.Value![1][0].RequiredRole);
}

[Fact]
public void DependencyGraph_IndependentTasks_RunAtSameLevel()
{
    var graph = new DependencyGraph();
    graph.AddDependency("ARCH", "CODE_Q");
    graph.AddDependency("ARCH", "REFACTOR"); // both CODE_Q and REFACTOR depend on ARCH

    var tasks = new List<SubTask>
    {
        new("design",    "ARCH"),
        new("implement", "CODE_Q"),
        new("refactor",  "REFACTOR"),
    };
    var result = graph.Sort(tasks);

    Assert.True(result.IsSuccess);
    // ARCH in level 0; CODE_Q and REFACTOR in level 1 (parallel)
    Assert.Equal("ARCH", result.Value![0][0].RequiredRole);
    var level1Roles = result.Value![1].Select(t => t.RequiredRole).OrderBy(r => r).ToList();
    Assert.Equal(new[] { "CODE_Q", "REFACTOR" }, level1Roles);
}

[Fact]
public void DependencyGraph_CycleDetected_ReturnsCycleError()
{
    var graph = new DependencyGraph();
    graph.AddDependency("A", "B");
    graph.AddDependency("B", "A"); // cycle

    var tasks = new List<SubTask>
    {
        new("task a", "A"),
        new("task b", "B"),
    };
    var result = graph.Sort(tasks);

    Assert.False(result.IsSuccess);
    Assert.Equal("DEPENDENCY_CYCLE", result.ErrorCode);
}

[Fact]
public void DependencyGraph_NoDependencies_AllTasksInOneLevel()
{
    var graph = new DependencyGraph(); // no edges

    var tasks = new List<SubTask>
    {
        new("write tests",       "tester"),
        new("write docs",        "writer"),
        new("review the output", "reader"),
    };
    var result = graph.Sort(tasks);

    Assert.True(result.IsSuccess);
    Assert.Single(result.Value!); // one level, all three tasks together
    Assert.Equal(3, result.Value![0].Count);
}
```

### Phase 4 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj  # count >= baseline + 14
git commit -m "phase 4: DependencyGraph with topological sort and cycle detection"
git push
```

---

## Phase 5 — AgentOrchestrator: dependency-ordered parallel execution

**What this delivers:**
- AgentOrchestrator accepts optional `DependencyGraph` (backward compatible: `null` → use default)
- `RunAsync` sorts tasks by dependency levels and runs each level with `Task.WhenAll`
- Results from earlier levels are passed as context to later levels
- Default dependency graph uses the relationships from E35 and E49

### File to change: `MultiAgent/AgentOrchestrator.cs`

Implement exactly as specified in the plan section "2б — Обновить AgentOrchestrator".
The `DependencyGraph? deps = null` parameter with `BuildDefaultDependencies()` fallback
ensures all existing tests compile and pass without changes.

### Tests for Phase 5

```csharp
// ── AgentOrchestrator (enhanced) ─────────────────────────────────────────────

[Fact]
public async Task AgentOrchestrator_ContextPropagated_LaterTaskSeesEarlierResult()
{
    // ARCH runs first and returns "arch result"
    // CODE_Q runs second and should receive "arch result" in its task description
    var bus = new ContextCapturingMessageBus();
    var decomposer = new ComplexTaskDecomposer();

    var graph = new DependencyGraph();
    graph.AddDependency("architect", "coder");

    var orchestrator = new AgentOrchestrator(bus, decomposer, graph);
    orchestrator.Register(new SubAgentDefinition("Architect", "architect", []));
    orchestrator.Register(new SubAgentDefinition("Coder",     "coder",     []));

    bus.SetResponse("Architect", "the architecture is defined");

    // "design auth system and implement it" → two sub-tasks
    var result = await orchestrator.RunAsync(
        "design auth system and implement it");

    Assert.True(result.IsSuccess);
    // The coder task description should contain the architect's output
    var coderTask = bus.ReceivedTasks.FirstOrDefault(t => t.Agent == "Coder");
    if (coderTask != default)
        Assert.Contains("architecture is defined", coderTask.Task);
}

[Fact]
public async Task AgentOrchestrator_MissingSubAgent_ReturnsFailure()
{
    // Unchanged from original test — must still pass
    var decomposer = new ComplexTaskDecomposer();
    var bus        = new StubAgentMessageBus();
    var orch       = new AgentOrchestrator(bus, decomposer);
    // No agents registered — any decomposed task will fail

    var result = await orch.RunAsync("refactor and test the service");
    Assert.False(result.IsSuccess);
    Assert.Equal("MISSING_SUBAGENT", result.ErrorCode);
}
```

**Note:** `ContextCapturingMessageBus` is a test helper that records received tasks
and returns configurable responses. Add it to the test file's `file sealed class` section:

```csharp
file sealed class ContextCapturingMessageBus : IAgentMessageBus
{
    private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);
    public List<(string Agent, string Task)> ReceivedTasks { get; } = new();

    public void SetResponse(string agentName, string response)
        => _responses[agentName] = response;

    public Task<OperationResult<string>> AssignTaskAsync(string agentName, string task)
    {
        ReceivedTasks.Add((agentName, task));
        string response = _responses.TryGetValue(agentName, out var r) ? r : $"{agentName}: done";
        return Task.FromResult(OperationResult<string>.Ok(response));
    }
}
```

### Phase 5 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj  # count >= baseline + 16
# Verify existing AgentOrchestrator tests still pass:
dotnet test Tests/LlmFromScratch.Tests.csproj \
  --filter "AgentOrchestrator_RunsAllSubTasks"
dotnet test Tests/LlmFromScratch.Tests.csproj \
  --filter "AgentOrchestrator_SingleTask_AssignsToAgent"
git commit -m "phase 5: AgentOrchestrator dependency-ordered parallel execution"
git push
```

---

## Final verification

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj

echo "=== New files created ==="
git diff --name-only HEAD~5 | sort

echo "=== Test counts ==="
grep -c "\[Fact\]" Tests/AgentAlgorithmsTests.cs   # was 28, should be >= 44
grep -c "\[Fact\]" Tests/AgentInfrastructureTests.cs # unchanged from prerequisites

echo "=== Interface check ==="
grep -n "ITokenBudgetCalculator\|ISkillFilter" \
  22.AgentAlgorithms/Fabric/Interfaces.cs

echo "=== Backward compat check ==="
# These must compile:
grep -n "new ContextWindowManager(ranker, compactor, tokenBudget:" \
  Tests/AgentAlgorithmsTests.cs
grep -n "new TextChunk(\"c1\"" Tests/AgentAlgorithmsTests.cs
grep -n "new AgentOrchestrator(bus, decomposer)" Tests/AgentAlgorithmsTests.cs
```

---

## What each phase delivers, linked to experiments

| Phase | Files | From experiments |
|---|---|---|
| 1 | ContentType, AstTokenBudgetCalculator, TextChunk.Type | E11 AST efficiency 5.2x |
| 2 | ISkillFilter, KnnSkillFilter | E38v2 +100%, 86% load reduction |
| 3 | ContextWindowManager updated | E11 + E38v2 combined |
| 4 | DependencyGraph | E35 +30% dependency accuracy |
| 5 | AgentOrchestrator updated | E35 + E49 +44% graph coverage |

---

## Rules that apply throughout this session

**Zero failures — hard gate.** At every phase boundary, zero failures required.
If tests fail after a change, fix them before committing.

**Backward compatibility is required.** Three call sites must compile without changes:
`new ContextWindowManager(ranker, compactor, tokenBudget: 100_000)`,
`new TextChunk("id", "content")`, and `new AgentOrchestrator(bus, decomposer)`.
Verify by grep before each commit.

**Use `ISemanticSearchProvider` not `ICodeIndex`.** The specification uses a type
that does not exist. The correct type is `ISemanticSearchProvider` which has the same
`SearchAsync(query, topK)` contract. Every occurrence of `ICodeIndex` in the spec
maps to `ISemanticSearchProvider` in the implementation.

**One commit per phase.** Each phase ends with a commit and push.
The phase is not complete until the push succeeds.

---

## Session target

Project: 22.AgentAlgorithms (plus Tests)
Branch: main
Pre-condition: PROJECTS-21-23-IMPROVEMENT-SESSION-PLAN-v1.0 complete
Starting test count: >= 113
Target test count: >= 129
New production files: 3 (ContentType.cs, AstTokenBudgetCalculator.cs, KnnSkillFilter.cs, DependencyGraph.cs)
New tests: 16 minimum

---

*If this plan and the AGENT-ALGORITHMS-ENHANCEMENT-PLAN-v5_0.md disagree,
AGENT-ALGORITHMS-ENHANCEMENT-PLAN-v5_0.md governs the code.
If a type named in either plan does not exist in the codebase,
use the closest existing interface with the same contract.*
