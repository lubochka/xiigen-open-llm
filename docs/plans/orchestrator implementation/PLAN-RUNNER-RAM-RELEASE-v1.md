# Plan: Runner RAM Release Fix
# Version: 1
# Date: 2026-05-07
# Addresses: accumulated registry and trainer cache across physical batches
# Floor before: 1251
# Floor after: 1253

---

## Problem

The calibration runner creates registries, a trainer, and its internal warm cache once
before the batch loop and keeps them alive for the entire run. Every physical batch
appends into the same `InMemoryLogicalUnitRegistry`, `InMemoryLogicalFlowRegistry`,
and topology registry without ever clearing them. The trainer's `_teachingCache`
accumulates live `LlmModel` objects — one per domain — that are never released
between batches. The reference model on the trainer is a second `LlmModel` that
lives for the full run. Neither `LlmModel` nor `BpeTokenizer` implements `IDisposable`,
so there is no explicit release path: collection only happens when the objects go
out of scope.

The result is that each batch starts with the full memory footprint of every
previous batch still resident. On a machine with other processes running, this
compounds the baseline pressure and causes the safety gate to fire early.

---

## What is not the problem

The learning process itself is small. Runner working set across the completed
batches stayed between roughly 90 MB and 166 MB per batch. Checkpoint disk
footprint after 61 rows was 7.54 MB. CPU peaked at 12.3%. The only resource
that grew across batches was the accumulated in-memory state from the previous
batch that was never released.

---

## Phase 1 — Reconstruct registries and trainer per physical batch

### File changed

```
experiments-v2/XIIGEN-DOCS-BATCH-COMPARISON/runner/XiigenDocsBatchComparisonRunner/Program.cs
```

### What changes

In `RunBatchSizeAsync`, the following five objects are currently constructed
once before the batch loop:

```csharp
var topology = new InMemoryLogicalTopologyRegistry();
var flows = new InMemoryLogicalFlowRegistry();
var units = new InMemoryLogicalUnitRegistry();
var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
```

Move all five inside the `for (var batchIndex = 0; ...)` loop, at the top of
each iteration before the batch preparation step. Add an explicit GC collect
and wait after the five declarations so the previous batch's objects are
collected before the new batch allocates.

```csharp
// Reconstruct registries and trainer fresh for each batch so the previous
// batch's model weights, unit entries, and flow entries do not accumulate
// across batches. This is the primary RAM release mechanism.
var topology = new InMemoryLogicalTopologyRegistry();
var flows = new InMemoryLogicalFlowRegistry();
var units = new InMemoryLogicalUnitRegistry();
var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
if (!seed.IsSuccess)
    throw new InvalidOperationException(seed.ErrorMessage);
var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);
GC.Collect();
GC.WaitForPendingFinalizers();
```

Remove the original five declarations from before the loop and remove the
`if (!seed.IsSuccess)` guard that was adjacent to them — it moves inside
with the new code above.

The `regressionSummaries`, `domainChains`, `batchSummaries`, `rowSummaries`,
`stageSummaries`, and `domainChains` lists remain outside the loop. They
accumulate across batches intentionally for the final report.

### Gate

Run `dotnet test Tests/LlmFromScratch.Tests.csproj`. Test count must be 1251
or higher. No failures.

---

## Phase 2 — Add ClearCache to DefaultLogicalCapabilityTrainer

### File changed

```
21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
```

### What changes

Add one public method after the constructor:

```csharp
// Release the in-memory teaching cache and manifest hydration state so the
// next training session starts clean without stale model weights from a
// prior batch. Call this between epochs when the trainer is kept alive
// across batches. In the calibration runner, reconstruction per batch
// (Phase 1) makes this implicit; this method serves long-running
// production sessions where reconstruction is not wanted.
public void ClearCache()
{
    _teachingCache.Clear();
    _manifestCacheByUnitId.Clear();
    _manifestCacheHydrated = false;
}
```

### New test

Add one test to `Tests/` covering `ClearCache` behaviour. The test must:

1. Construct a `DefaultLogicalCapabilityTrainer` with a temp checkpoint root.
2. Call `TrainAndRegisterAsync` with a minimal valid request so the warm cache
   and manifest cache are populated.
3. Call `ClearCache()`.
4. Verify that the next call to `TrainAndRegisterAsync` for the same domain does
   not use the warm cache — the returned `BranchTiming.UsedWarmCache` must be
   `false`.

Name the test file `DefaultLogicalCapabilityTrainerClearCacheTests.cs`.

### Gate

Run `dotnet test Tests/LlmFromScratch.Tests.csproj`. Test count must be 1252
or higher. No failures.

---

## Execution order

Phase 1 first. Phase 2 second. Each phase ends with a passing test gate before
the next phase begins.

---

## What does not change

- `DefaultLogicalCapabilityTrainer`'s internal caching logic for within-batch
  domain continuity is unchanged. The warm cache is intentional and correct
  for accumulating weight steps across rows of the same domain within one batch.
- `InMemoryLogicalUnitRegistry` and `InMemoryLogicalFlowRegistry` are unchanged.
  They do not need a `Clear` method — reconstruction in the runner is sufficient.
- The regression window logic in the runner reads from the full `rows` list and
  the accumulated `batchSummaries`, both of which remain outside the batch loop
  and are unaffected.
- No test is removed or renamed.

---

## Floor summary

| After | Floor |
|---|---:|
| Before Phase 1 | 1251 |
| After Phase 1 | 1251 (no new test — structural change only) |
| After Phase 2 | 1252 |
