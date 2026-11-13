# Corpus v4 Final Completion Audit
# Date: 2026-05-05

---

## Purpose

Audit whether the v4 corpus can honestly be declared complete.

This document uses the ten final declaration conditions from
`CORPUS-EXPANSION-PLAN-v4.md`. A condition is marked complete only when the
current artifacts and runtime tests satisfy it directly.

---

## Final Declaration Conditions

| # | v4 completion condition | Current status | Evidence |
|---:|---|---|---|
| 1 | WDA-00 through WDA-06 pass | PASS | WDA suite 8/8 passing after Phase 99 |
| 2 | all 31 sub-phases pass at least 17/20 held-out documents | FAIL | only Batch 001 has 5 held-out rows; no sub-phase has 20/20 held-out rows |
| 3 | all 31 sub-phases pass all conflict documents with zero formatter leakage | FAIL | conflict guard exists for Batch 001 only at router/pair level; no full 31-sub-phase conflict execution |
| 4 | all six transitions pass the 60-document transition evaluation | FAIL | transition held-out rows remain 0/60 |
| 5 | all required logical unit cells are trusted | FAIL | node-01 has executable router proof; nodes 02-09 have pair files but not learned-unit replacement conformance for Batch 001 |
| 6 | all active sub-phase flows are registered | FAIL | security-verification-3.4 is registered; full 31-sub-phase flow registration is not complete |
| 7 | a probationary regression suspends dependent flows until retraining | FAIL | no full corpus-level probationary regression control has been proven |
| 8 | topology-intent-classifier handles methodology mutation requests or returns clean knowledge edge | FAIL | topology mutation commands exist; natural-language topology-intent classifier is still not complete |
| 9 | phase-context-compiler and transition-gate preserve predicate-level carry-forward | FAIL | WDA transition fixtures pass; learned phase-context compiler and transition-gate cells are not complete |
| 10 | every corpus batch has source ledger, pair files, conformance entries, manifest, zip, and next action item | PARTIAL | Batch 001 satisfies this; full corpus batches do not yet exist |

---

## Honest Verdict

```text
Full corpus v4 complete: NO
```

The current corpus work has made real progress, but it cannot be called fully
done without violating the plan's own final declaration rules.

---

## What Is Complete

```text
WDA topology/orchestration experiment set: pass
Batch 001 source ledger: complete
Batch 001 node-01 through node-09 pair coverage: complete
Batch 001 held-out router conformance: pass
Security-verification-3.4 executable proof slice: strong through S1-S24
```

---

## What Blocks Full Completion

The blocking items are not formatting gaps. They are material corpus and runtime
gaps:

```text
3,080 training document packets still missing
615 sub-phase held-out document packets still missing
60 transition held-out document packets still missing
node-02 through node-09 runtime learned-unit conformance still missing
31 active sub-phase flows not registered
phase-context compiler learned cell not complete
transition-gate learned cell not complete
topology-intent-classifier not complete
probationary regression suspension not proven
```

---

## Required Path To Completion

The remaining work must proceed in this order:

1. Repeat the Batch 001 artifact contract until Wave 2 reaches 500 training
   document packets and 100 held-out packets.
2. Execute held-out conformance for every Wave 2 batch at the router boundary.
3. Add learned-unit replacement conformance for nodes 02 through 09.
4. Expand Waves 3, 4, and 5 to cover the remaining 26 sub-phases.
5. Produce and execute the 60 transition held-out rows in Wave 6.
6. Register all active sub-phase flows.
7. Prove the topology-intent-classifier, phase-context-compiler,
   transition-gate, and probationary regression controls.

---

## Completion Boundary

Further automatic file generation without additional reviewed source packets
would create synthetic or repetitive rows that do not satisfy the intent of the
v4 source authority rule.

The corpus can continue, but it cannot honestly be finished in the current
state by generating more files from the same small Batch 001 source set.
