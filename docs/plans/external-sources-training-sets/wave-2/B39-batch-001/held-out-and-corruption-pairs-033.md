# Held-Out and Corruption Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H033-001 — Held-out: CartFlowTest — badge + button + input

A: "Add E2E tests for the coupon code input on the cart page — assert the discount badge appears, verify the proceed-to-checkout button activates."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("add E2E tests" = test-writing, "assert", "verify"). UI (coupon code input → input, cart page → page, discount badge → badge, proceed-to-checkout button → button — four triggers → one UI). Result: [TEST, UI]. TransitionType: documentation (no IMPLEMENT).

---

## H033-002 — Held-out: CartFlowTest — empty cart screen

A: "Write test coverage asserting the empty cart screen shows after removing the last item."
  Expected: ["TEST", "UI"]
  Held-out: TEST (write test coverage, asserting). UI (empty cart screen → screen). Result: [TEST, UI]. TransitionType: documentation.

---

## H033-003 — Held-out: CartFlowTest — IMPLEMENT+TEST+UI+QA

A: "Implement the save-for-later feature, write E2E tests for the cart page, assert the cart badge, and QA gate before release."
  Expected: ["IMPLEMENT", "TEST", "UI", "QA"]
  Held-out: IMPLEMENT (implement ... feature). TEST (write, assert). UI (cart page → page, cart badge → badge). QA (QA gate). QA → 7.1 (before release). TransitionType: feature_addition. AllSubPhaseIds: [4.1, 5.3, 1.5, 7.1].

---

## C033-001 — Corruption v6-02: "add tests" → IMPLEMENT

Pattern: v6-02. Corrupt: "Add E2E tests for the cart" → predicates = ["IMPLEMENT"].
Correct: "add tests" → TEST (test-writing). IMPLEMENT requires "add [feature noun]", not "add [test noun]". predicates = ["TEST", "UI"].

---

## C033-002 — Corruption v6-01: E2E predicate invented

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "E2E"].
Correct: "E2E" is a test-type modifier, not a predicate. predicates = ["TEST", "UI"].
