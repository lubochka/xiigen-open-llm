# Layer 0-B — Router Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-006-001 — QA alone: single flow

A: ["QA"] | [QAGatingFlow73] | One flow. No IMPLEMENT, no TEST.

## B28-router-006-002 — QA+TEST: two independent flows

A: ["QA", "TEST"] | [QAGatingFlow73, UnitTestingFlow51] | Two flows. QA gating and test execution are independent.

## B28-router-006-003 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY → 5 flows

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT", "SECURITY"] | 5 flows. Feature_addition.

## B28-router-006-004 — gap: no QA trigger

A: "Deploy the service to staging." | [] → GapHandler. "deploy" is gap.

## B28-router-006-005 — QA: sub-phase routing summary

A: ["QA"] with release/deployment vocabulary → 7.1
  ["QA"] with general acceptance/approval vocabulary → 7.3
  Teaching: QA sub-phase (7.1 vs 7.3) depends on vocabulary co-occurrence in the request.

