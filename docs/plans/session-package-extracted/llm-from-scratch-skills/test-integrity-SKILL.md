# Test Integrity — LlmFromScratch
## For: xUnit · C# · algorithm verification

---

## Core Principle

**A passing test confirms an assertion holds. It does not confirm that the
branch you care about actually executed.**

These two things come apart whenever:
- A `Null*` stub is changed to skip a DNA check (the guard is never reached)
- A guard is stacked on another guard, making a downstream branch unreachable
- An integration test is added but uses only `NullStorage` (the persistence path never runs)
- A DNA rule check exists in one algorithm but is missing in its sibling algorithm

---

## The Four Failure Modes

| Mode | Trigger | Symptom | Rule |
|------|---------|---------|------|
| **Mock-Fix Masking** | `Null*` stub changed to skip OperationResult check | Test passes, guard never runs | `test-fix-or-code-fix` |
| **Guard Stacking** | Two checks guard the same condition | Downstream branch unreachable | `branch-reachability` |
| **Coverage Inflation** | Integration tests added, all use `NullStorage` | Test count up, real persistence path: 0% | `coverage-vs-execution` |
| **Sibling Guard Gap** | Rule 1 guard in `AstTokenizer`, missing in `RoleAssigner` | Guard applied inconsistently | `sibling-parity` |

---

## When to Invoke

- A test fails after a guard condition is tightened in an algorithm
- A `Null*` stub is modified (not just created) to make a test pass
- A phase ends with "all tests pass" and at least one test was modified
- An integration test is added but uses only `Null*` stubs for everything
- Rule 1 (`OperationResult<T>`) guard appears in one algorithm but not its
  sibling algorithm in the same namespace

---

## Rule: test-fix-or-code-fix

Before modifying any existing `Assert` or `Null*` stub setup:

1. Identify WHY the test is failing
2. Is the test failing because:
   a. The algorithm has a bug → fix the algorithm, not the test
   b. The test was wrong about expected behavior → requires Luba approval to change
   c. The test setup was wrong → fix the setup, but verify it still fails before the fix

Changing `new NullSessionStorage()` to a stub that returns a specific error to make
a test pass → **stop**. If the production code path requires specific storage behavior,
inject a real stub that implements the interface correctly.

---

## Rule: branch-reachability

After adding any guard condition (e.g., `if (!result.IsSuccess) return result.MapError<T>()`):

```bash
# Find all callers of the method you just changed
grep -rn "MethodName" . --include="*.cs" | grep -v "/bin/\|/obj/\|//\|Test"
```

For each caller: does the test suite exercise the branch where the new guard fires?

```bash
# Check test coverage of the specific branch
dotnet test --filter "ComponentTests" Tests/LlmFromScratch.Tests.csproj --verbosity diagnostic 2>&1 | \
  grep -A2 "GuardedMethod"
```

If the error branch of a guard has no test:
→ Add a test with input that causes the guard to fire
→ Confirm `result.IsSuccess == false` and `result.ErrorCode` is the expected code

---

## Rule: coverage-vs-execution

After adding integration tests:

```csharp
// ❌ Coverage inflation — all stubs, no real execution path
[Fact]
public void Integration_PipelineRunsSuccessfully()
{
    var pipeline = new SemanticCompilerPipeline(
        new NullAstTokenizer(),     // stub
        new NullRoleAssigner(),     // stub
        new NullSemanticNormalizer() // stub
    );
    var result = pipeline.Compile(new PipelineInput("test", null));
    Assert.True(result.IsSuccess);
    // This tests that the pipeline CALLS the components, not that they WORK
}

// ✅ Real execution — at least one component is real
[Fact]
public void Integration_AstTokenizerInRealPipeline()
{
    var pipeline = new SemanticCompilerPipeline(
        new AstTokenizer(new NullCompilerStorage(), new NullCompilerEventLogger()), // REAL
        new NullRoleAssigner(),
        new NullSemanticNormalizer()
    );
    var result = pipeline.Compile(new PipelineInput("validate null check", null));
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.Value!.Tokens);
    // This tests that AstTokenizer actually tokenizes the input
}
```

Rule: at least one component in an integration test must be the real implementation.

---

## Rule: sibling-parity

When `OperationResult<T>` guard is added to one algorithm in a namespace,
check all sibling algorithms in the same namespace:

```bash
# Find all public methods in the same namespace that might need the same guard
grep -rn "public OperationResult" NN.SubprojectName/Algorithms/ --include="*.cs"

# For each method: does it have the same guard?
# Example: if AstTokenizer checks for empty input, RoleAssigner should too
grep -rn "IsNullOrEmpty\|NullOrWhiteSpace" NN.SubprojectName/Algorithms/ --include="*.cs"
```

Sibling algorithms that process the same type of input should have consistent
error handling.

---

## Quick Routing Table

| What just happened | Apply first | Then |
|-------------------|-------------|------|
| Test fails after guard tightened | `test-fix-or-code-fix` | `branch-reachability` |
| Null stub changed to make test pass | `test-fix-or-code-fix` | `coverage-vs-execution` |
| Two guards check same input path | `branch-reachability` | `coverage-vs-execution` |
| Test count up, but only Null stubs used | `coverage-vs-execution` | `branch-reachability` |
| Rule 1 guard in tokenizer not in normalizer | `sibling-parity` | `coverage-vs-execution` |

---

## Anti-Patterns

### Changing assertions to accept wrong output
```csharp
// Bug: Domain.BUG is classified as Domain.CODE_Q
// Wrong fix:
Assert.Equal(Domain.CODE_Q, result.Value!.Domain); // changed to accept the bug
// Right approach: fix RouteClassifier, assertion stays as Domain.BUG
```

### All integration tests use all-Null stubs
```
+6 integration tests
All 6: NullStorage, NullLogger, NullAnnotator
Test count: +6. Real algorithm execution in integration context: +0
```

### Guard in one sibling, not others
```csharp
// AstTokenizer.Tokenize() — checks empty input ✅
// RoleAssigner.AssignRoles() — no empty check ❌ — same failure mode possible
// SemanticNormalizer.Normalize() — no empty check ❌ — same
```
