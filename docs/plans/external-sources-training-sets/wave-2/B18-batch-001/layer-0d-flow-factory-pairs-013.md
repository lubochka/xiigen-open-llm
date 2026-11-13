# Layer 0-D — Flow Factory Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-013-001 — AddressBook: factory mode for first-run migration

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "AddressBookMigration",
  factory_mode: "probationary_accepted"
}
```

**Factory reasoning:**
- AddressBookMigration is not previously in trusted_learned
- New scenario → probationary_accepted (first known good classification)
- Gate B type: row_count_assertion (standard for DataMigrationFlow44)
- Gate B condition: count(addresses) + count(phone_numbers) + count(emails) == count(legacy_contacts)

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "DataMigrationFlow44",
  sub_phase: "4.4",
  factory_mode: "probationary_accepted",
  gate_b_status: "PENDING",
  connector_name: "AddressBookMigration"
}
```

---

## B18-0D-013-002 — AddressBook: factory promotes to trusted_learned after Gate B pass

**Promotion rule:**
- AddressBookMigration Gate B passes → factory_mode promoted to trusted_learned
- Subsequent AddressBookMigration invocations: factory selects DataMigrationFlow44 directly
- No repeat probation phase

---

## B18-0D-013-003 — AddressBook + ARCHITECTURE: two-flow factory output

**Factory input:**
```
FlowFactoryInput {
  predicates: ["IMPLEMENT", "ARCHITECTURE"],
  sub_phases: ["4.4", "3.1"],
  transition_type: "feature_addition",
  connector_name: "AddressBookMigration",
  factory_mode: "probationary_accepted"
}
```

**Factory output:**
```
FlowFactoryOutput {
  flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  sub_phases: ["3.1", "4.4"],
  factory_mode: "probationary_accepted",
  gate_b_conditions: [
    {flow: "ArchitectureFlow31", condition: "schema_spec_approved"},
    {flow: "DataMigrationFlow44", condition: "migrated_count == source_count"}
  ]
}
```

---

## B18-0D-013-004 — AddressBook + SECURITY: factory adds SecurityReviewFlow

**Factory input includes SECURITY predicate:**
- predicates: ["IMPLEMENT", "SECURITY"]
- Factory adds SecurityReviewFlow to output

**Additional Gate B condition added by factory:**
- SecurityReviewFlow Gate B: "no_PII_fields_unprotected == true"

---

## B18-0D-013-005 — AddressBook: factory_mode selection logic

**Factory mode selection:**
1. If connector_name exists in trusted_learned registry → trusted_learned
2. If connector_name exists in probationary_accepted registry → probationary_accepted
3. If connector_name is new → probationary_accepted (first run)
4. If connector_name exists in probationary_rejected → deterministic_generic (fallback)

**AddressBookMigration status:** new scenario → probationary_accepted on first run.
