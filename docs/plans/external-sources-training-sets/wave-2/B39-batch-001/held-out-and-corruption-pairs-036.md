# Held-Out and Corruption Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H036-001 — Held-out: complete TEST+UI reference — all Round 2 gap verb check

A: "Run the checkout tests, execute the login flow, automate the search tests, scan filter results, monitor wishlist activity, track the order."
  Expected: []
  Held-out: "run", "execute", "automate", "scan", "monitor", "track" — all gap verbs. Zero UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## H036-002 — Held-out: complete TEST+UI reference — dense TEST triggers

A: "Write, assert, validate, and verify E2E test scenarios for the cart page."
  Expected: ["TEST", "UI"]
  Held-out: Four TEST triggers → one TEST predicate (deduplication). "cart page" → page → one UI trigger → one UI predicate. Result: [TEST, UI]. TransitionType: documentation.

---

## H036-003 — Held-out: complete TEST+UI reference — IMPLEMENT overrides dense test request

A: "Build the cart page, write E2E tests for it, assert the cart badge, verify the remove button, validate the coupon input, and QA gate before merge."
  Expected: ["IMPLEMENT", "TEST", "UI", "QA"]
  Held-out: IMPLEMENT (build). TEST (write, assert, verify, validate — deduplicated). UI (cart page → page, cart badge → badge, remove button → button, coupon input → input — four triggers → one UI). QA (QA gate → 7.3). TransitionType: feature_addition. AllSubPhaseIds: [4.1, 5.3, 1.5, 7.3].

---

## C036-001 — Corruption v6-01: full summary for Round 2 invented predicates

Pattern: v6-01. All Round 2 domain names that corrupt models have tried to use as predicates: CHECKOUT_FLOW, PAYMENT, REGISTRATION, LOGIN, PASSWORD_RESET, PROFILE, SEARCH, FILTER, CART, WISHLIST, ORDER_TRACKING.
Correct: None of these are valid predicates. The only seven valid predicates are: TEST, UI, QA, IMPLEMENT, DOCUMENT, ARCHITECTURE, REFACTOR.

---

## C036-002 — Corruption v6-02: full summary for Round 2 gap verbs fired as predicates

Pattern: v6-02. All Round 2 gap verbs that corrupt models fired as predicates: run→TEST, check→TEST/QA, execute→TEST, automate→TEST, scan→TEST, configure→IMPLEMENT, track→TEST, monitor→TEST, update→IMPLEMENT.
Correct: All are gap. None fire any predicate.
