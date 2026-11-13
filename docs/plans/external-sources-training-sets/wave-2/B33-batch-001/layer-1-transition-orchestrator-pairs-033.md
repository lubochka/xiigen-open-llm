# Layer 1 — Transition Orchestrator Pairs (033)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-033-001 — Orchestration: semantic search gap

Gap request: "Semantic search returns results based on meaning rather than keyword overlap."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-033-002 — Orchestration: build semantic search + integrate knowledge graph + write tests

Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Request: "Build the semantic search service, integrate with the knowledge graph API, and write tests for relevance."
Flow order: APIContractFlow33 → ImplementationFlow41 → TestFlow51
TransitionType: feature_addition

---

## TO-033-003 — Orchestration: knowledge graph gap

Gap request: "The knowledge graph links entities and relationships for reasoning support."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-033-004 — v6 corruption: semantic-search → IMPLEMENT without code verb (v6-02)

v6 corrupt: "semantic search" → IMPLEMENT (treating AI retrieval term as code verb)
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-02. "semantic search" is AI vocabulary. Does not fire IMPLEMENT without code verb.

---

## TO-033-005 — Orchestration: implement knowledge graph adapter + document schema

Predicates: ["IMPLEMENT", "DOCUMENT"]
Request: "Implement the knowledge graph query adapter and document the entity schema."
Flow order: ImplementationFlow41 → DocumentationFlow62
TransitionType: feature_addition
