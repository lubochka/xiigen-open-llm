# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E023-001 — three predicates from dense signal: multiple TEST triggers deduplicate

A: "Write e2e test cases for the accessibility-compliant modal, validate every ARIA role, and QA the test suite."
B: ["TEST", "UI", "QA"] — "write e2e test cases" → TEST (first trigger); "validate" → TEST (second trigger, deduplicated); "modal" → UI; "ARIA role" → UI (second trigger, deduplicated); "QA the test suite" → QA.
Edge: Two TEST triggers (write test cases, validate) and two UI triggers (modal, ARIA role) → each deduplicates to one predicate. Three predicates total.

---

## E023-002 — "update" is gap (not IMPLEMENT); three predicates still fire

A: "Create test coverage for the checkout button states, update the responsive layout, and QA sign-off."
B: ["TEST", "UI", "QA"] — "create test coverage" → TEST; "checkout button" → UI (button); "responsive layout" → UI (responsive, layout); "update" → gap (IMPLEMENT does not fire); "QA sign-off" → QA.
Edge: "update" is consistently gap for IMPLEMENT. Three predicates fire despite the gap verb.

---

## E023-003 — "WCAG (Web Content Accessibility Guidelines) 2.1 AA compliance" fires UI via accessibility sub-lexicon

A: "Write accessibility test scenarios for form input and button components, assert WCAG 2.1 AA compliance."
B: ["TEST", "UI"] — "write accessibility test scenarios" + "assert" → TEST; "form input" + "button components" → UI; "WCAG 2.1 AA compliance" → UI (accessibility vocabulary fires UI).
Edge: WCAG compliance vocabulary fires UI as part of the accessibility sub-lexicon.

---

## E023-004 — "release acceptance" without staging/deploy vocabulary → QA → 7.3 not 7.1

A: "Verify the mobile screen layout for the checkout flow and QA the results for release acceptance."
B: ["TEST", "UI", "QA"] — "verify" → TEST; "screen" + "layout" → UI; "QA the results" → QA.
Edge: "release acceptance" without staging/deployment vocabulary → QA → 7.3 (feature acceptance gate), not 7.1 (release pipeline). The word "release" alone does not shift QA to 7.1. "staging deployment", "pre-release pipeline", "production cutover" are the 7.1 triggers.

---

## E023-005 — "interaction tests" fires TEST; "navigation component" fires UI

A: "Assert that every navigation component passes interaction tests and QA sign-off."
B: ["TEST", "UI", "QA"] — "assert" → TEST; "interaction tests" → TEST (second trigger, deduplicated); "navigation component" → UI (nav, component); "QA sign-off" → QA.
Edge: "interaction tests" is a recognized TEST compound. "navigation component" fires UI via both "navigation" (nav) and "component".
