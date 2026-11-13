# DPO Pair Batch — Testing (5.1, 5.2, 5.3) — Batch 002
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md + EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This batch supplies DPO training pairs for the Phase 5 testing sub-phases that
were empty in calibration run 2026-05-06: 5.1 Unit Test Design, 5.2 Integration
Tests, 5.3 End-to-End Scenario Testing. These are the secondary-gap sub-phases
identified in the v2 executor prompt.

The most common confusion for these sub-phases:
- 5.1 vs 5.2 — using mocks where real containers are required, or vice versa.
  Testcontainers documentation is the authority.
- 5.3 vs 6.1 — automating a user journey when a human checklist is the gate,
  or specifying a human checklist when automation is the gate.

---

## Files in this batch

```
dpo-pairs-testing-batch-002/
  MANIFEST.md                                        (this file)
  testing-pairs-5-1-unit-test-design.md              (5 pairs)
  testing-pairs-5-2-integration-tests.md             (5 pairs)
  testing-pairs-5-3-end-to-end-scenario-testing.md   (5 pairs)
```

Total: 15 DPO pairs.

---

## Sub-phase coverage

| Sub-phase | Pairs in batch | Primary rejected target |
|---|---|---|
| 5.1 Unit Test Design | 5 | 5.2 (3), 6.1 (1), 4.1 (1) |
| 5.2 Integration Tests | 5 | 5.1 (3), 5.3 (1), 7.1 (1) |
| 5.3 End-to-End Scenario Testing | 5 | 6.1 (3), 5.2 (2) |

---

## Batch composition (against v8 minimums)

| Pair type | This batch | v8 minimum | Status |
|---|---|---|---|
| Type A (adjacent sub-phase confusion) | 15 of 15 (100%) | 40% | met |
| Type B (wrong dependency order) | 0 | 20% if multi-task | not applicable — all prompts single-task |
| Type C (wrong domain) | 0 | 30% per domain | not applicable — single domain (TEST) |
| Stage 9 corruption rows | 0 | ≥1 per kind | NOT INCLUDED — separate batch |

The v8 minimums for Type B and Type C apply only when the prompts in the
batch are multi-task or cross-domain. This batch focuses on tight TEST
sub-phase confusion, which is exclusively Type A.

---

## Sources

All pairs are first-party. Where a specific framework is named (Testcontainers,
xUnit, Playwright, k6, etc.) the usage matches that framework's public
documentation.

```
source_license_state: first_party
attribution: xiigen-testing-batch-002
```
