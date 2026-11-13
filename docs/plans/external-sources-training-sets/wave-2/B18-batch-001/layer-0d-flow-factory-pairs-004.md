# Layer 0-D — Flow Factory Pairs (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-004-001 — trusted_learned: 4.4 promoted flow

**Scenario:** CustomerDataMigration (original reference scenario — already written, promoted to trusted_learned)

**Context:** CustomerDataMigration has been run successfully in previous training. The factory now uses trusted_learned mode for CustomerDataMigration-type scenarios.

**Factory input (trusted_learned — subsequent invocation):**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "CustomerDataMigration",
  "factory_mode": "trusted_learned"
}
```

**Factory behavior:**
- Skip probationary phase (already trusted)
- Select DataMigrationFlow44 directly
- Gate B still evaluated for quality assurance

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "trusted_learned",
  "gate_b_status": "PASS",
  "connector_name": "CustomerDataMigration"
}
```

---

## B18-0D-004-002 — trusted_learned: when it applies in B18

**trusted_learned mode applies when:**
1. This connector name has been previously run in probationary_accepted mode
2. Gate B passed on the probationary run
3. The factory has a learned entry for this connector → flow mapping

**trusted_learned vs deterministic_generic:**
- deterministic_generic: rule-based selection (applies to all new scenarios)
- trusted_learned: history-based selection (applies to previously run scenarios with Gate B pass)
- Both select the SAME flow (DataMigrationFlow44 for 4.4); the difference is the selection mechanism

---

## B18-0D-004-003 — trusted_learned: OrderHistoryMigration after promotion

**Scenario:** OrderHistoryMigration (second run, after probationary_accepted from file 003)

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "OrderHistoryMigration",
  "factory_mode": "trusted_learned"
}
```

**Factory behavior:**
- OrderHistoryMigration was promoted to trusted_learned in file 003
- Factory selects DataMigrationFlow44 from learned mapping
- No probationary phase needed

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "trusted_learned",
  "gate_b_status": "PASS",
  "connector_name": "OrderHistoryMigration"
}
```

---

## B18-0D-004-004 — trusted_learned for refactoring flow

**Scenario:** PaymentServiceRefactoring (after trusted_learned promotion)

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "connector_name": "PaymentServiceRefactoring",
  "factory_mode": "trusted_learned"
}
```

**Factory output:**
```
{
  "flow_name": "BackendServiceFlow41",
  "sub_phase": "4.1",
  "factory_mode": "trusted_learned",
  "gate_b_status": "PASS",
  "connector_name": "PaymentServiceRefactoring"
}
```

---

## B18-0D-004-005 — trusted_learned vs deterministic_generic: functional equivalence

**Key insight:** trusted_learned and deterministic_generic select the SAME flows.

For any B18 scenario:
- deterministic_generic: IMPLEMENT + 4.4 → DataMigrationFlow44
- trusted_learned: IMPLEMENT + 4.4 → DataMigrationFlow44 (from learned mapping)
- Same output, different selection mechanism

**The difference is semantic:** trusted_learned indicates this specific connector has been validated. deterministic_generic applies to all new scenarios by rule.

**From the orchestrator's perspective:** both produce the same flow configuration. The factory mode is metadata about the selection history, not the flow behavior.

---

## B18-0D-004-006 — trusted_learned lifecycle in B18

**B18 trusted_learned lifecycle:**

1. **First run:** deterministic_generic (no history)
2. **Probationary run:** probationary_accepted (if Gate B passes) or probationary_rejected (if fails)
3. **After acceptance:** trusted_learned (promoted; used for all subsequent runs)
4. **Regression check:** if trusted_learned Gate B fails → alert; may revert to probationary

**For B18 training set:** The original CustomerDataMigration scenario represents trusted_learned status. All new scenarios (OrderHistoryMigration, ProductCatalogMigration, etc.) start as deterministic_generic, then move through probationary_accepted → trusted_learned after Gate B confirmation.
