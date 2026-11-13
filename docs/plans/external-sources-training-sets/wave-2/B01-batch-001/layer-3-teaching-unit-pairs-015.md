# Layer 3 — Teaching Unit Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-015-001 — PairParserUnit: files 015

```
source_id: xiigen-b01-l3-015-001
pair_kind: teaching_unit
```

A: BatchId: "B01-batch-001" | Theme: "SettingsDashboard expansion"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "SettingsDashboard UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "SettingsDashboard routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "sidebar gap; spacing IS UI; user experience gap"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_rejected"
    - Layer: "1" | PairCount: 4 | Theme: "AddressBook, DeliveryOptions, checkout build, three-predicate"
    - Layer: "2" | PairCount: 9 | Theme: "SettingsPanelFlow UI alone"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 015"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-015-002 — ModelTrainerUnit: files 015

```
source_id: xiigen-b01-l3-015-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 015]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-015-003 — Teaching summary: file 015 key lessons

```
source_id: xiigen-b01-l3-015-003
pair_kind: teaching_unit
```

Key lessons from file 015:
  1. "sidebar" — NOT in UI lexicon → gap vocabulary → knowledge-edge
  2. "spacing" IS in the UI lexicon → fires UI
  3. "user experience" (longform) does NOT fire UI — only "ux" does
  4. SettingsDashboard nine-node: 8 screens across 5 sections, all form inputs catalogued
  5. DOCUMENT (define) + UI (responsive, layout) → two independent routing decisions
