# Layer 0-B — Router Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-007-001 — TEST+DOCUMENT route

A: "Write unit tests for the authentication service and document the test coverage strategy."
Router decision: Predicates = ["TEST", "DOCUMENT"] → UnitTestingFlow51 + DocumentationFlow
TransitionType: documentation (no IMPLEMENT)

---

## B34-router-007-002 — Gap route: "advise" → gap

A: "Advise the team on the best microservices decomposition strategy."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "advise" = advisory/consulting vocabulary → gap.

---

## B34-router-007-003 — IMPLEMENT route with skill context

A: "Implement the progress tracking dashboard for the junior developer learning program."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "implement" = code verb; "progress tracking dashboard" = software component (dashboard is an IMPLEMENT target).
Note: "dashboard" is an IMPLEMENT target here, not a UI predicate trigger (UI predicate fires on UI component vocabulary in testing context, not on dashboard as general software artifact in B34 context).

---

## B34-router-007-004 — Gap route: "buddy system" → gap

A: "Set up a buddy system for new engineer onboarding."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "set up a buddy system" = HR/mentoring program vocabulary → gap.

---

## B34-router-007-005 — Six-predicate route

A: "Design the learning platform architecture, build it, integrate with the LMS, secure auth, write tests, and document the API."
Router decision: Predicates = ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"]
Routes: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestingFlow + DocumentationFlow
TransitionType: feature_addition
