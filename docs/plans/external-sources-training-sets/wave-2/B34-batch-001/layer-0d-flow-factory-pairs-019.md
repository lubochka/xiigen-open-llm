# Layer 0-D — Flow Factory Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-019-001 — Guidance + TEST embedded: factory selection examples

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Guide through writing unit tests" | [TEST] | UnitTestingFlow51 | documentation |
| "Coach through writing E2E tests" | [TEST] | E2ETestingFlow53 | documentation |
| "Show how to write integration tests" | [TEST] | UnitTestingFlow51 | documentation |
| "Mentor through TDD: write failing test, implement, pass" | [TEST, IMPLEMENT] | UnitTestingFlow51 + ImplementationFlow41 | feature_addition |
| "Coach on test coverage" (no write verb) | [] | GapHandler | none |

---

## FF-019-002 — Unit vs E2E factory selection in guidance context

"Guide through writing unit tests" → UnitTestingFlow51 (unit test keyword)
"Coach through writing end-to-end tests" → E2ETestingFlow53 (E2E keyword)
"Walk through writing smoke tests" → UnitTestingFlow51 (smoke tests → unit test sub-type)
"Show how to write integration tests" → UnitTestingFlow51 (integration tests → unit test sub-type in factory)
"Guide through writing acceptance tests" → E2ETestingFlow53 (acceptance = E2E scope)

---

## FF-019-003 — TEST gap warning ID assignment

When TEST fires alongside guidance verb:
CapabilityGapWarning ID: gap-skills-guidance-mentor-001 (default for mentor/guide/coach)
CapabilityGapWarning ID: gap-skills-guidance-train-001 (for teach/demonstrate/show)
Both = supplemental warnings; GapHandler NOT invoked; SCHEDULE_CAPABILITY_LEARNING NOT emitted.
