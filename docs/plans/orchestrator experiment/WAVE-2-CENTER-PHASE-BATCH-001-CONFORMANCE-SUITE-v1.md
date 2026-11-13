# Wave 2 Center Phase Batch 001 - Conformance Suite v1
# Date: 2026-05-05

---

## Purpose

Bind the Wave 2 center-phase batch to executable conformance expectations.

This suite is the control surface for the 20 training rows and 5 held-out rows
created in Batch 001. It prevents the batch from being counted as corpus
progress unless the unit boundary, source authority, and held-out behavior are
all explicit.

---

## Suite Identity

```text
SuiteId: wave-2-center-phase-batch-001-sub-phase-router-conformance
Logical unit role: sub-phase-router
Domain tag: whole_development_area
Source ledger: WAVE-2-CENTER-PHASE-BATCH-001-SOURCE-LEDGER-v1.md
Training set: WAVE-2-CENTER-PHASE-BATCH-001-SUB-PHASE-ROUTER-SET-v1.md
Held-out manifest: WAVE-2-CENTER-PHASE-BATCH-001-HELDOUT-MANIFEST-v1.md
```

---

## Source Ledger Checks

| Check | Required result |
|---|---|
| 20 training rows exist | true |
| 5 held-out rows exist | true |
| every row has source path or accepted source anchor | true |
| every row has source authority or license annotation | true |
| every row has exactly one primary phase | true |
| every row has exactly one primary sub-phase | true |
| every row has at least three verifiable claims | true |
| every row has a known failure mode | true |
| every row has expected top predicates | true |
| held-out rows are not used in the training pair set | true |

---

## Training Pair Checks

| Check | Required result |
|---|---|
| 20 router A/B rows exist | true |
| every A is `SubPhaseRouterSourceDocument` | true |
| every B is `LogicalUnitResult<SubPhaseRoutingDecision>` | true |
| B contains actual predicted phase and sub-phase ids | true |
| B contains confidence | true |
| B contains nearest wrong sub-phase score | true |
| every confidence is >= 0.70 | true |
| every nearest wrong sub-phase score is < 0.50 | true |
| every row has a Phrasing B companion | true |
| no B describes the desired output instead of producing it | true |

---

## Held-Out Runtime Checks

| Check | Required result |
|---|---|
| held-out rows executed through the real router surface | next code phase |
| expected phase id matched | 5/5 required |
| expected sub-phase id matched | 5/5 required |
| confidence floor held | 5/5 required |
| wrong-domain guard held | 5/5 required |
| source knowledge edges produced | 0 required |
| router knowledge edges produced | 0 required |
| conflict rows leaked to formatter | 0 allowed |

The held-out rows are defined in this round. The executable held-out test is the
next action because this round is a corpus artifact round, not a code round.

---

## Batch Promotion Verdict

```text
Artifact completeness: PASS
Training pair shape: PASS
Held-out manifest shape: PASS
Executable held-out test: PENDING NEXT PHASE
Full Wave 2 quota: NOT COMPLETE
Full v4 corpus: NOT COMPLETE
```

This batch can be counted as a corpus artifact seed. It cannot yet be counted
as an executable conformance pass until Phase 99 runs the held-out rows through
the actual `SubPhaseRouterUnit`.
