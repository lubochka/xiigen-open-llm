# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E008-001 — "sanity check" is gap

A: "Do a sanity check on the OrderService before going live."
  "sanity check" — gap (informal QA phrase); "going live" — gap (release vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## E008-002 — "double-check" is gap

A: "Double-check the discount logic in the PricingService."
  "double-check" — gap; "discount logic" — domain
  CompiledPredicateIds: [] → knowledge_edge

---

## E008-003 — "test" as a verb in a non-unit-testing command fires TEST

A: "Test the performance of the SearchService under high load."
  "test" → TEST; "performance" — gap; "high load" — gap
  CompiledPredicateIds: ["TEST"]
  Routing: TEST → 5.1 (not a perf/load sub-phase distinction; TEST always routes to 5.1 unless E2E vocabulary present)

---

## E008-004 — "expected behaviour" is gap

A: "Define the expected behaviour of the CartService when a product goes out of stock mid-checkout."
  "define" → ARCHITECTURE; "expected behaviour" — gap (not TEST without a test action verb)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "expected behaviour" alone does not fire TEST. "define" fires ARCHITECTURE.

---

## E008-005 — "testing" (gerund) fires TEST

A: "Testing the EmailService retry logic is blocked by the missing SMTP mock."
  "testing" → TEST fires on "testing" (gerund form of test)
  "SMTP mock" — gap (infrastructure term); "blocked" — state description; no predicate
  CompiledPredicateIds: ["TEST"]
  Teaching: "testing" as a gerund/verb form fires TEST just as "test" does.
