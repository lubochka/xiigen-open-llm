# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-017-001 — "advise" full disambiguation table

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Advise on best practices" | [] gap | advisory vocabulary → gap |
| "Advise on security" | [] gap | teaching about security ≠ SECURITY |
| "Advise and design the architecture" | ARCHITECTURE | embedded "design" fires |
| "Advise and harden the gateway" | SECURITY | embedded "harden" fires |
| "Advise and document the guidelines" | DOCUMENT | embedded "document" fires |
| "Advise and implement the solution" | IMPLEMENT | embedded "implement" fires |

---

## KE-017-002 — "advise on" vs "advise to [action]"

"Advise on [topic]" = advisory without embedded action = gap
"Advise to implement X" = gap ("advise to X" = recommend others implement = gap; requester's action = advise = gap)
"Advise and then implement X" = IMPLEMENT (requester performs implement after advising)

Key distinction: "advise to" (recommendation that others act) = gap; "advise and [do]" (requester also performs code action) = code predicate fires.

---

## KE-017-003 — "consult" and "recommend" as gap synonyms of "advise"

"Consult with the team on the approach" → gap (gap-skills-guidance-mentor-001)
"Recommend the best API design patterns" → gap (gap-skills-guidance-mentor-001)
"Review the approach with the team" → gap (review-with = mentoring vocabulary → gap)

All advisory/consultation verbs with no embedded requester code-action → gap.
