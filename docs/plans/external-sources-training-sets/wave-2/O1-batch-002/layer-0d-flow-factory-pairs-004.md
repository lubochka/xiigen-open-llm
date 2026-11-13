# Layer 0-D — Flow Factory Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-004-001 — 3.3 probationary_accepted (Gate B passed)

A: RoutingDecision: { sub_phase: "3.3", state: "probationary_accepted", gate_b_result: "passed" }

B: FlowSelection: { adapter: "ApiContract33FlowAdapter", state: "probationary_accepted", flow: "ApiContractFlow33", gate_b: "passed", note: "Specialized adapter now used; Gate B passed this session" }

---

## O1b-0D-004-002 — probationary_accepted upgrade from probationary

A: StateTransition: { from: "probationary", to: "probationary_accepted", sub_phase: "3.3", trigger: "gate_b_passed" }

B: Effect: { adapter: "ApiContract33FlowAdapter", active: true, fallback: false, note: "Once Gate B passes, the specialized adapter activates. State advances to probationary_accepted within this session." }

---

## O1b-0D-004-003 — probationary_accepted: Gate B condition for 3.3

A: What is Gate B for sub-phase 3.3 in O1b?

B: Gate B for 3.3 (API Contract): The ApiContract33FlowAdapter must produce a valid OpenAPI 3.0 spec with correct endpoint definitions, request/response schemas, and security scheme declarations. Gate B condition: "OpenAPI spec validates against openapi-spec-validator with zero errors AND security scheme includes OAuth2 client credentials flow." When this passes, state advances to probationary_accepted.

---

## O1b-0D-004-004 — Factory state ordering for 3.3

A: What are the possible factory states for 3.3 in order?

B: 3.3 state progression: probationary → probationary_accepted (Gate B passed in session) → deterministic_generic (stable, recurring). In O1b batch: 3.3 is deterministic_generic (standard mode). probationary_accepted is demonstrated in O1b-factory-004 as a teaching example.

---

## O1b-0D-004-005 — Probationary states summary

A: List all factory states in O1b and their meaning

B: trusted_learned: Gate B passed, specialized adapter fully active (3.4 in O1b). deterministic_generic: standard generic flow, no specialization required (3.3, 4.1, 2.1). probationary_accepted: specialized adapter activated this session, Gate B passed (3.3 example). probationary_rejected: specialized adapter failed Gate B, generic fallback used (5.1 in O1b).
