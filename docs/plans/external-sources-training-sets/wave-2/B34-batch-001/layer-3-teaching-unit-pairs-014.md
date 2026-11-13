# Layer 3 — Teaching Unit Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-014 — "Pair with" vocabulary teaching unit

**Core rule**: "pair with" = collaborative learning/pairing vocabulary → always gap unless embedded code action fires.

**Disambiguation table**:
| Request | Verdict | Why |
|---------|---------|-----|
| "Pair with the team on API work" | gap | pair-with = guidance; advisory context |
| "Pair with to write unit tests" | TEST + gap warning | "write tests" embedded → TEST |
| "Pair with to integrate the SSO" | API_CONTRACT + gap warning | "integrate" always fires |
| "Pair with to implement the service" | IMPLEMENT + gap warning | "implement" embedded → IMPLEMENT |
| "Pair with to learn the architecture" | gap | learning intent → gap |
| "Pair with to run tests" | gap | run ≠ write; TEST fires on write/create only |
| "Pair with to understand the codebase" | gap | understanding = learning intent |

**Key principle**: "pair with to [code verb]" = embedded verb rule applies (code verb fires). "pair with to learn/understand" = learning-intent → gap.

**Gap ID**: gap-skills-guidance-mentor-001.
