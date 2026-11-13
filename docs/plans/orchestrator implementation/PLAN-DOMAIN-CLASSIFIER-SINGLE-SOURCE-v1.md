# Plan — Domain Classifier as Single Source of Truth ILogicalUnit
# Version: 1.0 — 2026-05-06
# Session type: ARCHITECT
# Test floor entering this plan: current floor from v4 Phase 0 completion
# Companion to: PLAN-CONTEXT-ENRICHED-ROUTING-v1.md

---

## The problem this plan solves

Domain classification is re-implemented five times across the codebase. Each copy
has diverged vocabulary, diverged enum names, and diverged priority rules. A fix
to the keyword set in one copy is invisible to the other four.

| Location | Enum used | Domain names |
|---|---|---|
| `12.SemanticCompiler/Routing/RouteClassifier.cs` | `DomainLabel` | CODE_Q, BUG, PLAN, ARCH, REFACTOR |
| `22.AgentAlgorithms/Routing/RouteClassifier.cs` | `DomainLabel` | CodeQuestion, Bug, Plan, Architecture, Refactor |
| `29.SemanticCompiler/Routing/RouteClassifier.cs` | `Domain` | CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN |
| `21.AgentInfrastructure/Loop/InternalLlmGateway.cs` | `InternalModelDomain` | CodeQ, Bug, Plan, Arch, Refactor, Generic |
| `35.ExperimentsV2/Infrastructure/DomainClassifier.cs` | integer constants | CODE_QUESTION=0, BUG_FIX=1, CODE_CHANGE=2, ORCHESTRATION=3, ARCHITECTURE=4 |

Additionally, the experiment `Infrastructure` folder contains its own standalone
production-class reimplementations:
- `PromptSplitter` — duplicates `12.SemanticCompiler/Planning/Decomposer`
- `AttributedRouter` — never in production, builds on the experiment PromptSplitter
- `DatasetRouter` — experiment-only dataset relevance routing
- `SemanticNormalizer` — duplicates `12.SemanticCompiler/Compression/SemanticNormalizer`
- `SentencePatternClassifier` — duplicates `12.SemanticCompiler/Translation/PatternClassifier`
- `FamilyCollapser` — duplicates `12.SemanticCompiler/Compression/FamilyCollapser`
- `LogicExtractor` — duplicates `12.SemanticCompiler/Compression/LogicExtractor`
- `StopSignalDetector` — duplicates `12.SemanticCompiler/Planning/StopSignalDetector`

E51B has three private methods (`NormalizeRoots`, `PredicateNamesForRoot`,
`BuildVector`) that implement the 60-slot feature vector contract and exist
nowhere in production.

The goal of this plan: **one `DomainClassifierUnit` implementing `ILogicalUnit`,
registered in the unit registry, used everywhere — production and experiments.**

---

## The canonical implementation

The single source of truth is `12.SemanticCompiler/Routing/RouteClassifier.cs`.
It has:
- The correct D-04 canonical enum values: `CODE_Q, BUG, PLAN, ARCH, REFACTOR`
- Both keyword baseline (`ClassifyByKeywords`) and KNN trained path (`Classify`)
- `AutoLabel` for training data generation
- The correct priority order: BUG before REFACTOR before PLAN before ARCH before CODE_Q

Project 29's `RouteClassifier` is structurally better (injectable, BaseService) but
has a subset of functionality. Project 12 is canonical for vocabulary.

The plan: move the canonical implementation into `23.SemanticCompilerAdapter` as
`DomainClassifierUnit : ILogicalUnit<string, DomainClassification>`, with project 12
as its backing implementation. All five copies become callers of this one unit.

---

## Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Record the count. Every subsequent gate must match or exceed it.

---

## Phase 1 — Create DomainClassifierUnit in project 23

**Prerequisites:** v4 Phase 0 must be complete (Domain enum unified in project 29,
project 12 RouteClassifier delegates to project 29).

### File: `23.SemanticCompilerAdapter/DomainClassifierUnit.cs`

