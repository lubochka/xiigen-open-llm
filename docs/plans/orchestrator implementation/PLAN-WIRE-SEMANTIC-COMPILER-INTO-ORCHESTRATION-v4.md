# Plan — Full Wiring of All Proven Experiments into the Orchestration Path
# Version: 4.0 — 2026-05-06
# Supersedes: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v3.md
# Session type: ARCHITECT
# Test floor entering this plan: 1224

---

## What v3 missed — and what v4 adds

Version 3 covered all 27 experiments being wired into the execution path but did not
address the underlying source duplication problem. Before any wiring can work cleanly,
the duplicate implementations must be unified:

- `RouteClassifier` exists in three separate namespaces (project 12, 22, 29) with
  three diverged implementations and different method signatures
- `DomainLabel` enum exists three times with three different value name sets:
    project 12: `DomainLabel` — values in Core.cs
    project 22: `DomainLabel` — values `CodeQuestion, Bug, Plan, Architecture, Refactor`
    project 29: `Domain` — values `CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN`
- Thirteen pipeline classes duplicated between projects 12 and 29
- E51B has an inline `Compile` function (`NormalizeRoots`, `PredicateNamesForRoot`,
  `BuildVector`) that exists nowhere in production — the 60-slot vector contract
  is only in the experiment file
- Only one experiment (E52) imports from a production project. All others inline
  their own logic or call nothing at all

Version 4 adds Phase 0 — source consolidation — as the first phase. All subsequent
phases from v3 are renumbered and now execute against clean, unified source.

---

## Phase 0 — Consolidate duplicate source code into one canonical location

This phase makes zero behaviour changes. It only redirects callers to one canonical
implementation. Gate B after each step verifies no tests break.

### Step 0A — Canonise the domain enum

Project 29's `Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }` is the
canonical form. It matches the locked D-04 decision names exactly.

Actions:
1. Delete `enum DomainLabel` from `12.SemanticCompiler/Core/Core.cs`.
2. Delete `enum DomainLabel { CodeQuestion, Bug, Plan, Architecture, Refactor }`
   from `22.AgentAlgorithms/Routing/RouteClassifier.cs`.
3. In both projects, add a project reference to project 29 and replace all enum
   usages with `SemanticCompiler29.Domain.*` values.
   Mapping: `CodeQuestion` → `Domain.CODE_Q`, `Bug` → `Domain.BUG`,
   `Architecture` → `Domain.ARCH`, `Plan` → `Domain.PLAN`,
   `Refactor` → `Domain.REFACTOR`.
4. Any type alias or compatibility shim in project 12 or 22 that re-exposes
   the old names is removed after all callers are updated.

Gate B — Step 0A:
```bash
dotnet build LlmFromScratch.sln 2>&1 | grep "error" | head -10
```
Expected: zero errors.

---

### Step 0B — Canonise RouteClassifier to project 29

Project 29's `RouteClassifier : BaseService, IRouteClassifier29` is the canonical
implementation. It has proper dependency injection and the correct signatures.

Actions:
1. Delete `RouteClassifier` from `12.SemanticCompiler/Routing/RouteClassifier.cs`.
   Replace with a thin type alias pointing to `SemanticCompiler29.RouteClassifier`.
2. Delete `RouteClassifier` from `22.AgentAlgorithms/Routing/RouteClassifier.cs`.
   Replace with a thin type alias pointing to `SemanticCompiler29.RouteClassifier`.
3. All callers in projects 12 and 22 that previously used their local copy now
   resolve to the project 29 version through the alias.
4. The `IRouteClassifier` interface in project 22 is replaced by `IRouteClassifier29`
   from project 29. Any callers of `IRouteClassifier` are updated.

Gate B — Step 0B:
```bash
dotnet build LlmFromScratch.sln 2>&1 | grep "error" | head -10
```
Expected: zero errors.

---

### Step 0C — Make project 12 delegate to project 29 for all thirteen duplicated classes

The thirteen classes duplicated between projects 12 and 29 are:
`AstTokenizer`, `FamilyCollapser`, `LogicExtractor`, `SemanticNormalizer`,
`TemplateBeautifier`, `Decomposer`, `DependencyGraph`, `StopSignalDetector`,
`StackDetector`, `PatternClassifier`, `ContextualDictionary`, `PatternReorderer`,
`SemanticTokenizerPipeline`.

Actions for each class:
1. Keep the project 12 class file in place — do not delete it.
   Existing callers of project 12 must keep working without changes.
