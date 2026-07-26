# API Shape Verification

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

## When to Invoke

- At infrastructure discovery (Gate 0 of phase-planning-SKILL.md) — verify every class/interface before writing tests for it
- Before writing any test that calls a class method — verify method signature and return type
- After any phase that had plan-execution-feedback discovery rate > 120%

---

## Why This Exists

Most write-time plan fixes are API shape discoveries:
- Assumed method takes 2 parameters → actually takes 3
- Assumed method is async → it's synchronous
- Assumed property is a field → it's a getter with different semantics
- Assumed interface has method X → it doesn't (was removed or renamed)

Verifying API shapes at Gate 0 costs 30 seconds per symbol.
Discovering a shape mismatch mid-implementation costs a blocked session.

---

## Verification Protocol

For each class or interface the plan will use:

```bash
# Step 1: Verify the class/interface exists
grep -n "public.*class ClassName\|public.*interface IName" Src/ -r --include="*.cs"

# Step 2: Verify the specific method exists with correct signature
grep -n "public.*MethodName\s*(" Src/NN.Project/ClassName.cs | head -5

# Step 3: For async methods — verify Task<T> vs T return
grep -n "public.*Task.*MethodName\|public.*async.*MethodName" Src/NN.Project/ClassName.cs

# Step 4: For properties — verify property vs method
grep -n "public.*\bPropertyName\b" Src/NN.Project/ClassName.cs
# Check: is it { get; } (property) or () method?

# Step 5: Verify the return type wrapping
grep -n "OperationResult\|Task<" Src/NN.Project/ClassName.cs | head -10
```

---

## Verification Table

Produce this table before writing any test code that uses a class:

| Symbol | Source File | Type | Signature | Return Type | Notes | Verified |
|--------|-------------|------|-----------|-------------|-------|---------|
| `BruteForceIndex` | `Src/09.VectorSearch/BruteForceIndex.cs` | class | `new BruteForceIndex(int dimensions)` | instance | constructor | ☐ |
| `Search` | method on BruteForceIndex | method | `Search(float[] query, int topK)` | `OperationResult<List<SearchResult>>` | sync, not async | ☐ |
| `IVectorIndex` | `Src/09.VectorSearch/IVectorIndex.cs` | interface | — | — | confirm Search signature | ☐ |
| `SessionStateStore` | `Src/21.AgentInfrastructure/SessionStateStore.cs` | class | `SaveAsync(string id, object state)` | `Task<OperationResult<bool>>` | async | ☐ |

---

## Rules

1. **Never assume** — even "obvious" APIs may have changed since the last session
2. **Check return type wrapping** — `OperationResult<T>` vs `T` vs `Task<T>` — all different
3. **Check sync vs async** — calling a sync method with `await` will fail to compile
4. **Check parameter order** — `Search(query, topK)` not `Search(topK, query)`
5. **If symbol not found** — STOP. Do not write the test. Report to Luba.
6. **Complete the verification table before writing the first test line**

---

## Examples from This Project

Common shape mismatches to watch for:

```bash
# Verify OperationResult<T> usage in a class
grep -n "OperationResult<" Src/09.VectorSearch/BruteForceIndex.cs | head -10
# Confirm: return type is OperationResult<List<SearchResult>> not just List<SearchResult>

# Verify constructor parameter names
grep -n "public BruteForceIndex\|public HnswIndex" Src/ -r --include="*.cs"
# Confirm: dimensions is the parameter name, not vectorSize or dimension

# Verify async pattern
grep -n "public async\|Task<OperationResult" Src/21.AgentInfrastructure/SessionStateStore.cs | head -5
# Confirm: SaveAsync is truly async (returns Task)
```

---

## Integration with Plan Execution Feedback

After each phase, record any shape mismatches that were discovered during implementation:

```
API shape discovery (write-time fix):
  Planned: BruteForceIndex.Search(float[] query, int topK)
  Actual:  BruteForceIndex.Search(float[] query, int topK, CancellationToken ct = default)
  Fix: Update test calls to not rely on exact parameter count
```

This feeds into plan-execution-feedback-SKILL.md discovery rate tracking.
If discovery rate > 120% for 2 phases: the next phase must complete this full verification table at Gate 0.
