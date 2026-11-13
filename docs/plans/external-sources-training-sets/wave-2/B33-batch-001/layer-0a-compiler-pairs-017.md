# Layer 0-A — Compiler Pairs (017)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-017-001 — gap: "self-healing" agent behavior

A: "The agent should be self-healing — automatically correcting errors in its reasoning chain."
B: [] — "self-healing" = AI agent behavior vocabulary. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-017-002 — gap: "self-healing" as desired capability

A: "We want the AI agent to be self-healing when it encounters unexpected inputs."
B: [] — "want to be self-healing" = AI capability desire. "want" does not fire a code predicate. Gap.
CompiledPredicateIds: []

---

## C0A-017-003 — IMPLEMENT fires: implement self-healing retry logic

A: "Implement self-healing retry logic that re-prompts the agent with corrective context when output validation fails."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "retry logic" is a software artifact; "self-healing" is AI-adjacent adjective.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-017-004 — ARCHITECTURE+IMPLEMENT fires: design + build self-healing system

A: "Design the self-healing architecture for agent failure recovery and implement the fallback routing service."
B: ["ARCHITECTURE", "IMPLEMENT"] — "design" + "architecture" (structural noun) → ARCHITECTURE; "implement" → IMPLEMENT. "self-healing" is AI descriptor.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

---

## C0A-017-005 — gap: "self-healing" evaluation question

A: "How do self-healing agents decide when to retry versus escalate?"
B: [] — question about AI agent behavior strategy. No code verb. Gap.
CompiledPredicateIds: []
