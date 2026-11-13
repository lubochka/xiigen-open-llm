# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-037-001 — CustomerSegmentation: gap verbs

```
source_id: xiigen-b06-0c-037-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-037-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for CustomerSegmentation:
    "Assign customers to cohorts based on purchase frequency." → KNOWLEDGE_EDGE (assign = operational)
    "Recalculate RFM scores every night." → KNOWLEDGE_EDGE (recalculate = operational)
    "Group customers by lifetime value." → KNOWLEDGE_EDGE (group = operational)
    "Refresh segment assignments after each purchase." → KNOWLEDGE_EDGE (refresh = operational)
  Teaching: "customer", "segment", "cohort", "RFM", "lifetime value" are domain nouns — none fire predicates.

---

## B06-0C-037-002 — CustomerSegmentation: boundary cases

```
source_id: xiigen-b06-0c-037-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-037-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for CustomerSegmentation:
    "Define customer segments." → NOT knowledge-edge (define fires DOCUMENT)
    "Assign customer segments according to the requirements." → NOT knowledge-edge (requirements fires DOCUMENT)
    "Build a segmentation service." → NOT knowledge-edge (build fires IMPLEMENT)
    "Assign customers to segments." → KNOWLEDGE_EDGE (no predicate term present)
  Teaching: A single DOCUMENT or IMPLEMENT term prevents knowledge-edge classification.

---

## B06-0C-037-003 — CustomerSegmentation: partial predicate wins

```
source_id: xiigen-b06-0c-037-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-037-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Partial predicate examples:
    "Track business segmentation rules." → DOCUMENT fires on "business" (Rule 7) → NOT knowledge-edge
    "Assign customers based on purchase history epics." → DOCUMENT fires on "epics" (Rule 15) → NOT knowledge-edge
    "Develop customer segment capability." → DOCUMENT fires on "capability" (Rule 16), IMPLEMENT fires on "Develop" → NOT knowledge-edge
  Teaching: predicate fires regardless of the operational verb surrounding it.
