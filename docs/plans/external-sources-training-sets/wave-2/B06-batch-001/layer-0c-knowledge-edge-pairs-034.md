# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-034-001 — SeasonalPromotion: gap verbs

```
source_id: xiigen-b06-0c-034-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-034-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_seasonal_promotion_domain:
      "activate Black Friday discounts at midnight" → KNOWLEDGE_EDGE (activate = operational)
      "schedule Christmas sale campaign for December" → KNOWLEDGE_EDGE (schedule = operational)
      "expire promotional codes after campaign ends" → KNOWLEDGE_EDGE (expire = operational)
      "enforce blackout periods during regular pricing" → KNOWLEDGE_EDGE (enforce = operational)
      "coordinate multi-region campaign timing" → KNOWLEDGE_EDGE (coordinate = operational)
      "bulk upload discount rules for seasonal catalog" → KNOWLEDGE_EDGE (bulk upload = operational)

---

## B06-0C-034-002 — SeasonalPromotion: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-034-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-034-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: activate discounts" vs "spec: define discount activation requirements" → spec fires DOCUMENT
    "gap: schedule campaign" vs "spec: specify campaign scheduling behavior" → spec fires DOCUMENT
    "gap: expire promo codes" vs "spec: document expiry policy criteria" → spec fires DOCUMENT
    "gap: enforce blackout" vs "spec: describe blackout period enforcement rules" → spec fires DOCUMENT
    "gap: coordinate timing" vs "spec: define multi-region timing requirements" → spec fires DOCUMENT

---

## B06-0C-034-003 — SeasonalPromotion: complete gap vocabulary consolidation

```
source_id: xiigen-b06-0c-034-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-034-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    total_gap_cases_025_to_034: 40+
    gap_domains_covered_round_2: 10
    gap_verb_types_covered: ["operational execution", "scheduling", "activation", "enforcement",
      "calculation", "coordination", "display/show", "filtering", "routing", "assignment"]
    spec_predicates_confirmed: ["DOCUMENT alone", "DOCUMENT+IMPLEMENT", "DOCUMENT+ARCHITECTURE",
      "DOCUMENT+ARCHITECTURE+IMPLEMENT"]
    boundary_cases_confirmed:
      "partial predicate wins": confirmed across all 10 Round 2 domains
      "domain noun alone = gap": confirmed for all Round 2 domain vocabulary
      "DOCUMENT lexicon term fires regardless of surrounding gap verbs": confirmed
    teaching_note: "Knowledge-edge occurs ONLY when the entire request contains zero
      predicate-lexicon terms. A single DOCUMENT term anywhere prevents knowledge-edge."
