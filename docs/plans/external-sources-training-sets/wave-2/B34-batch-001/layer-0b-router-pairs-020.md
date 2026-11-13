# Layer 0-B — Router Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-020-001 — IMPLEMENT route: "coach through building" → IMPLEMENT fires

A: "Coach the engineers through building the event streaming consumer service."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: Embedded "building the event streaming consumer service" → IMPLEMENT fires; "coach through" = gap warning.

---

## B34-router-020-002 — IMPLEMENT route: "mentor through implementing"

A: "Mentor the junior dev through implementing the repository pattern for the user domain."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: Embedded "implementing the repository pattern" → IMPLEMENT fires; "mentor through" = gap warning.

---

## B34-router-020-003 — Gap route: "coach on how to build" → "teach to X" pattern

A: "Coach the backend team on how to build resilient services with circuit breakers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "coach on how to build" = "teach to X" pattern → gap. The "to build" infinitive is what others learn. Requester's action = coach = gap.

---

## B34-router-020-004 — IMPLEMENT+DOCUMENT route: "coach through building and documenting"

A: "Coach the team through building the rate limiter and documenting the design decisions."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 + DocumentationFlow
Evidence: "building the rate limiter" → IMPLEMENT; "documenting the design decisions" → DOCUMENT; "coach through" = gap warning alongside both predicates.

---

## B34-router-020-005 — Gap route: "help understand building" → learning intent

A: "Help the team understand how to build fault-tolerant distributed systems."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "help understand how to build" = "teach to X" pattern + understanding intent → gap. No direct "build" action by requester or with requester's direct agency.
