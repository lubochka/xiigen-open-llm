# 14.ContinuousLearning — Session Plan v1.0
## LoRA per-project specialisation
## Claude Code execution plan — self-contained, no cross-references

---

## Goal advancement (Gate D)

**Goal statement (verbatim):** "implement 14.ContinuousLearning — LoRA adaptation on the user's
project so the model specialises on their codebase, with feedback routing
(ACCEPTED / EDITED / REJECTED) producing DPO pairs and incremental fine-tuning."

**Before this session:** 29.SemanticCompiler complete, ISemanticCompiler interface exists,
no 14.ContinuousLearning/ directory, model has no project-specific knowledge.

**After this session:** ProjectIndexer scans .cs files → DomainDatasetCache stores labelled
examples → FeedbackRouter converts user corrections into DPO pairs →
IncrementalTrainer fine-tunes LoRA on accumulated examples → DomainModelRegistry
saves and reloads adapters. The model can specialise on a codebase. The
first real gradient-based test of the E28 hypothesis (project adaptation improves
accuracy) becomes runnable.

**Acceptance criterion:** `dotnet test --filter ContinuousLearning` reports 899 passed,
0 failed, and `grep -rn "ISemanticCompiler" 14.ContinuousLearning/` returns at
least one line (interface injected, not copied).

---

## Pre-condition gate — run before touching anything

```bash
# 0. Real .csproj filenames — read BEFORE writing the .csproj for 14
find . -name "*.csproj" \( -path "*/10.*" -o -path "*/11.*" -o -path "*/21.*" -o -path "*/29.*" \) \
  | grep -v bin | grep -v ".claude" | sort
# Record exact filenames. Use these in 14.ContinuousLearning.csproj ProjectReference entries.
# DO NOT use filenames from memory — copy from this output.

# 1. BaseService constructor signature — read BEFORE writing ProjectIndexer
grep -n "public abstract class BaseService\|protected BaseService\|protected readonly" \
  21.AgentInfrastructure/Core/Core.cs
# Record the exact constructor parameters.
# ProjectIndexer must call base(...) with the correct argument types.

# 2. ISemanticCompiler exists (14 will inject this, not copy it)
grep -rn "public interface ISemanticCompiler" 29.SemanticCompiler/
# Expected: one line in Fabric/Interfaces.cs

# 3. LoRA public API
grep -n "public static.*Apply\|public record LoraConfig" 10.LoRA/*.cs
# Expected: LoraApplicator.Apply and LoraConfig present

# 4. DPO public API
grep -n "public class DpoTrainer\|public class DpoPair\|public class DpoDataset" 11.DPO/*.cs
# Expected: all three present

# 5. Actual test baseline — run now, record the number
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Record N = actual passing count.
#
# PARALLEL TRACK NOTE: 14.ContinuousLearning and 15.OrchestratorModel are
# parallel tracks. Whichever runs second will see a higher baseline.
# If N = 877: 15.OrchestratorModel has not run yet — targets below are correct.
# If N = 893: 15.OrchestratorModel ran first (+16) — add 16 to every phase target:
#             880→896, 886→902, 891→907, 895→911, 899→915
# If N = 877 + something else: check git log to identify what ran, recalculate.
# Formula: adjusted_target = stated_target + (N - 877)

# 6. Target folder does not exist
ls 14.ContinuousLearning 2>/dev/null && echo "EXISTS — STOP" || echo "CLEAN"
# Expected: CLEAN
```

State before starting:
- Branch: `[git branch --show-current]`
- Tests: `[actual dotnet test result — N passing]`
- Adjusted targets if N ≠ 877: `[880 + (N-877), 886 + (N-877), ...899 + (N-877)]`
- All pre-conditions: ALL PASS or stop and report

---

## Loading sequence

```bash
cat CLAUDE.md        # 10 rules — OperationResult, BaseService, Fabric First
cat AGENTS.md        # 6 OP rules + Round Contract
cat docs/DECISIONS-LOCKED.md  # D-04 canonical domains, D-09 max_weight formula
```

---

## Round Contract — prepend to every response

```
ROUND CONTRACT:
1. Baseline:   877 passed, 0 failed  [verify before session: run dotnet test]
2. Current:    [run dotnet test now — actual numbers, not from memory]
3. Produced:   [list every file created or changed this turn]
4. Phase gate: [exact passing condition for this phase]
5. Gap:        [what remains between item 3 and item 4]
```

