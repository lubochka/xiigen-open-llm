# Layer 3 — Teaching Unit Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-024 — B34 complete knowledge summary (extended 013–024)

**New vocabulary covered in 013–024**:
shadow, pair-with, walk-through, workshop, advise, facilitate + embedded TEST/IMPLEMENT/API_CONTRACT scenarios, explain boundary cases, mixed multi-verb requests.

**Consolidated gap vocabulary for B34** (013–024 additions):
| Verb | Gap ID | Notes |
|------|--------|-------|
| shadow | gap-skills-guidance-mentor-001 | observation vocabulary |
| pair-with | gap-skills-guidance-mentor-001 | collaborative learning |
| walk-through | gap-skills-guidance-mentor-001 or onboard-001 | context-dependent |
| workshop (event/verb) | gap-skills-guidance-train-001 | training event |
| advise | gap-skills-guidance-mentor-001 | advisory vocabulary |
| facilitate | gap-skills-guidance-mentor-001 | facilitation vocabulary |
| explain | gap-skills-guidance-train-001 | always gap, no exceptions |

**Three critical rules summarized**:
1. Embedded verb rule: code verb fires if requester performs it (participial form); does NOT fire if in "teach to X" infinitive.
2. Integrate exception: "integrate" fires API_CONTRACT in any frame EXCEPT "teach to integrate."
3. Explain rule: "explain" is always gap, regardless of topic.

**Third-party action rule**:
"Shadow while she builds" → gap (she builds; requester shadows)
"Facilitate session where team integrates" → special: API_CONTRACT fires (integrate exception)
"Facilitate session where team writes tests" → gap (team writes = third-party; integrate exception does not extend to other predicates)

**DomainId**: software-development for all B34 gaps.
**SCHEDULE_CAPABILITY_LEARNING**: emitted once per full gap (predicate set = []) only.
