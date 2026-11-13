# Layer 0-D — Flow Factory Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-003-001 — QA+SECURITY → QAGatingFlow73, SecurityAuditFlow63

A: ["QA", "SECURITY"] | [QAGatingFlow73, SecurityAuditFlow63]

## B28-factory-003-002 — QA+ARCHITECTURE → QAGatingFlow73, InfrastructureArchitectureFlow35

A: ["QA", "ARCHITECTURE"] (infra context) | [QAGatingFlow73, InfrastructureArchitectureFlow35]

## B28-factory-003-003 — QA+API_CONTRACT → QAGatingFlow73, APIContractFlow33

A: ["QA", "API_CONTRACT"] | [QAGatingFlow73, APIContractFlow33]

## B28-factory-003-004 — QA+TEST+IMPLEMENT → 3 flows, feature_addition

A: ["QA", "TEST", "IMPLEMENT"] | [QAGatingFlow73, UnitTestingFlow51, ImplementationFlow41] | TransitionType: feature_addition

## B28-factory-003-005 — QA+IMPLEMENT+TEST+DOCUMENT → 4 flows, feature_addition

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | [QAGatingFlow73, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | feature_addition

