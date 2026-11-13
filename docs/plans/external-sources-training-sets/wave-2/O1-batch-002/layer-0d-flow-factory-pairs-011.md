# Layer 0-D — Flow Factory Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-011-001 — probationary_rejected: detailed execution example

A: RoutingDecision: { sub_phase: "5.1", predicates: ["TEST"], context: "write tests for OAuth scope enforcement" }

B: FlowExecution: { step_1: "Factory selects UnitTestingFlow51", step_2: "Gate B check: UnitTestingFlow51 not yet passed Gate B", step_3: "Fallback triggered: GenericFallbackFlow selected", step_4: "GenericFallbackFlow executes standard nine-node pipeline for test scenario", step_5: "Result flagged with fallback_mode: true for review", output: "Generic test plan produced (not specialized test flow)" }

---

## O1b-0D-011-002 — probationary_rejected: what the fallback produces

A: What does GenericFallbackFlow produce for a TEST scenario?

B: GenericFallbackFlow for 5.1 produces: { test_plan: "Standard unit test scaffold for scope enforcement", coverage_targets: ["ScopeValidator", "TenantResolver", "AccessDecider"], note: "Generic coverage only — specialized assertions for edge cases (tenant isolation, scope intersection boundary) not included. Flagged for UnitTestingFlow51 re-evaluation." }

---

## O1b-0D-011-003 — probationary_rejected in O1b training impact

A: Why is 5.1 probationary_rejected in O1b?

B: O1b training purpose: demonstrating that not all sub-phases have specialized flows ready. 5.1 probationary_rejected teaches the model that: (1) routing to 5.1 is valid; (2) the specialized test flow is NOT yet production-ready; (3) fallback is correct behavior (not an error). This prevents the model from inventing specialized test adapters.

---

## O1b-0D-011-004 — Recovery path from probationary_rejected

A: How does 5.1 move from probationary_rejected to probationary_accepted?

B: Recovery path: (1) UnitTestingFlow51 runs in test harness. (2) Gate B condition met: coverage threshold passed, isolation validated, no cross-tenant data leakage. (3) State transitions: probationary_rejected → probationary → probationary_accepted. (4) On next routing to 5.1, specialized adapter is used instead of fallback.

---

## O1b-0D-011-005 — probationary_rejected does not block other flows

A: If 5.1 is probationary_rejected and 3.4 is trusted_learned, does 3.4 still run?

B: YES. probationary_rejected for 5.1 does not affect 3.4. Each flow is independent. SecurityVerification34Flow runs with trusted_learned as normal. The fallback for 5.1 runs concurrently. Results are merged after all flows complete.
