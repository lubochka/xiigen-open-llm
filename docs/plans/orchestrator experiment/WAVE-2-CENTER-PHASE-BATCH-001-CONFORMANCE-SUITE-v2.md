# Wave 2 Center Phase Batch 001 - Conformance Suite v2
# Date: 2026-05-05

---

## Purpose

Close the executable conformance gap left open by v1.

Version 1 defined the source-ledger, pair-shape, and held-out expectations.
Version 2 records the actual runtime execution of the five held-out rows
through `SubPhaseRouterUnit`.

---

## Suite Identity

```text
SuiteId: wave-2-center-phase-batch-001-sub-phase-router-conformance
Logical unit role: sub-phase-router
Domain tag: whole_development_area
Runtime unit: AgentInfrastructure.Logical.SubPhaseRouterUnit
Source ledger: WAVE-2-CENTER-PHASE-BATCH-001-SOURCE-LEDGER-v1.md
Training set: WAVE-2-CENTER-PHASE-BATCH-001-SUB-PHASE-ROUTER-SET-v1.md
Held-out manifest: WAVE-2-CENTER-PHASE-BATCH-001-HELDOUT-MANIFEST-v1.md
```

---

## Runtime Finding

The first executable run exposed two confident wrong routes:

```text
W2C-HLD-002 -> expected 3.3 API contract definition, predicted 2.1 user stories and acceptance criteria
W2C-HLD-004 -> expected 4.1 backend services, predicted 7.4 coverage and reporting
```

Both failures had the same cause: generic predicate overlap outweighed exact
sub-phase identity wording.

The fix added identity evidence as a separate scoring component:

```text
predicate score: 0.40
identity score: 0.40
general text score: 0.20
```

The router now also tokenizes `DocumentType` together with title, compiled text,
and path.

---

## Executable Held-Out Checks

| Check | Required result | Actual result |
|---|---|---|
| held-out rows executed through the real router surface | yes | PASS |
| expected phase id matched | 5/5 | PASS |
| expected sub-phase id matched | 5/5 | PASS |
| confidence floor held | 5/5 >= 0.70 | PASS |
| wrong-domain guard held | 5/5 < 0.50 | PASS |
| source knowledge edges produced | 0 | PASS |
| router knowledge edges produced | 0 | PASS |
| conflict rows leaked to formatter | 0 allowed | PASS at router boundary; downstream guard remains required |

---

## Verification Commands

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA01_Wave2CenterBatch001HeldOut" --no-restore /m:1
Result: 1/1 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
Result: 8/8 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
Result: 1177/1177 passing

dotnet build LlmFromScratch.sln --no-restore /m:1
Result: Build succeeded, 0 warnings, 0 errors
```

---

## Batch Promotion Verdict

```text
Artifact completeness: PASS
Training pair shape: PASS
Held-out manifest shape: PASS
Executable held-out test: PASS
Wave 2 Batch 001 router conformance: PASS
Full Wave 2 quota: NOT COMPLETE
Full v4 corpus: NOT COMPLETE
```

Batch 001 now has both artifact coverage and executable held-out router
coverage.