2. Replace the class body with a thin wrapper that delegates to the project 29
   equivalent. Example for `LogicExtractor`:
   ```csharp
   // Delegates to SemanticCompiler29.LogicExtractor (canonical implementation).
   public static class LogicExtractor
   {
       private static readonly SemanticCompiler29.LogicExtractor _impl = new(...);
       public static IReadOnlyList<int> Extract(string source) => _impl.Extract(source);
   }
   ```
3. Project 12 adds a project reference to project 29 if not already present.
4. The `SemanticCompilerFacade` in project 12 now instantiates project 29 classes
   internally. All its output types remain unchanged — callers are not affected.

Gate B — Step 0C:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```
Expected: 1224 passing, 0 failing. No behaviour change — delegation is transparent.

---

### Step 0D — Move E51B's inline Compile logic into TrainingVectorBuilder

E51B contains three private methods that exist nowhere in production:
`NormalizeRoots`, `PredicateNamesForRoot`, `BuildVector`. Together they implement
the 60-slot feature vector contract specified in E51B's `FeatureVectorContract`
constant.

Actions:
1. Create `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs` (this file will
   be created fully in Phase 1 but the algorithm must come from here).
2. Move `NormalizeRoots`, `PredicateNamesForRoot`, and `BuildVector` verbatim
   from E51B into `TrainingVectorBuilder` as internal static methods.
3. Add a public entry point:
   `public static float[] Build(string prompt)` — calls the pipeline, then builds
   the 60-slot vector using these methods.
4. In E51B, replace the three private methods with calls to
   `TrainingVectorBuilder.Build` and the internal helpers via `internal` visibility.
5. E51B's behaviour is now identical but the production code is authoritative.

Gate B — Step 0D:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```
Expected: 1224 passing, 0 failing.

---

### Step 0E — Remove inline production class copies from experiments

Two experiment files contain their own production class implementations:

`E21PromptSplitter.cs` — contains an inline `PromptSplitter` class that
reimplements the `Decomposer` algorithm. Replace with:
```csharp
// Use canonical Decomposer from project 29 via project 12 wrapper.
var splitter = new SemanticCompiler.Decomposer();
var result = splitter.Split(prompt);
```

`E22DatasetRouter.cs` — contains an inline `DatasetRouter` class. The production
equivalent is `AstRelevanceRanker` in project 22 (BM25-based retrieval). Replace
with a call to `AstRelevanceRanker.Search` for the retrieval step.

After these changes: no experiment file contains a production class implementation.
All experiments call production classes through project references.

Gate B — Step 0E:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```
Expected: 1224 passing, 0 failing.

---

### Step 0F — Verify single source of truth

After Steps 0A–0E, grep the entire solution to confirm:

```bash
grep -rn "enum DomainLabel" . --include="*.cs" | grep -v obj
```
Expected: zero results outside project 29. `Domain` enum in project 29 only.

```bash
grep -rn "class RouteClassifier" . --include="*.cs" | grep -v obj
```
Expected: one result — project 29 only. Projects 12 and 22 have aliases/wrappers.

```bash
grep -rn "NormalizeRoots\|PredicateNamesForRoot\|BuildVector" . --include="*.cs" | grep -v obj | grep -v Experiment
```
Expected: all three appear in `TrainingVectorBuilder.cs` only.

Gate B — Phase 0 complete:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```
Expected: 1224 passing, 0 failing. Floor unchanged — consolidation is zero-behaviour-change.

---

## The complete picture: three paths, all experiments

