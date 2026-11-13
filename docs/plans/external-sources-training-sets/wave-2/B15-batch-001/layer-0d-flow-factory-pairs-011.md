# Layer 0-D — Flow Factory Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-011-001 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA six flows
Predicates: all six | Factory: all six flows | TransitionType: feature_addition | QA absolute last
This is the standard full-cycle factory for B15.

## FF-011-002 — IMPLEMENT alone (extend)
Request: "Extend the order service to support international orders."
Predicates: ["IMPLEMENT"] | Factory: ImplementationFlow41 | TransitionType: feature_addition

## FF-011-003 — IMPLEMENT+TEST: integration test factory
Request: "Implement the payment webhook handler and write integration tests."
Predicates: ["IMPLEMENT", "TEST"] | Factory: ImplementationFlow41 + E2ETestingFlow53 (5.3 — integration)

## FF-011-004 — IMPLEMENT+SECURITY+QA: security gated
Predicates: ["IMPLEMENT", "SECURITY", "QA"]
Factory: ImplementationFlow41 + SecurityAuditFlow + QAGatingFlow73 | QA validates security posture

## FF-011-005 — v6 corruption: incorrect TransitionType for refactor
v6 corrupt: "Refactor the order service" → TransitionType: feature_addition
Correct: TransitionType: refactoring (refactor verb overrides feature_addition)
Error type: TransitionType miscalculation. "refactor" always → refactoring.
