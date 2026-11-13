# Layer 0-A — Compiler Pairs (027)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-027-001 — gap: "RAG pipeline" as AI system goal

A: "Set up a RAG (Retrieval-Augmented Generation) pipeline for the support bot."
B: [] — "set up" in AI context = model-level operation, not code verb. "RAG pipeline" = AI system goal. Gap.
CompiledPredicateIds: []

---

## C0A-027-002 — IMPLEMENT fires: build RAG pipeline

A: "Implement a RAG pipeline that retrieves documents and passes context to the LLM."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "RAG pipeline", "LLM" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-027-003 — gap: "context window" vocabulary

A: "The context window needs to be managed to avoid token overflow."
B: [] — "context window" = AI model property. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-027-004 — IMPLEMENT fires: build context window manager

A: "Build a context window manager that trims conversation history."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "context window" is object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-027-005 — gap: "attention mechanism" vocabulary

A: "The attention mechanism is not capturing long-range dependencies."
B: [] — "attention mechanism" = AI model internal component. No code verb. Gap.
CompiledPredicateIds: []
