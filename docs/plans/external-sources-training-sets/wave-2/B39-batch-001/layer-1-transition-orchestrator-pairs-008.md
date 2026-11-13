# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-008-001 — IMPLEMENT+UI+QA: no TEST in path

A: ["IMPLEMENT", "UI", "QA"] | "Build breadcrumb nav component and QA sign-off"
Orchestration:
  1. ImplementationFlow41 — build breadcrumb navigation
  2. UIInteractionFlow15 — verify/fix breadcrumb styling
  3. QAGatingFlow73 — QA validates implementation + UI (no TEST → QA does not require tests in this path)
TransitionType: feature_addition | QA last

---

## TO-008-002 — DOCUMENT+UI: documentation orchestration

A: ["UI", "DOCUMENT"] | "Document the design system components"
Orchestration:
  1. UIInteractionFlow15 — capture UI component specifications
  2. DocumentationFlow62 — write component documentation
TransitionType: documentation | No QA → no ordering requirement.

---

## TO-008-003 — TEST+UI: accessibility orchestration

A: ["TEST", "UI"] | "Write ARIA compliance tests and fix missing ARIA labels"
Orchestration:
  1. E2ETestingFlow53 — write ARIA compliance tests (defines what must pass)
  2. UIInteractionFlow15 — fix missing ARIA labels (to make tests pass)
Dependency: UI fix may occur in response to TEST failures. Iterative allowed.

---

## TO-008-004 — GapHandler: no orchestration emitted

A: [] | "Set up Playwright in the CI pipeline"
Orchestration: GapHandler → no flows, no TransitionType. SCHEDULE_CAPABILITY_LEARNING → subproject 16.

---

## TO-008-005 — QA 7.1 orchestration: release gate

A: ["TEST", "QA"] | "Run smoke tests before production release" (QA routes to 7.1)
Orchestration:
  1. E2ETestingFlow53 — execute smoke test suite
  2. ReleaseGateFlow71 — validates smoke test results + release readiness
Teaching: QA routing to 7.1 vs 7.3 affects which gate flow is instantiated, not the ordering rule.
