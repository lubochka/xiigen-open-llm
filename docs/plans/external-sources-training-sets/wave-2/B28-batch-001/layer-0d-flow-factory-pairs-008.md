# Layer 0-D — Flow Factory Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-008-001 — QA+IMPLEMENT: build + gate

A: ["QA", "IMPLEMENT"] | [QAGatingFlow73, ImplementationFlow41] | feature_addition

## B28-factory-008-002 — QA+TEST: write + gate

A: ["QA", "TEST"] | [QAGatingFlow73, UnitTestingFlow51] | documentation (no IMPLEMENT)

## B28-factory-008-003 — QA+DOCUMENT: release notes + gate

A: ["QA", "DOCUMENT"] | [QAGatingFlow73, DocumentationFlow62] | documentation

## B28-factory-008-004 — QA+SECURITY: security patch + gate

A: ["QA", "SECURITY"] | [QAGatingFlow73, SecurityAuditFlow63]

## B28-factory-008-005 — QA+ARCHITECTURE: infra sign-off

A: ["QA", "ARCHITECTURE"] | [QAGatingFlow73, InfrastructureArchitectureFlow35]

