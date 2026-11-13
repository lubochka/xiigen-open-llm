# Layer 0-A — Compiler Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-021-001 — gap: "adapt the agent" — pure AI gap

A: "Adapt the agent to handle Spanish-language customer queries."
B: [] — "adapt the agent" = AI adaptation vocabulary. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-021-002 — IMPLEMENT fires: "build the agent" boundary

A: "Build the agent that handles Spanish-language customer queries."
B: ["IMPLEMENT"] — "build the agent" = code verb + code artifact (agent service). IMPLEMENT fires.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-021-003 — gap: "adapt" vs "build" side-by-side

A: "Adapt the existing agent for a new vertical rather than build a new one."
B: [] — "adapt" = AI vocabulary; "build a new one" is subordinate to "rather than" negative framing. Predicate set scoped to primary action: "adapt" → gap.
CompiledPredicateIds: []

---

## C0A-021-004 — IMPLEMENT fires: "build vs adapt" resolved by explicit build

A: "Instead of adapting the agent, build a dedicated vertical-specific agent from scratch."
B: ["IMPLEMENT"] — "build" is the primary action verb. "adapting" is in a negative subordinate clause and does not fire. IMPLEMENT fires on "build".
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-021-005 — gap: "evolve the agent" boundary

A: "Evolve the agent over time by incorporating user feedback into its behavior."
B: [] — "evolve the agent" = AI capability evolution vocabulary. No code verb fires. Gap.
CompiledPredicateIds: []
