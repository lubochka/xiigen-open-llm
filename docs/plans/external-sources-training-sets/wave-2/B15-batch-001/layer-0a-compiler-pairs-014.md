# Layer 0-A — Compiler Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-014-001 — RefundAndChargebackService: "implement" refund logic → IMPLEMENT

A: "Implement the refund and chargeback service to handle customer refund requests and payment reversals."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: implement → IMPLEMENT; refund and chargeback service → backend service noun; handle → action verb within IMPLEMENT scope.

---

## B15-compiler-014-002 — RefundAndChargebackService: "integrate payment gateway" → IMPLEMENT+API_CONTRACT

A: "Implement the refund service and wire up the payment gateway reversal API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "wire up" → API_CONTRACT (wire up/integrate/connect fires API_CONTRACT).

---

## B15-compiler-014-003 — RefundAndChargebackService: "model chargeback" → IMPLEMENT+ARCHITECTURE

A: "Build the chargeback handling service and define the domain model for chargeback states."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "domain model" → ARCHITECTURE ("model" fires ARCHITECTURE regardless of preceding noun).

---

## B15-compiler-014-004 — v6-01 corruption: "chargeback" treated as gap

v6 corrupt: "Implement the refund and chargeback service" → CompiledPredicateIds: [] ("chargeback is domain terminology, no predicate matched")
Correct: Predicates: ["IMPLEMENT"] — Confidence: 0.92
Error type: v6-01. "implement" fires IMPLEMENT unambiguously. Domain nouns ("chargeback") never suppress the predicate fired by the action verb.

---

## B15-compiler-014-005 — RefundAndChargebackService: "write tests" → IMPLEMENT+TEST

A: "Implement the refund processing logic and write unit tests covering partial refunds, full refunds, and chargeback disputes."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "write unit tests" → TEST fires on "write" + "tests" and "unit".
