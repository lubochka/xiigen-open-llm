# Layer 0-A — Compiler Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-012-001 — gap: "agent reasoning"

A: "The agent reasoning is not transparent enough for compliance auditing."
B: [] — observation about AI agent behavior. No predicate fires.
CompiledPredicateIds: []

---

## C0A-012-002 — IMPLEMENT fires: "add reasoning trace logging"

A: "Add reasoning trace logging to the agent for compliance auditing."
B: ["IMPLEMENT"] — "add reasoning trace logging" → IMPLEMENT (software feature addition; logging is a code artifact).
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-012-003 — gap: "AI agent" as subject, strategic request

A: "We need an AI agent that can autonomously handle Tier 1 support tickets."
B: [] — requirement statement without predicate-triggering verb. "need an AI agent" → gap.
CompiledPredicateIds: []

---

## C0A-012-004 — IMPLEMENT+TEST+DOCUMENT fires: full AI feature cycle

A: "Implement the intent classification module, write unit tests for the classifier, and document the intent taxonomy."
B: ["IMPLEMENT", "TEST", "DOCUMENT"] — three predicates. AI context (intent classification) is object; code verbs fire predicates.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT"]

---

## C0A-012-005 — gap: "agentic" as adjective

A: "Make the workflow more agentic."
B: [] — "make more agentic" → vague AI vocabulary gap. No predicate fires.
CompiledPredicateIds: []
