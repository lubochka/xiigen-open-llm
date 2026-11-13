# Layer 0-B — Router Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-022-001 — IMPLEMENT+TEST: code predicates in AI context

A: ["IMPLEMENT", "TEST"] | "Implement the agent skill router and write integration tests for routing decisions."
B: IMPLEMENT → ImplementationFlow41; TEST → E2ETestingFlow53 (5.3 integration). TransitionType: feature_addition.

---

## C0B-022-002 — ARCHITECTURE+IMPLEMENT+API_CONTRACT: triple code predicates in AI context

A: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"] | "Design the multi-agent architecture, build the orchestration service, and integrate with the agent registry API."
B: ARCHITECTURE → ArchitectureFlow31; IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33. TransitionType: feature_addition.
Ordering: ARCHITECTURE → API_CONTRACT → IMPLEMENT.

---

## C0B-022-003 — Gap → GapHandler: AI observation with multiple gap terms

A: [] | "The agent's chain-of-thought is helpful, but fine-tuning would further improve accuracy."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Observation + AI strategy suggestion. "chain-of-thought" + "fine-tuning" → gap vocabulary. No code verb fires. Gap ID: gap-ai-agent-llm-001.

---

## C0B-022-004 — SECURITY fires: prompt injection in AI agent context

A: ["SECURITY"] | "Harden the agent input layer against prompt injection attacks."
B: SECURITY → SecurityAuditFlow.
Reason: "prompt injection" = security vocabulary. SECURITY fires regardless of AI agent context.

---

## C0B-022-005 — IMPLEMENT+SECURITY: build + secure agent

A: ["IMPLEMENT", "SECURITY"] | "Build the agent response filter and secure it against adversarial prompt injection."
B: IMPLEMENT → ImplementationFlow41; SECURITY → SecurityAuditFlow. TransitionType: feature_addition.
Ordering: IMPLEMENT before SECURITY (security audits built artifact).
