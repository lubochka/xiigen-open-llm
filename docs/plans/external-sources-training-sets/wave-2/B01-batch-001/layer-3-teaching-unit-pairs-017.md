# Layer 3 — Teaching Unit Pairs (017)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-017-001 — PairParserUnit: files 017

```
source_id: xiigen-b01-l3-017-001
pair_kind: teaching_unit
```

A: Theme: "CartView and MiniCart expansion"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "CartView and MiniCart UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "CartView and MiniCart routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "input IS UI for cart; cart abandonment email → gap; color IS UI"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_rejected"
    - Layer: "1" | PairCount: 4 | Theme: "color theme develop, filter panel implement, notification center develop, write wishlist"
    - Layer: "2" | PairCount: 9 | Theme: "CartAndCheckoutViews UI+IMPLEMENT"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 017"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-017-002 — ModelTrainerUnit: files 017

```
source_id: xiigen-b01-l3-017-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 017]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-017-003 — Teaching summary: file 017 key lessons

```
source_id: xiigen-b01-l3-017-003
pair_kind: teaching_unit
```

Key lessons from file 017:
  1. "input" IS in the UI lexicon — fires UI for any input field context
  2. "cart abandonment email" — no UI terms, no IMPLEMENT terms → knowledge-edge
  3. "color" IS in the UI lexicon — fires UI
  4. IMPLEMENT verb "develop" fires feature_addition regardless of other predicates
  5. CartAndCheckout nine-node: 7 screens across cart, mini-cart, checkout, post-checkout sections
