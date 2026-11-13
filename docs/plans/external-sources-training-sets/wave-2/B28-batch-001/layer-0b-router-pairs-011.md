# Layer 0-B — Router Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-011-001 — QA+IMPLEMENT+TEST: canonical QA delivery

A: ["QA", "IMPLEMENT", "TEST"] | [QAGatingFlow73, ImplementationFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. Canonical order: IMPLEMENT → TEST → QA gate.

## B28-router-011-002 — QA+API_CONTRACT: API QA

A: ["QA", "API_CONTRACT"] | [QAGatingFlow73, APIContractFlow33]
  Teaching: API contract QA: TEST validates schema; QA gates the release; API_CONTRACT manages the contract.

## B28-router-011-003 — QA alone: no TransitionType contribution

A: ["QA"] | TransitionType: determined by other predicates. QA alone does not drive TransitionType.

## B28-router-011-004 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY+ARCHITECTURE → 6 flows

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT", "SECURITY", "ARCHITECTURE"] | 6 flows. TransitionType: feature_addition.

## B28-router-011-005 — Gap: CI/CD vocabulary without QA trigger

A: "Set up the GitHub Actions pipeline for continuous deployment." | []
  Teaching: CI/CD vocabulary (continuous deployment, pipeline, workflow, actions) is gap across all predicates.

