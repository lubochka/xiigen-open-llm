# Layer 3 — Teaching Unit Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-017 — "Advise" vocabulary teaching unit

**Core rule**: "advise" = advisory vocabulary → always gap when used alone. Code verbs embedded by the requester fire their predicates.

**Disambiguation table**:
| Request | Verdict | Why |
|---------|---------|-----|
| "Advise on best practices" | gap | advisory → gap |
| "Advise on security" | gap | teaching-about-security ≠ SECURITY |
| "Advise on how to implement" | gap | "advise on how to" = advisory pattern |
| "Advise and design architecture" | ARCHITECTURE + gap warning | "design" fires |
| "Advise and harden the gateway" | SECURITY + gap warning | "harden" fires |
| "Advise and integrate the system" | API_CONTRACT + gap warning | "integrate" fires |
| "Advise team to implement X" | gap | recommend others act = gap |

**"Advise on" vs "advise and do"**:
"Advise on [topic]" = advisory without action = gap
"Advise and [code verb]" = gap warning + code predicate fires (requester performs code action)
"Advise [others] to [code verb]" = recommend others act = gap (requester's action = advise)

**Gap ID**: gap-skills-guidance-mentor-001.
**Related gap verbs**: consult, recommend, review-with, suggest.
