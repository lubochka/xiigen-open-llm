# PLAN-KNN-MATRIX-INTEGRATION-v2

## Source of truth

This plan is based on direct reading of:
- All 20 KNN-matrix experiment files in `35.ExperimentsV2/Experiments/`
- `docs/EXPERIMENT-RESULTS-MAP.md` — authoritative pass/fail/scale-up status for all 59 runs
- Every production class in projects 12, 15, 21, 22, 23, 29 that the experiments target
- The actual wiring state in `21.AgentInfrastructure/Logical/` — what calls what

The `experiment_knn_state.json` produced last session contained several errors that this plan corrects.

---

## Actual state: which experiments passed, which failed, which are already wired

### What the results map says (authoritative)

Of the 20 experiments in the JSON, the actual outcomes per `EXPERIMENT-RESULTS-MAP.md` are:

**Passed and relevant to the KNN matrix:**

| Experiment | Proven delta | Production class | Location |
|-----------|-------------|-----------------|----------|
| E08 (NonCode) | +56% top1_accuracy | `RoleBasedTaskDecomposer` | 22.AgentAlgorithms |
| E09 | +45% top1_accuracy | `KnnSkillFilter` / `AstRelevanceRanker` | 22.AgentAlgorithms |
| E18 | +15% top1_accuracy, 95% domain | `RouteClassifier` → `DomainClassifierUnit` | 23.SemanticCompilerAdapter |
| E23 | +833% template_match_rate | `TemplateBeautifier` | 29.SemanticCompiler |
| E25 | +155% top1_accuracy | `PatternClassifier` | 29.SemanticCompiler |
| E26 | +150% disambiguation_accuracy | `ContextualDictionary` | 29.SemanticCompiler |
| E32 | +11% count_accuracy | `EpisodeDecomposer` | 15.OrchestratorModel |
| E34V2 | +8% hit@1 | `TransformerEmbeddingKnnLogicalUnit` | 21.AgentInfrastructure |
| E35 | +30% dependency_accuracy | `DependencyGraph` | 22.AgentAlgorithms |
| E38V2 | +100%, 86% load reduction | `ContextWindowManager` | 22.AgentAlgorithms |
| E40V2 | +50% trust_accuracy | `TrustScorer` | 16.AutonomousLearning |

**Failed — do NOT integrate:**

| Experiment | Outcome | Why |
|-----------|---------|-----|
| E13 | 0% | Methods too short to embed meaningfully |
| E22 | +1% macro_F1 | Only 3 languages at Tier 3 — too small to matter |
| E33 | 0% | Trivially solvable data — both arms reach 1.00 |
| E36 | 0% | Pattern matching too rigid |
| E47 | -8% | Exact tag match beats BM25 on overlapping queries |

**Scale-up required — not ready for integration:**

| Experiment | Why |
|-----------|-----|
| E08TextSyntax | SpaCy Python dependency — blocks integration |
| E20 | Synthetic data 100% trivial at Tier 3 — needs real corpus |
| E14 | Data too separable at Tier 3 |

### Current actual wiring in project 21

| Experiment | Status | Problem |
|-----------|--------|---------|
| E18 | Wired but dead | `DomainClassifierUnit._trained = false` always — `PromptDomainClassifier` constructs with `new()`, `Train()` never called |
| E20 | Wired — two paths | (1) Inside `SemanticCompilerPipeline` Stage 2 (project 29 `StackDetector`) — active via `DevelopmentPromptCompiler`. (2) `LogicalExperimentOptimizations.CompressForTeaching()` uses `AgentInfrastructure.StackDetector` — separate class, same marker logic. Both fire correctly. No gap. |
| E23 | Wired and active | `UniversalNodeTransformations.BuildTemplate()` calls `Beautifier.Beautify(predicates)` — real path, instantiated with `UniversalCompilerStorage` + logger |
| E32 | Wired via reflection | `UniversalNineNodeDevelopmentFlow.TryDecompose()` loads `OrchestratorModel.EpisodeDecomposer` via `Activator.CreateInstance`. Fragile — silently returns null if assembly not loaded |
| E34V2 | Wired but partial | Fusion implemented (`vectorScore×0.55 + normalizedLexical×0.45`). Gap: `Tokenize()` uses whitespace split, not E15 stemmed tokens. The proven +8% requires E15 tokens. |
| E40V2 | Wired via reflection | `AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust()` loads `TrustScorer` via `Type.GetType()`. Fragile — returns `Probationary` on any exception |
| E08, E09, E25, E26, E35, E38V2 | Not wired | No reference in project 21 — production classes exist in projects 22 and 29, no call path from project 21 |

