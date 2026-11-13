# Layer 0-D — Flow Factory Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-005-001 — 2.1 deterministic_generic

A: RoutingDecision: { sub_phase: "2.1", predicates: ["DOCUMENT"] }

B: FlowSelection: { adapter: "DocumentationFlow21", state: "deterministic_generic", flow: "DocumentationFlow21" }

---

## O1b-0D-005-002 — 4.1 deterministic_generic

A: RoutingDecision: { sub_phase: "4.1", predicates: ["IMPLEMENT"] }

B: FlowSelection: { adapter: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic", flow: "UniversalNineNodeDevelopmentFlow" }

---

## O1b-0D-005-003 — 2.1 in multi-flow context

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "2.1" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "2.1", adapter: "DocumentationFlow21", state: "deterministic_generic" }]

---

## O1b-0D-005-004 — deterministic_generic: no Gate B required

A: Why is 2.1 deterministic_generic and not probationary?

B: deterministic_generic sub-phases do not require Gate B validation because they use a universal flow with no specialization. The output is predictable (deterministic) and applies generically. Gate B is only required for specialized adapters (trusted_learned, probationary states).

---

## O1b-0D-005-005 — Full factory map for simple cases

A: O1b sub-phase to factory state quick reference

B: | Sub-phase | State | Adapter |
   |-----------|-------|---------|
   | 2.1 | deterministic_generic | DocumentationFlow21 |
   | 3.3 | deterministic_generic | ApiContract33FlowAdapter |
   | 3.4 | trusted_learned | SecurityVerification34FlowAdapter |
   | 4.1 | deterministic_generic | UniversalNineNodeDevelopmentFlow |
   | 5.1 | probationary_rejected | GenericFallbackFlow (fallback) |
