# mental-debug-SKILL.md — Debug C# problems systematically

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
## v3.0: 8 rules translated from xiigen (NestJS→.NET) + diagnostic routing table

## When to load
Load when a test fails unexpectedly or a result is wrong.

---

## The 8 Rules

| # | Rule | What it catches |
|---|------|----------------|
| 1 | pattern-recognition | Misidentifying what kind of failure this is |
| 2 | contextual-gap | Missing context the code assumes was injected |
| 3 | mental-execution | Wrong mental model of how the code actually runs |
| 4 | boundary-data-lifecycle | Data shape changes at method/subproject boundaries |
| 5 | loop-off-by-one | Off-by-one in iteration, slicing, or count checks |
| 6 | error-silent-swallow | IsSuccess not checked — wrong data silently consumed |
| 7 | sibling-guard-parity | Guard exists in one service but missing in its sibling |
| 8 | interface-resolution-trace | Wrong concrete type resolved through interface |

---

## Diagnostic routing

| Symptom | Apply first | Then |
|---------|-------------|------|
| Wrong count (off by 1) | pattern-recognition → loop-off-by-one | mental-execution |
| All predictions same class (data collapse) | pattern-recognition → contextual-gap | boundary-data-lifecycle |
| Negative delta (baseline better than enhanced) | pattern-recognition → contextual-gap | mental-execution |
| Result is null or default unexpectedly | contextual-gap → error-silent-swallow | boundary-data-lifecycle |
| Test passes but output value is wrong | error-silent-swallow → mental-execution | sibling-guard-parity |
| DI resolves wrong type or NullReference | interface-resolution-trace → contextual-gap | mental-execution |
| OperationResult.IsSuccess=true but Value wrong | error-silent-swallow → boundary-data-lifecycle | mental-execution |
| Guard in ProjectIndexer but not IncrementalTrainer | sibling-guard-parity → contextual-gap | — |

---

## Rule 1 — Pattern Recognition

Before debugging code, identify which class of failure you have.

| Pattern | Shape | Starting rule |
|---------|-------|---------------|
| Wrong count | actual=N, expected=M, diff=1 | loop-off-by-one |
| Silent wrong value | no exception, wrong output | error-silent-swallow |
| Null or default everywhere | all outputs identical | contextual-gap |
| Wrong class predicted | ARCH predicted as BUG | boundary-data-lifecycle |
| Works in one method, broken in sibling | ProjectIndexer OK, DomainDatasetCache fails | sibling-guard-parity |

Anti-pattern: "The code looks correct so the failure must be in the test." The failure is in the code until proven otherwise.

---

## Rule 2 — Contextual Gap

The code assumes something is injected or set that wasn't.

```
Checklist:
☐ What interface does this service need? Is the stub/concrete providing it?
☐ Does the constructor receive all required parameters?
☐ Is ISemanticCompiler29 injected (not reimplemented) in ContinuousLearning services?
☐ Is ISessionStorage passed to BaseService via : base(storage)?
☐ Is the ICodeReader returning the expected file list in this test path?
☐ Is the stub returning a fixed domain — does the test assert on that exact domain?
```

Common gaps in this project:

| Missing context | Where needed | Symptom |
|----------------|-------------|---------|
| ISemanticCompiler29 not injected | ProjectIndexer | Domain always returns default CODE_Q |
| ISessionStorage null | BaseService subclass | NullReferenceException or Fail("Not persisted.") |
| ICodeReader returns empty list | ProjectIndexer.Index | Returns empty list, IsSuccess=true |
| StubSC29CompilerCL returns wrong domain | any ContinuousLearning test | Domain assertion fails |

---

## Rule 3 — Mental Execution

Wrong mental model. Trace the actual execution path, not the path you assume.

**Protocol:**
1. Write the exact input to the method you are debugging
2. At every `if` / `switch`, check the ACTUAL condition value — do not assume
3. At every `await`, trace what actually returns
4. At every `OperationResult<T>` boundary, check: is `.IsSuccess` true or false here?
5. At every method call, check: does the return type match what the caller expects?

Common mental execution traps in this project:

```csharp
// Trap: MapDomain returns "CODE_Q" for unknown domains — never null
private string MapDomain(SC29.Domain d) => d switch
{
    SC29.Domain.BUG  => "BUG",
    SC29.Domain.PLAN => "PLAN",
    _                => "CODE_Q",  // catches ARCH, REFACTOR, and unknown
};

// Trap: OperationResult<bool>.Value is bool (value type) — cannot be null
// !(result.IsSuccess && result.Value!)  ← correct
// result.Value ?? false                 ← WRONG — compiler error on value type
```

---

## Rule 4 — Boundary Data Lifecycle

Data changes shape or meaning as it crosses service and subproject boundaries.

```
Checklist:
☐ What type does the caller produce at the boundary?
☐ What type does the callee expect? Do they match exactly?
☐ Is it AgentInfrastructure.OperationResult<T> or SemanticCompiler29.OperationResult<T>?
☐ Is DpoPair the ContinuousLearning record or DPO.Models.DpoPair (different fields)?
☐ Does IReadOnlyList<T> need to be explicitly cast to the concrete type?
```

Common boundary type mismatches:

| Boundary | Wrong assumption | Correct rule |
|----------|-----------------|-------------|
| 29 → 14 | Same OperationResult<T> | SC29 vs AgentInfrastructure — never mix |
| DPO → ContinuousLearning | DpoPair is same record | DPO.Models.DpoPair has no Domain field; CL.DpoPair does |
| 22 → 15 | Same OperationResult<T> | AgentAlgorithms vs AgentInfrastructure — different assemblies |

---

## Rule 5 — Loop Off-By-One

Off-by-one errors in array indexing, batch processing, or count assertions.

```
Checklist:
☐ What is the expected first iteration? Does the loop start at 0 or 1?
☐ Does < vs <= include or exclude the last element?
☐ For .Take(n) / .Skip(n): is n the count or the last index?
☐ Is the loop modifying the collection it iterates?
☐ For count assertions: Assert.Equal(3, result.Count) — is 3 the right number?
```

Trace protocol: write out the first, second, and last iterations manually. The bug is almost always at the last iteration (fencepost error).

---

## Rule 6 — Error Silent Swallow

`IsSuccess` not checked — code reads `.Value!` on a failed result.

```csharp
// Bug: Value! on failed result → NullReferenceException
var result = cache.GetByDomain("BUG");
var count = result.Value!.Count; // if IsSuccess=false, Value is null → crash

// Fix: guard before access
var result = cache.GetByDomain("BUG");
if (!result.IsSuccess)
    return OperationResult<int>.Fail(result.ErrorMessage!, result.ErrorCode!);
var count = result.Value!.Count;
```

```
Checklist:
☐ Every OperationResult<T> consumer checks .IsSuccess before reading .Value
☐ No Fail() returned with empty ErrorCode string
☐ value-type T (bool, int): do NOT use ??  — use .Value! directly
☐ Any catch block that only logs but returns success = silent swallow → REJECT
☐ Any method returning Ok(defaultValue) after a Fail path = silent swallow → REJECT
```

---

## Rule 7 — Sibling Guard Parity

An IsSuccess check or validation exists in one service but not its sibling
that processes the same input type.

Common sibling pairs in this project:

| Sibling A | Sibling B | Guard that must be in both |
|-----------|-----------|--------------------------|
| ProjectIndexer.Index | DomainDatasetCache.Add | IsSuccess check on ICodeReader result |
| IncrementalTrainer | DomainModelRegistry | non-empty domain string guard |
| FeedbackRouter ACCEPTED path | FeedbackRouter REJECTED path | cache.Add called with correct domain |

```
Checklist:
☐ If service A checks IsSuccess on interface X, does service B also check it?
☐ If A validates Domain is non-empty, does B apply the same validation?
☐ If A returns EMPTY_EXAMPLES on 0 count, does B return a parallel error code?
```

---

## Rule 8 — Interface Resolution Trace

The wrong concrete type is resolved through an interface — usually a test stub
returning the wrong value, or a missing DI registration.

```
Checklist:
☐ Which concrete type does the test pass for ISemanticCompiler29?
☐ Is StubSC29CompilerCL returning the domain that the test asserts on?
☐ Is the ICodeReader stub returning the expected file list or an empty list?
☐ Does the stub match the interface signature exactly (same method names)?
☐ In production code: is BaseService registered before the subclass that inherits it?
```

Anti-pattern: "The interface is correct — it must be the algorithm." Check the stub implementation first. In this project, most test failures are caused by a stub returning the wrong value, not by the algorithm being wrong.
