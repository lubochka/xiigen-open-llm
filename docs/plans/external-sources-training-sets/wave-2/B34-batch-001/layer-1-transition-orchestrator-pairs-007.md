# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-007-001 — SECURITY+IMPLEMENT orchestration

Request: "Build the training portal and secure the user profile data."
Predicates: ["IMPLEMENT", "SECURITY"]
Orchestration: ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition

---

## TO-007-002 — API_CONTRACT+DOCUMENT orchestration

Request: "Integrate the LMS with Confluence and document the integration API."
Predicates: ["API_CONTRACT", "DOCUMENT"]
Orchestration: APIContractFlow33 ‖ DocumentationFlow (parallel; independent)
TransitionType: documentation (no IMPLEMENT)

---

## TO-007-003 — Gap within five-verb: one fires, GapHandler suppressed

Request: "Train, mentor, coach, guide, and document the engineering practices."
Predicates: ["DOCUMENT"] (four gap verbs + one DOCUMENT verb)
Orchestration: DocumentationFlow only
GapHandler: NOT invoked (DOCUMENT fires)
Gap warning: gap-skills-guidance-mentor-001 as CapabilityGapWarning only

---

## TO-007-004 — AUTHORIZATION+IMPLEMENT orchestration

Request: "Build the learning portal and add role-based access for instructors and students."
Predicates: ["IMPLEMENT", "AUTHORIZATION"]
Orchestration: ImplementationFlow41 → AuthorizationFlow
TransitionType: feature_addition

---

## TO-007-005 — v6 corruption: ordering violation

v6 corrupt: "Build LMS and QA the registration flow" → Orchestration: QAGatingFlow73 → ImplementationFlow41 (QA first)
Correct: ImplementationFlow41 → QAGatingFlow73 (QA always last)
Error type: ordering violation. QA validates completed implementation; running QA before IMPLEMENT is invalid.
