# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-013-001 — OrderFulfillmentService: "fulfillment" is not a predicate keyword

A: "Build the order fulfillment service that dispatches orders to carriers."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "fulfillment" — not in any predicate lexicon; "build" fires IMPLEMENT; "service" reinforces IMPLEMENT; "dispatches" — action verb, not predicate trigger.
  Knowledge edge: No secondary predicate fires from domain nouns.

---

## B15-kedge-013-002 — "integrate carrier API" always fires API_CONTRACT

A: "Build the fulfillment service and integrate with the carrier dispatch API."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "integrate" → API_CONTRACT fires. "carrier dispatch API" confirms API context. API_CONTRACT fires regardless of whether the word "contract" appears — "integrate" alone is sufficient.

---

## B15-kedge-013-003 — "packing model" fires ARCHITECTURE — "model" is the keyword

A: "Implement the fulfillment service and define the packing model for multi-item orders."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "model" in "packing model" → ARCHITECTURE fires. The preceding noun ("packing") is irrelevant. Any "X model" where X is a domain noun triggers ARCHITECTURE on the word "model" alone.

---

## B15-kedge-013-004 — "warehouse routing logic" does not fire ARCHITECTURE

A: "Implement the warehouse routing logic for the fulfillment service."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "warehouse routing logic" — "routing" is not in ARCHITECTURE lexicon in this context; "logic" is in IMPLEMENT context. No ARCHITECTURE fires because "routing logic" is a code construct noun, not a domain model noun. Only "domain model" / "bounded context" / "architecture" / "architect" fire ARCHITECTURE.

---

## B15-kedge-013-005 — "harden" fires SECURITY even without "secure"

A: "Build the fulfillment service and harden the packing slip generation against injection attacks."
B:
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Edge analysis: "harden" → SECURITY fires. SECURITY trigger words: secure, harden, protect, encrypt, authenticate. Any of these alone is sufficient — "secure" is not required.
