# Layer 0-B — Router Pairs (053)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-053-001 — mixed purpose clause + direct code verbs routing

A: "For knowledge sharing purposes, implement the reference pattern and document it for the team."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 ‖ DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "for knowledge sharing purposes" = purpose clause → gap warning; implement → IMPLEMENT; document → DOCUMENT. TransitionType: feature_addition.

---

## B34-router-053-002 — guidance in subordinate clause does not change routing

A: "Implement the health check endpoint so you can walk the team through it later."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "implement health check endpoint" = IMPLEMENT; "walk team through later" = future guidance intent (subordinate) = gap warning (mild). IMPLEMENT is the primary action.

---

## B34-router-053-003 — "show how to" in question form = GapHandler

A: "Can you show us how to write idiomatic error handling in Go (Go programming language)?"
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: interrogative form of "show how to" = still teaching frame → GapHandler.

---

## B34-router-053-004 — "demonstrate" in question form = GapHandler

A: "Could you demonstrate how the circuit breaker pattern works?"
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: interrogative + demonstrate how = teaching frame → GapHandler. Patterns topic does not fire IMPLEMENT.

---

## B34-router-053-005 — "consult" in request form = GapHandler

A: "I'd like to consult with the architecture team on our database sharding options."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: request form of "consult with" = advisory vocabulary → GapHandler. Database topic does not fire ARCHITECTURE.
