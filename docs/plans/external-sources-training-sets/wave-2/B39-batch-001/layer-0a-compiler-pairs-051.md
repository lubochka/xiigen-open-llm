# Layer 0-A — Compiler Pairs (051)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-051-001 — LCP_FID_CLSTest: predicate compilation

A: "Create E2E test assertions for LCP < 2.5s, FID < 100ms, and CLS < 0.1 thresholds on the home page."
B: TEST ("create E2E test assertions" — "create" + "assertions" = test-writing → TEST; "assertions" also fires TEST standalone). UI ("home page" → page). Result: [TEST, UI]. TransitionType: documentation. Metric thresholds do not fire predicates.

---

## C0A-051-002 — LCP_FID_CLSTest: "FID" alone does not fire TEST

A: "Check FID for the home page." Predicates?
B: "check" is gap. "FID" is a metric acronym — gap. "home page" → page → UI fires. Result: [UI]. TEST does not fire.

---

## C0A-051-003 — LCP_FID_CLSTest: "verify CLS passes" fires TEST

A: "Verify CLS passes 0.1 threshold for the product page." Predicates?
B: TEST (verify). UI (product page → page). Result: [TEST, UI].

---

## C0A-051-004 — LCP_FID_CLSTest: all three metrics in one request

A: "Write tests that assert LCP < 2.5s, FID < 100ms, and CLS < 0.1 for the checkout page." How many TEST predicates?
B: One TEST. "write tests" + "assert" = two TEST triggers → deduplicated → one TEST predicate. "checkout page" → one UI trigger. Result: [TEST, UI]. Deduplication applies regardless of how many metrics are in the request.

---

## C0A-051-005 — LCP_FID_CLSTest: QA before production

A: "Write LCP/FID/CLS assertion tests for the checkout page and QA sign-off before production." Predicates?
B: TEST (write, assert). UI (checkout page → page). QA (QA sign-off → 7.1 "before production"). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
