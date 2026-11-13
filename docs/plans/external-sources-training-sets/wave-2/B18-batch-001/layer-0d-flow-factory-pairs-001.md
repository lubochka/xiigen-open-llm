# Layer 0-D — Flow Factory Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-001-001 — B18 flow factory overview

**The flow factory instantiates the correct flow object given a predicate + sub-phase input.**

**B18 flow factory inputs:**
- Predicate (IMPLEMENT, ARCHITECTURE, TEST, DOCUMENT, SECURITY)
- Sub-phase (4.4, 4.1, 3.1, 5.x, 2.1, security)
- TransitionType (refactoring, feature_addition)
- ConnectorName (scenario name)
- Factory mode (deterministic_generic, probationary_accepted, probationary_rejected, trusted_learned)

**B18 flow factory outputs:**
- Flow instance (DataMigrationFlow44, BackendServiceFlow41, ArchitectureFlow31, etc.)
- Flow configuration (sub-phase parameters, connector metadata)

---

## B18-0D-001-002 — Factory mode: deterministic_generic

**deterministic_generic:** The factory produces a flow using deterministic rules. No learning, no probation. Used for well-known B18 scenarios.

**B18 deterministic_generic factory rules:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44 (deterministic)
- IMPLEMENT + refactoring + 4.1 → BackendServiceFlow41 (deterministic)
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (deterministic)
- TEST + 5.x → UnitTestingFlow51 (deterministic)
- DOCUMENT + 2.1 → DocumentationFlow21 (deterministic)
- SECURITY + security → SecurityReviewFlow (deterministic)

---

## B18-0D-001-003 — Factory mode: probationary_accepted

**probationary_accepted:** The factory tested a candidate flow, the Gate B condition passed, and the flow was accepted.

**Gate B in B18 context:**
- Migration: Gate B = "all source records migrated to target; count(source) == count(target)"
- Seeding: Gate B = "all seed records inserted; count(seed_table) == expected_count"
- Refactoring: Gate B = "all existing tests pass after refactoring; no regression"
- ETL: Gate B = "all rows transformed; source_row_count == target_row_count"

**probationary_accepted result:** flow promoted to trusted_learned status after Gate B passes.

---

## B18-0D-001-004 — Factory mode: trusted_learned

**trusted_learned:** The flow has been promoted from probationary after Gate B passed. It is now the default for this connector/scenario combination.

**B18 trusted_learned examples:**
- CustomerDataMigration flow was tested (probationary) → Gate B passed → now trusted_learned
- OrderServiceRefactoring flow was tested → tests pass → now trusted_learned

**trusted_learned behavior:** factory selects this flow without probation phase for subsequent invocations of the same connector type.

---

## B18-0D-001-005 — Factory mode: probationary_rejected

**probationary_rejected:** The factory tested a candidate flow, the Gate B condition failed, and the flow was rejected. Fallback to deterministic_generic.

**B18 Gate B failure scenarios:**
- Migration: target row count does not match source count → Gate B fails → probationary_rejected
- Seeding: expected seed count not reached → Gate B fails → probationary_rejected
- ETL: transformation errors in output rows → Gate B fails → probationary_rejected

**probationary_rejected behavior:** factory falls back to deterministic_generic flow. The candidate is logged as rejected.

---

## B18-0D-001-006 — B18 flow factory input/output schema

**Factory input schema:**
```
FlowFactoryInput {
  predicate: string,          // IMPLEMENT, ARCHITECTURE, etc.
  sub_phase: string,          // 4.4, 4.1, 3.1, etc.
  transition_type: string,    // refactoring, feature_addition
  connector_name: string,     // scenario/flow name
  factory_mode: string        // deterministic_generic, probationary_*, trusted_learned
}
```

**Factory output schema:**
```
FlowFactoryOutput {
  flow_name: string,          // DataMigrationFlow44, BackendServiceFlow41, etc.
  sub_phase: string,          // confirmed sub-phase assignment
  factory_mode: string,       // mode used for this instantiation
  gate_b_status: string,      // PASS, FAIL, PENDING (for probationary modes)
  connector_name: string      // passed through from input
}
```
