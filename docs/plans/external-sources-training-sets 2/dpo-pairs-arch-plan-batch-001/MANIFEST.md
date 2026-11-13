# DPO Pair Batch — ARCH and PLAN — Batch 001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Method: EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This batch supplies DPO training pairs for the ARCH primary-gap sub-phases
(3.1, 3.2, 3.5, 6.1) and the PLAN domain. Calibration run 2026-05-06 showed
zero training batches for ARCH and only 2 of 6 PLAN batches with positive
reward margin. Sub-phases 3.1, 3.2, 3.5 had zero pairs; sub-phases 5.1, 5.2,
5.3 had zero pairs.

Every pair in this batch follows the v1 guidance:
- the rejected response is a complete, plausible adjacent-sub-phase response —
  not a negation of the correct one
- every rejected ends with a `[WRONG: ...]` annotation that names the specific
  confusion
- the chosen and rejected differ in the decision they make, not in wording

---

## Files in this batch

```
dpo-pairs-arch-plan-batch-001/
  MANIFEST.md                                        (this file)
  arch-pairs-3-1-component-topology.md               (6 pairs)
  arch-pairs-3-2-data-model.md                       (5 pairs)
  arch-pairs-3-5-observability.md                    (5 pairs)
  arch-pairs-6-1-manual-qa.md                        (5 pairs)
  plan-pairs.md                                      (6 pairs)
```

Total: 27 DPO pairs.

---

## Batch composition (against v8 minimums)

| Pair type | This batch | v8 minimum | Status |
|---|---|---|---|
| Type A (adjacent sub-phase confusion) | 21 of 27 (77%) | 40% | met |
| Type B (wrong dependency order) | 3 of 27 (11%) | 20% if multi-task | met for PLAN |
| Type C (wrong domain) | 3 of 27 (11%) | 30% per domain | partial — covered in PLAN file |
| Stage 9 corruption rows | 0 | ≥1 per kind | NOT INCLUDED — separate batch |

Stage 9 corruption rows are deferred to a follow-up batch focused on corruption
patterns; this batch covers DPO chosen/rejected pairs only.

---

## Sub-phase coverage

| Sub-phase | Pairs in batch | Primary rejected target |
|---|---|---|
| 3.1 Architecture and Component Topology | 6 | 4.1 (5), 4.4 (1) |
| 3.2 Data Model and Persistence Design | 5 | 4.1 (3), 4.5 (2) |
| 3.5 Observability and Operational Design | 5 | 4.1 (3), 7.3 (2) |
| 6.1 Manual QA and Acceptance Review | 5 | 5.1 (2), 5.3 (3) |
| PLAN (multi-branch) | 6 | Type B wrong-order (3), Type C explanation (3) |

---

## Sources

All pairs in this batch are first-party. The technical scenarios are
synthesized from common multi-tenant SaaS architecture patterns. Where a
specific technology is named (Elasticsearch, JWT, OpenTelemetry, etc.) the
usage matches the public documentation of that technology.

```
source_license_state: first_party
attribution: xiigen-arch-plan-batch-001
```

---

## Pre-submission checklist (per pair)

```
□ Rejected is a complete, coherent response — not a negation phrase
□ Rejected contains real technical content that relates to the prompt
□ Rejected addresses a different but plausible sub-phase — not a random one
□ The [WRONG: ...] explanation names the specific confusion, not generic criticism
□ Chosen and rejected differ in the decision they make, not only in wording
□ For PLAN: the rejected reverses or omits a real dependency
□ For ARCH: the rejected implements instead of deciding, or monitors instead of designing
□ The prompt appears exactly once in this batch as chosen
```

Every pair in this batch passes this checklist.
