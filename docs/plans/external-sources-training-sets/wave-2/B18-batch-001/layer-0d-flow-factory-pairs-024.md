# Layer 0-D — Flow Factory Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-024-001 — Complete factory mode reference

**Factory modes and their selection criteria:**

| Mode | Selection criteria | Effect |
|---|---|---|
| deterministic_generic | Standard pattern; no history required | Rule-based flow selection; Gate B PENDING |
| probationary_accepted | First run OR recent Gate B pass | Normal execution; Gate B evaluated |
| trusted_learned | Repeated Gate B PASS history | Direct flow selection; no probation overhead |
| probationary_rejected | Gate B FAIL on previous run | Fallback to deterministic_generic; candidate logged |

---

## B18-0D-024-002 — Complete Gate B type reference

**All Gate B types used in B18:**

| Gate B type | Applied to flow | Pass condition |
|---|---|---|
| row_count_assertion | DataMigrationFlow44 (migration) | count(target) == count(source) |
| idempotency_check | DataMigrationFlow44 (seeding) | seeded_count stable; no duplicates on re-run |
| test_regression_check | BackendServiceFlow41 | all_pre_refactor_tests_pass |
| schema_review_complete | ArchitectureFlow31 | schema_spec_reviewed_and_approved |
| security_scan_pass | SecurityReviewFlow | all_security_conditions_true |
| test_pass_assertion | UnitTestingFlow51 | all_specified_tests_pass |
| doc_complete | DocumentationFlow21 | all_specified_docs_published |
| schema_change_verification | DataMigrationFlow44 (DDL refactoring) | DDL applied; no data loss |

---

## B18-0D-024-003 — Complete factory flow selection matrix

**Factory flow selection by predicate + TransitionType + sub_phase:**

| Predicate | TransitionType | Sub-phase | Flow selected |
|---|---|---|---|
| IMPLEMENT | feature_addition | 4.4 | DataMigrationFlow44 |
| IMPLEMENT | refactoring | 4.1 | BackendServiceFlow41 |
| IMPLEMENT | refactoring | 4.4 | DataMigrationFlow44 |
| IMPLEMENT | feature_addition | 4.3 | ApiContractFlow43 |
| ARCHITECTURE | any | 3.1 | ArchitectureFlow31 |
| SECURITY | any | security | SecurityReviewFlow |
| TEST | any | 5.x | UnitTestingFlow51 |
| DOCUMENT | any | 2.1 | DocumentationFlow21 |

---

## B18-0D-024-004 — Complete factory Gate B dependency rules

**Gate B evaluation dependencies:**

1. ArchitectureFlow31 Gate B must pass before DataMigrationFlow44 executes
2. DataMigrationFlow44 Gate B must pass before UnitTestingFlow51 evaluates
3. DataMigrationFlow44 Gate B must pass before SecurityReviewFlow evaluates
4. DocumentationFlow21 Gate B is independent (parallel eligible; evaluated alongside step 3-4)
5. All Gate B evaluations feed into a single N08 verdict for the scenario

---

## B18-0D-024-005 — Complete factory: probationary_rejected fallback behavior

**Probationary_rejected fallback sequence:**
1. Candidate flow runs → Gate B FAIL
2. Factory marks connector_name as probationary_rejected
3. Factory logs rejection reason (Gate B condition that failed)
4. On next run: factory selects deterministic_generic (rule-based fallback)
5. deterministic_generic does not retry the same candidate
6. Operator must investigate root cause; clear rejected status to retry probationary path

**B18 example:** LegacyPaymentMigration — Gate B failed (count mismatch) → probationary_rejected → deterministic_generic on subsequent runs.
