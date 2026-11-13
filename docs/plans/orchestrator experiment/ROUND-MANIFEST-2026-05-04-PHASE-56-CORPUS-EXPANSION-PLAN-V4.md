# Round Manifest - Phase 56 Corpus Expansion Plan v4
# Date: 2026-05-04

---

## Purpose

Finish the corpus expansion plan by correcting the lifecycle count and binding
broad corpus intake to the WDA proof gates.

---

## Files Produced

- `CORPUS-EXPANSION-PLAN-v4.md`

---

## Corrections

- Corrected canonical sub-phase count from 27 to 31.
- Corrected training target from 2,700 to 3,100 documents.
- Corrected held-out sub-phase target from 540 to 620 documents.
- Preserved the 60 cross-phase transition held-out documents as additional
  transition evaluation coverage.
- Reorganized waves around WDA proof gates rather than document abundance.

---

## Verification

Document-only round. No production code changed.

Checks performed:

- v3 read.
- WDA experiment set read.
- development phase guide read.
- learning-set translation guide read.
- v4 created with corrected arithmetic and explicit promotion gates.

Code tests were not run because no C# files changed in this round.

---

## Next Action

Run WDA-01 Round A and then WDA-01 Round B. Broad corpus intake starts only
after classification probes identify which sub-phases have ready source
coverage and which are source knowledge edges.
