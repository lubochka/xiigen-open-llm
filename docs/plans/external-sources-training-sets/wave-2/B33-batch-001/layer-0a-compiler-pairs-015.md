# Layer 0-A — Compiler Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-015-001 — gap: "chain of thought" as reasoning request

A: "Enable chain of thought reasoning so the agent explains its decisions step by step."
B: [] — "chain of thought" = AI reasoning vocabulary. "enable" in AI behavior context → gap. No code verb fires.
CompiledPredicateIds: []

---

## C0A-015-002 — gap: "chain-of-thought prompting" strategy

A: "Apply chain-of-thought prompting to improve multi-step arithmetic accuracy."
B: [] — "apply chain-of-thought prompting" = AI prompting strategy. "apply" does not fire a code predicate. Gap.
CompiledPredicateIds: []

---

## C0A-015-003 — IMPLEMENT fires: implement CoT (chain-of-thought) prompt renderer

A: "Implement a chain-of-thought prompt renderer that formats reasoning steps into the system message."
B: ["IMPLEMENT"] — "implement" = code verb. Software artifact is "renderer". CoT (chain-of-thought) is AI object only.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-015-004 — ARCHITECTURE fires: design chain-of-thought reasoning layer

A: "Design the chain-of-thought reasoning layer for the agent's response pipeline."
B: ["ARCHITECTURE"] — "design" + structural noun "layer" → ARCHITECTURE. "chain-of-thought" is AI context only.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-015-005 — gap: "chain of thought" evaluation

A: "Does chain of thought reasoning reduce hallucinations in our production agent?"
B: [] — question about AI behavior quality. No code action verb. "chain of thought" + "hallucinations" both → gap.
CompiledPredicateIds: []
