# Layer 0-B — Router Pairs (047)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-047-001 — Gap: embedding model standalone

A: [] | "The embedding model maps text to 768-dimensional vectors for similarity search."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "embedding model" = AI model type. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-047-002 — IMPLEMENT: build embedding model client

A: ["IMPLEMENT"] | "Build an embedding model client that batches requests and caches results."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "embedding model client" = implementation artifact.

---

## C0B-047-003 — Gap: vector database standalone

A: [] | "The vector database stores high-dimensional embeddings for similarity retrieval."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "vector database" = AI infrastructure term. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-047-004 — IMPLEMENT: implement vector database connector

A: ["IMPLEMENT"] | "Implement a vector database connector that abstracts CRUD (create, read, update, delete) operations."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "vector database connector" = implementation artifact.

---

## C0B-047-005 — API_CONTRACT: integrate embedding model with vector database

A: ["API_CONTRACT"] | "Integrate the embedding model API with the vector database write endpoint."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. Both AI terms in object position suppressed.
