# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E004-001 — "regression" fires TEST (not QA)

A: "Run regression tests on the UserService before the hotfix." → ["TEST"]
  "run regression tests" → TEST | "hotfix" → gap | "before the release" → gap
  Teaching: "regression tests" → TEST, not QA. QA requires "QA" abbreviation.

---

## E004-002 — "QA regression" fires both QA and TEST

A: "QA regression testing on the SearchService before release." → ["QA", "TEST"]
  "QA" → QA | "regression testing" → TEST
  Teaching: "QA regression" compound: "QA" fires QA, "regression testing" fires TEST. Both fire.

---

## E004-003 — "E2E" (End-to-End) fires TEST

A: "Write E2E tests for the checkout flow and QA sign-off." → ["TEST", "QA"]
  "E2E tests" → TEST (sub-phase 5.3) | "QA sign-off" → QA
  Teaching: "E2E" fires TEST. Sub-phase: 5.3 (E2E/system test flow, not unit test 5.1).

---

## E004-004 — "acceptance testing" fires TEST (not QA)

A: "Perform acceptance testing on the NotificationService." → ["TEST"]
  "acceptance testing" → TEST | "perform" → gap alone
  Teaching: "acceptance testing" fires TEST via "testing" gerund. Not QA. QA requires "QA" abbreviation.

---

## E004-005 — "QA acceptance testing" fires both

A: "Perform QA acceptance testing on the v3 release." → ["QA", "TEST"]
  "QA" → QA | "acceptance testing" → TEST
  Teaching: Compound with both "QA" and "testing" → both predicates fire independently.

