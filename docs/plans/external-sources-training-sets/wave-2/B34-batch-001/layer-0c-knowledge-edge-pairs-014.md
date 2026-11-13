# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-014-001 — "pair with" full disambiguation table

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Pair with the team on their API work" | [] gap | pair-with = collaborative learning → gap |
| "Pair with to learn the architecture" | [] gap | learning intent → gap |
| "Pair with me to write unit tests" | TEST | embedded "write unit tests" → TEST |
| "Pair with the team to integrate the SSO" | API_CONTRACT | embedded "integrate" always fires |
| "Pair with to implement the service" | IMPLEMENT | embedded "implement" fires |
| "Pair with to learn how to implement" | [] gap | "teach to X" / learning-intent pattern → gap |

---

## KE-014-002 — "pair with" vs "pair program" vs "pair on"

"Pair with [person] on [task]" = collaborative learning → gap
"Pair program with [person]" = pair programming = mentoring vocabulary → gap
"Pair with to [direct action verb]" = guidance frame + embedded action → code verb fires if requester performs it
"Pair on [topic]" = topic pairing = gap (no embedded code action)

---

## KE-014-003 — Embedded TEST rule: "write" is the key verb

"Pair with to write tests" → TEST fires (write = TEST trigger)
"Pair with on testing" → gap (on = advisory framing, no write verb)
"Pair with to run tests" → gap (run ≠ TEST trigger; TEST fires on write/create/assert only)
"Pair with to create automated tests" → TEST fires (create + test noun → TEST)
