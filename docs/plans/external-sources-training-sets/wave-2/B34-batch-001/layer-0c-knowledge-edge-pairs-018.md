# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-018-001 — "facilitate" full disambiguation table

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Facilitate knowledge transfer" | [] gap | facilitation vocabulary → gap |
| "Facilitate a session" | [] gap | event facilitation → gap |
| "Facilitate integration of system A with B" | API_CONTRACT | "integrate" fires in facilitation context |
| "Facilitate the team writing tests" | [] gap | team writes = third-party action |
| "Facilitate and implement the feature" | IMPLEMENT | requester implements → IMPLEMENT |
| "Facilitate collaboration on architecture review" | [] gap | advisory/facilitation → gap |

---

## KE-018-002 — "facilitate integration" — critical rule

"facilitate integration" → API_CONTRACT fires.
"Integrate" always fires API_CONTRACT regardless of framing verb. "facilitate" as the outer verb does NOT suppress the integration action.
This is the most important edge case for facilitate: the integration noun/verb is real and must be routed.

Compare:
"Facilitate the team's implementation" → gap (team implements = third-party; facilitate = requester's action)
"Facilitate the integration of System A with B" → API_CONTRACT (integration action is real regardless of facilitate wrapper)

---

## KE-018-003 — Why does "integrate" always fire but "implement" does not in third-party context?

"Facilitate the integration of X with Y" → API_CONTRACT fires because:
- The integration action is a concrete technical coupling that must be routed regardless of who orchestrates it.
- "integrate" is a structural action, not a learning/guidance action.

"Facilitate the team's implementation" → gap because:
- The team is the actor; requester's role = facilitate = gap.
- "implementation" in this context is the team's output, not a direct routable action for this system.