---

## Gap taxonomy

After eliminating the failed experiments and the already-correctly-wired ones, six real gaps remain:

**Gap 1 — E18 KNN path dead** (high impact, single-file fix)
`_trained = false` always. Fix: pre-seed the static `DomainClassifierUnit`.

**Gap 2 — E34V2 tokenizer not E15** (medium impact, single-method replacement)
`Tokenize()` uses whitespace split. Proven +8% requires stemmed tokens.

**Gap 3 — E25 PatternClassifier not in fusion** (medium impact, new component)
Pure C#, already in project 29, can be mirrored inline. Third fusion component.

**Gap 4 — E32 reflection wiring is fragile** (reliability, not accuracy)
`TryDecompose()` silently returns null on assembly load failure. No test coverage for the failure path.

**Gap 5 — E40V2 reflection wiring is fragile** (reliability, not accuracy)
`ScoreCapabilityTrust()` catches all exceptions and returns `Probationary`. Same pattern.

**Gap 6 — E35 DependencyGraph not wired at all** (high impact — affects parallel scheduling)
`DependencyGraph` (+30% dependency accuracy) sits in project 22 with no reference from project 21.
This is the biggest structural gap: multi-branch scheduling currently has no dependency ordering
beyond what `DomainGraphClosure` (E50) provides. E35 would let the orchestrator detect which
sub-phases can run in parallel vs must run sequentially.

