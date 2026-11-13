# Layer 0-A — Compiler Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-036-001 — Complete TEST+UI reference: Round 2 summary

A: What is the definitive predicate firing rule for all Round 2 (CheckoutFlow through OrderTracking) test requests?
B: TEST fires when: write, create, add, assert, validate, verify co-occur with test nouns (test cases, specs, scenarios, coverage, assertions). UI fires when: button, input, form, modal, page, screen, component, badge, toggle, checkbox, dropdown, breadcrumb, toast, skeleton, panel, sidebar, banner, layout, navigation appear. "run", "check", "execute", "automate", "scan", "monitor", "profile", "describe", "configure" are all gap. Framework names (Cypress, Playwright, Selenium) are gap. QA fires only on literal "QA" abbreviation.

---

## C0A-036-002 — Complete TEST+UI reference: TransitionType determination

A: When does a Round 2 E2E test request produce feature_addition vs documentation?
B: documentation when: TEST+UI fires without IMPLEMENT. This covers all pure test-writing requests for CheckoutFlow, PaymentFlow, LoginFlow, etc. feature_addition when: IMPLEMENT is also present (build, create, implement, add [feature noun]). The presence of QA does not change TransitionType — QA is a validator only. Summary: if you see "write tests" + "assert" + UI vocabulary but no "build/implement/create [feature]" → documentation.

---

## C0A-036-003 — Complete TEST+UI reference: deduplication in dense Round 2 scenarios

A: "Write checkout E2E test coverage, assert the payment button, cart badge, and order summary modal all render correctly."
B: TEST triggers: "write" + "assert" = two triggers → one TEST predicate. UI triggers: "payment button" + "cart badge" + "order summary modal" = three UI vocabulary words ("button", "badge", "modal") → one UI predicate. Result: [TEST, UI]. No IMPLEMENT. No QA. TransitionType: documentation. Routing: [5.3, 1.5].

---

## C0A-036-004 — Complete TEST+UI reference: QA routing edge cases

A: When does QA route to 7.1 vs 7.3 in Round 2 scenarios?
B: QA → 7.3 (default): when "QA gate", "QA acceptance", "QA sign-off", "QA before merge", "QA the suite" appear without release/staging/deployment vocabulary. QA → 7.1 (release): when "QA before release", "QA before staging", "QA the deployment", "QA pre-production" appear. Examples from Round 2: "QA gate before sprint review" → 7.3; "QA sign-off before release" → 7.1.

---

## C0A-036-005 — Complete TEST+UI reference: sub-phase routing summary

A: Summarize all sub-phase routing rules for Round 2 test requests.
B: TEST → 5.3 (E2ETestingFlow53) for checkout, payment, login, registration, password reset, profile, search, filter, cart, wishlist, order tracking — all are E2E/UI/browser context. UI → 1.5 (UIInteractionFlow15) always. QA → 7.3 or 7.1 depending on release vocabulary. IMPLEMENT → 4.1 (BackendServiceFlow41). All flows are independent; QA always executes last.
