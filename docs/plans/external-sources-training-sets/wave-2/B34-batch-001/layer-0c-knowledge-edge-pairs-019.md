# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-019-001 — Guidance + TEST embedded: complete rule reference

TEST fires when: write/create/assert test verbs are present in the embedded action.
TEST does NOT fire when: guidance verb used without embedded write/create/assert verb.

| Request | TEST fires? | Reason |
|---------|-------------|--------|
| "Guide through writing unit tests" | yes | "writing tests" = embedded TEST action |
| "Coach through writing E2E tests" | yes | "writing E2E tests" = embedded TEST action |
| "Show how to write parameterized tests" | yes | "write tests" embedded → TEST |
| "Mentor on testing philosophy" | no | no write/create/assert verb |
| "Coach on test coverage" | no | quality goal, no write verb |
| "Advise on testing approach" | no | advisory without write verb |
| "Run a workshop on TDD" | no | facilitation of methodology = gap |
| "Guide through running tests" | no | "running tests" ≠ TEST; run ≠ write/create |

---

## KE-019-002 — TDD (Test-Driven Development) methodology alone = gap

"Facilitate a TDD workshop" → gap (methodology facilitation)
"Coach on TDD principles" → gap (advisory on methodology)
"Guide through the TDD red-green-refactor cycle" → depends:
  - If "write tests" embedded: TEST fires
  - If only "red-green-refactor" described without write verb: gap

---

## KE-019-003 — Gap warning text for guidance+TEST

When TEST fires alongside a guidance verb:
CapabilityGapWarning: "gap-skills-guidance-mentor-001: '[guidance verb]' is guidance vocabulary"
GapHandler is NOT invoked. Gap warning is supplemental.
SCHEDULE_CAPABILITY_LEARNING is NOT emitted (reserved for full gap only).
