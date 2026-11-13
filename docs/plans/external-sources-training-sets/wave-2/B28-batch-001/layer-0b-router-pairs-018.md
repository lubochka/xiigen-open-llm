# Layer 0-B — Router Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-018-001 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | DB migration runner + QA gate | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-018-002 — QA+IMPLEMENT+TEST → 7.1, 4.1, 5.1

A: ["QA", "IMPLEMENT", "TEST"] | Migration + data integrity verification + QA | 3 flows | feature_addition

## B28-router-018-003 — QA+DOCUMENT → 7.1, DocumentationFlow62

A: ["QA", "DOCUMENT"] | Migration release criteria docs + QA review | 2 flows | documentation

## B28-router-018-004 — QA+IMPLEMENT+TEST+DOCUMENT → 7.1, 4.1, 5.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | Full migration lifecycle | 4 flows | feature_addition

## B28-router-018-005 — TEST alone: "validate" fires TEST

A: ["TEST"] | "validate row counts after migration" → TEST | [UnitTestingFlow51] | documentation
  Teaching: "validate" fires TEST even in DB migration context. "run migration" is gap.
