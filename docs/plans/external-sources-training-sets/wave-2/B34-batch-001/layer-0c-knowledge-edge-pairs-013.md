# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-013-001 — "shadow" full disambiguation table

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Shadow me on the deployment" | [] gap | shadow = observation → gap |
| "Shadow her while she refactors" | [] gap | third-party action; requester = shadow = gap |
| "Shadow and then build the same pattern" | IMPLEMENT | embedded "build" by requester fires |
| "Shadow and write the runbook" | DOCUMENT | embedded "write" by requester fires |
| "Shadow the QA lead and write regression tests" | TEST | embedded "write tests" by requester fires |

---

## KE-013-002 — "shadow in code context" edge case

"Shadow the CI/CD configuration" (as a verb meaning observe/copy): gap
"The service shadows another service's config" (technical usage, architecture): ARCHITECTURE may fire if architect verb also present
Rule: "shadow" as a guidance/observation verb always → gap. "shadow" as a technical term (config shadowing) is rare and context-specific; default to gap unless explicit architecture or implement verb accompanies it.

---

## KE-013-003 — Third-party embedded action vs requester embedded action

"Shadow the engineer while she builds the service" → gap (she builds; requester shadows)
"Shadow the engineer and build the same service yourself" → IMPLEMENT (requester builds)
"Pair with the team while they write tests" → gap (they write; requester pairs)
"Pair with the team to write tests together" → TEST (requester writes together = requester's action)

Principle: The actor of the embedded code verb determines whether the predicate fires. Requester's action = predicate fires. Third-party action = no predicate.
