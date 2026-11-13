# Layer 0-B — Router Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-005-001 — UI always 1.5 even in testing context

A: ["TEST", "UI"] | "Write accessibility tests for the form controls"
B: TEST → 5.3; UI → 1.5 (not overridden to 5.3 by test context)
Reason: UI predicate routes to 1.5 even when the request is about UI testing. TEST is 5.3; UI is always 1.5.

---

## C0B-005-002 — TEST 5.3 disambiguates via "browser", "E2E", "smoke", "integration UI"

A: ["TEST"] | "Write browser-based integration tests for the checkout workflow"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: "browser-based integration tests" + "workflow" → system-level E2E → 5.3.

---

## C0B-005-003 — TEST 5.1 for pure function tests (contrast)

A: ["TEST"] | "Write unit tests for the price calculation helper"
B: Sub-phase 5.1 → UnitTestingFlow51
Reason: "unit tests" + "price calculation helper" (pure function, no UI/browser) → 5.1.

---

## C0B-005-004 — QA 7.3 default when ambiguous

A: ["QA"] | "QA this feature"
B: Sub-phase 7.3 → QAGatingFlow73
Reason: No release/deploy/staging vocabulary → default to 7.3 (feature gate). Bare "QA this" → 7.3.

---

## C0B-005-005 — DOCUMENT routes to DocumentationFlow62

A: ["UI", "DOCUMENT"] | "Document the button component API in Storybook"
B: UI → 1.5; DOCUMENT → DocumentationFlow62. Two flows.
Reason: "document" → DOCUMENT (DocumentationFlow62). "button component" → UI (1.5). Independent routing.
