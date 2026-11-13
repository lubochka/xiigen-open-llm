# Layer 0-A — Compiler Pairs (035)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-035-001 — ChargebackService: IMPLEMENT alone

A: "Implement a ChargebackService that handles disputed payment reversals."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"implement" → IMPLEMENT. "chargeback" is a domain noun — does NOT suppress IMPLEMENT (v6-01 corruption risk).

---

## B15-compiler-035-002 — ChargebackService: IMPLEMENT+SECURITY

A: "Implement a ChargebackService and authenticate all dispute resolution requests."
B: Predicates: ["IMPLEMENT", "SECURITY"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "authenticate" → SECURITY.

---

## B15-compiler-035-003 — ChargebackService: IMPLEMENT+API_CONTRACT

A: "Implement a ChargebackService and wire up the payment processor dispute API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "wire up" → API_CONTRACT.

---

## B15-compiler-035-004 — ChargebackService: IMPLEMENT+ARCHITECTURE+API_CONTRACT

A: "Implement a ChargebackService, architect the dispute resolution domain model, and define the chargeback API contracts."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT"] | feature_addition | DecisionCount: 3
"implement" → IMPLEMENT. "architect" + "domain model" → ARCHITECTURE (deduplicated). "API contracts" → API_CONTRACT. Three predicates.

---

## B15-compiler-035-005 — v6-01 risk: "chargeback" must not suppress IMPLEMENT

A: "Process chargeback disputes for the payment service."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"process" → IMPLEMENT (process = code generation verb). "chargeback" is a domain noun, NOT a predicate. v6-01 corruption: treating "chargeback" as an unrecognized predicate and returning [].
