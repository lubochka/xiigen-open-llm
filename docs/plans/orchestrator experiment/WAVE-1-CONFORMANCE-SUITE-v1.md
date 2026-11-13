# Wave 1 Conformance Suite v1
# Date: 2026-05-05

---

## Purpose

Define the conformance checks a source row or router replacement must pass
before it counts toward the v4 corpus.

---

## Router Conformance Checks

For each source row:

| Check | Required value |
|---|---|
| source path exists | true |
| source tier populated | true |
| license or authority populated | true |
| primary phase populated | true |
| primary sub-phase populated | true |
| expected top predicates populated | true |
| known failure mode populated | true |
| router returns success envelope | true |
| predicted phase equals oracle | true |
| predicted sub-phase equals oracle | true |
| confidence | >= 0.70 |
| nearest wrong-domain score | < 0.50 |
| ready source becomes knowledge edge | false |

---

## Batch Promotion Checks

A corpus batch can be promoted only when:

1. every source row has a complete source ledger entry;
2. every training row has a pair file with actual A and actual B;
3. every held-out row is marked as held-out and not used for training;
4. every conflict row has zero formatter leakage;
5. every logical unit touched by the batch has conformance evidence;
6. the batch writes a manifest;
7. the batch writes a zip;
8. the batch names the next action.

---

## Regression Checks

Every future router or corpus batch must rerun:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA01" --no-build
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
```

If WDA-01 fails, the batch does not promote.

If WDA passes but full suite fails, the batch does not promote.

---

## Current Conformance State

| Surface | Status |
|---|---|
| 31 Wave 1 seed rows | pass WDA-01 real-router proof |
| full 3,100 training quota | not complete |
| 620 sub-phase held-out rows | not complete |
| 60 transition held-out rows | not complete |
| transition-gate learned unit | not complete |
| phase-context-compiler learned unit | not complete |
| topology-intent-classifier learned unit | not complete |

---

## Next Action

Build the first quota batch for the center-phase group:

```text
3.1 Architecture and component topology
3.3 API contract definition
3.4 Security and authorization model
4.1 Backend services
4.3 Integration and provider connectors
```

The batch should produce real source ledger rows first, then pair files, then
held-out entries, then conformance results.
