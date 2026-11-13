# Layer 0-B — Router Pairs (048)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-048-001 — Gap: semantic search standalone

A: [] | "Semantic search returns results based on meaning rather than keyword overlap."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "semantic search" = AI retrieval capability. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-048-002 — IMPLEMENT: build semantic search service

A: ["IMPLEMENT"] | "Build a semantic search service that combines embedding lookup with BM25 reranking."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "semantic search service" = software artifact.

---

## C0B-048-003 — Gap: knowledge graph standalone

A: [] | "The knowledge graph links entities and relationships for reasoning support."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "knowledge graph" = AI knowledge representation. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-048-004 — IMPLEMENT: implement knowledge graph query adapter

A: ["IMPLEMENT"] | "Implement a knowledge graph query adapter that translates natural language to SPARQL."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "knowledge graph query adapter" = implementation artifact.

---

## C0B-048-005 — IMPLEMENT+API_CONTRACT: build semantic search + integrate knowledge graph API

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the semantic search layer using the embedding model, integrate with the knowledge graph API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33.
Teaching: "build" fires IMPLEMENT; "integrate" fires API_CONTRACT. All AI vocabulary in object position.
