# Layer 0-B — Router Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-002-001 — Gap: "adapt agent" → GapHandler

A: [] | "Adapt the agent to handle sales queries"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING | DomainId: software-development

---

## C0B-002-002 — IMPLEMENT: "implement agent" → ImplementationFlow41

A: ["IMPLEMENT"] | "Implement an AI agent for query routing"
B: Sub-phase 4.1 → ImplementationFlow41 | TransitionType: feature_addition

---

## C0B-002-003 — ARCHITECTURE: "design agent architecture" → sub-phase 3.x

A: ["ARCHITECTURE"] | "Design the multi-agent orchestration architecture"
B: Sub-phase 3.5 (InfrastructureArchitectureFlow35) — infrastructure/orchestration vocabulary
OR: Sub-phase 3.1 if domain service vocabulary

---

## C0B-002-004 — API_CONTRACT: "integrate agent" → API_CONTRACT flow

A: ["API_CONTRACT"] | "Integrate the AI agent with the helpdesk API"
B: APIContractFlow33 | TransitionType: documentation (no IMPLEMENT)

---

## C0B-002-005 — SECURITY: "secure agent endpoint" → SecurityAuditFlow

A: ["SECURITY"] | "Validate inputs to prevent prompt injection in the agent endpoint"
B: Security sub-phase | DomainId: software-development
