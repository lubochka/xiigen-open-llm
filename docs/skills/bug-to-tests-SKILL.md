# Bug to Tests

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
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

## The Iron Rule

Для переноса учебного результата сначала применять [договор45](../plans/replanning-possible-diffs/experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md): проверить контракт и наличие нужного обучения, затем собрать продукт/тестовый проект и выполнить назначенные проверки. Непредъявленная конструкция не доказывает неспособность её выучить. Не создавать product-исключение для исправления такого утверждения. Не закрывать реализацию с UNRUN; при падении эскалировать архитектору фактический журнал.

**Preserve approved behavior; repair incorrect tests against independent authority.**

Assigned transfer/code QA/test repair includes builds, test execution and justified test adaptation. Correct a type, fixture or assertion against an independently approved contract without renewed permission; record its source, old/new expectation and preserved discriminating control. Changing the user's requirement, accepting broken actual output, dropping coverage or adding Skip is not adaptation. Use [QA46](../plans/replanning-possible-diffs/experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md).

## Bug ID Format

Every bug gets an ID before work starts:

```
BUG-NN: [one-line description]
Component: [subproject or class name]
Symptom: [what was observed]
Hypothesis: [suspected root cause — one sentence]
```

Examples:
```
BUG-01: BruteForceIndex.Search returns wrong nearest neighbour for zero-vector input
Component: 09.VectorSearch / BruteForceIndex
Symptom: Search(query: float[5]{0,0,0,0,0}) returns index 3 instead of index 0
Hypothesis: cosine distance formula divides by magnitude; zero vector has magnitude 0

BUG-02: HybridRetriever score overflow when BM25 score > 1.0
Component: 12.HybridRetriever / HybridRetriever
Symptom: Combined score returns NaN for documents with term frequency > threshold
Hypothesis: normalisation step missing before combining BM25 and vector scores
```

---

## The 6-Step Protocol

### Step 1 — Assign Bug ID
Write `BUG-NN` to docs/STATE-P[N].json (or a dedicated BUG-LOG.md if volume warrants).

### Step 2 — Write discriminating regression coverage and preserve positive controls
Choose the needed unit/integration coverage from the actual contract. The counterexample must expose the defect; valid positive controls may already pass. A suite execution receipt is evidence of running a suite, not a fake test with Assert.True(true).

```csharp
// Unit test — pure logic, no I/O
[Fact]
public void BruteForceIndex_ZeroVector_ReturnsNearestIndex()
{
    // Arrange: build index with known vectors
    var index = new BruteForceIndex(dimensions: 3);
    index.Add(0, new float[] { 1.0f, 0.0f, 0.0f }); // index 0
    index.Add(1, new float[] { 0.0f, 1.0f, 0.0f }); // index 1
    var query = new float[] { 0.0f, 0.0f, 0.0f };   // zero vector

    // Act: search with zero vector
    var result = index.Search(query, topK: 1);

    // Assert: returns a result without throwing (not NaN, not exception)
    Assert.True(result.IsOk);
    Assert.NotNull(result.Value);
}

// Integration test — real DI, in-process
[Fact]
public async Task HybridRetriever_HighBm25Score_DoesNotReturnNaN()
{
    // Arrange: create retriever via DI with real BM25 scorer
    var services = new ServiceCollection();
    services.AddSingleton<IBm25Scorer, Bm25Scorer>();
    services.AddSingleton<IVectorIndex, BruteForceIndex>();
    services.AddSingleton<HybridRetriever>();
    var provider = services.BuildServiceProvider();
    var retriever = provider.GetRequiredService<HybridRetriever>();

    // Act: query with document that has high term frequency
    var result = await retriever.SearchAsync("the the the the the", topK: 3);

    // Assert: score is finite number, not NaN or Infinity
    Assert.True(result.IsOk);
    Assert.All(result.Value, r => Assert.False(double.IsNaN(r.Score)));
}

// The regression suite is executed separately and produces a real test-runner
// receipt with discovered/executed counts. No always-true placeholder test.
```

### Step 3 — Confirm the counterexample fails for the intended reason
Run before any fix:

```
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~BruteForceIndex_ZeroVector|FullyQualifiedName~HybridRetriever_HighBm25"
→ Verify matched names and the intended counterexample assertion failure; lawful controls may already pass. No fixed two-failure quota.
```

**If the counterexample already passes, inspect whether the defect is already fixed or the path is not exercised. Do not force valid positive controls to fail. A compile/setup failure is not the expected behavioral red.**

### Step 4 — Fix the diagnosed layer
When the independent contract proves the product wrong, fix the product and preserve the assertion. When it proves the test wrong or stale, adapt the test within the assigned repair and preserve meaningful coverage. Actual output alone is never the oracle; a disputed change to the user's requirement goes to the architect/owner.

```
Fix location: Src/09.VectorSearch/BruteForceIndex.cs
Fix: handle zero-magnitude vector in cosine distance calculation
     return 0.0 distance (treat as equally distant from all) or skip gracefully
```

### Step 5 — Confirm All Tests PASS
Run after the fix:

```
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~BruteForceIndex_ZeroVector|FullyQualifiedName~HybridRetriever_HighBm25"
→ All assigned discovered cases pass, Failed:0; use actual names/counts, not an invented two-test total.
```

### Step 6 — Zero-Regression Gate
Run the full test suite:

```
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release
→ Failed: 0
→ Reconcile actual names/cases with the measured start baseline; if unknown, report UNKNOWN rather than substitute an old total.
```

Preserve the full log and original exit; for recorded TRX use the unique run directory from QA46. --no-build requires a verified matching fresh build. **If the total is lower, reconcile discovered names, Theory cases, filters, frameworks and skips against the measured baseline.** A lower count does not prove accidental deletion; an old number such as1488 is not a fresh baseline.

---

## Test Level Reference

| Level | xUnit Pattern | What It Tests |
|-------|--------------|---------------|
| Unit | `[Fact]` + new object directly | Pure logic, no I/O, no DI |
| Integration | `[Fact]` + `ServiceCollection` DI | Component wiring, in-process |
| Regression gate | `dotnet test --filter "Class~ComponentName"` | Full component test suite |

---

## What "Fix the Engine" Means

Fix the class that produces the wrong output, not the code that consumes it.

```
❌ Wrong: Change test to Assert.True(result.IsOk || result.IsFail) to handle NaN case
❌ Wrong: Add null check in the caller of BruteForceIndex
✅ Right: Fix the distance calculation in BruteForceIndex itself
```

Correct an assertion that contradicts an independently approved contract within the assigned repair. An owner decision is needed for a change to the requirement itself, not routine test adaptation.

---

## Anti-Patterns

1. **Fix first, write tests after** — tests written after a fix are post-hoc justifications, not regression guards
2. **Treat a passing positive control as an invalid test** — only the intended counterexample must distinguish the defect; keep lawful cases green
3. **Change the assertion to match broken output** — this documents the bug, it does not fix it
4. **One test for a bug** — bugs need unit + integration coverage minimum
5. **Skip zero-regression gate** — a fix that introduces a new failure is not a fix
