# Layer 0-B — Router Pairs (047)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-047-001 — "show [output]" vs "show how to" routing distinction

A: "Show the deployment logs to the team."
Router decision: Predicates = [] → No gap, no code predicate. (Display action, not guidance.)
Note: "show [output/noun]" alone = not guidance vocabulary, not code predicate. Neutral statement.

A: "Show the team how to read the deployment logs."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Note: "show how to" = guidance frame → GapHandler.

---

## B34-router-047-002 — "brief [document]" vs "brief [person] on" routing

A: "Write a technical brief for the client."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Note: "brief" as noun/document type + write action = DOCUMENT. No gap.

A: "Brief the engineering team on the client requirements."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Note: "brief [person] on" = guidance vocabulary → GapHandler.

---

## B34-router-047-003 — "run [pipeline]" vs "run through [X] with [person]" routing

A: "Run the deployment pipeline."
Router decision: Predicates = [] or ["IMPLEMENT"] depending on deploy action context.
Note: "run [pipeline]" = direct action, not guidance vocabulary.

A: "Run through the deployment pipeline with the on-call team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Note: "run through [X] with [person]" = explanation vocabulary → GapHandler.

---

## B34-router-047-004 — "discuss [X]" does not fire topic predicate

A: "Discuss the test coverage gaps with the QA (Quality Assurance) team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: discuss = guidance vocabulary → GapHandler. TEST does not fire even with "test coverage gaps" topic.

---

## B34-router-047-005 — "present [architecture]" does not fire ARCHITECTURE

A: "Present the hexagonal architecture model to the wider engineering team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: present = guidance vocabulary → GapHandler. Architecture topic does not trigger ARCHITECTURE predicate in presentation frame.
