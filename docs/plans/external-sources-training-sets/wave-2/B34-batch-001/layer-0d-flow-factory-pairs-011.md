# Layer 0-D — Flow Factory Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-011-001 — IMPLEMENT+AUTHORIZATION factory

Predicates: ["IMPLEMENT", "AUTHORIZATION"]
Request: "Build the mentorship platform and add role-based access for managers and mentees."
Factory: ImplementationFlow41 + AuthorizationFlow
TransitionType: feature_addition

---

## FF-011-002 — IMPLEMENT+SECURITY factory in skills context

Predicates: ["IMPLEMENT", "SECURITY"]
Request: "Build the training portal and secure the user data storage."
Factory: ImplementationFlow41 + SecurityAuditFlow
TransitionType: feature_addition

---

## FF-011-003 — TEST alone: E2E test factory for portal

Predicates: ["TEST"]
Request: "Write E2E tests for the developer onboarding portal user registration flow."
Factory: E2ETestingFlow53 (5.3 — E2E + user flow → 5.3)
TransitionType: documentation

---

## FF-011-004 — Gap factory: "upskill" request

Predicates: []
Request: "Upskill the backend team on Kubernetes deployment."
Factory: GapHandler → gap-skills-guidance-train-001
DomainId: software-development

---

## FF-011-005 — v6 corruption: ONBOARDING flow invented

v6 corrupt: "Onboard new engineers" → Predicates: ["ONBOARDING"] → OnboardingFlow selected
Correct: Predicates: [] → GapHandler → gap-skills-guidance-onboard-001
Error type: v6-01 — ONBOARDING predicate does not exist; OnboardingFlow does not exist.
Fix: Onboarding vocabulary → GapHandler. No predicate factory selection.
