# Layer 0-D — Flow Factory Pairs (028)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-028-001 — "rubber duck" factory: GapHandler only

A: "Rubber duck the state machine design for the order processing flow."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: rubber duck = advisory/reflective vocabulary → GapHandler → gap-skills-guidance-mentor-001.

---

## FF-028-002 — "sounding board" factory: GapHandler only

A: "Use the architecture guild as a sounding board for the event sourcing design."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: sounding board = advisory vocabulary → GapHandler → gap-skills-guidance-mentor-001.

---

## FF-028-003 — "show how to" + IMPLEMENT factory

A: "Show the team how to set up the local dev environment, then implement the CI (Continuous Integration) lint checks."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show how to set up" = gap warning; "implement CI lint checks" = IMPLEMENT → ImplementationFlow41.

---

## FF-028-004 — "demonstrate" + TEST factory

A: "Demonstrate the test-driven development approach to the team and write unit tests for the parser module."
Factory: UnitTestingFlow51 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate TDD approach" = gap warning; "write unit tests" = TEST → UnitTestingFlow51.

---

## FF-028-005 — "brief" + SECURITY factory

A: "Brief the engineers on OWASP (Open Web Application Security Project) injection risks and harden the input sanitization layer."
Factory: SecurityAuditFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief on OWASP risks" = gap warning; "harden input sanitization" = SECURITY → SecurityAuditFlow.
