# Layer 0-D — Flow Factory Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-006-001 — QA+TEST+DOCUMENT: release package

A: ["QA", "TEST", "DOCUMENT"] | [QAGatingFlow73, UnitTestingFlow51, DocumentationFlow62] | documentation

## B28-factory-006-002 — QA+SECURITY+TEST → 3 flows

A: ["QA", "SECURITY", "TEST"] | [QAGatingFlow73, SecurityAuditFlow63, UnitTestingFlow51]

## B28-factory-006-003 — QA+ARCHITECTURE+IMPLEMENT → 3 flows, feature_addition

A: ["QA", "ARCHITECTURE", "IMPLEMENT"] | [QAGatingFlow73, InfrastructureArchitectureFlow35, ImplementationFlow41]

## B28-factory-006-004 — QA+API_CONTRACT+TEST → 3 flows

A: ["QA", "API_CONTRACT", "TEST"] | [QAGatingFlow73, APIContractFlow33, UnitTestingFlow51]

## B28-factory-006-005 — QA+DOCUMENT+SECURITY: security release documentation

A: ["QA", "DOCUMENT", "SECURITY"] | [QAGatingFlow73, DocumentationFlow62, SecurityAuditFlow63] | documentation

