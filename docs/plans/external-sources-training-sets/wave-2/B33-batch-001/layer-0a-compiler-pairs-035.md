# Layer 0-A — Compiler Pairs (035)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-035-001 — gap: "model serving" + "inference endpoint" compound

A: "Model serving should route requests to the correct inference endpoint."
B: [] — "model serving" and "inference endpoint" both = AI operational vocabulary. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-035-002 — IMPLEMENT fires: create serving router

A: "Create a serving router that dispatches requests to the appropriate inference endpoint."
B: ["IMPLEMENT"] — "create" fires IMPLEMENT. AI vocabulary in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-035-003 — gap: "hallucination mitigation" + "RAG pipeline" compound

A: "Apply hallucination mitigation to the RAG pipeline output."
B: [] — "hallucination mitigation" and "RAG pipeline" both = AI vocabulary. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-035-004 — gap: adjective AI terms do not suppress code verbs

A: "Build a RAG-powered, hallucination-mitigating document search service."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "RAG-powered" and "hallucination-mitigating" are adjectives modifying the artifact; they do not suppress IMPLEMENT.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-035-005 — gap: "latency optimization" without code verb

A: "Latency optimization for the transformer-based model serving layer is needed."
B: [] — "latency optimization" = AI operational concern. "transformer-based" = adjective AI vocabulary. No code verb. Gap.
CompiledPredicateIds: []
