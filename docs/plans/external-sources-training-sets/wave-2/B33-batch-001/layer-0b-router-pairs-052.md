# Layer 0-B — Router Pairs (052)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-052-001 — Gap: vector database + embedding model combined

A: [] | "The vector database stores embeddings produced by the embedding model."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-052-002 — IMPLEMENT: build embedding + vector storage pipeline

A: ["IMPLEMENT"] | "Build an embedding pipeline that stores vectors in the vector database and exposes a query interface."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. All AI terms in object position.

---

## C0B-052-003 — Gap: GPT + BERT comparison

A: [] | "GPT generates text autoregressively while BERT uses bidirectional encoding."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Explanatory. No code verb. Both AI model names gap. Gap ID: gap-ai-ml-model-001.

---

## C0B-052-004 — API_CONTRACT: integrate GPT and BERT endpoints

A: ["API_CONTRACT"] | "Integrate the GPT and BERT inference endpoints into a unified model gateway."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. Both model names suppressed.

---

## C0B-052-005 — SECURITY: secure LLM inference endpoint

A: ["SECURITY"] | "Secure the LLM inference endpoint against unauthorized access and prompt injection."
B: SECURITY → SecurityAuditFlow.
Teaching: "secure" fires SECURITY; "prompt injection" reinforces. "LLM" = object context.