**Not gaps (JSON was wrong about these):**
- E22: failed experiment, do not integrate
- E33: failed experiment, do not integrate
- E36: failed experiment, do not integrate
- E47: failed experiment, do not integrate — exact tag match beats BM25 here
- E13: failed experiment, do not integrate
- E08NonCode/E09: PASSED but their role is code-structure analysis (C# AST, Roslyn), not developer-prompt routing. They feed `AstRelevanceRanker` for code search, not `ScoreCandidates` for prompt matching. Out of scope for this plan.
- E26 ContextualDictionary: for Russian-to-English translation disambiguation — not applicable to the current English-only prompt path.
- E38V2 SkillRetriever: feeds `ContextWindowManager` for skill selection, separate concern from the KNN scoring matrix.

---

## Scope of this plan

Four phases. One commit each. Floor before: **1311**.

| Phase | Gap | Files touched | New tests | New floor |
|-------|-----|--------------|-----------|-----------|
| 1 | E18 dead KNN path | `PromptDomainClassifier.cs` | 2 | 1313 |
| 2 | E34V2 tokenizer | `LlmIngredientLogicalUnits.cs` | 2 | 1315 |
| 3 | E25 third fusion component | `LlmIngredientLogicalUnits.cs` | 3 | 1318 |
| 4 | E32 + E40V2 reflection fragility | `UniversalNineNodeDevelopmentFlow.cs`, `LogicalExperimentOptimizations.cs` | 3 | 1321 |

E35 wiring is deferred — it requires a new `DependencyScheduler` integration point in `AdaptiveLogicalOrchestrator` and belongs in `PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4` Phase 3.

---

## Phase 1 — Fix the E18 dead KNN path

### Root cause (confirmed in code)

`PromptDomainClassifier.cs` line 14:
```csharp
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = new();
```

`new()` calls `DomainClassifierUnit(classifier: null)` which sets `_classifier = new SemanticCompiler.RouteClassifier()` and leaves `_trained = false`.

`ClassifyBaseDomain` checks:
```csharp
if (_trained && request.Predicates is { Length: > 0 } predicates)
```

Since `_trained` is always false, every call falls to `RouteClassifier.ClassifyByKeywords()`. The proven +15% from E18 is permanently bypassed.

### Fix

Replace the static field with a factory that seeds the classifier with one representative predicate profile per domain. `DevelopmentPromptCompiler.Compile()` produces `PipelinePredicates` as `string[]` — we need `int[]` for `RouteClassifier.Train()`. The `SemanticCompilerPipeline` assigns predicate IDs sequentially. The canonical predicate vocabulary from `LogicExtractor` has 22 entries. The seeding maps the first few known predicate IDs to domains.

The fix constructs five synthetic training examples from the canonical 22-predicate vocabulary, one per D-04 domain, using the same predicate-to-domain affinity already hardcoded in `RouteClassifier.PredicateDomainMap`:

```csharp
// Before
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = new();

// After
private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = BuildSeededClassifier();

private static SemanticCompilerAdapter.DomainClassifierUnit BuildSeededClassifier()
{
    // Seed with one representative predicate profile per D-04 domain.
    // Predicate IDs match RouteClassifier.PredicateDomainMap affinity:
    //   NULL_CHECK(0) + CATCH_OP(1) + THROW_OP(2) → BUG
    //   AUTH_OP(11) → ARCH
    //   AGGREGATE_OP(9) + SEND_OP(10) → PLAN
    //   FILTER_OP(7) + CONVERT_OP(8) → REFACTOR
    //   VALIDATE_OP(3) + GET_OP(4) → CODE_Q
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

Before: `PromptDomainClassifier.Classify("fix the null reference")` always returns evidence containing `"semantic_domain_keyword_fallback"`.

After: `DomainClassifierUnit._trained == true` (observable: `Classify` called with `Predicates: [0, 1]` returns `Domain.BUG` with evidence NOT containing only `"semantic_domain_keyword_fallback"`).

Measurable assertion: call `Classify(new DomainClassifierRequest("fix crash", Predicates: [0, 1, 2]))` — result domain must be `BUG` and evidence must not contain `"semantic_domain_keyword_fallback"`.

### Tests — 2 new in `DomainClassifierKnnPathTests.cs`

**Test 1:** `DomainClassifierUnit_WhenTrained_KnnPathReachableForPredicateInput`
Directly train a `DomainClassifierUnit` with BUG predicates. Call `Classify(new DomainClassifierRequest("", Predicates: [0,1,2]))`. Assert domain is `BUG` and evidence does not contain `"semantic_domain_keyword_fallback"`.

**Test 2:** `PromptDomainClassifier_StaticClassifier_IsPreSeededAtInit`
Call `PromptDomainClassifier.Classify("", metadata: null)`. Would return UNKNOWN from keyword path with confidence 0.20. Then call with a prompt that carries matching predicates via metadata. Assert that a pre-seeded instance does not always return keyword fallback.

### Commit
```
phase 1: fix E18 dead KNN path — PromptDomainClassifier pre-seeds DomainClassifierUnit at static init
Files: PromptDomainClassifier.cs, DomainClassifierKnnPathTests.cs
```

---

## Phase 2 — Replace BM25 whitespace tokenizer with E15 stemmed tokens

### Root cause (confirmed in code)

`LlmIngredientLogicalUnits.cs` `Tokenize()` method splits on `[' ', '\t', '\r', '\n', '.', ',', ...]` and lowercases only. E34V2 proved +8% hit@1 requires E15-normalised tokens from `SemanticNormalizer.Normalize()` — stop-word removal and suffix stemming.

The `SemanticNormalizer` lives in project 29 and requires `ICompilerStorage + ICompilerEventLogger` through `BaseService`. Adding a project 29 reference to `LlmIngredientLogicalUnits.cs` in project 21 would couple the learned-unit infrastructure to the full semantic compiler stack, violating Rule 2 (Fabric First). The fix inlines the stemming and stop-word logic as private statics — the same approach used by `SemanticTeachingPromptCompiler`.

### Fix

Replace the `Tokenize()` method body and add two private static members mirroring E15:

```csharp
// Before: whitespace-only split
private static Dictionary<string, int> Tokenize(string text)
{
    var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var token in (text ?? string.Empty)
        .Split([' ', '\t', '\r', '\n', '.', ...], StringSplitOptions.RemoveEmptyEntries | ...)
        .Select(token => token.ToLowerInvariant())
        .Where(token => token.Length > 1))
    {
        tokens[token] = tokens.TryGetValue(token, out var count) ? count + 1 : 1;
    }
    return tokens;
}