---

## Principles compliance (applies to every file in this plan)

The following rules are derived from CLAUDE.md and DECISIONS-LOCKED.md.
Every file written in this session must satisfy all five.

**Principle 1 — OperationResult<T>:** every public method returns
`OperationResult<T>`. No exceptions thrown for business logic. No nulls returned.

**Principle 2 — BaseService:** every service class extends `BaseService`
from `21.AgentInfrastructure/Core/Core.cs`.

**Principle 3 — Fabric First:** external dependencies pass through interfaces
declared in `14.ContinuousLearning/Fabric/Interfaces.cs`. Concrete types
are only instantiated in the project's Program.cs or test helpers.

**Principle 4 — ISemanticCompiler injected, not copied:** `ProjectIndexer`
receives `ISemanticCompiler` via constructor. No reimplementation of
StackDetector, SemanticNormalizer, FamilyCollapser, or LogicExtractor
is permitted in this project.
```bash
# Verify after Phase 2:
grep -rn "ISemanticCompiler" 14.ContinuousLearning/
# Must return at least one line. Zero lines = violation.
```

**Principle 5 — max_weight is dynamic, not a constant (D-09):**
`FeedbackRouter` must compute:
```csharp
double maxWeight = totalExamples / ((double)numClasses * rareExamples);
// Example: 1000 examples, 5 domains, 50 security examples → maxWeight = 4.0
// NEVER hardcode ×10 — caused catastrophic forgetting at small dataset sizes
```

---

## Component map — all five must appear in exactly one phase each

| Component | Phase | File |
|---|---|---|
| Core types (TrainingExample, DpoPair, FeedbackSignal, ModelAdapter) | 1 | Core/Core.cs |
| Fabric interfaces (IProjectIndexer, IDomainDatasetCache, IFeedbackRouter, IIncrementalTrainer, IDomainModelRegistry) | 1 | Fabric/Interfaces.cs |
| ProjectIndexer | 2 | Indexer/ProjectIndexer.cs |
| DomainDatasetCache | 2 | Cache/DomainDatasetCache.cs |
| FeedbackRouter | 3 | Feedback/FeedbackRouter.cs |
| IncrementalTrainer | 4 | Training/IncrementalTrainer.cs |
| DomainModelRegistry | 5 | Registry/DomainModelRegistry.cs |
| ContinuousLearningPipeline | 5 | Pipeline/ContinuousLearningPipeline.cs |

No component appears in two phases. No phase creates a component not in this table.

---

---

## Phase 1 — Scaffold: types + interfaces + project file
**Tests added:** +3 (877 → 880)
**Commit message:** `"phase 1: scaffold 14.ContinuousLearning — Core types, Fabric interfaces, .csproj"`

### Files to create

**`14.ContinuousLearning/ContinuousLearning.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\10.LoRA\LoRA.csproj" />
    <ProjectReference Include="..\11.DPO\DPO.csproj" />
    <ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
    <ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler.csproj" />
  </ItemGroup>
</Project>
```

**`14.ContinuousLearning/Core/Core.cs`**
```csharp
namespace ContinuousLearning;

// ─── Training example ────────────────────────────────────────────────────────
// One labelled sample stored in the domain dataset cache.
// weight: computed dynamically — see FeedbackRouter.ComputeMaxWeight.
public sealed record TrainingExample(
    string   Prompt,       // original user prompt
    string   Response,     // model's response at capture time
    string   Domain,       // canonical: CODE_Q / BUG / PLAN / ARCH / REFACTOR (D-04)
    double   Weight,       // importance weight, 1.0 = standard
    DateTimeOffset CapturedAt);

// ─── DPO pair ────────────────────────────────────────────────────────────────
// Chosen/rejected pair for DPO fine-tuning.
// Created by FeedbackRouter when user edits or rejects a response.
public sealed record DpoPair(
    string   Prompt,
    string   Chosen,       // preferred response (user's edit or a better answer)
    string   Rejected,     // response the user did not accept
    string   Domain,
    DateTimeOffset CapturedAt);

// ─── Feedback signal ─────────────────────────────────────────────────────────
// The three signals a user can send about any model response.
public enum FeedbackSignal { Accepted, Edited, Rejected }

// ─── Feedback event ──────────────────────────────────────────────────────────
// Wraps a feedback signal with the context needed to produce a training example.
public sealed record FeedbackEvent(
    string         Prompt,
    string         ModelResponse,     // what the model said
    string?        UserCorrection,    // non-null when Signal == Edited
    FeedbackSignal Signal,
    string         Domain);

// ─── Model adapter ───────────────────────────────────────────────────────────
// Represents one LoRA adapter file on disk (2 MB per adapter).
public sealed record ModelAdapter(
    string   Domain,           // which domain this adapter specialises for
    string   AdapterPath,      // relative path to .lora file
    int      TrainingExamples, // how many examples produced this adapter
    DateTimeOffset TrainedAt);
```

