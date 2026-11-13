# PLAN-KNN-MATRIX-INTEGRATION-v1

## What this plan fixes

The KNN scoring matrix in `ScoreCandidates` (file: `21.AgentInfrastructure/Logical/LlmIngredientLogicalUnits.cs`) currently uses two
signals: transformer embedding vector (weight 0.55) and a BM25-style lexical score (weight 0.45). The experiment state file
(`experiment_knn_state.json`) documents three gaps that prevent the proven accuracy gains from reaching the production path:

**Gap 1 — E18 KNN path is dead (A61).** `PromptDomainClassifier` constructs `DomainClassifierUnit` with `new()`, which means
`_trained = false` always. The `ClassifyBaseDomain` method falls straight to `ClassifyByKeywords` and never reaches the
trained KNN route. The proven +15.2% accuracy from E18 is unreachable in the current code.

**Gap 2 — BM25 tokenizer uses whitespace split, not E15 stemmed tokens (E34V2 partial).** The `Tokenize()` method in
`ScoreCandidates` splits on whitespace and punctuation. E34V2 proved +8% hit@1 with E15 stemmed tokens from
`SemanticNormalizer.Normalize()`. That gain is currently left unrealised.

**Gap 3 — No third fusion component (E25 pending).** The experiment state proposes adding `PatternClassifier.Classify()`
as a third component (posScore) at proposed weights of vector×0.45 + bm25×0.35 + pos×0.20. `PatternClassifier` (+154.6%)
already exists in `29.SemanticCompiler/Translation/PatternClassifier.cs` and is pure C# with no SpaCy dependency. Its
`Classify()` method depends on `ICompilerStorage` and `ICompilerEventLogger` through `BaseService`, but all its logic
(`InferPos`, `ActionWords`, `StructureWords`) is in private static members — making the pattern score
extractable as a static helper without instantiating the full service.

Additionally: the query fed into `ScoreCandidates` is already compiled through `SemanticTeachingPromptCompiler` at
inference time (the A59 fix), which means `queryText` arriving in `ScoreCandidates` is the canonical predicate form —
the E15-normalised form would be applied consistently to training and query.

---

## Scope

Three phases. Each phase is one commit. Test floor before this plan: **1311**.

| Phase | What changes | Files touched | New tests | New floor |
|-------|-------------|--------------|-----------|-----------|
| 1 | Fix E18 dead KNN path | `DefaultLogicalCapabilityTrainer.cs` (trainer seeding), `PromptDomainClassifier.cs` (optional seeded classifier) | 2 | 1313 |
| 2 | Replace BM25 whitespace tokenizer with E15 stemmed tokens | `LlmIngredientLogicalUnits.cs` | 2 | 1315 |
| 3 | Add E25 PatternClassifier as third fusion component | `LlmIngredientLogicalUnits.cs` | 3 | 1318 |

---

## Locked decisions referenced

- D-11: No external ML frameworks — TensorCore only. All three phases comply; `SemanticNormalizer` and `PatternClassifier` are
  pure C# with no external dependencies.
- D-04: Domain split CODE_Q / BUG / PLAN / ARCH / REFACTOR. Phase 1 feeds this classifier.
- A43: A class that exists in project 29 but has no reference path from project 21 is NOT implemented. Phase 1 closes this
  gap for E18.
- A59: Inference query compiled before KNN lookup. Phases 2 and 3 build on this — the E15 normalizer and PatternClassifier
  both receive the already-compiled canonical text, not raw input.

---

## Phase 1 — Fix the E18 dead KNN path

### What is wrong

`PromptDomainClassifier` holds a static `DomainClassifierUnit Classifier = new()`. This creates the unit with no training
examples. `DomainClassifierUnit.ClassifyBaseDomain` checks `if (_trained && predicates != null)` — since `_trained` is
false, it always falls to `ClassifyByKeywords`. The E18 KNN route is never reached.

