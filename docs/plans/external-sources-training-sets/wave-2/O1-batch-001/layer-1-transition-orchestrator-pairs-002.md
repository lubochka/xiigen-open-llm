# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-002-001 — SECURITY alone → SecurityVerification34Flow

Request: "Harden the authentication flow to prevent replay attacks."
Predicates: ["SECURITY"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Flow: SecurityVerification34Flow instantiated; single-branch execution

---

## TO-002-002 — AUTHORIZATION alone → SecurityVerification34Flow (same flow)

Request: "Build the role assignment service for tenant administrators."
Predicates: ["AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Flow: SecurityVerification34Flow instantiated; AUTHORIZATION alone is sufficient to trigger 3.4
Note: The flow is identical whether SECURITY, AUTHORIZATION, or both fire. The 3.4 flow
  is the Security and Authorization Model flow — both predicates map to the same flow.

---

## TO-002-003 — SECURITY alone from token prompt

Request: "The auth token expiry is set too short — fix the calculation."
Predicates: ["SECURITY"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Note: "auth" and "token" both fire SECURITY; single predicate → single flow branch

---

## TO-002-004 — AUTHORIZATION alone from permission prompt

Request: "Add permission scoping to the entitlement service for each tenant."
Predicates: ["AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Note: "permission", "entitlement", "tenant", "scope" all fire AUTHORIZATION; single flow branch

---

## TO-002-005 — knowledge_edge → no transition registered

Request: "The login page shows an error after the deploy."
Predicates: [] | Orchestration: none (knowledge_edge synthetic path)
TransitionType: none
KnowledgeEdge: true | FailureClassification: no_predicate_match
Flow: no production flow instantiated; knowledge_edge handler invoked
