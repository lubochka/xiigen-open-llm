# AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0.md
## 16.AutonomousLearning — .NET Dataset Collection + Training Cycle

**Subproject:** 16.AutonomousLearning
**Plan version:** v1.0
**Experiment basis:** E41 (IdleDetector +100%), E40V2 (TrustScorer +50%), E43 (ReplayBuffer +23%)
**Depends on:** 14.ContinuousLearning (Done), 29.SemanticCompiler (Done), 21.AgentInfrastructure (Done)
**Target baseline:** 916 passed, 0 failed
**Target after:** 916 + 22 = 938 passed, 0 failed
**Gate B:** `dotnet test --filter "AutonomousLearning_" | grep -E "passed|failed"`

---

## Hard gates (pre-conditions from EXPERIMENT-RESULTS-MAP R1 analysis)

Before any code in this plan runs:

| Gate | Rule | Source |
|------|------|--------|
| E20 rule | StackDetector must run BEFORE any normalization in dataset pipeline | E20 SCALE_UP — rule confirmed |
| D-09 formula | max_weight = total_examples / (num_classes × rare_examples) — never × 10.0 constant | E44 FAILED on x10 constant |
| StratifiedSplit | All train/val splits use StratifiedSplit, never seed=42 | E40 FAILED on seed-42 collapse |
| No ML frameworks | All training calls existing 10.LoRA and 11.DPO — no new ML dependencies | D-11 locked |
| Real LoRA required | Before/after accuracy measurement uses real LoRA (not bigram proxy) | E28-E31 FAILED — bigram cannot show gradient signal |

---

## Pre-condition check (run before Phase 1)

```
PC-0: dotnet build 16.AutonomousLearning\AutonomousLearning.csproj → 0 errors
PC-1: grep -r "IncrementalTrainer" 14.ContinuousLearning → exists (Phase 5 of CL plan)
PC-2: grep -r "ISemanticCompiler29" 29.SemanticCompiler → exists (needed for labeling)
PC-3: grep -r "LoraApplicator" 10.LoRA → exists (training infrastructure)
PC-4: dotnet test Tests/ | grep "916 passed" → confirm baseline
```

---

## Architecture: 8-step autonomous learning cycle

The cycle runs overnight when the user's machine is idle:

```
1. E41 IdleDetector       → SAFE_TO_TRAIN? (CPU<20% + input idle 10min + battery>50% + disk>2GB)
2. DataSourceFinder       → collect .NET sources from Tier 1/2/3
3. E40V2 TrustScorer      → score each source (HIGH/MEDIUM/LOW)
4. DatasetPreprocessor    → StackDetector FIRST (E20 gate), then normalization
5. E43 ReplayBuffer       → 70% new + 30% replay buffer (prevents catastrophic forgetting)
6. DatasetWeighter        → max_weight = total/(classes×rare) [D-09, not ×10]
7. IncrementalTrainer     → LoRA rank=8, alpha=16 (from 14.ContinuousLearning)
8. DomainModelRegistry    → save LoRA adapter, update domain accuracy
```

---

## Dataset sources (from DATASET-COLLECTION-PLAN-DOTNET-v1_0.md R12 analysis)

### Tier 1 — Highest trust (0.7–1.0)
- `dotnet/runtime`, `dotnet/aspnetcore`, `dotnet/efcore`, `dotnet/roslyn` (GitHub)
- Microsoft Learn documentation (docs.microsoft.com/dotnet)
- Stack Overflow accepted answers on C# tags

### Tier 2 — Medium trust (0.3–0.7)
- GitHub repos with ≥ 500 stars, C# language, CI passing
- Community .NET blogs with author verification

### Tier 3 — Lower trust (0.1–0.3)
- GitHub repos with 100–500 stars, C# language
- .NET developer blog (devblogs.microsoft.com)

**Minimum for first training run:** 1,400 total examples, 500 in CODE_Q domain.

---

## Phases

### Phase 1 — Project setup and IdleDetector (5 tests)

**What:** Create 16.AutonomousLearning project. Implement IdleDetector (E41 algorithm).

**Files:**
- `16.AutonomousLearning/AutonomousLearning.csproj`
- `16.AutonomousLearning/Core/Interfaces.cs` — IAuLearningStorage, ISystemMonitor, IDataSourceFinder
- `16.AutonomousLearning/IdleDetector/IdleDetector.cs` — E41 four-condition algorithm
- `Tests/AutonomousLearningTests.cs` (add to existing Tests project)

**IdleDetector algorithm (E41 validated):**
```
SAFE_TO_TRAIN iff:
  cpu_usage < 20%
  AND last_input > 10 minutes ago
  AND (is_charging OR battery_pct >= 50%)
  AND disk_free_gb >= 2.0
```

