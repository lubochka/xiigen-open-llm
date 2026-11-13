# Plan: E02, E03, E05, E11 — Integration into Production
# Version: 1
# Date: 2026-05-07
# Floor before: 1265
# Floor after:  1281 (+16 tests)

---

## Context

Four experiments are proven but not wired into production:

- E02 Frequency Ranking: +26.5% top1 accuracy on method completion
- E03 N-gram Model: +44.1% perplexity reduction, optimal N=2 at current scale
- E05 Syntax Filter: +29.9% top1 accuracy via grammar mask on AST predictions
- E11 Step Timer: proven AST is 1.5× faster than BPE per training step — but the
  trainer has no convergence monitoring, it runs a fixed step count every time

The three model classes that E03 and E05 depend on (UnigramModel, BigramModelV2,
TrigramModelV2) live only in `35.ExperimentsV2/Core/NgramModel.cs`. The frequency
table from E02 lives only in `35.ExperimentsV2/Experiments/E02FrequencyRanking.cs`.
Neither is reachable from `12.SemanticCompiler` or `21.AgentInfrastructure`.

E11's ProbeNeuralNetwork convergence pattern is not the right production mechanism —
the production trainer uses DPO, not a probe network. What E11 proves is the signal:
AST tokenization converges faster than BPE. The production consequence is to use
AST step count as the convergence budget, not a fixed ceiling.

---

## What each experiment integrates into

**E02** → `12.SemanticCompiler/Completion/MethodFrequencyTable.cs` (new).
Ranks method completions by observed call frequency per receiver type.
Called from `SemanticCompilerFacade.RankCompletions()` (new method).

**E03** → `12.SemanticCompiler/Prediction/NgramPredictor.cs` (new).
Moves UnigramModel, BigramModelV2, TrigramModelV2 from experiments to production.
Exposes `Train(tokens)` + `PredictNext(prev1, prev2)` as a production service.
Used by E05 grammar filter and directly in `SemanticCompilerFacade.Compile()`.

**E05** → `12.SemanticCompiler/Prediction/GrammarMaskFilter.cs` (new).
Wraps NgramPredictor with a grammar table built from Roslyn AST continuations.
Filters next-token predictions to syntactically valid continuations.
Called from `SemanticCompilerFacade.Compile()` after AST tokenization.

**E11** → `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs` (modified).
Adds convergence monitoring to the DPO training loop: stop early when loss delta
falls below threshold for two consecutive measurement windows.
The measurement interval and threshold are configurable.

---

## Phase 1 — Move N-gram models to production (E03 foundation)

**New file:** `12.SemanticCompiler/Prediction/NgramPredictor.cs`

Move `UnigramModel`, `BigramModelV2`, `TrigramModelV2`, and `NgramUtils` from
`35.ExperimentsV2/Core/NgramModel.cs` into the production namespace
`SemanticCompiler12.Prediction`. Change namespace, keep all logic identical.
Add a thin `NgramPredictor` façade that selects optimal N automatically:

```csharp
namespace SemanticCompiler12.Prediction;

// Production N-gram predictor. Trains unigram + bigram; selects bigram
// when it achieves lower perplexity than unigram (E03 proven: optimal N=2
// at current data scale). Falls back to unigram when bigram has insufficient
// observations (avg < 3 per context key).
public sealed class NgramPredictor
{
    private UnigramModel?   _unigram;
    private BigramModelV2?  _bigram;
    private bool            _useBigram;

    public void Train(IReadOnlyList<int> tokens) { ... }
    public int PredictNext(int prev) { ... }
    public double Probability(int prev, int token) { ... }
    public int OptimalN { get; private set; }
}
```

The experiments stay in `35.ExperimentsV2` — they reference the production class
via the project reference that already exists.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 2 — Method frequency table (E02)

**New file:** `12.SemanticCompiler/Completion/MethodFrequencyTable.cs`

Extracts `ExtractMethodCalls` from `E02FrequencyRanking` into a production class.
Exposes `Train(IReadOnlyList<string> sources)` + `RankMethods(string receiver,
IReadOnlyList<string> candidates)`.

```csharp
namespace SemanticCompiler12.Completion;

// Ranks method candidates by observed call frequency per receiver type.
// E02 proven: frequency ordering beats alphabetical by +26.5% top1 at tier 3.
// Receiver is the left-hand identifier: "list" in "list.Add(...)".
public sealed class MethodFrequencyTable
{
    // frequency[receiver][method] = call count across all trained sources
    private readonly Dictionary<string, Dictionary<string, int>> _freq = new();

    public void Train(IReadOnlyList<string> csharpSources) { ... }

    // Return candidates sorted by descending frequency for this receiver.
    // Unknown receivers return candidates in original order (stable fallback).
    public IReadOnlyList<string> RankMethods(
        string receiver,
        IReadOnlyList<string> candidates) { ... }

    // Total unique (receiver, method) pairs observed.
    public int TotalPairs { get; private set; }
}
```

