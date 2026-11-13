# Layer 0-A — Compiler Pairs (033)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-033-001 — IMPLEMENT fires: build dataset curation tool

A: "Implement a dataset curation pipeline that filters and deduplicates examples."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "dataset curation pipeline" is the implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-033-002 — mixed: AI gap vocabulary + IMPLEMENT

A: "Build the training data pipeline and tune the hyperparameters to reduce overfitting."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "tune the hyperparameters" and "overfitting" are AI gap vocabulary in object/subordinate position; gap suppressed.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-033-003 — gap: tokenize + embed + RAG without code verb

A: "Tokenize the corpus, embed each document, and set up the RAG retrieval pipeline."
B: [] — "tokenize", "embed", and "set up the RAG" = AI model-level operations. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-033-004 — ARCHITECTURE fires: design tokenization architecture

A: "Design the tokenization and embedding architecture for the document store."
B: ["ARCHITECTURE"] — "design" + "architecture" (structural noun) fires ARCHITECTURE. AI vocabulary in object position.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-033-005 — gap: complete Round 2 AI gap boundary reference

A: "Tokenize, embed, vector search, RAG pipeline, context window, attention mechanism, transformer, inference endpoint, model serving, latency optimization, hallucination mitigation."
B: [] — All Round 2 AI/ML vocabulary items. No code verb. Single gap event.
CompiledPredicateIds: []
