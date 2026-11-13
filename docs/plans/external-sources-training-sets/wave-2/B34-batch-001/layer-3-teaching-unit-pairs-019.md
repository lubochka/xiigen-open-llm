# Layer 3 — Teaching Unit Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-019 — Guidance + TEST embedded teaching unit

**Core rule**: TEST fires when "write/create/assert + test noun" appears as an embedded action inside a guidance frame, performed by the requester.

**TEST fires inside guidance frames**:
| Request | TEST fires? | Trigger |
|---------|-------------|---------|
| "Guide through writing unit tests" | yes | write + unit tests |
| "Coach through writing E2E tests" | yes | write + E2E tests |
| "Show how to write parameterized tests" | yes | write + tests |
| "Mentor through TDD: write failing test, implement" | yes | write + test |

**TEST does NOT fire in guidance frames**:
| Request | TEST fires? | Reason |
|---------|-------------|--------|
| "Mentor on testing philosophy" | no | no write/create/assert verb |
| "Coach on test coverage" | no | quality goal, no write verb |
| "Run a workshop on TDD" | no | facilitation → gap |
| "Guide through running tests" | no | run ≠ write/create |

**Gap ID when TEST fires**: gap-skills-guidance-mentor-001 (guide/coach/mentor)
**Gap ID when TEST fires**: gap-skills-guidance-train-001 (show/demonstrate)

**GapHandler when TEST fires**: NOT invoked. Gap warning only. SCHEDULE_CAPABILITY_LEARNING NOT emitted.
