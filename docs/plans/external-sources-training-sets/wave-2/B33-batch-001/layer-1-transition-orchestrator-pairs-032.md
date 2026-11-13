# Layer 1 — Transition Orchestrator Pairs (032)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-032-001 — Orchestration: embedding model gap

Gap request: "The embedding model maps text to 768-dimensional vectors."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-032-002 — Orchestration: build embedding client + integrate vector database

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Build the embedding model client and integrate it with the vector database API."
Flow order: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-032-003 — Orchestration: vector database gap

Gap request: "The vector database stores high-dimensional embeddings for similarity retrieval."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-032-004 — Orchestration: implement connector + secure endpoint + write tests

Predicates: ["IMPLEMENT", "SECURITY", "TEST"]
Request: "Implement the vector database connector, secure the write endpoint, and write unit tests."
Flow order: ImplementationFlow41 → SecurityAuditFlow → TestFlow51
TransitionType: feature_addition
Rule: IMPLEMENT before SECURITY (rule 4); IMPLEMENT before TEST (rule 3).

---

## TO-032-005 — v6 corruption: VECTOR_SEARCH invented predicate (v6-01)

v6 corrupt: "vector database" → "VECTOR_SEARCH" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "vector database" is AI vocabulary. No VECTOR_SEARCH predicate exists.
