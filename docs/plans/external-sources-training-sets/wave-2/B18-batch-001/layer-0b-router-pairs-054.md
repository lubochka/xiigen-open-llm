# Layer 0-B — Router Pairs (054)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-054-001 — Complete router reference: B18 absolute routing rules 001-060

**B18 absolute routing rules that never change:**

1. DataMigrationFlow44 activates on IMPLEMENT + 4.4 vocabulary (DDL/data vocabulary). Always.
2. ArchitectureFlow31 ALWAYS precedes DataMigrationFlow44 when both are present. No exception.
3. SecurityReviewFlow ALWAYS follows DataMigrationFlow44 when SECURITY and IMPLEMENT both present.
4. UnitTestingFlow51 ALWAYS follows DataMigrationFlow44 when TEST and IMPLEMENT both present.
5. SecurityReviewFlow and UnitTestingFlow51 are parallel-eligible (neither blocks the other).
6. DocumentationFlow21 is always parallel-eligible (never blocking).
7. QA flow is always position-locked last when present.
8. "migrate from X to Y DBMS" → DataMigrationFlow44 (never ApiContractFlow43).
9. "seed/fixture/populate" → DataMigrationFlow44 (never UnitTestingFlow51).
10. ARCHITECTURE-only (no IMPLEMENT) → ArchitectureFlow31 only; no DataMigrationFlow44.

---

## B18-0B-054-002 — Router reference: v6 routing errors specific to security + migration

**v6 routing errors where SECURITY flow is misplaced:**

| v6 error | Correct placement |
|---|---|
| SecurityReviewFlow placed BEFORE DataMigrationFlow44 | SecurityReviewFlow always AFTER DataMigrationFlow44 |
| SecurityReviewFlow placed BEFORE ArchitectureFlow31 | ArchitectureFlow31 is always first |
| SecurityReviewFlow run in parallel with DataMigrationFlow44 | SecurityReviewFlow must wait for migration to complete |
| SecurityReviewFlow absent despite "PII" token | SecurityReviewFlow must be present |
| SecurityReviewFlow absent despite "encrypted" token | SecurityReviewFlow must be present |

---

## B18-0B-054-003 — Router reference: five-predicate execution schedule template

**Universal five-predicate execution schedule for B18 037-060:**

```
Step 1: ArchitectureFlow31
  - Input: schema design request
  - Output: schema spec / design document
  - Gate B: schema_review_complete

Step 2: DataMigrationFlow44
  - Input: schema spec from Step 1
  - Output: migration files, migrated data
  - Gate B: domain-specific (row_count_assertion, schema_change_verification, or idempotency_check)

Step 3 (parallel): SecurityReviewFlow ‖ UnitTestingFlow51
  - SecurityReviewFlow: verify PII/encryption conditions
  - UnitTestingFlow51: run tests against migrated data

Step 4 (parallel with Steps 2-3): DocumentationFlow21
  - Output: runbook, spec, or policy document
  - Gate B: doc_complete
```

---

## B18-0B-054-004 — Router reference: ARCHITECTURE only routing variants

**When ARCHITECTURE fires without IMPLEMENT in 037-060:**

| Scenario | Predicates | Flow |
|---|---|---|
| "design the anonymization pattern" | [ARCHITECTURE] | ArchitectureFlow31 |
| "design the shard key structure" | [ARCHITECTURE] | ArchitectureFlow31 |
| "design the composite index pattern for reporting" | [ARCHITECTURE] | ArchitectureFlow31 |
| "review the rollback architecture" | [ARCHITECTURE] | ArchitectureFlow31 |
| "evaluate the multi-tenant isolation pattern" | [ARCHITECTURE] | ArchitectureFlow31 |

**Output:** ArchitectureFlow31 only — no migration generated.

---

## B18-0B-054-005 — Router reference: complete anti-pattern list for 037-060

**Complete routing anti-patterns to avoid in 037-060:**

| Anti-pattern | Error type | Correct output |
|---|---|---|
| "create GiST index" → BackendServiceFlow41 | v6-03 | DataMigrationFlow44 |
| "migrate from MongoDB to PostgreSQL" → ApiContractFlow43 | v6-03 | DataMigrationFlow44 |
| "write rollback script" → BackendServiceFlow41 | v6-03 | DataMigrationFlow44 |
| "seed test fixtures" → UnitTestingFlow51 | v6-03 | DataMigrationFlow44 |
| "anonymize PII data" → IMPLEMENT only (no SECURITY) | v6-04 | DataMigrationFlow44 + SecurityReviewFlow |
| "GDPR-compliant migration" → IMPLEMENT only (no SECURITY) | v6-04 | DataMigrationFlow44 + SecurityReviewFlow |
| ARCHITECTURE after IMPLEMENT | ordering error | ArchitectureFlow31 first |
| SecurityReviewFlow before DataMigrationFlow44 | ordering error | DataMigrationFlow44 first |
