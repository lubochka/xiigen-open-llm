# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-015-001 — "walk through" full disambiguation table

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Walk through how the API works" | [] gap | explain intent → gap |
| "Walk through integrating the API" | API_CONTRACT | "integrate" fires always |
| "Walk through building the service" + "implement it" | IMPLEMENT | direct "implement" action fires |
| "Walk through writing tests" | TEST | embedded "writing tests" → TEST |
| "Walk through architecting" = explain past design | [] gap | explain context → gap |
| "Walk through to design the architecture" | ARCHITECTURE | embedded "design" → ARCHITECTURE |

---

## KE-015-002 — "walk through" vs "walk through to [action]"

"Walk through [noun/topic]" = explain/present = gap
"Walk through [verb-ing]" = guidance frame + participial action = code verb may fire
"Walk through to [infinitive action]" = guidance frame + purpose infinitive = code verb fires

Examples:
"Walk through the codebase" → gap (explain existing artifact)
"Walk through deploying the service" → gap (deploy is not a predicate-firing verb; deploy alone ≠ IMPLEMENT)
"Walk through implementing the service" → IMPLEMENT fires (implementing + service noun)
"Walk through integrating the API" → API_CONTRACT fires (integrate always fires)

---

## KE-015-003 — "explain" vs "walk through" — identical treatment

Both "explain" and "walk through" when used to present/describe existing artifacts or concepts = gap.
Both may have embedded code-action verbs that fire predicates.
"Explain and implement" → IMPLEMENT fires (explain = gap warning).
"Walk through and implement" → IMPLEMENT fires (walk through = gap warning).
