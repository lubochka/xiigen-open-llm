# Layer 0-D — Flow Factory Pairs (042)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-042-001 — IMPLEMENT: build embedding + vector storage pipeline

A: ["IMPLEMENT"] | "Build an embedding pipeline that stores vectors in the vector database and exposes a query interface."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "embedding pipeline" = software artifact.

---

## FF-042-002 — API_CONTRACT: integrate GPT and BERT endpoints

A: ["API_CONTRACT"] | "Integrate the GPT and BERT inference endpoints into a unified model gateway."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. Both model names suppressed.

---

## FF-042-003 — IMPLEMENT+API_CONTRACT: build Llama + embedding orchestrator

A: ["IMPLEMENT"] | "Implement an orchestrator that calls Llama for generation and the embedding model for retrieval."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "orchestrator" here = software artifact being built.

---

## FF-042-004 — IMPLEMENT+API_CONTRACT: build semantic search + integrate knowledge graph

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the semantic search module and integrate it with the knowledge graph REST API."
Factory: [APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: APIContractFlow33 → ImplementationFlow41.

---

## FF-042-005 — TEST: write tests for semantic search

A: ["TEST"] | "Write tests for the semantic search service to validate result relevance scores."
Factory: [TestFlow51] | TransitionType: N/A (testing phase)
Teaching: "write tests" fires TEST. "semantic search service" = object context.
