# Layer 0-A — Compiler Pairs (025)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-025-001 — gap: "tokenize" as AI model operation

A: "Tokenize the input before passing it to the model."
B: [] — "tokenize" = AI model-level operation (splitting text into tokens for LLM input). No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-025-002 — IMPLEMENT overrides tokenize as object

A: "Build a tokenizer that splits the input text into subword units."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "tokenizer" is the implementation artifact. Gap vocabulary in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-025-003 — gap: "embed" as AI model operation

A: "Embed the user query using the sentence transformer."
B: [] — "embed" = AI model embedding operation. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-025-004 — IMPLEMENT fires: build embedding system

A: "Implement a service that calls the embedding model and stores the vectors."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "embedding model" is object context, not predicate source.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-025-005 — API_CONTRACT fires: integrate embedding endpoint

A: "Integrate the embedding endpoint into the search pipeline."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT. "embedding endpoint" is AI context in object position.
CompiledPredicateIds: ["API_CONTRACT"]