**Modified:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

Add `RankCompletions(string receiver, IReadOnlyList<string> candidates)` that
delegates to `MethodFrequencyTable`. The table is trained lazily on the first
call using the source files already compiled in the session.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 3 — Grammar mask filter (E05)

**New file:** `12.SemanticCompiler/Prediction/GrammarMaskFilter.cs`

```csharp
namespace SemanticCompiler12.Prediction;

// Restricts N-gram next-token predictions to syntactically valid continuations.
// Grammar table: (prev2, prev1) → {valid next AST node type IDs}.
// Built from Roslyn parse of all available source files (E05 proven: +29.9%).
// When the mask is empty for a context, falls through to unconstrained prediction.
public sealed class GrammarMaskFilter
{
    private Dictionary<(int, int), HashSet<int>> _table = new();
    private NgramPredictor? _predictor;

    public void Train(
        IReadOnlyList<int> tokens,
        IReadOnlyList<string> allSources,
        Dictionary<string, int> kindToIdx) { ... }

    // Predict next token: grammar-masked if context is known, unconstrained otherwise.
    public int PredictNext(int prev2, int prev1) { ... }

    // Number of grammar contexts (bigram pairs) in the table.
    public int ContextCount { get; private set; }
}
```

**Modified:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

`Compile(source)` adds an optional post-step: when `GrammarMaskFilter` is
initialized, the AST token sequence is run through the filter to produce a
`PredictedNextKinds` list in `CompilationResult`. This is the grammar-aware
next-token prediction output.

The filter is initialized once per `SemanticCompilerFacade` instance from the
source files seen so far, rebuilt lazily when new files are added.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 4 — Convergence monitoring in the trainer (E11)

**Modified:** `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`

E11 shows AST converges in ~100 steps vs BPE ~150 steps. The current trainer
runs a fixed `TrainingStepCount` ceiling regardless of whether the model has
already converged. This wastes DPO time on steps that move loss by <0.001.

Add `ConvergenceMonitor` as an inner class:

```csharp
// Detects DPO training convergence: when loss delta falls below threshold
// for two consecutive measurement windows, signals early stop.
// Measurement window = every 25 steps. Threshold = 0.002 (E11 proven).
private sealed class ConvergenceMonitor
{
    private readonly float _threshold;
    private readonly int   _windowSteps;
    private float  _prevLoss    = float.MaxValue;
    private int    _stableCount = 0;

    public ConvergenceMonitor(float threshold = 0.002f, int windowSteps = 25) { ... }

    // Call after each window of steps. Returns true when training should stop.
    public bool ShouldStop(float currentLoss) { ... }

    // Steps actually taken before convergence (for manifest recording).
    public int StepsTaken { get; private set; }
}
```

Wire into the DPO loop in `TrainCapabilityAsync` (or equivalent): after each
measurement window, check `monitor.ShouldStop(loss)`. If true, break and record
`StepsTaken` in the manifest instead of the ceiling.

This does not change the training output — it only stops early when the model
has converged. The floor stays the same; the calibration run gets shorter.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero failures.

---

## Phase 5 — Tests

**New file:** `Tests/E02E03E05E11IntegrationTests.cs`

### E02 — Method frequency table

**Test 1 — Frequency ranking beats alphabetical**
```
Given: sources with 10 calls to list.Add, 3 to list.Count, 1 to list.Aggregate
When:  MethodFrequencyTable trained, RankMethods("list", ["Aggregate","Add","Count"])
Then:  result[0] == "Add"
       result[1] == "Count"
       result[2] == "Aggregate"
```

**Test 2 — Unknown receiver returns original order**
```
Given: table trained on sources with no calls to "unknownObj"
When:  RankMethods("unknownObj", ["Foo", "Bar"])
Then:  result == ["Foo", "Bar"]  (original order preserved)
```

**Test 3 — TotalPairs counts distinct (receiver, method) pairs**
```
Given: source with 5 calls to list.Add and 2 calls to dict.TryGetValue
When:  Train called
Then:  TotalPairs == 2
```

### E03 — N-gram predictor

**Test 4 — Bigram selected over unigram when more data available**
```
Given: token sequence [1,2,1,2,1,2,3,1,2,1,2] (bigram 1→2 dominant)
When:  NgramPredictor.Train called
Then:  OptimalN == 2
       PredictNext(1) == 2
```

