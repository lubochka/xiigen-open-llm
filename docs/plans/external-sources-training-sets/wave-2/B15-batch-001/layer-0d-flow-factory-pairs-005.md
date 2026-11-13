# Layer 0-D — Flow Factory Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-005-001 — ARCHITECTURE+IMPLEMENT+TEST+QA four flows
Predicates: ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"]
Factory: ArchitectureFlow31 + ImplementationFlow41 + TestingFlow + QAGatingFlow73
TransitionType: feature_addition | QA last

## FF-005-002 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT five flows
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"]
Factory: APIContractFlow33 + ImplementationFlow41 + SecurityAuditFlow + TestingFlow + DocumentationFlow
TransitionType: feature_addition

## FF-005-003 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA six flows
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Factory: six flows | QA last | TransitionType: feature_addition

## FF-005-004 — ARCHITECTURE+IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA seven flows
Predicates: all seven (maximum pipeline)
Factory: seven flows | TransitionType: feature_addition | QA absolute last

## FF-005-005 — v6 corruption: multi-predicate collapse
v6 corrupt: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] → only ImplementationFlow41 selected
Correct: four factories selected — one for each predicate
Error type: v6-03 (multi-predicate collapse)
