# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-023-001 — "jurisdiction-specific tax rates" does not fire additional predicates

A: "Implement the tax calculation service that applies jurisdiction-specific tax rates to order line items."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "jurisdiction-specific tax rates" — domain noun phrase; "applies" — action verb in IMPLEMENT scope. Single IMPLEMENT fires on "implement".

---

## B15-kedge-023-002 — "domain model for tax jurisdiction" fires ARCHITECTURE

A: "Build the tax service and design the domain model for tax jurisdiction rules and exemption categories."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "domain model" → ARCHITECTURE fires. "tax jurisdiction rules" and "exemption categories" are domain nouns following "model" — they do not independently trigger ARCHITECTURE.

---

## B15-kedge-023-003 — "architect" verb fires ARCHITECTURE

A: "Architect the tax calculation engine and implement the rate lookup and exemption logic."
B:
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Edge analysis: "architect" (verb) → ARCHITECTURE fires. "implement" → IMPLEMENT fires. "architect" as a verb is a first-class ARCHITECTURE trigger — equivalent to "design the architecture of".

---

## B15-kedge-023-004 — "tax model" fires ARCHITECTURE — "model" is the keyword regardless of preceding noun

A: "Build the tax service and define the tax model for multi-jurisdiction compliance."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "tax model" → ARCHITECTURE fires on "model". The preceding noun "tax" is irrelevant. Any "[X] model" phrase fires ARCHITECTURE when X is a domain concept.

---

## B15-kedge-023-005 — "exemption categories" alone does not fire ARCHITECTURE

A: "Implement the tax exemption category lookup service."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "exemption categories" — domain noun; "lookup service" → IMPLEMENT scope. No "model" or "architecture" keyword present → ARCHITECTURE does not fire. Key distinction: "category model" would fire ARCHITECTURE; "categories" alone does not.
