# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-008-001 — IMPLEMENT+TEST+QA three-flow

Request: "Create the automated onboarding checklist app, write tests for the workflow, and QA sign-off."
Predicates: ["IMPLEMENT", "TEST", "QA"]
Orchestration: ImplementationFlow41 → TestingFlow → QAGatingFlow73
TransitionType: feature_addition | QA sub-phase 7.3

---

## TO-008-002 — ARCHITECTURE alone

Request: "Architect the internal learning management platform."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31 (3.1 — application-level platform)
TransitionType: documentation (IMPLEMENT absent)

---

## TO-008-003 — TEST alone (embedded test-writing in guidance)

Request: "Walk the junior team through writing integration tests for the payment service."
Predicates: ["TEST"] (write integration tests → E2ETestingFlow53)
Orchestration: E2ETestingFlow53 (5.3 — integration test → 5.3)
TransitionType: documentation
Gap: "walk through" → suppressed by TEST firing.

---

## TO-008-004 — QA+DOCUMENT orchestration

Request: "Document the certification requirements and QA the process before rollout."
Predicates: ["DOCUMENT", "QA"]
Orchestration: DocumentationFlow → QAGatingFlow73
TransitionType: documentation | QA last

---

## TO-008-005 — Gap: multiple onboarding vocabulary items

Request: "Set up a buddy system and run orientation sessions for new hires."
Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Evidence: "set up buddy system" → gap; "run orientation sessions" → gap. Both onboarding vocabulary.
Single SCHEDULE_CAPABILITY_LEARNING emitted.
