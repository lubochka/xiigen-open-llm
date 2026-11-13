# HOW-TO-USE-SKILLS.md — When to load which skill
## v2.0: four session types, thirteen activation triggers, ARCHITECT as default,
## D-07/D-09/vocabSize critical rules added, subproject gate added.

---

## Load order — every session starts here

    Step 0: Read CLAUDE.md + AGENTS.md                (every session — before skills)
    Step 1: Run dotnet test — record live baseline     (every session)
    Step 2: Load session-type skill (see table below)  (every session)
    Step 3: Declare session type and test baseline     (every session, one paragraph)

---

## Session type → skills to load

| Session type | Load before first output |
|---|---|
| ARCHITECT (default) | plan-review-SKILL.md + no-product-decisions-SKILL.md |
| EXECUTOR | dev-safety-SKILL.md + code-execution-SKILL.md |
| EXPERIMENT | experiment-design-SKILL.md + code-execution-SKILL.md |
| REVIEW | plan-review-SKILL.md |

ARCHITECT is the default. Every session starts as ARCHITECT unless Luba
explicitly declares a different type.

---

## Activation triggers — load these skills when the condition is met

| When | Load |
|---|---|
| Any test fails unexpectedly | mental-debug-SKILL.md |
| Plan has no Gate B for any phase | plan-review-SKILL.md |
| Choosing between two architectural approaches | no-product-decisions-SKILL.md |
| New experiment E-N being designed | experiment-design-SKILL.md |
| Commit about to happen | dev-safety-SKILL.md |
| Result is FAILED or SCALE_UP | experiment-design-SKILL.md (failure classification section) |
| New class created that wraps an external service | code-execution-SKILL.md (BaseService pattern) |
| Plan references a test count | code-execution-SKILL.md (live baseline rule) |
| E20 order (StackDetector vs normalization) in question | experiment-design-SKILL.md + DECISIONS-LOCKED D-07 |
| vocabSize or weight formula appears in plan | experiment-design-SKILL.md + DECISIONS-LOCKED D-09 |
| StratifiedSplit mentioned or omitted | experiment-design-SKILL.md + DECISIONS-LOCKED D-08 |
| Session produces React pages or UI | (none — no UI in this project) |
| Any session where Luba gives a correction | Re-read Round Contract Check 3 before next response |

---

## Critical locked decisions — know before any plan

These three decisions are the most commonly violated. They are in DECISIONS-LOCKED.md
but repeated here because violations cause test failures.

**D-07 — StackDetector runs before normalization**
StackDetector must execute before SemanticNormalizer in the pipeline.
Running it after destroys C#/TypeScript/Python markers before detection.
Any plan that reorders these two components violates D-07.

**D-08 — StratifiedSplit for all experiments**
All experiments use StratifiedSplit, not a simple random split.
Seed-42 naive split collapses class distribution (E40, E42).
Any plan that calls for random split without stratification violates D-08.

**D-09 — max_weight formula**
Weight formula: max_weight = total_samples / (num_classes × rare_class_count)
Using a constant multiplier (for example ×10) causes catastrophic forgetting (E44).
Any plan that proposes a fixed weight constant without this formula violates D-09.

---

## Gate B format — required in every plan phase

Gate B is a bash command that returns a number or string, not English prose.

    ✅  grep -c "public.*OperationResult" 29.SemanticCompiler/Pipeline.cs  # expected: >= 5
    ✅  dotnet test --filter "SC29_" 2>&1 | tail -3                        # expected: N passed, 0 failed
    ❌  "Verify the pipeline works correctly"
    ❌  "Confirm the tests pass"

Every phase in every plan needs one Gate B that can be run literally as written.

---

## BaseService pattern — required for every new service class

Every new service class must extend BaseService and pass ISessionStorage to base.

```csharp
// ✅ Correct
public sealed class MyService : BaseService, IMyInterface
{
    public MyService(ISessionStorage storage) : base(storage) { }
}

// ❌ Missing BaseService
public sealed class MyService : IMyInterface { }

// ❌ Missing constructor
public sealed class MyService : BaseService { }
```

---

## OperationResult pattern — required for every public method

Every public method returns OperationResult<T>, never throws, never returns null.

```csharp
// ✅ Correct
public OperationResult<string> GetName() { ... }
public Task<OperationResult<bool>> SaveAsync(...) { ... }

// ❌ Missing wrapper
public string GetName() { ... }
public bool Save() { ... }
```
