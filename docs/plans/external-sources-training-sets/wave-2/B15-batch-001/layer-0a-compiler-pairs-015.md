# Layer 0-A — Compiler Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-015-001 — InventoryAllocationService: "create" → IMPLEMENT

A: "Create the inventory allocation service that reserves stock when an order is placed and releases it on cancellation."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: create → IMPLEMENT; inventory allocation service → backend service noun; reserves/releases → domain action verbs.

---

## B15-compiler-015-002 — InventoryAllocationService: "implement" + "data contracts" → IMPLEMENT+API_CONTRACT

A: "Implement the inventory allocation service and define the data contracts for stock reservation requests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "data contracts" → API_CONTRACT ("contract" keyword fires API_CONTRACT regardless of modifier).

---

## B15-compiler-015-003 — InventoryAllocationService: "bounded context" → IMPLEMENT+ARCHITECTURE

A: "Build the inventory allocation service within the inventory bounded context and implement reservation logic."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "bounded context" → ARCHITECTURE (fires ARCHITECTURE same as "domain model").

---

## B15-compiler-015-004 — v6-02 corruption: "bounded context" not caught

v6 corrupt: "Build the inventory allocation service within the inventory bounded context" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Error type: v6-02. "bounded context" fires ARCHITECTURE — same rule as "domain model". The corrupt model only recognized "build" and ignored the architectural term.

---

## B15-compiler-015-005 — InventoryAllocationService: "secure" → IMPLEMENT+SECURITY

A: "Create the inventory allocation service and harden it against race conditions in concurrent reservation requests."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: create → IMPLEMENT; harden → SECURITY (harden/secure/protect fires SECURITY).