```
PATH A — Runtime prompt path (what the developer types)

raw prompt
  → SemanticCompilerPipeline      (E01,E15,E16,E17,E18,E20 — 6 stages)
  → N-gram enrichment             (E03 — bigram transition probabilities)
  → Decomposer.Split              (E21 — multi-task boundary detection)
  → For each segment:
      → DomainClassifierUnit      (E18 — 95% domain accuracy, ILogicalUnit)
      → ComplexityRouter          (E19 — SINGLE_STEP vs MULTI_STEP)
      → MultiSubPhaseRouterUnit   (existing, scoring augmented by E04 BM25)
          → AstRelevanceRanker    (E04 BM25 +100%, E34V2 fusion +8%)
          → KnnSkillFilter        (E09 structural KNN +45%)
          → ContextWindowManager  (E38V2 skill retrieval +100%)
      → EpisodeDecomposer         (E32 — episode-level task decomposition)
      → DependencyGraph           (E35 — dependency ordering +30%)
  → Execute flows with domain adapter
  → QualityOrchestrator           (E39 — verify result quality +20%)
  → TemplateBeautifier            (E23 — result translation +833%)
  → human result


PATH B — Training path (what the capability trainer learns from)

knowledge edge detected
  → SemanticCompilerPipeline.Compile — same as Path A
  → TrainingVectorBuilder.Build → 60-slot feature vector (E51B symmetry)
  → ReplayBuffer.Sample           (E43 — 70/30 mix prevents forgetting)
  → IdleDetector gate             (E41 — only train when system is idle)
  → DpoTrainer.StepVector         (feature-vector DPO, not raw string)
  → TrustScorer.Score             (E40V2 — promote unit to Trusted)
  → checkpoint saved


PATH C — Result translation (compressed predicates → human language)

domain adapter output predicates
  → TemplateBeautifier.FindBest   (E23 — template lookup +833%)
  → CSharpBeautifier fallback     (E05 syntax filter informs templates)
  → RoleBasedTaskDecomposer       (E08 SyntaxRoles +56% for complex results)
  → KvCache                       (E11 — cache repeated predicate lookups)
  → human guidance string
```

---

## What exists and what must be built

### Already exists — no new code

- `29.SemanticCompilerPipeline` — full 6-stage pipeline (project 29)
- `12.SemanticCompiler.Planning.Decomposer` — E21 boundary detection
- `12.SemanticCompiler.Beautification.TemplateBeautifier` — E23
- `12.SemanticCompiler.Beautification.CSharpBeautifier` — E05 fallback
- `22.AgentAlgorithms.Context.AstRelevanceRanker` — E04 BM25
- `22.AgentAlgorithms.Context.KnnSkillFilter` — E09 KNN
- `22.AgentAlgorithms.Context.ContextWindowManager` — E38V2 skill retrieval
- `22.AgentAlgorithms.Tasks.RoleBasedTaskDecomposer` — E08 SyntaxRoles
- `22.AgentAlgorithms.MultiAgent.DependencyGraph` — E35
- `15.OrchestratorModel.EpisodeDecomposer` — E32
- `15.OrchestratorModel.QualityOrchestrator` — E39
- `16.AutonomousLearning.Training.ReplayBuffer` — E43
- `16.AutonomousLearning.TrustScorer.TrustScorer` — E40V2
- `16.AutonomousLearning.IdleDetector.IdleDetector` — E41
- `09.Inference.KvCache` — E11

### Must be built

**New project: `23.SemanticCompilerAdapter`** (from v2, expanded)
Wraps all proven components as `ILogicalUnit` adapters. Project 21 depends on this.

**New project: `17.DynamicDomains`** (new in v3)
Implements E49 `OrchestratorExpander` and E50 `DomainGraphClosure`. These require
building from the experiment files — the production classes do not exist yet.

**New class: `22.AgentAlgorithms.Context.HybridRanker`** (E34V2)
Fuses BM25 and KNN scores: `alpha × BM25 + (1-alpha) × KNN`. Auto-tunes alpha
on training examples. Belongs in project 22 alongside `AstRelevanceRanker`.

**Changes to project 21:**
- `PromptRoutingInputCompilerUnit` — call pipeline, segment, use BM25 scoring
- `MultiSubPhaseRouterUnit` — augment with BM25 retrieval via `AstRelevanceRanker`
- `DefaultLogicalCapabilityTrainer` — feature vectors, replay buffer, idle gate, trust scorer
- `UniversalNineNodeDevelopmentFlow` node 06 — result translator unit

---

## Baseline gate — run before touching anything

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: 1224 passing, 0 failing.

---

## Phase 1 — Create project 23.SemanticCompilerAdapter

New project: `23.SemanticCompilerAdapter/SemanticCompilerAdapter.csproj`

References:
```xml
<ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler29.csproj" />
<ProjectReference Include="..\12.SemanticCompiler\SemanticCompiler.csproj" />
<ProjectReference Include="..\22.AgentAlgorithms\AgentAlgorithms.csproj" />
<ProjectReference Include="..\15.OrchestratorModel\OrchestratorModel.csproj" />
<ProjectReference Include="..\16.AutonomousLearning\AutonomousLearning.csproj" />
<ProjectReference Include="..\09.Inference\Inference.csproj" />
<ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
```

### File: SemanticCompilerAdapterUnit.cs
```
ILogicalUnit<string, PipelineOutput>
Input:  raw prompt
Output: PipelineOutput (domain, predicates[], stack, compression_ratio)
Calls:  SemanticCompilerPipeline.Compile
```

