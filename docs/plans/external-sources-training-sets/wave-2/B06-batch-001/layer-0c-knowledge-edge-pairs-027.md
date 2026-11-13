# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-027-001 — FlashSaleEngine: gap verbs

```
source_id: xiigen-b06-0c-027-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-027-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_flash_sale_domain:
      "run a flash sale for two hours" → KNOWLEDGE_EDGE (run = operational)
      "activate flash sale discounts" → KNOWLEDGE_EDGE (activate = operational)
      "reserve inventory for flash sale" → KNOWLEDGE_EDGE (reserve = operational)
      "expire flash sale at end of window" → KNOWLEDGE_EDGE (expire = operational)
      "throttle concurrent purchases during flash sale" → KNOWLEDGE_EDGE (throttle = operational)
      "roll back pricing after sale ends" → KNOWLEDGE_EDGE (roll back = operational)

---

## B06-0C-027-002 — FlashSaleEngine: partial predicate wins

```
source_id: xiigen-b06-0c-027-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-027-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  partial_predicate_cases:
    "Activate the flash sale requirements" → DOCUMENT fires on "requirements"
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    "Run the functional flash sale setup" → DOCUMENT fires on "functional"
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: partial predicate fires even when primary intent is operational.
    The lexicon scan finds DOCUMENT terms regardless of surrounding operational verbs.

---

## B06-0C-027-003 — FlashSaleEngine: gap domain noun reference

```
source_id: xiigen-b06-0c-027-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-027-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_nouns_flash_sale: ["flash sale", "countdown timer", "sale window", "inventory hold",
    "discount tier", "sale price", "rollback", "activation window", "surge protection"]
  Teaching: These are business domain nouns. None fire any predicate.
    A sentence containing ONLY these words results in KNOWLEDGE_EDGE.
    Example: "Flash sale inventory hold surge protection" → KNOWLEDGE_EDGE.
