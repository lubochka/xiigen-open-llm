# Layer 0-A — Compiler Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-013-001 — OrderFulfillmentService: "build" + "fulfillment" → IMPLEMENT

A: "Build the order fulfillment service that picks, packs, and dispatches orders to the carrier."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; fulfillment service → backend service noun; picks/packs/dispatches → action verbs within IMPLEMENT scope.

---

## B15-compiler-013-002 — OrderFulfillmentService: "implement" + "integrate carrier" → IMPLEMENT+API_CONTRACT

A: "Implement the order fulfillment service and integrate with the carrier dispatch API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; integrate → API_CONTRACT (integrate/wire up/connect fires API_CONTRACT).

---

## B15-compiler-013-003 — OrderFulfillmentService: "design model" → IMPLEMENT+ARCHITECTURE

A: "Build the order fulfillment service and design the domain model for the packing workflow."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "domain model" → ARCHITECTURE ("model" fires ARCHITECTURE even in IMPLEMENT request).

---

## B15-compiler-013-004 — v6-02 corruption: "fulfillment model" → ARCHITECTURE suppressed

v6 corrupt: "Build the fulfillment service and model the packing workflow" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Error type: v6-02. "model" in "model the packing workflow" fires ARCHITECTURE. The corrupt model read "model" as a generic verb and suppressed ARCHITECTURE.

---

## B15-compiler-013-005 — OrderFulfillmentService: complete predicate reference for fulfillment domain

Key firing patterns for fulfillment requests:
- build/implement/create/extend → IMPLEMENT (always)
- "carrier API" / "dispatch API" / "integrate" → API_CONTRACT
- "domain model" / "packing model" / "fulfillment model" → ARCHITECTURE
- secure/harden packing slip → SECURITY
- write tests for fulfillment → TEST
- document fulfillment API → DOCUMENT
