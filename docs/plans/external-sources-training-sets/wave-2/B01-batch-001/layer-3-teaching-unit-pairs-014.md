# Layer 3 — Teaching Unit Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-014-001 — PairParserUnit: files 014 expansion

```
source_id: xiigen-b01-l3-014-001
pair_kind: teaching_unit
```

A:
  BatchId: "B01-batch-001"
  Files: ["layer-0a-compiler-pairs-014.md", ..., "layer-2-nine-node-1.1-pairs-014.md"]

B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "NotificationCenter/AlertBanner UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "NotificationCenter routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "wireframe IS UI; onboarding flow is gap; prototype IS UI in B01"
    - Layer: "0-D" | PairCount: 3 | Theme: "probationary_accepted, deterministic_generic, trusted_learned"
    - Layer: "1" | PairCount: 4 | Theme: "SearchResults, CartView, WishlistPage, OrderHistory"
    - Layer: "2" | PairCount: 9 | Theme: "NotificationCenterScreens UI+DOCUMENT"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 014"
  TotalPairCount: 31
  ChainIntegrityCheck: "passed"

---

## B01-L3-014-002 — ModelTrainerUnit: files 014

```
source_id: xiigen-b01-l3-014-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 014]
B:
  Metrics:
    total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
    knowledge_edge_pairs: "3" | chain_integrity: "passed" | min_confidence: "0.88"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-014-003 — Teaching summary: file 014 key lessons

```
source_id: xiigen-b01-l3-014-003
pair_kind: teaching_unit
```

Key lessons from file 014:
  1. "wireframe" IS in the UI lexicon — always fires UI
  2. "prototype" IS in the UI lexicon for B01 (contrast with B32 where it is gap)
  3. "onboarding flow" — "flow" is NOT in the UI lexicon → gap → knowledge-edge
  4. UI+DOCUMENT dual predicate → two independent RoutingDecision entries
  5. NotificationCenter nine-node: UI+DOCUMENT → DomainSplitter produces 2 DomainPackets
