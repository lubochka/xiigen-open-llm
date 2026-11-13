# Layer 0-B — Router Pairs (057)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-057-001 — "explain how [system] works" routing = GapHandler

A: "Explain how the distributed tracing pipeline works to the platform team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "explain how [X] works" = teaching frame (explanation of existing system) → GapHandler.

---

## B34-router-057-002 — "walk through how [system] works" routing = GapHandler

A: "Walk the team through how the auto-scaling policy works."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "walk through how [X] works" = explanation vocabulary → GapHandler.

---

## B34-router-057-003 — "demonstrate how [system] works" routing = GapHandler

A: "Demonstrate how the circuit breaker handles cascading failures to the new engineers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "demonstrate how [X] works" = teaching frame → GapHandler. Failure handling topic does not fire SECURITY or IMPLEMENT.

---

## B34-router-057-004 — "present how [system] works" routing = GapHandler

A: "Present how the event replay mechanism works to the incident response team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "present how [X] works" = presentation vocabulary → GapHandler.

---

## B34-router-057-005 — "discuss how [system] works" routing = GapHandler

A: "Discuss how the service mesh handles mTLS (mutual Transport Layer Security) with the security team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "discuss how [X] works" = collaborative guidance → GapHandler. Security topic in discussion frame does not route to SecurityAuditFlow.
