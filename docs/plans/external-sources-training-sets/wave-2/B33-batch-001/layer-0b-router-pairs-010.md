# Layer 0-B — Router Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-010-001 — SECURITY: "prompt injection" triggers SECURITY

A: ["SECURITY"] | "Add prompt injection protection to the AI agent API"
B: Security sub-phase → SecurityAuditFlow
Reason: "prompt injection" is a security vulnerability class → SECURITY fires (like SQL injection, XSS).

---

## C0B-010-002 — API_CONTRACT: "define the agent tool schema"

A: ["API_CONTRACT"] | "Define the tool-calling schema for the agent API contract"
B: APIContractFlow33
Reason: "define the tool-calling schema" + "API contract" → API_CONTRACT. Schema definition is contract work.

---

## C0B-010-003 — Gap: "agent tool-calling behavior" without schema verb

A: [] | "The agent tool-calling behavior needs to be improved"
B: GapHandler
Reason: Observation about AI behavior → gap. No schema/contract/implementation verb.

---

## C0B-010-004 — IMPLEMENT+TEST: "implement" + "write tests" both fire

A: ["IMPLEMENT", "TEST"] | "Implement the tool registry and write integration tests"
B: IMPLEMENT → 4.1; TEST → 5.3 (integration context). Two flows.

---

## C0B-010-005 — Gap: "autonomous" as adjective

A: [] | "Build a more autonomous sales pipeline"
B: Actually — "build" fires IMPLEMENT. "more autonomous sales pipeline" is object (AI-adjacent but not AI vocabulary per se). → ["IMPLEMENT"]
Teaching: "build" fires IMPLEMENT. Even "autonomous" as adjective doesn't suppress the code verb.
CompiledPredicateIds: ["IMPLEMENT"]
