# DNA Compliance — .NET Translation for LlmFromScratch

## Context

The original nine DNA coding rules were written for XIIGen's NestJS/TypeScript SaaS
platform. This document translates each rule to the .NET ML library context so the same
compliance gate can run here.

---

## Rule 1 — No typed model classes for configuration / freedom data

**Original:** All business data uses `Record<string, unknown>`. Never create typed model classes.

**In .NET:** Model architecture configuration (layer count, hidden dimensions, learning rate)
lives in `ModelConfig` and `TrainingConfig` records — these are typed because they ARE the
machine contracts. But training hyperparameters that a user might want to override
externally must NOT be hardcoded as `private const` values deep in logic. They belong in
config objects passed in from outside.

**Violation pattern:**
```csharp
// BAD — hardcoded magic number buried in logic
var lr = 0.0003f;  // hidden constant

// GOOD — comes from TrainingConfig
var lr = config.LearningRate;
```

**Detection:**
```bash
grep -rn "= 0\.[0-9]\+" --include="*.cs" | grep -v "readonly\|const\|//\|test\|Tests"
```

---

## Rule 2 — Dynamic filter building (no hardcoded selectors)

**Original:** All searches use dynamic filter builder. Empty fields auto-excluded.

**In .NET (VectorSearch, GraphEngine):** Search and graph traversal methods must accept
filter parameters, not hardcode field checks. A search function that only searches on
`RelationType == "Calls"` is hardcoded. Filters come in as parameters.

**Violation:**
```csharp
// BAD
edges.Where(e => e.RelationType == "Calls")

// GOOD
edges.Where(e => filter == null || e.RelationType == filter.RelationType)
```

---

## Rule 3 — Result pattern, never throw for business logic

**Original:** Every service method returns `DataProcessResult<T>`. Exceptions for
infrastructure failures only.

**In .NET:** Every public method that handles expected failure cases returns
`Result<T>` (a discriminated union of success + error). Never throw
`ArgumentException` or similar for expected inputs. Throw only for programming errors
(null refs, out-of-bounds in internal logic).

**Violation:**
```csharp
// BAD
if (tokens.Count == 0) throw new InvalidOperationException("No tokens");

// GOOD
if (tokens.Count == 0) return Result<string>.Failure("EMPTY_INPUT", "No tokens provided");
```

**Note:** For pure mathematical operations (TorchSharp forward passes), exceptions from
the tensor library itself are infrastructure — catch them at the boundary, wrap in Result.

---

## Rule 4 — Base class / interface contracts

**Original:** All services extend MicroserviceBase.

**In .NET:** All model layers implement `ILayer` (forward interface). All indexing
structures implement `IVectorIndex`. All trainers implement `ITrainer`. No orphan
concrete classes with no interface.

**Violation:**
```csharp
// BAD — standalone class, no interface
public class BruteForceIndex { ... }

// GOOD
public class BruteForceIndex : IVectorIndex { ... }
```

---

## Rule 5 — Scope isolation

**Original:** Every query scoped to tenant via AsyncLocalStorage.

**In .NET:** Every inference call is stateless with respect to other calls. The
`KvCache` is tied to a single generation request — it must not be shared across
calls. The `EmbeddingModel` must be thread-safe (read-only weights, no shared mutable state
per call). No static mutable fields in model classes.

**Detection:**
```bash
grep -rn "static.*=" --include="*.cs" | grep -v "readonly\|const\|//\|test"
```

---

## Rule 6 — Generic controllers (N/A)

No HTTP controllers in this project. Rule 6 does not apply.

---

## Rule 7 — Idempotency for checkpoints

**Original:** All queue consumers deduplicate using idempotency keys.

**In .NET:** Checkpoint saves must be atomic and idempotent. Write to a temp file
first, then rename. Calling `SaveCheckpoint` twice with the same step number must not
corrupt the previous checkpoint.

**Violation:**
```csharp
// BAD — overwrites directly, crash mid-write = corrupt checkpoint
File.WriteAllBytes(path, data);

// GOOD — atomic rename
File.WriteAllBytes(path + ".tmp", data);
File.Move(path + ".tmp", path, overwrite: true);
```

---

## Rule 8 — Persist state before dispatching work (outbox pattern)

**Original:** `storeDocument()` MUST happen before `enqueue()`.

**In .NET:** In the training loop, write the checkpoint to disk BEFORE advancing the
step counter and before dispatching the next batch. If the process crashes, the
step counter should not be ahead of the last saved checkpoint.

---

## Rule 9 — CloudEvents (N/A)

No inter-service messaging in this project. Rule 9 does not apply.

---

## Dev Safety Gate (.NET)

Run at start and end of every phase:

```bash
dotnet build LlmFromScratch.sln
# Expected: 0 errors, 0 warnings (treat warnings as errors where feasible)

dotnet test LlmFromScratch.sln --logger "console;verbosity=minimal"
# Expected: 0 failed, all passed
```

**Blocked condition:** any build error or test failure blocks the phase from closing.

---

## Test Integrity Rules (.NET)

- Tests must assert domain outcomes, not just "ran without exception"
- Tensor shape assertions are behavioral assertions — include them
- Value-range assertions on known mathematical properties (e.g. softmax sums to 1.0)
  are behavioral assertions — include them
- `Assert.True(result != null)` alone is a stub test — add shape/value assertions
- Zero failures = zero failures. No "pre-existing" carve-outs.

---

## Applicable Rules Summary

| Rule | Applies | How |
|------|---------|-----|
| 1 — no typed config classes | Partial | Config values must come from config objects, not inline constants |
| 2 — dynamic filters | Yes | VectorSearch, GraphEngine queries |
| 3 — Result pattern | Yes | All public API methods returning business-logic outcomes |
| 4 — interface contracts | Yes | ILayer, IVectorIndex, ITrainer |
| 5 — scope isolation | Yes | Thread safety, no shared mutable state per call |
| 6 — generic controllers | No | No HTTP layer |
| 7 — idempotent writes | Yes | Checkpoint saves, index serialization |
| 8 — persist before dispatch | Yes | Training loop step ordering |
| 9 — event envelope | No | No messaging layer |
