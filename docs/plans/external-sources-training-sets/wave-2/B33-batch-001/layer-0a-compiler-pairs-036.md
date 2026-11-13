# Layer 0-A — Compiler Pairs (036)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-036-001 — gap: AI/ML vocabulary gap trigger list Round 2

A: "Tokenize, embed, vector search, RAG, context window, attention mechanism, transformer, inference endpoint, model serving, latency optimization, hallucination mitigation."
B: [] — All Round 2 AI/ML vocabulary. No code verb present. Entire request → gap.
CompiledPredicateIds: []

---

## C0A-036-002 — IMPLEMENT overrides all Round 2 AI objects

A: "Build a service that tokenizes input, embeds documents, runs vector search, and serves via an inference endpoint."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. All AI vocabulary (tokenize, embed, vector search, inference endpoint) is object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-036-003 — gap: Round 2 AI gap vocabulary without code verb

A: "The RAG pipeline needs context window management and hallucination mitigation."
B: [] — Requirement statement. No code verb fires. "RAG pipeline", "context window", "hallucination mitigation" → gap.
CompiledPredicateIds: []

---

## C0A-036-004 — API_CONTRACT fires in pure Round 2 AI vocabulary sentence

A: "Integrate the transformer inference endpoint with the embedding vector store."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT. "transformer inference endpoint" and "embedding vector store" are AI context only.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-036-005 — All predicates: Round 2 AI vocabulary + all code verbs

A: "Architect the RAG platform, integrate the embedding API, implement the tokenizer, secure the inference endpoint, test the vector search output, document the context window policy, QA."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — seven predicates. All Round 2 AI vocabulary items are objects; code verbs fire independently.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
