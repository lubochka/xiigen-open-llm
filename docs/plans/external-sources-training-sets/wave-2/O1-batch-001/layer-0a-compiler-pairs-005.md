# Layer 0-A — Compiler Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-005-001 — IMPLEMENT+SECURITY from "build" + "secure"

A: "Build a secure token storage service that encrypts credentials at rest."
B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  SECURITY matched on: "secure", "token", "encrypt", "credentials"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-005-002 — IMPLEMENT+SECURITY from "implement" + "hash"

A: "Implement password hashing using bcrypt before storing user credentials."
B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "implement"
  SECURITY matched on: "hash", "credential"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-005-003 — IMPLEMENT+SECURITY from "scaffold" + "tls"

A: "Scaffold the TLS (Transport Layer Security) certificate rotation service."
B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "scaffold"
  SECURITY matched on: "tls", "certificate"
  TLS expansion: TLS = Transport Layer Security
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-005-004 — IMPLEMENT+SECURITY from "write" (code) + "validate"

A: "Write the input sanitization and validation layer for the auth service."
B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "write" (code context — "write the layer")
  SECURITY matched on: "sanitize", "validate" (security context), "auth"
  TransitionType: feature_addition — IMPLEMENT fires
  Note: "write" in code context fires IMPLEMENT; "write tests" fires TEST instead

---

## O1-compiler-005-005 — IMPLEMENT+SECURITY+AUTHORIZATION from "add" + "secret" + "rbac"

A: "Add secret rotation support and enforce RBAC (Role-Based Access Control) on the secrets API."
B: Predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "add"
  SECURITY matched on: "secret"
  AUTHORIZATION matched on: "rbac", "access control"
  API_CONTRACT matched on: "api"
  RBAC expansion: RBAC = Role-Based Access Control
  TransitionType: feature_addition — IMPLEMENT fires
