# Layer 0-A — Compiler Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-020-001 — gap: "orchestrate agent workflows" — AI context

A: "Orchestrate the agent workflows so tasks are handed off between specialist agents automatically."
B: [] — "orchestrate" in AI context = AI operational vocabulary. No code predicate fires. Gap.
CompiledPredicateIds: []

---

## C0A-020-002 — API_CONTRACT fires: "orchestrate via API" boundary

A: "Orchestrate the agent calls via the orchestration API to manage task routing."
B: ["API_CONTRACT"] — "orchestrate via API" — "integrate/via API" pattern → API_CONTRACT fires. The API object triggers API_CONTRACT despite "orchestrate".
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-020-003 — gap: "orchestrate" standalone

A: "The platform should orchestrate multi-step reasoning across agents."
B: [] — "orchestrate" standalone = AI operational gap. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-020-004 — ARCHITECTURE fires: "architect the orchestration layer"

A: "Architect the orchestration layer that coordinates tasks across the specialist agents."
B: ["ARCHITECTURE"] — "architect" = design verb + "layer" = structural noun → ARCHITECTURE. "orchestrate" not present; "architect" fires.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-020-005 — IMPLEMENT fires: "build orchestration service" boundary

A: "Build the agent orchestration service that routes tasks to the appropriate specialist."
B: ["IMPLEMENT"] — "build" = code verb. "orchestration service" = software artifact. "orchestration" is noun; code verb overrides.
CompiledPredicateIds: ["IMPLEMENT"]