### File: PromptSegmenterUnit.cs
```
ILogicalUnit<string, PromptSegment[]>
Input:  prompt
Output: one segment if single task, multiple if boundary detected (E21)
Calls:  Decomposer.Split
```

### File: DomainClassifierUnit.cs
```
ILogicalUnit<string, DomainClassification>
Input:  prompt
Output: DomainClassification (label, confidence, evidence[])
Calls:  RouteClassifier.ClassifyByKeywords (E18, 95% accuracy)
NOTE:   This is the ILogicalUnit wrapper that makes domain classification teachable.
        The scoring logic (RouteClassifier) is deterministic now but the unit
        can be replaced by a learned unit when training data is sufficient.
```

### File: ComplexityClassifierUnit.cs
```
ILogicalUnit<string, ComplexityClass>
Input:  prompt
Output: SINGLE_STEP or MULTI_STEP (E19 +18%)
Calls:  StopSignalDetector.Detect from project 12
NOTE:   SINGLE_STEP prompts route to one sub-phase only.
        MULTI_STEP prompts use the full multi-branch routing.
        This prevents over-routing on simple one-task prompts.
```

### File: RelevanceRankerUnit.cs
```
ILogicalUnit<RelevanceRankerRequest, RankedCandidate[]>
Input:  query text + candidate sub-phase list
Output: candidates ranked by BM25+KNN fusion score (E04+E34V2)
Calls:  AstRelevanceRanker (BM25) + KnnSkillFilter (KNN)
        HybridRanker.Rank (alpha × BM25 + (1-alpha) × KNN)
NOTE:   This replaces the hardcoded predicate overlap scoring in
        MultiSubPhaseRouterUnit with a retrieval-based approach.
        The predicate overlap score becomes the fallback when the
        retrieval index has fewer than 5 examples.
```

### File: EpisodeDecomposerUnit.cs
```
ILogicalUnit<string, SubTask[]>
Input:  task description string
Output: ordered sub-task list (E32 +11%)
Calls:  EpisodeDecomposer.Decompose from project 15
```

### File: DependencyResolverUnit.cs
```
ILogicalUnit<SubTask[], SubTask[][]>
Input:  flat sub-task list
Output: sub-tasks ordered in dependency layers — parallel groups as inner arrays (E35 +30%)
Calls:  DependencyGraph from project 22
```

### File: QualityCheckUnit.cs
```
ILogicalUnit<string, QualityReport>
Input:  result text
Output: QualityReport (checks passed, failures, score) (E39 +20%)
Calls:  QualityOrchestrator.Check from project 15
NOTE:   Wired into node 07 (arbiter selector position) of the nine-node flow.
        If quality score is below threshold, result goes to reprocess.
```

### File: ResultTranslatorUnit.cs
```
ILogicalUnit<ResultTranslatorRequest, string>
Input:  predicate int[] + stack label
Output: human-readable guidance (E23 +833%)
Calls:  TemplateBeautifier.FindBest, CSharpBeautifier fallback
```

### File: SkillRetrieverUnit.cs
```
ILogicalUnit<SkillRetrieverRequest, Skill[]>
Input:  query + candidate skills
Output: top-K relevant skills (E38V2 +100%, 86% load reduction)
Calls:  ContextWindowManager with KnnSkillFilter
```

### File: TrainingVectorBuilder.cs
```
Static helper — not an ILogicalUnit
TrainingVectorBuilder.Build(string prompt) → float[60]
Calls:  SemanticCompilerPipeline.Compile
        Builds 60-slot vector per E51B contract:
          slots 0-21:  22 predicate counts
          slots 22-26: 5 domain one-hot
          slots 27-58: 32 normalised root presence
          slot 59:     segment count (1=single task, 2+=multi-task)
```

### Gate B — Phase 1

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero build errors, 1224 tests passing (no behaviour changed yet).

---

## Phase 2 — Create project 17.DynamicDomains (E49, E50)

New project: `17.DynamicDomains/DynamicDomains.csproj`

References: project 22 (AgentAlgorithms), project 12 (SemanticCompiler)

### OrchestratorExpander.cs (E49, +44% dependency accuracy)

