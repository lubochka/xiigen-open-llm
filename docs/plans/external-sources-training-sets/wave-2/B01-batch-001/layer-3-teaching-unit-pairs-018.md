# Layer 3 — Teaching Unit Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-018-001 — PairParserUnit: files 018

```
source_id: xiigen-b01-l3-018-001
pair_kind: teaching_unit
```

A: Theme: "WishlistPage and SavedItems expansion"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "WishlistPage and SavedItems UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "WishlistPage and SavedItems routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "visual IS UI; wishlist sharing → gap; accessibility IS UI"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_accepted"
    - Layer: "1" | PairCount: 4 | Theme: "create profile, describe modal (UI+DOCUMENT), build delete modal, create responsive grid"
    - Layer: "2" | PairCount: 9 | Theme: "WishlistAndSavedItems UI alone"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 018"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-018-002 — ModelTrainerUnit: files 018

```
source_id: xiigen-b01-l3-018-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 018]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-018-003 — Teaching summary: file 018 key lessons

```
source_id: xiigen-b01-l3-018-003
pair_kind: teaching_unit
```

Key lessons from file 018:
  1. "visual" IS in the UI lexicon — fires UI for any visual hierarchy/design context
  2. "wishlist sharing" — "share", "link", "unique" are not predicate terms → gap
  3. "accessibility" IS in the UI lexicon — always fires UI
  4. UI+DOCUMENT (describe + modal) → feature_addition (UI drives, no IMPLEMENT needed)
  5. WishlistPage nine-node: 3 screens (populated, empty, move-to-cart modal); 4 components
