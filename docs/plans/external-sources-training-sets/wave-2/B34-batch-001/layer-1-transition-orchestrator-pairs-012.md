# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-012-001 — Canonical orchestration order for B34

For any predicate combination in B34:
1. ARCHITECTURE first (if present)
2. API_CONTRACT before IMPLEMENT
3. IMPLEMENT
4. SECURITY (post-validates IMPLEMENT output)
5. TEST (runs on implemented code)
6. DOCUMENT (parallel with TEST after IMPLEMENT)
7. QA last — absolute

Guidance vocabulary = gap warning if predicates present; zero predicates = full GapHandler invocation.

---

## TO-012-002 — IMPLEMENT+AUTHORIZATION+QA

Request: "Build the learning platform, add RBAC for course instructors, and QA the access control."
Predicates: ["IMPLEMENT", "AUTHORIZATION", "QA"]
Orchestration: ImplementationFlow41 → AuthorizationFlow → QAGatingFlow73
TransitionType: feature_addition | QA last

---

## TO-012-003 — Gap: "pair programming" → gap

Request: "Organize pair programming sessions for code quality improvement."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001

---

## TO-012-004 — Embedded verb in complex guidance frame

Request: "Run a bootcamp where the team will architect and build the new service."
Analysis: "run a bootcamp" = gap frame; "architect" + "build" = embedded direct actions.
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-012-005 — DomainId: no education/training domain

All B34 gap requests:
DomainId: software-development (always)
Gap IDs: gap-skills-guidance-mentor-001, gap-skills-guidance-onboard-001, gap-skills-guidance-train-001
Event: SCHEDULE_CAPABILITY_LEARNING
Response: "Capability not available. Request logged for learning pipeline."
No alternative response for guidance gaps. Identical GapHandler flow for all three gap IDs.
