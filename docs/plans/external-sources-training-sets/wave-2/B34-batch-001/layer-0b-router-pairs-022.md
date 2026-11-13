# Layer 0-B — Router Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-022-001 — Gap route: "explain the architecture" → always gap

A: "Explain the event-driven architecture to the new backend team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "explain" = always gap, regardless of architecture topic.

---

## B34-router-022-002 — DOCUMENT route: "explain and document" → DOCUMENT fires

A: "Explain the system design and document the component interactions in the architecture guide."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "document the component interactions" → DOCUMENT fires; "explain" = gap warning. Explain itself never fires any predicate.

---

## B34-router-022-003 — Gap route: "explain the security model"

A: "Explain the OAuth2 security model and token refresh strategy to the frontend team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "explain" = always gap. Security topic does not change classification.

---

## B34-router-022-004 — IMPLEMENT route: "explain and implement"

A: "Explain the circuit breaker pattern and implement it in the order service."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "implement it in the order service" → IMPLEMENT; "explain" = gap warning alongside IMPLEMENT.

---

## B34-router-022-005 — TEST route: "explain and write tests"

A: "Explain the testing strategy and write the acceptance tests for the login flow."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51
Evidence: "write the acceptance tests" → TEST; "explain" = gap warning alongside TEST. Explain contributes no predicate even in same sentence as TEST-firing verb.