```
Contract:
  DomainClassifierUnit : ILogicalUnit<DomainClassifierRequest, DomainClassification>

DomainClassifierRequest:
  string Prompt                     — the text to classify
  int[]? Predicates                 — optional compiled predicates (if already compiled)
  LogicalContextPacket? PriorContext — optional prior session context

DomainClassification:
  Domain Label                      — CODE_Q / BUG / PLAN / ARCH / REFACTOR
  double Confidence                 — 0.20 to 0.99
  string[] Evidence                 — matched keywords or predicate IDs
  bool UsedContext                  — true if prior context influenced result
```

Algorithm — ExecuteAsync:

```
1. If Predicates provided AND classifier has been trained (KNN path):
   Call RouteClassifier.Classify(predicates) → domain + confidence.

2. Else (keyword path, no training required):
   Call RouteClassifier.ClassifyByKeywords(prompt) → domain.
   Confidence = keyword hit ratio (matched keywords / total keywords in winner set).

3. If PriorContext is not null and UseContextCarryForward flag is set:
   Read prior domain from context fragments.
   If prior domain matches this classification: confidence boost +0.10.
   If prior domain differs: confidence reduction -0.05.
   Clamp confidence to [0.20, 0.99].
   Set UsedContext = true.

4. Map Domain to string for SubPhaseRouterSourceDocument.DomainLabel.

5. Return DomainClassification.
```

This is the single unit that replaces all five copies. The KNN path activates
when training data has been provided via the unit registry's TrainAsync mechanism.
The keyword path is always available as fallback.

### Register in unit registry at startup

```csharp
// Register keyword baseline as initial Trusted unit.
// When enough training data accumulates, a learned KNN version
// will be registered as Probationary and promoted to Trusted.
unitRegistry.Register(
    new DomainClassifierUnit(routeClassifier),
    role: "domain_classifier",
    subPhaseId: "*",           // applies to all sub-phases
    trustState: Trusted);
```

### Gate B — Phase 1

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero build errors, floor unchanged.

New tests to add:
- Keyword path returns CODE_Q for "what does this do"
- Keyword path returns BUG for "fix the null reference"
- Keyword path returns PLAN for "plan the deployment sequence"
- KNN path requires training before use, falls back to keyword when untrained
- Prior context boost increases confidence for matching domain
- Prior context penalty reduces confidence for different domain
- Confidence is always clamped to [0.20, 0.99]

---

## Phase 2 — Replace all five copies with DomainClassifierUnit calls

### Step 2A — Replace `22.AgentAlgorithms/Routing/RouteClassifier.cs`

Delete the `enum DomainLabel { CodeQuestion, Bug... }` local definition.
Delete the `RouteClassifier` class body.
Replace callers in project 22 with calls to project 12's `RouteClassifier`
(which itself delegates to project 29 after v4 Phase 0).

Files to update in project 22:
- `Routing/RouteClassifier.cs` — becomes a thin delegating wrapper
- `Comparison/AgentComparisonHarness.cs` — update import
- `Comparison/ComparisonResult.cs` — update enum reference

Grep to verify before editing:
```bash
grep -rn "DomainLabel\." 22.AgentAlgorithms/ --include="*.cs" | grep -v obj
```

Gate B — Step 2A:
```bash
dotnet build 22.AgentAlgorithms/ 2>&1 | grep "error" | head -10
```

---

### Step 2B — Replace `21.AgentInfrastructure/Loop/InternalLlmGateway.cs`

The `InternalModelDomain` enum and the private `Classify` method in
`InternalLlmGateway` must be removed.

Current: `InternalLlmGateway` has its own `Classify(string prompt)` method
that uses `InternalModelDomain` keyword scoring inline.

Target: inject `DomainClassifierUnit` into `InternalLlmGateway`.
The gateway resolves it from the unit registry by role `"domain_classifier"`.

Before editing:
```bash
grep -rn "InternalModelDomain\|private.*Classify\b" \
  21.AgentInfrastructure/ --include="*.cs" | grep -v obj
```

Algorithm change in InternalLlmGateway:
```
Old: private (InternalModelDomain Domain, double Confidence) Classify(string prompt)
     → inline keyword scoring using InternalModelDomain enum

New: var result = await _domainClassifier.ExecuteAsync(
         new DomainClassifierRequest(prompt, null, null), ct)
     → maps Domain.BUG → InternalModelDomain.Bug etc for the gateway's adapter registry
     → the mapping table is a 5-entry switch, lives in the gateway only
```