```
OrchestratorExpander discovers implicit dependencies between sub-phases
by measuring predicate-profile overlap.

Algorithm:
  Given a set of selected sub-phases from routing:
  1. For each pair of sub-phases A and B:
     Compute Jaccard similarity of their predicate sets.
  2. If similarity exceeds expansion threshold (0.35):
     Add the higher-confidence sub-phase as a dependency of the lower.
  3. Return the expanded sub-phase set with dependency edges added.

This prevents routing from missing a sub-phase that is semantically
required by a selected sub-phase but was not directly triggered.
Example: routing selects 3.4 (security) — expander adds 3.3 (API contract)
because security and API contract have high predicate overlap.
```

### DomainGraphClosure.cs (E50, +75% final edge delta)

```
DomainGraphClosure runs iterative closure over the dependency graph
until no new edges are added.

Algorithm:
  1. Start with direct edges from OrchestratorExpander.
  2. For each node, follow transitive edges and add them as direct edges.
  3. Repeat until the edge count stops growing.
  4. Return the closed graph — every node now has direct edges to all
     nodes it depends on, directly or transitively.

This ensures that when sub-phase 5.1 (unit tests) is selected and
depends on 4.1 (implementation) which depends on 3.4 (security),
all three appear in the routing result.
```

### Gate B — Phase 2

```bash
dotnet build 17.DynamicDomains/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero errors, 1224 tests still passing.

---

## Phase 3 — Build E34V2 HybridRanker in project 22

New file: `22.AgentAlgorithms/Context/HybridRanker.cs`

```
HybridRanker fuses BM25 and KNN scores per E34V2 (+8% hit@1).

Algorithm:
  Input: query string, candidate list
  
  1. Score each candidate with AstRelevanceRanker (BM25).
     Normalise scores to [0, 1].
  
  2. Score each candidate with KnnSkillFilter (cosine similarity KNN).
     Normalise scores to [0, 1].
  
  3. Fused score = alpha × BM25 + (1 - alpha) × KNN
     where alpha is tuned on training examples.
     Default alpha = 0.6 (BM25 weighted slightly higher for keyword-rich prompts).
  
  4. Return candidates sorted by fused score descending.

Alpha auto-tuning (when training examples are available):
  Try alpha in {0.3, 0.5, 0.6, 0.7, 0.9}.
  Select alpha that maximises hit@1 on held-out examples.
  Cache the selected alpha per domain.
```

### Gate B — Phase 3

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "HybridRanker" 2>&1 | tail -5
```

New tests to add:
- BM25-only score matches AstRelevanceRanker for alpha=1.0.
- KNN-only score matches KnnSkillFilter for alpha=0.0.
- Fused score is between the two component scores for 0 < alpha < 1.

---

## Phase 4 — Wire all adapter units into project 21

Add reference in `AgentInfrastructure.csproj`:
```xml
<ProjectReference Include="..\23.SemanticCompilerAdapter\SemanticCompilerAdapter.csproj" />
<ProjectReference Include="..\17.DynamicDomains\DynamicDomains.csproj" />
```

### 4A — PromptRoutingInputCompilerUnit

Replace hardcoded keyword lexicon with pipeline + segmenter + classifier:

```
New ExecuteAsync algorithm:

1. Call SemanticCompilerAdapterUnit.ExecuteAsync(prompt)
   → PipelineOutput (domain, predicates[], stack, compression)

2. Call ComplexityClassifierUnit.ExecuteAsync(prompt)
   → SINGLE_STEP or MULTI_STEP

3. Call PromptSegmenterUnit.ExecuteAsync(prompt)
   → PromptSegment[] (one or many)

4. For each segment:
   a. Extract predicates from PipelineOutput filtered to this segment's words
   b. Set domain label from DomainClassifierUnit result
   c. Construct SubPhaseRouterSourceDocument for this segment

5. Return array of SubPhaseRouterSourceDocuments
   (one per segment, or one total if single task)

The existing hardcoded PredicateLexicon stays as fallback if pipeline fails.
```

### 4B — MultiSubPhaseRouterUnit scoring augmentation

The router currently uses predicate overlap scoring only. Add BM25 retrieval:

```
New scoring algorithm:

For each sub-phase candidate:
  1. Compute existing predicate overlap score (unchanged, 70% weight)
  2. Call RelevanceRankerUnit with the prompt text and sub-phase description
     → HybridRanker returns BM25+KNN fused score (E04+E34V2)
  3. Combined score = 0.70 × predicate_overlap + 0.30 × retrieval_score

When the retrieval index has fewer than 5 examples for a sub-phase:
  Fall back to anchor-term token overlap (existing behaviour).

This makes the router's scoring partially learned:
  - Predicate overlap is deterministic (hardcoded predicates)
  - BM25+KNN score improves as the index grows with each training row
```

