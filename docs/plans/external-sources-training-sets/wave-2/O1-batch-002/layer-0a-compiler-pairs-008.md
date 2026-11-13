# Layer 0-A — Compiler Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-008-001 — SECURITY+AUTHORIZATION+API_CONTRACT: three-predicate

A: "design the OAuth 2.0 token API with per-tenant scope enforcement"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

---

## O1b-0A-008-002 — SECURITY+AUTHORIZATION+API_CONTRACT: three-predicate

A: "specify the authorization server API contract including scope definitions and token endpoint security"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: documentation

---

## O1b-0A-008-003 — SECURITY+AUTHORIZATION+API_CONTRACT: three-predicate

A: "the token introspection endpoint must validate JWT claims and enforce tenant scope policy"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

---

## O1b-0A-008-004 — SECURITY+AUTHORIZATION+API_CONTRACT: three-predicate

A: "build the API gateway authentication layer with OAuth scopes and tenant ACL (Access Control List)"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT"] | TransitionType: feature_addition

---

## O1b-0A-008-005 — SECURITY+AUTHORIZATION+API_CONTRACT: three-predicate

A: "define client credential grant in the OpenAPI spec, including scope claims and tenant principal binding"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: documentation
