# Layer 0-B — Router Pairs (053)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-053-001 — Gap: Llama + embedding model combined

A: [] | "Llama generates text while a separate embedding model encodes queries for retrieval."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-053-002 — IMPLEMENT: implement Llama + embedding orchestrator

A: ["IMPLEMENT"] | "Implement an orchestrator that calls Llama for generation and the embedding model for retrieval."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. Both AI model names in object position.

---

## C0B-053-003 — Gap: semantic search + knowledge graph enrichment

A: [] | "Semantic search results are enriched with knowledge graph relationships."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## C0B-053-004 — IMPLEMENT+API_CONTRACT: build semantic search + integrate knowledge graph API

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the semantic search module and integrate it with the knowledge graph REST API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33.
Teaching: "build" fires IMPLEMENT; "integrate" fires API_CONTRACT. Standard order.

---

## C0B-053-005 — TEST: write tests for semantic search relevance

A: ["TEST"] | "Write tests for the semantic search service to validate result relevance scores."
B: TEST → TestFlow51.
Teaching: "write tests" fires TEST. "semantic search service" = object context.
