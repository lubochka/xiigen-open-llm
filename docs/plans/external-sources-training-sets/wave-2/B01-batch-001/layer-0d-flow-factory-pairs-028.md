# Layer 0-D — Flow Factory Selection Pairs (028)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-028-001 — Three-sub-phase factory: UI+IMPLEMENT+DOCUMENT

```
source_id: xiigen-b01-0d-028-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build the hero section component and write the specification for its visual behavior."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"] → three factory invocations

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: factory invocations are independent; no cross-sub-phase flow sharing.

---

## B01-0D-028-002 — Three-sub-phase factory: UI+IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0d-028-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create the product card component and define the product data service architecture."
CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"] → three factory invocations

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-028-003 — FallbackUsed false for DETERMINISTIC_GENERIC even when no flow registered

```
source_id: xiigen-b01-0d-028-003
source_license_state: first_party
pair_kind: flow_factory
```

Clarification: FallbackUsed = true ONLY when:
  1. A probationary candidate was proposed AND
  2. That candidate was explicitly rejected (gate_b_passed: false)

FallbackUsed = false when:
  - trust_state is null (no candidate exists) → DETERMINISTIC_GENERIC_SELECTED
  - trust_state is "trusted_learned" → TRUSTED_LEARNED_SELECTED
  - trust_state is "probationary_accepted" (gate_b passed) → ACCEPTED_PROBATIONARY_GATE_B_PASSED

Rule: absence of a registered flow is NOT a fallback. Fallback requires an explicit rejection.
