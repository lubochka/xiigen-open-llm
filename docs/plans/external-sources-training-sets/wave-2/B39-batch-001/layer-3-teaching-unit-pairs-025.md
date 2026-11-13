# Layer 3 — Teaching Unit Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU025-001 — CheckoutFlowTest: core lesson

A: What is the single most important rule taught by the CheckoutFlowTest topic?
B: "run the checkout tests" produces [UI] not [TEST, UI]. Gap verbs (run, execute, automate) never fire TEST. Only test-writing verbs (write, create, add [test noun], assert, validate, verify) fire TEST. The presence of "tests" as a noun is insufficient without a test-writing verb.

---

## TU025-002 — CheckoutFlowTest: "validate" always fires TEST

A: Is "validate" always a TEST trigger in B39?
B: Yes. "validate" fires TEST when used as a standalone assertion verb or test-writing verb, regardless of context (valid, invalid, expired, error scenarios). "Automate and validate the checkout flow" → TEST fires (validate) + UI (from UI vocabulary present). "validate" is one of four standalone test assertion verbs: assert, validate, verify, + confirm.

---

## TU025-003 — CheckoutFlowTest: payment button + order summary modal → [TEST, UI]

A: "Write checkout E2E tests — assert the payment button and order summary modal." Teach the predicate pattern.
B: Three TEST triggers (write tests, assert) → one TEST predicate. Two UI triggers (button, modal) → one UI predicate. Deduplication: n triggers → 1 predicate. Result always [TEST, UI]. This is the canonical Round 2 pattern.

---

## TU025-004 — CheckoutFlowTest: success screen fires UI

A: Does "success screen" fire UI?
B: Yes. "screen" is in the UI lexicon → fires UI. "success screen", "payment success screen", "order confirmation screen" all fire UI via the "screen" token.

---

## TU025-005 — CheckoutFlowTest: "add tests" distinction

A: Teach the "add tests" vs "add a button" disambiguation.
B: "add E2E test cases" → TEST (add + test artifact noun). "add a checkout button" → IMPLEMENT (add + feature/product noun). The same verb "add" produces different predicates based on the direct object. Test artifact nouns: tests, test cases, test coverage, assertions, specs, scenarios. Feature/product nouns: button, page, component, service, endpoint, feature.
