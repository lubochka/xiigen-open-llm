# Layer 0-D — Flow Factory Pairs (037)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-037-001 — GapHandler: embedding model standalone

A: [] | "The embedding model maps text to 768-dimensional vectors."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "embedding model" = AI model type. No code verb.

---

## FF-037-002 — IMPLEMENT: build embedding model client

A: ["IMPLEMENT"] | "Build an embedding model client that batches requests and caches results."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "embedding model client" = implementation artifact.

---

## FF-037-003 — GapHandler: vector database standalone

A: [] | "The vector database stores high-dimensional embeddings for similarity retrieval."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "vector database" = AI infrastructure term. No code verb.

---

## FF-037-004 — IMPLEMENT: implement vector database connector

A: ["IMPLEMENT"] | "Implement a vector database connector that abstracts CRUD operations."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "vector database connector" = implementation artifact.

---

## FF-037-005 — API_CONTRACT: integrate embedding model with vector database

A: ["API_CONTRACT"] | "Integrate the embedding model API with the vector database write endpoint."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. Both AI terms in object position.
