# Layer 0-A — Compiler Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-002-001 — gap: "adapt the agent"

A: "Adapt the agent to handle customer support queries."
B: [] — "adapt" (AI sense) + "agent" → gap vocabulary. No predicate fires.
CompiledPredicateIds: []

---

## C0A-002-002 — gap: "AI orchestration"

A: "Set up an AI orchestration layer for multi-agent workflows."
B: [] — "AI orchestration" + "multi-agent" → gap. "set up" is operational gap. No predicate.
CompiledPredicateIds: []

---

## C0A-002-003 — IMPLEMENT fires on "implement an AI agent" (code verb wins)

A: "Implement an AI agent that routes customer queries to the correct department."
B: ["IMPLEMENT"] — "implement" (code-writing verb) fires IMPLEMENT. Object is AI vocabulary but verb determines predicate.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-002-004 — gap: "LLM prompt engineering"

A: "Improve the LLM prompt engineering for the support bot."
B: [] — "LLM", "prompt engineering" → gap vocabulary. "improve" alone does not fire a predicate.
CompiledPredicateIds: []

---

## C0A-002-005 — ARCHITECTURE fires on "design the agent architecture"

A: "Design the multi-agent orchestration architecture."
B: ["ARCHITECTURE"] — "design" (architecture verb) fires ARCHITECTURE. AI/agent object does not suppress it.
CompiledPredicateIds: ["ARCHITECTURE"]
