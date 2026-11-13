# Layer 0-A — Compiler Pairs (052)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-052-001 — gap: vector database + embedding model combined

A: "The vector database stores embeddings produced by the embedding model."
B: [] — Descriptive. No code verb. Both terms gap-ai-ml-model-001. One event.
CompiledPredicateIds: []

---

## C0A-052-002 — IMPLEMENT: build embedding + vector storage pipeline

A: "Build an embedding pipeline that stores vectors in the vector database and exposes a query interface."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "embedding pipeline" and "vector database" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-052-003 — gap: GPT + BERT comparison

A: "GPT generates text autoregressively while BERT uses bidirectional encoding."
B: [] — Explanatory statement. No code verb. Both AI model names gap. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-052-004 — API_CONTRACT: integrate GPT and BERT endpoints into unified gateway

A: "Integrate the GPT and BERT inference endpoints into a unified model gateway."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. Both model names in object position.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-052-005 — SECURITY: secure LLM inference endpoint

A: "Secure the LLM inference endpoint against unauthorized access and prompt injection."
B: ["SECURITY"] — "secure" fires SECURITY; "prompt injection" reinforces SECURITY. "LLM" = object context.
CompiledPredicateIds: ["SECURITY"]