**`14.ContinuousLearning/Fabric/Interfaces.cs`**
```csharp
namespace ContinuousLearning;

// IProjectIndexer — scans a codebase and produces labelled training examples.
// Injects ISemanticCompiler from 29.SemanticCompiler — does NOT reimplement it.
public interface IProjectIndexer
{
    // Scan all .cs / .ts / .py files under rootPath.
    // Returns one TrainingExample per file pair (prompt=symbol, response=body).
    OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath);
}

// IDomainDatasetCache — persists training examples by domain.
// Backed by JSON via ICodeWriter — no SQLite dependency.
public interface IDomainDatasetCache
{
    // Add one example to the cache for its domain.
    OperationResult<bool> Add(TrainingExample example);
    // Retrieve all examples for one domain.
    OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain);
    // Total example count across all domains.
    OperationResult<int> TotalCount();
    // Persist cache to disk (JSON per domain file).
    Task<OperationResult<bool>> FlushAsync();
}

// IFeedbackRouter — converts user feedback events into training signals.
// Routes ACCEPTED → positive example, EDITED/REJECTED → DPO pair.
public interface IFeedbackRouter
{
    // Process a feedback event: add to cache or create DPO pair.
    OperationResult<bool> Route(FeedbackEvent feedback);
    // Retrieve all DPO pairs accumulated so far.
    OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs();
}

// IIncrementalTrainer — runs LoRA fine-tuning on accumulated examples.
// Calls LoraApplicator.Apply from 10.LoRA.
public interface IIncrementalTrainer
{
    // Run one LoRA training pass on examples for the given domain.
    // Returns the path to the saved adapter file.
    Task<OperationResult<string>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair>         dpoPairs);
}

// IDomainModelRegistry — loads and saves LoRA adapters per domain.
public interface IDomainModelRegistry
{
    // Register a newly-trained adapter.
    OperationResult<bool> Register(ModelAdapter adapter);
    // Get the adapter for a domain, or null if none trained yet.
    OperationResult<ModelAdapter?> GetAdapter(string domain);
    // List all registered adapters.
    OperationResult<IReadOnlyList<ModelAdapter>> ListAll();
}

// IContinuousLearningPipeline — fabric interface for 16.AutonomousLearning.
// 16.AutonomousLearning must inject this, not copy the implementation.
// Declared here (Fabric/Interfaces.cs) per Principle 3 — Fabric First.
public interface IContinuousLearningPipeline
{
    Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath);
}
```

### Add to Tests.csproj

```xml
<ProjectReference Include="..\14.ContinuousLearning\ContinuousLearning.csproj" />
```

### Tests — Phase 1 (+3, total 880)

```csharp
// File: Tests/ContinuousLearningTests.cs

// ContinuousLearning_TrainingExample_StoresDomainAndWeight
// var ex = new TrainingExample("fix null", "use ?.", "BUG", 1.5, DateTimeOffset.UtcNow);
// Assert.Equal("BUG", ex.Domain);
// Assert.Equal(1.5, ex.Weight);

// ContinuousLearning_DpoPair_StoresChosenAndRejected
// var pair = new DpoPair("prompt", "good", "bad", "CODE_Q", DateTimeOffset.UtcNow);
// Assert.Equal("good", pair.Chosen);
// Assert.Equal("bad", pair.Rejected);

// ContinuousLearning_FeedbackSignal_HasThreeValues
// Assert.Equal(3, Enum.GetValues<FeedbackSignal>().Length);
```

### Gate B — Phase 1

```bash
dotnet build 14.ContinuousLearning/ContinuousLearning.csproj  # zero errors
dotnet test --filter "ContinuousLearning_Training\|ContinuousLearning_Dpo\|ContinuousLearning_Feedback" --no-build
# Expected: 3 passed, 0 failed
grep "ISemanticCompiler" 14.ContinuousLearning/Fabric/Interfaces.cs
# Expected: present (IProjectIndexer declares ISemanticCompiler dependency)
```

