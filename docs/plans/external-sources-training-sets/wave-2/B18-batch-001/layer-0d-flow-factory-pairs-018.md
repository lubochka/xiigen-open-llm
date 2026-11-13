# Layer 0-D — Flow Factory Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-018-001 — TaxRecord: factory for single IMPLEMENT migration

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "TaxRecordMigration",
  factory_mode: "probationary_accepted"
}
```

**Gate B type:** row_count_assertion
**Condition:** count(tax_records) == count(legacy_tax_history)

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "DataMigrationFlow44",
  sub_phase: "4.4",
  gate_b_status: "PENDING",
  connector_name: "TaxRecordMigration"
}
```

---

## B18-0D-018-002 — TaxRecord + SECURITY: compliance adds SecurityReviewFlow Gate B

**Factory input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- compliance vocabulary present

**Security Gate B condition for compliance scenarios:**
- "compliance_requirements_verified == true"
- "audit_trail_established == true" (required for tax compliance)

**Factory output (extended):**
```
FlowFactoryEntry {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "DataMigrationFlow44", condition: "count(tax_records) == count(legacy)"},
    {flow: "SecurityReviewFlow", condition: "compliance_requirements_verified AND audit_trail_established"}
  ]
}
```

---

## B18-0D-018-003 — TaxRecord: ETL factory mode

**ETL factory — same factory path as standard migration:**
- ETL uses DataMigrationFlow44 (same flow)
- Gate B: row_count_assertion (extracted == loaded)
- factory_mode: probationary_accepted (first run)

**ETL-specific Gate B additional conditions:**
- transformation_error_count == 0 (verify no rows lost during transformation)
- currency_field_format_valid (domain-specific assertion)

---

## B18-0D-018-004 — TaxRecord: probationary_rejected for hypothetical failure

**What causes probationary_rejected for TaxRecordMigration:**
- Gate B fails: count(tax_records) != count(legacy_tax_history)
- Possible cause: NULL handling error drops rows with missing jurisdictions
- Factory mode: probationary_rejected
- Fallback: deterministic_generic

**Recovery:** fix NULL handling; re-run; factory enters probationary_accepted for next attempt.

---

## B18-0D-018-005 — TaxRecord + ARCHITECTURE + SECURITY: three-flow factory

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "tax_schema_design_approved"},
    {flow: "DataMigrationFlow44", condition: "count(tax_records) == count(legacy)"},
    {flow: "SecurityReviewFlow", condition: "compliance_requirements_verified"}
  ]
}
```
