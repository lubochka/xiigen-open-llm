# Layer 0-D — Flow Factory Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-007-001 — TEST 5.1 vs 5.3 in B34 context

TEST → UnitTestingFlow51 (5.1): "write unit tests" + module/function
TEST → E2ETestingFlow53 (5.3): "write E2E/integration/smoke tests" + user flow

B34 examples:
- "Write unit tests for the skill scoring algorithm" → 5.1
- "Write E2E tests for the onboarding portal user flow" → 5.3
- "Write integration tests for the LMS API" → 5.3

---

## FF-007-002 — Gap + single predicate: suppression mechanics

Request: "Mentor the team and build the onboarding tracker."
Predicates: ["IMPLEMENT"] (mentor→gap; build→IMPLEMENT)
Factory: ImplementationFlow41 only
GapHandler: NOT invoked
Gap-skills-guidance-mentor-001: logged as CapabilityGapWarning (not a full gap log)
TransitionType: feature_addition

Key distinction: When predicates fire, guidance gap is logged as a WARNING, not a full SCHEDULE_CAPABILITY_LEARNING emission.

---

## FF-007-003 — DOCUMENT sub-phase selection

DocumentationFlow always routes to documentation phase. No sub-phase ambiguity in B34 context.
"Write onboarding guide" → DocumentationFlow
"Write API reference" → DocumentationFlow
"Spec out the certification criteria" → DocumentationFlow
All → same DocumentationFlow factory; sub-phase 6.2 default.

---

## FF-007-004 — Zero-predicate: single GapHandler invocation

Request: "Coach, mentor, teach, and train the junior developers."
Predicates: [] (all four verbs are guidance vocabulary)
Factory: GapHandler (invoked once; not four times)
Gap ID: gap-skills-guidance-mentor-001 (mentor priority)
SCHEDULE_CAPABILITY_LEARNING: emitted once.

---

## FF-007-005 — v6 corruption: multi-flow collapse

v6 corrupt: "Build tracker, write tests, document criteria" → Predicates: ["IMPLEMENT"] (collapsed)
Correct: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | Three factories selected
Error type: v6-03 (multi-predicate collapse). All three action verbs fire independently.
