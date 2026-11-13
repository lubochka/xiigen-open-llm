# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-009-001 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA full

Request: "Build the skills platform, integrate with Okta, secure auth, write E2E tests, document the API, and QA."
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → E2ETestingFlow53 ‖ DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition | QA absolute last

---

## TO-009-002 — Mixed guidance + code: one gap item, four predicates

Request: "Coach the team, build the tracker, integrate with HR, write tests, and document the API."
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
Gap: coach → gap warning only (GapHandler not invoked)
Orchestration: APIContractFlow33 → ImplementationFlow41 → TestingFlow ‖ DocumentationFlow
TransitionType: feature_addition

---

## TO-009-003 — Observation sentence → gap

Request: "The junior developers are struggling with the codebase."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Evidence: Observation with no action verb firing any predicate.
Note: Observation statements always yield zero predicates, same as B33.

---

## TO-009-004 — ARCHITECTURE+IMPLEMENT+DOCUMENT

Request: "Design the certification system architecture, build the backend, and document the design decisions."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition

---

## TO-009-005 — Gap: pure "training program" request

Request: "Create a six-week training program for mid-level engineers."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Evidence: "create a training program" = curriculum creation → gap (not IMPLEMENT — training program is not a software artifact).