---

## Phase 2 — ProjectIndexer + DomainDatasetCache
**Tests added:** +6 (880 → 886)
**Commit message:** `"phase 2: ProjectIndexer (ISemanticCompiler injected) + DomainDatasetCache (JSON-backed)"`

### ProjectIndexer

**`14.ContinuousLearning/Indexer/ProjectIndexer.cs`**
```csharp
namespace ContinuousLearning;

// ProjectIndexer: scans a codebase and produces labelled training examples.
// Uses ISemanticCompiler from 29.SemanticCompiler — does NOT reimplement
// StackDetector, normaliser, or predicate extractor. Fabric First rule.
public sealed class ProjectIndexer : BaseService, IProjectIndexer
{
    private readonly ISemanticCompiler _compiler; // injected from 29.SemanticCompiler
    private readonly ICodeReader       _reader;   // injected from 21.AgentInfrastructure

    public ProjectIndexer(ISemanticCompiler compiler, ICodeReader reader)
        : base(reader as ISessionStorage ?? throw new ArgumentException("reader must implement ISessionStorage"))
    {
        _compiler = compiler;
        _reader   = reader;
    }

    // Scan all .cs / .ts / .py files under rootPath.
    // For each file: compile through ISemanticCompiler → produce TrainingExample.
    // TrainingExample.Domain comes from ISemanticCompiler.Compile().Domain.
    // TrainingExample.Weight = 1.0 (default — FeedbackRouter adjusts later).
    public OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
    {
        // Implementation: list files → read each → compile → collect results
        // Returns Fail if rootPath does not exist or contains zero supported files
    }
}
```

### DomainDatasetCache

**`14.ContinuousLearning/Cache/DomainDatasetCache.cs`**
```csharp
namespace ContinuousLearning;

// DomainDatasetCache: in-memory + JSON-backed store for training examples.
// Backed by ICodeWriter (21.AgentInfrastructure) — no SQLite dependency.
// On construction: attempts to load existing JSON files from cacheDir.
// FlushAsync: writes one JSON file per domain to cacheDir.
public sealed class DomainDatasetCache : BaseService, IDomainDatasetCache
{
    // Storage: one list per canonical domain (CODE_Q / BUG / PLAN / ARCH / REFACTOR)
    private readonly Dictionary<string, List<TrainingExample>> _store = new();
    private readonly ICodeWriter _writer;  // injected
    private readonly string      _cacheDir;

    public OperationResult<bool> Add(TrainingExample example);
    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain);
    public OperationResult<int> TotalCount();
    public Task<OperationResult<bool>> FlushAsync();
}
```

### Tests — Phase 2 (+6, total 886)

```csharp
// ProjectIndexer_EmptyDirectory_ReturnsEmpty
// ProjectIndexer_CSharpFile_ProducesExampleWithCsharpDomain
// ProjectIndexer_ISemanticCompilerInjected_NotCopied
//   — Assert.NotNull(indexer.GetType().GetConstructors()[0]
//       .GetParameters().FirstOrDefault(p => p.ParameterType.Name == "ISemanticCompiler"))

// DomainDatasetCache_Add_IncrementsTotalCount
// DomainDatasetCache_GetByDomain_ReturnsOnlyMatchingDomain
// DomainDatasetCache_TotalCount_SumsAcrossDomains
```

### Gate B — Phase 2

```bash
dotnet test --filter "ProjectIndexer|DomainDatasetCache" --no-build
# Expected: 6 passed, 0 failed

# Critical: ISemanticCompiler injected, not copied
grep -rn "ISemanticCompiler" 14.ContinuousLearning/Indexer/ProjectIndexer.cs
# Expected: at least one line (constructor parameter declaration)

# Critical: no reimplementation of pipeline stages
grep -rn "StackDetector\|SemanticNormalizer\|FamilyCollapser\|LogicExtractor" \
  14.ContinuousLearning/ --include="*.cs"
# Expected: 0 hits — any hit = violation of Fabric First
```

---

## Phase 3 — FeedbackRouter
**Tests added:** +5 (886 → 891)
**Commit message:** `"phase 3: FeedbackRouter — ACCEPTED/EDITED/REJECTED → DPO pairs, dynamic max_weight"`

