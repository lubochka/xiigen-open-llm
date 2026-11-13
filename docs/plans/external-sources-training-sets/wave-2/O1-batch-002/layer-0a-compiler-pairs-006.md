# Layer 0-A — Compiler Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-006-001 — SECURITY+AUTHORIZATION: oauth + scope

A: "configure oauth scopes for each tenant's service account"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-006-002 — SECURITY+AUTHORIZATION: jwt + tenant

A: "validate the JWT (JSON Web Token) claims to enforce tenant-level access boundaries"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-006-003 — SECURITY+AUTHORIZATION: token + role

A: "decode the bearer token and apply RBAC (Role-Based Access Control) role checks for each tenant"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-006-004 — SECURITY+AUTHORIZATION: credential + permission

A: "validate client credentials and check permission grants before allowing API access"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-006-005 — SECURITY+AUTHORIZATION: authentication + authorization

A: "the authentication layer must enforce authorization scopes on every tenant API call"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
