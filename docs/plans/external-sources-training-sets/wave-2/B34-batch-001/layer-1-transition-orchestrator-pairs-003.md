# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-003-001 — API_CONTRACT+IMPLEMENT ordering

Request: "Integrate the LMS with the SSO provider and build the authentication module."
Predicates: ["API_CONTRACT", "IMPLEMENT"]
Orchestration: APIContractFlow33 → ImplementationFlow41
Ordering: API_CONTRACT first (contract spec informs implementation)
TransitionType: feature_addition

---

## TO-003-002 — ARCHITECTURE+IMPLEMENT ordering

Request: "Architect the learning platform and build the course enrollment service."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41
Ordering: ARCHITECTURE first (design before build)
TransitionType: feature_addition

---

## TO-003-003 — TEST+DOCUMENT parallel-eligible

Request: "Write E2E tests for the portal and document the test coverage requirements."
Predicates: ["TEST", "DOCUMENT"]
Orchestration: E2ETestingFlow53 ‖ DocumentationFlow (parallel; no dependency)
TransitionType: documentation (no IMPLEMENT)

---

## TO-003-004 — Gap with mixed guidance vocabulary

Request: "Mentor, coach, and onboard the new engineering cohort."
Predicates: [] | Single GapHandler invocation
Gap ID: gap-skills-guidance-mentor-001 (mentor priority over onboard)
Single SCHEDULE_CAPABILITY_LEARNING | DomainId: software-development

---

## TO-003-005 — Refactor TransitionType in B34

Request: "Mentor the team on refactoring and refactor the legacy onboarding module."
Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Orchestration: ImplementationFlow41
"mentor" → gap; "refactor" → IMPLEMENT + refactoring TransitionType.
