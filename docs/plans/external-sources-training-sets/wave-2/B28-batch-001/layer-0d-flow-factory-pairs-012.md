# Layer 0-D — Flow Factory Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B28-factory-012-001 — QA alone: minimal flow

A: ["QA"] | [QAGatingFlow73] | One flow

## B28-factory-012-002 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY+ARCHITECTURE+AUTHORIZATION → 7 flows

A: ["QA","IMPLEMENT","TEST","DOCUMENT","SECURITY","ARCHITECTURE","AUTHORIZATION"] | 7 flows | feature_addition

## B28-factory-012-003 — QA: compound noun → QAGatingFlow73

A: Any "QA [noun]" compound | [QAGatingFlow73]

## B28-factory-012-004 — Gap: no QA present

A: "Deploy and monitor." | [] | GapFlow | SCHEDULE_CAPABILITY_LEARNING

## B28-factory-012-005 — QA deduplication summary

A: ["QA", "QA", "IMPLEMENT"] → ["QA", "IMPLEMENT"] (deduplicated)
  B: [QAGatingFlow73, ImplementationFlow41] | Two flows (one per unique predicate)

