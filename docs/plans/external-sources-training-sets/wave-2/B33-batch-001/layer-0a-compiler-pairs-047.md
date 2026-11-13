# Layer 0-A — Compiler Pairs (047)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-047-001 — gap: embedding model standalone

A: "The embedding model maps text to 768-dimensional vectors for similarity search."
B: [] — "embedding model" = AI model type. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-047-002 — IMPLEMENT: build embedding model client

A: "Build an embedding model client that batches requests and caches results."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "embedding model client" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-047-003 — gap: vector database standalone

A: "The vector database stores high-dimensional embeddings for similarity retrieval."
B: [] — "vector database" = AI infrastructure term. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-047-004 — IMPLEMENT: implement vector database connector

A: "Implement a vector database connector that abstracts CRUD (create, read, update, delete) operations."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "vector database connector" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-047-005 — API_CONTRACT: integrate embedding model with vector database

A: "Integrate the embedding model API with the vector database write endpoint."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. Both AI terms in object position.
CompiledPredicateIds: ["API_CONTRACT"]