**Tests (5):**
```
AutonomousLearning_IdleDetector_AllConditionsMet_ReturnsSafeToTrain
AutonomousLearning_IdleDetector_HighCpu_ReturnsWait
AutonomousLearning_IdleDetector_RecentInput_ReturnsWait
AutonomousLearning_IdleDetector_LowBattery_ReturnsWait
AutonomousLearning_IdleDetector_LowDisk_ReturnsWait
```

**Gate B:** `dotnet test --filter "AutonomousLearning_IdleDetector" | grep "5 passed"`

---

### Phase 2 — TrustScorer and DataSourceFinder (5 tests)

**What:** Implement TrustScorer (E40V2 algorithm) and DataSourceFinder that lists .NET sources.

**Files:**
- `16.AutonomousLearning/TrustScorer/TrustScorer.cs` — K-NN on 7 source features
- `16.AutonomousLearning/DataSourceFinder/DataSourceFinder.cs` — discovers configured .NET repos
- `16.AutonomousLearning/DataSourceFinder/SourceEntry.cs` — record: Url, Tier, TrustLevel

**TrustScorer features (E40V2 validated):**
```
IsInternal        — internal organization repository
HasCIPassing      — green CI/CD pipeline
HasTests          — contains test files (*Tests.cs, *Test.cs)
UpdateDaysAgo     — days since last commit (0=today)
PopularityScore   — 0-3 (0=unknown, 3=1000+ stars)
IsOfficialDocs    — official Microsoft documentation
DomainMatch       — matches .NET/C# stack
```

**Rule: StratifiedSplit required (E40 failure lesson).** Training split uses stratified sampling across HIGH/MEDIUM/LOW trust levels.

**Tests (5):**
```
AutonomousLearning_TrustScorer_InternalRepoWithCI_ScoresHigh
AutonomousLearning_TrustScorer_UnknownBlog_ScoresLow
AutonomousLearning_TrustScorer_OfficialDocs_ScoresHigh
AutonomousLearning_DataSourceFinder_DefaultSources_ReturnsNonEmpty
AutonomousLearning_DataSourceFinder_EmptyConfig_ReturnsFailWithCode
```

**Gate B:** `dotnet test --filter "AutonomousLearning_TrustScorer|AutonomousLearning_DataSourceFinder" | grep "5 passed"`

---

### Phase 3 — DatasetPreprocessor and collection pipeline (5 tests)

**What:** Implement DatasetPreprocessor that applies E20 gate (StackDetector first), then normalization, then stores labeled examples via DomainDatasetCache from 14.ContinuousLearning.

**Files:**
- `16.AutonomousLearning/Preprocessing/DatasetPreprocessor.cs`
- `16.AutonomousLearning/Preprocessing/DatasetWeighter.cs` — D-09 formula

**Critical E20 gate (non-negotiable):**
```csharp
// Step 1: StackDetector FIRST — before ANY normalization
var stackResult = _compiler.Compile(new PipelineInput(rawText, "csharp"));
// Step 2: THEN normalize
// NEVER reverse this order — E20 showed language markers are destroyed by normalization
```

**D-09 formula (not ×10 constant):**
```csharp
double ComputeMaxWeight(int total, int numClasses, int rareExamples)
    => (double)total / (numClasses * rareExamples);
// Gate B test: ComputeMaxWeight(1000, 5, 50) == 4.0
```

**Tests (5):**
```
AutonomousLearning_DatasetPreprocessor_StackDetectorRunsBeforeNormalization
AutonomousLearning_DatasetWeighter_Formula_Produces4Point0ForStandardInput
AutonomousLearning_DatasetWeighter_ZeroRareExamples_ReturnsFail
AutonomousLearning_DatasetPreprocessor_LowTrustSource_Filtered
AutonomousLearning_DatasetPreprocessor_EmptyText_ReturnsFail
```

**Gate B:** `dotnet test --filter "AutonomousLearning_Dataset" | grep "5 passed"`

---

### Phase 4 — ReplayBuffer and training integration (4 tests)

**What:** Implement ReplayBuffer (E43 algorithm: 70/30 mix). Wire to IncrementalTrainer from 14.ContinuousLearning.

**Files:**
- `16.AutonomousLearning/Training/ReplayBuffer.cs` — stores past examples, samples 30%
- `16.AutonomousLearning/Training/AutonomousTrainer.cs` — wraps IncrementalTrainer + replay

**ReplayBuffer algorithm (E43 validated):**
```
Training batch = 70% new examples + 30% random sample from ReplayBuffer
Prevents catastrophic forgetting when new domain data arrives
Retention target: ≥ 70% accuracy on old domain after new domain training
```

