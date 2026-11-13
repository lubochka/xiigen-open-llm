# Layer 0-A — Compiler Pairs (029)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-029-001 — IMPLEMENT fires: latency optimization with code verb

A: "Refactor the serving pipeline to reduce inference latency."
B: ["IMPLEMENT"] — "refactor" fires IMPLEMENT. "inference latency" and "serving pipeline" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-029-002 — SECURITY fires: latency-related security hardening

A: "Secure the model serving endpoint against unauthorized access."
B: ["SECURITY"] — "secure" fires SECURITY. "model serving endpoint" is object context.
CompiledPredicateIds: ["SECURITY"]

---

## C0A-029-003 — gap: "hallucination mitigation" vocabulary

A: "Hallucination mitigation should be applied to reduce incorrect outputs."
B: [] — "hallucination mitigation" = AI quality concern. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-029-004 — IMPLEMENT fires: build hallucination guard

A: "Build a hallucination guard that validates model outputs against a fact store."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "hallucination guard" is the implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-029-005 — gap: complete AI gap boundary — tokenize, embed, RAG, context window, attention, transformer, inference, serving, hallucination

A: "Tokenize inputs, embed them, run the RAG pipeline, manage context window, apply attention mechanism, use transformer, serve via inference endpoint, optimize serving latency, mitigate hallucination."
B: [] — All AI model-level vocabulary. No code verb fires. Single gap.
CompiledPredicateIds: []
