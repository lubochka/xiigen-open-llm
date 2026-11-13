# Layer 0-B — Router Pairs (007)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-007-001 — SECURITY routes to security sub-phase for AI context

A: ["SECURITY"] | "Prevent prompt injection attacks in the AI agent input layer"
B: Security flow (6.x) | Prompt injection is a security vulnerability → SECURITY predicate fires correctly.

---

## C0B-007-002 — API_CONTRACT routes to APIContractFlow33

A: ["API_CONTRACT"] | "Integrate the AI agent with the external CRM API"
B: APIContractFlow33 | "integrate" always fires API_CONTRACT.

---

## C0B-007-003 — ARCHITECTURE+IMPLEMENT: design + build AI system

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the agent architecture and implement the routing service"
B: ARCHITECTURE → InfrastructureArchitectureFlow35 or AppArchitectureFlow31; IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.

---

## C0B-007-004 — Gap: "coordinate agents" → no predicate

A: [] | "Coordinate the AI agents to collaborate on complex tasks"
B: GapHandler
Reason: "coordinate" is operational/AI vocabulary gap. Does not fire ARCHITECTURE or IMPLEMENT.

---

## C0B-007-005 — Gap: "agent capability" → no predicate

A: [] | "Expand the agent's capabilities to handle finance queries"
B: GapHandler
Reason: "expand capabilities" + "agent" → AI capability gap. No code-writing or architecture verb.
