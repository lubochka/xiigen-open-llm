# Bug-to-Tests — LlmFromScratch
## For: Every bug fix — tests BEFORE code

---

## The Iron Rule

> A test assertion change requires **Luba explicit approval**.
> Adapting a test to accept wrong output = **session failure**.

If a test is failing because the algorithm produces wrong output, the fix
goes in the **algorithm** — not in the `Assert` call.

---

## The Protocol

### Step 1 — Assign a Bug Identifier

```
BUG-LLM-NN: <one-sentence description of the wrong output>
CLASS:       A | B | C | D | E
SUBPROJECT:  <e.g., 29.SemanticCompiler, 22.AgentAlgorithms>
COMPONENT:   <class name>
EXPERIMENT:  <E-N or N/A>
REPORTED IN: <file:line or test name>
```

**Bug class quick reference:**
- A: OperationResult shape wrong (IsSuccess true when it should be false, or vice versa)
- B: Algorithm output wrong (correct shape, wrong values)
- C: DNA rule violation in production code (Rule 1–10)
- D: Fabric-first violation (concrete type used where interface required)
- E: Test isolation failure (test depends on another test's side effect)

**NN assignment:** Check existing test files for highest `BUG-LLM-NN` in comments.
Use next available number.

---

### Step 2 — Write 3 FAILING Tests (before any fix)

Write one test per level. Run each. Confirm it **FAILS** before proceeding.

#### Test 1 — Unit (isolated algorithm)

```csharp
// In: Tests/LlmFromScratch.Tests/[SubprojectName]Tests.cs
// Or a dedicated test class if the file is large

[Fact]
public void BugLlmNN_ComponentName_DescriptionOfWrongBehavior()
{
    // Arrange — minimal setup using Null* stubs
    var storage = new NullCompilerStorage();
    var logger  = new NullCompilerEventLogger();
    var component = new ComponentUnderTest(storage, logger);

    // Act
    var result = component.MethodBeingFixed("input that triggers the bug");

    // Assert — assert the CORRECT expected behavior, not what the bug produces
    // This MUST FAIL before the fix
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedCorrectValue, result.Value!.SpecificField);
}
```

**Run and confirm failure:**
```bash
dotnet test --filter "BugLlmNN" Tests/LlmFromScratch.Tests.csproj
# Expected: FAILED — 1 test(s) failed
# If it passes: the test is not reproducing the bug — re-examine the assertion
```

#### Test 2 — Integration (real caller chain)

```csharp
[Fact]
public void BugLlmNN_Integration_BugNotPresentInCallerChain()
{
    // Arrange — use the real constructor chain that a caller would use
    var pipeline = new SemanticCompilerPipeline(
        new AstTokenizer(new NullCompilerStorage(), new NullCompilerEventLogger()),
        new RoleAssigner(new NullCompilerStorage(), new NullCompilerEventLogger()),
        // ... full chain
    );

    // Act — use input that mirrors real usage
    var input   = new PipelineInput("validate null check in user service", null);
    var result  = pipeline.Compile(input);

    // Assert correct expected behavior — MUST FAIL before fix
    Assert.True(result.IsSuccess);
    Assert.Equal(Domain.BUG, result.Value!.Domain);
}
```

#### Test 3 — Regression (full suite anchor)

```csharp
[Fact]
public void BugLlmNN_Regression_BaselineHolds()
{
    // The simplest possible test that would catch a regression
    // after the fix is applied

    var component = new ComponentUnderTest(
        new NullCompilerStorage(),
        new NullCompilerEventLogger()
    );

    // Test the most basic valid input
    var result = component.MethodBeingFixed("valid baseline input");
    Assert.True(result.IsSuccess,
        $"BUG-LLM-{NN} regression: basic valid input must succeed after fix");
}
```

---

### Step 3 — Confirm All 3 Tests FAIL

```bash
dotnet test --filter "BugLlmNN" Tests/LlmFromScratch.Tests.csproj --verbosity normal
```

Expected before fix:
```
Failed  BugLlmNN_ComponentName_DescriptionOfWrongBehavior
Failed  BugLlmNN_Integration_BugNotPresentInCallerChain
Failed  BugLlmNN_Regression_BaselineHolds
Failed! - Failed: 3, Errors: 0, Passed: 0
```

If any test PASSES before the fix: that test does not reproduce the bug.
Re-examine the assertion. A passing "reproduction" test is not protection.

---

### Step 4 — Fix the Algorithm (NOT the tests)

Apply the fix to the production code:
- Algorithm: `NN.SubprojectName/Algorithms/ComponentName.cs`
- Core pattern: `NN.SubprojectName/Core/Core.cs`
- Interface: `NN.SubprojectName/Fabric/Interfaces.cs`

**Never fix:**
- The test's `Assert` call (without Luba approval)
- A different file that is not the root cause

---

### Step 5 — Confirm All 3 Tests PASS

```bash
dotnet test --filter "BugLlmNN" Tests/LlmFromScratch.Tests.csproj --verbosity normal
```

Expected after fix:
```
Passed  BugLlmNN_ComponentName_DescriptionOfWrongBehavior
Passed  BugLlmNN_Integration_BugNotPresentInCallerChain
Passed  BugLlmNN_Regression_BaselineHolds
Passed! - Failed: 0, Errors: 0, Passed: 3
```

---

### Step 6 — Zero-Regression Gate

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Passed count must be ≥ session-start baseline + 3 (the 3 new tests)
```

---

## Bug Registry Format

Track every bug as a comment block in the test file:

```csharp
// BUG-LLM-NN: <description>
// CLASS: B
// SUBPROJECT: 29.SemanticCompiler
// COMPONENT: FamilyCollapser
// ROOT CAUSE: Stop-word "get" filtered before role assignment; both segments collapse to "coder"
// WRONG ASSUMPTION: stop-word filtering commutes with role assignment (it does not)
// FIX: FamilyCollapser.cs — apply role assignment before stop-word filtering
// DNA RULES AFFECTED: none
// STATUS: FIXED in commit abc123
```

---

## Anti-Patterns

### Fix before writing tests
```
BUG found → fix applied → "write a test to cover it"
```
**Why wrong:** The test is written to match the fix. It proves the fix exists,
not that the bug was real. It cannot fail on a regression.

### Changing the assertion only
```csharp
// Bug: result.Value!.Domain == Domain.CODE_Q but should be Domain.BUG
// "Fix": Assert.Equal(Domain.CODE_Q, result.Value!.Domain) // changed to accept bug output
```
**What to do:** Fix the RouteClassifier so it correctly classifies the input as
`Domain.BUG`. Then Assert.Equal(Domain.BUG, ...) passes naturally.

### Writing only the unit test
```
L1: written, failing ✓
L2: "unit covers it"
L3: "too much setup"
```
**Why wrong:** L2 failures catch mock-assumption errors. L3 failures catch
composition regressions. See three-level-verification-SKILL.md.
