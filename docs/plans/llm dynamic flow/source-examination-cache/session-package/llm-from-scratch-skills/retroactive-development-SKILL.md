# Retroactive Development — LlmFromScratch
## For: Bug in algorithm → fix the source, not the output

---

## The Core Rule

**Bug in algorithm output → fix the algorithm, not a downstream workaround.**

An algorithm's output is the result of its implementation. Patching the output
without fixing the algorithm means the next run of the same algorithm produces
the same bug.

---

## When to Invoke

- Any bug found in the output of an algorithm that feeds a composite pipeline
- Any DNA violation found in an existing algorithm (Rule 1–10)
- After any algorithm fix: to re-validate all subprojects that call the same
  algorithm or share the same base class

---

## Fix Propagation Protocol

### Step 1: Identify the source

```
Algorithm output has a wrong value
  ↓
Which algorithm produced this output?
  Direct algorithm → fix the algorithm class directly
  Composite pipeline → identify which stage in the pipeline is wrong
    (trace back through OperationResult<T> chain — find first failure)
  Shared base class → fix BaseService or Core.cs, then verify all subprojects
```

### Step 2: Fix the source

| Bug type | Fix location |
|----------|-------------|
| Rule 1 violation (raw return type) | Algorithm class — change return type to `OperationResult<T>` |
| Rule 2 violation (direct instantiation) | Constructor — inject interface instead |
| Algorithm produces wrong value | Algorithm class logic |
| Wrong pipeline stage order | Pipeline composition class |
| BaseService method bug | `NN.SubprojectName/Core/Core.cs` — BaseService |
| Shared interface contract wrong | `NN.SubprojectName/Fabric/Interfaces.cs` |

### Step 3: Re-validate all callers

```bash
# Find all callers of the fixed algorithm
grep -rn "FixedClassName\|FixedMethodName" . --include="*.cs" | grep -v "/bin/\|/obj/"

# For each calling subproject: run its tests
dotnet test --filter "CallerSubprojectTests" Tests/LlmFromScratch.Tests.csproj

# Run the full suite to confirm nothing is broken
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

### Step 4: Verify the fix is at the source, not cosmetic

**Test:** Re-run the algorithm on the input that triggered the bug.
The algorithm must now produce correct output directly — not because
a wrapper catches and corrects it.

If the algorithm still produces wrong output and only appears fixed because
of a downstream transformation → the fix is cosmetic, not real.

---

## The Iron Rule

**"Adapt test to accept wrong output" = session failure.**

If a test fails because the algorithm produces wrong output, the options are:
1. ✅ Fix the algorithm → test passes with correct output
2. ❌ Change the `Assert` to accept the wrong output → REJECT

Changing an assertion requires Luba's explicit approval.

---

## Blast Radius After Algorithm Fix

When any algorithm, interface, or BaseService changes:

```
Change in AstTokenizer    → affects SemanticNlpPipeline, SemanticCompilerPipeline
Change in BaseService     → affects EVERY class inheriting BaseService across ALL subprojects
Change in OperationResult → affects EVERY public method in EVERY subproject
Change in ISessionStorage → affects EVERY class injecting ISessionStorage
```

For each affected subproject, run:
```bash
dotnet test --filter "SubprojectTests" Tests/LlmFromScratch.Tests.csproj
```

If any subproject tests fail: fix them before marking the original fix complete.

---

## Anti-Patterns

1. **"I fixed the output directly."** The algorithm will produce the same wrong
   output next time. Fix the algorithm.

2. **"Only the one failing test needed fixing."** If the algorithm is wrong,
   ALL inputs that hit the same code path may produce wrong output.
   Run the full subproject test suite.

3. **"The DNA violation is minor, I'll note it."** DNA violations compound.
   A missing `OperationResult<T>` on one method becomes a pattern.
   Fix it now.

4. **"I'll fix the source algorithm after Phase N."** Retroactive fixes
   compound. The longer an algorithm bug exists, the more downstream
   code relies on the wrong output.

---

## Integration

```
retroactive-development
  → triggered by: dna-compliance-guard (violation in existing code)
  → uses: bug-to-tests (3 failing tests before fix)
  → verifies with: three-level-verification (unit + integration + full suite)
  → re-validates with: self-verification Rule 4 (cascade check)
```
