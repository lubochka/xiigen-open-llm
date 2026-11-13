# Layer 0-A — Compiler Pairs (059)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-059-001 — Complete B18 nine-node rule summary: N01 SelectedLogicalTransition

**N01 SelectedLogicalTransition mapping (all 4.4 scenarios, 001-060):**

| Predicates | SelectedLogicalTransition | Primary flow |
|---|---|---|
| [IMPLEMENT] | IMPLEMENT alone | DataMigrationFlow44 |
| [IMPLEMENT] refactoring | IMPLEMENT alone | DataMigrationFlow44 |
| [IMPLEMENT, ARCHITECTURE] | IMPLEMENT+ARCHITECTURE | ArchitectureFlow31 → DataMigrationFlow44 |
| [IMPLEMENT, SECURITY] | IMPLEMENT+SECURITY | DataMigrationFlow44 → SecurityReviewFlow |
| [IMPLEMENT, TEST] | IMPLEMENT+TEST | DataMigrationFlow44 → UnitTestingFlow51 |
| [IMPLEMENT, DOCUMENT] | IMPLEMENT+DOCUMENT | DataMigrationFlow44 ‖ DocumentationFlow21 |
| [IMPLEMENT, ARCHITECTURE, SECURITY] | IMPLEMENT+ARCHITECTURE+SECURITY | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow |
| [IMPLEMENT, ARCHITECTURE, TEST] | IMPLEMENT+ARCHITECTURE+TEST | ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 |
| [IMPLEMENT, SECURITY, TEST] | IMPLEMENT+SECURITY+TEST | DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | Full five-predicate | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51; DocumentationFlow21 parallel |

---

## B18-0A-059-002 — Complete B18 nine-node rule summary: N08 Gate B by domain

**Gate B condition format rule:** Every Gate B condition must be a comparison, count, or assertion. "Migration complete" is NOT a valid Gate B condition.

| Domain | Gate B type | Example valid condition |
|---|---|---|
| Standard migration | row_count_assertion | count(target) == count(source) |
| Seed/fixture/populate | idempotency_check | seeded_count == expected AND idempotent == true |
| DDL schema change (index/FK/column) | schema_change_verification | index_exists AND query_plan_uses_index == true |
| Security review | security_scan_pass | no_plaintext_pii AND all_fields_encrypted == true |
| Architecture | schema_review_complete | schema_spec_approved == true |
| Test suite | test_pass_assertion | all_tests_pass == true |
| Documentation | doc_complete | runbook_published == true |

---

## B18-0A-059-003 — Complete B18 nine-node rule summary: N09 output format

**N09 mandatory output fields for all 4.4 scenarios:**

```
N09Output {
  connector_name: string,            // Scenario name (e.g., "DataAnonymizationMigration")
  flows_completed: string[],         // Array of completed flow names
  gate_b_status: "PASS" | "FAIL",
  transition_type: "feature_addition" | "refactoring",
  files_generated: integer,
  artifacts: string[],               // List of generated file names
  n08_verdict: "PASS" | "FAIL",
  completion_message: string,        // Present on PASS
  rejection_reason?: string,         // Present on FAIL only
  error_count?: integer,             // Present on FAIL only
  recommended_fix?: string,          // Present on FAIL only
  rollback_status?: "COMPLETE",      // Present when rollback executed
  execution_mode?: "parallel"        // Present for parallel flow scenarios
}
```

---

## B18-0A-059-004 — Complete B18 nine-node rule summary: probationary_rejected behavior

**When Gate B FAILS in any B18 4.4 scenario:**

1. N08 verdict: FAIL
2. Rollback: {connector_name}_down.sql executed automatically
3. Factory mode: set to probationary_rejected
4. N09 includes: rejection_reason, error_count, recommended_fix, rollback_status: "COMPLETE"
5. No partial output accepted: no target rows remain after failed migration
6. Re-run requirement: operator must investigate root cause before re-attempting

**Gate B FAIL scenarios specific to 037-060:**
- DataAnonymization FAIL: PII fields found not anonymized after migration
- ShardingMigration FAIL: row count mismatch across shards
- FullTextSearchIndex FAIL: query plan does not use new index
- MultiTenantSchema FAIL: tenant_id NULL found after backfill
- SeedDataMigration FAIL: idempotency broken (duplicate rows on re-run)
- RollbackScript FAIL: schema not fully restored after rollback execution

---

## B18-0A-059-005 — Complete B18 DomainId invariant

**DomainId invariant rule (all B18 scenarios 001-060):**

- DomainId is ALWAYS "software-development"
- DomainId is set at Layer 0-A (compiler output)
- DomainId persists unchanged through Layer 0-B, 0-C, 0-D, L1, L2, L3
- DomainId in N09 output is always "software-development"
- No B18 scenario changes DomainId to another value
- This is an absolute invariant; no domain, predicate combination, or migration type changes it
