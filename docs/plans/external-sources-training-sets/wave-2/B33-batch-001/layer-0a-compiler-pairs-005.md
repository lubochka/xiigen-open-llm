# Layer 0-A — Compiler Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-005-001 — gap: "few-shot prompting"

A: "Add few-shot examples to the classifier prompt."
B: [] — "add few-shot examples to prompt" → AI/prompt vocabulary gap. "add" here is prompt-engineering context, not code feature addition.
CompiledPredicateIds: []

---

## C0A-005-002 — TEST fires: "write tests for the agent"

A: "Write unit tests for the agent routing logic."
B: ["TEST"] — "write unit tests" fires TEST (5.1, unit context). "agent" is object, not a gap trigger when test-writing verb is present.
CompiledPredicateIds: ["TEST"]

---

## C0A-005-003 — gap: "vector embeddings"

A: "Improve the vector embeddings for the semantic search agent."
B: [] — "improve vector embeddings" → AI/ML vocabulary gap. No code-writing verb.
CompiledPredicateIds: []

---

## C0A-005-004 — DOCUMENT fires: "document the agent API"

A: "Document the AI agent API endpoints and tool-calling schema."
B: ["DOCUMENT"] — "document" fires DOCUMENT. AI context is object; documentation verb determines predicate.
CompiledPredicateIds: ["DOCUMENT"]

---

## C0A-005-005 — gap: "LLM hallucination"

A: "Reduce LLM hallucination in the support agent responses."
B: [] — "reduce LLM hallucination" → AI behavior gap. No code-writing or architecture verb.
CompiledPredicateIds: []
