# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-004-001 — "skill" gap edge (AI capability sense vs software context)

"AI skill" / "agent skill" / "add a skill to the agent" (AI capability sense) → gap if "add" = prompt/config action
"skill routing" / "skill selection" (AI) → gap
BUT: "implement a new skill module" → IMPLEMENT (code verb; "skill" is object of code implementation)
"create an escalation skill service" → IMPLEMENT

Rule: "skill" in AI capability sense without code verb → gap. Code verb present → predicate fires.

---

## KE-004-002 — "tool" gap edge in AI tool-calling context

"tool-calling" / "tool-use" → gap (AI capability vocabulary)
"enable tool calling" → gap
"define tool schema" → depends: "define" alone is gap; "define the tool API schema" → API_CONTRACT if schema/contract context
"build a tool registry" → IMPLEMENT (build is code verb)
"implement tool-calling support" → IMPLEMENT

---

## KE-004-003 — "embedding" and "vector" gap edges

"improve embeddings" → gap
"change embedding model" → gap
"vector search not accurate" → gap (observation)
"build the vector index service" → IMPLEMENT (code verb)
"design the vector database architecture" → ARCHITECTURE (design verb)

---

## KE-004-004 — "hallucination" gap edge

"reduce hallucinations" → gap
"hallucination guard" (without code verb) → gap
BUT: "build a hallucination detection service" → IMPLEMENT
"implement output validation to catch hallucinations" → IMPLEMENT + SECURITY context

---

## KE-004-005 — "agent memory" gap edge

"agent memory" (AI capability) → gap without code verb
"add agent memory" (AI sense, not code) → gap
BUT: "build a memory service for the agent" → IMPLEMENT
"implement a session memory store" → IMPLEMENT