**`14.ContinuousLearning/Feedback/FeedbackRouter.cs`**
```csharp
namespace ContinuousLearning;

// FeedbackRouter: converts user feedback signals into training data.
//
// Three routing rules:
//   ACCEPTED  → add response as positive TrainingExample (weight = standard 1.0)
//   EDITED    → create DpoPair(chosen=UserCorrection, rejected=ModelResponse)
//               + add chosen as positive TrainingExample
//   REJECTED  → create DpoPair(chosen="[no preferred response]", rejected=ModelResponse)
//               — rejected signal without a correction still trains "avoid this"
//
// Weight formula (D-09 — dynamic, not a constant ×10):
//   maxWeight = totalExamples / (numDomains × rareExamples)
//   Called once per batch when computing weights for domain balancing.
public sealed class FeedbackRouter : BaseService, IFeedbackRouter
{
    private readonly IDomainDatasetCache _cache; // injected
    private readonly List<DpoPair>       _dpoPairs = new();

    // Route one feedback event.
    public OperationResult<bool> Route(FeedbackEvent feedback);

    // Retrieve accumulated DPO pairs (cleared after each IncrementalTrainer run).
    public OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs();

    // Compute the maximum training weight for a rare domain.
    // numDomains = 5 (canonical domains), rareExamples = count in that domain.
    // Example: totalExamples=1000, numDomains=5, rareExamples=50 → maxWeight=4.0
    public static double ComputeMaxWeight(int totalExamples, int numDomains, int rareExamples)
    {
        if (rareExamples <= 0) return 1.0; // guard: avoid division by zero
        return totalExamples / ((double)numDomains * rareExamples);
    }
}
```

### Tests — Phase 3 (+5, total 891)

```csharp
// FeedbackRouter_Accepted_AddsPositiveExample
// FeedbackRouter_Edited_CreatesDpoPair_WithUserCorrectionAsChosen
// FeedbackRouter_Rejected_CreatesDpoPair_WithEmptyChosen
// FeedbackRouter_ComputeMaxWeight_Formula_1000_5_50_Returns4
//   Assert.Equal(4.0, FeedbackRouter.ComputeMaxWeight(1000, 5, 50));
// FeedbackRouter_ComputeMaxWeight_ZeroRare_Returns1
//   Assert.Equal(1.0, FeedbackRouter.ComputeMaxWeight(100, 5, 0));
```

### Gate B — Phase 3

```bash
dotnet test --filter "FeedbackRouter" --no-build
# Expected: 5 passed, 0 failed

# D-09 formula present (not a constant):
grep -n "totalExamples.*numDomains\|numDomains.*rareExamples" \
  14.ContinuousLearning/Feedback/FeedbackRouter.cs
# Expected: the formula line — not "× 10" or "weight = 10"
```

---

## Phase 4 — IncrementalTrainer
**Tests added:** +4 (891 → 895)
**Commit message:** `"phase 4: IncrementalTrainer — LoRA fine-tuning via LoraApplicator + DpoTrainer"`

**`14.ContinuousLearning/Training/IncrementalTrainer.cs`**
```csharp
namespace ContinuousLearning;

// IncrementalTrainer: runs one LoRA fine-tuning pass on accumulated examples.
//
// Uses LoraApplicator.Apply from 10.LoRA and DpoTrainer from 11.DPO.
// Does NOT reimplement gradient logic — both are called through their
// existing public APIs.
//
// Training pass:
//   1. Apply LoRA layers to model: LoraApplicator.Apply(model, config)
//   2. For each TrainingExample: standard cross-entropy step
//   3. For each DpoPair: DpoTrainer.Step(batch, step)
//   4. Save adapter weights to adapterPath
//   5. Return adapter path wrapped in OperationResult<string>
public sealed class IncrementalTrainer : BaseService, IIncrementalTrainer
{
    // LoRA configuration — rank=8, alpha=16 matching existing LoRA tests
    private static readonly LoraConfig DefaultConfig = new()
    {
        Rank          = 8,
        Alpha         = 16f,
        Dropout       = 0.05f,
        TargetModules = ["q_proj", "v_proj"],
    };

    public Task<OperationResult<string>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair>         dpoPairs);
}
```

### Tests — Phase 4 (+4, total 895)

