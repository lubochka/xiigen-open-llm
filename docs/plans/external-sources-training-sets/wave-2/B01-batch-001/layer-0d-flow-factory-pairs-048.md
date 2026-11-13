# Layer 0-D — Flow Factory Selection Pairs (048)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-048-001 — UI+SECURITY: three-factory invocation

```
source_id: xiigen-b01-0d-048-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a login form component with input validation, XSS protection, and CSRF token integration."
CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[SEC.1]: null → "universal-nine-node-development-flow-sec.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-048-002 — UI+SECURITY: document security requirements factory

```
source_id: xiigen-b01-0d-048-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document security requirements for all form components including input sanitization."
CompiledPredicateIds: ["UI", "DOCUMENT", "SECURITY"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[SEC.1]: null → "universal-nine-node-development-flow-sec.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-048-003 — UI+SECURITY: six-factory sweep

```
source_id: xiigen-b01-0d-048-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build secure component library, define auth API schema, document security requirements, write penetration tests."
CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "SECURITY", "TEST"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[SEC.1]: null → "universal-nine-node-development-flow-sec.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: six predicates → six independent factory invocations.
Only sub-phase 1.1 (UI) has a registered trusted_learned flow; all others are deterministic_generic.
