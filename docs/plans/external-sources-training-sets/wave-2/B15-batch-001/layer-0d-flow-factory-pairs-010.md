# Layer 0-D — Flow Factory Pairs (010)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-010-001 — IMPLEMENT+SECURITY+AUTHORIZATION+DOCUMENT four flows
Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "DOCUMENT"]
Factory: ImplementationFlow41 + SecurityAuditFlow + AuthorizationFlow + DocumentationFlow

## FF-010-002 — IMPLEMENT+API_CONTRACT+SECURITY+TEST three-four flows
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
Factory: APIContractFlow33 + ImplementationFlow41 + SecurityAuditFlow + TestingFlow

## FF-010-003 — IMPLEMENT+DOCUMENT+QA: release documentation
Predicates: ["IMPLEMENT", "DOCUMENT", "QA"]
Factory: ImplementationFlow41 ‖ DocumentationFlow + QAGatingFlow73 | QA last | TransitionType: feature_addition

## FF-010-004 — ARCHITECTURE+IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT five flows
Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"]
Factory: six flows selected | TransitionType: feature_addition

## FF-010-005 — Deduplication in factory selection
"Build + implement + create order service" → one IMPLEMENT predicate → one ImplementationFlow41 invocation
Rule: same predicate from multiple triggers → one factory selection, not multiple.
