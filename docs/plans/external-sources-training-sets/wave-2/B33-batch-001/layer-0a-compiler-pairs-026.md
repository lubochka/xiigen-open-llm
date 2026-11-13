# Layer 0-A — Compiler Pairs (026)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-026-001 — gap: "embed" vocabulary without code verb

A: "We need to embed the documents before indexing them."
B: [] — "embed" = AI embedding operation. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-026-002 — gap: "vector search" as AI retrieval operation

A: "Use vector search to find the most relevant documents."
B: [] — "vector search" = AI retrieval method. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-026-003 — API_CONTRACT fires: vector search via API

A: "Integrate the vector search API into the document retrieval flow."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. "vector search API" is object context.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-026-004 — IMPLEMENT fires: build vector search

A: "Build a vector search component that queries the embedding store."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "vector search" and "embedding store" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-026-005 — IMPLEMENT + API_CONTRACT: build and integrate vector search

A: "Implement the vector search module and integrate it with the Pinecone API."
B: ["IMPLEMENT", "API_CONTRACT"] — "implement" fires IMPLEMENT; "integrate" fires API_CONTRACT. Two code verbs, two predicates.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
