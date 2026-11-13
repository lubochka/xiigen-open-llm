# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-014-001 — Edge: "few-shot" in prompt vs "few-shot" as system feature

"add few-shot examples to the prompt" → gap (AI prompt configuration; "add to prompt" = AI config, not code artifact)
"add a few-shot example database table" → IMPLEMENT (code artifact: database table)
Teaching: The object determines the gap/predicate boundary. "prompt" = AI artifact → gap. "database table" = code artifact → IMPLEMENT fires on "add".

---

## KE-014-002 — Edge: "few-shot" evaluation vs "few-shot" implementation

"evaluate few-shot prompting vs zero-shot" → gap ("evaluate" = AI quality assessment vocabulary)
"implement a few-shot vs zero-shot evaluation harness" → IMPLEMENT ("implement" = code verb; "harness" = software artifact)
Teaching: "evaluate" in AI context = gap. "implement the harness" = IMPLEMENT regardless of AI evaluation context.

---

## KE-014-003 — Edge: "few-shot" does not suppress API_CONTRACT

"integrate the few-shot example store with the recommendation API" → ["API_CONTRACT"]
"integrate few-shot retrieval with the embedding API" → ["API_CONTRACT"]
Teaching: "integrate" always fires API_CONTRACT. "few-shot" in the request does not suppress or replace API_CONTRACT with a gap.

---

## KE-014-004 — Edge: "few-shot" + "build" boundary

"build a few-shot learning agent" → IMPLEMENT (code verb; "agent" = software artifact)
"few-shot learning for the agent" → gap (no code verb; "for the agent" = AI context)
"use few-shot learning in the agent" → gap ("use" in AI context does not fire code predicate)

---

## KE-014-005 — Edge: "few-shot" as adjective does not suppress code verbs

"build a few-shot-capable retrieval service" → IMPLEMENT ("build" fires; "few-shot-capable" = adjective)
"design a few-shot example management architecture" → ARCHITECTURE ("design" + "architecture" = structural noun)
Teaching: "few-shot" as adjective modifying a software component does not suppress predicate firing.
