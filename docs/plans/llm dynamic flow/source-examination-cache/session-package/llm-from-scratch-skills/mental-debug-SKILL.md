# Mental Debug — LlmFromScratch
## For: C# algorithm debugging

---

## When to Invoke

- When a test fails and the failure message points to a symptom, not a cause
- When algorithm output looks correct but produces wrong values
- After `code-examination` has mapped the call chain and a hypothesis is formed

---

## The 4 Debug Moves (apply in order)

### Move 1 — Pattern Recognition

What shape does the failure have?

| Failure shape | Most likely cause |
|---------------|------------------|
| `result.IsSuccess` is false with empty ErrorMessage | Exception caught silently, `Fail("", "ERROR")` returned |
| `result.Value` is null when IsSuccess is true | Factory method has bug — Ok(null) |
| Wrong count from algorithm | Off-by-one in loop or wrong collection operation |
| Zero accuracy / all predictions same class | Data collapse — class imbalance or bad seed |
| Negative delta (enhanced worse than baseline) | Wrong metric direction or baseline is too strong |
| `NullReferenceException` in test | `result.Value` accessed without checking `IsSuccess` first |
| Compile error on `result.Value!.Field` | `!` used on null — check `IsSuccess` before accessing Value |

---

### Move 2 — Contextual Gap

List what the failing code NEEDS vs what it RECEIVES.

```csharp
// Example: FamilyCollapser.Collapse(IReadOnlyList<string> stemmedWords)
// NEEDS: stemmed words (stop-words already removed, all lowercase)
// RECEIVES: what does SemanticNormalizer.Normalize() actually produce?
```

Find the first line where NEEDS ≠ RECEIVES:

```bash
# Trace the output of the upstream method
grep -n "return OperationResult" NN.SubprojectName/Algorithms/UpstreamMethod.cs
```

---

### Move 3 — Mental Execution

Run the method in your head with the **smallest failing input**.

```
Input: "get the value"

AstTokenizer.Tokenize("get the value")
→ tokens: ["get", "the", "value"]

SemanticNormalizer.Normalize(["get", "the", "value"])
→ removes stop-words: "get", "the" are stop-words
→ result: ["value"]          ← "get" removed here!

FamilyCollapser.Collapse(["value"])
→ "value" → no match → UNKNOWN_OP
```

The bug: stop-word removal happens BEFORE role assignment needed "get" to
identify the operation role. E20 pipeline order rule violated.

Check every branch, every index, every condition.

---

### Move 4 — Data Lifecycle

Trace the variable from creation to assertion.

```csharp
// Where is 'tokens' created?
var tokens = tokenizer.Tokenize(source).Value!;  // line 12

// What transforms it?
var normalized = normalizer.Normalize(tokens).Value!;  // line 15 — TRANSFORMS tokens

// What is asserted?
Assert.Contains("VALIDATE_OP", collapser.Collapse(normalized).Value!);  // line 20
                                                          ↑ uses normalized, not tokens
```

Find the first step where the assumption breaks.

---

## LlmFromScratch-Specific Failure Patterns

### Pattern: OperationResult.IsSuccess false with no error message

```csharp
// In the algorithm
try { ... }
catch (Exception) { return OperationResult<T>.Fail("", "ERROR"); }  // swallowed message
```

Fix: always include the exception message:
```csharp
catch (Exception ex) { return OperationResult<T>.Fail(ex.Message, "UNEXPECTED"); }
```

### Pattern: Null value accessed with `!` operator

```csharp
// In test
var result = tokenizer.Tokenize("input");
var tokens = result.Value!.Length;  // NullReferenceException if IsSuccess is false
```

Fix: always check `IsSuccess` before accessing `Value`:
```csharp
Assert.True(result.IsSuccess, result.ErrorMessage);
Assert.True(result.Value!.Length > 0);
```

### Pattern: Constructor injection gone wrong

```csharp
// Service has two constructors, wrong one called in test
var service = new MyService(new NullStorage());  // uses BaseService(IXxxStorage)
// But algorithm requires: new MyService(new NullStorage(), new NullLogger())
// → second parameter defaults to null → NullReferenceException in EventLogger.Log()
```

Fix: read the constructor signature before instantiating in tests.

### Pattern: E20 pipeline order violation

```
Error: StackDetector (E20) must run BEFORE SemanticNormalizer
If normalized words go into StackDetector instead of raw source → TypeScript/C# markers stripped
```

Check pipeline composition class for correct stage ordering.

### Pattern: Experiment scope leak across tests

```csharp
// Test A modifies shared state
static int _counter = 0;

// Test B assumes _counter starts at 0, but Test A ran first
```

Fix: each test creates its own instance. Never share mutable state across tests.

---

## Diagnostic Routing

| Symptom | Apply first | Then |
|---------|-------------|------|
| Algorithm output has wrong value | Move 2 (contextual gap) | Move 3 (mental execution) |
| `IsSuccess` false, empty ErrorMessage | Move 1 (pattern: swallowed exception) | Read catch blocks |
| Test passes but wrong value asserted | Move 1 (pattern: assertion too weak) | Move 4 (data lifecycle) |
| NullReferenceException on `result.Value!` | Move 1 (check IsSuccess first) | Move 2 |
| Count off by one | Move 3 (mental execution with boundary) | Move 4 |
| Integration test fails, unit passes | Move 2 (what does the real caller produce?) | three-level-verification |
