# Layer 0-D — Flow Factory Pairs (038)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-038-001 — GapHandler: semantic search standalone

A: [] | "Semantic search returns results based on meaning rather than keyword overlap."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "semantic search" = AI retrieval capability. No code verb.

---

## FF-038-002 — IMPLEMENT: build semantic search service

A: ["IMPLEMENT"] | "Build a semantic search service that combines embedding lookup with BM25 reranking."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "semantic search service" = software artifact.

---

## FF-038-003 — GapHandler: knowledge graph standalone

A: [] | "The knowledge graph links entities and relationships for reasoning support."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "knowledge graph" = AI knowledge representation. No code verb.

---

## FF-038-004 — IMPLEMENT: implement knowledge graph query adapter

A: ["IMPLEMENT"] | "Implement a knowledge graph query adapter that translates natural language to SPARQL."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "knowledge graph query adapter" = implementation artifact.

---

## FF-038-005 — IMPLEMENT+API_CONTRACT: build semantic search + integrate knowledge graph API

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the semantic search layer using the embedding model, integrate with the knowledge graph API."
Factory: [APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: APIContractFlow33 → ImplementationFlow41 (API_CONTRACT before IMPLEMENT).
