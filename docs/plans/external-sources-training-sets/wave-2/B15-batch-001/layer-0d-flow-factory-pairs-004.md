# Layer 0-D — Flow Factory Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-004-001 — IMPLEMENT+TEST+DOCUMENT three flows
Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] → ImplementationFlow41 + TestingFlow + DocumentationFlow
TransitionType: feature_addition

## FF-004-002 — IMPLEMENT+API_CONTRACT+TEST three flows
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] → APIContractFlow33 + ImplementationFlow41 + TestingFlow

## FF-004-003 — IMPLEMENT+SECURITY+TEST three flows
Predicates: ["IMPLEMENT", "SECURITY", "TEST"] → ImplementationFlow41 + SecurityAuditFlow + TestingFlow

## FF-004-004 — IMPLEMENT+TEST+DOCUMENT+QA four flows
Predicates: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] → ImplementationFlow41 + TestingFlow + DocumentationFlow + QAGatingFlow73
QA last.

## FF-004-005 — v6 corruption: ORDER_PROCESSING flow invented
v6 corrupt: "Implement order processing" → Predicates: ["ORDER_PROCESSING"] → OrderProcessingFlow
Correct: Predicates: ["IMPLEMENT"] → ImplementationFlow41
Error type: v6-01 — ORDER_PROCESSING is not a predicate; no such flow exists.
