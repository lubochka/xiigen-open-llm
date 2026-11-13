# Layer 0-A — Compiler Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-016-001 — gap: "prompt engineering" as strategy

A: "We need better prompt engineering to reduce hallucinations in the customer support agent."
B: [] — "prompt engineering" = AI vocabulary. "need" is not a code verb. Gap.
CompiledPredicateIds: []

---

## C0A-016-002 — gap: "prompt engineering" as discipline

A: "Invest in prompt engineering to align the agent's tone with our brand guidelines."
B: [] — "invest in prompt engineering" = AI capability strategy. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-016-003 — IMPLEMENT fires: build prompt engineering toolkit

A: "Build a prompt engineering toolkit with a template manager and a/b test harness."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "prompt engineering toolkit" is the software artifact; "prompt engineering" does not suppress the code verb.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-016-004 — IMPLEMENT+TEST fires: create prompt template library + tests

A: "Create the prompt template library and write regression tests to ensure templates produce valid outputs."
B: ["IMPLEMENT", "TEST"] — "create" → IMPLEMENT; "write regression tests" → TEST. "prompt template" is AI-adjacent but code artifact.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## C0A-016-005 — gap: "prompt engineering" self-assessment

A: "The current prompt engineering approach is producing inconsistent agent responses."
B: [] — observation statement. No action verb. "prompt engineering" + "agent responses" → gap.
CompiledPredicateIds: []
