# Layer 3 — Teaching Unit Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-018 — "Facilitate" vocabulary teaching unit

**Core rule**: "facilitate" = facilitation vocabulary → always gap unless an integrate action or direct requester code action is embedded.

**Special rule for "facilitate integration"**:
"Facilitate integration of A with B" → API_CONTRACT fires.
This is the one case where "facilitate" wrapping an action still fires a predicate — because "integrate" always fires regardless of frame.

**Disambiguation table**:
| Request | Verdict | Why |
|---------|---------|-----|
| "Facilitate knowledge transfer" | gap | facilitation vocabulary → gap |
| "Facilitate a retrospective" | gap | event facilitation → gap |
| "Facilitate integration of A with B" | API_CONTRACT + gap warning | integrate always fires |
| "Facilitate session where team writes tests" | gap | team's action = third-party |
| "Facilitate and implement feature" | IMPLEMENT + gap warning | requester implements |
| "Facilitate and write the API docs" | DOCUMENT + gap warning | requester writes docs |

**Third-party action rule**:
"Facilitate [session] where [team] [code action]" → gap.
The team's code action during the facilitated session is a third-party action.
Only requester's direct actions fire predicates.

**Exception**: "Facilitate integration" → API_CONTRACT regardless of actor framing.

**Gap ID**: gap-skills-guidance-mentor-001.
