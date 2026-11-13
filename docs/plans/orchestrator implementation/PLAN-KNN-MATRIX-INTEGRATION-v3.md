# PLAN-KNN-MATRIX-INTEGRATION-v3

## How this was built

Every experiment file read directly from `35.ExperimentsV2/Experiments/`. Every production class
header read. Every cross-project call from `21.AgentInfrastructure/` traced. The
`EXPERIMENT-RESULTS-MAP.md` used as the authoritative pass/fail registry.

Previous versions of this plan (v1, v2) contained errors:
- v1: included 5 failed experiments as integration targets
- v2: missed the second BM25 path (`HybridRanker`), missed the complete unwired experiment list,
  mislabeled E20 as fully wired without checking both implementations

---

## True current state

### The two active ranking paths in project 21

**Path A — Sub-phase routing** (`MultiSubPhaseRouterUnit` → `HybridRanker`):
Active on every prompt. Ranks sub-phase candidates using `HybridRanker` from `22.AgentAlgorithms`.
`HybridRanker` fuses BM25 (weight 0.55) + KNN seed (weight 0.45).
Gap: `HybridRanker.Tokenize()` uses whitespace split, no E15 stemming.

**Path B — Learned capability routing** (`GeneratedCapabilityFlow` → `TransformerEmbeddingKnnLogicalUnit`):
Active only after training. Ranks learned capability units using `TransformerEmbeddingKnnLogicalUnit.ScoreCandidates`.
Gap: `Tokenize()` also uses whitespace split, no E15 stemming.

Both paths have the same E34V2 tokenizer gap. They are independent and need the fix applied separately.

### Experiment outcomes (from `EXPERIMENT-RESULTS-MAP.md`)

**Passed — relevant to integration:**

| ID | Delta | Target in production | Where |
|----|-------|---------------------|-------|
| E08 (NonCode) | +56% top1_accuracy | `RoleBasedTaskDecomposer` | 22 Tasks/ |
| E09 | +45% top1_accuracy | `AstRelevanceRanker` (KNN structural keys) | 22 Context/ |
| E18 | +15% domain accuracy | `RouteClassifier` → `DomainClassifierUnit` | 23 / 29 |
| E23 | +833% template_match_rate | `TemplateBeautifier` | 29 Beautification/ |
| E25 | +155% top1_accuracy | `PatternClassifier` | 29 Translation/ |
| E26 | +150% disambiguation_accuracy | `ContextualDictionary` | 29 Translation/ |
| E32 | +11% count_accuracy | `EpisodeDecomposer` | 15 OrchestratorModel |
| E34V2 | +8% hit@1 | `HybridRanker` + `TransformerEmbeddingKnn` | 22 Context/ + 21 |
| E35 | +30% dependency_accuracy | `DependencyGraph` | 29 Planning/ |
| E38V2 | +100%, 86% load reduction | `KnnSkillFilter` → `ContextWindowManager` | 22 Context/ |
| E40V2 | +50% trust_accuracy | `TrustScorer` | 16 AutonomousLearning |

**Failed — do not integrate:**

| ID | Outcome |
|----|---------|
| E13 | 0% — methods too short to embed |
| E22 | +1% macro_F1 — 3 languages only, too small |
| E33 | 0% — trivially solvable data, both arms reach 1.00 |
| E36 | 0% — pattern matching too rigid |
| E47 | -8% — exact tag match beats BM25 here |

**Scale-up / blocked:**

| ID | Status |
|----|--------|
| E08TextSyntax | SpaCy Python dependency — cannot integrate without rewrite |
| E14 | Data too separable at Tier 3 — needs larger real corpus |
| E20 | Synthetic data 100% trivial — needs real corpus |

### Actual wiring state per experiment

