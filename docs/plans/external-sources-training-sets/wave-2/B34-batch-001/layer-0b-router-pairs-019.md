# Layer 0-B — Router Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-019-001 — TEST route: "guide through writing tests" → TEST fires

A: "Guide the team through writing integration tests for the inventory service."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 (integration tests sub-type)
Evidence: Embedded "writing integration tests" → TEST fires; "guide through" = gap warning alongside TEST.

---

## B34-router-019-002 — TEST route: "mentor through TDD cycle" + write tests embedded

A: "Mentor the engineers through the TDD (Test-Driven Development) cycle — write a failing test, implement the feature, make it pass."
Router decision: Predicates = ["TEST", "IMPLEMENT"] → UnitTestingFlow51 + ImplementationFlow41
Evidence: "write a failing test" → TEST; "implement the feature" → IMPLEMENT; "mentor through" = gap warning alongside both predicates.

---

## B34-router-019-003 — Gap route: "guide on testing philosophy" → gap

A: "Guide the team on the philosophy behind behavior-driven testing."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "guide on philosophy" = advisory vocabulary → gap. "behavior-driven testing" = methodology topic without write/create verb. No TEST fires.

---

## B34-router-019-004 — TEST route: "show how to write" → TEST fires

A: "Show the developers how to write property-based tests for the pricing algorithm."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51
Evidence: Embedded "write property-based tests" → TEST fires; "show" = gap warning.

---

## B34-router-019-005 — Gap route: "coach on test coverage" → gap (no write verb)

A: "Coach the team on achieving 80% test coverage across the payment services."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "coach on test coverage" = advisory vocabulary → gap. "achieving 80% coverage" = quality goal, not a test-writing action. No write/create/assert verb → no TEST fires.