### 4C — DomainClassifierUnit as ILogicalUnit (making it teachable)

The domain classifier is currently a static keyword method. Wrapping it as
`DomainClassifierUnit : ILogicalUnit<string, DomainClassification>` means:

- At runtime, the orchestrator resolves it via `ILogicalUnitRegistry.ResolveLatest`
- When a Probationary trained version is registered, it replaces the keyword version
- The keyword version becomes the Trusted fallback if the learned version fails
- This is the same Candidate → Probationary → Trusted lifecycle as other units

The hardcoded `RouteClassifier.ClassifyByKeywords` becomes the initial Trusted unit.
When enough training data is available, a learned `TransformerEmbeddingKnnLogicalUnit`
replaces it. The interface never changes — only the implementation behind it does.

### 4D — DefaultLogicalCapabilityTrainer — three improvements

**Improvement 1: 60-slot feature vector training (E51B symmetry)**

Replace `BuildTrainingText` raw string with `TrainingVectorBuilder.Build`:
```
Chosen vector: Build(userRequest) → float[60]
Rejected vector: Build(userRequest) with domain slots shuffled to wrong domain
Pass both to DpoTrainer.StepVector (new method, Phase 5)
```

**Improvement 2: ReplayBuffer 70/30 mixing (E43 +23%)**

Replace inline `Dictionary<DomainLabel, List<string>>` replay with real `ReplayBuffer`:
```
Before each DPO step:
  replayBuffer.Add(new TrainingExample(chosenVector, domainLabel))
  batch = replayBuffer.Sample([currentExample]) — returns 70% new + 30% buffer
  DpoTrainer.StepVector(batch)
```

The `ReplayBuffer` from project 16 is now referenced via project 23's adapter
rather than reimplemented inline. The inline implementation is removed.

**Improvement 3: IdleDetector gate (E41 +100%)**

Wrap all training in an idle check:
```
Before training a batch:
  if (!idleDetector.IsIdle()) {
    learningEventScheduler.QueueIfReady(domain, 1, "deferred — system busy")
    return CapabilityNotRegisteredResult
  }
  // proceed with training
```

Training that fires during a busy system wastes CPU and produces weaker gradients.
The `IdleDetector` from project 16 is called via project 23's adapter.

**Improvement 4: TrustScorer promotion (E40V2 +50%)**

After training and verification:
```
var score = trustScorer.Score(unit, verificationResults)
if (score >= TrustThreshold) {
  registry.Promote(unit.UnitId, LogicalUnitTrustState.Trusted)
}
```

This replaces the current behaviour where units remain Probationary indefinitely.

### Gate B — Phase 4

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: all 1224 + new tests from phases 1-3 passing.

New tests to add in Phase 4:
- Multi-segment prompt produces N source documents
- Domain classifier resolves via registry (not just static keyword)
- Training uses 60-slot vectors not raw strings
- ReplayBuffer is called (not inline list)
- IdleDetector gate blocks training when busy
- TrustScorer promotes unit after N passing verifications

---

## Phase 5 — DpoTrainer.StepVector (feature-vector DPO)

**File:** `11.DPO/DpoTrainer.cs`

New method alongside existing `Step(DpoBatch, int)`:

```
Algorithm — StepVector(float[] chosen, float[] rejected, int step):

1. Project both 60-slot vectors into hidden dimension via trainable linear layer:
   projected_chosen   = projection_matrix × chosen    (shape: hidden_dim)
   projected_rejected = projection_matrix × rejected  (shape: hidden_dim)
   The projection_matrix is a LoRA adapter parameter (60 × hidden_dim).

2. Run projected vectors through the policy model's final normalisation + LM head
   to get log-probabilities.

3. Run same projections through frozen reference model.

4. Compute margin = (pol_chosen_logp - ref_chosen_logp)
                  - (pol_rejected_logp - ref_rejected_logp)

5. Loss = -log(sigmoid(margin))

6. Backward through projection layer and LoRA adapter parameters only.
   Base model frozen.

7. AdamW step.

8. Return DpoMetrics (same structure as existing Step).
```

The projection layer bridges the 60-slot predicate space and the model's hidden
space. For tiny model (hidden=16): 60×16 = 960 parameters. For Small (hidden=512):
60×512 = 30,720 parameters. Both are tiny relative to the base model.

