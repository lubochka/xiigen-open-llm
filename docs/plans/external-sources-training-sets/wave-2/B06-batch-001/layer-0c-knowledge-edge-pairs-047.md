# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## Method note

File 047 is the gap summary for files 037-048 (Round 3 domains), consolidating all gap patterns.

---

## B06-0C-047-001 — Gap summary: files 037-048

```
source_id: xiigen-b06-0c-047-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-047-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    b06_gap_summary_037_to_048:
      "Gap pattern: operational execution verbs":
        verbs: ["assign", "recalculate", "group", "refresh", "adjust", "monitor", "apply",
                "upgrade", "downgrade", "check", "resolve", "waive", "block", "validate",
                "show", "reject", "queue", "notify", "fulfill", "charge", "authorize",
                "cancel", "schedule", "detect", "lookup", "override", "aggregate"]
        result: "KNOWLEDGE_EDGE (no predicate fires)"
      "Gap pattern: domain nouns alone":
        nouns: ["customer segmentation", "RFM", "cohort", "segment", "dynamic pricing",
                "demand signals", "competitor feed", "geofenced pricing", "region",
                "volume discount", "quantity break", "SKU eligibility", "membership tier",
                "Bronze", "Silver", "Gold", "Platinum", "coupon stacking", "free shipping",
                "minimum order", "backorder", "restock", "preorder", "deposit", "release date"]
        result: "KNOWLEDGE_EDGE (nouns never fire predicates alone)"
      "Boundary: partial predicate wins":
        rule: "If ANY predicate term is present, result is NOT knowledge-edge"
        examples:
          - "'Track business segmentation rules' → DOCUMENT fires on 'business' (Rule 7)"
          - "'Assign customers per the requirements' → DOCUMENT fires on 'requirements'"
          - "'Recalculate pricing specification' → DOCUMENT fires on 'specification'"
          - "'Develop segmentation capability' → IMPLEMENT fires on 'develop', DOCUMENT fires on 'capability'"

---

## B06-0C-047-002 — Gap vs spec vocabulary: Round 3 complete reference

```
source_id: xiigen-b06-0c-047-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-047-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  complete_gap_vs_spec_reference_037_to_048:
    CustomerSegmentation: "assign/group/refresh segments" → gap; "define/specify requirements" → spec
    DynamicPricing: "recalculate/adjust/apply/monitor prices" → gap; "describe/specify requirements" → spec
    GeofencedPricing: "detect/lookup/apply/override region price" → gap; "define/document requirements" → spec
    VolumeDiscount: "calculate/apply/check/aggregate discount" → gap; "describe/specify requirements" → spec
    MembershipTiers: "upgrade/downgrade/assign tier" → gap; "define/describe requirements" → spec
    CouponStackingRules: "apply/check/resolve/calculate coupons" → gap; "define/document stacking rules" → spec
    FreeShippingThreshold: "waive/check/remove/notify shipping" → gap; "define/specify requirements" → spec
    MinimumOrderValue: "block/validate/reject order" → gap; "describe/define requirements" → spec
    BackorderManagement: "queue/notify/fulfill backorder" → gap; "define/specify requirements" → spec
    PreorderRules: "charge/authorize/cancel/schedule preorder" → gap; "define/describe requirements" → spec

---

## B06-0C-047-003 — Teaching consolidation: knowledge-edge cases in files 037-048

```
source_id: xiigen-b06-0c-047-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-047-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    total_gap_cases_037_to_048: 40+
    gap_domains_covered: 10 (Round 3)
    gap_verb_types_covered: ["calculation", "assignment", "monitoring", "application", "enforcement",
      "scheduling", "authorization", "notification", "resolution", "aggregation"]
    spec_predicates_confirmed: ["DOCUMENT alone", "DOCUMENT+IMPLEMENT", "DOCUMENT+ARCHITECTURE",
      "ARCHITECTURE alone", "DOCUMENT+ARCHITECTURE+IMPLEMENT"]
    boundary_cases:
      "partial predicate wins": confirmed across all 10 Round 3 domains
      "domain noun alone = gap": confirmed for all Round 3 vocabulary
    teaching_note: "Knowledge-edge classification requires the ENTIRE request to contain
      zero predicate-lexicon terms. A single lexicon term from any predicate type prevents knowledge-edge."
