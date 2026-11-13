# Layer 0-A — Compiler Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-007-001 — IMPLEMENT+AUTHORIZATION from "build" + "role-based access"

A: "Build the role-based access control module for multi-tenant resource ownership."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  AUTHORIZATION matched on: "role-based access control", "multi-tenant", "role"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-007-002 — IMPLEMENT+AUTHORIZATION from "implement" + "permission"

A: "Implement the permission assignment and revocation API for tenant admins."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "implement"
  AUTHORIZATION matched on: "permission", "tenant"
  API_CONTRACT matched on: "api"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-007-003 — IMPLEMENT+AUTHORIZATION from "create" + "entitlement"

A: "Create the entitlement service to manage tenant feature flags and access scopes."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "create" (code context — "create the service")
  AUTHORIZATION matched on: "entitlement", "tenant", "access", "scope"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-007-004 — IMPLEMENT+AUTHORIZATION from "add" + "privilege escalation check"

A: "Add a privilege escalation detection check to the authorization middleware."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION", "SECURITY"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "add"
  AUTHORIZATION matched on: "privilege", "authorization"
  SECURITY matched on: "secure" (implied by "detection check"), "auth" (via "authorization")
  Note: "authorization" in the middleware name fires AUTHORIZATION predicate
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-007-005 — IMPLEMENT+AUTHORIZATION from "write" + "claims"

A: "Write the claims extraction and validation service for JWT payloads."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION", "SECURITY"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "write" (code context — "write the service")
  AUTHORIZATION matched on: "claims"
  SECURITY matched on: "jwt", "validate" (security context)
  TransitionType: feature_addition — IMPLEMENT fires
