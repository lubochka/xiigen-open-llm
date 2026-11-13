# Layer 0-A — Compiler Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-009-001 — IMPLEMENT+SECURITY+TEST from "build" + "token" + "write tests"

A: "Build the token blacklist service and write tests for revocation behavior."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  SECURITY matched on: "token"
  TEST matched on: "write tests"
  TransitionType: feature_addition — IMPLEMENT fires (takes priority over documentation)

---

## O1-compiler-009-002 — IMPLEMENT+SECURITY+TEST from "implement" + "auth" + "assert"

A: "Implement the auth session manager and assert session expiry boundaries in the test suite."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "implement"
  SECURITY matched on: "auth", "session"
  TEST matched on: "assert", "test suite"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-009-003 — IMPLEMENT+SECURITY+TEST from "add" + "credential" + "unit tests"

A: "Add credential hashing to the user service and add unit tests for all hash variants."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "add" (twice; fires once)
  SECURITY matched on: "credential", "hash"
  TEST matched on: "unit tests"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-009-004 — IMPLEMENT+SECURITY+TEST from "create" + "certificate" + "integration test"

A: "Create the certificate validation module and write integration tests for TLS (Transport Layer Security) handshake scenarios."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "create"
  SECURITY matched on: "certificate", "validate" (security context), "tls"
  TEST matched on: "integration tests"
  TLS expansion: TLS = Transport Layer Security
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-009-005 — IMPLEMENT+SECURITY+AUTHORIZATION+TEST four-predicate

A: "Build tenant-aware JWT middleware, enforce RBAC (Role-Based Access Control), and write unit tests for permission boundary assertions."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  SECURITY matched on: "jwt"
  AUTHORIZATION matched on: "tenant", "rbac", "permission", "role"
  TEST matched on: "write unit tests", "assertions"
  RBAC expansion: RBAC = Role-Based Access Control
  TransitionType: feature_addition — IMPLEMENT fires
