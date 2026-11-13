# Layer 0-B — Router Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-008-001 — QA+IMPLEMENT: build QA harness

A: ["QA", "IMPLEMENT"] | [QAGatingFlow73, ImplementationFlow41] | TransitionType: feature_addition

## B28-router-008-002 — QA+TEST+IMPLEMENT: test + build + gate

A: ["QA", "TEST", "IMPLEMENT"] | [QAGatingFlow73, UnitTestingFlow51, ImplementationFlow41] | TransitionType: feature_addition

## B28-router-008-003 — QA+DOCUMENT: release notes + QA

A: ["QA", "DOCUMENT"] | [QAGatingFlow73, DocumentationFlow62] | TransitionType: documentation

## B28-router-008-004 — QA+TEST+DOCUMENT → documentation

A: ["QA", "TEST", "DOCUMENT"] | [QAGatingFlow73, UnitTestingFlow51, DocumentationFlow62] | TransitionType: documentation (IMPLEMENT absent)

## B28-router-008-005 — QA+IMPLEMENT+TEST+DOCUMENT+QA deduplication

A: "QA sign-off, build the QA harness, write tests, document the results — QA approval required."
  Multiple QA triggers → ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] (QA deduplicated)
  B: [QAGatingFlow73, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | 4 flows.

