# Layer 0-D — Flow Factory Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-014-001 — Analytics/EventLog: factory for ETL pipeline

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "AnalyticsEventLogMigration",
  factory_mode: "probationary_accepted"
}
```

**Gate B type for ETL scenarios:**
- Gate B: row_count_assertion
- Condition: "extracted_row_count == loaded_row_count" (standard ETL Gate B)

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "DataMigrationFlow44",
  sub_phase: "4.4",
  factory_mode: "probationary_accepted",
  gate_b_status: "PENDING",
  connector_name: "AnalyticsEventLogMigration"
}
```

---

## B18-0D-014-002 — EventLog: ETL Gate B type is row_count_assertion (same as migration)

**ETL Gate B types in B18:**
- Simple ETL → row_count_assertion: extracted == loaded
- ETL with transformation → row_count_assertion + transformation_error_count == 0
- ETL with encryption → row_count_assertion + security_scan_pass (requires SecurityReviewFlow)

---

## B18-0D-014-003 — EventLog + SECURITY: factory adds SecurityReviewFlow with security Gate B

**Factory input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- connector_name: "AnalyticsEventLogMigration"

**Factory output (extended):**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "extracted == loaded"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass", condition: "no_unencrypted_PII_fields"}
  ]
}
```

---

## B18-0D-014-004 — EventLog: trusted_learned after repeated ETL success

**trusted_learned promotion:**
- AnalyticsEventLogMigration passed Gate B three consecutive times → promoted to trusted_learned
- Factory mode for subsequent runs: trusted_learned
- Factory selects DataMigrationFlow44 without probation

---

## B18-0D-014-005 — EventLog: factory mode for three-predicate scenario

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "event_log_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "extracted == loaded"},
    {flow: "SecurityReviewFlow", condition: "no_unencrypted_user_ids"}
  ],
  factory_mode: "probationary_accepted"
}
```
