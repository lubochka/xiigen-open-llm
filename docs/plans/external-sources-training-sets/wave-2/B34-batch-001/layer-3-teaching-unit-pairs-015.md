# Layer 3 — Teaching Unit Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-015 — "Walk through" vocabulary teaching unit

**Core rule**: "walk through" = guided explanation vocabulary → always gap when used to present/explain. Code predicates fire when action verbs are embedded.

**Disambiguation table**:
| Request | Verdict | Why |
|---------|---------|-----|
| "Walk through how the API works" | gap | explain existing behavior → gap |
| "Walk through the architecture decisions" | gap | explain past design → gap |
| "Walk through integrating the API" | API_CONTRACT + gap warning | "integrating" fires API_CONTRACT |
| "Walk through writing the tests" | TEST + gap warning | "writing tests" fires TEST |
| "Walk through and then implement it" | IMPLEMENT + gap warning | "implement" fires IMPLEMENT |
| "Walk through architecting the system" | ARCHITECTURE + gap warning | "architecting" fires ARCHITECTURE |
| "Walk through the deployment process" | gap | explain process → gap |

**"Walk through" vs "explain" equivalence**:
Both produce gap when presenting/explaining. Both produce gap warnings + code predicate when embedding code actions.

**Gap ID context**:
Onboarding context → gap-skills-guidance-onboard-001
General mentoring context → gap-skills-guidance-mentor-001
