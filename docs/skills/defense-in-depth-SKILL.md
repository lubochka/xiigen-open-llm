# Defense-in-Depth Validation

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

## Core Principle

When you fix a bug caused by invalid data, adding validation at one place feels
sufficient. But that single check can be bypassed by different code paths,
refactoring, or test mocks.

**Single validation: "We fixed the bug"**
**Multiple layers: "We made the bug structurally impossible"**

---

## When to Invoke

- After root-cause-tracing-SKILL.md identifies the source of invalid data
- When a bug was caught only at a deep call site (symptom was far from cause)
- After any fix to a method that receives untrusted or externally-provided input
- When adding a new service that crosses a system boundary (user input, file I/O, IPC)

---

## The Four Layers

### Layer 1: Entry Point Validation (system boundary)

Reject obviously invalid input at the first point it enters the system.

```csharp
// ✅ Layer 1: reject at system boundary
public OperationResult<SessionState> LoadSession(string sessionId, string storagePath)
{
    if (string.IsNullOrWhiteSpace(sessionId))
        return OperationResult<SessionState>.Fail("sessionId cannot be empty", "INVALID_SESSION_ID");

    if (!Directory.Exists(storagePath))
        return OperationResult<SessionState>.Fail($"storagePath does not exist: {storagePath}", "INVALID_PATH");
    // ... proceed
}
```

### Layer 2: Business Logic Validation

Ensure data makes sense for the specific operation being performed.

```csharp
// ✅ Layer 2: validate invariants at the business logic level
public OperationResult<SearchResult> Search(float[] query, int topK)
{
    if (query == null || query.Length == 0)
        return OperationResult<SearchResult>.Fail("query vector cannot be empty", "EMPTY_QUERY");

    if (topK <= 0)
        return OperationResult<SearchResult>.Fail($"topK must be positive, got {topK}", "INVALID_TOPK");

    if (query.Length != _expectedDimensions)
        return OperationResult<SearchResult>.Fail(
            $"query dimension {query.Length} does not match index dimension {_expectedDimensions}",
            "DIMENSION_MISMATCH");
    // ... proceed
}
```

### Layer 3: Environment Guards

Prevent dangerous operations in specific contexts (test vs production).

```csharp
// ✅ Layer 3: refuse destructive operations outside safe context
public OperationResult<bool> ClearAllSessions(string storagePath)
{
    // In test environments, refuse to clear paths outside the test temp directory
    var testRoot = Environment.GetEnvironmentVariable("TEST_TEMP_ROOT");
    if (testRoot != null && !storagePath.StartsWith(testRoot))
        return OperationResult<bool>.Fail(
            $"Refusing to clear sessions outside test temp root during testing: {storagePath}",
            "UNSAFE_TEST_OPERATION");
    // ... proceed
}
```

### Layer 4: Debug Instrumentation

Capture context for forensics when other layers fail.

```csharp
// ✅ Layer 4: log context at the call site for forensic analysis
public async Task<OperationResult<bool>> SaveAsync(string id, object state)
{
    _logger.LogDebug("SaveAsync called: id={Id}, stateType={Type}, callerStack={Stack}",
        id, state?.GetType().Name, new StackTrace().ToString());
    // ... proceed
}
```

---

## Applying the Pattern

When you find a bug that reached a deep call site:

1. **Trace the data flow** — where did the bad value originate? Where was it used?
2. **Map all checkpoints** — list every point the data passes through
3. **Add validation at each layer** — entry, business logic, environment, debug
4. **Test each layer independently** — verify that bypassing Layer 1 is caught by Layer 2

---

## .NET-Specific Examples

Bug: `null` session ID caused `NullReferenceException` deep in `SessionStateStore`.

**Data flow:**
1. Test setup → `string sessionId = null`
2. `HybridRetriever.SearchAsync(query, sessionId)`
3. `ISessionStorage.SaveAsync(sessionId, result)`
4. `File.WriteAllText(Path.Combine(root, sessionId + ".json"), ...)` → throws

**Four layers added:**
- Layer 1: `ISessionStorage.SaveAsync()` validates `id` is not null/empty
- Layer 2: `HybridRetriever` validates `sessionId` not null before calling storage
- Layer 3: If `TEST_TEMP_ROOT` is set, refuse paths outside it (prevents accidental
  production writes from test code)
- Layer 4: Log parameters at `SaveAsync` entry including call stack frame

**Result:** Bug is structurally impossible — it would be caught at Layer 1 before
reaching Layer 4.

---

## OperationResult Pattern Enforcement

Every layer returns `OperationResult<T>` — never throws (Rule 1). The check at
each layer is:

```csharp
// ✅ validation returns OperationResult.Fail, never throws
if (invalidCondition)
    return OperationResult<T>.Fail("reason", "ERROR_CODE");
```

A validation layer that throws instead of returning `Fail` is a DNA Rule 1 violation.

---

## Integration

- Load after root-cause-tracing-SKILL.md identifies the bug source
- Use blast-radius-SKILL.md to find all callers before adding Layer 1 validation
- Each validation layer produces one test case (bug-to-tests-SKILL.md: one failing
  test per layer before adding the check)
