# Three-Level Verification — LlmFromScratch
## For: Algorithm fix verification — Unit + Integration + Full suite

---

## The Problem This Skill Solves

A fix to an algorithm class or fabric interface can pass all unit tests and
still break at integration or full-suite level. This happens because unit tests
use `NullSessionStorage` and `NullEventLogger` stubs that accept anything —
the stub insulates the test from the real failure path.

Three failure scenarios in LlmFromScratch:

1. **Unit passes, integration fails**: The fix works in isolation but breaks
   when the algorithm is used by a real dispatcher or pipeline. The stub
   accepted inputs that real callers do not produce.

2. **Integration passes, full suite fails**: The integration test used a
   synthetic input. The real codebase has edge cases (empty strings, boundary
   values, specific domain combinations) that the synthetic input never covered.

3. **All pass locally, subproject composition fails**: The fix works for
   the component but breaks when another subproject calls it through the
   fabric interface.

**This skill prevents declaring a fix done before all 3 levels confirm it.**

---

## The 3-Level Protocol

Run in sequence. Stop at the first level that fails.

### Level 1 — Unit Test

**Command:**
```bash
dotnet test --filter "ClassName" Tests/LlmFromScratch.Tests.csproj --verbosity normal
# Or: dotnet test --filter "FullyQualifiedName~BugDescription" ...
```

**What passes Level 1:**
- The specific test class for the changed algorithm
- 0 failures, 0 errors
- The test asserts on the specific output path that was fixed — not just
  `Assert.True(result.IsSuccess)` but the actual value

**What fails Level 1:**
- Any test failure in the specific test class
- Test passes only because a `Null*` stub bypasses the fixed path entirely

**Stop rule:** If Level 1 fails, fix the algorithm. Re-run Level 1.

---

### Level 2 — Integration (real callers)

**Command:**
```bash
# Run the tests for the subproject that CALLS the fixed algorithm
dotnet test --filter "CallerSubprojectTests" Tests/LlmFromScratch.Tests.csproj
```

**What passes Level 2:**
- Test for the calling subproject uses a real constructor chain
  (e.g., `new SemanticCompilerPipeline(new AstTokenizer(...), ...)`)
- No hand-crafted minimum inputs — use inputs that mirror real usage
- The fix holds when the algorithm receives inputs from its actual caller

**What fails Level 2:**
- Test fails with real-caller inputs even though unit test passed
- `OperationResult<T>.IsSuccess` is false on a path that should succeed
- Output shape mismatch between what the fixed algorithm produces and what
  the caller expects

**Stop rule:** If Level 2 fails, the Level 1 stub was masking the real problem.
Re-examine the fix. A Level 2 failure after Level 1 pass = stub was too permissive.

---

### Level 3 — Full Suite

**Command:**
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: Passed! — Failed: 0, Errors: 0, Passed: N (≥ session-start baseline)
```

**What passes Level 3:**
- Full suite passes at or above the session-start baseline
- The fix does not break any other subproject
- Test count equals or exceeds baseline (no tests were accidentally removed)

**What fails Level 3:**
- Any test outside the fixed subproject now fails
- Test count dropped (a test file was accidentally modified or deleted)
- A subproject that shares a base class with the fixed component now breaks

**Stop rule:** If Level 3 fails after Levels 1 and 2 passed, there is a
composition problem — the fix is correct in isolation but breaks shared
infrastructure. Look at `BaseService` or shared `Core.cs` patterns.

---

## When to Invoke

**Mandatory:**
- After ANY fix to an algorithm class in any subproject
- After ANY change to a `Core.cs` file (OperationResult, BaseService, interfaces)
- After ANY `Fabric/Interfaces.cs` change
- Before declaring any bug fix complete
- Before phase-end gate (one commit per phase — Rule 6)

**Optional but recommended:**
- After adding new test fixtures
- After refactoring that does not change observable output

---

## Quick Routing Table

| What was fixed | Level 1 filter | Level 2 filter | Level 3 |
|----------------|---------------|----------------|---------|
| Algorithm in `29.SemanticCompiler` | `SC29_` | `SemanticNlpTests` (caller) | Full suite |
| `BaseService` in any `Core.cs` | Subproject unit tests | All subprojects using BaseService | Full suite |
| `OperationResult<T>` | Subproject that changed it | All subprojects (every one uses it) | Full suite |
| `ISemanticStorage` implementation | `SemanticNlpTests` | Integration with actual algorithm | Full suite |
| Experiment algorithm (E-N) | `ExperimentsV2Tests` | Dependent experiment tests | Full suite |
| `ReactAgentLoop` | `AgentAlgorithmsTests` | `AgentInfrastructureTests` | Full suite |

---

## Anti-Patterns

### Declaring done after Level 1 only
```
✗ "All unit tests pass" → phase complete
✓ Run Level 2 before declaring done — stubs may be too permissive
```

### Using minimal synthetic inputs for Level 2
```
✗ Level 2 test uses: new PipelineInput("x", null)
✓ Level 2 test uses: new PipelineInput("validate null check in loop body", null)
   (mirrors actual Claude Code prompt that triggered the bug)
```

### Skipping Level 3 because "the full suite is slow"
```
✗ Level 1+2 pass → skip Level 3
✓ Level 3 is: dotnet test 2>&1 | tail -5 — takes seconds, not minutes
```

### Level 3 without checking count
```
✗ "Level 3 passed" without verifying count ≥ baseline
✓ Passed: 877 ≥ 877 baseline → confirmed. Or: Passed: 874 → BLOCKED (3 tests missing)
```

---

## Integration with Other Skills

```
bug-to-tests:           writes 3 FAILING tests → three-level-verification confirms all 3 pass
test-integrity:         Level 2 uses real constructor chains, not NullStorage everywhere
retroactive-development: Level 3 re-validates all subprojects using the same BaseService
dna-compliance-guard:   Level 1 unit test includes DNA rule check (OperationResult on every public method)
```

---

## One-Line Summary

> **A fix is done when unit, integration, AND full suite all pass — not when
> the isolated unit test alone goes green.**