The `InternalModelDomain` enum stays — it is the gateway's internal adapter key.
Only the classification logic moves to DomainClassifierUnit. The mapping from
`Domain` to `InternalModelDomain` is a 5-line switch in the gateway.

Gate B — Step 2B:
```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

---

### Step 2C — Replace experiment Infrastructure/DomainClassifier.cs

The `35.ExperimentsV2/Infrastructure/DomainClassifier.cs` uses integer constants
`CODE_QUESTION=0, BUG_FIX=1, CODE_CHANGE=2, ORCHESTRATION=3, ARCHITECTURE=4`.

These do not map cleanly to D-04 — `CODE_CHANGE` and `ORCHESTRATION` are not
D-04 domains. They were experiment-level groupings used before D-04 was locked.

Target: replace `DomainClassifier.PredictByKeywords(text)` calls in experiments
with `RouteClassifier.ClassifyByKeywords(text)` from project 12, returning
`Domain` enum values. Update any experiment that reads integer constants to
read `Domain` string names instead.

Experiments that use `DomainClassifier` directly:
```bash
grep -rln "DomainClassifier\b" 35.ExperimentsV2/ --include="*.cs" | grep -v obj
```

For each caller: replace `DomainClassifier.PredictByKeywords(x)` with
`RouteClassifier.ClassifyByKeywords(x)` and update the comparison from
integer constants to `Domain.BUG`, `Domain.PLAN`, etc.

The `DomainClassifier.cs` file in Infrastructure is deleted after all callers
are updated.

Gate B — Step 2C:
```bash
dotnet build 35.ExperimentsV2/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

---

### Step 2D — Replace experiment Infrastructure production class copies

The following Infrastructure files duplicate production classes and must be
replaced with calls to the canonical production implementation:

| Infrastructure file | Replace with |
|---|---|
| `PromptSplitter.cs` | `12.SemanticCompiler.Planning.Decomposer` |
| `SemanticNormalizer.cs` | `12.SemanticCompiler.Compression.SemanticNormalizer` |
| `SentencePatternClassifier.cs` | `12.SemanticCompiler.Translation.PatternClassifier` |
| `FamilyCollapser.cs` | `12.SemanticCompiler.Compression.FamilyCollapser` |
| `LogicExtractor.cs` | `12.SemanticCompiler.Compression.LogicExtractor` |
| `StopSignalDetector.cs` | `12.SemanticCompiler.Planning.StopSignalDetector` |

For each file:
1. Grep every experiment that imports it:
   ```bash
   grep -rln "PromptSplitter\b" 35.ExperimentsV2/ --include="*.cs"
   ```
2. Update each caller to import from `SemanticCompiler` namespace instead.
3. Delete the Infrastructure file.

The `35.ExperimentsV2` project adds a reference to `12.SemanticCompiler` if not
already present.

`AttributedRouter.cs` and `DatasetRouter.cs` are kept for now — they have no
production equivalent yet. `AttributedRouter` will be replaced by
`AttributedSegmentRouter` from the context-enriched routing plan. `DatasetRouter`
is experiment-only and has no production role.

Gate B — Step 2D:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: floor unchanged. Behaviour is identical — same algorithm, different import.

---

### Step 2E — Move E51B's 60-slot vector builder to production

E51B has four private methods that implement the feature vector contract:
`NormalizeRoots`, `PredicateNamesForRoot`, `BuildVector`, `PredicateSlot`.
These exist nowhere in production. The contract they implement is the E51B
`FeatureVectorContract` constant: "60 slots: 22 predicate counts, 5 domain slots,
up to 32 normalized roots, and 1 decomposition-count slot."

Target: create `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`.

Move the four methods verbatim from E51B into `TrainingVectorBuilder` as
`internal static` methods. Add one public entry point:

```csharp
// Build a 60-slot feature vector from a raw prompt.
// Calls SemanticCompilerPipeline internally for normalisation.
// Both training and runtime must call this method — E51B symmetry requirement.
public static float[] Build(string prompt)
{
    var roots = NormalizeRoots(prompt).ToArray();
    var predicates = roots.SelectMany(PredicateNamesForRoot).Distinct().ToArray();
    // rootVocab is built from the current training corpus in the unit registry.
    var vocab = BuildRootVocab(roots);
    return BuildVector(roots, predicates, prompt, vocab);
}
```

