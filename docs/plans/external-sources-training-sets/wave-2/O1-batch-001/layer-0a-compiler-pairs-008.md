# Layer 0-A — Compiler Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-008-001 — IMPLEMENT+SECURITY+AUTHORIZATION from "build" + "jwt" + "tenant"

A: "Build the JWT validation pipeline with tenant-aware claims enforcement."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  SECURITY matched on: "jwt", "claims" (security context — token claims)
  AUTHORIZATION matched on: "tenant", "claims" (authorization context — claims enforcement)
  Note: "claims" fires both SECURITY (token claims) and AUTHORIZATION (entitlement claims)
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-008-002 — IMPLEMENT+SECURITY+AUTHORIZATION from "add" + "mfa" + "tenant admin"

A: "Add MFA (Multi-Factor Authentication) enforcement for tenant admin roles."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "add"
  SECURITY matched on: "mfa", "authentication"
  AUTHORIZATION matched on: "tenant", "admin", "role"
  MFA expansion: MFA = Multi-Factor Authentication
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-008-003 — IMPLEMENT+SECURITY+AUTHORIZATION from "implement" + "oidc" + "rbac"

A: "Implement OIDC (OpenID Connect) token introspection and RBAC (Role-Based Access Control) policy enforcement."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "implement"
  SECURITY matched on: "oidc", "token"
  AUTHORIZATION matched on: "rbac", "policy"
  OIDC expansion: OIDC = OpenID Connect
  RBAC expansion: RBAC = Role-Based Access Control
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-008-004 — IMPLEMENT+SECURITY+AUTHORIZATION from "scaffold" + "oauth" + "scope"

A: "Scaffold the OAuth2 (Open Authorization 2.0) consent service with tenant-scoped permission grants."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "scaffold"
  SECURITY matched on: "oauth"
  AUTHORIZATION matched on: "scope", "permission", "tenant"
  OAuth2 expansion: OAuth2 = Open Authorization 2.0
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-008-005 — IMPLEMENT+SECURITY+AUTHORIZATION+API_CONTRACT full quad

A: "Build the secure tenant API with role-based token scoping and OpenAPI contract."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  SECURITY matched on: "secure", "token"
  AUTHORIZATION matched on: "tenant", "role", "scope"
  API_CONTRACT matched on: "api", "openapi", "contract"
  TransitionType: feature_addition — IMPLEMENT fires
