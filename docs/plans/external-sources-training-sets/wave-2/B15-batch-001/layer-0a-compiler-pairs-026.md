# Layer 0-A — Compiler Pairs (026)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-026-001 — SubscriptionService: IMPLEMENT alone

A: "Create a SubscriptionService that manages recurring billing plans."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"create" → IMPLEMENT. "recurring billing" is domain context, not a predicate trigger.

---

## B15-compiler-026-002 — SubscriptionService: IMPLEMENT+ARCHITECTURE

A: "Create a SubscriptionService and define the subscription domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"create" → IMPLEMENT. "domain model" → ARCHITECTURE. DecisionCount: 2.

---

## B15-compiler-026-003 — SubscriptionService: IMPLEMENT+TEST

A: "Create a SubscriptionService and write unit tests for the billing logic."
B: Predicates: ["IMPLEMENT", "TEST"] | feature_addition | DecisionCount: 2
"create" → IMPLEMENT. "write unit tests" → TEST (routes to 5.1, not 5.3).

---

## B15-compiler-026-004 — SubscriptionService: refactoring TransitionType

A: "Refactor the SubscriptionService to support tiered billing plans."
B: Predicates: ["IMPLEMENT"] | refactoring | DecisionCount: 1
"refactor" → IMPLEMENT fires. TransitionType: refactoring (overrides feature_addition).

---

## B15-compiler-026-005 — SubscriptionService: four predicates

A: "Create a SubscriptionService, define the subscription domain model, integrate with the billing provider, and write integration tests."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "TEST"] | feature_addition | DecisionCount: 4
"create" → IMPLEMENT. "domain model" → ARCHITECTURE. "integrate" → API_CONTRACT. "write integration tests" → TEST (5.1). Four independent predicates.
