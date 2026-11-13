# Layer 0-D — Flow Factory Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-022-001 — IMPLEMENT+TEST: code predicates in AI context

A: ["IMPLEMENT", "TEST"] | "Implement the agent skill router and write integration tests."
Factory: [ImplementationFlow41, E2ETestingFlow53] | TransitionType: feature_addition
Ordering: IMPLEMENT before TEST.

---

## FF-022-002 — ARCHITECTURE+IMPLEMENT+API_CONTRACT: triple code predicates in AI context

A: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"] | "Design the multi-agent architecture, build the orchestration service, and integrate with the agent registry API."
Factory: [ArchitectureFlow31, APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41.

---

## FF-022-003 — GapHandler: AI observation with multiple gap terms

A: [] | "The agent's chain-of-thought is helpful, but fine-tuning would further improve accuracy."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: Observation + AI strategy. "chain-of-thought" + "fine-tuning" → gap. No code verb.

---

## FF-022-004 — SECURITY: prompt injection in agent context

A: ["SECURITY"] | "Harden the agent input layer against prompt injection attacks."
Factory: [SecurityAuditFlow]
Reason: "prompt injection" → SecurityAuditFlow. "harden" reinforces SECURITY. AI context does not suppress.

---

## FF-022-005 — IMPLEMENT+SECURITY: build + secure agent

A: ["IMPLEMENT", "SECURITY"] | "Build the agent response filter and secure against adversarial prompt injection."
Factory: [ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition
Ordering: ImplementationFlow41 before SecurityAuditFlow (security audits built artifact).
