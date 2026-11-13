# Layer 0-A — Compiler Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-014-001 — gap: "few-shot learning" as strategy request

A: "Use few-shot learning to help the agent handle new query categories."
B: [] — "few-shot learning" = AI training strategy vocabulary. "use" does not fire a code predicate. Gap.
CompiledPredicateIds: []

---

## C0A-014-002 — gap: "few-shot examples" in prompt

A: "Add a few-shot example block to the system prompt so the agent returns structured JSON."
B: [] — "add to the system prompt" = AI configuration (prompt modification). Not a code artifact. Gap.
CompiledPredicateIds: []

---

## C0A-014-003 — IMPLEMENT fires: build few-shot example loader

A: "Build a few-shot example loader that reads from the database and injects examples into the prompt template."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. Software artifact (example loader) is the object; "few-shot" is AI context only.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-014-004 — gap: "few-shot" evaluation request

A: "Evaluate whether few-shot prompting outperforms zero-shot for our classification task."
B: [] — "evaluate" = AI quality assessment vocabulary. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-014-005 — IMPLEMENT+TEST fires: build + test few-shot harness

A: "Implement the few-shot example retrieval service and write unit tests to cover edge-case inputs."
B: ["IMPLEMENT", "TEST"] — "implement" → IMPLEMENT; "write unit tests" → TEST. "few-shot" is object context only.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]
