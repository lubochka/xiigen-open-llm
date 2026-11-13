# Layer 0-A — Compiler Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-010-001 — gap: "agent evaluation"

A: "Evaluate the agent's performance on a benchmark dataset."
B: [] — "evaluate performance" → observational/analytical gap. No predicate fires.
CompiledPredicateIds: []

---

## C0A-010-002 — IMPLEMENT fires: "create evaluation harness"

A: "Create an evaluation harness to benchmark the agent's accuracy."
B: ["IMPLEMENT"] — "create" fires IMPLEMENT. "evaluation harness" is a software artifact (test tool).
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-010-003 — gap: "autonomous decision-making"

A: "The agent needs better autonomous decision-making for edge cases."
B: [] — "autonomous decision-making" → AI behavior gap. No predicate fires.
CompiledPredicateIds: []

---

## C0A-010-004 — gap: "grounding"

A: "Improve the agent's grounding to reduce off-topic responses."
B: [] — "improve grounding" → AI capability gap. "improve" alone is not a code-writing verb.
CompiledPredicateIds: []

---

## C0A-010-005 — IMPLEMENT+SECURITY fires: build + secure AI endpoint

A: "Implement the agent API endpoint with input sanitization to prevent prompt injection."
B: ["IMPLEMENT", "SECURITY"] — "implement" → IMPLEMENT; "input sanitization", "prevent prompt injection" → SECURITY.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
