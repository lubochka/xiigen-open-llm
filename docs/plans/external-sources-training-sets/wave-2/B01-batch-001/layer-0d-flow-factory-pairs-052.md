# Layer 0-D — Flow Factory Selection Pairs (052)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-052-001 — FileUploadWidget: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-052-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a file upload widget with drag-and-drop zone, progress indicator, and multi-file support."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-052-002 — FileUploadWidget: ARCHITECTURE only factory

```
source_id: xiigen-b01-0d-052-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Define the file upload service API schema."
CompiledPredicateIds: ["ARCHITECTURE"]

Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
FallbackUsed: false
TransitionType: documentation

---

## B01-0D-052-003 — FileUploadWidget: five-predicate factory

```
source_id: xiigen-b01-0d-052-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build file upload widget, implement virus scanning, document upload requirements, define storage API."
CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY", "DOCUMENT", "ARCHITECTURE"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[SEC.1]: null → "universal-nine-node-development-flow-sec.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: five predicates → five factory invocations.
