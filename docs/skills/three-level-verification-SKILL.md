# three-level-verification-SKILL.md — Verify a fix at three levels before claiming done

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
## v1.0: translated from xiigen (NestJS Unit+Simulation+Contract → C# Unit+Integration+Suite)

## When to load
Load after any fix to a service or algorithm.
Load before declaring any bug fix complete.
Load before phase-end gate.

---

## The problem this skill solves

A fix can pass all unit tests and still break at a higher level. This happens
because unit tests use stubs that don't exercise the real code paths.

Three failure scenarios:

1. **Unit passes, integration fails**: The fix works in isolation but breaks
   when real dependencies are wired together. The stub was too permissive
   and accepted inputs the real type doesn't produce.

2. **Integration passes, suite fails**: The integration test used specific
   inputs. The full test suite covers edge cases (empty domain, zero examples,
   OperationResult namespace mismatch) that the narrow integration test missed.

3. **All pass locally but the wrong thing was fixed**: The fix corrects the
   test-visible output but leaves the root cause in place. A different test
   added later will expose it.

A fix is done when all three levels confirm it — not when the unit test alone goes green.

---

## The 3-Level Protocol

Run in sequence. Stop at the first level that fails — do not skip ahead.

### Level 1 — Unit Test (xUnit [Fact] with stubs)

**Command:**
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~<ClassName>"
```

**What passes Level 1:**
- The specific [Fact] for the fixed behaviour
- 0 test failures
- The assertion covers the specific output path that was fixed, not just IsSuccess

**What fails Level 1:**
- Any test failure in the specific test class
- Test passes only because the stub is intercepting (bypassing) the fixed path

**Stop rule:** If Level 1 fails, fix the unit. Re-run Level 1. Do not proceed.

---

### Level 2 — Integration Test (multiple real classes, no stubs for the fixed path)

Write a [Fact] or [Theory] that wires real classes together for the fixed code path.
No stubs on the interfaces involved in the fix — use real implementations.

```csharp
// Example: Level 2 integration test — real FeedbackRouter + real DomainDatasetCache
[Fact]
public void FeedbackRouter_Accepted_IntegrationWithRealCache_CountsIncrease()
{
    var storage = new NullSessionStorage();
    var writer  = new NullCodeWriter();
    var cache   = new DomainDatasetCache(storage, writer, "test");  // real
    var router  = new FeedbackRouter(storage, cache);               // real

    router.Route(new FeedbackEvent("prompt", "response", null, FeedbackSignal.Accepted, "BUG"));

    Assert.Equal(1, cache.TotalCount().Value!);  // real cache, real count
}
```

**What passes Level 2:**
- Real classes wired together for the fixed path
- The fix holds when the service receives real-shaped inputs (not stub-shaped ones)
- OperationResult shape is correct throughout the chain

**What fails Level 2:**
- Test fails with real inputs even though unit stub test passed
- OperationResult.Fail is returned on a path that should succeed
- Field name mismatch between what one service produces and what the next expects

**Stop rule:** If Level 2 fails, the unit stub was masking the real problem.
A Level 2 failure after a Level 1 pass = the stub was too permissive.

---

### Level 3 — Full Suite (all tests in the project)

**Command:**
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

**What passes Level 3:**
- 0 failures
- Count >= session-start baseline (currently 916 as of 2026-05-01)
- No test that was passing before the fix is now failing

**What fails Level 3:**
- Any test failure
- Count decrease (a test was accidentally broken or deleted)

**Stop rule:** If Level 3 fails after Levels 1 and 2 passed, there is a
side-effect from the fix. The fix is correct for the target behaviour but
breaks something else. Find the newly failing test and fix the root cause.

---

## When to invoke

Mandatory:
- After any fix to a service class (any file in a subproject's non-Test directory)
- After any change to a BaseService subclass constructor
- After any OperationResult<T> change that affects a public method signature
- Before declaring any bug fix complete
- Before phase-end gate (OP-6 commit)

Optional but recommended:
- After adding new test stubs to a test file
- After changing any error code string (verify no test asserts on the old code)
- After modifying a record type that crosses subproject boundaries

---

## Quick routing table

| What was fixed | Level 1 spec | Level 2 spec | Level 3 command |
|---------------|-------------|-------------|-----------------|
| FeedbackRouter route logic | FeedbackRouter [Fact] test | Real FeedbackRouter + real DomainDatasetCache | dotnet test (full suite) |
| ProjectIndexer domain mapping | ProjectIndexer [Fact] test | Real ProjectIndexer + real ICodeReader stub | dotnet test (full suite) |
| IncrementalTrainer empty guard | IncrementalTrainer [Fact] test | Real trainer + real config | dotnet test (full suite) |
| OperationResult<T> method signature | All [Fact] tests for that service | Caller+callee wired together | dotnet test (full suite) |
| BaseService constructor | Constructor [Fact] test | Subclass integration | dotnet test (full suite) |

---

## Anti-patterns

### Anti-pattern 1: Declaring done after Level 1 only
```
✗ "The specific [Fact] passes" → phase complete
✓ Run Level 2 (integration) before declaring done — stubs may be too permissive
```

### Anti-pattern 2: Skipping Level 2 because "it takes longer"
```
✗ Level 1 passes → skip to Level 3
✓ Level 2 catches stub-masking failures that Level 3 cannot diagnose as quickly
```

### Anti-pattern 3: Level 2 with the same stubs as Level 1
```
✗ Level 2 test uses NullSessionStorageCL (same as Level 1) and never tests the fixed path
✓ Level 2 uses real classes for the components involved in the fix
```

### Anti-pattern 4: Trusting Level 3 without Level 2
```
✗ Full suite passes → fix must be correct
✓ Full suite passing only means no regression — it does not confirm the fix is correct
   Level 2 provides the proof of correctness for the specific scenario
```
