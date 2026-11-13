# Layer 0-A — Compiler Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-002-001 — SECURITY fires on "jwt"

A: "We need to implement jwt validation for all API requests."
B: Predicates: ["SECURITY", "API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  SECURITY matched on: "jwt"
  API_CONTRACT matched on: "api"
  TransitionType: feature_addition — IMPLEMENT fires on "implement"

---

## O1-compiler-002-002 — AUTHORIZATION fires on "tenant"

A: "Each tenant should have its own isolated data scope."
B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  AUTHORIZATION matched on: "tenant"
  IMPLEMENT fires on implicit "have" (structural addition)
  Note: no SECURITY predicate — no jwt/auth/token/session/etc. in this prompt

---

## O1-compiler-002-003 — SECURITY+AUTHORIZATION from "jwt" + "tenant"

A: "Add tenant-aware jwt token validation to the auth middleware."
B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  SECURITY matched on: "jwt", "token", "auth"
  AUTHORIZATION matched on: "tenant"
  IMPLEMENT fires on "add"

---

## O1-compiler-002-004 — SECURITY alone from "authentication"

A: "Harden the authentication flow to prevent replay attacks."
B: Predicates: ["SECURITY"] | TransitionType: feature_addition

Lexicon evidence:
  SECURITY matched on: "authentication", "harden"
  IMPLEMENT fires on "harden" (structural modification)
  No AUTHORIZATION — no tenant/role/permission/scope signal

---

## O1-compiler-002-005 — AUTHORIZATION alone from "role"

A: "Build the role assignment service for tenant administrators."
B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  AUTHORIZATION matched on: "role", "tenant"
  IMPLEMENT fires on "build"
  No SECURITY — no jwt/auth/token/session/etc. in this prompt
