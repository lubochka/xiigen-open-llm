# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-014-001 — "chargeback" is a domain noun, not a gap trigger

A: "Implement the refund and chargeback service."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "chargeback" — domain term; "implement" fires IMPLEMENT unambiguously. Domain nouns never suppress predicate detection. CompiledPredicateIds: ["IMPLEMENT"], not [].

---

## B15-kedge-014-002 — "payment reversal" does not fire API_CONTRACT alone

A: "Implement the refund service with payment reversal logic."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "payment reversal" — domain phrase; "logic" → IMPLEMENT context. No API_CONTRACT fires because there is no API integration verb (integrate/wire up/connect) and no "contract" keyword. Payment reversal is an internal business rule, not an API integration term.

---

## B15-kedge-014-003 — "wire up payment gateway" fires API_CONTRACT

A: "Implement the refund service and wire up the payment gateway reversal API."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "wire up" → API_CONTRACT fires. "payment gateway reversal API" — "API" noun confirms external integration. Two predicates fire.

---

## B15-kedge-014-004 — "chargeback states model" fires ARCHITECTURE

A: "Build the chargeback handling service and define the domain model for chargeback states."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "domain model" → ARCHITECTURE fires. The phrase "chargeback states model" or "domain model for chargeback states" — both trigger ARCHITECTURE on "model".

---

## B15-kedge-014-005 — partial refund vs full refund: no predicate difference

A: "Implement partial refund and full refund logic."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "partial refund" and "full refund" — domain qualifiers; "logic" → IMPLEMENT context. No additional predicates fire from refund amount qualifiers.
