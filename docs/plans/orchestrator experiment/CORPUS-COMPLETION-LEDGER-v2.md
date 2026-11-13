# Corpus Completion Ledger v2
# Date: 2026-05-05

---

## Answer

The whole corpus is still not complete.

Phase 81 closes the first blocker from v1: WDA-01 no longer depends on the
deterministic classification fixture for the 31 Wave 1 seed rows.

Current state after Phase 81:

- WDA-00 through WDA-06 pass at deterministic-fixture level.
- WDA-01 now also passes through `SubPhaseRouterUnit` for the 31 ready source
  seed rows.
- The real-source ledger has 31 rows, one per canonical sub-phase.
- The broad v4 quota remains open: 3,100 training documents, 620 sub-phase
  held-out documents, and 60 transition held-out documents are not complete.

---

## What Changed Since v1

| Item | v1 state | v2 state |
|---|---|---|
| real source ledger | missing | `WDA-01-REAL-SOURCE-LEDGER-v1.md` created |
| router unit | missing | `SubPhaseRouterUnit` implemented |
| WDA-01 runtime gap | `sub-phase-router unit not yet implemented` | removed for the seed proof |
| source knowledge edges | 31 | 0 for the 31 seed rows |
| deterministic fixture | used by WDA-01 | replaced for WDA-01 seed proof |
| full corpus | not complete | still not complete |

---

## Test Verification

Focused WDA-01:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA01"
Passed: 1, Failed: 0, Total: 1
```

Focused WDA:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
Passed: 7, Failed: 0, Total: 7
```

Full suite:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
Passed: 1171, Failed: 0, Total: 1171
```

---

## Remaining v4 Completion Contract

| # | v4 completion condition | Current status |
|---:|---|---|
| 1 | WDA-00 through WDA-06 pass | Passed deterministic proof |
| 2 | All 31 sub-phases pass at least 17/20 held-out documents | Not complete |
| 3 | All 31 sub-phases pass all conflict documents with zero formatter leakage | Not complete |
| 4 | All six transitions pass the 60-document transition evaluation | Not complete |
| 5 | All required logical unit cells are trusted | Started; router seed unit exists |
| 6 | All active sub-phase flows are registered | Not complete |
| 7 | A probationary regression suspends dependent flows until retraining | Not complete at corpus level |
| 8 | The topology-intent-classifier handles methodology mutation or returns a clean knowledge edge | Not complete |
| 9 | Phase-context-compiler and transition-gate preserve predicate-level carry-forward | Deterministic proof only |
| 10 | Every corpus batch has source ledger entries, pair files, conformance entries, manifest, zip, and next action item | Not complete |

---

## Next Action

Start Phase 82:

```text
Wave 1 quota ledger and held-out split plan
```

Required artifacts:

1. `WAVE-1-QUOTA-LEDGER-v1.md`
   - expands the 31 seed rows into the v4 quota structure;
   - marks which rows are real, external, synthetic, or missing;
   - does not count incomplete rows as promoted.

2. `WAVE-1-HELD-OUT-SPLIT-v1.md`
   - defines the 20 held-out categories per sub-phase;
   - names conflict leakage tests explicitly.

3. `WAVE-1-CONFORMANCE-SUITE-v1.md`
   - defines the router conformance checks before any new source row counts.

4. Round manifest, verification, and zip.

Until Phase 82 and later quota waves finish, corpus v4 remains in progress.
