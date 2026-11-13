# Layer 0-A — Compiler Pairs (048)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-048-001 — gap: semantic search standalone

A: "Semantic search returns results based on meaning rather than keyword overlap."
B: [] — "semantic search" = AI retrieval capability. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-048-002 — IMPLEMENT: build semantic search service

A: "Build a semantic search service that combines embedding lookup with BM25 (Best Match 25) reranking."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "semantic search service" = software artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-048-003 — gap: knowledge graph standalone

A: "The knowledge graph links entities and relationships for reasoning support."
B: [] — "knowledge graph" = AI knowledge representation. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-048-004 — IMPLEMENT: implement knowledge graph query adapter

A: "Implement a knowledge graph query adapter that translates natural language to SPARQL."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "knowledge graph query adapter" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-048-005 — AI+IMPLEMENT mixed: embedding model + semantic search + code verb

A: "Build the semantic search layer using the embedding model, integrate with the knowledge graph API."
B: ["IMPLEMENT", "API_CONTRACT"] — "build" fires IMPLEMENT; "integrate" fires API_CONTRACT. All AI vocabulary in object position.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
