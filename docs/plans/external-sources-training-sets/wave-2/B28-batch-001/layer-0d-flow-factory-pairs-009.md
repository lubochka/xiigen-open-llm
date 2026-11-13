# Layer 0-D — Flow Factory Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-009-001 — QA+IMPLEMENT+TEST+DOCUMENT+QA (deduplicated) → 4 flows

A: Multiple QA + IMPLEMENT + TEST + DOCUMENT | ["QA","IMPLEMENT","TEST","DOCUMENT"] → 4 flows

## B28-factory-009-002 — QA: compound noun → QAGatingFlow73

A: "QA checklist" | ["QA"] | [QAGatingFlow73]

## B28-factory-009-003 — QA: imperative verb → QAGatingFlow73

A: "QA this feature" | ["QA"] | [QAGatingFlow73]

## B28-factory-009-004 — QA+AUTHORIZATION: access control QA

A: ["QA", "AUTHORIZATION"] | [QAGatingFlow73, AuthorizationFlow]

## B28-factory-009-005 — QA: QAGatingFlow73 is always last in sequential ordering

A: Any multi-predicate set with QA | QAGatingFlow73 always last in sequential chain
  Teaching: FlowFactory places QA gate as the final flow in any sequential execution.

