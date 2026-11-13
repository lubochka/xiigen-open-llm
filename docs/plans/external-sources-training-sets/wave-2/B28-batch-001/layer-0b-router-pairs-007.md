# Layer 0-B — Router Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-007-001 — QA+ARCHITECTURE (infra) → 7.3, 3.5

A: ["QA", "ARCHITECTURE"] (infra context) | [QAGatingFlow73, InfrastructureArchitectureFlow35]

## B28-router-007-002 — QA+AUTHORIZATION → 7.3, AUTHZ sub-phase

A: ["QA", "AUTHORIZATION"] | QA gating on authorization model. Two flows.

## B28-router-007-003 — QA+API_CONTRACT+TEST → 7.3, 3.3, 5.1

A: ["QA", "API_CONTRACT", "TEST"] | Three flows. API contract QA with validation tests.

## B28-router-007-004 — Six predicates → 6 flows

A: ["QA", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT", "ARCHITECTURE"] | 6 flows. TransitionType: feature_addition.

## B28-router-007-005 — QA: "quality assurance" spelled out → gap (v6-02 trap)

A: "Perform quality assurance on the feature." | Correct: []
  Corrupt: ["QA"] ← v6-02 (model fires QA on "quality assurance" spelled out)
  Teaching: Only "QA" abbreviation fires QA. "quality assurance" spelled out → gap.

