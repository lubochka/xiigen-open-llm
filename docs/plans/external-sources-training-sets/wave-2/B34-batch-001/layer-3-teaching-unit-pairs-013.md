# Layer 3 — Teaching Unit Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-013 — "Shadow" vocabulary teaching unit

**Core rule**: "shadow" = observation/learning vocabulary → always gap.

**When shadow is gap (no embedded requester code action)**:
| Request | Verdict |
|---------|---------|
| "Shadow me on the deployment" | gap-skills-guidance-mentor-001 |
| "Shadow the senior engineer while she codes" | gap (she codes = third-party) |
| "Shadow the on-call rotation" | gap-skills-guidance-mentor-001 |

**When shadow produces gap warning (embedded requester code action)**:
| Request | Verdict |
|---------|---------|
| "Shadow and then implement the same pattern" | IMPLEMENT fires + gap warning |
| "Shadow and write the runbook" | DOCUMENT fires + gap warning |
| "Shadow and write regression tests" | TEST fires + gap warning |

**Key principle**: Only the requester's own direct actions fire predicates. Third-party actions observed during shadowing do not fire predicates.

**Gap ID assignment**: gap-skills-guidance-mentor-001 (observation/mentoring context).
