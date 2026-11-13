# Layer 0-B — Router Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-003-001 — TEST route (embedded in guidance)

A: "Coach the team through writing integration tests for the payments module."
Router decision: Predicates = ["TEST"] → TestingFlow (E2ETestingFlow53 for integration)
Evidence: "coach" → gap; "writing integration tests" → TEST embedded. TEST fires.

---

## B34-router-003-002 — Gap route: "train on" methodology

A: "Train the team on event sourcing and CQRS patterns."
Router decision: Predicates = [] → GapHandler
Gap: gap-skills-guidance-train-001 | DomainId: software-development

---

## B34-router-003-003 — ARCHITECTURE route (embedded design verb)

A: "Walk the team through architecting the new microservices platform."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 (3.1)
Evidence: "architect" embedded in "walk through" guidance frame → ARCHITECTURE fires.

---

## B34-router-003-004 — QA route in guidance context

A: "QA the developer onboarding workflow before the new cohort starts."
Router decision: Predicates = ["QA"] → QAGatingFlow73 (7.3 — feature acceptance)
Evidence: "QA" abbreviation fires; "developer onboarding workflow" = feature under QA gate.

---

## B34-router-003-005 — v6 corruption: "explain" routed to DOCUMENT

v6 corrupt: "Explain the API documentation structure" → ["DOCUMENT"] → DocumentationFlow
Correct: [""] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "explain" is teaching vocabulary → gap. Even with documentation object, "explain" ≠ DOCUMENT trigger.
Fix: DOCUMENT requires document/write/create (documentation artifact) action verbs.
