# Layer 0-A — Compiler Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-022-001 — IMPLEMENT+TEST fires: code predicates embedded in AI context

A: "Implement the agent skill router and write integration tests to validate routing decisions."
B: ["IMPLEMENT", "TEST"] — "implement" → IMPLEMENT; "write integration tests" → TEST. "agent skill" is AI context; code verbs fire independently.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## C0A-022-002 — ARCHITECTURE+IMPLEMENT+API_CONTRACT fires: triple code predicates in AI context

A: "Design the multi-agent architecture, build the orchestration service, and integrate it with the agent registry API."
B: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"] — "design" + "architecture" → ARCHITECTURE; "build" → IMPLEMENT; "integrate" → API_CONTRACT.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]

---

## C0A-022-003 — gap: AI vocabulary sandwiched with observation

A: "The agent's chain-of-thought reasoning is helping, but fine-tuning would further improve accuracy."
B: [] — observation + AI strategy suggestion. No code verb fires. "chain-of-thought" and "fine-tuning" → gap.
CompiledPredicateIds: []

---

## C0A-022-004 — SECURITY fires: prompt injection in AI agent context

A: "Harden the agent input layer against prompt injection attacks."
B: ["SECURITY"] — "prompt injection" = security vocabulary → SECURITY fires regardless of agent AI context. "harden" reinforces SECURITY.
CompiledPredicateIds: ["SECURITY"]

---

## C0A-022-005 — IMPLEMENT+SECURITY fires: build + secure agent

A: "Build the agent response filter and secure it against adversarial prompt injection."
B: ["IMPLEMENT", "SECURITY"] — "build" → IMPLEMENT; "prompt injection" → SECURITY. Both predicates fire in AI agent context.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