In E51B: replace the four private methods with calls to `TrainingVectorBuilder`:
```csharp
// Compile now delegates to the production TrainingVectorBuilder.
private static (string[] Predicates, string[] Roots, float[] Vector)
    Compile(string request, Dictionary<string, int> rootVocab)
{
    var roots = TrainingVectorBuilder.NormalizeRootsInternal(request).ToArray();
    var predicates = TrainingVectorBuilder.PredicateNamesForRootInternal(roots);
    return (predicates, roots,
        TrainingVectorBuilder.BuildVectorInternal(roots, predicates, request, rootVocab));
}
```

Gate B — Step 2E:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: floor unchanged. E51B test output must be bit-identical to before.

---

## Phase 3 — Wire DomainClassifierUnit into AttributedSegmentRouter

The `AttributedSegmentRouter` (from the context-enriched routing plan) must
use `DomainClassifierUnit` — not `RouteClassifier.ClassifyByKeywords` directly —
so that:

- Classification is registry-resolved (Probationary learned version can replace it)
- Context packet biasing applies to each segment's classification
- Training examples for the domain classifier are generated from segment calls

```
AttributedSegmentRouter.Route(segments[], contextSignals):

  For each segment:
    request = new DomainClassifierRequest(
        segment.Text,
        compiledPredicates,     // from SemanticCompilerAdapterUnit
        contextSignals.PriorContextPacket)

    result = unitRegistry.ResolveLatest(role: "domain_classifier")
             .ExecuteAsync(request)

    → DomainClassification for this segment only
    → Constructs SubPhaseRouterSourceDocument with segment text + domain
```

This is the connection between the two plans: context-enriched routing uses
the DomainClassifierUnit registry entry. Any improvement to the domain
classifier — whether from new training data, new keywords, or a new learned
KNN model — automatically flows to both the single-prompt path and the
multi-segment attributed path.

Gate B — Phase 3:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- AttributedSegmentRouter resolves DomainClassifierUnit from registry
- Replacing DomainClassifierUnit in registry changes segment routing
  (proves the registry-resolution path, not hard-coded calls)
- Segment with BUG text gets Domain.BUG; segment with PLAN text gets Domain.PLAN

---

## Phase 4 — Verify single source of truth

Run the following greps. Every expected result must hold before committing.

