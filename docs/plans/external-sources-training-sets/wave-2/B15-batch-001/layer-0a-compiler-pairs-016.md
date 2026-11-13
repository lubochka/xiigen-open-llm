# Layer 0-A — Compiler Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-016-001 — ProductCatalogService: "build" → IMPLEMENT

A: "Build the product catalog service with endpoints for listing, searching, and retrieving product details."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; product catalog service → backend service noun.

---

## B15-compiler-016-002 — ProductCatalogService: "domain model" → IMPLEMENT+ARCHITECTURE

A: "Build the product catalog service and design the domain model for product categories and variants."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "domain model" → ARCHITECTURE fires. Even though the primary verb is "build" (IMPLEMENT), "domain model" independently fires ARCHITECTURE.

---

## B15-compiler-016-003 — ProductCatalogService: "architecture" keyword → IMPLEMENT+ARCHITECTURE

A: "Design the catalog architecture and implement the product listing, filtering, and detail retrieval services."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: design + architecture → ARCHITECTURE; implement → IMPLEMENT. Order in output: ARCHITECTURE listed before IMPLEMENT (fired first in request).

---

## B15-compiler-016-004 — v6-02 corruption: "catalog architecture" → ARCHITECTURE suppressed

v6 corrupt: "Design the catalog architecture and implement the product listing service" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Error type: v6-02. "architecture" is an explicit ARCHITECTURE keyword. The corrupt model treated "catalog architecture" as a domain phrase and suppressed ARCHITECTURE.

---

## B15-compiler-016-005 — ProductCatalogService: four predicates fire

A: "Design the catalog domain model, define the product API contracts, build the catalog service, and document the product endpoints."
B: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: "domain model" → ARCHITECTURE; "API contracts" → API_CONTRACT; build → IMPLEMENT; document → DOCUMENT. Four independent predicates fire.
