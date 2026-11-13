# DPO Pair Batch — Phase 6 Review (6.2, 6.3, 6.4) — Batch 003
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md + EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This batch supplies DPO training pairs for the remaining Phase 6 sub-phases
(6.2 Release Readiness, 6.3 Security and Compliance Review, 6.4 Performance
and Reliability Review). Sub-phase 6.1 was covered in batch 001.

6.3 was explicitly flagged as Wave 2 priority in the v2 executor prompt.
The most damaging confusion for 6.3 is with 3.4 Security Model — review of
the implemented system vs design of the security model.

---

## Files in this batch

```
dpo-pairs-review-batch-003/
  MANIFEST.md                                          (this file)
  review-pairs-6-2-release-readiness.md                (4 pairs)
  review-pairs-6-3-security-compliance-review.md       (5 pairs)
  review-pairs-6-4-performance-reliability-review.md   (4 pairs)
```

Total: 13 DPO pairs.

---

## Sub-phase coverage

| Sub-phase | Pairs in batch | Primary rejected target |
|---|---|---|
| 6.2 Release Readiness and Risk Review | 4 | 6.1 (1), 7.2 (2), 4.3 (1) |
| 6.3 Security and Compliance Review | 5 | 3.4 (3), 5.1 (1), 7.2 (1) |
| 6.4 Performance and Reliability Review | 4 | 5.4 (2), 3.5 (1), 7.3 (1) |

---

## Wave 2 priority — 6.3

The v2 executor prompt identifies 6.3 as Wave 2 priority. The chosen-vs-rejected
distinction that matters most:

> 6.3 Security and Compliance Review owns reviewing whether the implementation
> satisfies the security model — not designing the model itself. Every 6.3
> pair must use Type A rejected with 3.4 (design the security model) as the
> rejected sub-phase to teach the boundary clearly.

Three of the five 6.3 pairs in this batch use 3.4 as the rejected sub-phase
to drive the boundary distinction.

---

## Sources

All pairs are first-party. References to specific compliance frameworks
(SOC 2, PCI DSS, GDPR, HIPAA) match those frameworks' published control
catalogues at a high level; the pairs do not claim to cover the full
control set.

```
source_license_state: first_party
attribution: xiigen-review-batch-003
```
