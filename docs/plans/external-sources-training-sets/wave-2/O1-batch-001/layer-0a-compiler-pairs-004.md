# Layer 0-A — Compiler Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-004-001 — TEST fires on "write tests"

A: "Write tests for the JWT token validation middleware."
B: Predicates: ["TEST", "SECURITY"] | TransitionType: documentation

Lexicon evidence:
  TEST matched on: "write tests"
  SECURITY matched on: "jwt", "token"
  TransitionType: documentation — TEST/DOCUMENT fire without IMPLEMENT
  Note: "write tests" is in the TEST lexicon; TransitionType = documentation

---

## O1-compiler-004-002 — TEST fires on "assert"

A: "Assert that each tenant's data is unreachable by other tenants in integration tests."
B: Predicates: ["TEST", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  TEST matched on: "assert", "integration tests"
  AUTHORIZATION matched on: "tenant"
  TransitionType: documentation — TEST fires without IMPLEMENT

---

## O1-compiler-004-003 — "verify isolation" = TEST not SECURITY (critical boundary)

A: "Verify isolation between tenant auth contexts in the test suite."
B: Predicates: ["TEST", "SECURITY", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  TEST matched on: "verify isolation", "test suite"
  SECURITY matched on: "auth"
  AUTHORIZATION matched on: "tenant"
  CRITICAL: "verify isolation" fires TEST, not SECURITY
  "isolation" alone is not in the SECURITY lexicon
  TransitionType: documentation — TEST fires without IMPLEMENT

---

## O1-compiler-004-004 — TEST fires on "unit tests"

A: "Create unit tests for the permission service to check role boundary enforcement."
B: Predicates: ["TEST", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  TEST matched on: "unit tests"
  AUTHORIZATION matched on: "permission", "role"
  TransitionType: documentation — TEST fires without IMPLEMENT
  IMPLEMENT does NOT fire on "create" in this context (create tests = documentation intent)

---

## O1-compiler-004-005 — TEST fires on "write test suite"

A: "Write a test suite covering all RBAC (Role-Based Access Control) permission scenarios."
B: Predicates: ["TEST", "AUTHORIZATION"] | TransitionType: documentation

Lexicon evidence:
  TEST matched on: "write test suite"
  AUTHORIZATION matched on: "rbac", "permission", "access control", "role"
  TransitionType: documentation — TEST fires without IMPLEMENT
  Note: RBAC expansion: RBAC = Role-Based Access Control
