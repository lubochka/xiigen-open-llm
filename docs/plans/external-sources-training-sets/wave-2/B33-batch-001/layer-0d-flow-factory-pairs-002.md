# Layer 0-D — Flow Factory Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-002-001 — Zero predicates → GapHandler

A: [] | "Adapt the agent to handle enterprise queries"
Factory: GapHandler | SCHEDULE_CAPABILITY_LEARNING | DomainId: software-development

---

## FF-002-002 — IMPLEMENT → ImplementationFlow41

A: ["IMPLEMENT"] | "Build the intent classification service"
Factory: [ImplementationFlow41] | Sub-phase 4.1 | TransitionType: feature_addition

---

## FF-002-003 — ARCHITECTURE → InfrastructureArchitectureFlow35 or AppArchitectureFlow31

A: ["ARCHITECTURE"] | "Design the multi-agent system architecture"
Factory: [InfrastructureArchitectureFlow35] (orchestration/infrastructure vocabulary) or [AppArchitectureFlow31] (domain vocabulary)

---

## FF-002-004 — API_CONTRACT → APIContractFlow33

A: ["API_CONTRACT"] | "Integrate the AI agent with the helpdesk API"
Factory: [APIContractFlow33] | TransitionType: documentation

---

## FF-002-005 — SECURITY → SecurityAuditFlow

A: ["SECURITY"] | "Prevent prompt injection in the agent input layer"
Factory: [SecurityAuditFlow] | SECURITY predicate fires on prompt injection vocabulary
