# LlmFromScratch - Closed Gap Validation Document
# Version: 1.1 - 2026-05-07
# Covers: A51, A52, A53, A54
# Status: CLOSED in pushed code
# Code repo validated: C:\Projects\llm_mvp_core
# Code remote head validated: c054908a04c84d983dcf4a9265aab7748739ef60
# Test gate validated: 1289/1289 passing

---

## Overview

This document supersedes the earlier open-gap version for A51-A54. The four gaps are
resolved in the live code repository and guarded by tests.

Validated code repository:

```text
C:\Projects\llm_mvp_core
HEAD        c054908a04c84d983dcf4a9265aab7748739ef60
origin/main c054908a04c84d983dcf4a9265aab7748739ef60
```

Validation command:

```bash
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
```

Result:

```text
Passed! - Failed: 0, Passed: 1289, Skipped: 0, Total: 1289
```

Focused gap-validation tests:

```text
Passed! - Failed: 0, Passed: 13, Skipped: 0, Total: 13
```

---

## A51 - Infrastructure copies not deleted

### Prior problem

Production-class copies remained under `35.ExperimentsV2/Infrastructure/`, leaving
experiments able to exercise stale implementations instead of canonical production
classes.

### Resolution

A51 is closed.

The production-class copies were removed from:

```text
35.ExperimentsV2/Infrastructure/
```

Historical compatibility surfaces that are still needed by experiment fixtures were
moved to:

```text
35.ExperimentsV2/Compatibility/
```

Current compatibility files:

```text
DomainClassifier.cs
AttributedRouter.cs
AttributedRoutingResult.cs
StopSignalDetector.cs
```

Runtime/canonical source remains in the production projects. `35.ExperimentsV2/GlobalUsings.cs`
aliases canonical SemanticCompiler types for experiment use.

### Validation

The Infrastructure folder was checked for the stale production-copy names:

```text
DomainClassifier.cs
DomainClassifierCompatibility.cs
FamilyCollapser.cs
LogicExtractor.cs
SemanticNormalizer.cs
SentencePatternClassifier.cs
StopSignalDetector.cs
PromptSplitter.cs
AttributedRouter.cs
AttributedRoutingResult.cs
```

No matching files were found in `35.ExperimentsV2/Infrastructure/`.

Guard test:

```text
Tests/ExperimentInfrastructureConsolidationTests.cs
```

---

## A52 - ContextPacketReader reflection

### Prior problem

`ContextPacketReader` used reflection (`System.Reflection`, `BindingFlags`,
`GetProperty`) to inspect context packet fragments indirectly.

### Resolution

A52 is closed.

`ContextPacketReader` now reads typed `ContextSignals` from metadata through the
canonical metadata key:

```text
context.signals
```

Project 21 converts `LogicalContextPacket` into `ContextSignals` before passing the
metadata to project 23. The legacy `context.packet` path is accepted only when the
value is already a typed `ContextSignals` instance.

### Validation

The live file was checked:

```text
23.SemanticCompilerAdapter/ContextPacketReader.cs
```

No matches remain for:

```text
Reflection
BindingFlags
GetProperty
```

Guard tests include typed context-signal round-trip coverage in:

```text
Tests/PromptRoutingInputCompilerUnitTests.cs
```

---

## A53 - E13B held-out gate

### Prior problem

The original E13B KNN-style experiment memorised known expressions and failed on
held-out expressions. The exact-memory design needed validation that unseen expressions
abstain and do not collide after normalisation.

### Resolution

A53 is closed.

The E13B exact-memory tests now validate:

- held-out corpus rows abstain
- training corpus rows recall the correct completion
- held-out rows do not normalise into training keys
- realistic query variants still abstain when unseen

### Validation

Guard tests live in:

```text
Tests/ExpressionCompletionTests.cs
```

The focused validation suite passed.

---

## A54 - ReplayBuffer and manifest growth

### Prior correction

AdamW optimizer moments were originally suspected as a cross-row cost driver. That
was incorrect. `DpoTrainer` is constructed fresh inside each
`LogicalDpoTrainingUnit.ExecuteAsync` call, so AdamW moments do not accumulate across
rows.

### Prior real problems

Two real growth problems remained:

1. Project-16 `ReplayBuffer` had no capacity cap.
2. `DefaultLogicalCapabilityTrainer` repeatedly scanned/deserialised generated
   capability manifests from disk.

### Resolution

A54 is closed.

Project-16 `ReplayBuffer` now has:

```text
DefaultMaxCapacity = 4096
_maxCapacity
RemoveRange(...) eviction when capacity is exceeded
Count
```

`DefaultLogicalCapabilityTrainer` now has a manifest cache:

```text
_manifestCacheByUnitId
_manifestCacheHydrated
EnsureManifestCacheHydrated()
CountGeneratedCapabilityManifests()
```

Manifests are hydrated once per trainer session and new manifests are inserted into
the cache after save.

### Validation

Guard tests:

```text
Tests/AutonomousLearningTests.cs
Tests/DefaultLogicalCapabilityTrainerTests.cs
```

Focused gap tests passed.

---

## Summary Table

| Gap | Status | Validation |
|---|---|---|
| A51 - Infrastructure copies | Closed | No stale production-copy files remain in `35.ExperimentsV2/Infrastructure`; consolidation tests pass |
| A52 - ContextPacketReader reflection | Closed | No `Reflection`, `BindingFlags`, or `GetProperty` remains in `ContextPacketReader`; typed signal tests pass |
| A53 - E13B held-out gate | Closed | Held-out abstain, training recall, collision, and variant tests pass |
| A54 - ReplayBuffer / manifest growth | Closed | ReplayBuffer capacity tests and manifest-cache tests pass |

## Remaining Work Outside A51-A54

The following are still outside this gap document and remain separate future work:

- E40V2 TrustScorer promotion wiring
- E41 IdleDetector production idle gate wiring
- broader SemanticCompilerFacade prompt-path integration items that are not part of A51-A54
- result translation / TemplateBeautifier wiring

---

# Gap document version: 1.1
# Date: 2026-05-07
# Companion to: IMPLEMENTATION-DOCUMENTATION-v7.md
# Closed gap references: A51, A52, A53, A54
