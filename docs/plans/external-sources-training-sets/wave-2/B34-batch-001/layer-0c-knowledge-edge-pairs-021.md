# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-021-001 — Guidance + API_CONTRACT embedded: complete rule reference

API_CONTRACT fires when: "integrate/connect" verb present in any position (requester's action or embedded).
"integrate" always fires API_CONTRACT — this is the most aggressive predicate rule.

| Request | API_CONTRACT fires? | Reason |
|---------|---------------------|--------|
| "Walk through integrating the payment API" | yes | integrate in guidance frame → fires |
| "Guide through integrating the SSO" | yes | integrate in guidance frame → fires |
| "Facilitate the integration of A with B" | yes | integration noun as direct object → fires |
| "Show how to integrate the webhook" | yes | embedded "integrate" → fires |
| "Walk through the API" (no integrate) | no | explain intent; no integrate verb |
| "Teach to integrate systems" | no | "teach to X" exception; infinitive = learning target |
| "Advise on integration patterns" | no | advisory + "integration patterns" noun = gap; no "integrate" verb |

---

## KE-021-002 — "integration patterns" vs "integrating"

"Advise on integration patterns" → gap (patterns = advisory topic noun; "integrate" verb not present)
"Walk through integrating" → API_CONTRACT (integrate verb present as participial action)
"Discuss integration strategy" → gap (discuss = advisory; "integration strategy" = topic noun)
"Document the integration guide" → DOCUMENT (document verb fires; integration = context noun)

---

## KE-021-003 — "teach to integrate" — the infinitive exception

"Teach the team to integrate third-party services" → gap
"Teach to integrate" = "teach to X" pattern → gap (infinitive "to integrate" = learning target)
"Show how to integrate" → API_CONTRACT (embedded "integrate" fires; "show how to" ≠ "teach to")

Difference: "teach to" = direct infinitive of "teach" = gap-pattern. "show how to" = guidance frame followed by how-to construction that embeds the action verb → API_CONTRACT fires from "integrate."
