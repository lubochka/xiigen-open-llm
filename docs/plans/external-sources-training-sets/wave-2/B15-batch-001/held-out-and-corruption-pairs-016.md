# Held-Out and Corruption Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-016-A — Held-out: ProductCatalogService four predicates

A: "Design the catalog domain model, define the product API contracts, build the catalog service, and document the product endpoints."
B: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"] | feature_addition | DecisionCount: 4

## HC-016-B — Held-out: "catalog architecture" fires ARCHITECTURE

A: "Design the catalog architecture and implement the product listing service."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | feature_addition | "architecture" keyword fires ARCHITECTURE

## HC-016-C — Corruption v6-02: "catalog architecture" not caught

v6 corrupt: "Design the catalog architecture and implement the product listing service" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | "architecture" fires ARCHITECTURE.
Error: v6-02. "architecture" is an explicit keyword — not suppressible by a preceding domain noun ("catalog").

## HC-016-D — Corruption v6-02: "product categories model" not caught

v6 corrupt: "Build the catalog service and design the domain model for product categories" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | "domain model" fires ARCHITECTURE.
Error: v6-02.

## HC-016-E — Held-out: "product variants" alone does not fire ARCHITECTURE

A: "Build the catalog service for product variants and bundles."
B: Predicates: ["IMPLEMENT"] | "variants" and "bundles" are domain nouns; no "model" or "architecture" keyword present → ARCHITECTURE does not fire.