// After: E15-equivalent — stop-word removal + suffix stemming, no cross-project dependency
private static Dictionary<string, int> Tokenize(string text)
{
    var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var word in System.Text.RegularExpressions.Regex
        .Split((text ?? string.Empty).ToLowerInvariant(), @"[^a-z]+")
        .Where(w => w.Length >= 2 && !E15Stopwords.Contains(w))
        .Select(E15Stem)
        .Where(w => w.Length >= 2))
    {
        tokens[word] = tokens.TryGetValue(word, out var count) ? count + 1 : 1;
    }
    return tokens;
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

Before: `Tokenize("authorization implementing")` returns `{"authorization": 1, "implementing": 1}`.

After: `Tokenize("authorization implementing")` returns `{"authoriz": 1, "implement": 1}` — both stemmed.
Also: `Tokenize("the fix for this")` returns `{"fix": 1}` — stop words removed.

Measurable assertion: for a training document containing "implementing the authorization pattern" and a query "implement authorization", the lexical score after E15 tokenization must be strictly greater than before, because both stems now match.

### Tests — 2 new in `TransformerKnnTokenizerTests.cs`

**Test 1:** `Tokenize_E15_StemsAndRemovesStopWords`
Test `Tokenize` (exposed as `internal` for testing) on "authorization implementing the pattern". Assert result contains `"authoriz"` and `"implement"` and does NOT contain `"the"`, `"authorization"` (unstemmed), or `"implementing"` (unstemmed).

**Test 2:** `ScoreCandidates_E15Tokenizer_MatchesStemmedQueryToStemmedDocument`
Train a unit on one document: "implementing the authorization security pattern". Query with "implement authorization". Assert the selected candidate's `FusedScore >= 0.65` — higher than achievable with whitespace-only tokenization on this stem-diverged pair.

### Commit
```
phase 2: replace BM25 whitespace tokenizer with E15 stemmed tokens in ScoreCandidates
Files: LlmIngredientLogicalUnits.cs, TransformerKnnTokenizerTests.cs
```

---

## Phase 3 — Add E25 PatternClassifier as third fusion component

### Root cause

`ScoreCandidates` computes only two signals. E25 `PatternClassifier` (+155% top1_accuracy) proved that POS-pattern sequence (SVO / SOV / IMPERATIVE) is a strong structural signal for natural language routing. The production class is in `29.SemanticCompiler/Translation/PatternClassifier.cs` and is pure C# — no SpaCy, no external dependency.

`PatternClassifier.Classify(string[] tokens)` returns a pattern string. Its only logic is:
- `InferPos(token)` via two static `HashSet<string>` (ActionWords, StructureWords)  
- First-verb / first-noun position arithmetic

All of this can be inlined in `LlmIngredientLogicalUnits.cs` as private statics, same as Phase 2.

The `posScore` between query and document: 1.0 if patterns match, 0.5 if both are non-UNKNOWN but differ, 0.0 if either is UNKNOWN. New weights: vector×0.45 + bm25×0.35 + pos×0.20.

### Fix

Add `E25PatternScore`, `E25ClassifyPattern`, `E25InferPos`, `E25ActionWords`, `E25StructureWords` as private statics.
Update `ScoreCandidates` fusion line.
Add `"E25:PATTERN_CLASSIFIER"` to manifest `IngredientIds`.

