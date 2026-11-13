# Layer 0-A — Compiler Pairs (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-003-001 — "scaffold" → IMPLEMENT

A: "Scaffold the new notification service for order status updates."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: scaffold → IMPLEMENT (scaffold is in the IMPLEMENT lexicon). Sub-phase: 4.1.

---

## B15-compiler-003-002 — "create" + service → IMPLEMENT

A: "Create the payment reconciliation service that matches charges to orders."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: create + service → IMPLEMENT (service = software component noun).

---

## B15-compiler-003-003 — IMPLEMENT+SECURITY → two predicates

A: "Build the order authentication middleware and secure the payment processing endpoint."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; secure the payment endpoint → SECURITY.

---

## B15-compiler-003-004 — "bootstrap" → IMPLEMENT

A: "Bootstrap the order microservice with health checks and telemetry."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: bootstrap → IMPLEMENT (bootstrap is in the IMPLEMENT lexicon). Health checks and telemetry = service components.

---

## B15-compiler-003-005 — IMPLEMENT+TEST → two predicates

A: "Implement the order cancellation service and write unit tests for the cancellation logic."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; write unit tests → TEST (test-writing verb + unit → 5.1).