The fix is straightforward: seed the static classifier with the known canonical domain predicate pairs during
`DevelopmentLifecycleTopologySeeder.Seed()` or in a dedicated static initialiser. The `RouteClassifier.Train()` method
in project 29 accepts `IReadOnlyList<(int[] Predicates, Domain Domain)>` — predicate integer arrays mapped to domains.

The canonical predicate seed is already available in `SemanticTeachingPromptCompiler.CanonicalPredicateSeed`:

```
"SECURITY AUTHORIZATION API_CONTRACT ARCHITECTURE IMPLEMENT TEST QA UI DOCUMENT LEARNING"
```

Each of these maps to one of the five D-04 domains. The seeding gives the KNN classifier at least one representative
per domain, enough to let the trained path open. In production the classifier grows as more prompts pass through the
`DevelopmentPromptCompiler` and their `PipelinePredicates` are observed.

### Files to change

**`21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`**

Replace the current static `Classifier = new()` field with a lazily-seeded version. Add a static method
`SeedClassifier(IReadOnlyList<(int[] predicates, Domain domain)> examples)` so the topology seeder can
inject canonical examples at startup.

```
Before (line 14):
    private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = new();

After:
    private static readonly SemanticCompilerAdapter.DomainClassifierUnit Classifier = BuildSeededClassifier();

    // Build the E18 route classifier pre-seeded with canonical domain predicate examples.
    private static SemanticCompilerAdapter.DomainClassifierUnit BuildSeededClassifier()
    {
        // One representative predicate set per D-04 domain gives the KNN path a chance to fire.
        var unit = new SemanticCompilerAdapter.DomainClassifierUnit();
        unit.Train([
            ([1, 2, 3], SemanticCompiler29.Domain.ARCH),      // architecture/design predicates
            ([4, 5, 6], SemanticCompiler29.Domain.BUG),       // null/catch/throw predicates
            ([7, 8, 9], SemanticCompiler29.Domain.PLAN),      // sequence/aggregate/send predicates
            ([10, 11],  SemanticCompiler29.Domain.REFACTOR),  // filter/convert predicates
            ([12, 13],  SemanticCompiler29.Domain.CODE_Q),    // validate/get predicates
        ]);
        return unit;
    }
```

The actual integer ids come from whatever `DevelopmentPromptCompiler` assigns — the placeholder integers above will
be replaced with the real `PipelinePredicates` integer sequences from the seeder. The key requirement is that
`_trained` becomes `true` and the KNN path opens.

### Gate B (Phase 1)

Before: `PromptDomainClassifier.Classify("this is a null reference bug")` always returns evidence including
`"semantic_domain_keyword_fallback"` and never includes `"context_domain_match"` from KNN path.

After: at least one of the five domain seeds produces a KNN-path result with confidence ≥ 0.82 (the `evidenceCount == 1`
branch in `CalculateBaseConfidence`) when a prompt containing matching predicates is classified.

Measurable assertion: `classification.Evidence` must NOT contain `"semantic_domain_keyword_fallback"` for prompts whose
compiled `PipelinePredicates` overlap with the seed examples.

### Tests (Phase 1) — 2 new tests in `DomainClassifierKnnPathTests.cs`

**Test 1:** `DomainClassifier_WithTrainedSeed_UsesKnnPathForPredicateInput`
Train `DomainClassifierUnit` with 5 seed entries (one per domain). Pass a request that includes non-null `Predicates`
matching the BUG seed. Assert `_trained == true` (observable via `Classify()` succeeding without `"semantic_domain_keyword_fallback"`
in evidence) and the returned domain equals `Domain.BUG`.

**Test 2:** `PromptDomainClassifier_StaticInstance_IsPreSeededAndKnnPathReachable`
Call `PromptDomainClassifier.Classify("fix this null reference")`. Assert that the static classifier is trained
(confidence > 0.20 and evidence does not contain only `"semantic_domain_keyword_fallback"`). This proves the static
initialiser ran and the path is open.