```csharp
// IncrementalTrainer_EmptyExamples_ReturnsFailure
//   — training with zero examples should return Fail, not throw

// IncrementalTrainer_DefaultConfig_RankIsEight
//   var config = IncrementalTrainer.DefaultConfig;
//   Assert.Equal(8, config.Rank);

// IncrementalTrainer_DefaultConfig_TargetModulesContainQProj
//   Assert.Contains("q_proj", IncrementalTrainer.DefaultConfig.TargetModules);

// IncrementalTrainer_UsesLoraApplicator_NotCustomGradients
//   — verify by reflection: no "backward\|gradient\|Backward\|Gradient" in source
//   grep -c "gradient\|Backward\|backward" 14.ContinuousLearning/Training/IncrementalTrainer.cs
//   — Expected: 0 hits (delegates to LoraApplicator and DpoTrainer)
```

### Gate B — Phase 4

```bash
dotnet test --filter "IncrementalTrainer" --no-build
# Expected: 4 passed, 0 failed

# Verify delegation — no reimplemented gradient logic
grep -c "gradient\|Backward\|backward" 14.ContinuousLearning/Training/IncrementalTrainer.cs
# Expected: 0

# LoraApplicator is referenced
grep -n "LoraApplicator\|DpoTrainer" 14.ContinuousLearning/Training/IncrementalTrainer.cs
# Expected: both present

# Verify DefaultConfig matches actual LoRA module names in 10.LoRA
grep -n "q_proj\|v_proj\|TargetModules\|Rank.*8\|Alpha.*16" 10.LoRA/*.cs
# Compare output with DefaultConfig values in IncrementalTrainer.cs
# If module names differ — update DefaultConfig to match, then re-run tests
```

---

## Phase 5 — DomainModelRegistry + ContinuousLearningPipeline + integration
**Tests added:** +4 (895 → 899)
**Commit message:** `"phase 5: DomainModelRegistry, ContinuousLearningPipeline — full round-trip test"`

### DomainModelRegistry

**`14.ContinuousLearning/Registry/DomainModelRegistry.cs`**
```csharp
namespace ContinuousLearning;

// DomainModelRegistry: stores and retrieves LoRA adapters per domain.
// Backed by ICodeWriter (JSON) — one file: adapters/registry.json.
// Adapter files themselves: adapters/{domain}.lora (written by IncrementalTrainer).
public sealed class DomainModelRegistry : BaseService, IDomainModelRegistry
{
    private readonly Dictionary<string, ModelAdapter> _registry = new();
    private readonly ICodeWriter _writer;

    public OperationResult<bool>                  Register(ModelAdapter adapter);
    public OperationResult<ModelAdapter?>         GetAdapter(string domain);
    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll();
}
```

### ContinuousLearningPipeline

**`14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs`**
```csharp
namespace ContinuousLearning;

// ContinuousLearningPipeline: orchestrates one complete learning cycle.
//
// Cycle steps (called by 16.AutonomousLearning at night, or manually):
//   1. ProjectIndexer.Index(rootPath)  → new TrainingExamples
//   2. DomainDatasetCache.Add(...)     → persist to domain buckets
//   3. FeedbackRouter.GetDpoPairs()    → accumulated corrections since last run
//   4. IncrementalTrainer.TrainAsync() → one LoRA pass per domain with new data
//   5. DomainModelRegistry.Register()  → save new adapters
//   6. DomainDatasetCache.FlushAsync() → write JSON to disk
//
// This class is the ISemanticCompiler of 14 — it is what 16.AutonomousLearning
// will inject via IContinuousLearningPipeline (declared below).
public sealed class ContinuousLearningPipeline : BaseService
{
    public Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath);
}

// NOTE: IContinuousLearningPipeline is declared in Fabric/Interfaces.cs (Principle 3 — Fabric First).
// Do NOT redeclare it here. ContinuousLearningPipeline implements it via the reference below.
```

### Tests — Phase 5 (+4, total 899)

```csharp
// DomainModelRegistry_Register_StoresAdapter
//   var adapter = new ModelAdapter("BUG", "adapters/bug.lora", 120, DateTimeOffset.UtcNow);
//   var reg = new DomainModelRegistry(...);
//   reg.Register(adapter);
//   Assert.Equal("BUG", reg.GetAdapter("BUG").Value!.Domain);

// DomainModelRegistry_GetAdapter_UnknownDomain_ReturnsNull
//   var result = reg.GetAdapter("UNKNOWN_DOMAIN");
//   Assert.True(result.IsSuccess && result.Value is null);

// DomainModelRegistry_ListAll_ReturnsAllRegistered
//   // register 3 adapters, assert count == 3

// ContinuousLearning_IContinuousLearningPipeline_InterfaceExists
//   // Verify IContinuousLearningPipeline is declared in Fabric/Interfaces.cs
//   var type = typeof(IContinuousLearningPipeline);
//   Assert.NotNull(type);
```

