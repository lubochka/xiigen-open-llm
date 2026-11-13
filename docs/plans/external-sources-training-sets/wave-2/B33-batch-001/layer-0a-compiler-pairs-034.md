# Layer 0-A — Compiler Pairs (034)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-034-001 — gap: "tokenize" + "embed" compound gap request

A: "Tokenize the queries and embed them using the sentence model."
B: [] — "tokenize" and "embed" both = AI model operations. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-034-002 — gap: "context window" + "attention mechanism" compound

A: "Manage the context window and tune the attention mechanism for long documents."
B: [] — "context window" and "attention mechanism" both = AI model properties. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-034-003 — API_CONTRACT fires: integrate tokenizer endpoint

A: "Wire the tokenizer service into the preprocessing pipeline via API."
B: ["API_CONTRACT"] — "wire into" fires API_CONTRACT. "tokenizer service" is object context.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-034-004 — IMPLEMENT + TEST fires: build and test embedding pipeline

A: "Implement the embedding pipeline and write unit tests for the vector output."
B: ["IMPLEMENT", "TEST"] — "implement" fires IMPLEMENT; "write unit tests" fires TEST.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## C0A-034-005 — all predicates: full AI vocabulary + all code verbs

A: "Architect the RAG platform, integrate the embedding API, implement the retrieval service, secure against injection, write tests for context window handling, document the transformer config, QA."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — seven code verbs fire seven predicates.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
