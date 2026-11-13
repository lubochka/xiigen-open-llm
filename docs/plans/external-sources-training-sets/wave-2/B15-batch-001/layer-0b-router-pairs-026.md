# Layer 0-B — Router Pairs (026)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-026-001 — SubscriptionService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Create a SubscriptionService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-026-002 — SubscriptionService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Create SubscriptionService + domain model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | Ordering: ARCHITECTURE first | DecisionCount: 2

---

## B15-router-026-003 — SubscriptionService: IMPLEMENT+TEST routing

A: Predicates: ["IMPLEMENT", "TEST"] from "Create SubscriptionService + write unit tests."
B: Routes: ImplementationFlow41 (4.1) → TestingFlow51 (5.1) | Ordering: implement before test | DecisionCount: 2

---

## B15-router-026-004 — SubscriptionService: four-predicate route ordering

A: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "TEST"].
B: Routes: ArchitectureFlow31 (3.1) → APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) → TestingFlow51 (5.1) | DecisionCount: 4

---

## B15-router-026-005 — Router: refactoring TransitionType passes through

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring (from "Refactor SubscriptionService").
B: Route: ImplementationFlow41 (4.1) | TransitionType: refactoring propagated to flow | DecisionCount: 1
Router does not reset TransitionType. refactoring metadata passes to ImplementationFlow41 as-is.
