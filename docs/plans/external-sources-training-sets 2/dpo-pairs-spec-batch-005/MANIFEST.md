# DPO Pair Batch — Phase 2 Functional Specification — Batch 005
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md + EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This batch teaches the orchestrator to distinguish the four Phase 2
sub-phases that share DOCUMENT and QA predicates:

- 2.1 Business Capability Definition — capability boundaries, user value,
       actors, expected outcomes.
- 2.2 User Stories and Acceptance Criteria — story slices, acceptance
       gates, observable behaviour requirements.
- 2.3 Data and Validation Rules — input data, validation policy, state
       transitions, failure behaviour.
- 2.4 Edge Cases and Boundary Conditions — edge cases, boundary examples,
       exception paths, ambiguity closure.

The v2 executor prompt warned that 2.1 and 2.2 are commonly confused (the
"what and who" vs the "observable behaviour"), and that 2.4 is the most
likely confusion source with Stage 9 corruption — this batch addresses
2.4-vs-stage-9 ambiguity in the rejected text.

---

## Files in this batch

```
dpo-pairs-spec-batch-005/
  MANIFEST.md                                          (this file)
  spec-pairs-2-1-business-capability.md                (4 pairs)
  spec-pairs-2-2-user-stories-acceptance.md            (4 pairs)
  spec-pairs-2-3-data-validation-rules.md              (4 pairs)
  spec-pairs-2-4-edge-cases-boundaries.md              (4 pairs)
```

Total: 16 DPO pairs.

---

## Sub-phase coverage

| Sub-phase | Pairs | Primary rejected target |
|---|---|---|
| 2.1 Business Capability Definition | 4 | 2.2 (2), 3.1 (1), 4.1 (1) |
| 2.2 User Stories and Acceptance Criteria | 4 | 2.1 (1), 5.3 (1), 6.1 (1), 5.1 (1) |
| 2.3 Data and Validation Rules | 4 | 4.1 (2), 3.2 (1), 3.3 (1) |
| 2.4 Edge Cases and Boundary Conditions | 4 | 5.1 (1), 6.1 (1), 4.1 (1), 2.2 (1) |

---

## Sources

All pairs are first-party.

```
source_license_state: first_party
attribution: xiigen-spec-batch-005
```
