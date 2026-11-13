# Layer 3 — Teaching Unit Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-020-001 — PairParserUnit: files 020

```
source_id: xiigen-b01-l3-020-001
pair_kind: teaching_unit
```

A: Theme: "AddressBook and DeliveryOptions expansion"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "AddressBook and DeliveryOptions UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "AddressBook and DeliveryOptions routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "layout IS UI; address validation → IMPLEMENT; checkout flow → gap"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_accepted"
    - Layer: "1" | PairCount: 4 | Theme: "order history develop, tracking view implement, delivery ETA document, four-predicate"
    - Layer: "2" | PairCount: 9 | Theme: "AddressAndDelivery UI+IMPLEMENT"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 020"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-020-002 — ModelTrainerUnit: files 020

```
source_id: xiigen-b01-l3-020-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 020]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-020-003 — Teaching summary: file 020 key lessons

```
source_id: xiigen-b01-l3-020-003
pair_kind: teaching_unit
```

Key lessons from file 020:
  1. "layout" IS in the UI lexicon — fires UI for any layout/grid/structure context
  2. "address validation" fires IMPLEMENT — validation logic is code, not UI
  3. "checkout flow" — "flow", "process", "steps" are not predicate terms → gap
  4. UI+IMPLEMENT nine-node: DomainSplitter produces 2 packets (UI + IMPLEMENT)
  5. AddressAndDelivery nine-node: 6 screens (3 address book + 3 delivery options); form inputs catalogued
