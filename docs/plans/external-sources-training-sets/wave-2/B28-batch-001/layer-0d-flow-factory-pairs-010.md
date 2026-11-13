# Layer 0-D — Flow Factory Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-010-001 — QA+IMPLEMENT+TEST+DOCUMENT: full release prep

A: ["QA","IMPLEMENT","TEST","DOCUMENT"] | [QAGatingFlow73, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | feature_addition

## B28-factory-010-002 — QA+TEST+SECURITY: security regression QA

A: ["QA","TEST","SECURITY"] | [QAGatingFlow73, UnitTestingFlow51, SecurityAuditFlow63]

## B28-factory-010-003 — QA+IMPLEMENT+DOCUMENT: build + document + gate

A: ["QA","IMPLEMENT","DOCUMENT"] | [QAGatingFlow73, ImplementationFlow41, DocumentationFlow62] | feature_addition

## B28-factory-010-004 — QA+TEST+API_CONTRACT: API test + QA

A: ["QA","TEST","API_CONTRACT"] | [QAGatingFlow73, UnitTestingFlow51, APIContractFlow33]

## B28-factory-010-005 — QA+ARCHITECTURE+IMPLEMENT+TEST → 4 flows

A: ["QA","ARCHITECTURE","IMPLEMENT","TEST"] | [QAGatingFlow73, InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51] | feature_addition

