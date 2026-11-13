# Layer 0-D — Flow Factory Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-004-001 — QA+SECURITY+IMPLEMENT → 3 flows

A: ["QA", "SECURITY", "IMPLEMENT"] | [QAGatingFlow73, SecurityAuditFlow63, ImplementationFlow41] | feature_addition

## B28-factory-004-002 — QA+IMPLEMENT+TEST+SECURITY+DOCUMENT → 5 flows

A: ["QA", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"] | 5 flows | feature_addition

## B28-factory-004-003 — v6-03: QA suppressed by IMPLEMENT

A: ["QA", "IMPLEMENT"] | Corrupt: [ImplementationFlow41] only → v6-03 | Correct: [QAGatingFlow73, ImplementationFlow41]
  Teaching: QA is never suppressed.

## B28-factory-004-004 — v6-01: DEVOPS invented

A: ["QA"] | Corrupt: ["QA", "DEVOPS"] → v6-01 | Correct: ["QA"]

## B28-factory-004-005 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY+ARCHITECTURE → 6 flows

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT", "SECURITY", "ARCHITECTURE"] | 6 flows | feature_addition

