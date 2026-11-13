# Layer 0-D — Flow Factory Pairs (046)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-046-001 — IMPLEMENT: refactor LLM client for latency

A: ["IMPLEMENT"] | "Refactor the LLM client to reduce inference latency and add request pooling."
Factory: [ImplementationFlow41] | TransitionType: refactoring
Teaching: "refactor" fires IMPLEMENT. TransitionType = refactoring (refactor without new feature).

---

## FF-046-002 — SECURITY: secure vector database endpoint

A: ["SECURITY"] | "Secure the vector database endpoint against unauthorized read and write access."
Factory: [SecurityAuditFlow] | TransitionType: N/A (security audit)
Teaching: "secure" fires SECURITY. "vector database endpoint" = object context.

---

## FF-046-003 — GapHandler: knowledge graph + semantic search

A: [] | "Knowledge graph enrichment improves semantic search recall by adding entity links."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive. No code verb. Both gap-ai-ml-model-001 terms. One event.

---

## FF-046-004 — IMPLEMENT+TEST: build knowledge graph + write tests

A: ["IMPLEMENT", "TEST"] | "Build the knowledge graph adapter and write integration tests for query results."
Factory: [ImplementationFlow41, TestFlow51] | TransitionType: feature_addition
Ordering: ImplementationFlow41 → TestFlow51.

---

## FF-046-005 — API_CONTRACT: integrate LLM + embedding + vector database APIs

A: ["API_CONTRACT"] | "Integrate the LLM, embedding model, and vector database APIs into the search service."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires one API_CONTRACT regardless of how many APIs are listed.
