# Layer 0-B — Router Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-011-001 — Gap route: "onboard" verb alone

A: "Onboard three new engineers to the payments team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001

---

## B34-router-011-002 — IMPLEMENT route: "create" + software tool

A: "Create the automated onboarding checklist tool for new hires."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "create" + "tool" = software artifact → IMPLEMENT.

---

## B34-router-011-003 — TEST route: embedded "write tests"

A: "Teach the team TDD and write a test suite for the user service."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 (unit + service → 5.1)
Evidence: "teach TDD" → gap; "write a test suite" → TEST. GapHandler not invoked.

---

## B34-router-011-004 — Gap route: "facilitate" → gap

A: "Facilitate a team discussion on improving our code review process."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-011-005 — v6 corruption: DOCUMENT invented for "teach to write docs"

v6 corrupt: "Teach juniors to write documentation" → Predicates: ["DOCUMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "teach to write documentation" = teaching vocabulary → gap. The action "write documentation" is what the team will learn to do, not what the requester is doing. Requester action = "teach" = gap.
