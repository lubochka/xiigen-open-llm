# Layer 1 — Transition Orchestrator Pairs (038)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-038-001 — Orchestration: embedding + vector database + semantic search gap chain

Gap request: "Embedding model produces vectors stored in vector database queried by semantic search."
Predicates: [] | GapHandler → gap-ai-ml-model-001 | One SCHEDULE_CAPABILITY_LEARNING.

---

## TO-038-002 — Orchestration: design + integrate + implement embedding pipeline

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"]
Flow order: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 | TransitionType: feature_addition

---

## TO-038-003 — Orchestration: refactor embedding client

Predicates: ["IMPLEMENT"]
Request: "Refactor the embedding model client to use connection pooling."
Flow: ImplementationFlow41 | TransitionType: refactoring
Teaching: "refactor" fires IMPLEMENT. TransitionType = refactoring.

---

## TO-038-004 — v6 corruption: EMBEDDING_MODEL invented predicate (v6-01)

v6 corrupt: "embedding model" → "EMBED" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "embedding model" is AI vocabulary. No EMBED predicate exists.

---

## TO-038-005 — Orchestration: implement + test + document semantic search service

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
Flow order: ImplementationFlow41 → TestFlow51 → DocumentationFlow62 | TransitionType: feature_addition
