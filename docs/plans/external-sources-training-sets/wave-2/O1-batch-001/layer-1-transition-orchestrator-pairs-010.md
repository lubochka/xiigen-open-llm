# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-010-001 — QA always last: SECURITY+QA → SecurityVerification34Flow → QAGatingFlow73

Request: "Implement JWT validation and QA the security gates."
Predicates: ["IMPLEMENT", "SECURITY", "QA"] | Orchestration: ImplementationFlow41 → SecurityVerification34Flow → QAGatingFlow73
TransitionType: feature_addition
Branch 1: ImplementationFlow41 (4.1) | sequential Phase 1
Branch 2: SecurityVerification34Flow (3.4) | sequential Phase 2
Branch 3: QAGatingFlow73 (7.x) | sequential Phase 3 (always last)
QA rule: QA is always the final branch regardless of predicate discovery order

---

## TO-010-002 — QA with all predicates: SECURITY+AUTHORIZATION+API_CONTRACT+TEST+QA

Request: "Build the JWT tenant auth API, test all isolation boundaries, and QA the full security suite."
Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST", "QA"]
Orchestration: ImplementationFlow41 → (SecurityVerification34Flow ‖ ApiContract33Flow) → generic-5.1 → QAGatingFlow73
TransitionType: feature_addition
Phase 1: ImplementationFlow41 (4.1)
Phase 2: SecurityVerification34Flow (3.4) ‖ ApiContract33Flow (3.3)
Phase 3: generic nine-node (5.1)
Phase 4: QAGatingFlow73 (7.x) — last

---

## TO-010-003 — QA-only prompt (with SECURITY co-trigger)

Request: "QA the authentication service before the release."
Predicates: ["SECURITY", "QA"] (SECURITY fires on "authentication")
Orchestration: SecurityVerification34Flow → QAGatingFlow73
TransitionType: feature_addition (SECURITY implies structural work)
Branch 1: SecurityVerification34Flow (3.4) | sequential Phase 1
Branch 2: QAGatingFlow73 (7.x) | sequential Phase 2 (last)

---

## TO-010-004 — QA ordering rule: QA fires in user prompt first but still goes last

Request: "QA the JWT token flow and then implement the token refresh mechanism."
Predicates: ["QA", "IMPLEMENT", "SECURITY"] (QA appears first in text)
Orchestration: ImplementationFlow41 → SecurityVerification34Flow → QAGatingFlow73
TransitionType: feature_addition
Note: QA appears first in the user request text, but the orchestrator always sequences
  QAGatingFlow73 last. The text order of predicates does not override the canonical ordering.
Branch sequence: [ImplementationFlow41 (Phase 1), SecurityVerification34Flow (Phase 2), QAGatingFlow73 (Phase 3 — last)]

---

## TO-010-005 — QA with DOCUMENT: QA after DOCUMENT

Request: "Document the JWT security model and then QA the auth flow."
Predicates: ["SECURITY", "DOCUMENT", "QA"]
Orchestration: SecurityVerification34Flow ‖ RequirementsAuthoringFlow21 → QAGatingFlow73
TransitionType: documentation (no IMPLEMENT)
Branch 1: SecurityVerification34Flow (3.4) | parallel in Group A
Branch 2: RequirementsAuthoringFlow21 (2.1) | parallel in Group A
Branch 3: QAGatingFlow73 (7.x) | sequential — after Group A (always last)
