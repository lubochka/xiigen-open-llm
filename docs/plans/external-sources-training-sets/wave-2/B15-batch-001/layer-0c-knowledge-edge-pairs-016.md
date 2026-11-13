# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-016-001 — "product categories model" fires ARCHITECTURE

A: "Build the product catalog service and design the domain model for product categories and variants."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "domain model" → ARCHITECTURE fires. "product categories" is the domain noun — "model" is the keyword that triggers ARCHITECTURE.

---

## B15-kedge-016-002 — "catalog architecture" fires ARCHITECTURE

A: "Design the catalog architecture and implement the product listing service."
B:
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Edge analysis: "architecture" → ARCHITECTURE fires on the word "architecture" alone. "design" reinforces the architectural intent but "architecture" is the primary trigger word.

---

## B15-kedge-016-003 — "listing, filtering, and detail retrieval" does not fire additional predicates

A: "Build the product catalog service with listing, filtering, and detail retrieval."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "listing, filtering, detail retrieval" — feature descriptions within IMPLEMENT scope. None of these are predicate-firing terms. Only "build" fires, producing a single IMPLEMENT predicate.

---

## B15-kedge-016-004 — "API contracts" fires API_CONTRACT

A: "Define the product API contracts for catalog listing and product detail endpoints."
B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Edge analysis: "API contracts" → API_CONTRACT fires on "contract" (with "API" as the modifier confirming context). "define" also fires DOCUMENT — two predicates fire: ["API_CONTRACT", "DOCUMENT"].
  Correction: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"] (define fires DOCUMENT as well).

---

## B15-kedge-016-005 — "catalog variants" vs "variant model": ARCHITECTURE fires only on model/architecture noun

A: "Build the catalog service for product variants."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "product variants" — domain noun; "catalog service" → IMPLEMENT scope. ARCHITECTURE does NOT fire for "variants" alone. Only "variant model" or "variant domain model" would fire ARCHITECTURE because "model" is required.