### Commit message

```
phase 1: fix E18 dead KNN path — DomainClassifierUnit now pre-seeded at static init
Files: PromptDomainClassifier.cs, DomainClassifierKnnPathTests.cs
```

---

## Phase 2 — Replace BM25 whitespace tokenizer with E15 stemmed tokens

### What is wrong

`Tokenize()` in `TransformerEmbeddingKnnLogicalUnit.ScoreCandidates` splits on whitespace and punctuation and lowercases.
It does NOT apply E15 stemming or stop-word removal. E34V2 proved +8% hit@1 specifically requires E15-normalised tokens.

`SemanticNormalizer.Normalize()` is the E15 production class in project 29. It strips stop words and applies a
dictionary-free suffix stemmer. It requires `ICompilerStorage` and `ICompilerEventLogger` through `BaseService`, but
both can be satisfied with `InMemoryCompilerStorage` and `NoopCompilerEventLogger` — which `DevelopmentPromptCompiler`
already uses internally. However, adding a project-29 dependency to project 21's `LlmIngredientLogicalUnits.cs` is not
the right design — it would couple the learned-unit infrastructure to the full semantic compiler stack.

The correct approach: extract the stemming and stop-word logic as a private static helper inside
`TransformerEmbeddingKnnLogicalUnit`, mirroring what E15 does without adding a cross-project dependency. The logic
is small: split on non-alpha, lowercase, remove a fixed stop-word set, apply suffix stemming. The same list used
in `SemanticNormalizer` can be inlined as private static readonly collections.

This keeps `LlmIngredientLogicalUnits.cs` self-contained (Rule 2 — Fabric First; no new concrete dependency on
project 29 from project 21).

### Files to change

**`21.AgentInfrastructure/Logical/LlmIngredientLogicalUnits.cs`**

Replace the `Tokenize()` method body:

```
// Before: whitespace+punctuation split, lowercase only
private static Dictionary<string, int> Tokenize(string text)
{
    var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var token in (text ?? string.Empty)
        .Split([' ', '\t', '\r', '\n', '.', ',', ':', ';', '/', '\\', '-', '_', ...], ...)
        .Select(token => token.ToLowerInvariant())
        .Where(token => token.Length > 1))
    { ... }
    return tokens;
}

// After: E15-equivalent — stop-word removal + suffix stemming inline
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

// E15 stop words mirrored from SemanticNormalizer — no cross-project dependency needed.
private static readonly HashSet<string> E15Stopwords = new(StringComparer.OrdinalIgnoreCase)
{
    "a","an","the","of","in","on","at","by","for","with","about","from","to",
    "into","through","is","are","was","were","be","been","being",
    "have","has","had","do","does","did","can","could","will","would",
    "shall","should","may","might","must","need","used","let","get","got",
    "i","we","it","its","and","or","but","not","no","as","if","so",
    "then","else","this","that","which","who","when","where","how","why",
};

// E15 suffix stemmer mirrored from SemanticNormalizer — dictionary-free, Rule 10 compliant.
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

### Gate B (Phase 2)

Before: `Tokenize("authorization")` returns `{"authorization": 1}`.

After: `Tokenize("authorization")` returns `{"authoriz": 1}` (suffix "ation" stripped).
Also: `Tokenize("the fix")` returns `{"fix": 1}` (stop word "the" removed, "fix" kept as is — shorter than stem threshold).

Measurable assertion: on a pair of training examples where one document contains "implementing" and the query
contains "implement", the lexical score with E15 tokenization is strictly higher than with the old whitespace tokenizer,
because both stem to "implement". Assert `lexicalScore_e15 > lexicalScore_old` on this controlled pair.

### Tests (Phase 2) — 2 new tests in `TransformerKnnTokenizerTests.cs`

**Test 1:** `ScoreCandidates_WithE15Tokenizer_StemsMatchBetterThanWhitespaceSplit`
Create a `TransformerEmbeddingKnnLogicalUnit` trained on a document containing "implementing the authorization pattern".
Query with "implement authorization". Assert that the fused score for this document under E15 tokenization is strictly
greater than it would be under raw whitespace split (prove by also testing the raw split path via the old test and
comparing). Specifically: assert the selected example's `FusedScore >= 0.60`.

**Test 2:** `Tokenize_StopWordsRemovedAndSuffixStemmed`
Call the (now-accessible-via-internal-for-test) `Tokenize` directly or invoke through a scored pair. Assert that
"authorization" becomes "authoriz", "implementing" becomes "implement", and stop words "the", "a", "in" are absent
from the token dictionary.

### Commit message

```
phase 2: replace BM25 whitespace tokenizer with E15 stemmed tokens in ScoreCandidates
Files: LlmIngredientLogicalUnits.cs, TransformerKnnTokenizerTests.cs
```

---

## Phase 3 — Add E25 PatternClassifier as third fusion component

### What is add

Add `posScore` as the third component in `ScoreCandidates`. Weights shift from `vector×0.55 + bm25×0.45` to
`vector×0.45 + bm25×0.35 + pos×0.20` as proposed in the experiment state.

`PatternClassifier.Classify(string[] tokens)` in project 29 returns one of: `"SVO"`, `"SOV"`, `"VSO"`, `"IMPERATIVE"`,
`"UNKNOWN"`. The Jaccard similarity on POS-pattern sequence is the proposed `posScore` — but since we're matching
a query pattern to a document pattern (not a sequence), the simpler version is: compute the pattern for both query and
document, assign `posScore = 1.0` if patterns match, `0.5` if both are non-UNKNOWN but differ, `0.0` if either is UNKNOWN.

`PatternClassifier` cannot be directly instantiated in project 21 without introducing a project-29 service dependency.
The fix is the same as Phase 2: extract the static logic inline. `PatternClassifier.InferPos`, `ActionWords`, and
`StructureWords` are all private static — they can be mirrored as private static readonly members in
`TransformerEmbeddingKnnLogicalUnit` alongside `E15Stopwords` and `E15Stem`. The `Classify()` logic itself is
20 lines of pure token-index arithmetic — inlineable.

### Files to change

**`21.AgentInfrastructure/Logical/LlmIngredientLogicalUnits.cs`**

1. Add `E25PatternScore(string query, string document)` private static method returning `double` in `[0.0, 1.0]`.
2. Add `E25InferPos(string token)` private static method (mirrors `PatternClassifier.InferPos`).
3. Add `E25ActionWords` and `E25StructureWords` private static readonly `HashSet<string>` (mirrors the sets).
4. Add `E25ClassifyPattern(string text)` that tokenizes on whitespace, infers POS, returns pattern string.
5. Update `ScoreCandidates` to compute `posScore` for each candidate and blend into fused score.
6. Update the `fused` calculation: `(vectorScore * 0.45) + (normalizedLexical * 0.35) + (posScore * 0.20)`.
7. Add `"E25:PATTERN_CLASSIFIER"` to the manifest `IngredientIds` array in `Train()`.

The pattern comparison:

```csharp
private static double E25PatternScore(string query, string document)
{
    // Compare POS-pattern sequence between query and document for structural similarity.
    var queryPattern = E25ClassifyPattern(query);
    var docPattern   = E25ClassifyPattern(document);
    if (queryPattern == "UNKNOWN" || docPattern == "UNKNOWN") return 0.0;
    if (queryPattern == docPattern) return 1.0;
    return 0.5; // both have structure but differ — partial credit
}
```

### Gate B (Phase 3)

Before: manifest `IngredientIds` contains `"E34V2:BM25_KNN_FUSION"` but not `"E25:PATTERN_CLASSIFIER"`.

After: manifest `IngredientIds` contains both `"E34V2:BM25_KNN_FUSION"` and `"E25:PATTERN_CLASSIFIER"`.
Also: for two documents where one matches the query's POS pattern and one does not, the matching document
must score strictly higher than the non-matching document in `ScoreCandidates` when both have equal vector scores.

Measurable assertion: train a unit on two examples — one IMPERATIVE ("fix this null reference", output A) and
one SVO ("this function returns null", output B). Query with "fix the crash". Assert output A is selected
(IMPERATIVE query matches IMPERATIVE document) and the fused score delta is attributable to posScore.

### Tests (Phase 3) — 3 new tests in `TransformerKnnPatternFusionTests.cs`

**Test 1:** `ScoreCandidates_PatternMatch_BoostsImperativeQueryOverSvoDocument`
Train on IMPERATIVE example and SVO example with equal embedding distances (controlled by seeding identical text).
Query with an IMPERATIVE prompt. Assert the IMPERATIVE document wins.

**Test 2:** `PatternScore_MatchingPatterns_ReturnsOne_MismatchedReturnsHalf_UnknownReturnsZero`
Unit-test `E25PatternScore` directly: SVO vs SVO → 1.0; SVO vs SOV → 0.5; UNKNOWN vs SVO → 0.0.

**Test 3:** `Manifest_IngredientIds_ContainsE25PatternClassifier`
Train a `TransformerEmbeddingKnnLogicalUnit` and assert `Manifest.IngredientIds` contains
`"E25:PATTERN_CLASSIFIER"`.

### Commit message

```
phase 3: add E25 PatternClassifier as third fusion component in ScoreCandidates
Files: LlmIngredientLogicalUnits.cs, TransformerKnnPatternFusionTests.cs
```

---

## Floor summary

| State | Test count |
|-------|-----------|
| Before this plan (latest zip) | 1311 |
| After Phase 1 | 1313 |
| After Phase 2 | 1315 |
| After Phase 3 | 1318 |

---

## What this plan does NOT change

- Weights are a tunable decision. The proposed weights (0.45 / 0.35 / 0.20) are locked as Phase 3 defaults.
  Calibration against a real run will refine them — that calibration is a separate plan after the next
  batch-100 run.
- E18 training data. Phase 1 opens the KNN path with a minimal canonical seed. Training on real compiled
  prompts from the calibration run will grow the classifier's accuracy toward the proven +15.2%. The
  held-out validation is future work.
- E34V2 BM25 variant with auto-tuned alpha. The `0.35` weight for lexical is the first fixed value.
  Auto-tuning is deferred until the calibration run produces hit@1 data.
- The nine `dpc_applies=true` experiments (E18, E22, E32, E33, E34, E35, E36, E38) that receive raw
  strings instead of compiled form. These are all on the `ILogicalUnit` wiring path —
  `PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4` covers them.

---

## Locked decisions added by this plan (A61–A63)

**A61:** `DomainClassifierUnit` in `PromptDomainClassifier` must be pre-seeded with canonical domain predicate
examples at static construction time. A `DomainClassifierUnit` constructed with `new()` and never trained has
`_trained=false`, making the KNN route permanently unreachable. Fix: `BuildSeededClassifier()` static factory.

**A62:** The `Tokenize()` method in `TransformerEmbeddingKnnLogicalUnit.ScoreCandidates` must apply E15-equivalent
stemming and stop-word removal, not whitespace-only splitting. The E34V2 +8% hit@1 gain requires stem-matched tokens.
Implementation: inline the E15 logic as `E15Stopwords`, `E15Stem` private statics — no cross-project dependency.

**A63:** The KNN fusion in `ScoreCandidates` has three components: vector (0.45), BM25-E15 (0.35), POS pattern (0.20).
`PatternClassifier` (E25, +154.6%) provides the pattern score via inline-mirrored static logic.
All three weights are initial defaults pending calibration from the batch-100 run.

---

# Last updated: 2026-05-07
# Floor before: 1311 | Floor after: 1318 (+7 tests)
# Decisions added: A61, A62, A63