**Test 5 — Unigram fallback when sequence too short**
```
Given: token sequence [1,2,3] (only 3 tokens — bigram has 0 avg observations)
When:  NgramPredictor.Train called
Then:  no exception thrown
       PredictNext returns a valid token id
```

**Test 6 — Probability sums are valid**
```
Given: trained predictor on [1,2,1,2,1,3]
When:  Probability(1, 2) called
Then:  result is in (0.0, 1.0]
```

### E05 — Grammar mask filter

**Test 7 — Grammar mask restricts prediction to observed continuations**
```
Given: C# source "if (x) { return y; }"
       GrammarMaskFilter trained on that source
       AST tokens for the same source
When:  PredictNext(prev2, prev1) called for a known context
Then:  result is a token id that actually appears after (prev2, prev1) in training
       result is NOT a token id that never appears after that context
```

**Test 8 — Empty mask falls back to unconstrained prediction**
```
Given: filter trained on source A
       unseen context (prev2=999, prev1=998) not in grammar table
When:  PredictNext(999, 998) called
Then:  no exception thrown
       result is a valid token id (from unigram fallback)
```

**Test 9 — ContextCount reflects number of bigram pairs in grammar table**
```
Given: source with 5 distinct consecutive AST node pairs
When:  GrammarMaskFilter.Train called
Then:  ContextCount >= 5
```

**Test 10 — Accuracy with filter >= accuracy without filter**
```
Given: C# source file with at least 20 AST nodes
       GrammarMaskFilter trained on 80% of AST tokens
       Evaluation on remaining 20%
When:  top1 accuracy computed with and without mask
Then:  accuracy_with_mask >= accuracy_without_mask - 0.05
       (allows 5% tolerance for edge cases; E05 proven +29.9% on average)
```

### E11 — Convergence monitor

**Test 11 — ShouldStop returns false while loss is still improving**
```
Given: ConvergenceMonitor(threshold=0.01, windowSteps=25)
When:  ShouldStop called with losses [0.9, 0.7, 0.5, 0.3]
Then:  all calls return false
```

**Test 12 — ShouldStop returns true after two stable windows**
```
Given: ConvergenceMonitor(threshold=0.01, windowSteps=25)
When:  ShouldStop(0.691) → false
       ShouldStop(0.690) → false  (delta=0.001 < threshold)
       ShouldStop(0.690) → true   (second stable window)
Then:  third call returns true
```

**Test 13 — StepsTaken records actual steps not ceiling**
```
Given: ConvergenceMonitor(threshold=0.01, windowSteps=25)
       converges after 3 windows = 75 steps
       ceiling would be 500 steps
When:  ShouldStop triggers stop at step 75
Then:  StepsTaken == 75
```

### Integration — E03 feeds E05

**Test 14 — NgramPredictor and GrammarMaskFilter share the same token vocabulary**
```
Given: C# source compiled to AST tokens via SemanticCompilerFacade
When:  NgramPredictor trained on those tokens
       GrammarMaskFilter trained on same tokens + source files
Then:  PredictNext from GrammarMaskFilter returns token ids
       that are valid indices in the NgramPredictor vocabulary
```

### Integration — E02 callable from SemanticCompilerFacade

**Test 15 — RankCompletions delegates to MethodFrequencyTable**
```
Given: SemanticCompilerFacade with MethodFrequencyTable trained on C# sources
       where "list.Add" appears 5 times, "list.Clear" appears 1 time
When:  facade.RankCompletions("list", ["Clear", "Add", "Count"])
Then:  result[0] == "Add"
```

**Test 16 — Compile output includes PredictedNextKinds when GrammarMaskFilter active**
```
Given: SemanticCompilerFacade with GrammarMaskFilter initialized
       C# source "class Foo { void Bar() { } }"
When:  facade.Compile(source)
Then:  result.Value.PredictedNextKinds is not null
       result.Value.PredictedNextKinds.Count > 0
```

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1281, zero failures.
(+16 new tests)

---

## What stays in experiments

The `35.ExperimentsV2` experiment classes (E02FrequencyRanking, E03NgramModel,
E05SyntaxFilter, E11StepTimer) are not deleted. They continue to reference the
production classes through project references. This means future experiment runs
measure the production implementation, not a separate copy.

---

## Execution order

Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5.

Each phase gate: `dotnet test`, floor 1265, zero failures before next phase.
Phase 5 raises floor to 1281.

---

## Floor summary

| After | Floor |
|---|---:|
| Start | 1265 |
| Phases 1–4 | 1265 |
| Phase 5 | 1281 |
