# Layer 0-B — Router Pairs (033)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-033-001 — BillingCycleService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Scaffold BillingCycleService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-033-002 — BillingCycleService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Scaffold BillingCycleService + billing cycle domain model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-033-003 — BillingCycleService: IMPLEMENT+TEST routing

A: Predicates: ["IMPLEMENT", "TEST"] from "Scaffold BillingCycleService + write tests."
B: Routes: ImplementationFlow41 (4.1) → TestingFlow51 (5.1) | DecisionCount: 2

---

## B15-router-033-004 — BillingCycleService: IMPLEMENT+DOCUMENT routing

A: Predicates: ["IMPLEMENT", "DOCUMENT"] from "Scaffold BillingCycleService + spec out rules."
B: Routes: ImplementationFlow41 (4.1) ‖ DocumentationFlow21 (2.1) | DecisionCount: 2

---

## B15-router-033-005 — refactoring TransitionType in full pipeline

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring from "Refactor + extend BillingCycleService."
B: Route: ImplementationFlow41 (4.1) | TransitionType: refactoring | DecisionCount: 1
Teaching: TransitionType: refactoring passed through unchanged. "extend" also fires IMPLEMENT but does not override TransitionType.