### Gate B — Phase 5

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "DpoTrainer" 2>&1 | tail -5
```

New tests:
- StepVector returns DpoMetrics
- Loss decreases when chosen and rejected are clearly separated
- Projection layer weights update; base model weights do not

---

## Phase 6 — Wire result translation and quality check into the nine-node flow

**File:** `21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs`

**Node 06 (result translator):** wire `ResultTranslatorUnit`

```
Node 06 receives: accumulated predicate evidence from nodes 04 and 05
Node 06 calls:    ResultTranslatorUnit.ExecuteAsync(predicates, stackLabel)
Node 06 emits:    human-readable guidance string via TemplateBeautifier (E23)
```

**Node 07 (arbiter selector):** wire `QualityCheckUnit`

```
Node 07 receives: the translated result from node 06
Node 07 calls:    QualityCheckUnit.ExecuteAsync(resultText)
Node 07 emits:    QualityReport — if score below threshold, flags for reprocess
                  This is the E39 QualityOrchestrator (+20% check_f1) path
```

**Node 03 (scheme planner):** wire `EpisodeDecomposerUnit` + `DependencyResolverUnit`

```
Node 03 receives: all translated domain statements from node 02
Node 03 calls:    EpisodeDecomposerUnit to break work into sub-tasks (E32)
                  DependencyResolverUnit to order sub-tasks by dependency (E35)
Node 03 emits:    dependency-ordered work plan
```

**Node 04/05 (processor/applier):** use `SkillRetrieverUnit`

```
Before processing each work branch:
  Call SkillRetrieverUnit to retrieve the most relevant skills (E38V2)
  Pass retrieved skills as context into the processor
  This reduces noise by 86% (E38V2 result: 6 skills instead of 46)
```

### Gate B — Phase 6

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- Node 06 emits non-scaffold result for prompts with known predicate patterns
- Node 07 quality check flags results with low predicate coverage
- Node 03 scheme plan is dependency-ordered
- Node 04/05 use retrieved skills not full skill set

---

## Phase 7 — Wire OrchestratorExpander and DomainGraphClosure

**File:** `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

After `MultiSubPhaseRouterUnit` returns routing decisions, before branch execution:

```
Algorithm — post-routing expansion:

1. Pass routing decisions to OrchestratorExpander.Expand(decisions, topology)
   → Adds implicit dependency sub-phases (E49, +44% dependency accuracy)

2. Pass expanded set to DomainGraphClosure.Close(expanded)
   → Runs transitive closure until stable (E50, +75% final edge delta)

3. Apply branch cap of 8 to the closed set
   → Take top-8 by confidence score

4. Proceed with ExecuteBranchesAsync on the final capped set
```

This replaces the current situation where a prompt about security might miss the API
contract sub-phase because it was not directly scored. The expander adds it because
the security and API contract predicate sets overlap above threshold.

### Gate B — Phase 7

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- A security prompt expands to include API contract sub-phase
- Transitive closure adds grandparent dependencies
- Branch cap still applies after expansion

---

## Phase 8 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 1 (adapter project): 8 tests
- Phase 2 (DynamicDomains): 6 tests
- Phase 3 (HybridRanker): 3 tests
- Phase 4 (project 21 wiring): 6 tests
- Phase 5 (StepVector): 3 tests
- Phase 6 (nine-node wiring): 4 tests
- Phase 7 (expander + closure): 3 tests
- Total new: 33 tests
- New floor: 1257 (floor after consolidation is still 1224 — Phase 0 adds no tests)

