# E13B — Expression Prefix Completion via KNN
# Version: 1.0 — 2026-05-06
# Session type: ARCHITECT
# Status: NEW EXPERIMENT — not yet run
# Companion to: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md (add as Phase 9)

---

## The hypothesis

A partial expression like `Y + Z =` is a prefix of a complete expression row
`Y + Z = X`. When both the partial and complete expressions are embedded into the
same vector space using the existing `KnnLogicalUnit` two-tier vectorisation
(Byte-Pair-Encoding token tier + predicate tier), the partial expression lands
close to its correct complete counterpart because the shared tokens (`Y`, `+`,
`Z`, `=`) dominate the representation. The missing right-hand side (`X`)
contributes relatively few tokens and does not pull the complete vector far from
the partial one. The nearest neighbour in the index is the correct completion.

**Baseline:** return the most frequent right-hand side in the training corpus
for any partial expression (majority-class predictor).

**Enhanced:** `KnnLogicalUnit<string, string>` trained on `(partial, complete)`
pairs, queried with the partial expression, returns the nearest complete row's
right-hand side.

**Primary metric:** `top1_completion_accuracy` — how often the nearest neighbour
returns the exact correct right-hand side.

**Secondary metric:** `cosine_gap` — distance between the top-1 hit and the
top-2 hit. A small gap means the index is ambiguous; a large gap means the
correct completion is unambiguously closest.

**Scaling signal:** `prefix_coverage_ratio` — fraction of the partial expression's
tokens that appear in the top-1 hit's stored vector. Should be > 0.80 for the
hypothesis to hold.

---

## Why this is different from E13

E13 asked: given method N's AST structure, predict the type of method N+1.
That failed because method-to-method sequence in a file has no reliable pattern.

E13B asks: given a partial expression, predict the rest of it. This works because
the partial expression and its completion share almost all tokens — the vector
distance is structurally guaranteed to be small when the prefix is long enough
relative to the completion.

The mechanism is prefix containment, not structural neighbourhood. E13 was
looking for a next-item pattern in a sequence. E13B is looking for the nearest
complete item in a corpus of complete items, starting from a partial item.

---

## What already exists — zero new classes required for the experiment

`KnnLogicalUnit<TInput, TOutput>` in project 21 already provides:
- `Train(unitId, unitRole, domainTag, examples, textSelector)` — builds the
  Byte-Pair-Encoding tokeniser from the training corpus and inserts each example
  into the HNSW index.
- `ExecuteAsync(request)` — vectorises the query using the same tokeniser,
  searches the index, returns the nearest example's `Output` field.
- `LogicalKnnTrainingExample<TInput, TOutput>` — carries `(ExampleId, Input,
  Output, PredicateIds, DomainTags, EvidenceIds)`.

The training examples for E13B are:
```
Input  = "Y + Z ="       (the partial expression — what the user typed)
Output = "X"             (the right-hand side — what we want to predict)
ExampleId = "expr-row-001"
PredicateIds = []        (no predicate enrichment needed for this experiment)
```

The `textSelector` function passed to `Train` and used in `ExecuteAsync` is
`(string s) => s` — the input is already a plain string.

---

## Experiment contract

```text
Contract:
  KnnLogicalUnit<string, string> : ILogicalUnit<string, string>

A (training time — one example per complete expression row):
  ExampleId:    "expr-row-001"
  Input:        "Y + Z ="         ← partial expression (left side + operator)
  Output:       "X"               ← right-hand side completion
  PredicateIds: []
  DomainTags:   ["expression_completion"]
  EvidenceIds:  ["source-corpus-row-001"]

B (inference time — query with partial expression):
  Input:        "Y + Z ="
  → ExecuteAsync returns:
      Output:     "X"
      Confidence: cosine similarity score (0.0–1.0)
      Warnings:   [] if confidence ≥ floor, ["knowledge_edge: ..."] if below

Baseline (majority predictor):
  Count right-hand sides in training set.
  For any query, return the most frequent right-hand side.
  Accuracy = fraction of validation queries where most-frequent equals correct.
```

---

## Algorithm — plain language

**Training pass:**

For each complete expression row in the corpus:
1. Extract the left-hand side and operator as the partial expression string.
   For `Y + Z = X`: partial = `"Y + Z ="`, completion = `"X"`.
2. Create a `LogicalKnnTrainingExample` with `Input = partial`, `Output = completion`.
3. Call `KnnLogicalUnit.Train` with all examples, `textSelector = s => s`,
   `vectorDimensions = 64`, `k = 3`, `confidenceFloor = 0.65`.

The tokeniser trains on all partial expressions combined. Each partial expression
is vectorised as: Byte-Pair-Encoding token counts in the first 32 slots, predicate
hash counts (empty, so zero) in the last 32 slots, then L2 normalised.

