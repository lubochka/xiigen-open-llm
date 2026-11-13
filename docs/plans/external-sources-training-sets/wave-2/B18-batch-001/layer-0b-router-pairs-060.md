# Layer 0-B — Router Pairs (060)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-060-001 — Complete B18 router reference: master routing decision matrix

**B18 master routing decision matrix (all 60 numbered files):**

```
Input predicates → Router output (flow sequence)

[IMPLEMENT] → DataMigrationFlow44
[ARCHITECTURE] → ArchitectureFlow31
[TEST] → UnitTestingFlow51
[IMPLEMENT, ARCHITECTURE] → ArchitectureFlow31 → DataMigrationFlow44
[IMPLEMENT, SECURITY] → DataMigrationFlow44 → SecurityReviewFlow
[IMPLEMENT, TEST] → DataMigrationFlow44 → UnitTestingFlow51
[IMPLEMENT, DOCUMENT] → DataMigrationFlow44 ‖ DocumentationFlow21
[IMPLEMENT, ARCHITECTURE, SECURITY] → ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow
[IMPLEMENT, ARCHITECTURE, TEST] → ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51
[IMPLEMENT, ARCHITECTURE, DOCUMENT] → ArchitectureFlow31 → DataMigrationFlow44 ‖ DocumentationFlow21
[IMPLEMENT, SECURITY, TEST] → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51
[IMPLEMENT, SECURITY, DOCUMENT] → DataMigrationFlow44 → SecurityReviewFlow; DocumentationFlow21 parallel
[IMPLEMENT, TEST, DOCUMENT] → DataMigrationFlow44 → UnitTestingFlow51; DocumentationFlow21 parallel
[IMPLEMENT, ARCHITECTURE, SECURITY, TEST] → ARCH → IMPL → SEC ‖ TEST
[IMPLEMENT, ARCHITECTURE, SECURITY, DOCUMENT] → ARCH → IMPL → SEC; DOC parallel
[IMPLEMENT, ARCHITECTURE, TEST, DOCUMENT] → ARCH → IMPL → TEST; DOC parallel
[IMPLEMENT, SECURITY, TEST, DOCUMENT] → IMPL → SEC ‖ TEST; DOC parallel
[IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] → ARCH → IMPL → SEC ‖ TEST; DOC parallel
```

---

## B18-0B-060-002 — Complete B18 router reference: 10 absolute routing rules

**The 10 absolute routing rules for B18 (all domains 001-060):**

1. DataMigrationFlow44 = the 4.4 universal database operation flow
2. ArchitectureFlow31 ALWAYS precedes DataMigrationFlow44 (when both present)
3. SecurityReviewFlow ALWAYS follows DataMigrationFlow44 (when IMPLEMENT + SECURITY)
4. UnitTestingFlow51 ALWAYS follows DataMigrationFlow44 (when IMPLEMENT + TEST)
5. SecurityReviewFlow and UnitTestingFlow51 are parallel-eligible (after IMPLEMENT)
6. DocumentationFlow21 is always parallel-eligible (never blocking)
7. QA flow is position-locked last (when present)
8. Sub-phase is determined by vocabulary (not TransitionType)
9. All database DDL operations → DataMigrationFlow44 (not BackendServiceFlow41)
10. SecurityReviewFlow is mandatory when SECURITY predicate fires (cannot be omitted)

---

## B18-0B-060-003 — Complete B18 router reference: routing correctness checklist

**Use this checklist to verify any B18 037-060 routing output:**

- [ ] Is IMPLEMENT present? → DataMigrationFlow44 should be in output
- [ ] Is ARCHITECTURE present? → ArchitectureFlow31 before DataMigrationFlow44
- [ ] Is SECURITY present? → SecurityReviewFlow after DataMigrationFlow44
- [ ] Is TEST present? → UnitTestingFlow51 after DataMigrationFlow44
- [ ] Is DOCUMENT present? → DocumentationFlow21 in parallel
- [ ] Is QA present? → QA flow is last in execution order
- [ ] Is sub-phase 4.4 confirmed? → DDL/data vocabulary present in request
- [ ] Are SecurityReviewFlow and TEST in parallel (not sequential)? → Check ordering
- [ ] Is DocumentationFlow21 blocking any other flow? → It should NOT be blocking

---

## B18-0B-060-004 — Complete B18 router reference: 037-060 domain vocabulary catalogue

**Database operation vocabulary that triggers DataMigrationFlow44 in 037-060:**

anonymize, anonymization migration, archive (records), composite index, covering index, create (DB object), cross-database migration, CQRS projection tables, data anonymization, DDL migration, deleted_at column, encrypt at rest, event_store table, fixtures (DB), full-text search index, GeoIndex, GiST index, masking migration, migrate (records), migrate (from DBMS to DBMS), multi-tenant isolation migration, NoSQL to SQL migration, partial index, PII migration, populate (table), projection table, promote replica, read replica promotion, replication setup migration, rollback script, schema_version table, seed migration, sharding migration, soft-delete migration, tenant_id column, test data fixtures, write migration

---

## B18-0B-060-005 — Complete B18 router reference: never-DataMigrationFlow44 vocabulary

**Vocabulary that does NOT route to DataMigrationFlow44:**

| Vocabulary | Routes to | Reason |
|---|---|---|
| "create UserService" | BackendServiceFlow41 | Code artifact |
| "refactor OrderRepository" | BackendServiceFlow41 | Code artifact |
| "implement API webhook" | ApiContractFlow43 | API connector |
| "write integration tests" | UnitTestingFlow51 | TEST predicate |
| "test the migration" | UnitTestingFlow51 | TEST predicate |
| "document the runbook" (without migration) | DocumentationFlow21 | DOCUMENT only |
| "design the schema" (without implement) | ArchitectureFlow31 | ARCHITECTURE only |
| "QA the migration" | QA flow (last) | QA predicate |
