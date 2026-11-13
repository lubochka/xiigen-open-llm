# Layer 3 — Teaching Unit Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation, sub-phases 5.3/7.3/1.5
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B39B1-WDA-teach-001 — UI predicate routes to 1.5 even in a testing context

A: "Write tests for the checkout form, button, and modal." What sub-phase does UI route to?
B: UI routes to 1.5 (UI/UX Prototyping and Component Design) regardless of whether the
surrounding context is testing. UI predicate routing is deterministic: form/button/modal
→ UI → 1.5. The testing aspect is captured by TEST → 5.3 independently. Three routing
decisions: TEST → 5.3, UI → 1.5, IMPLEMENT → 4.1 (from "write").

---

## Pair B39B1-WDA-teach-002 — "automate" is gap vocabulary; TEST requires literal test terms

A: "Automate the checkout UI interactions." Does TEST fire?
B: No. "automate" is not in the TEST predicate lexicon. TEST fires on: test, verify,
assert, validate, spec, test suite. "UI interactions" contains "UI" but "UI" is not
a standalone TEST trigger. Result: zero predicates → knowledge_edge.
To fire TEST: use "write automated tests for the checkout UI interactions" — then
TEST fires via "tests" and UI fires via the checkout UI elements.

---

## Pair B39B1-WDA-teach-003 — "dashboard" confirmed not in UI lexicon even in E2E context

A: "Write E2E tests for the admin dashboard." Does UI fire?
B: TEST fires on "E2E tests"/"tests". UI does NOT fire on "dashboard" (not in UI lexicon —
established in B32 and B33). CompiledPredicateIds: ["TEST", "IMPLEMENT"]. UI does not
fire. CapabilityGapWarnings: gap-ui-dashboard-001. IMPLEMENT fires via "write".
Routing: TEST → 5.3, IMPLEMENT → 4.1. No UI routing.

---

## Pair B39B1-WDA-teach-004 — TEST routes to 5.3 (E2E) not 5.1 (unit) for UI test requests

A: "Write unit tests for the checkout service" vs "Write E2E tests for the checkout form."
   Both fire TEST. Do they both route to 5.1?
B: No. TEST routes based on co-occurring vocabulary:
   "unit tests" → TEST → 5.1 (Unit and Integration Testing)
   "end-to-end tests" / "E2E tests" → TEST → 5.3 (End-to-End and UI Testing)
   The sub-phase disambiguation uses test type vocabulary. "unit tests" → 5.1;
   "E2E", "end-to-end", "browser tests", "UI tests" → 5.3.

---

## Pair B39B1-WDA-teach-005 — framework names (Playwright, Cypress, Selenium) are gap

A: "Set up Selenium to test the login page." What predicates fire?
B: TEST fires on "test". UI fires on "page". IMPLEMENT fires on "set up"? No — "set up"
is gap (not in IMPLEMENT lexicon). "Selenium" is gap (framework name).
CompiledPredicateIds: ["TEST", "UI"]. Routing: TEST → 5.3, UI → 1.5.
"Set up" is a gap term — IMPLEMENT requires: build, create, write, implement, add, develop.
CapabilityGapWarnings: gap-testing-automation-001 (Selenium, set up).
