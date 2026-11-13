# Layer 0-D — Flow Factory Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-factory-001 — 3.4 → trusted_learned (SecurityVerification34FlowAdapter)

A: RoutingDecision: { sub_phase: "3.4", predicates: ["SECURITY", "AUTHORIZATION"] }

B: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned", flow: "SecurityVerification34Flow" }
   Reasoning: Sub-phase 3.4 with trusted_learned state maps exclusively to SecurityVerification34FlowAdapter. This is the factory state for the security and authorization model sub-phase. trusted_learned means the flow has passed Gate B and is production-ready.

---

## O1b-factory-002 — 3.3 → deterministic_generic

A: RoutingDecision: { sub_phase: "3.3", predicates: ["API_CONTRACT"] }

B: FlowSelection: { adapter: "ApiContract33FlowAdapter", state: "deterministic_generic", flow: "ApiContractFlow33" }
   Reasoning: Sub-phase 3.3 maps to deterministic_generic state. ApiContract33FlowAdapter is used. deterministic_generic means a standard generic flow applies without learned specialization.

---

## O1b-factory-003 — 5.1 → probationary_rejected (fallback used)

A: RoutingDecision: { sub_phase: "5.1", predicates: ["TEST"] }

B: FlowSelection: { adapter: "UnitTestingFlow51", state: "probationary_rejected", flow: "GenericFallbackFlow", fallback: true }
   Reasoning: Sub-phase 5.1 is in probationary_rejected state. The specialized UnitTestingFlow51 adapter has NOT passed Gate B. System falls back to generic flow. Fallback is logged and flagged for review.

---

## O1b-factory-004 — 3.3 → probationary_accepted (ApiContract33FlowAdapter, Gate B passed)

A: RoutingDecision: { sub_phase: "3.3", predicates: ["API_CONTRACT"], gate_b_result: "passed" }

B: FlowSelection: { adapter: "ApiContract33FlowAdapter", state: "probationary_accepted", flow: "ApiContractFlow33", gate_b: "passed" }
   Reasoning: Sub-phase 3.3 has passed Gate B in this session — state upgrades from probationary to probationary_accepted. The specialized ApiContract33FlowAdapter is now used instead of generic fallback.
