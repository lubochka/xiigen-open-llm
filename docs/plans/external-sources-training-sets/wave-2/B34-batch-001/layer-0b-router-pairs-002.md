# Layer 0-B — Router Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-002-001 — Gap route: pure mentoring

A: "Mentor the junior developers on clean code."
Router decision: Predicates = [] → GapHandler route
GapHandler: SCHEDULE_CAPABILITY_LEARNING → gap-skills-guidance-mentor-001 | DomainId: software-development
No flow selected.

---

## B34-router-002-002 — DOCUMENT route

A: "Write the onboarding guide for new backend engineers."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Sub-phase: 6.2 (standard documentation)

---

## B34-router-002-003 — IMPLEMENT route

A: "Build the skill assessment platform."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Sub-phase: 4.1

---

## B34-router-002-004 — IMPLEMENT+DOCUMENT route

A: "Build the training module system and document the curriculum structure."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 + DocumentationFlow
TransitionType: feature_addition

---

## B34-router-002-005 — API_CONTRACT route in guidance context

A: "Integrate the LMS (Learning Management System) with the corporate SSO (Single Sign-On) provider."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: "integrate" always fires API_CONTRACT; LMS + SSO = external system integration.