```bash
# One Domain enum — only in project 29
grep -rn "enum DomainLabel\|enum Domain\b\|enum InternalModel" \
  . --include="*.cs" | grep -v obj | grep "enum "
```
Expected: `enum Domain` in `29.SemanticCompiler/Core/Core.cs` only.
`InternalModelDomain` in `21.AgentInfrastructure/Loop/InternalLlmGateway.cs` only
(this is the gateway's adapter key, not a classifier enum — acceptable).

```bash
# One RouteClassifier implementation — only in project 12 (delegates to 29)
grep -rn "class RouteClassifier" . --include="*.cs" | grep -v obj
```
Expected: one result in project 12, one in project 29.
Project 22 has a delegating wrapper only (no class body with keyword arrays).

```bash
# No DomainClassifier in experiments Infrastructure
find 35.ExperimentsV2/Infrastructure -name "DomainClassifier.cs"
```
Expected: no file found.

```bash
# No PromptSplitter/SemanticNormalizer/PatternClassifier in experiments Infrastructure
find 35.ExperimentsV2/Infrastructure \
  -name "PromptSplitter.cs" -o -name "SemanticNormalizer.cs" \
  -o -name "SentencePatternClassifier.cs" -o -name "FamilyCollapser.cs" \
  -o -name "LogicExtractor.cs" -o -name "StopSignalDetector.cs"
```
Expected: no files found.

```bash
# NormalizeRoots only in TrainingVectorBuilder
grep -rn "NormalizeRoots\|PredicateNamesForRoot" \
  . --include="*.cs" | grep -v obj | grep -v Experiment
```
Expected: only in `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`.

```bash
# Domain classifier role registered once
grep -rn "domain_classifier" . --include="*.cs" | grep -v obj
```
Expected: one registration site (startup/seeder), multiple resolve sites.

Gate B — Phase 4:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```
Expected: floor unchanged.

---

## Phase 5 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 1 (DomainClassifierUnit): 7 tests
- Phase 2A (project 22 replacement): 2 tests
- Phase 2B (InternalLlmGateway): 2 tests
- Phase 2C (experiment DomainClassifier): 2 tests
- Phase 2D (Infrastructure copies): 0 new tests (behaviour-identical replacements)
- Phase 2E (TrainingVectorBuilder): 3 tests
- Phase 3 (AttributedSegmentRouter wiring): 3 tests
- Total new: 19 tests
- New floor: prior floor + 19

Commit message:
```
single source of truth domain classifier as ILogicalUnit

DomainClassifierUnit (23.SemanticCompilerAdapter):
  ILogicalUnit<DomainClassifierRequest, DomainClassification>
  keyword path always available; KNN path activates when trained
  context packet biasing built in (prior domain continuity)
  registered as role "domain_classifier" — registry-resolved everywhere
  replaces five diverged copies across projects 12, 21, 22, 29, and experiments

TrainingVectorBuilder (23.SemanticCompilerAdapter):
  NormalizeRoots, PredicateNamesForRoot, BuildVector, PredicateSlot moved from E51B
  public Build(string prompt) → float[60] — used by training and runtime equally
  E51B Compile delegates to TrainingVectorBuilder (E51B symmetry preserved)

Source consolidation:
  22.AgentAlgorithms RouteClassifier → thin wrapper over project 12
  InternalLlmGateway inline Classify → resolves DomainClassifierUnit from registry
  35.ExperimentsV2 DomainClassifier → deleted, callers use RouteClassifier
  Infrastructure/PromptSplitter → callers use 12.SemanticCompiler.Decomposer
  Infrastructure/SemanticNormalizer → callers use 12.SemanticCompiler.SemanticNormalizer
  Infrastructure/SentencePatternClassifier → callers use 12.SemanticCompiler.PatternClassifier
  Infrastructure/FamilyCollapser → callers use 12.SemanticCompiler.FamilyCollapser
  Infrastructure/LogicExtractor → callers use 12.SemanticCompiler.LogicExtractor
  Infrastructure/StopSignalDetector → callers use 12.SemanticCompiler.StopSignalDetector

Floor raises by 19 tests
```

---

## Execution order and stop conditions

Phases in strict order: baseline gate → 1 → 2A → 2B → 2C → 2D → 2E → 3 → 4 → 5.

Stop immediately if:
- Any Gate B step shows a test count below the current floor
- Any Gate B build step produces errors
- A test is modified to suppress a genuine regression

---

## What does NOT change

| Component | Unchanged |
|---|---|
| `DomainLabel` enum values in project 12 | Used as canonical — CODE_Q, BUG, PLAN, ARCH, REFACTOR |
| `InternalModelDomain` enum in gateway | Stays as adapter key — only classification logic moves |
| `AttributedRouter.cs` in Infrastructure | Kept — no production equivalent yet |
| `DatasetRouter.cs` in Infrastructure | Kept — experiment-only |
| Any experiment's test assertions | Assertions update only if the domain name format changes |
| Nine-node flow, orchestrator, trainer | Unchanged — covered in v4 plan |

---

## Relationship to other plans

This plan executes after v4 Phase 0 (Domain enum unification) because:
`DomainClassifierUnit` uses the canonical `Domain` enum from project 29.
Running this plan before v4 Phase 0 would build against the old `DomainLabel`.

This plan executes before or alongside the context-enriched routing plan because:
Phase 3 of this plan wires `DomainClassifierUnit` into `AttributedSegmentRouter`,
which is built by the context-enriched routing plan.

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-06
# Phases: 5 (with 2 split into 5 steps)
# New classes: DomainClassifierUnit, TrainingVectorBuilder
# Deleted: Infrastructure/DomainClassifier.cs + 6 Infrastructure production copies
# Replaced: 5 diverged domain classification implementations → 1 registry unit
# New tests: 19