**Query pass:**

Given partial expression `"Y + Z ="`:
1. Vectorise using the trained tokeniser.
2. Search the HNSW index for top-3 nearest neighbours by cosine similarity.
3. Return the Output field of the nearest hit (highest cosine similarity).
4. If cosine similarity < 0.65, emit a knowledge edge warning.

---

## Confidence floor consideration

The default confidence floor in `KnnLogicalUnit` is 0.70. For expression prefix
completion the floor should be lowered to 0.65 because:
- Partial expressions share most but not all tokens with their completions.
- The missing right-hand side tokens lower the raw cosine similarity compared
  to a full-expression-to-full-expression comparison.
- A partial expression with 80% token overlap to the correct completion will
  score around 0.65–0.72 depending on right-hand side length.

If the validation run shows median confidence above 0.75, the floor can be
raised back to 0.70. The floor is a parameter, not a contract.

---

## Success criteria

| Metric | Threshold | Rationale |
|---|---|---|
| top1_completion_accuracy | > baseline + 15% | Meaningful improvement over majority predictor |
| cosine_gap (top1 vs top2) | > 0.05 | Top-1 is unambiguously closest |
| prefix_coverage_ratio | > 0.80 | Partial expression tokens dominate the vector |
| knowledge_edge rate | < 20% | Most queries are above confidence floor |

If top1_completion_accuracy is below baseline or within 5% of baseline:
the hypothesis does not hold — partial expressions are too similar to each other
in vector space for the nearest neighbour to be the correct completion. Do not
promote to production.

---

## Where this fits in the wiring plan (Phase 9)

After `PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md` Phase 8 (full
suite verification), add Phase 9:

**Phase 9 — E13B Expression Prefix Completion Unit**

New file: `23.SemanticCompilerAdapter/ExpressionCompletionUnit.cs`

```
Contract:
  ExpressionCompletionUnit : ILogicalUnit<string, string>

Wraps KnnLogicalUnit<string, string> trained on expression corpus.
Registered in unit registry as role "expression_completion".
Called from node 04 or node 05 of the nine-node flow when the domain
processor detects an incomplete expression in the work branch.
```

Training source: the expression corpus extracted from the existing teaching
ledger rows. Each row that contains an assignment statement of the form
`LHS operator = RHS` contributes one training example.

Gate B — Phase 9:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "ExpressionCompletion" 2>&1 | tail -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- Partial expression `"Y + Z ="` retrieves completion `"X"` from a small
  training corpus containing `"Y + Z = X"`.
- Cosine similarity for exact partial match exceeds 0.65.
- Majority-class baseline is computed and improvement is measured.
- Knowledge edge warning fires when confidence is below floor.
- Floor parameter is configurable at construction time.

New floor: prior floor + 5 tests (E13B experiment) + 2 tests (unit registration).
Total addition: 7 tests.

---

## Relationship to existing infrastructure

`KnnLogicalUnit` already handles everything. `ExpressionCompletionUnit` in
project 23 is a thin wrapper that:
1. Holds the trained `KnnLogicalUnit<string, string>` instance.
2. Implements `ILogicalUnit<string, string>` by delegating `ExecuteAsync`.
3. Provides a `static Train(corpus)` factory that extracts partial/completion
   pairs from a `IReadOnlyList<string>` of complete expression rows and calls
   `KnnLogicalUnit.Train`.

No new ML infrastructure required. No new projects required. The experiment
runs entirely on existing `KnnLogicalUnit` + `HnswIndex` + `BpeTokenizer`.

---

## What the experiment measures that E13 did not

E13 measured: can KNN predict the type of the NEXT method in a file?
→ Failed because file-level method sequence has no pattern.

E13B measures: can KNN retrieve the completion of a PARTIAL expression?
→ Expected to succeed because partial expressions share most tokens with
  their completions — the vector distance is small by construction when the
  prefix covers most of the complete row.

The structural guarantee: if `partial` covers fraction `f` of `complete`'s
tokens, and both are L2-normalised, then `cosine(partial_vector, complete_vector)
≥ √f`. For a prefix covering 80% of tokens, cosine similarity ≥ 0.894 in the
ideal case. Real results will be lower due to tokenisation effects and vector
dimension collisions, but the floor should hold above 0.65 for any prefix
covering more than half the expression.

---

# Document version: 1.0
# Author: ARCHITECT session 2026-05-06
# New classes: ExpressionCompletionUnit (23.SemanticCompilerAdapter)
# New experiment: E13B (not yet run — this document is the contract)
# Infrastructure required: none — KnnLogicalUnit already supports this
# New tests if promoted: 7
# Prerequisite: v4 Phase 8 complete
