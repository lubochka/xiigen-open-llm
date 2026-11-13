# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-003-001 — SECURITY+AUTHORIZATION → SecurityVerification34Flow (one flow, two predicates)

Request: "Add tenant-aware JWT token validation to the auth middleware."
Predicates: ["SECURITY", "AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Flow: SecurityVerification34Flow instantiated once; EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Key rule: Two predicates, same sub-phase = one flow instance (not two). The orchestrator
  passes both predicates as evidence to the single flow branch.

---

## TO-003-002 — SECURITY+AUTHORIZATION from MFA prompt

Request: "Add MFA (Multi-Factor Authentication) enforcement for tenant admin roles."
Predicates: ["SECURITY", "AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
MFA expansion: MFA = Multi-Factor Authentication
Note: "mfa" fires SECURITY; "tenant", "role" fire AUTHORIZATION; single flow branch

---

## TO-003-003 — SECURITY+AUTHORIZATION from OAuth+tenant prompt

Request: "Implement OAuth2 consent with tenant-scoped permission grants."
Predicates: ["SECURITY", "AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Note: "oauth" fires SECURITY; "tenant", "permission", "scope" fire AUTHORIZATION; single flow branch

---

## TO-003-004 — SECURITY+AUTHORIZATION from OIDC+RBAC prompt

Request: "Implement OIDC (OpenID Connect) token introspection and RBAC (Role-Based Access Control) policy enforcement."
Predicates: ["SECURITY", "AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
OIDC expansion: OIDC = OpenID Connect
RBAC expansion: RBAC = Role-Based Access Control

---

## TO-003-005 — SECURITY+AUTHORIZATION from credential+privilege prompt

Request: "Validate credentials and enforce privilege boundaries in the session manager."
Predicates: ["SECURITY", "AUTHORIZATION"] | Orchestration: SecurityVerification34Flow
TransitionType: feature_addition
Sub-phase: 3.4 | FactoryState: trusted_learned
Note: "validate" (security context), "credential", "session" fire SECURITY;
  "privilege" fires AUTHORIZATION; single flow branch to 3.4
