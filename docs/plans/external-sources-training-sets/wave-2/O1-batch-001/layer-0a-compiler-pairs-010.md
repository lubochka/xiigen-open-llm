# Layer 0-A — Compiler Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-010-001 — DOCUMENT fires on "document the authentication API"

A: "Document the authentication API for the developer portal."
B: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation

Lexicon evidence:
  DOCUMENT matched on: "document"
  SECURITY matched on: "authentication"
  API_CONTRACT matched on: "api"
  TransitionType: documentation — DOCUMENT fires without IMPLEMENT

---

## O1-compiler-010-002 — DOCUMENT fires on "write docs"

A: "Write docs for the JWT token lifecycle and rotation policy."
B: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation

Lexicon evidence:
  DOCUMENT matched on: "write docs"
  SECURITY matched on: "jwt", "token"
  TransitionType: documentation — DOCUMENT fires without IMPLEMENT
  Note: "write docs" fires DOCUMENT, not IMPLEMENT — the key is "docs" qualifier

---

## O1-compiler-010-003 — DOCUMENT fires on "specification"

A: "Write the specification for tenant onboarding and permission provisioning."
B: Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  DOCUMENT matched on: "specification", "write"
  AUTHORIZATION matched on: "tenant", "permission"
  TransitionType: documentation — DOCUMENT+TEST fire without IMPLEMENT

---

## O1-compiler-010-004 — DOCUMENT+API_CONTRACT dual

A: "Document the REST endpoints for the RBAC (Role-Based Access Control) management API including request and response schema."
B: Predicates: ["DOCUMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  DOCUMENT matched on: "document"
  API_CONTRACT matched on: "rest", "endpoints", "api", "request", "response", "schema"
  AUTHORIZATION matched on: "rbac", "access control"
  RBAC expansion: RBAC = Role-Based Access Control
  TransitionType: documentation — DOCUMENT fires without IMPLEMENT

---

## O1-compiler-010-005 — DOCUMENT+SECURITY+AUTHORIZATION triple

A: "Write the security specification covering JWT (JSON Web Token) issuance, tenant scope claims, and MFA (Multi-Factor Authentication) enforcement policies."
B: Predicates: ["DOCUMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  DOCUMENT matched on: "write", "specification"
  SECURITY matched on: "security", "jwt", "token", "mfa"
  AUTHORIZATION matched on: "tenant", "scope", "claims"
  JWT expansion: JWT = JSON Web Token
  MFA expansion: MFA = Multi-Factor Authentication
  TransitionType: documentation — DOCUMENT fires without IMPLEMENT