### Gate B — Phase 5

```bash
dotnet test --filter "ContinuousLearning" --no-build
# Expected: 899 passed, 0 failed

# All five principles verified:
# P1 — OperationResult everywhere
grep -rn "void \|bool \|string \|int \|List<\|IReadOnly" \
  14.ContinuousLearning --include="*.cs" | grep "public " | grep -v "OperationResult\|Task<\|static\|record\|//\|interface\|enum\|class\|abstract"
# Expected: 0 hits — every public non-static method returns OperationResult or Task<OperationResult>

# P4 — ISemanticCompiler injected (not copied)
grep -rn "ISemanticCompiler" 14.ContinuousLearning/
# Expected: at least one line

# P5 — max_weight formula (not a constant)
grep -n "ComputeMaxWeight\|totalExamples.*numDomains" 14.ContinuousLearning/Feedback/FeedbackRouter.cs
# Expected: formula present

# IContinuousLearningPipeline in Fabric/Interfaces.cs (not in Pipeline/ — Fabric First)
grep -n "IContinuousLearningPipeline" 14.ContinuousLearning/Fabric/Interfaces.cs
# Expected: interface declaration present (NOT in Pipeline/ContinuousLearningPipeline.cs)
```

---

## FC self-containment checks — run before delivery

Run all 8 against this file. Expected result: 0 hits each.

```bash
# Check 1 — no cross-references
grep -n "see master plan\|see DECISIONS\|per CLAUDE\|see AGENTS\|load skill\|per plan" \
  CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
# Expected: 0 hits (all rules inlined above)

# Check 2 — test count consistency
grep -o "total [0-9][0-9][0-9]" CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md | sort -u
# Expected: 880, 886, 891, 895, 899 — each appearing exactly once as the running total

# Check 3 — all components have a creating phase
for component in ProjectIndexer DomainDatasetCache FeedbackRouter IncrementalTrainer DomainModelRegistry ContinuousLearningPipeline; do
    grep -c "$component" CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
done
# Expected: each returns > 0

# Check 4 — no undefined placeholders
grep -n "<[A-Z_][A-Z_]*>" CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md | grep -v "<!--"
# Expected: 0 hits

# Check 5 — Gate B commands are literal bash, not English
grep -n "verify that \|check whether \|ensure the \|confirm that " \
  CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
# Expected: 0 hits

# Check 6 — no stale test count (877 must only appear in pre-condition + baseline)
grep -n "877" CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
# Expected: pre-condition gate and Round Contract only — not as phase totals

# Check 7 — commit count matches phase count
grep -c "^## Phase " CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
grep -c "Commit message:" CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
# Expected: both return 5

# Check 8 — phase totals are arithmetic (877+3=880, +6=886, +5=891, +4=895, +4=899)
echo "877 + 3 = 880"; echo "880 + 6 = 886"; echo "886 + 5 = 891"
echo "891 + 4 = 895"; echo "895 + 4 = 899"
# Verify: each total equals previous + tests added in that phase
```

---

## Phase summary

| Phase | Component(s) | Files | Tests added | Running total | Key Gate B |
|---|---|---|---|---|---|
| 1 | Core types + Fabric interfaces | Core.cs, Interfaces.cs, .csproj | +3 | 880 | build clean |
| 2 | ProjectIndexer + DomainDatasetCache | 2 .cs files | +6 | 886 | ISemanticCompiler in constructor |
| 3 | FeedbackRouter | 1 .cs file | +5 | 891 | ComputeMaxWeight(1000,5,50)=4.0 |
| 4 | IncrementalTrainer | 1 .cs file | +4 | 895 | LoraApplicator referenced, 0 gradient reimplementations |
| 5 | DomainModelRegistry + Pipeline | 2 .cs files | +4 | 899 | IContinuousLearningPipeline declared |

**After this plan:** 16.AutonomousLearning can inject `IContinuousLearningPipeline`
and `ISemanticCompiler` without implementing either. The first real gradient
test of the E28 project-adaptation hypothesis becomes runnable in Phase 4.
