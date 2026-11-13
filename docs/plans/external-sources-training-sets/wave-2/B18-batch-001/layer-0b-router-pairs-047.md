# Layer 0-B — Router Pairs (047)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-047-001 — Router anti-patterns: v6-03 for 037-060 domains

**v6-03 routing errors specific to 037-060 domains:**

| Incorrect v6 routing | Domain | Correct routing |
|---|---|---|
| "anonymize customer data" → BackendServiceFlow41 | DataAnonymization | DataMigrationFlow44 |
| "create composite index" → BackendServiceFlow41 | CompositeIndex | DataMigrationFlow44 |
| "migrate MongoDB to PostgreSQL" → ApiContractFlow43 | NoSQLToSQL | DataMigrationFlow44 |
| "migrate Oracle to PostgreSQL" → ApiContractFlow43 | CrossDatabaseMigration | DataMigrationFlow44 |
| "seed test fixtures" → UnitTestingFlow51 | SeedDataMigration | DataMigrationFlow44 |
| "write rollback script" → BackendServiceFlow41 | RollbackScript | DataMigrationFlow44 |
| "create audit tables" → BackendServiceFlow41 | AuditTableSetup | DataMigrationFlow44 |
| "create event_store table" → BackendServiceFlow41 | EventSourcingSchema | DataMigrationFlow44 |

**Root cause of v6-03:** v6 treats all "create" verbs as code artifact creation (4.1). The correct rule is: "create" + database object vocabulary → 4.4.

---

## B18-0B-047-002 — Router rule: SecurityReviewFlow placement

**SecurityReviewFlow placement rules (all B18 scenarios 037-060):**

| Scenario | Placement |
|---|---|
| IMPLEMENT + SECURITY | SecurityReviewFlow after DataMigrationFlow44 |
| ARCHITECTURE + IMPLEMENT + SECURITY | SecurityReviewFlow after DataMigrationFlow44 (not after ARCHITECTURE) |
| SECURITY + TEST (after IMPLEMENT) | SecurityReviewFlow ‖ UnitTestingFlow51 (parallel) |
| SECURITY + DOCUMENT | SecurityReviewFlow ‖ DocumentationFlow21 (parallel) |
| SECURITY alone (no IMPLEMENT) | SecurityReviewFlow only (rare in B18; architecture-only security review) |

**Key rule:** SecurityReviewFlow ALWAYS comes after DataMigrationFlow44 when IMPLEMENT is present. It never precedes migration in B18.

---

## B18-0B-047-003 — Router rule: DocumentationFlow21 parallel windows

**DocumentationFlow21 parallel-eligibility windows (all B18 scenarios 037-060):**

| Scenario | DOC parallel window |
|---|---|
| [IMPLEMENT, DOCUMENT] | Parallel with DataMigrationFlow44 from start |
| [IMPLEMENT, SECURITY, DOCUMENT] | Parallel with SecurityReviewFlow (after IMPL) |
| [IMPLEMENT, TEST, DOCUMENT] | Parallel with UnitTestingFlow51 (after IMPL) |
| [ARCH, IMPLEMENT, DOCUMENT] | Parallel with DataMigrationFlow44 (after ARCH) |
| [ARCH, IMPLEMENT, SECURITY, TEST, DOCUMENT] | Parallel from step 2 (IMPL) onward through step 3 |

**DocumentationFlow21 NEVER blocks other flows. It is always parallel-eligible. It never holds up IMPLEMENT, SECURITY, or TEST.**

---

## B18-0B-047-004 — Router rule: QA flow placement in 037-060

**QA flow placement (all B18 scenarios 037-060):**

- QA fires ONLY when "QA" abbreviation or "quality assurance" phrase is present
- QA is ALWAYS position-locked last: runs after ALL other flows complete
- QA does NOT fire on: "verify", "review", "integration tests", "check"
- In five-predicate + QA scenarios: QA runs after SecurityReviewFlow ‖ UnitTestingFlow51 complete

**QA fires in 037-060 examples:**
- "QA the anonymization migration" → QA last
- "quality assurance of the sharding migration" → QA last

**QA does NOT fire:**
- "verify anonymization completed" → no QA
- "review the rollback script" → no QA (review ≠ QA)

---

## B18-0B-047-005 — Complete router reference: parallel eligibility matrix

**Parallel eligibility matrix for B18 037-060 flows:**

| Flow A | Flow B | Parallel eligible? |
|---|---|---|
| DataMigrationFlow44 | DocumentationFlow21 | YES (doc is runbook/spec) |
| SecurityReviewFlow | UnitTestingFlow51 | YES (both verify after IMPL) |
| SecurityReviewFlow | DocumentationFlow21 | YES |
| UnitTestingFlow51 | DocumentationFlow21 | YES |
| ArchitectureFlow31 | DataMigrationFlow44 | NO (schema before migration) |
| ArchitectureFlow31 | SecurityReviewFlow | NO (ARCH must complete first) |
| DataMigrationFlow44 | UnitTestingFlow51 | NO (tests verify migration) |
| DataMigrationFlow44 | SecurityReviewFlow | NO (security reviews migration) |
