# Root Cause Tracing

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## Overview

Bugs often manifest deep in the call stack. Fixing where the error appears treats a symptom.

**Core principle:** Trace backward through the call chain until you find the original trigger,
then fix at the source.

## When to Use

- Error appears in a method that is not the entry point
- Stack trace shows more than 2 levels
- Unclear where invalid data originated
- A test fails but the test code looks correct

---

## The Tracing Process

### 1. Observe the Symptom

```
Error: IndexOutOfRangeException in BruteForceIndex.Search()
  at Src/09.VectorSearch/BruteForceIndex.cs:45
  dimensions mismatch: expected 128, got 0
```

### 2. Find Immediate Cause

What line directly causes this?

```csharp
// Line 45: comparing query vector length to stored index dimensions
if (query.Length != _dimensions)
    // ← query.Length is 0; empty float[] was passed
```

### 3. Ask: What Called This?

```bash
grep -rn "BruteForceIndex\|IVectorIndex.*Search" Src/ --include="*.cs"
```

```
BruteForceIndex.Search() ← called by HybridRetriever.SearchAsync()
HybridRetriever.SearchAsync() ← called by AgentOrchestrator.SearchMemoryAsync()
AgentOrchestrator.SearchMemoryAsync() ← called by Session.RecallAsync()
Session.RecallAsync() ← called by test at AgentOrchestratorTests.cs:67
```

### 4. Keep Tracing Up

What value was passed at each level?

```csharp
// AgentOrchestratorTests.cs:67
var result = await orchestrator.SearchMemoryAsync(context.EmbeddedQuery);

// context.EmbeddedQuery is float[] — where was it set?
// context is created at line 42:
var context = SetupSearchContext(); // returns { EmbeddedQuery: new float[0] }

// SetupSearchContext() returns empty array because it runs
// before the embedding step that populates it
```

### 5. Find Original Trigger

```
Root cause: SetupSearchContext() accessed EmbeddedQuery before the embedding
  step populated it. The float[] is initialized as empty and never filled.

Fix location: Either:
  A. SetupSearchContext() should throw if EmbeddedQuery is empty (fail fast)
  B. The test should run the embedding step before creating the context
```

---

## Adding Instrumentation

When manual tracing fails, add debug output before the problematic operation:

```csharp
// In BruteForceIndex.Search() — log before the operation that fails
public OperationResult<List<SearchResult>> Search(float[] query, int topK)
{
    // DEBUG: log inputs to trace empty-query caller
    System.Diagnostics.Debug.WriteLine($"DEBUG BruteForceIndex.Search: query.Length={query.Length}, dimensions={_dimensions}, stack=\n{new System.Diagnostics.StackTrace()}");

    // Guard against empty query (fix at this layer too, as defense-in-depth)
    if (query.Length == 0)
        return OperationResult<List<SearchResult>>.Fail("Query vector is empty", "EMPTY_QUERY");
```

Run the test:
```bash
dotnet test --filter "AgentOrchestrator" -- --verbosity detailed 2>&1 | grep "DEBUG BruteForceIndex"
```

The stack trace shows which test file and line triggered the empty query.

---

## Finding Which Test Causes Pollution

If a test creates side effects (files, state) that affect other tests:

```bash
# Run tests one by one to find the polluter
# Bisection: run first half, check for pollution, then narrow down

dotnet test --filter "FullyQualifiedName~AgentOrchestrator" -- --verbosity detailed 2>&1 | grep "Src/"
```

If the pollution is a file on disk, add a cleanup in the test's `Dispose()` or `[Fact]` teardown.

---

## Defense-in-Depth After Root Cause Fix

After fixing the root cause, add validation at every layer in the call chain:

```
Layer 1 (source): SetupSearchContext() throws if EmbeddedQuery is empty
Layer 2 (entry):  AgentOrchestrator.SearchMemoryAsync() validates query not empty
Layer 3 (impl):   BruteForceIndex.Search() returns OperationResult.Fail for empty query
Layer 4 (test):   Test asserts EmbeddedQuery.Length > 0 before calling orchestrator
```

Each layer catches the bug independently. If one layer is bypassed, the others still catch it.

---

## Key Principle

**NEVER fix just where the error appears.** Trace back to find the original trigger.

```
Found error at BruteForceIndex.Search() → trace to HybridRetriever → AgentOrchestrator
  → test setup → FOUND: SetupSearchContext() called before embedding step

Fix at: test setup (fill EmbeddedQuery before creating context)
Also add: guard at BruteForceIndex.Search() (defense-in-depth, not primary fix)
```

---

## Real Tracing Commands for This Project

```bash
# Find all callers of a method
grep -rn "MethodName\|IInterfaceName\.Method" Src/ Tests/ --include="*.cs"

# Find where a variable is set
grep -rn "EmbeddedQuery\s*=" Src/ Tests/ --include="*.cs"

# Find all test files for a component
grep -rn "BruteForceIndex\|IVectorIndex" Tests/ --include="*.cs" -l

# Find all places a class is instantiated
grep -rn "new BruteForceIndex\|new HybridRetriever" Src/ Tests/ --include="*.cs"
```

---

## Anti-Patterns

1. **Fix at the symptom only** — the root cause remains, bug recurs on next run
2. **Single-layer defense** — if the source fix is bypassed, no other layer catches it
3. **Add null check at caller** — this silences the error without understanding why null was passed
4. **Trust the stack trace line number** — the crash site is not the root cause