| Experiment | Status | Problem |
|-----------|--------|---------|
| E18 | **Wired, KNN path dead** | `PromptDomainClassifier` constructs `DomainClassifierUnit` with `new()` → `_trained=false` always → always falls to `ClassifyByKeywords`. E18 +15% never reached. |
| E20 (project 29) | **Wired correctly** | `SemanticCompilerPipeline` Stage 2 runs project-29 `StackDetector` before E15 normalization — E20 Rule A satisfied. |
| E20 (project 21) | **Wired, different impl** | `LogicalExperimentOptimizations.CompressForTeaching()` uses `AgentInfrastructure.StackDetector` (separate class, same marker logic). Not the same as project-29 StackDetector — no E15 gap here because it runs before tokenization. |
| E23 | **Wired and active** | `UniversalNodeTransformations.BuildTemplate()` calls `Beautifier.Beautify(predicates)`. Real path, real instantiation. |
| E32 | **Wired via reflection** | `UniversalNineNodeDevelopmentFlow.TryDecompose()` loads `OrchestratorModel.EpisodeDecomposer` via `Activator.CreateInstance`. Silently returns null on failure. No test asserts the reflection path succeeds. |
| E34V2 (Path A) | **Wired, tokenizer gap** | `HybridRanker.Tokenize()` whitespace-splits, no E15 stemming. The proven +8% requires stemmed tokens. This path is active on every prompt. |
| E34V2 (Path B) | **Wired, tokenizer gap** | `TransformerEmbeddingKnnLogicalUnit.Tokenize()` same problem. This path fires after capability learning. |
| E35 | **Wired — but wrong version** | `DomainGraphClosure` (E50) handles sub-phase ordering in `AdaptiveLogicalOrchestrator`. But `SemanticCompiler29.DependencyGraph` (E35's production class) is **never called** — there is no reference to it anywhere in project 21. E35 +30% is unrealised. |
| E40V2 | **Wired via reflection** | `AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust()` loads `TrustScorer` via `Type.GetType()`. Silently returns `Probationary` on failure. No test asserts it. |
| E08, E09, E25, E26, E38V2 | **Not wired at all** | Production classes exist in projects 22 and 29. Zero references from project 21. |

---

## What needs fixing — six concrete gaps

**Gap 1 — E18 dead KNN path** (single-file fix, high value)
`PromptDomainClassifier._trained` is always false.

**Gap 2 — E34V2 tokenizer in HybridRanker (Path A)** (always-on path, highest value per effort)
`HybridRanker.Tokenize()` whitespace-only. Fires on every prompt.

**Gap 3 — E34V2 tokenizer in TransformerEmbeddingKnn (Path B)** (training path)
Same gap in the learned-capability path.

**Gap 4 — E25 PatternClassifier as third fusion component**
Pure C#, inlineable, proven +155%.

**Gap 5 — E32 and E40V2 reflection is untested** (reliability)
Silent catch-all means these may never fire in production.

**Gap 6 — E35 DependencyGraph not wired** (structural, medium scope)
`SemanticCompiler29.DependencyGraph` has no reference from project 21. E50 (`DomainGraphClosure`)
handles sub-phase-level ordering but operates on string IDs with pre-hardcoded transitions.
E35 adds learned ordering from predicate overlap. These complement each other.

**Not in scope (separate concerns):**
- E08/E09 (`RoleBasedTaskDecomposer`, `AstRelevanceRanker`): feed code-structure search, not prompt routing
- E26 (`ContextualDictionary`): Russian→English word disambiguation, not the English prompt path
- E38V2 (`KnnSkillFilter`): loads skills from disk for the agent loop, separate pipeline

---

## Phases

Floor before: **1311**

| Phase | Gap | Main file | New tests | New floor |
|-------|-----|-----------|-----------|-----------|
| 1 | E18 dead KNN path | `PromptDomainClassifier.cs` | 2 | 1313 |
| 2 | E34V2 tokenizer — HybridRanker (Path A) | `HybridRanker.cs` | 2 | 1315 |
| 3 | E34V2 tokenizer — TransformerEmbeddingKnn (Path B) | `LlmIngredientLogicalUnits.cs` | 2 | 1317 |
| 4 | E25 third fusion component — both paths | `HybridRanker.cs`, `LlmIngredientLogicalUnits.cs` | 3 | 1320 |
| 5 | E32 + E40V2 reflection coverage | `ReflectionWiringTests.cs` | 3 | 1323 |
| 6 | E35 DependencyGraph wiring | `AdaptiveLogicalOrchestrator.cs`, `UniversalNineNodeDevelopmentFlow.cs` | 3 | 1326 |

---

## Phase 1 — Fix E18 dead KNN path

### Root cause

`PromptDomainClassifier.cs` line 14:
```csharp
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = new();
```
`new()` → `_trained = false`. `ClassifyBaseDomain` guard:
```csharp
if (_trained && request.Predicates is { Length: > 0 } predicates)
```
Always skipped. Every call returns `ClassifyByKeywords` result.

### Fix

```csharp
// Replace
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = new();

// With
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = BuildSeededClassifier();

// Add
private static SemanticCompilerAdapter.DomainClassifierUnit BuildSeededClassifier()
{
    // Seed with one representative predicate profile per D-04 domain.
    // Predicate index affinity from RouteClassifier.PredicateDomainMap:
    //   NULL_CHECK(0)+CATCH_OP(1)+THROW_OP(2) → BUG
    //   AUTH_OP(11) → ARCH
    //   AGGREGATE_OP(9)+SEND_OP(10) → PLAN
    //   FILTER_OP(7)+CONVERT_OP(8) → REFACTOR
    //   VALIDATE_OP(3)+GET_OP(4) → CODE_Q
    var unit = new SemanticCompilerAdapter.DomainClassifierUnit();
    unit.Train([
        ([0, 1, 2],  SemanticCompiler29.Domain.BUG),
        ([11],       SemanticCompiler29.Domain.ARCH),
        ([9, 10],    SemanticCompiler29.Domain.PLAN),
        ([7, 8],     SemanticCompiler29.Domain.REFACTOR),
        ([3, 4],     SemanticCompiler29.Domain.CODE_Q),
    ]);
    return unit;
}
```

### Gate B

Before: `Classify(new DomainClassifierRequest("fix crash", Predicates: [0,1,2]))` returns evidence containing `"semantic_domain_keyword_fallback"`.

After: same call returns `Domain.BUG` and evidence does NOT contain `"semantic_domain_keyword_fallback"`.

### Tests — 2 new in `DomainClassifierKnnPathTests.cs`

**Test 1:** `DomainClassifierUnit_WhenTrained_KnnPathFires_ForPredicateInput`
Train a fresh `DomainClassifierUnit` with BUG examples. Call `Classify(predicates: [0,1,2])`. Assert domain = BUG, no keyword fallback evidence.

**Test 2:** `PromptDomainClassifier_StaticClassifier_IsSeeded_KnnPathReachable`
Call `PromptDomainClassifier.Classify("")` with explicit `Predicates: [0,1,2]` in request metadata. Assert `_trained` visible via non-fallback evidence in returned classification.

### Commit
```
phase 1: fix E18 — PromptDomainClassifier seeds DomainClassifierUnit at static init
Files: PromptDomainClassifier.cs, DomainClassifierKnnPathTests.cs
```

---

## Phase 2 — Fix E34V2 tokenizer in HybridRanker (Path A, always-on)

### Root cause

`HybridRanker.Tokenize()` in `22.AgentAlgorithms/Context/HybridRanker.cs`:
```csharp
private static string[] Tokenize(string text)
{
    return (text ?? string.Empty)
        .Split([' ', '\t', '\r', '\n', '-', '_', ...], StringSplitOptions.RemoveEmptyEntries)
        .Select(token => token.ToLowerInvariant())
        .Where(token => token.Length > 2)
        .ToArray();
}
```
No stemming. No stop-word removal. `HybridRanker` is called on every prompt in `MultiSubPhaseRouterUnit.BuildHybridScores()`. This is the highest-impact fix — it fires on every routing decision, not just after learning.

### Fix

Replace `Tokenize()` and add E15 statics in `HybridRanker.cs`:

```csharp
private static string[] Tokenize(string text)
{
    // E15-equivalent: stop-word removal + suffix stemming before BM25 scoring.
    // Proven +8% hit@1 (E34V2) requires stemmed tokens, not raw whitespace split.
    return System.Text.RegularExpressions.Regex
        .Split((text ?? string.Empty).ToLowerInvariant(), @"[^a-z]+")
        .Where(w => w.Length >= 2 && !E15Stopwords.Contains(w))
        .Select(E15Stem)
        .Where(w => w.Length >= 2)
        .ToArray();
}

private static readonly HashSet<string> E15Stopwords = new(StringComparer.OrdinalIgnoreCase)
{
    "a","an","the","of","in","on","at","by","for","with","about","from","to",
    "into","through","is","are","was","were","be","been","being",
    "have","has","had","do","does","did","can","could","will","would",
    "shall","should","may","might","must","need","used","let","get","got",
    "i","we","it","its","and","or","but","not","no","as","if","so",
    "then","else","this","that","which","who","when","where","how","why",
};

private static string E15Stem(string word)
{
    string[] suffixes = ["isation","ization","nesses","ments","tions","ings",
                         "ness","ment","tion","ing","ful","ous","ive","ed","er","ly"];
    foreach (var s in suffixes)
        if (word.Length > s.Length + 3
            && word.EndsWith(s, StringComparison.OrdinalIgnoreCase))
            return word[..^s.Length];
    return word;
}
```

### Gate B

Before: `HybridRanker.ScoreOne("implement authorization", "implementing the authorization pattern", 0.5)` treats "implementing" and "implement" as different tokens.

After: both stem to "implement" — `ScoreOne` returns strictly higher BM25 component than before.

Measurable assertion: call `ScoreOne("implement authorization", "implementing the authorization security pattern", 0.0)` with old and new tokenizer. New score must be strictly > old score (verifiable by comparing before/after).

### Tests — 2 new in `HybridRankerE15Tests.cs`

**Test 1:** `HybridRanker_E15Tokenizer_StemsAndRemovesStopWords_ImprovesStemDivergentMatch`
Build a `HybridRanker`. Score query "implement authorization" against document "implementing the authorization pattern". Assert `FusedScore >= 0.60` — higher than achievable with whitespace-split on stem-diverged pair.

**Test 2:** `HybridRanker_E15Stem_AuthorizationStemsToAuthoriz_ImplementingToImplement`
Call `HybridRanker.ScoreOne("implementing", "implement the code", 0.0)`. Assert score > 0 (stems now match). Also call `ScoreOne("the", "the a an in", 0.0)`. Assert score = 0 (all stop words removed, no match).

### Commit
```
phase 2: fix E34V2 — HybridRanker.Tokenize uses E15 stemming (Path A, always-on)
Files: HybridRanker.cs, HybridRankerE15Tests.cs
```

---

## Phase 3 — Fix E34V2 tokenizer in TransformerEmbeddingKnn (Path B)

### Root cause

Same whitespace-only `Tokenize()` in `LlmIngredientLogicalUnits.cs`. The `SemanticTeachingPromptCompiler` already compiles the query to canonical predicate form before it reaches `ScoreCandidates` (the A59 fix), so E15 stemming would apply consistently to both the compiled training text and the compiled query.

### Fix

Same E15 replacement as Phase 2, but in `LlmIngredientLogicalUnits.cs`. Share the stop-word list and stem function as private statics in `TransformerEmbeddingKnnLogicalUnit<TOutput>`.

```csharp
// Replace Tokenize() in LlmIngredientLogicalUnits.cs with identical E15 implementation.
// Add E15Stopwords and E15Stem as private static members — mirrored from Phase 2,
// no cross-file dependency needed.
```

### Gate B

Same as Phase 2 but applied to `TransformerEmbeddingKnnLogicalUnit`. After Phase 3:
`Train()` on document "implementing authorization" then query "implement auth" must match with fused score > 0.60.

### Tests — 2 new in `TransformerKnnTokenizerE15Tests.cs`

**Test 1:** `TransformerKnn_Tokenize_StemsAndRemovesStopWords`
Train a unit on "implementing the authorization pattern". Query "implement authorization". Assert selected candidate `FusedScore >= 0.60`.

**Test 2:** `TransformerKnn_E15Stem_ConsistentWithHybridRanker`
Assert `E15Stem("authorization")` returns "authoriz" and `E15Stem("implementing")` returns "implement" in both implementations (regression guard that the two copies stay in sync).

### Commit
```
phase 3: fix E34V2 — TransformerEmbeddingKnn.Tokenize uses E15 stemming (Path B)
Files: LlmIngredientLogicalUnits.cs, TransformerKnnTokenizerE15Tests.cs
```

---

## Phase 4 — Add E25 PatternClassifier as third fusion component (both paths)

### What adds

E25 `PatternClassifier` (+155%) classifies sentence word-order pattern (SVO / SOV / IMPERATIVE).
The production class is in `29.SemanticCompiler/Translation/PatternClassifier.cs`.
All logic (`InferPos`, `ActionWords`, `StructureWords`) is private static — inlineable.

Add to **both** `HybridRanker` (Path A) and `TransformerEmbeddingKnn` (Path B):
- A `E25PatternScore(string query, string document)` private static method
- Pattern helpers `E25ClassifyPattern`, `E25InferPos`, `E25ActionWords`, `E25StructureWords`
- Adjusted weights: vector×0.45 + bm25×0.35 + pos×0.20

For `HybridRanker`, which has no separate vector score, adjust to: bm25×0.65 + pos×0.35 (pos replaces part of bm25 weight since there is no vector component here — knnSeed is external).

Actually: `HybridRanker` fuses bm25 + external knnScore. The right adjustment:
bm25 weight: 0.45 (was 0.55), knn weight: 0.35 (was 0.45), pos weight: 0.20 (new).

```csharp
// HybridRanker constructor default parameters:
// Before: bm25Weight = 0.55, knnWeight = 0.45
// After:  bm25Weight = 0.45, knnWeight = 0.35  (pos adds 0.20)

// In Rank(): add posScore per candidate
var posScore = E25PatternScore(query, candidate.Text);
var fused = (_bm25Weight * normalizedBm25) + (_knnWeight * normalizedKnn) + (0.20 * posScore);
```

For `TransformerEmbeddingKnn`:
```csharp
// In ScoreCandidates():
var posScore = E25PatternScore(query, example.Text);
var fused = (vectorScore * 0.45) + (normalizedLexical * 0.35) + (posScore * 0.20);
```

### Gate B (both paths)

Before: `HybridRanker` manifest / `TransformerEmbeddingKnn.Manifest.IngredientIds` does NOT contain `"E25:PATTERN_CLASSIFIER"`.

After: both contain `"E25:PATTERN_CLASSIFIER"`. IMPERATIVE query scores strictly higher on IMPERATIVE document than on SVO document when all other signals are equal.

### Tests — 3 new in `PatternFusionTests.cs`

**Test 1:** `HybridRanker_E25_ImperativeQueryBoostsImperativeDocument`
Rank two equal-length documents: "fix the crash in auth" (IMPERATIVE) vs "this function returns null" (SVO). Query: "fix the authorization bug". Assert IMPERATIVE document wins.

**Test 2:** `TransformerKnn_E25_Manifest_ContainsPatternClassifier`
Train a `TransformerEmbeddingKnnLogicalUnit`. Assert `Manifest.IngredientIds` contains `"E25:PATTERN_CLASSIFIER"`.

**Test 3:** `E25PatternScore_SamePattern1_DifferentHalf_UnknownZero`
Direct test of `E25PatternScore`: SVO vs SVO → 1.0; IMPERATIVE vs SVO → 0.5; "??" vs SVO → 0.0.

### Commit
```
phase 4: add E25 PatternClassifier as third fusion component in HybridRanker and TransformerEmbeddingKnn
Files: HybridRanker.cs, LlmIngredientLogicalUnits.cs, PatternFusionTests.cs
```

---

## Phase 5 — Add coverage tests for E32 and E40V2 reflection paths

### Root cause

`TryDecompose()` and `ScoreCapabilityTrust()` both use `Type.GetType()` / `Activator.CreateInstance` and swallow all exceptions. If the assembly is not loaded, neither method gives any signal that it failed.

No existing test exercises the happy path through reflection to verify it returns a real result.

### Tests — 3 new in `ReflectionWiringTests.cs`

**Test 1:** `TryDecompose_BugPrompt_ReturnsNonNullResult`
Call `UniversalNineNodeDevelopmentFlow.TryDecompose("fix the null reference in auth flow", ["BUG", "NULL_CHECK"])`. Assert result is not null and Length >= 1. Failure = reflection path broken.

**Test 2:** `TrustScorer_DirectInstantiation_HighConfidenceRequest_ScoredHigh`
Directly instantiate `AutonomousLearning.TrustScorer`. Pass `SourceFeatures(isInternal: true, hasTests: true, domainMatch: true, ...)`. Assert returned level is `"High"`. Proves the production class works as expected before the reflection call reaches it.

**Test 3:** `ScoreCapabilityTrust_WithTestingPredicates_ReturnsTrustedOrProbationary`
Build a minimal `LogicalCapabilityTrainingRequest` with domain BUG and one branch plan containing predicate "TEST". Call `AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(...)`. Assert result is `Trusted` or `Probationary` (never throws). Documents current behaviour and catches future breakage.

### Commit
```
phase 5: add reflection coverage tests for E32 EpisodeDecomposer and E40V2 TrustScorer wiring
Files: ReflectionWiringTests.cs
```

---

## Phase 6 — Wire E35 DependencyGraph into the orchestration path

### Root cause

`SemanticCompiler29.DependencyGraph` has no reference in project 21. It exists in
`29.SemanticCompiler/Planning/DependencyGraph.cs` and detects whether two domains are
sequential (A produces what B consumes) or parallel.

`AdaptiveLogicalOrchestrator` currently uses `DomainGraphClosure` (E50) for sub-phase ordering,
but `DomainGraphClosure` operates on pre-hardcoded string transitions from
`BuildDynamicDomainTransitions()`. E35's `DependencyGraph` adds learned ordering based on
predicate overlap — it checks whether predicates from branch A appear as inputs in branch B,
indicating B depends on A.

The integration point: in `AdaptiveLogicalOrchestrator.ExecuteMultiBranchAsync()`, after routing
decisions are produced but before branches are sequenced, check E35 predicate overlap to flag
pairs that can run in parallel. This does not replace E50 — it adds learned signal on top.

### Fix

Add `TryGetDependencyOrdering(IReadOnlyList<LogicalFlowResult> branchResults)` method in
`AdaptiveLogicalOrchestrator` that calls `SemanticCompiler29.DependencyGraph` via a thin
adapter. The adapter needs `ICompilerStorage` and `ICompilerEventLogger` — instantiate them
with the already-used `InMemoryCompilerStorage` / `NoopCompilerEventLogger` pattern from
`DevelopmentPromptCompiler`.

Add `using SemanticCompiler29;` import to `AdaptiveLogicalOrchestrator.cs`.

```csharp
// New private method in AdaptiveLogicalOrchestrator:
private static IReadOnlyDictionary<string, bool> TryGetParallelFlags(
    IReadOnlyList<SubPhaseRoutingDecision> decisions,
    IReadOnlyList<string[]> predicateSets)
{
    // Use E35 DependencyGraph to detect which decision pairs can run in parallel.
    // A pair is parallel if neither A's predicates appear in B's inputs nor vice versa.
    var storage = new DevelopmentPromptCompiler().CompilerStorage; // reuse existing pattern
    var graph = new SemanticCompiler29.DependencyGraph(
        new InMemorySemanticStorage(), new NoopSemanticLogger());
    // ... (build Domain[] input from decisions, call graph.Order(), return parallel map)
}
```

### Gate B

Before: `AdaptiveLogicalOrchestrator` has no reference to `SemanticCompiler29.DependencyGraph`.

After: `AdaptiveLogicalOrchestrator` references `SemanticCompiler29.DependencyGraph`. A test with two routing decisions (PLAN + ARCH, which E35 knows is sequential) returns the correct ordering. A test with (CODE_Q + BUG) — which E35 knows CODE_Q must precede BUG — returns the correct order.

### Tests — 3 new in `DependencyGraphWiringTests.cs`

**Test 1:** `DependencyGraph_PlanBeforeArch_DetectedAsSequential`
Instantiate `SemanticCompiler29.DependencyGraph`. Pass PLAN → ARCH domain pair. Assert PLAN is marked as prerequisite for ARCH (as per E35 default edges: PLAN→ARCH→CODE_Q).

**Test 2:** `DependencyGraph_CodeQBeforeBug_DetectedAsSequential`
Pass CODE_Q + BUG domain pair. Assert CODE_Q is prerequisite for BUG (understand code before debugging it).

**Test 3:** `AdaptiveOrchestrator_MultiDomainBranches_UseDependencyOrdering`
Run a 2-branch orchestration with predicates from PLAN domain and CODE_Q domain. Assert the execution order reflects E35 dependency (PLAN executes before CODE_Q routing starts).

### Commit
```
phase 6: wire E35 DependencyGraph into AdaptiveLogicalOrchestrator multi-branch ordering
Files: AdaptiveLogicalOrchestrator.cs, UniversalNineNodeDevelopmentFlow.cs, DependencyGraphWiringTests.cs
```

---

## Floor summary

| After phase | Tests |
|-------------|-------|
| Before (latest zip) | 1311 |
| Phase 1 | 1313 |
| Phase 2 | 1315 |
| Phase 3 | 1317 |
| Phase 4 | 1320 |
| Phase 5 | 1323 |
| Phase 6 | 1326 |

---

## Decisions locked (A61–A66)

**A61:** `DomainClassifierUnit` in `PromptDomainClassifier` must be pre-seeded at static construction. `new()` leaves `_trained=false` permanently — E18 +15% is unreachable without this fix.

**A62:** `HybridRanker.Tokenize()` must use E15-equivalent stop-word removal and suffix stemming. This is the always-on routing path — every prompt uses it. Whitespace-split leaves E34V2 +8% unrealised on every routing decision.

**A63:** `TransformerEmbeddingKnnLogicalUnit.Tokenize()` must use the same E15-equivalent logic. This is the learning path. Both tokenizers must be kept in sync — same stop-word list, same stemmer.

**A64:** Both `HybridRanker` and `TransformerEmbeddingKnnLogicalUnit` add E25 `PatternClassifier` (+155%) as a third fusion component. Weights: bm25×0.45 + knn×0.35 + pos×0.20 (HybridRanker); vector×0.45 + bm25×0.35 + pos×0.20 (TransformerEmbeddingKnn). Initial weights pending batch-100 calibration.

**A65:** Reflection wiring for E32 and E40V2 must have direct coverage tests that assert the happy path succeeds — not just that exceptions are swallowed. Silent `catch { return safe_default }` without a green test means the +11% and +50% gains may never apply.

**A66:** `SemanticCompiler29.DependencyGraph` (E35, +30%) must be wired into `AdaptiveLogicalOrchestrator` multi-branch ordering. `DomainGraphClosure` (E50) handles sub-phase-level topology; E35 adds learned predicate-overlap dependency detection on top. These are complementary.

---

# Last updated: 2026-05-07
# Floor before: 1311 | Floor after: 1326 (+15 tests across 6 phases)
# Key corrections from v2: identified second BM25 path (HybridRanker, Path A — always-on);
#   HybridRanker.Tokenize has the same whitespace-split gap; E20 has two separate impls
#   both wired correctly; E35 completely unwired (DomainGraphClosure ≠ DependencyGraph);
#   Phase 2 is HybridRanker (higher impact than TransformerEmbeddingKnn);
#   E34V2 gap applies to both paths independently; decisions A61-A66
