# Layer 0-D — Flow Factory Selection Pairs (040)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-040-001 — ProgressBar: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-040-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a progress bar component that shows task completion percentage."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-040-002 — ProgressBar: UI+IMPLEMENT+TEST factory

```
source_id: xiigen-b01-0d-040-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Write a test suite for the progress bar component covering all state transitions."
CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-040-003 — ProgressBar: UI only factory (documentation mode)

```
source_id: xiigen-b01-0d-040-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design a progress indicator with visual states for loading, partial, and complete."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
SelectedFlowId: "screen-inventory-flow-1.1-v3"
FallbackUsed: false
SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
TransitionType: documentation (IMPLEMENT absent)
