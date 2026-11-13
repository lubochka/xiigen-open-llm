# Layer 0-B — Router Pairs (053)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-053-001 — Router reference: 037-060 new domain routing summary

**Routing summary table for all new 037-060 domains:**

| Domain | Typical predicates | Flow sequence |
|---|---|---|
| DataAnonymization | IMPLEMENT + SECURITY | DataMigrationFlow44 → SecurityReviewFlow |
| PIIMasking | IMPLEMENT + SECURITY | DataMigrationFlow44 → SecurityReviewFlow |
| DataArchival | IMPLEMENT | DataMigrationFlow44 |
| DataArchival + PII | IMPLEMENT + SECURITY | DataMigrationFlow44 → SecurityReviewFlow |
| ShardingMigration | IMPLEMENT + ARCHITECTURE | ArchitectureFlow31 → DataMigrationFlow44 |
| ReplicationSetup | IMPLEMENT | DataMigrationFlow44 |
| ReplicationSetup + encrypted | IMPLEMENT + SECURITY | DataMigrationFlow44 → SecurityReviewFlow |
| ReadReplicaPromotion | IMPLEMENT | DataMigrationFlow44 |
| FullTextSearchIndex | IMPLEMENT | DataMigrationFlow44 |
| GeoIndex | IMPLEMENT | DataMigrationFlow44 |
| CompositeIndex | IMPLEMENT | DataMigrationFlow44 |
| CoveringIndex | IMPLEMENT | DataMigrationFlow44 |
| PartialIndex | IMPLEMENT | DataMigrationFlow44 |
| CrossDatabaseMigration | IMPLEMENT | DataMigrationFlow44 |
| NoSQLToSQL | IMPLEMENT | DataMigrationFlow44 |
| EventSourcingSchema | IMPLEMENT | DataMigrationFlow44 |
| CQRSProjection | IMPLEMENT | DataMigrationFlow44 |
| AuditTableSetup | IMPLEMENT | DataMigrationFlow44 |
| SoftDeleteMigration | IMPLEMENT | DataMigrationFlow44 |
| MultiTenantSchema | IMPLEMENT | DataMigrationFlow44 |
| SchemaVersioning | IMPLEMENT | DataMigrationFlow44 |
| SeedDataMigration | IMPLEMENT | DataMigrationFlow44 |
| TestDataMigration | IMPLEMENT | DataMigrationFlow44 |
| RollbackScript | IMPLEMENT | DataMigrationFlow44 |

---

## B18-0B-053-002 — Router reference: 037-060 SECURITY domains

**Domains in 037-060 where SECURITY is expected (common):**

| Domain | SECURITY trigger | Common? |
|---|---|---|
| DataAnonymization | "PII", "GDPR", "sensitive" | Always |
| PIIMasking | "PII", "mask" | Always |
| DataArchival | "PII", "encrypted", "GDPR" | When explicitly mentioned |
| ReplicationSetup | "encrypted", "TLS" | When encrypted replication |
| CrossDatabaseMigration | "PII", "sensitive", "encrypted" | When data has PII |
| NoSQLToSQL | "PII", "encrypted" | When user data involved |
| EventSourcingSchema | "GDPR", "encrypted payload" | When events contain PII |
| AuditTableSetup | "PII", "GDPR", "access control" | When audit involves PII |
| MultiTenantSchema | "row-level security" | When tenant isolation is security requirement |
| TestDataMigration | "PII", "anonymized" | When test data has real PII |

---

## B18-0B-053-003 — Router reference: DOCUMENT parallel placement

**DOCUMENT parallel placement in 037-060:**

DocumentationFlow21 is parallel-eligible in ALL B18 scenarios where DOCUMENT predicate fires. It never blocks IMPLEMENT, SECURITY, or TEST. The following table shows at which point in the execution schedule DocumentationFlow21 can start:

| Execution schedule | DOC start point |
|---|---|
| DataMigrationFlow44 only | Parallel from start |
| ArchitectureFlow31 → DataMigrationFlow44 | Parallel from DataMigrationFlow44 start |
| DataMigrationFlow44 → SecurityReviewFlow | Parallel from DataMigrationFlow44 start |
| DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 | Parallel from DataMigrationFlow44 start |
| ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 | Parallel from DataMigrationFlow44 start |

---

## B18-0B-053-004 — Router reference: single vs multi-flow decision tree

**Decision tree for router output in B18 037-060:**

```
Predicates?
├── [IMPLEMENT] only → DataMigrationFlow44
├── [ARCHITECTURE] only → ArchitectureFlow31
├── [TEST] only → UnitTestingFlow51
├── [IMPLEMENT, ARCHITECTURE] → ArchitectureFlow31 → DataMigrationFlow44
├── [IMPLEMENT, SECURITY] → DataMigrationFlow44 → SecurityReviewFlow
├── [IMPLEMENT, TEST] → DataMigrationFlow44 → UnitTestingFlow51
├── [IMPLEMENT, DOCUMENT] → DataMigrationFlow44 ‖ DocumentationFlow21
├── [IMPLEMENT, ARCHITECTURE, SECURITY] → ARCH → IMPL → SEC
├── [IMPLEMENT, SECURITY, TEST] → IMPL → SEC ‖ TEST
├── [IMPLEMENT, ARCHITECTURE, SECURITY, TEST] → ARCH → IMPL → SEC ‖ TEST
└── [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] → ARCH → IMPL → SEC ‖ TEST; DOC parallel
```

---

## B18-0B-053-005 — Router reference: refactoring TransitionType routing summary

**How refactoring TransitionType affects routing in 037-060:**

| TransitionType | Flow | Notes |
|---|---|---|
| feature_addition + 4.4 | DataMigrationFlow44 | Standard migration |
| refactoring + 4.4 | DataMigrationFlow44 | Same flow; TransitionType propagated as metadata |
| refactoring + 4.1 | BackendServiceFlow41 | Code refactoring (service/class target) |

**Key rule:** TransitionType does NOT change which flow is selected when vocabulary determines the sub-phase. refactoring + 4.4 vocabulary → DataMigrationFlow44 (not BackendServiceFlow41).
