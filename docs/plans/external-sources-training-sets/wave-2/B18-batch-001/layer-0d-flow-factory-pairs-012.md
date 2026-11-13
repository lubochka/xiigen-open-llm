# Layer 0-D — Flow Factory Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-012-001 — Complete B18 factory map table

**B18 factory selection: complete reference**

| Predicate | TransitionType | Sub-phase | Selected Flow | Factory Mode (typical) |
|---|---|---|---|---|
| IMPLEMENT | feature_addition | 4.4 | DataMigrationFlow44 | deterministic_generic |
| IMPLEMENT | refactoring | 4.1 | BackendServiceFlow41 | deterministic_generic |
| IMPLEMENT | feature_addition | 4.4 + 4.1 (both) | DataMigrationFlow44 + BackendServiceFlow41 | deterministic_generic |
| ARCHITECTURE | any | 3.1 | ArchitectureFlow31 | deterministic_generic |
| ARCHITECTURE | any | 3.5 | ArchitectureFlow35 | deterministic_generic |
| TEST | any | 5.x | UnitTestingFlow51 | deterministic_generic |
| DOCUMENT | any | 2.1 | DocumentationFlow21 | deterministic_generic |
| SECURITY | any | security | SecurityReviewFlow | deterministic_generic |

---

## B18-0D-012-002 — Factory mode assignment by scenario

| Scenario | Factory Mode | Reason |
|---|---|---|
| CustomerDataMigration (original) | trusted_learned | Previously validated reference scenario |
| OrderHistoryMigration | probationary_accepted → trusted_learned | New scenario; passes Gate B |
| ProductCatalogMigration | deterministic_generic | Standard migration scenario |
| UserAccountMigration | probationary_accepted → trusted_learned | New scenario; passes Gate B |
| LegacyPaymentMigration | probationary_rejected → deterministic_generic | Gate B fails; fallback |
| ProductInventorySeeding | deterministic_generic | Standard seeding scenario |
| CategoryFixtureSeeding | deterministic_generic | Standard fixture scenario |
| ETLPipelineBuild | deterministic_generic | Standard ETL scenario |
| OrderServiceRefactoring | deterministic_generic | Standard refactoring scenario |
| PaymentServiceRefactoring | deterministic_generic | Standard refactoring scenario |
| SchemaConsolidation | deterministic_generic | Complex scenario; both flows |
| MultiStepMigration | deterministic_generic | Standard multi-flow scenario |

---

## B18-0D-012-003 — Factory Gate B types by flow

**Gate B types for each flow in B18:**

| Flow | Gate B Type | Gate B Condition |
|---|---|---|
| DataMigrationFlow44 | row_count_assertion | count(target) == count(source) |
| BackendServiceFlow41 | test_regression_check | count(passing_after) >= count(passing_before) |
| ArchitectureFlow31 | schema_validation | schema specification complete; reviewed |
| UnitTestingFlow51 | test_pass | all generated tests pass |
| DocumentationFlow21 | doc_completeness | all required sections documented |
| SecurityReviewFlow | security_assertion | all security requirements verified |

---

## B18-0D-012-004 — Factory mode lifecycle diagram

**B18 factory mode lifecycle:**

```
New scenario
    │
    ▼
deterministic_generic (first run)
    │
    ├── Gate B PASS → probationary_accepted
    │       │
    │       ▼
    │   trusted_learned (all subsequent runs)
    │
    └── Gate B FAIL → probationary_rejected
            │
            ▼
        deterministic_generic (fallback; retry after fix)
```

---

## B18-0D-012-005 — B18 factory invariants

**B18 factory invariants (always true):**

1. IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44 (always)
2. IMPLEMENT + refactoring + 4.1 vocabulary → BackendServiceFlow41 (always)
3. ARCHITECTURE → ArchitectureFlow31 (always; 3.1 in B18)
4. TEST → UnitTestingFlow51 (always)
5. DOCUMENT → DocumentationFlow21 (always)
6. SECURITY → SecurityReviewFlow (always)
7. No GapHandler instantiated in B18 (always)
8. UnitTestingFlow51 depends_on primary IMPLEMENT flow (always; tests after implementation)
9. ArchitectureFlow31 precedes DataMigrationFlow44 when both present (always; design before migration)
10. DomainId: "software-development" in all factory inputs (always)

---

## B18-0D-012-006 — B18 factory: files generated per flow

**Expected file counts per flow:**

| Flow | Files generated | Typical contents |
|---|---|---|
| DataMigrationFlow44 | 3-8 files | Migration script, rollback script, migration_log table, seed SQL, test data |
| BackendServiceFlow41 | 2-5 files | Refactored service files, extracted class files, updated unit tests |
| ArchitectureFlow31 | 1-3 files | Schema diagram, entity-relationship diagram, migration specification |
| UnitTestingFlow51 | 2-4 files | Test files, test runner configuration, test utilities |
| DocumentationFlow21 | 1-3 files | Schema change document, data mapping spec, policy document |
| SecurityReviewFlow | 1-2 files | Security review report, compliance checklist |

**B18 total files per scenario:**
- Single flow: 3-8 files
- Two flows: 5-12 files
- Five flows (GdprUserDataMigration): ~10 files
