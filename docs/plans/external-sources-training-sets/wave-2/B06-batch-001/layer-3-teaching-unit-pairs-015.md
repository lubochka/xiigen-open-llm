# Layer 3 — Teaching Unit Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 015 covers LoyaltyProgram and dual-route teaching — three pairs.

---

## B06-L3-015-001 — Teaching: "narrative" fires DOCUMENT (Rule 13)

```
source_id: xiigen-b06-l3-015-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-LOYALTY-TEACH-001"
  Content:
    rule: "Rule 13: narrative fires DOCUMENT"
    loyalty_examples:
      "document the narrative for loyalty checkout" → DOCUMENT (document + narrative fire)
      "describe the loyalty narrative" → DOCUMENT (describe + narrative fire)
    anti_pattern: "Thinking 'narrative' is informal writing — it IS a DOCUMENT predicate term"

---

## B06-L3-015-002 — Teaching: "stakeholder" → DOCUMENT + 2.4 routing

```
source_id: xiigen-b06-l3-015-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-LOYALTY-TEACH-002"
  Content:
    rule: "Rule 8: stakeholder fires DOCUMENT AND triggers 2.4 routing"
    loyalty_stakeholder_example:
      request: "Describe the stakeholder expectations for loyalty checkout"
      predicates: ["DOCUMENT"]
      routing: "2.1 AND 2.4"
      transition_count: 2
      transition_type: "documentation"
    v6_corruption: "v6-03: routing collapsed to 2.1 only (2.4 dropped). Must produce both decisions."

---

## B06-L3-015-003 — Teaching: "feature-brief" fires DOCUMENT (Rule 14)

```
source_id: xiigen-b06-l3-015-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-LOYALTY-TEACH-003"
  Content:
    rule: "Rule 14: feature-brief fires DOCUMENT (NOT 'feature' alone)"
    distinction:
      "feature-brief" → DOCUMENT fires
      "feature" alone → IMPLEMENT fires (feature is in IMPLEMENT lexicon)
    loyalty_examples:
      "specify the feature-brief for loyalty rewards" → DOCUMENT (feature-brief fires)
      "specify the loyalty feature" → DOCUMENT (specify fires) + possibly IMPLEMENT (feature fires)
    note: "feature-brief is a hyphenated compound — it fires as DOCUMENT. feature alone is IMPLEMENT."
