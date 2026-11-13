# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-039-001 — GeofencedPricing: gap verbs

```
source_id: xiigen-b06-0c-039-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-039-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for GeofencedPricing:
    "Apply the correct price for each customer based on their detected region." → KNOWLEDGE_EDGE (apply = operational)
    "Detect the customer's country from their IP." → KNOWLEDGE_EDGE (detect = operational)
    "Lookup the regional price table." → KNOWLEDGE_EDGE (lookup = operational)
    "Override the default price with the region-specific price." → KNOWLEDGE_EDGE (override = operational)
  Teaching: "geofenced pricing", "region", "country", "IP", "price override" are domain nouns — none fire predicates.

---

## B06-0C-039-002 — GeofencedPricing: boundary cases

```
source_id: xiigen-b06-0c-039-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-039-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for GeofencedPricing:
    "Define the geofenced pricing requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Apply regional pricing per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the geolocation resolver." → NOT knowledge-edge (create fires IMPLEMENT)
    "Apply regional pricing to all customers." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-039-003 — GeofencedPricing: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-039-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-039-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for GeofencedPricing:
    gap: "apply/detect/lookup/override/resolve regional price"
    spec: "define/describe/specify/document geofenced pricing requirements"
  Teaching: "geolocation" and "region" are domain nouns — they never fire predicates without a predicate-lexicon term.
