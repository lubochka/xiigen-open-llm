# Layer 0-A — Compiler Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-002-001 — SECURITY: "oauth"

A: "set up oauth for the backend services"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition

---

## O1b-0A-002-002 — SECURITY: "token"

A: "the token validation is failing after 1 hour"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition

---

## O1b-0A-002-003 — SECURITY: "credential"

A: "rotate the service credential before the quarterly audit"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition

---

## O1b-0A-002-004 — SECURITY: "certificate"

A: "install a new TLS (Transport Layer Security) certificate on the auth server"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition

---

## O1b-0A-002-005 — SECURITY: "encrypt"

A: "encrypt the client secrets at rest using AES-256 (Advanced Encryption Standard 256-bit)"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition
