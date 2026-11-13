# Layer 2 — Nine-Node 4.4 Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-024-001 — Nine-node rule summary: N01 SelectedLogicalTransition by predicate combination

**N01 SelectedLogicalTransition mapping (4.4 scenarios):**

| Predicates | TransitionType | SelectedLogicalTransition | Flows |
|---|---|---|---|
| [IMPLEMENT] | feature_addition | IMPLEMENT alone | DataMigrationFlow44 |
| [IMPLEMENT] | refactoring | IMPLEMENT alone | DataMigrationFlow44 or BackendServiceFlow41 |
| [IMPLEMENT, ARCHITECTURE] | feature_addition | IMPLEMENT+ARCHITECTURE | ArchitectureFlow31 + DataMigrationFlow44 |
| [IMPLEMENT, SECURITY] | feature_addition | IMPLEMENT+SECURITY | DataMigrationFlow44 + SecurityReviewFlow |
| [IMPLEMENT, DOCUMENT] | feature_addition | IMPLEMENT+DOCUMENT | DataMigrationFlow44 ‖ DocumentationFlow21 |
| [IMPLEMENT, TEST] | feature_addition | IMPLEMENT+TEST | DataMigrationFlow44 → UnitTestingFlow51 |
| [IMPLEMENT, ARCHITECTURE, SECURITY] | feature_addition | IMPLEMENT+ARCHITECTURE+SECURITY | Three flows |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | feature_addition | Full five-predicate | Five flows |
| [IMPLEMENT] (probationary_rejected) | feature_addition | IMPLEMENT alone | DataMigrationFlow44 → Gate B FAIL |

---

## B18-N9-024-002 — Nine-node rule summary: Gate B type by operation

**Gate B type selection (N08):**

| Operation type | Gate B type | Pass condition |
|---|---|---|
| Standard migration (migrate records) | row_count_assertion | count(target) == count(source) |
| One-to-many migration | row_count_assertion (estimated) | count(target) == estimated_expanded_count |
| Seeding (seed/fixture/populate) | idempotency_check | seeded_count == expected AND idempotent |
| ETL pipeline | row_count_assertion | extracted == loaded |
| Code refactoring | test_regression_check | all_tests_pass |
| Architecture design | schema_review_complete | schema_approved |
| Security review | security_scan_pass | all_security_conditions_true |
| Documentation | doc_complete | all_docs_published |

---

## B18-N9-024-003 — Nine-node rule summary: N09 mandatory fields

**N09 output mandatory fields (all scenarios):**
- connector_name (string)
- flow or flows_completed (string or string[])
- gate_b_status ("PASS" or "FAIL")
- transition_type ("feature_addition" or "refactoring")
- files_generated (integer)
- artifacts (string[])
- n08_verdict ("PASS" or "FAIL")
- completion_message or rejection_reason (depending on verdict)

**Optional fields (when applicable):**
- execution_mode ("parallel" for multi-flow parallel scenarios)
- factory_mode (when probationary_rejected)
- rollback_status ("COMPLETE" when rollback executed)

---

## B18-N9-024-004 — Nine-node rule summary: parallel vs sequential flow execution

**Parallel execution signals:**
- [IMPLEMENT, DOCUMENT] → DocumentationFlow21 is parallel-eligible (always)
- [SECURITY, TEST] after IMPLEMENT → parallel eligible (after IMPLEMENT completes)

**Sequential execution (never parallel):**
- ArchitectureFlow31 and DataMigrationFlow44 — always sequential (ARCH first)
- DataMigrationFlow44 and UnitTestingFlow51 — always sequential (IMPL first)
- DataMigrationFlow44 and SecurityReviewFlow — always sequential (IMPL first)

---

## B18-N9-024-005 — Nine-node rule summary: probationary_rejected behavior in N08/N09

**When N08 Gate B fails:**
1. N08 verdict: FAIL
2. Rollback execution: {connector_name}_down.sql called automatically
3. Factory mode changes to probationary_rejected
4. N09 output includes rejection_reason, error_count, recommended_fix
5. No partial output is accepted (failed migration leaves no target rows)

**This is the complete nine-node rule set for B18 DataMigrationFlow44 scenarios.**
