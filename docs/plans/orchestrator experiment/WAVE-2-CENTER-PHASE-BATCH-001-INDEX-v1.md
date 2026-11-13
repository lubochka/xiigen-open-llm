# Wave 2 Center Phase Batch 001 - Index v1
# Date: 2026-05-05

---

## Purpose

List every artifact that belongs to Wave 2 Center Phase Batch 001.

A pair that is not listed in this index does not count toward Batch 001
training.

---

## Source And Control Artifacts

| Artifact | Purpose | Status |
|---|---|---|
| WAVE-2-CENTER-PHASE-BATCH-001-SOURCE-LEDGER-v1.md | 20 training source rows and 5 held-out rows | active |
| WAVE-2-CENTER-PHASE-BATCH-001-HELDOUT-MANIFEST-v1.md | held-out expectations | active |
| WAVE-2-CENTER-PHASE-BATCH-001-CONFORMANCE-SUITE-v2.md | source, pair, and executable router conformance | active |
| WAVE-2-CENTER-PHASE-BATCH-001-COVERAGE-LEDGER-v2.md | quota and node coverage accounting | active |
| WAVE-2-CENTER-PHASE-BATCH-001-FULL-PATH-CONFORMANCE-SUMMARY-v1.md | full-path package verdict | active |

---

## Training Pair Files

| Role | Pair file | Pair id range | Original pairs | Phrasing B companions |
|---|---|---|---:|---:|
| sub-phase-router | WAVE-2-CENTER-PHASE-BATCH-001-SUB-PHASE-ROUTER-SET-v1.md | W2C-RTR-001 through W2C-RTR-020 | 20 | 20 |
| domain-translator | WAVE-2-CENTER-PHASE-BATCH-001-NODE-02-DOMAIN-TRANSLATOR-SET-v1.md | W2C-N02-001 through W2C-N02-020 | 20 | 20 |
| scheme-planner | WAVE-2-CENTER-PHASE-BATCH-001-NODE-03-SCHEME-PLANNER-SET-v1.md | W2C-N03-001 through W2C-N03-005 | 5 | 5 |
| domain-processor | WAVE-2-CENTER-PHASE-BATCH-001-NODE-04-05-06-CHAIN-SET-v1.md | W2C-N04-001 through W2C-N04-005 | 5 | 5 |
| applier | WAVE-2-CENTER-PHASE-BATCH-001-NODE-04-05-06-CHAIN-SET-v1.md | W2C-N05-001 through W2C-N05-005 | 5 | 5 |
| result-translator | WAVE-2-CENTER-PHASE-BATCH-001-NODE-04-05-06-CHAIN-SET-v1.md | W2C-N06-001 through W2C-N06-005 | 5 | 5 |
| arbiter-selector | WAVE-2-CENTER-PHASE-BATCH-001-NODE-07-08-09-REVIEW-FORMATTER-SET-v1.md | W2C-N07-001 through W2C-N07-005 | 5 | 5 |
| arbiter-unit | WAVE-2-CENTER-PHASE-BATCH-001-NODE-07-08-09-REVIEW-FORMATTER-SET-v1.md | W2C-N08-001 through W2C-N08-005 | 5 | 5 |
| formatter | WAVE-2-CENTER-PHASE-BATCH-001-NODE-07-08-09-REVIEW-FORMATTER-SET-v1.md | W2C-N09-001 through W2C-N09-005 | 5 | 5 |

---

## Runtime Evidence

| Artifact | Runtime surface | Result |
|---|---|---|
| WAVE-2-CENTER-PHASE-BATCH-001-HELDOUT-CONFORMANCE-EVIDENCE-2026-05-05.json | SubPhaseRouterUnit held-out execution | pass |

---

## Round Zips

| Phase | Zip |
|---|---|
| 98 | round-phase-98-wave-2-center-phase-batch-001-2026-05-05.zip |
| 99 | round-phase-99-wave-2-center-heldout-conformance-2026-05-05.zip |
| 100 | round-phase-100-wave-2-center-node-02-domain-translator-2026-05-05.zip |
| 101 | round-phase-101-wave-2-center-node-03-scheme-planner-2026-05-05.zip |
| 102 | round-phase-102-wave-2-center-node-04-05-06-chain-2026-05-05.zip |
| 103 | round-phase-103-wave-2-center-node-07-08-09-review-formatter-2026-05-05.zip |
| 104 | round-phase-104-wave-2-center-batch-001-index-and-summary-2026-05-05.zip |

---

## Batch Status

```text
Batch 001 indexed: yes
Batch 001 full-path pair package: yes
Batch 001 node-01 held-out runtime conformance: yes
Batch 001 nodes 02-09 runtime conformance: not yet
Wave 2 complete: no
Full v4 corpus complete: no
```
