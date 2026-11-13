# Layer 0-D — Flow Factory Pairs (033)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-033-001 — CrossDatabaseMigration factory entry

**Scenario:** "Migrate all product catalog data from legacy MySQL to PostgreSQL."

```
FlowFactoryEntry {
  ScenarioId: "CrossDatabaseMigration_MySQLToPostgres",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(postgres_products) == count(mysql_products)"}
  ]
}
```

---

## B18-0D-033-002 — CrossDatabaseMigration with ARCHITECTURE + SECURITY

**Scenario:** "Design cross-database migration pattern, migrate customer PII from Oracle to PostgreSQL, ensure all PII encrypted."

```
FlowFactoryEntry {
  ScenarioId: "SecureCrossDatabaseMigration",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "cross_db_migration_pattern_approved == true"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(postgres_customers) == count(oracle_customers)"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "no_plaintext_pii AND all_customer_pii_encrypted == true"}
  ]
}
```

---

## B18-0D-033-003 — NoSQLToSQL factory entry

**Scenario:** "Migrate all product reviews from MongoDB to PostgreSQL reviews table."

```
FlowFactoryEntry {
  ScenarioId: "MongoToPostgresReviewsMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(postgres_reviews) == count(mongodb_reviews_collection)"}
  ]
}
```

---

## B18-0D-033-004 — NoSQLToSQL with ARCHITECTURE factory entry

**Scenario:** "Design relational schema structure for DynamoDB migration and implement migration."

```
FlowFactoryEntry {
  ScenarioId: "DynamoToPostgresMigration",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "relational_schema_design_approved == true"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(postgres_items) == count(dynamo_items)"}
  ]
}
```

---

## B18-0D-033-005 — NoSQLToSQL probationary_rejected

**Scenario:** NoSQL to SQL migration FAILED — schema mismatch caused data truncation.

```
FlowFactoryEntry {
  ScenarioId: "MongoToPostgresReviewsMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_rejected",
  RejectionReason: "count(postgres_reviews) == 12,450 but count(mongodb_reviews_collection) == 13,200 — 750 reviews truncated",
  RecommendedFix: "Review schema mapping for JSONB fields; check nested document handling",
  RollbackStatus: "COMPLETE"
}
```
