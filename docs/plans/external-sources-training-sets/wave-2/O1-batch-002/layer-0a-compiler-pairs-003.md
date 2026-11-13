# Layer 0-A — Compiler Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-003-001 — AUTHORIZATION: "scope"

A: "define the scopes for the tenant billing API"

B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-003-002 — AUTHORIZATION: "tenant"

A: "add tenant-level isolation to the permission model"

B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-003-003 — AUTHORIZATION: "grant"

A: "grant the analytics service account read access to the reporting API"

B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-003-004 — AUTHORIZATION: "revoke"

A: "revoke all active tokens for deprovisioned service accounts"

B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-0A-003-005 — AUTHORIZATION: "principal"

A: "register a new principal in the ACL (Access Control List) for the data pipeline"

B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition
