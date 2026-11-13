# Layer 0-C — Knowledge Edge Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E027-001 — UserRegistrationTest: gap vocabulary

A: Which words in a user registration test request are gap?
B: Gap vocabulary: "check that registration works" ("check" is gap), "execute registration tests" ("execute" is gap), "run the registration spec" ("run" is gap), "automate registration flow" ("automate" is gap), "test the registration" (imperative "test" without test noun = ambiguous → gap in B39), "Selenium" (framework name → gap). "user registration" and "registration flow" alone do not fire any predicate.

---

## E027-002 — UserRegistrationTest: "create test scenarios" fires TEST

A: Does "create test scenarios for the registration form" fire both TEST and UI?
B: Yes. "create test scenarios" → "create" is a test-writing verb + "test scenarios" is a test noun → TEST fires. "registration form" → "form" fires UI. Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## E027-003 — UserRegistrationTest: password strength indicator is UI

A: Does "password strength indicator" fire UI?
B: Yes. "password strength indicator" → "indicator" is not in the core UI lexicon, but "password strength indicators" in context means a visual UI component. Re-evaluation: the base word "indicator" is not explicitly in the lexicon. If the request says "password strength indicator component" → "component" fires UI. If it says "password strength indicator" alone, the UI firing depends on whether any UI vocabulary word is present. Rule: "password strength indicators" → "indicators" does not fire UI by itself. Look for explicit UI word: if "form" or "input" is also present → UI fires on those. Safe rule: check for explicit UI vocabulary words, not inferred ones.

---

## E027-004 — UserRegistrationTest: inline error message fires UI

A: "Assert the registration form shows inline error messages." Does "inline error messages" fire UI?
B: "inline error messages" does not contain an explicit UI vocabulary word (not "modal", "button", "form" etc.). But "registration form" → "form" fires UI. So UI fires from "form", not from "inline error messages". Result: TEST ("assert") + UI ("form"). Always locate the explicit UI lexicon word; do not infer UI from error-message descriptions.

---

## E027-005 — UserRegistrationTest: "validate" fires TEST + "form" fires UI

A: "Validate the registration form submits with required fields." What fires?
B: TEST ("validate" → standalone assertion verb → TEST fires); UI ("registration form" → "form" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation. This is the standard validate + form pattern.