Commit message:
```
consolidate duplicate source + wire all proven experiments into orchestration path

Source consolidation (Phase 0 — zero behaviour change):
  Domain enum unified: Domain {{ CODE_Q, BUG, PLAN, ARCH, REFACTOR }} in project 29 only
  RouteClassifier: project 29 only; projects 12 and 22 are thin wrappers
  13 pipeline classes in project 12 delegate to project 29 canonical implementations
  E51B inline Compile logic moved to TrainingVectorBuilder (production code)
  E21/E22 inline class copies replaced with production class calls

Projects added:
  17.DynamicDomains: OrchestratorExpander (E49), DomainGraphClosure (E50)
  23.SemanticCompilerAdapter: ILogicalUnit wrappers for all proven components

Prompt path (Path A):
  SemanticCompilerPipeline wired: E01,E15,E16,E17,E18,E20 active per prompt
  PromptSegmenterUnit: E21 multi-task splitting
  DomainClassifierUnit: E18 as ILogicalUnit (teachable, registry-resolved)
  ComplexityClassifierUnit: E19 SINGLE/MULTI_STEP routing
  HybridRanker: E04+E34V2 BM25+KNN fusion in MultiSubPhaseRouterUnit scoring
  EpisodeDecomposerUnit: E32 episode-level decomposition in node 03
  DependencyResolverUnit: E35 dependency ordering in node 03
  SkillRetrieverUnit: E38V2 86% noise reduction in nodes 04/05
  QualityCheckUnit: E39 quality gate in node 07
  OrchestratorExpander: E49 implicit dependency discovery post-routing
  DomainGraphClosure: E50 transitive closure on dependency graph

Training path (Path B):
  TrainingVectorBuilder: E51B 60-slot feature vector (teaching=runtime symmetry)
  DpoTrainer.StepVector: feature-vector DPO with trainable projection layer
  ReplayBuffer: E43 70/30 mixing (replaces inline implementation)
  IdleDetector: E41 training gated on system idle state
  TrustScorer: E40V2 unit promotion on verified quality

Result translation path (Path C):
  ResultTranslatorUnit: E23 TemplateBeautifier in node 06 (+833%)

Floor raises from 1224 to 1257
```

---

## Execution order and stop conditions

Execute phases in strict order: baseline gate, 0 (consolidation), 1, 2, 3, 4, 5, 6, 7, 8.

Stop immediately if:
- Phase 0 gate shows below 1224
- Any Gate B build step produces errors
- Any phase causes a previously passing test to fail
- A test is modified to suppress a genuine regression

---

## Complete experiment coverage summary

| Experiment | Improvement | Phase that wires it |
|---|---|---|
| E01 AstTokenizer | +93% perplexity | Phase 4A (via pipeline) |
| E03 N-gram | +64% perplexity | Phase 4A (pipeline stage internal) |
| E04 BM25 | +100% recall@3 | Phase 3+4B (HybridRanker) |
| E05 Syntax filter | +30% accuracy | Phase 4A (via pipeline) |
| E08 SyntaxRoles | +56% accuracy | Phase 6 (node 03 decomposer) |
| E09 KNN structural | +45% accuracy | Phase 3+4B (HybridRanker KNN side) |
| E11 KvCache | +54% step speed | Phase 6 (result lookup cache) |
| E15 Semantic tokenizer | +10% perplexity | Phase 4A (via pipeline) |
| E16 Synonym collapse | +8% perplexity | Phase 4A (via pipeline) |
| E17 Logic predicates | +97% perplexity | Phase 4A (via pipeline) |
| E18 Domain classifier | +15%, 95% | Phase 4A (DomainClassifierUnit) |
| E19 ComplexityRouter | +18% accuracy | Phase 4A (ComplexityClassifierUnit) |
| E21 Prompt splitter | +143% accuracy | Phase 4A (PromptSegmenterUnit) |
| E23 TemplateBeautifier | +833% match | Phase 6 (node 06) |
| E25 PatternClassifier | +155% accuracy | Phase 4A (pipeline normalisation) |
| E26 ContextualDictionary | +150% accuracy | Phase 4A (pipeline normalisation) |
| E27 PatternReorderer | +250% match | Phase 4A (pipeline normalisation) |
| E32 EpisodeDecomposer | +11% accuracy | Phase 6 (node 03) |
| E34V2 BM25+KNN fusion | +8% hit@1 | Phase 3 (HybridRanker) |
| E35 DependencyDetector | +30% accuracy | Phase 6 (node 03) |
| E38V2 SkillRetriever | +100%, 86% load | Phase 6 (nodes 04/05) |
| E39 QualityOrchestrator | +20% check_f1 | Phase 6 (node 07) |
| E40V2 TrustScorer | +50% accuracy | Phase 4D |
| E41 IdleDetector | +100% idle acc. | Phase 4D |
| E43 ReplayBuffer | +23% retention | Phase 4D |
| E49 OrchestratorExpander | +44% dep. acc. | Phase 7 |
| E50 DomainGraphClosure | +75% edge delta | Phase 7 |
| E51B Symmetry | teaching=runtime | Phase 5 |

**All 27 positive experiments addressed. None left disconnected.**

---

# Plan version: 4.0
# Author: ARCHITECT session 2026-05-06
# Phases: 8 (plus baseline gate)
# New tests: 33
# New floor: 1257
# New projects: 17.DynamicDomains, 23.SemanticCompilerAdapter
# New production classes: HybridRanker, OrchestratorExpander, DomainGraphClosure
