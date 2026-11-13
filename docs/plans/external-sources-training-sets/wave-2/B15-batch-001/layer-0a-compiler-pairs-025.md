# Layer 0-A — Compiler Pairs (025)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-025-001 — PaymentGatewayService: IMPLEMENT alone

A: "Build a PaymentGatewayService that processes credit card charges."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"build" → IMPLEMENT fires. "payment" is a domain noun, not a predicate trigger. "charges" is a domain noun, not a trigger.

---

## B15-compiler-025-002 — PaymentGatewayService: IMPLEMENT+API_CONTRACT

A: "Build a PaymentGatewayService and integrate with the Stripe API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "integrate" → API_CONTRACT. Two predicates, two decisions.

---

## B15-compiler-025-003 — PaymentGatewayService: IMPLEMENT+SECURITY

A: "Build a PaymentGatewayService and secure the payment endpoints."
B: Predicates: ["IMPLEMENT", "SECURITY"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "secure" → SECURITY. "payment endpoints" is the target, not the trigger.

---

## B15-compiler-025-004 — PaymentGatewayService: IMPLEMENT+ARCHITECTURE

A: "Build a PaymentGatewayService and architect the payment domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "architect" → ARCHITECTURE (verb trigger). "domain model" also → ARCHITECTURE (deduplicated to one ARCHITECTURE predicate).

---

## B15-compiler-025-005 — Deduplication: two IMPLEMENT triggers

A: "Build and implement the PaymentGatewayService."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"build" → IMPLEMENT. "implement" → IMPLEMENT. Two IMPLEMENT triggers → deduplicated to one IMPLEMENT predicate.
