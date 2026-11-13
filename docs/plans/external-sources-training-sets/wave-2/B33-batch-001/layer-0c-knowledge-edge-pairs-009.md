# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-009-001 — "personalization" edge

"agent personalization" (AI behavior) → gap without code verb
"personalize agent responses" → gap (AI configuration)
BUT: "build the user personalization service" → IMPLEMENT
"implement personalization preferences storage" → IMPLEMENT

---

## KE-009-002 — "classification" edge

"classification model" → gap (ML model vocabulary)
"intent classification" (without code verb) → gap
BUT: "implement the intent classification service" → IMPLEMENT
"write tests for the classifier" → TEST
"build the classification pipeline" → IMPLEMENT

---

## KE-009-003 — "inference" edge

"inference" / "model inference" → gap (ML inference vocabulary)
"inference latency" → gap
"inference cost" → gap
BUT: "build the inference API server" → IMPLEMENT
"implement inference caching" → IMPLEMENT

---

## KE-009-004 — "context" edge in AI

"context window" (LLM context length) → gap
"agent context" (AI state) → gap
"context management" (AI conversation context) → gap without code verb
BUT: "implement context persistence for multi-turn conversations" → IMPLEMENT
"build the conversation context store" → IMPLEMENT

---

## KE-009-005 — "API" disambiguation: agent API vs API_CONTRACT

"the agent API" (describing a feature) → not automatically API_CONTRACT
"integrate the agent API" → API_CONTRACT ("integrate" verb fires API_CONTRACT)
"document the agent API" → DOCUMENT
"define the agent API contract" → API_CONTRACT ("define" + "contract" vocabulary)
"implement the agent API" → IMPLEMENT ("implement" fires IMPLEMENT)
Rule: The verb, not "API", determines the predicate.
