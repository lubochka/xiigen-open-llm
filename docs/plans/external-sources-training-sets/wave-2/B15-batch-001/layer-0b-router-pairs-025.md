# Layer 0-B — Router Pairs (025)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-025-001 — PaymentGatewayService: single routing decision

A: Predicates: ["IMPLEMENT"] from "Build a PaymentGatewayService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1 | NearestWrongSubPhase: 4.2 | NearestWrongScore: 0.40

---

## B15-router-025-002 — PaymentGatewayService: two routing decisions

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Build a PaymentGatewayService and integrate with Stripe."
B: Routes: ImplementationFlow41 (4.1) + APIContractFlow33 (3.3) | Ordering: 3.3 → 4.1 | DecisionCount: 2

---

## B15-router-025-003 — PaymentGatewayService: IMPLEMENT+SECURITY routing

A: Predicates: ["IMPLEMENT", "SECURITY"] from "Build a PaymentGatewayService and secure the payment endpoints."
B: Routes: SecurityAuditFlow + ImplementationFlow41 | Ordering: SecurityAuditFlow → 4.1 | DecisionCount: 2

---

## B15-router-025-004 — PaymentGatewayService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Build a PaymentGatewayService and architect the domain model."
B: Routes: ArchitectureFlow31 (3.1) + ImplementationFlow41 (4.1) | Ordering: 3.1 → 4.1 | DecisionCount: 2

---

## B15-router-025-005 — Router correctness: DecisionCount matches unique predicate count

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] from three-trigger request.
B: DecisionCount: 3 | Routes: SecurityAuditFlow → APIContractFlow33 → ImplementationFlow41 | SECURITY before API_CONTRACT before IMPLEMENT.
