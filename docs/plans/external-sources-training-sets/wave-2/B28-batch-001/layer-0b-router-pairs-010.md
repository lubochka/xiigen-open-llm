# Layer 0-B — Router Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-010-001 — QA: sequential position — always last

A: ["QA", "IMPLEMENT", "TEST"] | Sequential: IMPLEMENT → TEST → QA gate
  Teaching: QA gating always comes last in the sequential flow, after all implementation and testing.

## B28-router-010-002 — QA+DOCUMENT+SECURITY → 3 flows

A: ["QA", "DOCUMENT", "SECURITY"] | [QAGatingFlow73, DocumentationFlow62, SecurityAuditFlow63]
  TransitionType: documentation (no IMPLEMENT).

## B28-router-010-003 — QA+IMPLEMENT: feature_addition

A: ["QA", "IMPLEMENT"] | TransitionType: feature_addition. QA after implementation.

## B28-router-010-004 — QA+TEST (smoke, E2E context) → 7.3, 5.3

A: ["QA", "TEST"] | smoke/E2E test context → 5.3 (not 5.1)
  Teaching: TEST sub-phase disambiguation: unit/regression → 5.1; smoke/E2E → 5.3.

## B28-router-010-005 — QA: TransitionType summary

A: QA alone → no independent TransitionType (follows other predicates)
  QA + IMPLEMENT → feature_addition | QA + TEST only → documentation
  QA + DOCUMENT → documentation | QA + IMPLEMENT + anything → feature_addition

