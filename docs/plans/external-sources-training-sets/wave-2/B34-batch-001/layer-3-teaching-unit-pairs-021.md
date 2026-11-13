# Layer 3 — Teaching Unit Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-021 — Guidance + API_CONTRACT embedded teaching unit

**Core rule**: "integrate" always fires API_CONTRACT regardless of the guidance frame surrounding it. This is the strongest predicate rule in B34.

**API_CONTRACT fires in all guidance frames**:
| Request | API_CONTRACT fires? | Frame |
|---------|---------------------|-------|
| "Walk through integrating the payment API" | yes | walk-through |
| "Guide through integrating the SSO" | yes | guide-through |
| "Facilitate integration of A with B" | yes | facilitate |
| "Show how to integrate the webhook" | yes | show-how-to |
| "Coach through integrating the provider" | yes | coach-through |

**API_CONTRACT does NOT fire (exceptions)**:
| Request | API_CONTRACT fires? | Reason |
|---------|---------------------|--------|
| "Walk through the API" (no integrate) | no | explain intent; no integrate verb |
| "Teach to integrate" | no | "teach to X" infinitive exception |
| "Advise on integration patterns" | no | "integration patterns" = advisory noun; no integrate verb |
| "Workshop on API integration" | no | facilitation topic → gap |

**"Teach to integrate" exception**:
"Teach to integrate" = "teach to X" pattern. Infinitive "to integrate" = what others will learn = learning target. Requester's action = teach = gap.
This is the only construction where "integrate" does NOT fire API_CONTRACT.

**Gap ID when API_CONTRACT fires**: gap-skills-guidance-mentor-001 (walk-through/guide/facilitate); gap-skills-guidance-train-001 (show).
