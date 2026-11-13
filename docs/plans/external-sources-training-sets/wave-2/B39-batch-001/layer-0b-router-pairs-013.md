# Layer 0-B — Router Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-013-001 — TEST routes to 5.3 for Cypress e2e context

A: ["TEST", "UI"] | "Write Cypress e2e tests for the shopping cart button flow"
B: TEST → sub-phase 5.3 (E2ETestingFlow53); UI → sub-phase 1.5 (UIInteractionFlow15). Two flows.
Reason: "e2e tests" + "Cypress" context → 5.3 (not 5.1 unit). "button" + "cart" → UI → 1.5 (always).

---

## C0B-013-002 — TEST+UI+QA routes to 5.3 + 1.5 + 7.3

A: ["TEST", "UI", "QA"] | "Write e2e tests for the product detail page and QA sign-off"
B: TEST → 5.3; UI → 1.5; QA → 7.3. Three independent flows: E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73.
Reason: "e2e tests" → 5.3; "page" → UI → 1.5; "QA sign-off" → 7.3 (feature acceptance). No release vocabulary → 7.3 not 7.1.

---

## C0B-013-003 — zero predicates: gap route to GapHandler

A: [] | "Run the Cypress suite against staging checkout page"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING. Zero predicates. No flow instantiated.
Reason: "run" + "Cypress" are gap. Zero predicates always route to GapHandler (knowledge_edge).

---

## C0B-013-004 — TEST alone: assert in Cypress context → 5.3

A: ["TEST", "UI"] | "Assert that the Cypress test for the header navigation passes on all breakpoints"
B: TEST → 5.3; UI → 1.5. "assert" → TEST → 5.3 (E2E assertion context); "header navigation" → UI → 1.5.
Reason: "assert" in E2E/browser test context → 5.3. UI always → 1.5.

---

## C0B-013-005 — TEST: 5.3 vs 5.1 disambiguation

A: ["TEST"] | "Write unit tests for the cart service calculation logic"
B: TEST → sub-phase 5.1 (UnitIntegrationTestingFlow51).
Reason: "unit tests" + "service" + "logic" vocabulary → 5.1 (unit/integration). Contrast with 5.3 which requires E2E/browser/UI vocabulary.
