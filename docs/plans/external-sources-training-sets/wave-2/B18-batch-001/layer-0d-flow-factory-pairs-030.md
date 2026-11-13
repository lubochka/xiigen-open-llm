# Layer 0-D — Flow Factory Pairs (030)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-030-001 — DataAnonymization factory entry

**Scenario:** "Write a migration to anonymize all PII fields in the customers table."

```
FlowFactoryEntry {
  ScenarioId: "DataAnonymizationMigration",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(pii_fields_containing_real_data) == 0"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_pii AND anonymization_verified == true"}
  ]
}
```

---

## B18-0D-030-002 — PIIMasking factory entry

**Scenario:** "Add a migration to mask credit card numbers and SSN fields in the payments table."

```
FlowFactoryEntry {
  ScenarioId: "PiiMaskingMigration",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(masked_cc_numbers == '**** **** **** XXXX') == count(payments)"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_cc AND no_plaintext_ssn == true"}
  ]
}
```

---

## B18-0D-030-003 — DataArchival factory entry

**Scenario:** "Write a migration to archive orders older than 3 years to orders_archive."

```
FlowFactoryEntry {
  ScenarioId: "OrdersArchivalMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(orders_archive WHERE created_at < NOW() - INTERVAL '3 years') == count(archived_orders)"}
  ]
}
```

---

## B18-0D-030-004 — DataArchival with SECURITY factory entry

**Scenario:** "Migrate customer PII data to GDPR-compliant archive with encryption."

```
FlowFactoryEntry {
  ScenarioId: "GdprArchivalMigration",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(gdpr_archive) == count(archived_source)"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "all_pii_encrypted AND gdpr_compliance_verified == true"}
  ]
}
```

---

## B18-0D-030-005 — DataAnonymization with ARCHITECTURE factory entry

**Scenario:** "Design the anonymization pattern and implement the PII anonymization migration."

```
FlowFactoryEntry {
  ScenarioId: "AnonymizationWithArchitecture",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "anonymization_design_spec_approved == true"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(pii_fields_containing_real_data) == 0"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_pii AND anonymization_pattern_applied == true"}
  ]
}
```