```csharp
// Updated fused score in ScoreCandidates:
var posScore = E25PatternScore(query, example.Text);
var fused = (vectorScore * 0.45) + (normalizedLexical * 0.35) + (posScore * 0.20);

// New helpers:
private static double E25PatternScore(string query, string document)
{
    var qp = E25ClassifyPattern(query);
    var dp = E25ClassifyPattern(document);
    if (qp == "UNKNOWN" || dp == "UNKNOWN") return 0.0;
    return qp == dp ? 1.0 : 0.5;
}

private static string E25ClassifyPattern(string text)
{
    var tokens = (text ?? string.Empty)
        .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
    if (tokens.Length == 0) return "UNKNOWN";
    var tags = tokens.Select(E25InferPos).ToArray();
    int fv = Array.IndexOf(tags, 8);  // first VERB
    int fn = tags.Select((t, i) => (t, i)).FirstOrDefault(x => x.t == 7 || x.t == 3).i;
    if (fv < 0) return "SVO";
    bool verbFirst   = fv == 0 || (fv < fn && fn > 0);
    bool nounBefore  = fn >= 0 && fn < fv;
    bool verbLast    = fv == tags.Length - 1;
    if (verbFirst && !nounBefore) return "IMPERATIVE";
    if (verbLast  && nounBefore)  return "SOV";
    return "SVO";
}

private static int E25InferPos(string token)
{
    string lower = token.ToLowerInvariant();
    if (E25ActionWords.Contains(lower))    return 8; // VERB
    if (E25StructureWords.Contains(lower)) return 3; // PRON/ARTICLE
    return 7; // NOUN
}

private static readonly HashSet<string> E25ActionWords = new(StringComparer.OrdinalIgnoreCase)
{
    "add","fix","create","update","delete","validate","check","return","throw","send",
    "log","get","fetch","process","handle","run","build","set","remove","find","refactor",
};

private static readonly HashSet<string> E25StructureWords = new(StringComparer.OrdinalIgnoreCase)
{
    "a","an","the","in","on","at","by","for","with","and","or","but","not",
    "this","that","it","they","we","you","which","who","where","when","how",
};
```

### Gate B

Before: manifest `IngredientIds` does NOT contain `"E25:PATTERN_CLASSIFIER"`. Weights are 0.55 / 0.45.

After: manifest `IngredientIds` contains `"E25:PATTERN_CLASSIFIER"`. For two training documents where one is IMPERATIVE ("fix this null reference") and one is SVO ("this function returns null"), a query "fix the crash" (IMPERATIVE) must select the IMPERATIVE document.

### Tests — 3 new in `TransformerKnnPatternFusionTests.cs`

**Test 1:** `Manifest_ContainsE25PatternClassifier_AfterTraining`
Train a `TransformerEmbeddingKnnLogicalUnit`. Assert `Manifest.IngredientIds` contains `"E25:PATTERN_CLASSIFIER"`.

**Test 2:** `PatternScore_SamePattern_ReturnsOne_DifferentReturnsHalf_UnknownReturnsZero`
Unit-test `E25PatternScore` (internal visible): IMPERATIVE vs IMPERATIVE → 1.0; SVO vs SOV → 0.5; "???" vs SVO → 0.0.

**Test 3:** `ScoreCandidates_ImperativeQuery_SelectsImperativeDocument_OverSvoDocument`
Train on two examples with equal-length, equal-word-overlap text but different patterns. Imperative query must select the imperative document.

### Commit
```
phase 3: add E25 PatternClassifier as third fusion component in ScoreCandidates
Files: LlmIngredientLogicalUnits.cs, TransformerKnnPatternFusionTests.cs
```

---

## Phase 4 — Fix fragile reflection wiring for E32 and E40V2

### Root cause

Both E32 (`EpisodeDecomposer`) and E40V2 (`TrustScorer`) are wired via `Type.GetType()` / `Activator.CreateInstance` reflection. Both silently swallow all exceptions and return a safe default (null / Probationary). This means:

- If the assembly is not loaded, or the type name changes, or a constructor parameter mismatches — the experiment never fires and no test catches it.
- There is no test that asserts the reflection path actually succeeded vs fell back.

This is a reliability gap, not an accuracy gap. The proven gains (+11% from E32, +50% from E40V2) may never actually apply in production because the silent fallback masks the failure.

### Fix for E32 (`UniversalNineNodeDevelopmentFlow.cs`)

Add a non-reflection direct reference to `OrchestratorModel.EpisodeDecomposer`. The project already references project 15 (`OrchestratorModel`) via the existing assembly, but does not have a `using` import. Verify the project reference exists in the csproj. If it does: replace the reflection call with a direct typed call.

If the direct reference creates an unwanted coupling, the minimum fix is: add one test that verifies `TryDecompose` actually returns a non-null result for a known prompt — proving the reflection path succeeded on this machine. The test failure would immediately surface an assembly load problem.

### Fix for E40V2 (`LogicalExperimentOptimizations.cs`)

Same approach: if `AutonomousLearning` project is already referenced in the csproj, replace `Type.GetType("AutonomousLearning.TrustScorer, AutonomousLearning")` with a direct typed call to `TrustScorer`. If not, add a coverage test.

### Gate B

