# User Journey Testing

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## What This Skill Prevents

The "all unit tests pass but the feature doesn't work" failure.
Individual class tests validate classes. This skill validates the user's
experience of a complete feature flow. The scenarios must be written
BEFORE the implementation, not after.

---

## When to Run

At the start of any phase that implements a user-visible feature.
Produce test stubs BEFORE writing production code.
The stubs become acceptance criteria that constrain what the implementation must produce.

---

## Step 1 — Extract Scenarios from the Feature Description

Every feature description contains:
- **The happy path** (main flow, no errors)
- **Guard conditions** (invalid input, missing data, permission denied)
- **Edge cases** (empty corpus, single document, maximum size)

Map each to a test scenario:

```
Feature: Vector search for agent memory

HAPPY PATH:
  Scenario S-01: User queries with a valid embedding, gets relevant documents back
  Expected: OperationResult.Ok with topK results in descending score order

GUARD CONDITIONS:
  Scenario S-02: Query vector has wrong dimension
    Expected: OperationResult.Fail with "DIMENSION_MISMATCH" error code

  Scenario S-03: Empty corpus — no documents indexed
    Expected: OperationResult.Ok with empty list (not error)

EDGE CASES:
  Scenario S-04: topK requested > document count
    Expected: Returns all available documents (not error)

  Scenario S-05: Identical query and document vectors (exact match)
    Expected: Score = 1.0 (or maximum possible for the metric)
```

---

## Step 2 — Write Test Stubs Before Implementation

Each scenario becomes a test stub written before the production class:

```csharp
// S-01: Happy path — valid query returns topK results
[Fact]
public void BruteForceIndex_ValidQuery_ReturnsTopKResults()
{
    // Arrange: TODO — build index with known vectors once BruteForceIndex is implemented
    // Act: TODO — search with known query vector
    // Assert: result is OperationResult.Ok with K results in score order

    Assert.True(false, "TODO: implement after BruteForceIndex is written — S-01 acceptance criteria");
}

// S-02: Wrong dimension
[Fact]
public void BruteForceIndex_WrongDimension_ReturnsDimensionMismatchError()
{
    Assert.True(false, "TODO: implement — S-02 acceptance criteria");
}

// S-03: Empty corpus
[Fact]
public void BruteForceIndex_EmptyCorpus_ReturnsEmptyList()
{
    Assert.True(false, "TODO: implement — S-03 acceptance criteria");
}
```

These stubs will fail (Class 1 in test-failure-triage-SKILL.md) until the
production class is implemented. That is correct — they are acceptance criteria.

---

## Step 3 — Write Guard Condition Tests

Guard conditions are the most valuable tests. They are the failure modes that
matter most to users. Each guard condition test must:
1. Set up the guard condition (wrong dimension, empty input, etc.)
2. Execute the action
3. Verify the SPECIFIC expected behavior — not just "returns an error"

```csharp
// S-02: Guard condition — dimension mismatch
[Fact]
public void BruteForceIndex_QueryDimensionMismatch_ReturnsFail()
{
    // Arrange: build 128-dimensional index
    var index = new BruteForceIndex(dimensions: 128);
    index.Add(0, new float[128]);
    var wrongDimensionQuery = new float[64]; // wrong: 64 dimensions

    // Act: search with wrong-dimension query
    var result = index.Search(wrongDimensionQuery, topK: 1);

    // Assert: specific failure (not just result.IsFail)
    Assert.True(result.IsFail);
    Assert.Equal("DIMENSION_MISMATCH", result.ErrorCode); // specific code, not just any error
}
```

---

## Step 4 — Attach to Phase Gate B

The test scenarios from this skill become the Gate B conditions for the phase:

```
Gate B for Phase 2 (BruteForceIndex):
  dotnet test --filter "BruteForceIndexTests" shows:
  S-01 (valid query): PASS
  S-02 (wrong dimension): PASS
  S-03 (empty corpus): PASS
  S-04 (topK > count): PASS
  S-05 (exact match score): PASS
  Total: Failed: 0, Passed: ≥ 5
```

---

## Scenario File Structure

One file per feature, in the test project:

```
Tests/
  VectorSearch/
    BruteForceIndexTests.cs   ← happy path + guard conditions + edge cases
    HnswIndexTests.cs         ← same structure
    SCENARIO-MANIFEST.md      ← lists all scenarios with status
```

The SCENARIO-MANIFEST.md:
```markdown
## Vector Search Scenario Manifest

| Scenario | Test | Status |
|---------|------|--------|
| S-01: Valid query → topK results | BruteForceIndex_ValidQuery_ReturnsTopKResults | ✅ PASS |
| S-02: Wrong dimension → fail | BruteForceIndex_QueryDimensionMismatch_ReturnsFail | ✅ PASS |
| S-03: Empty corpus → empty list | BruteForceIndex_EmptyCorpus_ReturnsEmptyList | ✅ PASS |
| S-04: topK > count → all docs | BruteForceIndex_TopKExceedsCount_ReturnsAll | ✅ PASS |
| S-05: Exact match → score 1.0 | BruteForceIndex_ExactMatch_ReturnsMaxScore | ✅ PASS |
```

---

## Anti-Patterns

1. **Writing acceptance tests after implementation** — "tests to match the code" validates nothing
2. **Guard condition tests that only check return value, not error code** — specificity is the point
3. **Happy path only** — guard conditions are where most bugs live
4. **Tests that only check OperationResult.IsFail without checking ErrorCode** — too weak; fails are not interchangeable
5. **Not linking scenarios to Gate B** — if a scenario is not in Gate B, it can be forgotten
