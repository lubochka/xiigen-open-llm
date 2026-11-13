# Layer 0-D — Flow Factory Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-003-001 — 5.1 probationary_rejected → fallback

A: RoutingDecision: { sub_phase: "5.1", predicates: ["TEST"] }

B: FlowSelection: { adapter: "UnitTestingFlow51", state: "probationary_rejected", flow: "GenericFallbackFlow", fallback: true, note: "UnitTestingFlow51 has not passed Gate B; generic fallback applied" }

---

## O1b-0D-003-002 — probationary_rejected: fallback flow details

A: RoutingDecision: { sub_phase: "5.1", state: "probationary_rejected" }

B: FallbackDetails: { specialized_flow: "UnitTestingFlow51", reason_for_rejection: "Gate B not passed", fallback_flow: "GenericFallbackFlow", fallback_behavior: "Executes standard nine-node pipeline without specialized test-phase logic", logged: true }

---

## O1b-0D-003-003 — probationary_rejected in multi-flow selection

A: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "5.1", state: "probationary_rejected" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "5.1", adapter: "UnitTestingFlow51", state: "probationary_rejected", fallback: true, flow: "GenericFallbackFlow" }]
   Note: A probationary_rejected sub-phase does not block other flows. The 3.4 flow runs with trusted_learned normally.

---

## O1b-0D-003-004 — probationary_rejected: what triggers gate B failure

A: What causes 5.1 to be probationary_rejected?

B: Gate B failure for 5.1 means the UnitTestingFlow51 adapter failed to meet the exact passing condition for test-phase execution. In O1b, Gate B for 5.1 requires: test coverage metric threshold met, test isolation validated, no cross-tenant test data leakage. Until Gate B passes, the adapter stays probationary_rejected and generic fallback applies.

---

## O1b-0D-003-005 — Contrast: trusted_learned vs probationary_rejected

A: Compare 3.4 trusted_learned and 5.1 probationary_rejected factory states

B: | State | Sub-phase | Gate B | Adapter Used | Fallback |
   |-------|-----------|--------|--------------|---------|
   | trusted_learned | 3.4 | PASSED | SecurityVerification34FlowAdapter | No |
   | deterministic_generic | 3.3 | N/A (generic) | ApiContract33FlowAdapter | No |
   | probationary_rejected | 5.1 | FAILED | GenericFallbackFlow | Yes |
   | probationary_accepted | 3.3 | PASSED (session) | ApiContract33FlowAdapter | No |