Before: no test asserts that `TryDecompose("fix the null reference in auth flow", ["BUG"])` returns non-null.

After: test asserts a concrete non-null return from `TryDecompose` with a real prompt. If the reflection path is broken on the test machine, this test fails with a clear message rather than silently returning null.

### Tests — 3 new in `ReflectionWiringTests.cs`

**Test 1:** `TryDecompose_WithBugPrompt_ReturnsNonNullSubEpisodes`
Call `UniversalNineNodeDevelopmentFlow.TryDecompose("fix the null reference in the auth flow", ["BUG", "NULL_CHECK"])`. Assert result is not null and Length >= 1.

**Test 2:** `ScoreCapabilityTrust_WithTrainingRequest_ReturnsTrustedOrProbationary`
Build a minimal `LogicalCapabilityTrainingRequest` with domain BUG and one branch plan containing predicate "TEST". Call `AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(...)`. Assert it returns either `Trusted` or `Probationary` — NOT a thrown exception. (If reflection fails, the try-catch returns Probationary silently, test still passes — this test only catches hard breaks.)

**Test 3:** `TrustScorer_DirectInstantiation_ScoresHighConfidenceRequest`
Directly instantiate `AutonomousLearning.TrustScorer` (bypassing reflection). Pass features with `isInternal=true`, `hasTests=true`, `domainMatch=true`. Assert result level is `"High"`. This proves the production class works correctly and the reflection wiring connects to a correctly-behaving class.

### Commit
```
phase 4: add coverage tests for E32 and E40V2 reflection wiring paths
Files: UniversalNineNodeDevelopmentFlow.cs (or test only), ReflectionWiringTests.cs
```

---

## Floor summary

| State | Tests |
|-------|-------|
| Before this plan (latest zip) | 1311 |
| After Phase 1 | 1313 |
| After Phase 2 | 1315 |
| After Phase 3 | 1318 |
| After Phase 4 | 1321 |

---

## What this plan explicitly does NOT cover

**E35 DependencyGraph** (+30% dependency_accuracy) — not wired at all. This requires a new integration point in `AdaptiveLogicalOrchestrator` to use dependency ordering when scheduling multi-branch execution. Belongs in `PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4` Phase 3.

**E08 NonCode / E09** — passed, but their target is C# AST code search (Roslyn-backed), not developer prompt routing. They feed `AstRelevanceRanker` in a different pipeline.

**E38V2 SkillRetriever** (+100%) — feeds `ContextWindowManager` for skill loading from disk. Separate concern from KNN scoring. Out of scope.

**E26 ContextualDictionary** (+150%) — Russian-to-English translation disambiguation. Not applicable to the English-only prompt routing path.

**Failed experiments (E13, E22, E33, E36, E47)** — do not integrate. The results map is the authority.

---

## Decisions locked by this plan (A61–A64)

**A61:** `DomainClassifierUnit` in `PromptDomainClassifier` must be pre-seeded at static construction. A `new()` instance has `_trained=false` permanently, making the proven E18 +15% KNN path unreachable. Fix: `BuildSeededClassifier()` with five canonical domain predicate sets.

**A62:** `Tokenize()` in `TransformerEmbeddingKnnLogicalUnit.ScoreCandidates` must apply E15-equivalent stop-word removal and suffix stemming. Whitespace-only split loses the proven E34V2 +8% hit@1. Fix: inline `E15Stopwords` + `E15Stem` private statics.

**A63:** `ScoreCandidates` fuses three signals: vector (0.45), BM25-E15 (0.35), POS pattern (0.20). `PatternClassifier` E25 (+155%) provides the pattern score via inlined private statics. Initial weights are defaults pending batch-100 calibration.

**A64:** Reflection wiring for E32 and E40V2 must have direct test coverage asserting the reflection path succeeds. Silent `catch { return default }` patterns are not acceptable — they mask assembly load failures and make the +11% and +50% gains untestable.

---

# Last updated: 2026-05-07
# Floor before: 1311 | Floor after: 1321 (+10 tests)
# Corrections from v1: 5 failed experiments removed; E20 confirmed correctly wired (two paths);
#   E23 confirmed active; E32 and E40V2 identified as reflection-fragile; E35 deferred;
#   E08/E09/E26/E38V2 classified as separate concerns; decisions A61-A64 added