**Tests (4):**
```
AutonomousLearning_ReplayBuffer_MixRatio_Is70New30Old
AutonomousLearning_ReplayBuffer_EmptyBuffer_Returns100PercentNew
AutonomousLearning_AutonomousTrainer_RunsCycleAndSavesAdapter
AutonomousLearning_AutonomousTrainer_MinimumExamplesNotMet_ReturnsFail
```

**Gate B:** `dotnet test --filter "AutonomousLearning_Replay|AutonomousLearning_AutonomousTrainer" | grep "4 passed"`

---

### Phase 5 — Full cycle integration and before/after verification (3 tests)

**What:** Wire all components into AutonomousLearningPipeline. Add before/after accuracy measurement (E28 hypothesis test with real LoRA — bigram proxy is NOT sufficient).

**Files:**
- `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs` — full 8-step cycle

**Before/after measurement protocol:**
```
1. Load test set: 50 project-specific .NET examples with known domain labels
2. Measure baseline accuracy: DomainModelRegistry current adapter → score
3. Run one autonomous training cycle (collect → preprocess → weight → train → save)
4. Measure enhanced accuracy: same test set → score
5. Assert: enhanced > baseline (Gate B requires measurable delta)
```

**Tests (3):**
```
AutonomousLearning_Pipeline_IdleCheck_BlocksRunWhenActive
AutonomousLearning_Pipeline_FullCycle_CompletesWithoutError
AutonomousLearning_Pipeline_BeforeAfterMeasurement_EnhancedExceedsBaseline
```

**Gate B (integration test):**
```
dotnet test --filter "AutonomousLearning_Pipeline_BeforeAfterMeasurement_EnhancedExceedsBaseline"
→ 1 passed (confirmed enhanced accuracy > baseline accuracy)
```

---

## Test count summary

| Phase | Tests | Cumulative |
|-------|-------|-----------|
| Phase 1: IdleDetector | 5 | 921 |
| Phase 2: TrustScorer + DataSourceFinder | 5 | 926 |
| Phase 3: DatasetPreprocessor | 5 | 931 |
| Phase 4: ReplayBuffer + AutonomousTrainer | 4 | 935 |
| Phase 5: Pipeline + before/after | 3 | 938 |

**Total: 22 tests. Baseline: 916. Target: 938.**

---

## FC plan review checklist

- **FC-1** Count accuracy: 22 tests across 5 phases ✓
- **FC-2** File naming: `AutonomousLearning.csproj` (no numeric prefix) ✓
- **FC-3** No test duplication: all 22 test names unique ✓
- **FC-4** Gate B per phase: each phase has exact bash assertion ✓
- **FC-5** No phantom components: all interfaces reference existing 21.AgentInfrastructure types ✓
- **FC-6** Stale numbers: baseline=916 per 2026-05-01 run ✓
- **FC-7** Phase order: Phase 1 builds fabric, Phase 5 integrates — correct ✓
- **FC-8** Self-contained: each phase can be verified independently ✓
- **FC-9** Gate B format: `dotnet test --filter ... | grep "N passed"` ✓
- **FC-10** Type contracts: ISemanticCompiler29 from 29 only, never reimplemented ✓
- **FC-11** Overview-detail consistency: 22 tests in summary matches phase totals ✓
- **FC-12** Principles: Rule 1 (OperationResult), Rule 2 (Fabric First), Rule 4 (zero failures) ✓

---

## csproj — reference map

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- IncrementalTrainer, DomainModelRegistry, DomainDatasetCache, FeedbackRouter -->
    <ProjectReference Include="..\14.ContinuousLearning\ContinuousLearning.csproj" />
    <!-- ISemanticCompiler29, PipelineInput, Domain enum — E20 StackDetector gate -->
    <ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler.csproj" />
    <!-- BaseService, ISessionStorage, OperationResult<T> -->
    <ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
  </ItemGroup>
</Project>
```

---

## Commit sequence

| Phase | Commit message |
|-------|---------------|
| 1 | `phase 1: AutonomousLearning project, IdleDetector (E41), 5 tests` |
| 2 | `phase 2: TrustScorer (E40V2), DataSourceFinder, 5 tests` |
| 3 | `phase 3: DatasetPreprocessor (E20 gate), DatasetWeighter (D-09 formula), 5 tests` |
| 4 | `phase 4: ReplayBuffer (E43 70/30), AutonomousTrainer, 4 tests` |
| 5 | `phase 5: AutonomousLearningPipeline, before/after measurement, 3 tests` |

---

*Plan v1.0 — 16.AutonomousLearning · not XIIGen-related*
*Experiment evidence: E41 PASSED +100%, E40V2 PASSED +50%, E43 PASSED +23%*
*Hard gates: E20 (StackDetector first), D-09 (max_weight formula), E44 failure lesson*
