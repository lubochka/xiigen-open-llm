# Layer 0-A — Compiler Pairs (002)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-002-001 — "build" backend service → IMPLEMENT

A: "Build the customer account service with registration, login, and profile management."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; customer account service → backend service noun.

---

## B15-compiler-002-002 — "implement" business logic → IMPLEMENT

A: "Implement the discount pricing engine with tiered rate logic."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; discount pricing engine → business logic component.

---

## B15-compiler-002-003 — "implement" + "document" → IMPLEMENT+DOCUMENT

A: "Implement the inventory tracking service and document the API endpoints."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; document the API endpoints → DOCUMENT.

---

## B15-compiler-002-004 — "contract" in request → API_CONTRACT fires too

A: "Implement the order service and define the data contracts for the order schema."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "data contracts" → API_CONTRACT ("contract" modifier doesn't matter).

---

## B15-compiler-002-005 — "model" fires ARCHITECTURE

A: "Build the fulfillment service and design the domain model for order state."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "design the domain model" → ARCHITECTURE ("model" in domain model fires ARCHITECTURE).
