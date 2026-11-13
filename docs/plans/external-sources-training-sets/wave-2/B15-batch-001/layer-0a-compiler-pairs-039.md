# Layer 0-A — Compiler Pairs (039)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-039-001 — KYCService (Know Your Customer): IMPLEMENT from "create"

A: "Create a KYCService to verify customer identity documents."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"create" → IMPLEMENT. "KYCService" is a domain noun — does not fire any predicate.

---

## B15-compiler-039-002 — KYCService: IMPLEMENT+SECURITY on "secure"

A: "Create KYCService and secure the identity verification endpoints."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"create" → IMPLEMENT. "secure" → SECURITY.

---

## B15-compiler-039-003 — KYCService: IMPLEMENT+API_CONTRACT on "wire up"

A: "Create KYCService and wire up the third-party identity verification API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"create" → IMPLEMENT. "wire up" → API_CONTRACT.

---

## B15-compiler-039-004 — KYCService: IMPLEMENT+ARCHITECTURE on "KYC domain model"

A: "Create KYCService with a KYC domain model including identity state machine."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"create" → IMPLEMENT. "KYC domain model" → ARCHITECTURE ("domain model" pattern fires).

---

## B15-compiler-039-005 — KYCService: IMPLEMENT+SECURITY+TEST on multi-trigger

A: "Create KYCService, harden the document upload endpoint, and write tests for all identity checks."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition
"create" → IMPLEMENT. "harden" → SECURITY. "write tests" → TEST.
